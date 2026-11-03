using System;
using System.Collections.Generic;

namespace Genies.ServiceManagement
{
    /// <summary>
    /// Indicates this installer has dependencies on other installers.
    /// Implement <see cref="IRequiresInstaller{T}"/> to declare specific installer dependencies,
    /// and override <see cref="GetRequiredInstallers"/> to provide pre-configured instances.
    /// </summary>
    public interface IHasInstallerRequirements
    {
        /// <summary>
        /// Returns pre-configured instances of this installer's required dependencies.
        /// Each installer is responsible for instantiating its own required installers
        /// with appropriate configuration. <see cref="InstallerChainBuilder"/> walks this
        /// tree recursively to build the full chain.
        ///
        /// Override priority: explicit overrides passed to <see cref="InstallerChainBuilder.Build"/> &gt;
        /// higher-level installer's instances &gt; lower-level installer's instances.
        ///
        /// The default implementation throws <see cref="NotImplementedException"/>.
        /// If this is reached at runtime, <see cref="InstallerChainBuilder"/> will provide
        /// an actionable error message.
        /// </summary>
        IEnumerable<IGeniesInstaller> GetRequiredInstallers() =>
            throw new NotImplementedException(
                $"{GetType().Name}.GetRequiredInstallers() is not implemented.");
    }

    /// <summary>
    /// Indicates this installer requires another specific installer to be registered first.
    /// The required installer must have an earlier OperationOrder or be in the same group but processed earlier.
    /// </summary>
    /// <typeparam name="TRequiredInstaller">The installer type that must be registered first</typeparam>
    public interface IRequiresInstaller<TRequiredInstaller> : IHasInstallerRequirements
        where TRequiredInstaller : IGeniesInstaller
    {
    }
}
