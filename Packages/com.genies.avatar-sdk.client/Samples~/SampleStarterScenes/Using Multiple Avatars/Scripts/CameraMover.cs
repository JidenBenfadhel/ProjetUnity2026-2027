using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Genies.Sdk.Samples.MultipleAvatars
{
    public static class CameraMover
    {
        private static bool _movingCamera;

        /// <summary>
        /// Moves the Camera to look at a transform in front of it
        /// </summary>
        /// <param name="lookTarget">The transform to position the camera in front of</param>
        /// <param name="moveToExactPosition">If the camera should instead move to the provided transform's position
        /// instead of being positioned in front of it</param>
        public static async UniTask MoveCamera(Transform lookTarget, bool moveToExactPosition = false)
        {
            if (_movingCamera)
            {
                return;
            }

            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            _movingCamera = true;
            Vector3 startPos = cam.transform.position;
            Vector3 endPos = lookTarget.position - (cam.transform.forward * 1.7f) + Vector3.up;

            if (moveToExactPosition)
            {
                endPos = lookTarget.position;
            }

            float t = 0f;

            while (t < 1f)
            {
                cam.transform.position = Vector3.Lerp(startPos, endPos, t);

                t += Time.deltaTime;
                await UniTask.Yield();
            }

            cam.transform.position = endPos;
            _movingCamera = false;
        }
    }
}
