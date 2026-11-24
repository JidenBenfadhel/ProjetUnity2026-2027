using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Genies.Sdk.Avatar.Samples.CustomAvatarEditor
{
    /// <summary>
    /// Moves the camera between target transforms by interpolating in spherical coordinates
    /// around <see cref="_orbitPivot"/>. This produces natural arcing paths that stay at a
    /// consistent distance from the pivot, avoiding clipping through the avatar.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Camera _cam;

        [Tooltip("The point the camera orbits around (e.g. the avatar's chest). " +
                 "All camera movement arcs through spherical coordinates relative to this transform.")]
        [SerializeField] private Transform _orbitPivot;

        private CancellationTokenSource _moveCts;
        private Vector3 _originalCameraPosition;
        private Quaternion _originalCameraRotation;

        private void Awake()
        {
            if (CheckCamera() is false)
            {
                return;
            }

            _originalCameraPosition = _cam.transform.position;
            _originalCameraRotation = _cam.transform.rotation;

        }

        private bool CheckCamera()
        {
            if (_cam == null)
            {
                _cam = Camera.main;
            }

            if (_cam == null)
            {
                Debug.LogError("A camera needs to be assigned to the CameraController");
                return false;
            }

            return true;
        }

        public async UniTask MoveCameraAroundPivot(Transform target, float duration = 1f)
        {
            if (CheckCamera() is false)
            {
                return;
            }

            _moveCts?.Cancel();
            _moveCts?.Dispose();
            _moveCts = new CancellationTokenSource();
            var token = _moveCts.Token;

            var camTransform = _cam.transform;

            if (duration <= 0f || _orbitPivot == null)
            {
                camTransform.SetPositionAndRotation(target.position, target.rotation);
                return;
            }

            var pivot = _orbitPivot.position;
            var startPos = camTransform.position;
            var startRot = camTransform.rotation;
            var endPos = target.position;
            var endRot = target.rotation;

            ToSpherical(startPos - pivot, out var startRadius, out var startAzimuth, out var startElevation);
            ToSpherical(endPos - pivot, out var endRadius, out var endAzimuth, out var endElevation);

            float azimuthDelta = Mathf.DeltaAngle(startAzimuth * Mathf.Rad2Deg, endAzimuth * Mathf.Rad2Deg) * Mathf.Deg2Rad;
            float targetAzimuth = startAzimuth + azimuthDelta;

            var startForward = startRot * Vector3.forward;
            var endForward = endRot * Vector3.forward;

            float elapsed = 0f;
            while (elapsed < duration && !token.IsCancellationRequested)
            {
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

                float r = Mathf.Lerp(startRadius, endRadius, t);
                float az = Mathf.Lerp(startAzimuth, targetAzimuth, t);
                float el = Mathf.Lerp(startElevation, endElevation, t);

                var pos = pivot + FromSpherical(r, az, el);
                camTransform.position = pos;

                var forward = Vector3.Slerp(startForward, endForward, t).normalized;
                var up = Vector3.Slerp(startRot * Vector3.up, endRot * Vector3.up, t).normalized;
                camTransform.rotation = Quaternion.LookRotation(forward, up);

                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            if (!token.IsCancellationRequested)
            {
                camTransform.SetPositionAndRotation(target.position, target.rotation);
            }
        }

        private static void ToSpherical(Vector3 offset, out float radius, out float azimuth, out float elevation)
        {
            radius = offset.magnitude;
            azimuth = Mathf.Atan2(offset.x, offset.z);
            elevation = radius > 0f ? Mathf.Asin(Mathf.Clamp(offset.y / radius, -1f, 1f)) : 0f;
        }

        private static Vector3 FromSpherical(float radius, float azimuth, float elevation)
        {
            float cosElev = Mathf.Cos(elevation);
            return new Vector3(
                radius * cosElev * Mathf.Sin(azimuth),
                radius * Mathf.Sin(elevation),
                radius * cosElev * Mathf.Cos(azimuth)
            );
        }

        public async UniTask MoveCameraSimple(Vector3 targetPos, Quaternion targetRot, float duration = 1f)
        {
            if (CheckCamera() is false)
            {
                return;
            }

            _moveCts?.Cancel();
            _moveCts?.Dispose();
            _moveCts = new CancellationTokenSource();
            var token = _moveCts.Token;

            var camTransform = _cam.transform;

            if (duration <= 0f)
            {
                camTransform.SetPositionAndRotation(targetPos, targetRot);
                return;
            }

            var startPos = camTransform.position;
            var startRot = camTransform.rotation;

            float elapsed = 0f;
            while (elapsed < duration && !token.IsCancellationRequested)
            {
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

                camTransform.position = Vector3.Lerp(startPos, targetPos, t);
                camTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);

                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            if (!token.IsCancellationRequested)
            {
                camTransform.SetPositionAndRotation(targetPos, targetRot);
            }
        }

        public async UniTask MoveCameraToOriginalTransform(float duration = 1f)
        {
            await MoveCameraSimple(_originalCameraPosition, _originalCameraRotation, duration);
        }
    }
}
