using System;
using UnityEngine;

namespace Genies.Sdk.Avatar.Samples.CustomAvatarEditor
{
    public class BackgroundResizer : MonoBehaviour
    {
        [SerializeField] private RectTransformSizeChangeNotifier _sizeChangeNotifier;

        [SerializeField] private RectTransform _outerBackground;
        [SerializeField] private RectTransform _innerBackground;
        [SerializeField] private RectTransform _innerBackgroundDivider;
        [SerializeField] private RectTransform _scrollBar;

        [SerializeField] private float _outerBackgroundHeightOffset;
        [SerializeField] private float _innerBackgroundHeightOffset;
        [SerializeField] private float _scrollBarHeightOffset;

        [SerializeField] private float _outerBackgroundDefaultHeight;
        [SerializeField] private float _innerBackgroundDefaultHeight;

        [SerializeField] private float _maxOuterBackgroundHeight;
        [SerializeField] private float _maxInnerBackgroundHeight;

        private void Start()
        {
            SetBackgroundToDefaultWidth();
            _sizeChangeNotifier.OnSizeChanged += ResizeBackground;
        }

        private void SetBackgroundToDefaultWidth()
        {
            // TODO animate to this
            _outerBackground.sizeDelta = new Vector2(_outerBackground.sizeDelta.x, _outerBackgroundDefaultHeight);
            _innerBackground.sizeDelta = new Vector2(_innerBackground.sizeDelta.x, _innerBackgroundDefaultHeight);
            _innerBackgroundDivider.sizeDelta = new Vector2(_innerBackgroundDivider.sizeDelta.x, _innerBackgroundDefaultHeight);
            _scrollBar.sizeDelta = new Vector2(_scrollBar.sizeDelta.x, _innerBackgroundDefaultHeight + _scrollBarHeightOffset);
        }

        private void ResizeBackground(RectTransform rectTransform)
        {
            // TODO animate to this
            _outerBackground.sizeDelta = new Vector2(
                _outerBackground.sizeDelta.x,
                Mathf.Clamp(rectTransform.rect.height + _outerBackgroundHeightOffset, _outerBackgroundDefaultHeight, _maxOuterBackgroundHeight));

            var innerBackgroundHeight = rectTransform.rect.height + _innerBackgroundHeightOffset;

            _innerBackground.sizeDelta = new Vector2(
                _innerBackground.sizeDelta.x,
                Mathf.Clamp(innerBackgroundHeight, _innerBackgroundDefaultHeight, _maxInnerBackgroundHeight));

            _innerBackgroundDivider.sizeDelta = new Vector2(
                _innerBackgroundDivider.sizeDelta.x,
                Mathf.Clamp(innerBackgroundHeight, _innerBackgroundDefaultHeight, _maxInnerBackgroundHeight));

            _scrollBar.sizeDelta = new Vector2(
                _scrollBar.sizeDelta.x,
                Mathf.Clamp(innerBackgroundHeight + _scrollBarHeightOffset, _innerBackgroundDefaultHeight + _scrollBarHeightOffset, _maxInnerBackgroundHeight + _scrollBarHeightOffset));
        }

        public void SetSizeChangeNotifier(RectTransformSizeChangeNotifier sizeChangeNotifier)
        {
            _sizeChangeNotifier.OnSizeChanged -= ResizeBackground;
            _sizeChangeNotifier = sizeChangeNotifier;
            _sizeChangeNotifier.OnSizeChanged += ResizeBackground;
            ResizeBackground(sizeChangeNotifier.GetComponent<RectTransform>());
        }

        private void OnDestroy()
        {
            _sizeChangeNotifier.OnSizeChanged -= ResizeBackground;
        }
    }
}
