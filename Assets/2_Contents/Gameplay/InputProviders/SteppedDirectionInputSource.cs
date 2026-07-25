using System;
using BobaKami.Interfaces;
using Soar.Variables;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BobaKami.Gameplay
{
    /// <summary>
    /// Arcade-style stepped direction source for key/button input: each Left/Right press
    /// steps the direction one lane (e.g. Right -> Forward -> Left), clamped at the sides.
    /// It writes the equivalent absolute vector into faceVector so the direction converter
    /// can stay in absolute mode for pointer input. Steps are edge-triggered: a press (or a
    /// stick tilt past the threshold) steps once and re-arms when the control returns to
    /// center, so holding a key does not keep stepping.
    /// </summary>
    public class SteppedDirectionInputSource : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference moveActionReference;
        [SerializeField] private Variable<DirectionEnum> currentDirection;

        [Header("Output")]
        [SerializeField] private Variable<Vector2> faceVector;

        private const float ActuationThreshold = 0.5f;

        private DirectionEnum direction;
        private bool armed = true;
        private IDisposable subscription;

        private void OnEnable()
        {
            direction = currentDirection.Value;
            subscription = currentDirection.Subscribe(latest => direction = latest);

            moveActionReference.action.Enable();
            moveActionReference.action.performed += OnMovePerformed;
            moveActionReference.action.canceled += OnMoveCanceled;
        }

        private void OnDisable()
        {
            moveActionReference.action.performed -= OnMovePerformed;
            moveActionReference.action.canceled -= OnMoveCanceled;
            moveActionReference.action.Disable();

            subscription?.Dispose();
            subscription = null;
            armed = true;
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            var x = context.ReadValue<Vector2>().x;
            if (Mathf.Abs(x) < ActuationThreshold)
            {
                armed = true;
                return;
            }

            if (!armed) return;
            armed = false;

            var stepped = Mathf.Clamp((int)direction + Math.Sign(x), -1, 1);
            direction = (DirectionEnum)stepped;
            faceVector.Value = new Vector2(stepped, 0f);
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            armed = true;
        }
    }
}
