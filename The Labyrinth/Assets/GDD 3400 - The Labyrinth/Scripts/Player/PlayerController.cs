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
        private InputAction moveAction;
        private InputAction dashAction;
        private Vector3 inputVector;
        private Vector3 moveVector;
        private bool performDash;
        private bool isDashing;
        private Rigidbody rigidbody;

        [Header("Camera Settings")]
        [SerializeField] private Camera camera;
        [SerializeField] private float fov;
        [SerializeField] private float sensitivity;
        private float yaw;
        private float pitch;

        [Header("Pheromon Settings")]
        [SerializeField] private GameObject pheromonePrefab;
        [SerializeField] private Color playerColor;
        [SerializeField] private float playerLifeTime;
        [SerializeField] private float spawnCooldown;
        [SerializeField] private Vector3 spawnOffset;
        private string playerTag = "PlayerP";
        private Timer spawningTimer;
        private Timer hidingPheromoneTimer;
        private Timer hidingPheromoneCooldownTimer;
        [SerializeField] private float hidingPheromoneCooldown;
        private bool isHidingPheromone = false;
        private bool canHidePheromone = true;
        [SerializeField] private float pheromoneHidingCooldown;
        [SerializeField] private float hp;
        private bool invincible;
        private Timer invincibleTimer;
        [SerializeField] private float invincibleTime;

        private void Awake()
        {
            invincibleTimer = gameObject.AddComponent<Timer>();
            // Assign member variables
            rigidbody = GetComponent<Rigidbody>();
            spawningTimer = gameObject.AddComponent<Timer>();
            hidingPheromoneTimer = gameObject.AddComponent<Timer>();
            hidingPheromoneCooldownTimer = gameObject.AddComponent<Timer>();
            moveAction = InputSystem.actions.FindAction("Move");
            dashAction = InputSystem.actions.FindAction("Dash");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!invincibleTimer.IsRunning()) invincible = false;

            if (hp <= 0) Debug.Log("Your Dead");

            // Drop excited pheromone
            if (!spawningTimer.IsRunning() && rigidbody.linearVelocity != Vector3.zero && !isHidingPheromone)
            {
                spawningTimer.Run(spawnCooldown);
                SpawnPheromone(playerColor, playerLifeTime, playerTag);
            }

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

            if(!hidingPheromoneTimer.IsRunning()) isHidingPheromone = false;

            if(!hidingPheromoneCooldownTimer.IsRunning())
            {
                canHidePheromone = true;
            }

            if (Input.GetKeyDown(KeyCode.E) && canHidePheromone)
            {
                canHidePheromone = false;
                hidingPheromoneCooldownTimer.Run(hidingPheromoneCooldown);
                PheromoneHide();
            }

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

        private void SpawnPheromone(Color color, float lifeTime, string tag)
        {
            GameObject pheromone = Instantiate(pheromonePrefab, transform.position + spawnOffset, Quaternion.identity);
            pheromone.GetComponent<Pheomone>().Initialize(color, lifeTime, tag);
        }

        private void PheromoneHide()
        {
            isHidingPheromone = true;
            hidingPheromoneTimer.Run(pheromoneHidingCooldown);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.tag == "Enemy" && !invincible)
            {
                hp--;
                Debug.Log("HP: " + hp);
                invincible = true;
                invincibleTimer.Run(invincibleTime);
            }
        }
    }
}
