using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Genies.Sdk.Avatar.Samples.CustomAvatarEditor
{
    /// <summary>
    /// Handles transitions between different UI views.
    /// Delegates camera-related concerns to <see cref="_cameraController"/>
    /// </summary>
    public class UITransitionManager : MonoBehaviour
    {
        [Header("Camera Positions")]
        [Tooltip("The transform that the camera will move to when viewing the body of the avatar. " +
                 "(Uses position and rotation of the transform)")]
        [SerializeField] private Transform _cameraEditBodyTransform;
        [Tooltip("The transform that the camera will move to when viewing the face of the avatar. " +
                 "(Uses position and rotation of the transform)")]
        [SerializeField] private Transform _cameraEditFaceTransform;

        [SerializeField] private Transform _cameraEditForearmTransform;
        [SerializeField] private Transform _cameraEditOuterArmTransform;
        [SerializeField] private Transform _cameraEditThighTransform;
        [SerializeField] private Transform _cameraEditCalfTransform;
        [SerializeField] private Transform _cameraEditBelowKneeTransform;
        [SerializeField] private Transform _cameraEditAboveKneeTransform;
        [SerializeField] private Transform _cameraEditBackTransform;
        [SerializeField] private Transform _cameraEditStomachTransform;

        [Header("Main Views")]
        [SerializeField] private GameObject _editorView;

        [SerializeField] private GameObject _eyesView;
        [SerializeField] private GameObject _lipsView;
        [SerializeField] private GameObject _noseView;
        [SerializeField] private GameObject _jawView;
        [SerializeField] private GameObject _hairView;
        [SerializeField] private GameObject _facialHairView;
        [SerializeField] private GameObject _eyebrowsView;
        [SerializeField] private GameObject _eyelashesView;

        [SerializeField] private GameObject _shirtsView;
        [SerializeField] private GameObject _hoodieView;
        [SerializeField] private GameObject _jacketView;
        [SerializeField] private GameObject _pantsView;
        [SerializeField] private GameObject _shortsView;
        [SerializeField] private GameObject _skirtsView;
        [SerializeField] private GameObject _dressesView;
        [SerializeField] private GameObject _shoesView;

        [SerializeField] private GameObject _glassesView;
        [SerializeField] private GameObject _earringsView;
        [SerializeField] private GameObject _hatsView;
        [SerializeField] private GameObject _masksView;

        [SerializeField] private GameObject _stickersView;
        [SerializeField] private GameObject _lipstickView;
        [SerializeField] private GameObject _frecklesView;
        [SerializeField] private GameObject _faceGemsView;
        [SerializeField] private GameObject _eyeshadowView;
        [SerializeField] private GameObject _blushView;

        [SerializeField] private GameObject _bodyView;
        [SerializeField] private GameObject _tattoosView;

        [Header("Button Containers")]
        [SerializeField] private GameObject _faceButtonsParent;
        [SerializeField] private GameObject _outfitButtonsParent;
        [SerializeField] private GameObject _accessoriesButtonsParent;
        [SerializeField] private GameObject _makeupButtonsParent;
        [SerializeField] private GameObject _tattooButtonsParent;

        [Header("Buttons")]
        [SerializeField] private Button _faceSectionButton;
        [SerializeField] private Button _bodySectionButton;
        [SerializeField] private Button _outfitSectionButton;
        [SerializeField] private Button _accessoriesSectionButton;
        [SerializeField] private Button _makeupSectionButton;
        [SerializeField] private Button _tattooSectionButton;

        [SerializeField] private Button _eyesButton;
        [SerializeField] private Button _lipsButton;
        [SerializeField] private Button _noseButton;
        [SerializeField] private Button _jawButton;
        [SerializeField] private Button _hairButton;
        [SerializeField] private Button _facialHairButton;
        [SerializeField] private Button _eyebrowsButton;
        [SerializeField] private Button _eyelashesButton;

        [SerializeField] private Button _shirtsButton;
        [SerializeField] private Button _hoodieButton;
        [SerializeField] private Button _jacketButton;
        [SerializeField] private Button _pantsButton;
        [SerializeField] private Button _shortsButton;
        [SerializeField] private Button _skirtsButton;
        [SerializeField] private Button _dressesButton;
        [SerializeField] private Button _shoesButton;

        [SerializeField] private Button _glassesButton;
        [SerializeField] private Button _earringsButton;
        [SerializeField] private Button _hatsButton;
        [SerializeField] private Button _masksButton;

        [SerializeField] private Button _stickersButton;
        [SerializeField] private Button _lipsticksButton;
        [SerializeField] private Button _frecklesButton;
        [SerializeField] private Button _faceGemsButton;
        [SerializeField] private Button _eyeshadowButton;
        [SerializeField] private Button _blushButton;

        [SerializeField] private Button _forearmButton;
        [SerializeField] private Button _outerArmButton;
        [SerializeField] private Button _thighButton;
        [SerializeField] private Button _calfButton;
        [SerializeField] private Button _belowKneeButton;
        [SerializeField] private Button _aboveKneeButton;
        [SerializeField] private Button _lowerBackButton;
        [SerializeField] private Button _stomachButton;

        [SerializeField] private GameObject _saveButton;

        [Header("Other")]
        [SerializeField] private BackgroundResizer _backgroundResizer;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private GameObject _divider;
        [SerializeField] private CameraController _cameraController;

        private GameObject _activeSection;

        private void SetActive(GameObject go, bool active)
        {
            if (go != null)
            {
                go.SetActive(active);
            }
        }

        private void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private void RemoveListeners(Button button)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        private void OnEnable()
        {
            if (_cameraEditFaceTransform == null || _cameraEditBodyTransform == null)
            {
                Debug.LogWarning("No positions are provided for _cameraEditFacePos or _cameraEditBodyPos " +
                                 "in the inspector. The camera will not move. Assign these at edit time " +
                                 "in order to change where the camera moves while using the editor.");
            }

            SetupNotifierEvents();
            SetupScreenTransitions();
        }

        public async UniTask DisplayInitialView()
        {
            SetActive(_editorView, true);
            SetActive(_saveButton, true);
            ShowSection(_eyesView);

            if (_cameraEditFaceTransform != null && _cameraController != null)
            {
                await _cameraController.MoveCameraAroundPivot(_cameraEditFaceTransform);
            }
        }

        public async UniTask CloseView()
        {
            HideMainView();

            if (_cameraController != null)
            {
                await _cameraController.MoveCameraToOriginalTransform();
            }
        }

        public void HideMainView()
        {
            HideInternalViews();
            SetActive(_editorView, false);
        }

        private void HideInternalViews()
        {
            SetActive(_eyesView, false);
            SetActive(_lipsView, false);
            SetActive(_noseView, false);
            SetActive(_jawView, false);
            SetActive(_hairView, false);
            SetActive(_facialHairView, false);
            SetActive(_eyebrowsView, false);
            SetActive(_eyelashesView, false);

            SetActive(_shirtsView, false);
            SetActive(_hoodieView, false);
            SetActive(_jacketView, false);
            SetActive(_pantsView, false);
            SetActive(_shortsView, false);
            SetActive(_skirtsView, false);
            SetActive(_dressesView, false);
            SetActive(_shoesView, false);

            SetActive(_glassesView, false);
            SetActive(_earringsView, false);
            SetActive(_hatsView, false);
            SetActive(_masksView, false);

            SetActive(_stickersView, false);
            SetActive(_lipstickView, false);
            SetActive(_frecklesView, false);
            SetActive(_faceGemsView, false);
            SetActive(_eyeshadowView, false);
            SetActive(_blushView, false);

            SetActive(_bodyView, false);
            SetActive(_tattoosView, false);
        }

        private void HideButtonViews()
        {
            SetActive(_faceButtonsParent, false);
            SetActive(_outfitButtonsParent, false);
            SetActive(_accessoriesButtonsParent, false);
            SetActive(_makeupButtonsParent, false);
            SetActive(_tattooButtonsParent, false);
            SetActive(_bodyView, false);
            SetActive(_tattoosView, false);
        }

        private void ShowSection(GameObject section)
        {
            if (section == null || _activeSection == section)
            {
                return;
            }

            HideInternalViews();

            section.SetActive(true);
            _activeSection = section;

            if (_backgroundResizer != null)
            {
                _backgroundResizer.SetSizeChangeNotifier(section.GetComponent<RectTransformSizeChangeNotifier>());
            }

            if (_scrollRect != null)
            {
                _scrollRect.content = section.GetComponent<RectTransform>();
            }
        }

        private void SetupNotifierEvents()
        {
            AddListener(_eyesButton, () => ShowSection(_eyesView));
            AddListener(_lipsButton, () => ShowSection(_lipsView));
            AddListener(_noseButton, () => ShowSection(_noseView));
            AddListener(_jawButton, () => ShowSection(_jawView));
            AddListener(_hairButton, () => ShowSection(_hairView));
            AddListener(_facialHairButton, () => ShowSection(_facialHairView));
            AddListener(_eyebrowsButton, () => ShowSection(_eyebrowsView));
            AddListener(_eyelashesButton, () => ShowSection(_eyelashesView));

            AddListener(_shirtsButton, () => ShowSection(_shirtsView));
            AddListener(_hoodieButton, () => ShowSection(_hoodieView));
            AddListener(_jacketButton, () => ShowSection(_jacketView));
            AddListener(_pantsButton, () => ShowSection(_pantsView));
            AddListener(_shortsButton, () => ShowSection(_shortsView));
            AddListener(_skirtsButton, () => ShowSection(_skirtsView));
            AddListener(_dressesButton, () => ShowSection(_dressesView));
            AddListener(_shoesButton, () => ShowSection(_shoesView));

            AddListener(_glassesButton, () => ShowSection(_glassesView));
            AddListener(_earringsButton, () => ShowSection(_earringsView));
            AddListener(_hatsButton, () => ShowSection(_hatsView));
            AddListener(_masksButton, () => ShowSection(_masksView));

            AddListener(_stickersButton, () => ShowSection(_stickersView));
            AddListener(_lipsticksButton, () => ShowSection(_lipstickView));
            AddListener(_frecklesButton, () => ShowSection(_frecklesView));
            AddListener(_faceGemsButton, () => ShowSection(_faceGemsView));
            AddListener(_eyeshadowButton, () => ShowSection(_eyeshadowView));
            AddListener(_blushButton, () => ShowSection(_blushView));

            AddListener(_forearmButton, () => ShowTattooSection(_cameraEditForearmTransform));
            AddListener(_outerArmButton, () => ShowTattooSection(_cameraEditOuterArmTransform));
            AddListener(_thighButton, () => ShowTattooSection(_cameraEditThighTransform));
            AddListener(_calfButton, () => ShowTattooSection(_cameraEditCalfTransform));
            AddListener(_aboveKneeButton, () => ShowTattooSection(_cameraEditAboveKneeTransform));
            AddListener(_belowKneeButton, () => ShowTattooSection(_cameraEditBelowKneeTransform));
            AddListener(_stomachButton, () => ShowTattooSection(_cameraEditStomachTransform));
            AddListener(_lowerBackButton, () => ShowTattooSection(_cameraEditBackTransform));
        }

        private void ShowTattooSection(Transform cameraTransform)
        {
            ShowSection(_tattoosView);

            if (_cameraController != null && cameraTransform != null)
            {
                _cameraController.MoveCameraAroundPivot(cameraTransform).Forget();
            }
        }

        private void SetupScreenTransitions()
        {
            AddListener(_faceSectionButton, () =>
            {
                HideButtonViews();
                SetActive(_divider, true);
                SetActive(_faceButtonsParent, true);
                ShowSection(_eyesView);
                MoveCameraTo(_cameraEditFaceTransform);
            });

            AddListener(_outfitSectionButton, () =>
            {
                HideButtonViews();
                SetActive(_divider, true);
                SetActive(_outfitButtonsParent, true);
                ShowSection(_shirtsView);
                MoveCameraTo(_cameraEditBodyTransform);
            });

            AddListener(_accessoriesSectionButton, () =>
            {
                HideButtonViews();
                SetActive(_divider, true);
                SetActive(_accessoriesButtonsParent, true);
                ShowSection(_glassesView);
                MoveCameraTo(_cameraEditFaceTransform);
            });

            AddListener(_bodySectionButton, () =>
            {
                HideButtonViews();
                SetActive(_divider, false);
                SetActive(_bodyView, true);
                ShowSection(_bodyView);
                MoveCameraTo(_cameraEditBodyTransform);
            });

            AddListener(_makeupSectionButton, () =>
            {
                HideButtonViews();
                SetActive(_divider, true);
                SetActive(_makeupButtonsParent, true);
                ShowSection(_stickersView);
                MoveCameraTo(_cameraEditFaceTransform);
            });

            AddListener(_tattooSectionButton, () =>
            {
                HideButtonViews();
                SetActive(_divider, true);
                SetActive(_tattooButtonsParent, true);
                SetActive(_tattoosView, true);
                ShowSection(_tattoosView);
                MoveCameraTo(_cameraEditForearmTransform);
            });
        }

        private void MoveCameraTo(Transform target)
        {
            if (_cameraController != null && target != null)
            {
                _cameraController.MoveCameraAroundPivot(target).Forget();
            }
        }

        private void OnDisable()
        {
            RemoveListeners(_eyesButton);
            RemoveListeners(_lipsButton);
            RemoveListeners(_noseButton);
            RemoveListeners(_jawButton);
            RemoveListeners(_hairButton);
            RemoveListeners(_facialHairButton);
            RemoveListeners(_eyebrowsButton);
            RemoveListeners(_eyelashesButton);

            RemoveListeners(_shirtsButton);
            RemoveListeners(_hoodieButton);
            RemoveListeners(_jacketButton);
            RemoveListeners(_pantsButton);
            RemoveListeners(_shortsButton);
            RemoveListeners(_skirtsButton);
            RemoveListeners(_dressesButton);
            RemoveListeners(_shoesButton);

            RemoveListeners(_glassesButton);
            RemoveListeners(_earringsButton);
            RemoveListeners(_hatsButton);
            RemoveListeners(_masksButton);

            RemoveListeners(_stickersButton);
            RemoveListeners(_lipsticksButton);
            RemoveListeners(_frecklesButton);
            RemoveListeners(_faceGemsButton);
            RemoveListeners(_eyeshadowButton);
            RemoveListeners(_blushButton);

            RemoveListeners(_forearmButton);
            RemoveListeners(_outerArmButton);
            RemoveListeners(_thighButton);
            RemoveListeners(_calfButton);
            RemoveListeners(_aboveKneeButton);
            RemoveListeners(_belowKneeButton);
            RemoveListeners(_stomachButton);
            RemoveListeners(_lowerBackButton);

            RemoveListeners(_faceSectionButton);
            RemoveListeners(_bodySectionButton);
            RemoveListeners(_outfitSectionButton);
            RemoveListeners(_accessoriesSectionButton);
            RemoveListeners(_makeupSectionButton);
            RemoveListeners(_tattooSectionButton);
        }
    }
}
