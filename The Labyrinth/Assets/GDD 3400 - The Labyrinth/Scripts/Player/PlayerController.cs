using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace  GDD3400.Labyrinth
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Player Settings")]
        [SerializeField] private float moveSpeed = 10;
        [SerializeField] private float dashDistance = 2.5f;
        [SerializeField] private float dashCooldown = 1.5f;

        [Header("Camera Settings")]
        [SerializeField] private Camera camera;
        [SerializeField] private float fov;
        [SerializeField] private float sensitivity;
        private float yaw;
        private float pitch;

        private Rigidbody rigidbody;
        private InputAction moveAction;
        private InputAction dashAction;
        private Vector3 inputVector;
        private Vector3 moveVector;

        private bool performDash;
        private bool isDashing;

        private void Awake()
        {
            // Assign member variables
            rigidbody = GetComponent<Rigidbody>();

            moveAction = InputSystem.actions.FindAction("Move");
            dashAction = InputSystem.actions.FindAction("Dash");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            yaw += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime * 100;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime * 100;
            pitch = Mathf.Clamp(pitch, -90, 90);

            transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            camera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            // Store current input move vector, remap from X/Y to X/Z
            inputVector.x = moveAction.ReadValue<Vector2>().x;
            inputVector.z = moveAction.ReadValue<Vector2>().y;

            moveVector = (transform.forward * inputVector.z) + (transform.right * inputVector.x);
            moveVector.y = 0f;
            moveVector.Normalize();


            // If the dash is available and pressed this frame, perform the dash
            if (!isDashing && dashAction.WasPressedThisFrame()) PerformDash();
        }

        private void FixedUpdate()
        {
            // Apply the movement force
            rigidbody.AddForce(moveVector * moveSpeed * 4f, ForceMode.Force);

            // Add the dash force
            if (performDash)
            {
                rigidbody.AddForce(transform.forward * dashDistance * 5f, ForceMode.Impulse);
                performDash = false;
            }

        }

        private void PerformDash()
        {
            performDash = true;
            isDashing = true;

            // Call reset after the cooldown
            Invoke("ResetDash", dashCooldown);
        }

        private void ResetDash()
        {
            isDashing = false;
            rigidbody.linearVelocity = moveVector * moveSpeed;
        }
    }
}
