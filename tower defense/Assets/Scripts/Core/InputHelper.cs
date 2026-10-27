using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TowerDefense.Core
{
    /// <summary>
    /// Utility class ensuring compatibility across both the Unity New Input System and Legacy Input Manager.
    /// </summary>
    public static class InputHelper
    {
        public static bool GetLeftClickDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.leftButton.wasPressedThisFrame;
            }
#endif
            return Input.GetMouseButtonDown(0);
        }

        public static bool GetRightClickDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.rightButton.wasPressedThisFrame;
            }
#endif
            return Input.GetMouseButtonDown(1);
        }

        public static Vector2 GetMouseScreenPosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }
#endif
            return Input.mousePosition;
        }

        public static Vector3 GetMouseWorldPosition(Camera cam)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return Vector3.zero;

            Vector2 screenPos = GetMouseScreenPosition();
            Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;
            return worldPos;
        }

        public static bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}
