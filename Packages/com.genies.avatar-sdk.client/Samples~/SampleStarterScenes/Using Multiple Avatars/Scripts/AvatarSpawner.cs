using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Genies.Sdk.Samples.Common;
using Button = UnityEngine.UI.Button;

namespace Genies.Sdk.Samples.MultipleAvatars
{
    /// <summary>
    /// Spawns two avatars from different profile IDs once the user is logged in.
    /// Uses AvatarSdk.LoadFromLocalGameObjectAsync to load each avatar.
    /// </summary>
    public class AvatarSpawner : MonoBehaviour
    {
        [Header("Avatar Spawn Configuration")]
        [Tooltip("Transform positions where the avatars will be spawned")]
        [SerializeField] private Transform _avatar1Transform;
        [SerializeField] private Transform _avatar2Transform;
        [SerializeField] private Transform _avatar3Transform;
        [SerializeField] private Transform _avatar4Transform;

        [Header("Avatar Profiles/Definitions")]
        [SerializeField] private string _avatar1ProfileId = "ExampleProfile1";
        [SerializeField] private string _avatar2ProfileId = "ExampleProfile2";
        [TextArea] [SerializeField] private string _avatar3Definition;
        [TextArea] [SerializeField] private string _avatar4Definition;

        [Header("Status")]
        [SerializeField] private bool _hasAttemptedSpawn = false;

        [Header("LoginUI")]
        [SerializeField] private GeniesLoginUI geniesLoginUI;
        private List<ManagedAvatar> _spawnedAvatars = new List<ManagedAvatar>();

        [Header("Other")]
        [SerializeField] private GameObject _loadingSpinner;
        [SerializeField] private GameObject _selectAvatarText;
        [SerializeField] private GameObject _backButton;
        [SerializeField] private Transform _cameraStartPosition;

        private void Awake()
        {
            // Subscribe to login events
            AvatarSdk.Events.UserLoggedIn += OnUserLoggedIn;

            // Return to main view of spawned avatars when back button clicked
            if (_backButton != null && _backButton.TryGetComponent<Button>(out var button))
            {
                button.onClick.AddListener(() => CameraMover.MoveCamera(_cameraStartPosition, true).Forget());
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            AvatarSdk.Events.UserLoggedIn -= OnUserLoggedIn;

            if (_backButton != null && _backButton.TryGetComponent<Button>(out var button))
            {
                button.onClick.RemoveAllListeners();
            }

            // Clean up spawned avatars
            CleanupAvatars();
        }

        private void OnUserLoggedIn()
        {
            TrySpawnAsync().Forget();
        }

        private void HandleSpawnError(Exception ex)
        {
            Debug.LogError($"Error spawning avatars: {ex.Message}\n{ex.StackTrace}", this);

            if (_loadingSpinner != null)
            {
                _loadingSpinner.SetActive(false);
            }
        }

        private async UniTask TrySpawnAsync()
        {
            try
            {
                if (!_hasAttemptedSpawn)
                {
                    await SpawnAvatars();
                }
            }
            catch (Exception ex)
            {
                HandleSpawnError(ex);
            }
        }

        /// <summary>
        /// Spawns avatars from different profile IDs at specified transforms
        /// </summary>
        private async UniTask SpawnAvatars()
        {
            // Validate transforms are assigned before showing loading state
            if (_avatar1Transform == null ||
                _avatar2Transform == null ||
                _avatar3Transform == null ||
                _avatar4Transform == null)
            {
                Debug.LogError("One or more avatar transforms are not assigned! Please assign all transforms in the inspector.", this);
                return;
            }

            if (_loadingSpinner != null)
            {
                _loadingSpinner.SetActive(true);
            }

            _hasAttemptedSpawn = true;

            try
            {
                // Clean up any existing avatars first
                CleanupAvatars();

                // Spawn Avatar 1
                var avatar1 = await AvatarSdk.LoadFromLocalGameObjectAsync(_avatar1ProfileId);
                if (avatar1 != null)
                {
                    _spawnedAvatars.Add(avatar1);
                    PositionAvatar(avatar1, _avatar1Transform);
                }
                else
                {
                    Debug.LogError($"Failed to spawn Avatar 1: {_avatar1ProfileId}");
                }

                // Spawn Avatar 2
                var avatar2 = await AvatarSdk.LoadFromLocalGameObjectAsync(_avatar2ProfileId);
                if (avatar2 != null)
                {
                    _spawnedAvatars.Add(avatar2);
                    PositionAvatar(avatar2, _avatar2Transform);
                }
                else
                {
                    Debug.LogError($"Failed to spawn Avatar 2: {_avatar2ProfileId}");
                }

                // Spawn Avatar 3
                var avatar3 = await AvatarSdk.LoadAvatarAsync(new LoadAvatarOptions.ByDefinition { DefinitionToLoad = _avatar3Definition });
                if (avatar3 != null)
                {
                    _spawnedAvatars.Add(avatar3);
                    PositionAvatar(avatar3, _avatar3Transform);
                }
                else
                {
                    Debug.LogError("Failed to spawn Avatar 3");
                }

                // Spawn Avatar 4
                var avatar4 = await AvatarSdk.LoadAvatarAsync(new LoadAvatarOptions.ByDefinition { DefinitionToLoad = _avatar4Definition });
                if (avatar4 != null)
                {
                    _spawnedAvatars.Add(avatar4);
                    PositionAvatar(avatar4, _avatar4Transform);
                }
                else
                {
                    Debug.LogError("Failed to spawn Avatar 4");
                }

                if (_loadingSpinner != null)
                {
                    _loadingSpinner.SetActive(false);
                }

                if (_selectAvatarText != null)
                {
                    _selectAvatarText.SetActive(true);
                }

                if (_backButton != null)
                {
                    _backButton.SetActive(true);
                }
            }
            catch (Exception ex)
            {
                HandleSpawnError(ex);
                throw; // Re-throw to be caught by TrySpawnAsync
            }
        }


        /// <summary>
        /// Positions and names the spawned avatar at the target transform
        /// </summary>
        private void PositionAvatar(ManagedAvatar avatar, Transform targetTransform)
        {
            if (avatar != null && avatar.Component != null)
            {
                // Set position and rotation
                avatar.Component.transform.SetParent(targetTransform);
                avatar.Component.transform.localPosition = new Vector3(0, -1, 0);
                avatar.Component.transform.localRotation = Quaternion.identity;

                // Add ClickableAvatar component to make it clickable
                var clickableAvatar = avatar.Component.gameObject.GetComponent<ClickableAvatar>();
                if (clickableAvatar == null)
                {
                    var capsule = avatar.Component.gameObject.AddComponent<CapsuleCollider>();
                    capsule.height = 2;
                    capsule.center = new Vector3(0, 0.5f, 0);
                    avatar.Component.gameObject.AddComponent<ClickableAvatar>();
                }
            }
        }

        /// <summary>
        /// Cleans up all spawned avatars
        /// </summary>
        private void CleanupAvatars()
        {
            foreach (var avatar in _spawnedAvatars)
            {
                try
                {
                    avatar?.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error disposing avatar: {ex.Message}");
                }
            }

            _spawnedAvatars.Clear();
        }
    }
}
