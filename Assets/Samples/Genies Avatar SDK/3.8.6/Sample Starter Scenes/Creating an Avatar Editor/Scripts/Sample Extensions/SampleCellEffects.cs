using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Genies.Sdk.Avatar.Samples.CustomAvatarEditor
{
    /// <summary>
    /// Sample component that adds animations and sound effects to generated UI cells.
    /// This demonstrates how to extend <see cref="UICell"/> and <see cref="UIGenerator"/>
    /// behavior through their events without modifying the core prefab components.
    ///
    /// Attach this component alongside your <see cref="UIGenerator"/>(s) and assign
    /// them to <see cref="_generators"/>. When cells are created, this component will:
    /// <list type="bullet">
    ///   <item>Play a spawn sound and scale-bounce animation</item>
    ///   <item>Start per-cell idle animations (sine-wave rotation and scale)</item>
    ///   <item>Play a click sound and squish animation on cell clicks</item>
    ///   <item>Play slider tick sounds on slider drags</item>
    ///   <item>Have the cells spawn in succession instead of all at once</item>
    /// </list>
    ///
    /// Developers can use this as a reference for hooking their own logic into the
    /// same events.
    /// </summary>
    public class SampleCellEffects : MonoBehaviour
    {
        [Header("Generators to enhance with effects")]
        [SerializeField] private List<UIGeneratorGroup> _generatorGroups = new();

        [Header("Buttons with UICell components to enhance with effects")]
        [SerializeField] private List<UICell> _buttons = new();

        [Header("Spawn Animation")]
        [Tooltip("When enabled, cells animate in with a scale bounce on spawn.")]
        [SerializeField] private bool _useSpawnEffect = true;

        [Header("Idle Animation")]
        [Tooltip("Enable idle rotation oscillation on cells.")]
        [SerializeField] private bool _enableIdleRotation = true;
        [Tooltip("Enable idle scale oscillation on cells.")]
        [SerializeField] private bool _enableIdleScale = true;
        [Tooltip("Enable idle rotation oscillation on buttons.")]
        [SerializeField] private bool _enableButtonIdleRotation = false;
        [Tooltip("Enable idle rotation oscillation on buttons.")]
        [SerializeField] private bool _enableButtonIdleScale = true;
        [SerializeField] private bool _enableSoundEffects = true;

        [Header("Spawn Order")]
        [Tooltip("How the cells will spawn (all at once, one by one, or each section in order)")]
        [SerializeField] private UIGeneratorGroup.GenerationType _generationType = UIGeneratorGroup.GenerationType.CellByCell;

        /// <summary>
        /// Minimum rotation degrees before writing to the transform.
        /// Avoids dirtying the Canvas for sub-pixel changes.
        /// </summary>
        private const float RotationUpdateThreshold = 0.05f;

        /// <summary>
        /// Minimum uniform-scale change before writing to the transform.
        /// </summary>
        private const float ScaleUpdateThreshold = 0.0005f;

        private readonly List<CellIdleState> _idleCells = new();
        private readonly Dictionary<UICell, Action> _spawnHandlers = new();

        #region Lifecycle

        private void Awake()
        {
            foreach (var generatorGroup in _generatorGroups)
            {
                generatorGroup.GenerationMode = _generationType;

                foreach (var generator in generatorGroup.generators)
                {
                    if (generator != null)
                    {
                        generator.CellCreated += OnCellCreated;
                    }
                }
            }

            foreach (var button in _buttons)
            {
                Action handler = () => OnButtonCreated(button);

                _spawnHandlers[button] = handler;
                button.OnEnableCalled += handler;
            }
        }

        private void OnDestroy()
        {
            foreach (var generatorGroup in _generatorGroups)
            {
                foreach (var generator in generatorGroup.generators)
                {
                    if (generator != null)
                    {
                        generator.CellCreated -= OnCellCreated;
                    }
                }

                foreach (var button in _buttons)
                {
                    if (_spawnHandlers.TryGetValue(button, out var handler))
                    {
                        button.OnEnableCalled -= handler;
                    }
                }

                _spawnHandlers.Clear();
            }

            // Cancel all running per-cell animations
            foreach (var state in _idleCells)
            {
                state.AnimationCts?.Cancel();
                state.AnimationCts?.Dispose();
            }

            _idleCells.Clear();
        }

        private void Update()
        {
            DriveIdleAnimations();
        }

        #endregion

        #region Event Handlers

        private void OnButtonCreated(UICell cell)
        {
            for (int i = _idleCells.Count - 1; i >= 0; i--)
            {
                if (_idleCells[i].Cell == cell)
                {
                    _idleCells[i].IdleAnimating = false;
                    _idleCells[i].AnimationCts?.Cancel();
                    _idleCells[i].AnimationCts?.Dispose();
                    _idleCells.RemoveAt(i);
                }
            }

            var state = new CellIdleState
            {
                Cell = cell,
                Transform = cell.transform,
                AnimationCts = new CancellationTokenSource()
            };

            _idleCells.Add(state);

            if (_enableSoundEffects)
            {
                PlaySpawnSound(cell);
            }

            if (_useSpawnEffect)
            {
                PlaySpawnAnimation(state).Forget();
            }
            else
            {
                ConfigureAndStartIdle(state);
            }
        }

        private void OnCellCreated(UICell cell)
        {
            var state = new CellIdleState
            {
                Cell = cell,
                Transform = cell.transform,
                AnimationCts = new CancellationTokenSource()
            };

            _idleCells.Add(state);

            cell.Clicked += () => OnCellClicked(state);
            cell.SliderMoved += OnSliderMoved;

            if (_enableSoundEffects)
            {
                PlaySpawnSound(cell);
            }

            if (_useSpawnEffect)
            {
                PlaySpawnAnimation(state).Forget();
            }
            else
            {
                ConfigureAndStartIdle(state);
            }
        }

        private void OnCellClicked(CellIdleState state)
        {
            if (state.Cell == null)
            {
                return;
            }

            if (_enableSoundEffects)
            {
                PlayClickSound(state.Cell);
            }

            PlayClickAnimation(state).Forget();
        }

        private void OnSliderMoved(float value)
        {
            int cents = (int)Math.Round(value * 100f);
            if (cents % 4 == 0 && _enableSoundEffects)
            {
                AudioManager.Play(AudioManager.Clip.SliderClick);
            }
        }

        #endregion

        #region Spawn Animation

        private async UniTask PlaySpawnAnimation(CellIdleState state)
        {
            var token = state.AnimationCts.Token;

            if (state.Cell.CellCategory == UICell.CellType.Button)
            {
                await PlayButtonSpawnAnimation(state, token);
            }
            else
            {
                await PlayCellSpawnAnimation(state, token);
            }

            if (!token.IsCancellationRequested && state.Cell != null)
            {
                ConfigureAndStartIdle(state);
            }
        }

        private async UniTask PlayButtonSpawnAnimation(CellIdleState state, CancellationToken token)
        {
            state.Transform.localScale = Vector3.zero;

            await UIAnimationUtils.ScaleTo(
                state.Transform,
                Vector3.one * 1.4f,
                Random.Range(0.2f, 0.3f),
                token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            await UIAnimationUtils.ScaleTo(
                state.Transform,
                Vector3.one,
                Random.Range(0.2f, 0.5f),
                token);
        }

        private async UniTask PlayCellSpawnAnimation(CellIdleState state, CancellationToken token)
        {
            // Apply a random initial rotation for asset/none/feature/makeup cells
            if (state.Cell.CellCategory == UICell.CellType.Asset)
            {
                state.Transform.rotation = Quaternion.Euler(
                    Random.Range(-10f, 10f),
                    Random.Range(-10f, 10f),
                    Random.Range(-10f, 10f));
            }

            state.Transform.localScale = Vector3.zero;

            await UIAnimationUtils.ScaleTo(
                state.Transform,
                Vector3.one * 1.4f,
                0.05f,
                token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            await UIAnimationUtils.ScaleTo(
                state.Transform,
                Vector3.one,
                0.1f,
                token);
        }

        #endregion

        #region Click Animation

        private async UniTask PlayClickAnimation(CellIdleState state)
        {
            if (state.Cell == null)
            {
                return;
            }

            // Stop idle animation and reset the one-shot token
            state.IdleAnimating = false;
            if (state.AnimationCts != null)
            {
                state.AnimationCts.Cancel();
                state.AnimationCts.Dispose();
            }

            state.AnimationCts = new CancellationTokenSource();

            var token = state.AnimationCts.Token;

            if (token.IsCancellationRequested)
            {
                return;
            }

            await UIAnimationUtils.ScaleTo(state.Transform, Vector3.one * 0.8f, 0.06f, token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            await UIAnimationUtils.ScaleTo(state.Transform, Vector3.one, 0.06f, token);

            state.RotationBase = state.Transform.localEulerAngles.z;

            if (!token.IsCancellationRequested && state.Cell != null)
            {
                ConfigureAndStartIdle(state);
            }
        }

        #endregion

        #region Idle Animation

        private void ConfigureAndStartIdle(CellIdleState state)
        {
            // Reset params
            state.RotationAmount = 0f;
            state.ScaleDelta = 0f;

            switch (state.Cell.CellCategory)
            {
                case UICell.CellType.Asset:
                    if (_enableIdleRotation)
                    {
                        ConfigureIdleRotation(state, 2f, 4f, 15f);
                    }

                    if (_enableIdleScale)
                    {
                        ConfigureIdleScale(state, 2f, 5f, 0.05f);
                    }

                    break;
                case UICell.CellType.Button:
                    if (_enableButtonIdleRotation)
                    {
                        ConfigureIdleRotation(state, 2f, 4f, 5f);
                    }

                    if (_enableButtonIdleScale)
                    {
                        ConfigureIdleScale(state, 3f, 5f, 0.1f);
                    }

                    break;
                case UICell.CellType.Color:
                    if (_enableIdleScale)
                    {
                        ConfigureIdleScale(state, 1.5f, 3f, 0.15f);
                    }

                    break;
                case UICell.CellType.Slider:
                    if (_enableIdleScale)
                    {
                        ConfigureIdleScale(state, 3f, 5f, 0.05f);
                    }

                    break;
            }

            state.RotationTimeOffset = -Time.time;
            state.ScaleTimeOffset = -Time.time;
            state.LastAppliedRotation = state.RotationBase;
            state.LastAppliedScale = 1f;
            state.IdleAnimating = true;
        }

        private static void ConfigureIdleRotation(CellIdleState state, float timeMin, float timeMax, float amount)
        {
            float halfCycleTime = Mathf.Max(0.01f, Random.Range(timeMin, timeMax));
            state.RotationFrequency = Mathf.PI / halfCycleTime;
            state.RotationAmount = amount;
        }

        private static void ConfigureIdleScale(CellIdleState state, float timeMin, float timeMax, float delta)
        {
            float halfCycleTime = Mathf.Max(0.01f, Random.Range(timeMin, timeMax));
            state.ScaleFrequency = Mathf.PI / halfCycleTime;
            state.ScaleDelta = delta;
        }

        private void DriveIdleAnimations()
        {
            for (int i = _idleCells.Count - 1; i >= 0; i--)
            {
                var state = _idleCells[i];

                // Remove destroyed cells
                if (state.Cell == null)
                {
                    state.AnimationCts?.Cancel();
                    state.AnimationCts?.Dispose();
                    _idleCells.RemoveAt(i);
                    continue;
                }

                if (!state.IdleAnimating)
                {
                    continue;
                }

                if (_enableIdleRotation && state.RotationAmount > 0f)
                {
                    float angle = state.RotationBase
                                  + Mathf.Sin((Time.time + state.RotationTimeOffset) * state.RotationFrequency)
                                  * state.RotationAmount;

                    if (Mathf.Abs(angle - state.LastAppliedRotation) > RotationUpdateThreshold)
                    {
                        state.Transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                        state.LastAppliedRotation = angle;
                    }
                }

                if (_enableIdleScale && state.ScaleDelta > 0f)
                {
                    float scaleValue = 1f + Mathf.Sin((Time.time + state.ScaleTimeOffset) * state.ScaleFrequency)
                                            * state.ScaleDelta;

                    if (Mathf.Abs(scaleValue - state.LastAppliedScale) > ScaleUpdateThreshold)
                    {
                        state.Transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
                        state.LastAppliedScale = scaleValue;
                    }
                }
            }
        }

        #endregion

        #region Audio

        private static void PlaySpawnSound(UICell cell)
        {
            var audioSource = cell.GetComponent<AudioSource>();

            switch (cell.CellCategory)
            {
                case UICell.CellType.Asset:
                case UICell.CellType.Slider:
                    AudioManager.Play(AudioManager.Clip.AsstCellSpawn, audioSource);
                    break;
                case UICell.CellType.Color:
                    AudioManager.Play(AudioManager.Clip.ColorCellSpawn, audioSource);
                    break;
            }
        }

        private static void PlayClickSound(UICell cell)
        {
            switch (cell.CellCategory)
            {
                case UICell.CellType.Asset:
                    AudioManager.Play(AudioManager.Clip.ClickAssetCell);
                    break;
                case UICell.CellType.Color:
                    AudioManager.Play(AudioManager.Clip.ClickColorCell);
                    break;
            }
        }

        #endregion

        #region Per-Cell State

        private class CellIdleState
        {
            public UICell Cell;
            public Transform Transform;
            public CancellationTokenSource AnimationCts;

            // Idle animation state
            public bool IdleAnimating;

            // Rotation
            public float RotationBase;
            public float RotationAmount;
            public float RotationFrequency;
            public float RotationTimeOffset;
            public float LastAppliedRotation;

            // Scale
            public float ScaleDelta;
            public float ScaleFrequency;
            public float ScaleTimeOffset;
            public float LastAppliedScale = 1f;
        }

        #endregion
    }
}


