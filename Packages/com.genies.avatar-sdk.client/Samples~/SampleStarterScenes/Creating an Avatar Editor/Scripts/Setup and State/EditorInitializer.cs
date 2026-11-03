using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Genies.Sdk.Avatar.Samples.CustomAvatarEditor
{
    /// <summary>
    /// Entry point for the avatar editor prefab.
    ///
    /// Call <see cref="Initialize"/> with a <see cref="ManagedAvatar"/> to set up the editor.
    /// The prefab does not manage avatar loading or authentication — those are the caller's
    /// responsibility. This keeps the editor reusable across any scene or login flow.
    ///
    /// Camera target transforms are authored relative to an avatar at the origin. A
    /// <see cref="_cameraRig"/> transform parents all camera targets and the orbit pivot;
    /// <see cref="Initialize"/> aligns this rig to the avatar's world transform so the
    /// camera angles are correct regardless of where the avatar is placed.
    /// </summary>
    public class EditorInitializer : MonoBehaviour
    {
        [SerializeField] private Button _saveButton;

        [Header("Options")]
        [Tooltip("If true, will include user-specific outfits in the view")]
        [SerializeField] private bool _includeUserWearables;

        [Tooltip("If true, the asset cache will be cleared and refreshed.")]
        [SerializeField] private bool _clearCacheOnLoad;

        [Tooltip("Name of the scriptable object profile to save local avatar definitions. " +
                 "Found in Assts/Genies/AvatarEditor/Profiles/Resource")]
        [SerializeField] private string _localSaveProfileId = "SampleLocalAvatar";

        [Header("Camera")]
        [Tooltip("Parent transform of all camera target transforms and the orbit pivot. " +
                 "Aligned to the avatar's world transform at initialization so camera " +
                 "angles work regardless of avatar placement.")]
        [SerializeField] private Transform _cameraRig;

        [Tooltip("Rotation applied to the avatar to make it face the camera (e.g. 0,180,0). " +
                 "This is stripped from the avatar's transform when aligning the camera rig, " +
                 "since the camera targets were authored with this rotation already applied.")]
        [SerializeField] private Vector3 _avatarFacingEuler = new(0, 180, 0);

        private Quaternion _avatarFacingRotation;

        [Header("Other")]
        [SerializeField] private GameObject _loadingSpinner;
        [SerializeField] private GameObject _editorView;

        private UITransitionManager _uiTransitionManager;
        private ManagedAvatar _avatar;
        private bool _isSaving;

        public UITransitionManager UITransitionManager => _uiTransitionManager;

        private void OnEnable()
        {
            _uiTransitionManager = GetComponent<UITransitionManager>();
            _avatarFacingRotation = Quaternion.Euler(_avatarFacingEuler);

            if (_saveButton != null)
            {
                _saveButton.onClick.AddListener(OnSaveButtonClicked);
            }
        }

        /// <summary>
        /// Initializes the editor with an externally-provided avatar. Pre-caches asset data,
        /// aligns the camera rig, configures all UI generator groups, and displays the editor.
        /// </summary>
        public async UniTask Initialize(ManagedAvatar avatar)
        {
            if (!AvatarSdk.IsLoggedIn)
            {
                Debug.LogError("You must be logged in to use the editor");
                return;
            }

            if (_uiTransitionManager == null)
            {
                Debug.LogError("UITransitionManager component is missing on this GameObject.");
                return;
            }

            _uiTransitionManager.HideMainView();

            _avatar = avatar;

            if (_loadingSpinner != null)
            {
                _loadingSpinner.SetActive(true);
            }

            await PreCacheAssets();
            AlignCameraRig();

            var generatorGroups = GetComponentsInChildren<UIGeneratorGroup>(true);
            foreach (var generatorGroup in generatorGroups)
            {
                if (generatorGroup == null)
                {
                    continue;
                }

                generatorGroup.Avatar = _avatar;
                generatorGroup.IncludeUserWearables = _includeUserWearables;
            }

            if (_loadingSpinner != null)
            {
                _loadingSpinner.SetActive(false);
            }

            await _uiTransitionManager.DisplayInitialView();
        }

        public async UniTask Deactivate()
        {
            if (_uiTransitionManager != null)
            {
                await _uiTransitionManager.CloseView();
            }

            if (_cameraRig != null)
            {
                _cameraRig.SetParent(transform);
            }
        }

        private void AlignCameraRig()
        {
            if (_cameraRig == null || _avatar == null)
            {
                return;
            }

            var avatarTransform = _avatar.Root.transform;
            _cameraRig.SetParent(avatarTransform);

            var placementRotation = avatarTransform.rotation * Quaternion.Inverse(_avatarFacingRotation);
            var position = avatarTransform.position;

            _cameraRig.SetPositionAndRotation(position, placementRotation);
            _cameraRig.localScale = Vector3.one;
        }

        private async UniTask PreCacheAssets()
        {
            var tasks = new List<UniTask>
            {
                AssetInfoDataSource.GetWearablesDataForCategory(WearablesCategory.All),
                AssetInfoDataSource.GetAvatarHair(HairType.All),
                AssetInfoDataSource.GetAvatarFeatureDataForCategory(AvatarFeatureCategory.All),
                AssetInfoDataSource.GetColorDataForCategory(ColorType.All),
                AssetInfoDataSource.GetMakeupDataForCategory(AvatarMakeupCategory.All),
                AssetInfoDataSource.GetTattooData()
            };

            if (_includeUserWearables)
            {
                tasks.Add(AssetInfoDataSource.GetUserWearablesDataForCategory(UserWearablesCategory.All));
            }

            if (_clearCacheOnLoad)
            {
                AvatarSdk.ClearDefaultWearablesCache();
            }

            await UniTask.WhenAll(tasks);
        }

        private void OnSaveButtonClicked()
        {
            SaveAvatar().Forget();
        }

        private async UniTask SaveAvatar()
        {
            if (_isSaving)
            {
                return;
            }

            _isSaving = true;

            try
            {
                // Definition will be saved locally for anonymous users and to the server for logged in users.
                // In both cases, the avatar can be loaded later from the local definition.
                await AvatarSdk.SaveAvatarDefinitionLocallyAsync(_avatar, _localSaveProfileId);
                if (AvatarSdk.IsLoggedIn && !AvatarSdk.IsLoggedInAnonymously)
                {
                    await AvatarSdk.SaveUserAvatarDefinitionAsync(_avatar);
                }
            }
            finally
            {
                _isSaving = false;
            }
        }

        private void OnDisable()
        {
            if (_saveButton != null)
            {
                _saveButton.onClick.RemoveAllListeners();
            }
        }
    }
}
