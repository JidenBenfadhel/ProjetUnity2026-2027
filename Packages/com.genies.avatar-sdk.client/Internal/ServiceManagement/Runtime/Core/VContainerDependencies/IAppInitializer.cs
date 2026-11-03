using Cysharp.Threading.Tasks;

namespace Genies.ServiceManagement
{
    /// <summary>
    /// Represents an app-level initialization entry point.
    /// Any layer in the application stack — a Unity project, an SDK package, or a lower-level
    /// dependency — can implement this interface to participate in the initialization chain.
    /// Each initializer directly calls <see cref="ServiceManager.InitializeAppAsync"/> with
    /// its own installer chain. No parent-child chaining between initializers.
    ///
    /// <para><b>Requirement:</b> All implementations must have a <b>public parameterless constructor</b>.
    /// Override resolution via <see cref="IOverridesAppInitializer{T}"/> instantiates the
    /// resolved type at runtime using <c>Activator.CreateInstance</c>. Types without a public
    /// parameterless constructor will fail with a <see cref="ServiceManagerException"/> at
    /// initialization time.</para>
    ///
    /// <para>Overriders must register themselves at startup by calling
    /// <see cref="ServiceManager.RegisterAppInitializerOverrider{TOverrider}"/> from a
    /// <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]</c>
    /// method.</para>
    /// </summary>
    public interface IAppInitializer
    {
        /// <summary>
        /// Entry point called by <see cref="ServiceManager.InitializeAppAsync{T}"/>
        /// after override resolution. Implementations should build the full installer
        /// list via <see cref="InstallerChainBuilder"/> and call
        /// <see cref="ServiceManager.InitializeAppAsync(System.Collections.Generic.List{IGeniesInstaller}, System.Collections.Generic.List{IGeniesInitializer}, bool, AutoResolverSettings)"/>.
        /// </summary>
        UniTask InitializeAppAsync();
    }
}
