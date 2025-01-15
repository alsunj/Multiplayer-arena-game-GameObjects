using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private PlayerInteractionSettings playerInteractionSettings;
    [SerializeField] private float speed = 2f;
    [SerializeField] private InputReader _inputReader;
    private Rigidbody _rb;
    [SerializeField] private Camera _camera;

    public Vector3 offset = new Vector3(0, 7.4f, -6.4f);

    //camera public Vector3 eulerAngles = new Vector3(40.45f, 0, 0);
    private Vector2 _movementInput;
    public float fov = 60;


    public bool enableSprint = true;
    public bool unlimitedSprint = false;
    public float sprintSpeed = 7f;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;
    public float zoomStepTime = 5f;

    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public bool _isWalking = false;

    public float maxVelocityChange = 10f;


    // Internal Variables
    private bool _isSprinting = false;
    private float _sprintRemaining;
    private bool _isSprintCooldown = false;
    private float _sprintCooldownReset;


    // Internal Variables
    private Vector3 _jointOriginalPos;
    private float _timer = 0;

    private float _walkingSoundTimer = 0;
    private bool _isWalkingSoundCooldown = false;

    private float _sprintingSoundTimer = 0;
    private bool _isSprintingSoundCooldown = false;


    private void OnEnable()
    {
        if (_inputReader != null)
        {
            _inputReader.MoveEvent += OnMove;
            _inputReader.InteractEvent += OnInteract;
            _inputReader.SprintEvent += OnSprint;
        }
    }

    private void OnDisable()
    {
        if (_inputReader != null)
        {
            _inputReader.MoveEvent -= OnMove;
            _inputReader.InteractEvent -= OnInteract;
            _inputReader.SprintEvent -= OnSprint;
        }
    }

    private void Start()
    {
        _camera = GetComponentInChildren<Camera>();
        _inputReader.InitializeInput();
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = false; // Ensure isKinematic is false
    }

    private void OnSprint(bool state)
    {
        _isSprinting = state;
    }

    private void OnMove(Vector2 movement)
    {
        _movementInput = movement;
    }

    private void Update()
    {
        if (enableSprint)
        {
            if (_isSprinting && !_isSprintCooldown)
            {
                if (_isSprintingSoundCooldown)
                {
                    // if flash is on cooldown, increase the timer
                    _sprintingSoundTimer += Time.deltaTime;
                    // if the timer is greater than the cooldown, refresh the cooldown boolean and reset the timer
                    if (_sprintingSoundTimer >= 0.3f)
                    {
                        _isSprintingSoundCooldown = false;
                        _sprintingSoundTimer = 0f;
                    }
                }

                if (!_isSprintingSoundCooldown)
                {
                    _isSprintingSoundCooldown = true;
                }

                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, sprintFOV,
                    sprintFOVStepTime * Time.deltaTime);

                // Drain sprint remaining while sprinting
                if (!unlimitedSprint)
                {
                    _sprintRemaining -= 1 * Time.deltaTime;
                    if (_sprintRemaining <= 0)
                    {
                        _isSprinting = false;
                        _isSprintCooldown = true;
                    }
                }
            }
            else
            {
                // Regain sprint while not sprinting
                _sprintRemaining = Mathf.Clamp(_sprintRemaining += 1 * Time.deltaTime, 0, sprintDuration);
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, fov, zoomStepTime * Time.deltaTime);
            }

            // Handles sprint cooldown 
            // When sprint remaining == 0 stops sprint ability until hitting cooldown
            if (_isSprintCooldown)
            {
                sprintCooldown -= 1 * Time.deltaTime;
                if (sprintCooldown <= 0)
                {
                    _isSprintCooldown = false;
                }
            }
            else
            {
                sprintCooldown = _sprintCooldownReset;
            }
        }
    }

    private void FixedUpdate()
    {
        #region Movement

        if (playerCanMove)
        {
            // Use the input from _movementInput to determine movement
            Vector3 targetVelocity = new Vector3(_movementInput.x, 0, _movementInput.y);

            // Checks if player is walking and is grounded
            if (targetVelocity.x != 0 || targetVelocity.z != 0)
            {
                _isWalking = true;

                if (_isWalkingSoundCooldown)
                {
                    // if flash is on cooldown, increase the timer
                    _walkingSoundTimer += Time.fixedDeltaTime;
                    // if the timer is greater than the cooldown, refresh the cooldown boolean and reset the timer
                    if (_walkingSoundTimer >= 0.5f)
                    {
                        _isWalkingSoundCooldown = false;
                        _walkingSoundTimer = 0f;
                    }
                }

                if (!_isWalkingSoundCooldown)
                {
                    _isWalkingSoundCooldown = true;
                }
            }
            else
            {
                _isWalking = false;
            }

            // All movement calculations while sprint is active
            if (enableSprint && _isSprinting && _sprintRemaining > 0f && !_isSprintCooldown)
            {
                targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;

                Vector3 velocity = _rb.linearVelocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                if (velocityChange.x != 0 || velocityChange.z != 0)
                {
                    _isSprinting = true;
                }

                _rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
            // All movement calculations while walking
            else
            {
                _isSprinting = false;


                targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;
                Vector3 velocity = _rb.linearVelocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                _rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }

            if (targetVelocity != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetVelocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.fixedDeltaTime);
            }
        }

        UpdateCamera();

        #endregion
    }

    private void OnInteract()
    {
        Debug.Log("Interact");
        CheckForInteractableCollision();
    }


    private void UpdateCamera()
    {
        _camera.transform.localPosition = offset;
        _camera.transform.localRotation = Quaternion.Euler(40.45f, 0, 0);
    }

    private void CheckForInteractableCollision()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position,
            playerInteractionSettings.interactableRadius,
            playerInteractionSettings.interactableLayer);
        foreach (var hitCollider in hitColliders)
        {
            switch (hitCollider.GetComponent<IInteractable>())
            {
                case Chest chest:
                    chest.Interact();
                    break;
            }
        }
    }

    private void UpdateMovementBooleans()
    {
    }

    public bool IsWalking()
    {
        return _isWalking;
    }
}