using System;
using UnityEngine;

namespace Genies.Sdk
{
    /// <summary>
    /// Common options shared by all avatar load operations.
    /// Pass a concrete implementation to <see cref="AvatarSdk.LoadAvatarAsync{T}"/> to select the load path.
    /// For faster load times, consider pre-caching avatar assets before loading - see
    /// <see cref="AvatarSdk.PrecacheUserAvatarAssetsAsync"/>, <see cref="AvatarSdk.PrecacheDefaultAvatarAssetsAsync"/>,
    /// and <see cref="AvatarSdk.PrecacheAvatarAssetsByDefinitionAsync"/>.
    /// </summary>
    public interface ILoadAvatarOptions
    {
        /// <summary>Optional name for the avatar GameObject.</summary>
        string AvatarName { get; set; }

        /// <summary>Optional parent transform for the avatar.</summary>
        Transform Parent { get; set; }

        /// <summary>Optional animation controller to apply to the avatar.</summary>
        RuntimeAnimatorController PlayerAnimationController { get; set; }

        /// <summary>Whether to show a placeholder silhouette while the avatar is loading.</summary>
        bool ShowLoadingSilhouette { get; set; }

        /// <summary>
        /// The LOD level(s) to target for loading. Multiple values are loaded sequentially
        /// from lowest to highest quality as they become available. Pass a single-element array
        /// to target a specific LOD without sequential loading. Defaults to Low.
        /// Currently affects material/texture quality only. Mesh LOD support will be added in a future update.
        /// </summary>
        AvatarLods[] TargetLods { get; set; }

        /// <summary>
        /// Optional callback invoked when the full avatar finishes loading in the background.
        /// Only relevant when <see cref="ShowLoadingSilhouette"/> is true - the silhouette appears
        /// immediately and this callback fires once the full avatar replaces it.
        /// </summary>
        Action<ManagedAvatar> OnLoadingComplete { get; set; }
    }

    public struct LoadAvatarOptions
    {
        /// <summary>The default LOD levels used when no target LODs are specified.</summary>
        internal static readonly AvatarLods[] DefaultTargetLods = { AvatarLods.Low, };

        /// <summary>Returns <paramref name="lods"/> if non-null and non-empty, otherwise <see cref="DefaultTargetLods"/>.</summary>
        internal static AvatarLods[] WithDefault(AvatarLods[] lods) =>
            lods is { Length: > 0 } ? lods : DefaultTargetLods;

        /// <summary>
        /// Options for loading a user's avatar.
        /// When <see cref="UserId"/> is null or whitespace, loads the currently authenticated user's avatar
        /// by fetching the latest definition from the server. Falls back to the default avatar if the user
        /// is not logged in.
        /// When <see cref="UserId"/> is set, loads the specified user's avatar by their ID.
        /// The user must be logged in.
        /// </summary>
        public struct User : ILoadAvatarOptions
        {
            /// <summary>Optional user ID. When null or whitespace, loads the currently authenticated user's avatar.</summary>
            public string UserId { get; set; }

            /// <inheritdoc />
            public string AvatarName { get; set; }

            /// <inheritdoc />
            public Transform Parent { get; set; }

            /// <inheritdoc />
            public RuntimeAnimatorController PlayerAnimationController { get; set; }

            private bool? _showLoadingSilhouette;

            /// <inheritdoc />
            /// <remarks>Defaults to true.</remarks>
            public bool ShowLoadingSilhouette
            {
                get => _showLoadingSilhouette ?? true;
                set => _showLoadingSilhouette = value;
            }

            /// <inheritdoc />
            public AvatarLods[] TargetLods { get; set; }

            /// <inheritdoc />
            public Action<ManagedAvatar> OnLoadingComplete { get; set; }
        }

        /// <summary>
        /// Options for loading an avatar from a JSON definition string.
        /// Use <see cref="AvatarSdk.GetUserAvatarDefinition()"/> to retrieve a definition, then pass it
        /// via <see cref="DefinitionToLoad"/> for optimized subsequent loads that skip the server fetch.
        /// The user must be logged in.
        /// </summary>
        public struct ByDefinition : ILoadAvatarOptions
        {
            /// <summary>The JSON definition of the avatar to load.</summary>
            public string DefinitionToLoad { get; set; }

            /// <inheritdoc />
            public string AvatarName { get; set; }

            /// <inheritdoc />
            public Transform Parent { get; set; }

            /// <inheritdoc />
            public RuntimeAnimatorController PlayerAnimationController { get; set; }

            private bool? _showLoadingSilhouette;

            /// <inheritdoc />
            /// <remarks>Defaults to true.</remarks>
            public bool ShowLoadingSilhouette
            {
                get => _showLoadingSilhouette ?? true;
                set => _showLoadingSilhouette = value;
            }

            /// <inheritdoc />
            public AvatarLods[] TargetLods { get; set; }

            /// <inheritdoc />
            public Action<ManagedAvatar> OnLoadingComplete { get; set; }
        }

        /// <summary>
        /// Options for loading the default fallback avatar.
        /// Loads a generic placeholder avatar that does not require user authentication data.
        /// The user must be logged in.
        /// </summary>
        public struct Default : ILoadAvatarOptions
        {
            /// <inheritdoc />
            public string AvatarName { get; set; }

            /// <inheritdoc />
            public Transform Parent { get; set; }

            /// <inheritdoc />
            public RuntimeAnimatorController PlayerAnimationController { get; set; }

            private bool? _showLoadingSilhouette;

            /// <inheritdoc />
            /// <remarks>Defaults to true.</remarks>
            public bool ShowLoadingSilhouette
            {
                get => _showLoadingSilhouette ?? true;
                set => _showLoadingSilhouette = value;
            }

            /// <inheritdoc />
            public AvatarLods[] TargetLods { get; set; }

            /// <inheritdoc />
            public Action<ManagedAvatar> OnLoadingComplete { get; set; }
        }

        /// <summary>
        /// Options for loading a test avatar.
        /// Does not require user authentication - intended for demo mode and offline testing scenarios.
        /// Initialize the SDK in demo mode via <see cref="AvatarSdk.InitializeDemoModeAsync"/> before use.
        /// </summary>
        public struct Test : ILoadAvatarOptions
        {
            /// <inheritdoc />
            public string AvatarName { get; set; }

            /// <inheritdoc />
            public Transform Parent { get; set; }

            /// <inheritdoc />
            public RuntimeAnimatorController PlayerAnimationController { get; set; }

            private bool? _showLoadingSilhouette;

            /// <inheritdoc />
            /// <remarks>Defaults to true.</remarks>
            public bool ShowLoadingSilhouette
            {
                get => _showLoadingSilhouette ?? true;
                set => _showLoadingSilhouette = value;
            }

            /// <inheritdoc />
            public AvatarLods[] TargetLods { get; set; }

            /// <inheritdoc />
            public Action<ManagedAvatar> OnLoadingComplete { get; set; }
        }
    }
}
