using Genies.CrashReporting;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Genies.ServiceManagement
{
    // App initialization events and state used by IAppInitializer implementations.
    /// <summary>
    /// Implementation for <see cref="ServiceManager"/> initialization logic, the initialization logic, every app
    /// that wishes to use the <see cref="ServiceManager"/> should invoke <see cref="InitializeAppAsync"/> the initialization does the
    /// following:
    ///
    /// - Allows the user to pass custom <see cref="IGeniesInstaller"/> and <see cref="IGeniesInitializer"/> instances for initializing their app
    /// - Uses <see cref="AutoResolver"/> to get all Auto Service Resolvers that implement <see cref="IGeniesInstaller"/> and <see cref="IGeniesInitializer"/>
    /// - Creates the <see cref="GeniesSingletonLifetimeScope"/> so that the installers can get automatic injection for singleton instances that were registered
    ///   running the initialization.
    /// - Groups all <see cref="IGeniesInstaller"/> and <see cref="IGeniesInitializer"/> that have the same order <see cref="IGroupedOperation.OperationOrder"/>
    /// - Foreach group we create a new <see cref="GeniesRootLifetimeScope"/> parent it to the previous and install the services from all <see cref="IGeniesInstaller"/>
    ///   in group then we call <see cref="IGeniesInitializer.Initialize"/> for all initializers in that group.
    ///
    ///
    /// NOTE: For dev/editor we also try to resolve all the services (in production they are lazily resolved when required) to find any installation issues.
    /// NOTE: All scopes created are root scopes and will live as long as the app session lives.
    /// </summary>
    public static partial class ServiceManager
    {
        /// <summary>
        /// Initialize the app using a typed <see cref="IAppInitializer"/> entry point with
        /// deferred installer/initializer construction. The factories are only invoked if
        /// <typeparamref name="TInitializer"/> is the winning initializer (no override).
        /// This avoids building the installer chain when a higher-level override will
        /// discard it and build its own.
        /// </summary>
        public static async UniTask InitializeAppAsync<TInitializer>(
            Func<List<IGeniesInstaller>> installerFactory = null,
            Func<List<IGeniesInitializer>> initializerFactory = null,
            bool disableAutoResolve = false,
            AutoResolverSettings overrideSettings = null)
            where TInitializer : IAppInitializer, new()
        {
            if (IsAppInitialized)
            {
                return;
            }

            if (IsAppInitializing)
            {
                Debug.LogWarning($"[{nameof(ServiceManager)}] App initialization is already in progress. Skipping InitializeAppAsync<{typeof(TInitializer).Name}>().");
                return;
            }

            var highestInitializer = ResolveHighestAppInitializer<TInitializer>();
            InitializingAppInitializerType = highestInitializer.GetType();

            if (highestInitializer.GetType() != typeof(TInitializer))
            {
                // Override found — let it take over completely
                await highestInitializer.InitializeAppAsync();
            }
            else
            {
                // No override — invoke the factories and proceed
                var installers = installerFactory?.Invoke();
                var initializers = initializerFactory?.Invoke();
                await InitializeAppAsync(installers, initializers,
                    disableAutoResolve, overrideSettings);
            }
        }

        /// <summary>
        /// Checks whether <paramref name="candidateType"/> appears in the override chain
        /// that starts from <typeparamref name="TInitializer"/> and walks upward to the
        /// highest overrider. Returns true if <paramref name="candidateType"/> is
        /// <typeparamref name="TInitializer"/> itself or any type that overrides it
        /// (directly or transitively).
        /// </summary>
        public static bool IsInOverrideChainOf<TInitializer>(Type candidateType)
            where TInitializer : IAppInitializer
        {
            var overrideMap = AppInitializerOverrideMap;
            var visited = new HashSet<Type>();
            return IsInOverrideChain(typeof(TInitializer), candidateType, overrideMap, visited);
        }

        private static bool IsInOverrideChain(
            Type startType,
            Type candidateType,
            Dictionary<Type, List<Type>> overrideMap,
            HashSet<Type> visited)
        {
            if (startType == candidateType)
            {
                return true;
            }

            if (!visited.Add(startType))
            {
                return false;
            }

            var overriders = GetDirectAppInitializerOverriders(startType, overrideMap);

            foreach (var overrider in overriders)
            {
                if (IsInOverrideChain(overrider, candidateType, overrideMap, visited))
                {
                    return true;
                }
            }

            return false;
        }

        private static IAppInitializer ResolveHighestAppInitializer<TInitializer>()
            where TInitializer : IAppInitializer, new()
        {
            var overrideMap = AppInitializerOverrideMap;
            var highestType = ResolveHighestFromType(
                typeof(TInitializer), overrideMap, new HashSet<Type>());

            if (highestType == typeof(TInitializer))
            {
                return new TInitializer();
            }

            return CreateAppInitializerInstance(highestType);
        }

        private static IAppInitializer CreateAppInitializerInstance(Type type)
        {
            try
            {
                return (IAppInitializer)Activator.CreateInstance(type);
            }
            catch (MissingMethodException)
            {
                throw new ServiceManagerException(
                    $"{type.Name} was resolved as an app initializer override but could not be " +
                    $"instantiated. All IAppInitializer and IOverridesAppInitializer<T> " +
                    $"implementations must have a public parameterless constructor. " +
                    $"Add a public parameterless constructor to {type.Name}.");
            }
        }

        private static Type ResolveHighestFromType(
            Type startType,
            Dictionary<Type, List<Type>> overrideMap,
            HashSet<Type> visited)
        {
            if (!visited.Add(startType))
            {
                throw new ServiceManagerException(
                    $"Circular app initializer override detected: " +
                    $"{startType.Name} was already visited in the override chain.");
            }

            var overriders = GetDirectAppInitializerOverriders(startType, overrideMap);

            if (overriders.Count == 0)
            {
                return startType;
            }

            if (overriders.Count == 1)
            {
                return ResolveHighestFromType(overriders[0], overrideMap, visited);
            }

            // Multiple overriders (diamond pattern) — resolve each branch to its
            // highest point. If all branches converge to the same type, use it.
            var resolvedTypes = new HashSet<Type>();
            foreach (var overriderType in overriders)
            {
                resolvedTypes.Add(
                    ResolveHighestFromType(overriderType, overrideMap,
                        new HashSet<Type>(visited)));
            }

            if (resolvedTypes.Count != 1)
            {
                throw new ServiceManagerException(
                    $"Ambiguous app initializer override: " +
                    $"{string.Join(", ", overriders.Select(t => t.Name))} " +
                    $"both override {startType.Name} and do not converge to a single overrider.");
            }

            return resolvedTypes.Single();
        }

        private static Dictionary<Type, List<Type>> AppInitializerOverrideMap { get; set; } = new();

        /// <summary>
        /// Registers <typeparamref name="TOverrider"/> as an override of every
        /// <see cref="IAppInitializer"/> it targets via its
        /// <see cref="IOverridesAppInitializer{T}"/> interface declarations.
        /// Called from
        /// <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]</c>
        /// methods in each package that declares an <see cref="IOverridesAppInitializer{T}"/>.
        /// </summary>
        public static void RegisterAppInitializerOverrider<TOverrider>()
            where TOverrider : IOverridesAppInitializer<IAppInitializer>
        {
            var overriderType = typeof(TOverrider);

            foreach (var iface in overriderType.GetInterfaces())
            {
                if (!iface.IsGenericType ||
                    iface.GetGenericTypeDefinition() != typeof(IOverridesAppInitializer<>))
                {
                    continue;
                }

                var targetType = iface.GetGenericArguments()[0];

                if (!AppInitializerOverrideMap.TryGetValue(targetType, out var list))
                {
                    list = new List<Type>();
                    AppInitializerOverrideMap[targetType] = list;
                }

                list.Add(overriderType);
            }
        }

        private static List<Type> GetDirectAppInitializerOverriders(Type targetType, Dictionary<Type, List<Type>> overrideMap)
        {
            return overrideMap.TryGetValue(targetType, out var overriders)
                ? overriders
                : new List<Type>();
        }

        /// <summary>
        /// Initialize the app services, the initialization will create DontDestroyOnLoad scopes
        /// as we consider any app level services long lasting.
        /// </summary>
        /// <param name="customInstallers"> Extra installers </param>
        /// <param name="customInitializers"> Extra initializers </param>
        /// <param name="disableAutoResolve"> If you want to disable auto resolved services </param>
        /// <param name="overrideSettings"> If you want to override auto resolve settings for a demo or different scenes</param>
        public static async UniTask InitializeAppAsync(
            List<IGeniesInstaller> customInstallers = null,
            List<IGeniesInitializer> customInitializers = null,
            bool disableAutoResolve = false,
            AutoResolverSettings overrideSettings = null)
        {
            if (IsAppInitialized)
            {
                var exception = new ServiceManagerException("App was already initialized, if you need to re-initialize make sure to call ServiceManager.Dispose first.");
                CrashReporter.LogHandledException(exception);
                return;
            }

            if (IsAppInitializing)
            {
                Debug.LogWarning($"[{nameof(ServiceManager)}] App initialization is already in progress. Wait for the current initialization to complete before calling InitializeAppAsync again.");
                return;
            }

            IsAppInitializing = true;
            var initializerType = InitializingAppInitializerType;
            InitializingAppInitializerType = null;
            var startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            try
            {
                try
                {
                    AppInitializationStarted?.Invoke(initializerType);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[{nameof(ServiceManager)}] AppInitializationStarted handler threw: {ex.Message}");
                    CrashReporter.LogHandledException(ex);
                }

                var autoResolvedInstallers   = disableAutoResolve ? new List<IGeniesInstaller>() : AutoResolver.GetAutoInstallers(overrideSettings).ToList();
                var autoResolvedInitializers = disableAutoResolve ? new List<IGeniesInitializer>() : AutoResolver.GetAutoInitializers(overrideSettings).ToList();

                customInstallers ??= new List<IGeniesInstaller>();
                customInitializers ??= new List<IGeniesInitializer>();

                customInitializers = customInitializers.Concat(autoResolvedInitializers).ToList();

                // Install custom installers first to ensure no duplicates.
                var orderedInstallersList = new List<IGeniesInstaller>();
                orderedInstallersList.AddRange(customInstallers);
                orderedInstallersList.AddRange(autoResolvedInstallers);

                //Create app root scopes
                await CreateScopeAsync(null, orderedInstallersList, customInitializers, dontDestroyOnLoad: true);

                try
                {
                    AppInitializationCompleted?.Invoke(initializerType,
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds() - startTime);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[{nameof(ServiceManager)}] AppInitializationCompleted handler threw: {ex.Message}");
                    CrashReporter.LogHandledException(ex);
                }
            }
            catch (ServiceManagerException ex)
            {
                // Log app initialization failure and report to crash reporting
                Debug.LogError($"[{nameof(ServiceManager)}] App initialization failed: {ex.Message}");
                CrashReporter.LogHandledException(ex);
                throw;
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions (e.g., AutoResolver reflection errors, constructor failures)
                var wrappedException = new ServiceManagerException($"Unexpected error during app initialization: {ex.Message}", ex);
                Debug.LogError($"[{nameof(ServiceManager)}] App initialization failed: {wrappedException.Message}");
                CrashReporter.LogHandledException(wrappedException);
                throw wrappedException;
            }
            finally
            {
                // Always set IsAppInitialized to true to force Dispose() call for cleanup
                // This prevents potential duplicate initialization when partial state may exist
                IsAppInitialized = true;

                // Always reset the initializing flag regardless of success or failure
                IsAppInitializing = false;
            }
        }
    }
}
