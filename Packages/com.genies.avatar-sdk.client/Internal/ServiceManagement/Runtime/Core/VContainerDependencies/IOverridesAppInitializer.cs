namespace Genies.ServiceManagement
{
    /// <summary>
    /// Marks an <see cref="IAppInitializer"/> as an override of another initializer.
    /// When <see cref="ServiceManager.InitializeAppAsync{T}"/> is called, the system walks
    /// the override chain to find the highest-level overrider and executes its
    /// <see cref="IAppInitializer.InitializeAppAsync()"/> (non-generic) instead.
    ///
    /// <para><b>Requirements:</b></para>
    /// <list type="bullet">
    /// <item>Implementors must have a <b>public parameterless constructor</b>.
    /// Override types are instantiated at runtime using <c>Activator.CreateInstance</c>.
    /// A missing parameterless constructor will cause a <see cref="ServiceManagerException"/>
    /// at initialization time.</item>
    /// <item>Implementors must register themselves at startup by calling
    /// <see cref="ServiceManager.RegisterAppInitializerOverrider{TOverrider}"/> from a
    /// <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]</c>
    /// method.</item>
    /// </list>
    /// </summary>
    public interface IOverridesAppInitializer<out T> : IAppInitializer where T : IAppInitializer
    {
    }
}
