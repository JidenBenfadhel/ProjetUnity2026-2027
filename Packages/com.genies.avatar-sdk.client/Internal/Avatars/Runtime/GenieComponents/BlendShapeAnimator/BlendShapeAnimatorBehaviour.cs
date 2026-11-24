using System;
using System.Collections.Generic;
using Genies.Utilities;
using UnityEngine;
using ChannelRetargetBehavior = Genies.Avatars.BlendShapeAnimatorConfig.ChannelRetargetBehavior;
using Channel = Genies.Avatars.BlendShapeAnimatorConfig.Channel;
using DrivenAttribute = Genies.Avatars.BlendShapeAnimatorConfig.DrivenAttribute;

namespace Genies.Avatars
{
    /// <summary>
    /// Maps animator parameter to blend shape weights on one or multiple <see cref="SkinnedMeshRenderer"/> components.
    /// The parameter-to-blendshape mapping is given by the referenced <see cref="BlendShapeAnimatorConfig"/> asset.
    /// </summary>
    [RequireComponent(typeof(Animator))]
#if GENIES_SDK && !GENIES_INTERNAL
    [AddComponentMenu("")]
    internal sealed class BlendShapeAnimatorBehaviour : MonoBehaviour
#else
    public sealed class BlendShapeAnimatorBehaviour : MonoBehaviour
#endif
    {
        public BlendShapeAnimatorConfig config;
        [SerializeField] private List<SkinnedMeshRenderer> renderers = new();

        public List<SkinnedMeshRenderer> Renderers => renderers;

        // state
        private readonly List<DrivenAttrData> _drivenAttrData = new();
        private readonly List<BlendShapeTargetData> _blendShapeTargets = new();
        private readonly Dictionary<RendererBlendshape, int> _blendShapeTargetIndices = new();

        private Animator _animator;
        private AnimatorParameters _animatorParameters;
        private RuntimeAnimatorController _cachedRuntimeAnimatorController;
        private float[] _accumulatedWeights = Array.Empty<float>();
        private float[] _lastAppliedWeights = Array.Empty<float>();

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (!_animator)
            {
                Debug.LogError($"[{nameof(BlendShapeAnimatorBehaviour)}] missing Animator component");
            }

            _animatorParameters = new AnimatorParameters(_animator);

            RebuildMappings();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                RebuildMappings();
            }
        }

        private void OnDestroy()
        {
            RestoreInitialWeights();
            _drivenAttrData.Clear();
        }

        /// <summary>
        /// Call this to rebuild the mappings if the config or renderers changed.
        /// </summary>
        /// <param name="includedParameters">If provided, only channels mapped to parameters contained here will be
        /// mapped. This can be used to prevent some warning/error logs happening on each LateUpdate.</param>
        public void RebuildMappings(AnimatorParameters includedParameters = null)
        {
            ResetMappings();
            if (!CanBuildMappings())
            {
                return;
            }

            RefreshAnimatorParameters(force: true);
            BuildMappings(includedParameters ?? _animatorParameters);
            ResizeRuntimeBuffers();
        }

        private void LateUpdate()
        {
            if (!CanAnimate())
            {
                return;
            }

            RefreshAnimatorParameters();
            if (_blendShapeTargets.Count == 0)
            {
                return;
            }

            ResetAccumulatedWeights();
            AccumulateDrivenAttributeWeights();
            ApplyBlendShapeTargets();
        }

        private void RestoreInitialWeights()
        {
            foreach (DrivenAttrData data in _drivenAttrData)
            {
                if (!data.Renderer || !data.Mesh || data.Renderer.sharedMesh != data.Mesh)
                {
                    continue;
                }

                var blendShapeCount = data.Mesh.blendShapeCount;
                if (data.BlendShapeIndex >= 0 && data.BlendShapeIndex < blendShapeCount)
                {
                    data.Renderer.SetBlendShapeWeight(data.BlendShapeIndex, data.InitialBlendShapeWeight);
                }
            }
        }

        private void CreateDrivenAttributeData(string inputChannelName, string blendShapeName, ChannelRetargetBehavior behavior, float targetWeight)
        {
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in renderers)
            {
                if (!TryGetBlendShapeInfo(skinnedMeshRenderer, blendShapeName, out BlendShapeInfo blendShapeInfo))
                {
                    continue;
                }

                var animatorParameterId = Animator.StringToHash(inputChannelName);
                var targetIndex = GetOrCreateBlendShapeTarget(
                    skinnedMeshRenderer,
                    blendShapeInfo.Mesh,
                    blendShapeInfo.BlendShapeIndex,
                    blendShapeInfo.MaxWeight,
                    blendShapeInfo.InitialWeight);

                _drivenAttrData.Add(new DrivenAttrData(
                    skinnedMeshRenderer,
                    blendShapeInfo.Mesh,
                    blendShapeInfo.BlendShapeIndex,
                    blendShapeInfo.MaxWeight,
                    blendShapeInfo.InitialWeight,
                    animatorParameterId,
                    behavior,
                    targetWeight,
                    targetIndex));
            }
        }

        private void ResetMappings()
        {
            RestoreInitialWeights();
            _drivenAttrData.Clear();
            _blendShapeTargets.Clear();
            _blendShapeTargetIndices.Clear();
        }

        private bool CanBuildMappings()
        {
            return config && _animatorParameters != null;
        }

        private void BuildMappings(AnimatorParameters mappingParameters)
        {
            foreach (Channel channel in config.channels)
            {
                if (channel?.inputChannelName == null)
                {
                    continue;
                }

                if (!mappingParameters.Contains(channel.inputChannelName))
                {
                    continue;
                }

                foreach (DrivenAttribute drivenAttr in channel.drivenAttributes)
                {
                    if (drivenAttr == null)
                    {
                        continue;
                    }

                    CreateDrivenAttributeDataForSubmeshes(channel.inputChannelName, drivenAttr);

                    // glTF exports have all submeshes merged into a single blend shape, this line will support that
                    CreateDrivenAttributeData(
                        channel.inputChannelName,
                        drivenAttr.outputChannelName,
                        drivenAttr.retargetBehavior,
                        drivenAttr.targetWeight);
                }
            }
        }

        private void CreateDrivenAttributeDataForSubmeshes(string inputChannelName, DrivenAttribute drivenAttr)
        {
            if (drivenAttr == null)
            {
                return;
            }

            foreach (var submesh in drivenAttr.targetSubmeshes)
            {
                if (submesh == null)
                {
                    continue;
                }

                var blendShapeName = $"{submesh}_blendShape.{drivenAttr.outputChannelName}";
                CreateDrivenAttributeData(inputChannelName, blendShapeName, drivenAttr.retargetBehavior, drivenAttr.targetWeight);
            }
        }

        private bool CanAnimate()
        {
            return _animator && _animator.enabled && _animator.runtimeAnimatorController;
        }

        private void ResetAccumulatedWeights()
        {
            Array.Clear(_accumulatedWeights, 0, _blendShapeTargets.Count);
        }

        private void AccumulateDrivenAttributeWeights()
        {
            if (_drivenAttrData == null || _drivenAttrData.Count == 0)
            {
                return;
            }

            foreach (DrivenAttrData data in _drivenAttrData)
            {
                if (!CanDriveBlendShape(data) || !_animatorParameters.Contains(data.AnimatorParameterId))
                {
                    continue;
                }

                _accumulatedWeights[data.TargetIndex] += GetDrivenWeight(data);
            }
        }

        private void ApplyBlendShapeTargets()
        {
            for (var i = 0; i < _blendShapeTargets.Count; ++i)
            {
                BlendShapeTargetData target = _blendShapeTargets[i];
                if (!IsValidTarget(target))
                {
                    continue;
                }

                var value = Mathf.Clamp(_accumulatedWeights[i], target.InitialBlendShapeWeight, target.MaxBlendShapeWeight);
                if (Mathf.Approximately(_lastAppliedWeights[i], value))
                {
                    continue;
                }

                target.Renderer.SetBlendShapeWeight(target.BlendShapeIndex, value);
                _lastAppliedWeights[i] = value;
            }
        }

        private static bool CanDriveBlendShape(DrivenAttrData data)
        {
            return data.Renderer && data.Renderer.sharedMesh == data.Mesh;
        }

        private float GetDrivenWeight(DrivenAttrData data)
        {
            var value = _animator.GetFloat(data.AnimatorParameterId);
            value = data.Behaviour switch
            {
                ChannelRetargetBehavior.PositiveControl => value > 0.0f ? value : 0.0f,
                ChannelRetargetBehavior.NegativeControl => value < 0.0f ? -value : 0.0f,
                ChannelRetargetBehavior.TargetWeight => Mathf.Lerp(data.InitialBlendShapeWeight, data.TargetWeight, value),
                _ => value,
            };

            return value * 100;
        }

        private static bool IsValidTarget(BlendShapeTargetData target)
        {
            return target.Renderer && target.Renderer.sharedMesh == target.Mesh;
        }

        private static bool TryGetBlendShapeInfo(SkinnedMeshRenderer renderer, string blendShapeName, out BlendShapeInfo blendShapeInfo)
        {
            blendShapeInfo = default;
            if (!renderer)
            {
                return false;
            }

            Mesh mesh = renderer.sharedMesh;
            if (!mesh)
            {
                return false;
            }

            var blendShapeIndex = mesh.GetBlendShapeIndex(blendShapeName);
            if (blendShapeIndex < 0)
            {
                return false;
            }

            var lastFrameIndex = mesh.GetBlendShapeFrameCount(blendShapeIndex) - 1;
            blendShapeInfo = new BlendShapeInfo(
                mesh,
                blendShapeIndex,
                mesh.GetBlendShapeFrameWeight(blendShapeIndex, lastFrameIndex),
                renderer.GetBlendShapeWeight(blendShapeIndex));
            return true;
        }

        private int GetOrCreateBlendShapeTarget(SkinnedMeshRenderer skinnedMeshRenderer, Mesh mesh, int blendShapeIndex, float maxWeight, float initialWeight)
        {
            var key = new RendererBlendshape(skinnedMeshRenderer, blendShapeIndex);
            if (_blendShapeTargetIndices.TryGetValue(key, out var existingIndex))
            {
                return existingIndex;
            }

            var targetIndex = _blendShapeTargets.Count;
            _blendShapeTargets.Add(new BlendShapeTargetData(skinnedMeshRenderer, mesh, blendShapeIndex, maxWeight, initialWeight));
            _blendShapeTargetIndices[key] = targetIndex;
            return targetIndex;
        }

        private void RefreshAnimatorParameters(bool force = false)
        {
            if (_animatorParameters == null || _animator == null)
            {
                return;
            }

            RuntimeAnimatorController controller = _animator.runtimeAnimatorController;
            if (!force && ReferenceEquals(_cachedRuntimeAnimatorController, controller))
            {
                return;
            }

            _cachedRuntimeAnimatorController = controller;
            _animatorParameters.Refresh();
        }

        private void ResizeRuntimeBuffers()
        {
            var targetCount = _blendShapeTargets.Count;
            _accumulatedWeights = new float[targetCount];
            _lastAppliedWeights = new float[targetCount];

            for (var i = 0; i < targetCount; ++i)
            {
                _lastAppliedWeights[i] = float.NaN;
            }
        }

        private readonly struct DrivenAttrData
        {
            public readonly SkinnedMeshRenderer Renderer;
            public readonly Mesh Mesh;
            public readonly int BlendShapeIndex;
            public readonly float MaxBlendShapeWeight;
            public readonly float InitialBlendShapeWeight;
            public readonly int AnimatorParameterId;
            public readonly ChannelRetargetBehavior Behaviour;
            public readonly float TargetWeight;
            public readonly int TargetIndex;

            public DrivenAttrData(SkinnedMeshRenderer renderer, Mesh mesh, int blendShapeIndex, float maxBlendShapeWeight,
                float initialBlendShapeWeight, int animatorParameterId, ChannelRetargetBehavior behaviour, float targetWeight, int targetIndex)
            {
                Renderer = renderer;
                Mesh = mesh;
                BlendShapeIndex = blendShapeIndex;
                MaxBlendShapeWeight = maxBlendShapeWeight;
                InitialBlendShapeWeight = initialBlendShapeWeight;
                AnimatorParameterId = animatorParameterId;
                Behaviour = behaviour;
                TargetWeight = targetWeight;
                TargetIndex = targetIndex;
            }
        }

        private readonly struct BlendShapeTargetData
        {
            public readonly SkinnedMeshRenderer Renderer;
            public readonly Mesh Mesh;
            public readonly int BlendShapeIndex;
            public readonly float MaxBlendShapeWeight;
            public readonly float InitialBlendShapeWeight;

            public BlendShapeTargetData(SkinnedMeshRenderer renderer, Mesh mesh, int blendShapeIndex,
                float maxBlendShapeWeight, float initialBlendShapeWeight)
            {
                Renderer = renderer;
                Mesh = mesh;
                BlendShapeIndex = blendShapeIndex;
                MaxBlendShapeWeight = maxBlendShapeWeight;
                InitialBlendShapeWeight = initialBlendShapeWeight;
            }
        }

        private readonly struct BlendShapeInfo
        {
            public readonly Mesh Mesh;
            public readonly int BlendShapeIndex;
            public readonly float MaxWeight;
            public readonly float InitialWeight;

            public BlendShapeInfo(Mesh mesh, int blendShapeIndex, float maxWeight, float initialWeight)
            {
                Mesh = mesh;
                BlendShapeIndex = blendShapeIndex;
                MaxWeight = maxWeight;
                InitialWeight = initialWeight;
            }
        }

        private readonly struct RendererBlendshape : IEquatable<RendererBlendshape>
        {
            private readonly SkinnedMeshRenderer _renderer;
            private readonly int _blendShapeIndex;

            public RendererBlendshape(SkinnedMeshRenderer renderer, int blendShapeIndex)
            {
                _renderer = renderer;
                _blendShapeIndex = blendShapeIndex;
            }

            public bool Equals(RendererBlendshape other)
            {
                return (_renderer?.GetInstanceID() ?? 0) == (other._renderer?.GetInstanceID() ?? 0) &&
                       _blendShapeIndex == other._blendShapeIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is RendererBlendshape other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + (_renderer != null ? _renderer.GetInstanceID() : 0);
                    hash = hash * 23 + _blendShapeIndex;
                    return hash;
                }
            }
            public static bool operator ==(RendererBlendshape left, RendererBlendshape right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(RendererBlendshape left, RendererBlendshape right)
            {
                return !(left == right);
            }
        }
    }
}
