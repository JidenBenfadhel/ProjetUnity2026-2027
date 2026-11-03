using System;
using System.Collections.Generic;
using System.Linq;

namespace Genies.ServiceManagement
{
    /// <summary>
    /// Builds a complete installer chain by recursively merging all
    /// <see cref="IHasInstallerRequirements.GetRequiredInstallers"/> results for a given root installer.
    /// Supports explicit overrides so callers can substitute specific installer types
    /// with pre-configured instances. Higher-level installer instances take priority
    /// over lower-level ones (first-writer-wins).
    /// </summary>
    public static class InstallerChainBuilder
    {
        /// <summary>
        /// Builds the full list of installers required by the given root installer,
        /// including the root itself and all transitive dependencies resolved via
        /// <see cref="IHasInstallerRequirements.GetRequiredInstallers"/>. Installers are returned
        /// in dependency order (dependencies before dependents).
        /// </summary>
        /// <param name="rootInstaller">The top-level installer to resolve dependencies for.</param>
        /// <param name="overrides">
        /// Optional installer instances to use instead of ones returned by
        /// <see cref="IHasInstallerRequirements.GetRequiredInstallers"/>. Explicit overrides
        /// take highest priority over all other sources.
        /// </param>
        /// <returns>A list of all required installers in dependency order.</returns>
        public static List<IGeniesInstaller> Build(
            IGeniesInstaller rootInstaller,
            params IGeniesInstaller[] overrides)
        {
            return Build(rootInstaller, overrides as IEnumerable<IGeniesInstaller>);
        }

        /// <summary>
        /// Builds the full list of installers required by the given root installer,
        /// including the root itself and all transitive dependencies resolved via
        /// <see cref="IHasInstallerRequirements.GetRequiredInstallers"/>. Installers are returned
        /// in dependency order (dependencies before dependents).
        /// </summary>
        /// <param name="rootInstaller">The top-level installer to resolve dependencies for.</param>
        /// <param name="overrides">
        /// Optional installer instances to use instead of ones returned by
        /// <see cref="IHasInstallerRequirements.GetRequiredInstallers"/>. Explicit overrides
        /// take highest priority over all other sources.
        /// </param>
        /// <returns>A list of all required installers in dependency order.</returns>
        public static List<IGeniesInstaller> Build(
            IGeniesInstaller rootInstaller,
            IEnumerable<IGeniesInstaller> overrides = null)
        {
            var overrideMap = new Dictionary<Type, IGeniesInstaller>();

            if (overrides != null)
            {
                foreach (var installerOverride in overrides)
                {
                    overrideMap[installerOverride.GetType()] = installerOverride;
                }
            }

            var instanceMap = new Dictionary<Type, IGeniesInstaller>(overrideMap);
            var resolved = new Dictionary<Type, IGeniesInstaller>();
            var visiting = new HashSet<Type>();

            ResolveRecursive(rootInstaller, overrideMap, instanceMap, resolved, visiting);

            return InstallerTopologicalSorter.Sort(resolved.Values);
        }

        private static void ResolveRecursive(
            IGeniesInstaller installer,
            Dictionary<Type, IGeniesInstaller> overrideMap,
            Dictionary<Type, IGeniesInstaller> instanceMap,
            Dictionary<Type, IGeniesInstaller> resolved,
            HashSet<Type> visiting)
        {
            var installerType = installer.GetType();

            if (resolved.ContainsKey(installerType))
            {
                return;
            }

            if (!visiting.Add(installerType))
            {
                throw new ServiceManagerException(
                    $"Circular installer dependency detected: " +
                    $"{installerType.Name} is already being resolved. " +
                    $"Check IRequiresInstaller declarations for cycles.");
            }

            if (installer is IHasInstallerRequirements hasRequirements
                && !overrideMap.ContainsKey(installerType))
            {
                IEnumerable<IGeniesInstaller> children;

                try
                {
                    children = hasRequirements.GetRequiredInstallers();
                }
                catch (NotImplementedException)
                {
                    if (!AreRequirementsSatisfied(installerType, instanceMap, resolved))
                    {
                        throw new ServiceManagerException(
                            $"{installerType.Name} implements IRequiresInstaller<T> but " +
                            $"GetRequiredInstallers() is not implemented and not all requirements " +
                            $"have been provided by a higher-level installer. " +
                            $"Either implement GetRequiredInstallers() on {installerType.Name} to return " +
                            $"its required installer instances, or pass pre-configured instances as " +
                            $"overrides to InstallerChainBuilder.Build().");
                    }

                    children = null;
                }

                if (children != null)
                {
                    var childList = children.ToList();

                    // Phase 1: Pre-register all children into instanceMap.
                    // Explicit overrides take priority, then first-writer-wins
                    // ensures higher-level installer instances win over lower-level ones.
                    foreach (var child in childList)
                    {
                        var childType = child.GetType();

                        if (overrideMap.TryGetValue(childType, out var explicitOverride))
                        {
                            instanceMap[childType] = explicitOverride;
                        }
                        else
                        {
                            instanceMap.TryAdd(childType, child);
                        }
                    }

                    // Phase 2: Recurse into each child using the chosen instance.
                    foreach (var child in childList)
                    {
                        var childType = child.GetType();
                        ResolveRecursive(instanceMap[childType], overrideMap, instanceMap, resolved, visiting);
                    }
                }
            }

            visiting.Remove(installerType);
            resolved[installerType] = installer;
        }

        private static bool AreRequirementsSatisfied(
            Type installerType,
            Dictionary<Type, IGeniesInstaller> instanceMap,
            Dictionary<Type, IGeniesInstaller> resolved)
        {
            return InstallerRequirementAnalyzer.GetRequiredInstallerTypes(installerType)
                .All(req => instanceMap.Keys.Any(k => req.IsAssignableFrom(k))
                         || resolved.Keys.Any(k => req.IsAssignableFrom(k)));
        }
    }
}
