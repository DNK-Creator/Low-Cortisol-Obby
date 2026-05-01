using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;

/// <summary>
/// Player fly state: strafing movement with camera-driven forward/back (including vertical component).
/// Movement uses Rigidbody.velocity (no inertia) so collisions are handled by physics.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerFlyState : MonoBehaviour
{
    [Header("Flight Settings")]
    [Tooltip("Is the player currently in flying mode.")]
    [SerializeField] private bool isFlying = false;

    [Tooltip("Overall flight speed (units per second). Input magnitude scales this speed.")]
    [SerializeField] private float flySpeed = 10f;

    [Tooltip("If true, the player object will smoothly rotate to face camera forward while flying.")]
    [SerializeField] private bool alignRotationWithCamera = true;

    [Tooltip("Rotation smoothing speed used when alignRotationWithCamera is enabled.")]
    [SerializeField] private float rotationSpeed = 15f;

    [Tooltip("Optional on-screen joystick for mobile input. If null, keyboard/PC input is used.")]
    [SerializeField] private Joystick joystick;

    // === Dependencies / components ===
    private vThirdPersonController _controller;
    private vThirdPersonInput _input;
    private Rigidbody _rigidbody;
    private Camera _camera;

    // === Saved physics state ===
    private RigidbodyConstraints _originalConstraints;
    private readonly RigidbodyConstraints _flyingConstraints = RigidbodyConstraints.FreezeRotation;

    private void Start()
    {
        InitializeComponents();
        CacheOriginalConstraints();
    }

    private void Update()
    {
        /*// Toggle flight mode on F key
        if (Input.GetKeyDown(KeyCode.F))
            ToggleFlyMode(true);*/


    }

    private void FixedUpdate()
    {
        if (!isFlying)
            return;

        Vector2 input = ReadInput();
        Vector3 velocity = ComputeFlightVelocity(input.x, input.y);
        ApplyVelocity(velocity);

        
    }

    private void LateUpdate()
    {
        if (isFlying && alignRotationWithCamera)
        {
            AlignRotationWithCamera();
        }
    }

    #region Initialization & state
    private void InitializeComponents()
    {
        _controller = GetComponent<vThirdPersonController>();
        _input = GetComponent<vThirdPersonInput>();
        _rigidbody = GetComponent<Rigidbody>();
        _camera = Camera.main;
        if (_camera == null)
            Debug.LogWarning("PlayerFlyState: Main Camera not found.");
    }

    private void CacheOriginalConstraints()
    {
        _originalConstraints = _rigidbody.constraints;
    }

    [ContextMenu("Toggle Fly Mode")]
    public void ToggleFlyMode(bool setAnimation)
    {
        isFlying = !isFlying;

        // Disable the standard controller while flying
        if (_controller != null)
        {
            _controller.enabled = !isFlying;
            if(setAnimation)
                _controller.animator.SetBool("isFlying", isFlying);
        }

        if (_input != null)
            _input.LockInputExceptCamera(isFlying);

        _rigidbody.useGravity = !isFlying;

        if (isFlying)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.constraints = _flyingConstraints;
        }
        else
        {
            _rigidbody.constraints = _originalConstraints;
        }
    }

    public void ActivateFlightMode(bool setAnimation)
    {
        isFlying = true;

        transform.position += Vector3.up;
        // Disable the standard controller while flying
        if (_controller != null)
        {
            _controller.enabled = !isFlying;
            if (setAnimation)
                _controller.animator.SetBool("isFlying", isFlying);
        }

        if (_input != null)
            _input.LockInputExceptCamera(isFlying);

        _rigidbody.useGravity = !isFlying;

        if (isFlying)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.constraints = _flyingConstraints;
        }
        else
        {
            _rigidbody.constraints = _originalConstraints;
        }
    }
    public void DeactivateFlightMode(bool setAnimation)
    { 
        isFlying = false;
        // Disable the standard controller while flying
        if (_controller != null)
        {
            _controller.enabled = !isFlying;
            if (setAnimation)
                _controller.animator.SetBool("isFlying", isFlying);
        }

        if (_input != null)
            _input.LockInputExceptCamera(isFlying);

        _rigidbody.useGravity = !isFlying;

        if (isFlying)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.constraints = _flyingConstraints;
        }
        else
        {
            _rigidbody.constraints = _originalConstraints;
        }
    }
    #endregion

    #region Input
    /// <summary>
    /// Read horizontal (x) and vertical (y) inputs.
    /// Horizontal = strafe right/left, Vertical = forward/back.
    /// </summary>
    private Vector2 ReadInput()
    {
        float h, v;
        if (_input != null && _input.MobileControl && joystick != null)
        {
            // Mobile: joystick provides both axes
            h = joystick.Horizontal;
            v = joystick.Vertical;
        }
        else
        {
            // Desktop: keyboard/controller
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");
        }

        // Clamp to ensure magnitude <= 1
        Vector2 raw = new Vector2(h, v);
        if (raw.magnitude > 1f)
            raw = raw.normalized;

        return raw;
    }
    #endregion

    #region Movement calculation & application
    /// <summary>
    /// Compute desired velocity vector based on camera orientation and inputs.
    /// - Forward/back uses camera.forward (including Y) so pitch affects ascent/descent.
    /// - Strafe uses camera.right projected to horizontal plane (no vertical component).
    /// - No inertia: final velocity is directly set.
    /// </summary>
    private Vector3 ComputeFlightVelocity(float horizontalInput, float verticalInput)
    {
        if (_camera == null)
            return Vector3.zero;

        // Camera forward (this includes vertical component -> controls ascent/descent when moving forward/back)
        Vector3 camForward = _camera.transform.forward;
        // Camera right � we'll remove vertical component to keep strafing purely lateral
        Vector3 camRight = _camera.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        // Compose movement: forward/back follows full camera forward (with y), strafing uses horizontal-only right
        Vector3 desired = camForward * verticalInput + camRight * horizontalInput;

        // If both inputs zero -> no movement
        if (desired == Vector3.zero)
            return Vector3.zero;

        // If input magnitude > 1 (e.g., diagonal), normalize to preserve direction but keep consistent speed.
        // However we want input magnitude to scale speed naturally (joystick intensity). Use magnitude of input vector:
        float inputMagnitude = new Vector2(horizontalInput, verticalInput).magnitude;

        // Normalize direction (keep sign and direction from composed vector)
        Vector3 direction = desired.normalized;

        // Final velocity: direction * flySpeed * inputMagnitude
        return direction * flySpeed * inputMagnitude;
    }

    /// <summary>
    /// Apply computed velocity to Rigidbody. Use direct assignment to avoid inertia.
    /// </summary>
    private void ApplyVelocity(Vector3 velocity)
    {
        // Set velocity directly so movement is immediate and collisions still work.
        _rigidbody.linearVelocity = velocity;
    }
    #endregion

    #region Optional rotation alignment
    /// <summary>
    /// Smoothly align player rotation with camera forward (keeps only horizontal rotation to avoid pitching the body).
    /// This is optional and controlled by alignRotationWithCamera.
    /// </summary>
    private void AlignRotationWithCamera()
    {
        if (_camera == null)
            return;

        Vector3 camForward = _camera.transform.forward;
        camForward.y = 0f; // keep only yaw for character rotation
        if (camForward.sqrMagnitude < 0.0001f)
            return;

        Quaternion target = Quaternion.LookRotation(camForward);
        Quaternion smoothed = Quaternion.Slerp(_rigidbody.rotation, target, rotationSpeed * Time.fixedDeltaTime);
        _rigidbody.MoveRotation(smoothed);
    }
    #endregion
}
