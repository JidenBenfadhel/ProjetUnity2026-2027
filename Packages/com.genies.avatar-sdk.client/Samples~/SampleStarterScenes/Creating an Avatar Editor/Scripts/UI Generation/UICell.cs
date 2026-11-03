using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Genies.Sdk.Avatar.Samples.CustomAvatarEditor
{
    /// <summary>
    /// Represents an instantiated cell in the avatar editor UI.
    /// Purely presentational — handles displaying a sprite, color swatch, or slider
    /// and delegates all business logic to callbacks provided at setup time.
    ///
    /// Subscribe to <see cref="OnEnableCalled"/>, <see cref="Clicked"/>, and <see cref="SliderMoved"/>
    /// events to layer on animations, sounds, or any other external behavior.
    /// </summary>
    public class UICell : MonoBehaviour
    {
        public enum CellType
        {
            Asset,
            Color,
            Slider,
            Button
        }

        [Tooltip("The cell type determines what happens when this cell is clicked.")]
        public CellType CellCategory = CellType.Asset;
        [SerializeField] private Image _cellImage;
        [SerializeField] private GameObject _loadingIcon;
        [SerializeField] private TMP_Text _statisticText;

        /// <summary>Invoked once when the cell is enabled (e.g. after instantiation).</summary>
        public event Action OnEnableCalled;

        /// <summary>Invoked when the cell's button is clicked, before the async action runs.</summary>
        public event Action Clicked;

        /// <summary>Invoked when the cell's slider value changes, passing the new value.</summary>
        public event Action<float> SliderMoved;

        private Button _button;
        private Slider _slider;
        private Func<UniTask> _onClicked;
        private Action<float> _onSliderValueChanged;
        private static bool _isLoading;

        #region Initial Setup

        private void Awake()
        {
            _button = GetComponentInChildren<Button>();
            _slider = GetComponentInChildren<Slider>();
        }

        private void OnEnable()
        {
            if (CellCategory != CellType.Slider && _button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }

            if (CellCategory == CellType.Slider)
            {
                _slider.onValueChanged.AddListener(OnSliderValueChanged);
            }

            OnEnableCalled?.Invoke();
        }

        /// <summary>
        /// Sets up this cell to display a sprite-based asset (wearable, feature, makeup, "none", etc.)
        /// </summary>
        /// <param name="icon">The sprite to display.</param>
        /// <param name="onClicked">Async callback invoked when the cell is clicked.</param>
        /// <param name="tint">Optional image tint (e.g. <see cref="Color.black"/> for a "none" cell).</param>
        public void SetUpCellAsAsset(Sprite icon, Func<UniTask> onClicked, Color? tint = null)
        {
            CellCategory = CellType.Asset;

            if (_cellImage != null)
            {
                _cellImage.sprite = icon;
                if (tint.HasValue)
                {
                    _cellImage.color = tint.Value;
                }
            }

            _onClicked = onClicked;
        }

        /// <summary>
        /// Sets up this cell to display a color swatch.
        /// </summary>
        /// <param name="color">The color to fill the cell image with.</param>
        /// <param name="onClicked">Async callback invoked when the cell is clicked.</param>
        public void SetUpCellAsColor(Color color, Func<UniTask> onClicked)
        {
            CellCategory = CellType.Color;

            if (_cellImage != null)
            {
                _cellImage.color = color;
            }

            _onClicked = onClicked;
        }

        /// <summary>
        /// Sets up this cell as a labeled slider.
        /// </summary>
        /// <param name="label">Text displayed next to the slider.</param>
        /// <param name="value">Initial slider value.</param>
        /// <param name="onValueChanged">Callback invoked each time the slider moves.</param>
        public void SetUpCellAsSlider(string label, float value, Action<float> onValueChanged)
        {
            CellCategory = CellType.Slider;

            if (_statisticText != null)
            {
                _statisticText.text = label;
            }

            if (_slider != null)
            {
                _slider.value = value;
            }

            _onSliderValueChanged = onValueChanged;
        }

        #endregion

        #region Button/Slider Logic

        private void OnButtonClicked()
        {
            if (_isLoading)
            {
                return;
            }

            if (_onClicked != null)
            {
                ExecuteClickActionAsync().Forget();
            }

            Clicked?.Invoke();
        }

        private async UniTask ExecuteClickActionAsync()
        {
            _isLoading = true;
            _button.interactable = false;

            if (_loadingIcon != null)
            {
                _loadingIcon.SetActive(true);
            }

            try
            {
                await _onClicked();
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
            finally
            {
                _isLoading = false;

                if (this != null)
                {
                    _button.interactable = true;

                    if (_loadingIcon != null)
                    {
                        _loadingIcon.SetActive(false);
                    }
                }
            }
        }

        private void OnSliderValueChanged(float value)
        {
            SliderMoved?.Invoke(value);
            _onSliderValueChanged?.Invoke(value);
        }

        #endregion

        #region Disposal

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }

            if (_slider != null)
            {
                _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }
        }

        #endregion
    }
}
