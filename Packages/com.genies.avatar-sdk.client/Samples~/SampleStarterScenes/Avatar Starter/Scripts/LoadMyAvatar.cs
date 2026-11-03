using System;
using Cysharp.Threading.Tasks;
using Genies.Sdk.Samples.Common;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace Genies.Sdk.Samples.AvatarStarter
{
    public class LoadMyAvatar : MonoBehaviour
    {

        public bool IsAvatarLoaded { get; private set; }

        [Header("Input")]
        public InputSystemUIInputModule InputSystemUIInputModule;

        public ManagedAvatar LoadedAvatar;

        [Header("Scene References")]
        public GeniesAvatarController loadedController;
        public RuntimeAnimatorController OptionalController;

        [SerializeField] private GeniesLoginUI geniesLoginUI;
        [SerializeField] private GeniesInputs geniesInputs;

        private void Awake()
        {
            // Avatar controller will eat inputs... dont enable until we're done logging in.
            if (loadedController != null)
            {
                loadedController.enabled = false;
            }

            if (InputSystemUIInputModule != null)
            {
                InputSystemUIInputModule.enabled = true;
            }

            if (geniesLoginUI == null)
            {
                geniesLoginUI = FindAnyObjectByType<GeniesLoginUI>();
            }

            if (geniesInputs == null)
            {
                geniesInputs = FindAnyObjectByType<GeniesInputs>();
            }
        }

        private void Start()
        {
            if (!AvatarSdk.IsLoggedIn)
            {
                AvatarSdk.Events.UserLoggedIn += OnUserLoggedIn;
                AvatarSdk.Events.UserLoggedOut += OnUserLoggedOut;
            }
        }

        private void OnUserLoggedOut()
        {
            DestroyLoadedAvatar();
            geniesLoginUI.ResetUI();
        }

        private void OnUserLoggedIn()
        {
            LoadAvatarAsync().Forget();
        }

        private async UniTask LoadAvatarAsync()
        {
            try
            {
                if (loadedController != null)
                {
                    // Parenting the loaded avatar to an inactive GO and then immediately activating it will crash the application.
                    // Activate the parent object first.
                    loadedController.enabled = true;
                }

                LoadedAvatar = await AvatarSdk.LoadAvatarAsync(new LoadAvatarOptions.User
                {
                    Parent = loadedController.transform,
                    PlayerAnimationController = OptionalController != null ? OptionalController : null,
                });

                if (LoadedAvatar == null)
                {
                    Debug.LogError("Failed to load avatar: LoadUserAvatarAsync returned null", this);
                    return;
                }

                if (LoadedAvatar.Root == null)
                {
                    Debug.LogError("Loaded avatar has null Root component", this);
                    return;
                }

                var animatorEventBridge = LoadedAvatar.Root.gameObject.AddComponent<GeniesAnimatorEventBridge>();

                IsAvatarLoaded = true;
                AvatarLoadedNotifier.InvokeLoaded(LoadedAvatar);

                if (loadedController != null)
                {
                    loadedController.SetAnimatorEventBridge(animatorEventBridge);
                    loadedController.GenieSpawned = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading avatar: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void DestroyLoadedAvatar()
        {
            if (loadedController != null)
            {
                loadedController.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                loadedController.transform.localScale = Vector3.one;
            }
            if (LoadedAvatar != null)
            {

                LoadedAvatar.Dispose();
                AvatarLoadedNotifier.InvokeDestroyed(LoadedAvatar);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            AvatarSdk.Events.UserLoggedIn -= OnUserLoggedIn;
            AvatarSdk.Events.UserLoggedOut -= OnUserLoggedOut;
            DestroyLoadedAvatar();
        }
    }
}
