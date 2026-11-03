using System;
using UnityEngine;

namespace Genies.Sdk.Avatar.Samples.CustomAvatarEditor
{
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformSizeChangeNotifier : MonoBehaviour
    {
        public event Action<RectTransform> OnSizeChanged;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnRectTransformDimensionsChange()
        {
            OnSizeChanged?.Invoke(_rectTransform);
        }
    }
}
