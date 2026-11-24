using System;
using Cysharp.Threading.Tasks;
using Genies.Sdk.Samples.Common;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace Genies.Sdk.Samples.DemoMode
{
    public sealed class DemoMode : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputSystemUIInputModule _inputModule;

        [Header("Scene References")]
        [SerializeField] private GeniesAvatarController _avatarController;
        [SerializeField] private RuntimeAnimatorController _optionalController;

        private ManagedAvatar _loadedAvatar;

        private void Awake()
        {
            // Avatar controller will eat inputs... don't enable until we're done loading.
            if (_avatarController != null)
            {
                _avatarController.enabled = false;
            }

            if (_inputModule != null)
            {
                _inputModule.enabled = true;
            }
        }

        private async void Start()
        {
            await AvatarSdk.InitializeDemoModeAsync();
            await LoadAvatarAsync();
        }

        private async UniTask LoadAvatarAsync()
        {
            try
            {
                if (_avatarController != null)
                {
                    // Parenting the loaded avatar to an inactive GO and then immediately activating it can crash.
                    // Enable the controller first.
                    _avatarController.enabled = true;
                }

                _loadedAvatar = await AvatarSdk.LoadAvatarAsync(new LoadAvatarOptions.Test
                {
                    AvatarName = "name",
                    Parent = _avatarController != null ? _avatarController.transform : null,
                    PlayerAnimationController = _optionalController,
                });

                if (_loadedAvatar == null)
                {
                    Debug.LogError("Failed to load avatar: LoadTestAvatarAsync returned null", this);
                    return;
                }

                var root = _loadedAvatar.Root;
                if (root == null)
                {
                    Debug.LogError("Loaded avatar has null Root component", this);
                    return;
                }

                var animatorEventBridge = root.gameObject.AddComponent<GeniesAnimatorEventBridge>();

                AvatarLoadedNotifier.InvokeLoaded(_loadedAvatar);

                if (_avatarController == null)
                {
                    return;
                }

                _avatarController.SetAnimatorEventBridge(animatorEventBridge);
                _avatarController.GenieSpawned = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading avatar: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void DestroyLoadedAvatar()
        {
            if (_avatarController != null)
            {
                _avatarController.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                _avatarController.transform.localScale = Vector3.one;
            }

            if (_loadedAvatar == null)
            {
                return;
            }

            _loadedAvatar.Dispose();
            AvatarLoadedNotifier.InvokeDestroyed(_loadedAvatar);
            _loadedAvatar = null;
        }

        private void OnDestroy()
        {
            DestroyLoadedAvatar();
        }
    }
}
