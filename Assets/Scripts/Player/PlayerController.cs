using UnityEngine;
using UnityEngine.InputSystem;
using HW2.Core;
using HW2.Difficulty;

namespace HW2.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Lane Settings")]
        [SerializeField] private float laneDistance = 3.5f; // Distance between lanes
        [SerializeField] private float laneChangeSpeed = 22f;
        [SerializeField] private float tiltAngle = 14f;

        [Header("Components")]
        [SerializeField] private PlayerHealth health;
        [SerializeField] private FuelSystem fuel;
        [SerializeField] private Transform visualsContainer;

        private CharacterController characterController;
        private InputSystem_Actions inputActions;

        private int currentLane = 0; // -1 = Left, 0 = Center, 1 = Right
        private float targetX = 0f;
        private Vector3 lastPosition;
        private bool hasSwitchedThisPress = false;

        public PlayerHealth Health => health;
        public FuelSystem Fuel => fuel;
        public int CurrentLane => currentLane;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (health == null) health = GetComponent<PlayerHealth>();
            if (fuel == null) fuel = GetComponent<FuelSystem>();

            inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            if (inputActions != null)
            {
                inputActions.Player.Enable();
            }
        }

        private void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.Player.Disable();
            }
        }

        private void Start()
        {
            lastPosition = transform.position;
            targetX = 0f;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                return;
            }

            HandleInput();
            MoveCar();
            HandleVisualTilt();
            TrackDistance();
        }

        private void HandleInput()
        {
            Vector2 moveVector = inputActions.Player.Move.ReadValue<Vector2>();

            // Distinct lane shift per tap/press
            if (moveVector.x < -0.5f)
            {
                if (!hasSwitchedThisPress)
                {
                    ChangeLane(-1);
                    hasSwitchedThisPress = true;
                }
            }
            else if (moveVector.x > 0.5f)
            {
                if (!hasSwitchedThisPress)
                {
                    ChangeLane(1);
                    hasSwitchedThisPress = true;
                }
            }
            else
            {
                hasSwitchedThisPress = false;
            }
        }

        public void ChangeLane(int direction)
        {
            int nextLane = Mathf.Clamp(currentLane + direction, -1, 1);
            if (nextLane != currentLane)
            {
                currentLane = nextLane;
                targetX = currentLane * laneDistance;
            }
        }

        private void MoveCar()
        {
            float forwardSpeed = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentSpeed : 18f;

            // Smooth X movement towards target lane
            float currentX = transform.position.x;
            float newX = Mathf.MoveTowards(currentX, targetX, laneChangeSpeed * Time.deltaTime);

            Vector3 moveDelta = new Vector3(newX - currentX, -2f * Time.deltaTime, forwardSpeed * Time.deltaTime);
            characterController.Move(moveDelta);
        }

        private void HandleVisualTilt()
        {
            if (visualsContainer == null) return;

            float xDiff = targetX - transform.position.x;
            float targetZAngle = -Mathf.Clamp(xDiff, -1f, 1f) * tiltAngle;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetZAngle);
            visualsContainer.localRotation = Quaternion.Slerp(visualsContainer.localRotation, targetRotation, Time.deltaTime * 10f);
        }

        private void TrackDistance()
        {
            float deltaZ = transform.position.z - lastPosition.z;
            if (deltaZ > 0f && ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddDistance(deltaZ);
            }
            lastPosition = transform.position;
        }
    }
}
