using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Genies.Sdk.Avatar.Samples.CustomAvatarEditor
{
    /// <summary>
    /// Maps body-part buttons to <see cref="MegaSkinTattooSlot"/> values and applies the
    /// selected slot to a <see cref="UIGeneratorGroup"/>. Place this component alongside
    /// (or as a child/parent of) the target group and wire up the buttons in the inspector.
    /// </summary>
    public class TattooSlotSelector : MonoBehaviour
    {
        [SerializeField] private UIGeneratorGroup _targetGroup;

        [Header("Body Part Buttons")]
        [SerializeField] private Button _forearmButton;
        [SerializeField] private Button _outerArmButton;
        [SerializeField] private Button _thighButton;
        [SerializeField] private Button _aboveKneeButton;
        [SerializeField] private Button _calfButton;
        [SerializeField] private Button _belowKneeButton;
        [SerializeField] private Button _lowerBackButton;
        [SerializeField] private Button _stomachButton;

        private UnityAction[] _listeners;

        private (Button button, MegaSkinTattooSlot slot)[] GetMappings() => new[]
        {
            (_forearmButton, MegaSkinTattooSlot.LeftTopForearm),
            (_outerArmButton, MegaSkinTattooSlot.LeftTopOuterArm),
            (_thighButton, MegaSkinTattooSlot.RightSideThigh),
            (_aboveKneeButton, MegaSkinTattooSlot.RightSideAboveTheKnee),
            (_calfButton, MegaSkinTattooSlot.LeftSideCalf),
            (_belowKneeButton, MegaSkinTattooSlot.LeftSideBelowKnee),
            (_lowerBackButton, MegaSkinTattooSlot.LowerBack),
            (_stomachButton, MegaSkinTattooSlot.LowerStomach),
        };

        private void OnEnable()
        {
            if (_targetGroup == null)
            {
                _targetGroup = GetComponentInParent<UIGeneratorGroup>()
                               ?? GetComponentInChildren<UIGeneratorGroup>();
            }

            var mappings = GetMappings();
            _listeners = new UnityAction[mappings.Length];

            for (int i = 0; i < mappings.Length; i++)
            {
                if (mappings[i].button == null)
                {
                    continue;
                }

                var slot = mappings[i].slot;
                _listeners[i] = () => _targetGroup.TattooSlot = slot;
                mappings[i].button.onClick.AddListener(_listeners[i]);
            }
        }

        private void OnDisable()
        {
            if (_listeners == null)
            {
                return;
            }

            var mappings = GetMappings();
            for (int i = 0; i < mappings.Length; i++)
            {
                if (mappings[i].button != null && _listeners[i] != null)
                {
                    mappings[i].button.onClick.RemoveListener(_listeners[i]);
                }
            }
        }
    }
}
