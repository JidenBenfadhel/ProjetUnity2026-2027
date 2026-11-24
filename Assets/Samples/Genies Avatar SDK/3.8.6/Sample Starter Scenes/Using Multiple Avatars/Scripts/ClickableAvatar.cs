using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

namespace Genies.Sdk.Samples.MultipleAvatars
{
    /// <summary>
    /// Component that makes an avatar clickable to view them up close.
    /// Should be added to spawned avatar GameObjects.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ClickableAvatar : MonoBehaviour
    {
        private ManagedAvatarComponent _managedAvatarComponent;
        private static bool _movingCamera;

        private void Awake()
        {
            // Auto-find ManagedAvatarComponent if not assigned
            if (_managedAvatarComponent == null)
            {
                _managedAvatarComponent = GetComponent<ManagedAvatarComponent>();
            }
        }

        private void Update()
        {
            HandlePointerInput();
        }

        #region Input Handling

        private void HandlePointerInput()
        {
            // Skip if no pointer input device (e.g., Mouse or Touchscreen)
            if (Pointer.current == null)
            {
                return;
            }

            // Only process on pointer press this frame
            if (!Pointer.current.press.wasPressedThisFrame)
            {
                return;
            }

            // Skip clicks over UI
            if (IsPointerOverUI())
            {
                return;
            }

            Vector2 pointerPos = Pointer.current.position.ReadValue();
            ProcessPointer(pointerPos);
        }

        private void ProcessPointer(Vector2 screenPosition)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            Ray ray = cam.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    // Use Forget() with error handling wrapper
                    OnClick(hit.transform);
                }
            }
        }

        /// <summary>
        /// Checks if pointer is over any UI element.
        /// </summary>
        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            // Use device-specific ID for consistent simulator behavior
            int deviceId = Pointer.current.device.deviceId;
            return EventSystem.current.IsPointerOverGameObject(deviceId);
        }

        #endregion

        private void OnClick(Transform hitObject)
        {
            CameraMover.MoveCamera(hitObject).Forget();
        }
    }
}
