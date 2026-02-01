using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.AI;

public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Look")]
    public float mouseSensitivity = 100f;
    public Transform cameraTransform;

    [Header("Mouse Smoothing")]
    public float smoothTime = 0.05f;

    private Vector2 currentLook;
    private Vector2 lookVelocity;
    
    [Header("Sprint")]
    public float sprintSpeedMultiplier = 1.6f;
    public float sprintFOV = 75f;
    public float fovSmoothSpeed = 8f;

    [Header("Head Bob")]
    public float bobFrequency = 1.8f;
    public float bobAmplitude = 0.05f;
    public float sprintBobMultiplier = 1.4f;
    
    [Header("Crouch")]
    public float crouchHeight = 1.0f;
    public float crouchCameraOffset = -0.5f;
    public float crouchTransitionSpeed = 8f;

    private bool isCrouching;
    private float standingHeight;
    private Vector3 standingCenter;

    private bool isSprinting;
    private float bobTimer;
    private float defaultFOV;
    private Vector3 cameraStartLocalPos;
    private Camera cam;

    private CharacterController controller;
    private InputSystem_Actions input;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalVelocity;
    private float xRotation = 0f;
    public bool isInView = false;

    [SerializeField]
    private Canvas gameEnd;
    [SerializeField]
    private TextMeshProUGUI gameOverText;
    [SerializeField]
    private TextMeshProUGUI winText;
    [SerializeField]
    private GameObject hud;

    public static event Action OnPlayerDeath;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = new InputSystem_Actions();
    }

    void OnEnable()
    {
        input.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam = cameraTransform.GetComponent<Camera>();
        defaultFOV = cam.fieldOfView;
        cameraStartLocalPos = cameraTransform.localPosition;
        standingHeight = controller.height;
        standingCenter = controller.center;

    }

    void OnDisable()
    {
        input.Disable();
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        HandleCrouch();
        HandleHeadBob();
        HandleFOV();
    }

    void HandleMovement()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        float speed = isSprinting ? moveSpeed * sprintSpeedMultiplier : moveSpeed;
        Vector3 velocity = speed * move + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        currentLook = Vector2.SmoothDamp(
            currentLook,
            lookInput,
            ref lookVelocity,
            smoothTime
        );

        float mouseX = currentLook.x * mouseSensitivity * Time.deltaTime;
        float mouseY = currentLook.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
    void HandleCrouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        Vector3 targetCenter = isCrouching
            ? new Vector3(0, crouchHeight / 2f, 0)
            : standingCenter;

        controller.height = Mathf.Lerp(
            controller.height,
            targetHeight,
            Time.deltaTime * crouchTransitionSpeed
        );

        controller.center = Vector3.Lerp(
            controller.center,
            targetCenter,
            Time.deltaTime * crouchTransitionSpeed
        );

        Vector3 targetCameraPos = cameraStartLocalPos +
            (isCrouching ? Vector3.up * crouchCameraOffset : Vector3.zero);

        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            targetCameraPos,
            Time.deltaTime * crouchTransitionSpeed
        );
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed || !controller.isGrounded) return;

        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (isCrouching)
        {
            isSprinting = false;
            return;
        }
        isSprinting = context.ReadValueAsButton();
    }
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (isCrouching)
            TryStandUp();
        else
            StartCrouch();
    }

    void HandleHeadBob()
    {
        if (!controller.isGrounded || moveInput.magnitude < 0.1f)
        {

            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                cameraStartLocalPos,
                Time.deltaTime * 8f
            );
            bobTimer = 0f;
            return;
        }

        float frequency = bobFrequency;
        float amplitude = bobAmplitude;

        if (isSprinting)
        {
            frequency *= sprintBobMultiplier;
            amplitude *= sprintBobMultiplier;
        }

        bobTimer += Time.deltaTime * frequency;

        float bobOffsetY = Mathf.Sin(bobTimer) * amplitude;
        float bobOffsetX = Mathf.Cos(bobTimer * 2) * (amplitude * 0.5f);
        cameraTransform.localPosition = cameraStartLocalPos + Vector3.up * bobOffsetY;
    }
    void HandleFOV()
    {
        float targetFOV = isSprinting ? sprintFOV : defaultFOV;
        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            targetFOV,
            Time.deltaTime * fovSmoothSpeed);
    }
    void StartCrouch()
    {
        isCrouching = true;
        isSprinting = false;
    }

    void TryStandUp()
    {
        float checkDistance = standingHeight - crouchHeight;
        Vector3 origin = transform.position + Vector3.up * crouchHeight;

        if (Physics.SphereCast(origin, controller.radius, Vector3.up, out _, checkDistance))
            return; 

        isCrouching = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Enemy"))
        {
            playerDeath();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            isInView = true;
        }
        if (other.gameObject.CompareTag("Win") && ItemPickup.allMasksCollected)
        {
            playerWin();
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            isInView = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            isInView = false;
        }
    }
    void playerDeath()
    {
        hud.SetActive(false);
        // Implement player death logic here
        Debug.Log("Player has died.");
        TryGetComponent<NavMeshAgent>(out NavMeshAgent navMeshAgent);
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }
        if(TryGetComponent<Footsteps>(out Footsteps footstep))
        {
            footstep.enabled = false;
        }
        if (TryGetComponent<AudioSource>(out AudioSource audioSource))
        {
            audioSource.enabled = false;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        OnPlayerDeath?.Invoke();
        gameEnd.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(true);
        this.enabled = false;
    }
    void playerWin()
    {
        hud.SetActive(false);
        // Implement player death logic here
        Debug.Log("Player has Won.");
        TryGetComponent<NavMeshAgent>(out NavMeshAgent navMeshAgent);
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }
        if (TryGetComponent<Footsteps>(out Footsteps footstep))
        {
            footstep.enabled = false;
        }
        if(TryGetComponent<AudioSource>(out AudioSource audioSource))
        {
            audioSource.enabled = false;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        OnPlayerDeath?.Invoke();
        gameEnd.gameObject.SetActive(true);
        winText.gameObject.SetActive(true);
        this.enabled = false;
    }


}

