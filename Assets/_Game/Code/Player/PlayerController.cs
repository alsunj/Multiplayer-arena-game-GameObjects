using System.Collections;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    #region components

    [SerializeField] private PlayerInteractionSettings playerInteractionSettings;
    [SerializeField] private float speed = 2f;
    [SerializeField] private InputReader inputReader;

    private PlayerPlacements _playerPlacements;
    private PlayerManager _playerManager;
    private PlayerAnimator _playerAnimator;
    private Rigidbody _rb;
    private Camera _camera;

    #endregion

    #region cameraProperties

    public Vector3 offset = new Vector3(0, 7.4f, -6.4f);

    #endregion

    #region movementProperties

    public bool enableSprint = true;
    public bool unlimitedSprint;
    public float sprintSpeed = 7f;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;
    public float zoomStepTime = 5f;

    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;

    public float fov = 60;
    private bool _isWalking;


    // Internal Variables
    private Vector2 _movementInput;
    private bool _isSprinting;
    private float _sprintRemaining;
    private bool _isSprintCooldown;
    private float _sprintCooldownReset;


    // Internal Variables
    private Vector3 _jointOriginalPos;
    private float _timer;

    private float _walkingSoundTimer;
    private bool _isWalkingSoundCooldown;

    private float _sprintingSoundTimer;
    private bool _isSprintingSoundCooldown;

    #endregion

    #region attackProperties

    public float hitDamage = 5f;
    public float attackCooldown = 1f;
    private float _attackCooldownTimer;

    #endregion

    #region PickupPropertios

    private bool isRightHandFull;

    #endregion

    private void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.MoveEvent += OnMove;
            inputReader.InteractEvent += OnInteract;
            inputReader.SprintEvent += OnSprint;
            inputReader.AttackEvent += OnAttack;
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.MoveEvent -= OnMove;
            inputReader.InteractEvent -= OnInteract;
            inputReader.SprintEvent -= OnSprint;
            inputReader.AttackEvent -= OnAttack;
        }
    }

    private void Start()
    {
        _sprintRemaining = sprintDuration;
        _playerManager = GetComponent<PlayerManager>();

        if (_playerManager == null)
        {
            Debug.LogError("PlayerManager is null");
        }
        else
        {
            _playerManager.Initialize();
        }

        _playerAnimator = GetComponentInChildren<PlayerAnimator>();

        if (_playerAnimator == null)
        {
            Debug.LogError("PlayerAnimator is null");
        }
        else
        {
            _playerAnimator.InitializeEvents(_playerManager.playerEvents);
        }

        _camera = FindFirstObjectByType<Camera>();
        if (_camera == null)
        {
            Debug.LogError("Camera is null");
        }
        else
        {
            _camera.transform.rotation = Quaternion.Euler(40.45f, 0, 0);
        }


        inputReader.InitializeInput();
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError("Rigidbody is null");
        }
        else
        {
            _rb.isKinematic = false;
        }

        _playerPlacements = GetComponent<PlayerPlacements>();
        if (_playerPlacements == null)
        {
            Debug.LogError("PlayerPlacements is null");
        }
    }

    private void OnSprint(bool state)
    {
        _isSprinting = state;
    }

    private void OnMove(Vector2 movement)
    {
        _movementInput = movement;
    }

    private void OnAttack()
    {
        if (_attackCooldownTimer > 0) return;
        CheckForWeapons();
        _attackCooldownTimer = attackCooldown; // Set cooldown duration
    }

    private void OnInteract()
    {
        CheckForPickupableAndInteractableCollision();
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (_attackCooldownTimer > 0)
        {
            _attackCooldownTimer -= Time.deltaTime;
        }

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
        if (!IsOwner)
        {
            return;
        }

        #region Movement

        if (playerCanMove)
        {
            // Use the input from _movementInput to determine movement
            //Vector3 targetVelocity = new Vector3(_movementInput.x, 0, _movementInput.y);
            //the line below doesn't let the player move backwards
            Vector3 targetVelocity = new Vector3(_movementInput.x, 0, Mathf.Max(0, _movementInput.y));
            targetVelocity = transform.TransformDirection(targetVelocity);

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
                targetVelocity *= sprintSpeed;
            }
            else
            {
                targetVelocity *= walkSpeed;
            }

            _playerManager.playerEvents.PlayerRun(_isSprinting);
            _playerManager.playerEvents.PlayerWalk(_isWalking);


            Vector3 velocity = _rb.linearVelocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;

            _rb.AddForce(velocityChange, ForceMode.VelocityChange);
            // this check doesn't rotate player if he's not moving 
            if (targetVelocity != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetVelocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
            }
        }

        UpdateCamera();

        #endregion
    }


    private void UpdateCamera()
    {
        _camera.transform.position = offset + gameObject.transform.position;
    }

    private void CheckForPickupableAndInteractableCollision()
    {
        if (CheckForInteractableCollision()) return;
        CheckForPickupables();
    }


    private bool CheckForInteractableCollision()
    {
        Collider closestCollider = FindClosestCollider(transform.position,
            playerInteractionSettings.interactableRadius,
            playerInteractionSettings.interactableLayer);

        if (closestCollider != null)
        {
            var interactable = closestCollider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                RotatePlayerTowardsTarget(closestCollider);
                if (interactable.Interact())
                {
                    _playerManager.playerEvents.PlayerInteract();
                    return true;
                }
            }
        }

        return false;
    }

    private void CheckForPickupables()
    {
        Collider closestCollider = FindClosestCollider(transform.position,
            playerInteractionSettings.interactableRadius,
            playerInteractionSettings.pickupableLayer);

        if (closestCollider != null)
        {
            switch (closestCollider.GetComponent<Pickupable>())
            {
                case Key key:
                    PickupObject(key);
                    break;
                case Explosive explosive:
                    PickupObject(explosive);
                    break;
            }
        }
    }

    private void PickupObject(Pickupable pickupable)
    {
        if (!_playerPlacements.IsRightHandFull())
        {
            pickupable.RequestPickupObject(
                new NetworkObjectReference(gameObject.GetComponent<NetworkObject>()));
        }
        else
        {
            pickupable.RequestPutDownObject(
                new NetworkObjectReference(gameObject.GetComponent<NetworkObject>()));
        }
    }

    private void CheckForWeapons()
    {
        HitObject(hitDamage);
    }

    private void HitObject(float weaponHitDamage)
    {
        Collider closestCollider = FindClosestCollider(transform.position,
            playerInteractionSettings.interactableRadius,
            playerInteractionSettings.destructableLayer);

        if (closestCollider != null)
        {
            switch (closestCollider.GetComponent<IDestrucable>())
            {
                case Barrel barrel:
                    RotatePlayerTowardsTarget(closestCollider);
                    _playerManager.playerEvents.PlayerAttack();
                    barrel.TakeDamage(weaponHitDamage);
                    break;
            }
        }
        else
        {
            _playerManager.playerEvents.PlayerAttack();
        }
    }

    private Collider FindClosestCollider(Vector3 position, float radius, LayerMask layerMask)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, radius, layerMask);
        Collider closestCollider = null;
        float closestDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            float distance = Vector3.Distance(position, hitCollider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCollider = hitCollider;
            }
        }

        return closestCollider;
    }

    private void RotatePlayerTowardsTarget(Collider hit)
    {
        Vector3 direction = (hit.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.DORotateQuaternion(lookRotation, 0.3f);
    }
}