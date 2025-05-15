using System.Collections;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages player controls, including movement, interaction, sprinting, attacking, and defending.
/// </summary>
public class PlayerController : NetworkBehaviour
{
    #region components

    [SerializeField] private PlayerInteractionSettings playerInteractionSettings; // Settings for player interaction.
    private PlayerPlacements _playerPlacements; // Manages player item placements.
    private PlayerManager _playerManager; // Manages player-related events and input.
    private PlayerAnimator _playerAnimator; // Handles player animations.
    private Rigidbody _rb; // Rigidbody component for physics-based movement.
    private Camera _camera; // Main camera reference.

    #endregion

    #region cameraProperties

    public Vector3 offset = new Vector3(0, 7.4f, -6.4f); // Offset for the camera position.

    #endregion

    #region movementProperties

    public bool enableSprint = true; // Determines if sprinting is enabled.
    public bool unlimitedSprint; // Determines if sprinting is unlimited.
    public float sprintSpeed = 7f; // Speed multiplier when sprinting.
    public float sprintDuration = 5f; // Maximum duration of sprinting.
    public float sprintCooldown = .5f; // Cooldown time after sprinting.
    public float sprintFOV = 80f; // Field of view during sprinting.
    public float sprintFOVStepTime = 10f; // Speed of FOV transition during sprinting.
    public float zoomStepTime = 5f; // Speed of FOV transition when not sprinting.

    public bool playerCanMove = true; // Determines if the player can move.
    public float walkSpeed = 5f; // Speed multiplier when walking.
    public float maxVelocityChange = 10f; // Maximum change in velocity per frame.

    public float fov = 60; // Default field of view.
    private bool _isWalking; // Tracks if the player is walking.

    private Vector2 _movementInput; // Input for movement.
    private bool _isSprinting; // Tracks if the player is sprinting.
    private float _sprintRemaining; // Remaining sprint duration.
    private bool _isSprintCooldown; // Tracks if sprint is on cooldown.
    private float _sprintCooldownReset; // Resets sprint cooldown.

    private Vector3 _jointOriginalPos; // Original position of the joint.
    private float _timer; // General-purpose timer.

    private float _walkingSoundTimer; // Timer for walking sound cooldown.
    private bool _isWalkingSoundCooldown; // Tracks if walking sound is on cooldown.

    private float _sprintingSoundTimer; // Timer for sprinting sound cooldown.
    private bool _isSprintingSoundCooldown; // Tracks if sprinting sound is on cooldown.

    #endregion

    #region defenceProperties

    public float defenceCooldown = 0.5f; // Cooldown time for defending.
    private float _defenceCooldownTimer; // Tracks remaining cooldown for defending.

    #endregion

    #region attackProperties

    public float hitDamage = 5f; // Damage dealt by attacks.
    public float attackCooldown = 1f; // Cooldown time for attacking.
    private float _attackCooldownTimer; // Tracks remaining cooldown for attacking.

    #endregion

    #region PickupProperties

    private bool isRightHandFull; // Tracks if the player's right hand is holding an item.

    #endregion

    /// <summary>
    /// Unsubscribes from input events when the object is disabled.
    /// </summary>
    private void OnDisable()
    {
        if (_playerManager.inputReader != null)
        {
            _playerManager.inputReader.MoveEvent -= OnMove;
            _playerManager.inputReader.InteractEvent -= OnInteract;
            _playerManager.inputReader.SprintEvent -= OnSprint;
            _playerManager.inputReader.AttackEvent -= OnAttack;
            _playerManager.inputReader.DefenceEvent -= OnDefence;
        }
    }

    /// <summary>
    /// Initializes movement-related input events.
    /// </summary>
    private void InitializeMovements()
    {
        _playerManager.inputReader.MoveEvent += OnMove;
        _playerManager.inputReader.InteractEvent += OnInteract;
        _playerManager.inputReader.SprintEvent += OnSprint;
        _playerManager.inputReader.AttackEvent += OnAttack;
        _playerManager.inputReader.DefenceEvent += OnDefence;
    }

    /// <summary>
    /// Initializes player components and settings.
    /// </summary>
    private void Start()
    {
        _sprintRemaining = sprintDuration;
        _playerManager = GetComponent<PlayerManager>();
        if (_playerManager.inputReader != null)
        {
            _playerManager.Initialize();
            InitializeMovements();
        }
        else
        {
            Debug.LogError("InputReader is null");
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

        _camera = Camera.main;
        if (_camera == null)
        {
            Debug.LogError("Camera is null");
        }
        else
        {
            _camera.transform.rotation = Quaternion.Euler(40.45f, 0, 0);
        }

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

    /// <summary>
    /// Handles sprint input.
    /// </summary>
    /// <param name="state">True if sprinting, false otherwise.</param>
    private void OnSprint(bool state)
    {
        _isSprinting = state;
    }

    /// <summary>
    /// Handles movement input.
    /// </summary>
    /// <param name="movement">Movement input vector.</param>
    private void OnMove(Vector2 movement)
    {
        _movementInput = movement;
    }

    /// <summary>
    /// Handles attack input.
    /// </summary>
    private void OnAttack()
    {
        if (_attackCooldownTimer > 0) return;
        CheckForWeapons();
        _attackCooldownTimer = attackCooldown;
    }

    /// <summary>
    /// Handles defence input.
    /// </summary>
    /// <param name="state">True if defending, false otherwise.</param>
    private void OnDefence(bool state)
    {
        if (state)
        {
            if (_defenceCooldownTimer > 0) return;
            PlayerDefend(state);
            _defenceCooldownTimer = defenceCooldown;
        }
        else
        {
            PlayerDefend(state);
        }
    }

    /// <summary>
    /// Handles interaction input.
    /// </summary>
    private void OnInteract()
    {
        CheckForPickupableAndInteractableCollision();
    }

    /// <summary>
    /// Updates player state and handles cooldowns.
    /// </summary>
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

        if (_defenceCooldownTimer > 0)
        {
            _defenceCooldownTimer -= Time.deltaTime;
        }

        if (enableSprint)
        {
            HandleSprint();
        }
    }

    /// <summary>
    /// Handles player movement and camera updates.
    /// </summary>
    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        if (playerCanMove)
        {
            HandleMovement();
        }

        UpdateCamera();
    }

    /// <summary>
    /// Updates the camera position based on the player's position.
    /// </summary>
    private void UpdateCamera()
    {
        _camera.transform.position = offset + gameObject.transform.position;
    }

    /// <summary>
    /// Checks for interactable or pickupable objects and interacts with them.
    /// </summary>
    private void CheckForPickupableAndInteractableCollision()
    {
        if (CheckForInteractableCollision()) return;
        CheckForPickupables();
    }

    /// <summary>
    /// Checks for interactable objects within range and interacts with them.
    /// </summary>
    /// <returns>True if an interaction occurred, false otherwise.</returns>
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
                PlayPlayerInteract(interactable.Interact());
            }
        }

        return false;
    }

    /// <summary>
    /// Checks for pickupable objects within range and picks them up.
    /// </summary>
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
                    RotatePlayerTowardsTarget(closestCollider);
                    PlayPlayerInteract(PickupObject(key));
                    break;
                case Explosive explosive:
                    RotatePlayerTowardsTarget(closestCollider);
                    PlayPlayerInteract(PickupObject(explosive));
                    break;
            }
        }
    }

    /// <summary>
    /// Plays the interaction animation if the interaction was successful.
    /// </summary>
    /// <param name="state">True if the interaction was successful, false otherwise.</param>
    private void PlayPlayerInteract(bool state)
    {
        if (state)
        {
            _playerManager.playerEvents.PlayerInteract();
        }
    }

    /// <summary>
    /// Picks up or puts down an object.
    /// </summary>
    /// <param name="pickupable">The object to pick up or put down.</param>
    /// <returns>True if the action was successful, false otherwise.</returns>
    private bool PickupObject(Pickupable pickupable)
    {
        if (!_playerPlacements.IsRightHandFull())
        {
            return pickupable.RequestPickupObject(
                new NetworkObjectReference(gameObject.GetComponent<NetworkObject>()));
        }
        else
        {
            pickupable.RequestPutDownObject(
                new NetworkObjectReference(gameObject.GetComponent<NetworkObject>()));
            return true;
        }
    }

    /// <summary>
    /// Handles the player's defence state.
    /// </summary>
    /// <param name="state">True if defending, false otherwise.</param>
    private void PlayerDefend(bool state)
    {
        Debug.Log("player defending" + state);
        _playerManager.playerEvents.PlayerDefence(state);
    }

    /// <summary>
    /// Checks for weapons and performs an attack.
    /// </summary>
    private void CheckForWeapons()
    {
        HitObject(hitDamage);
    }

    /// <summary>
    /// Deals damage to the closest destructible object within range.
    /// </summary>
    /// <param name="weaponHitDamage">The amount of damage to deal.</param>
    private void HitObject(float weaponHitDamage)
    {
        Collider closestCollider = FindClosestCollider(transform.position,
            playerInteractionSettings.interactableRadius,
            playerInteractionSettings.destructableLayer);

        if (closestCollider != null)
        {
            switch (closestCollider.GetComponent<IDamagable>())
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

    /// <summary>
    /// Finds the closest collider within a specified radius and layer mask.
    /// </summary>
    /// <param name="position">The position to search from.</param>
    /// <param name="radius">The search radius.</param>
    /// <param name="layerMask">The layer mask to filter colliders.</param>
    /// <returns>The closest collider, or null if none are found.</returns>
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

    /// <summary>
    /// Rotates the player to face a target collider.
    /// </summary>
    /// <param name="hit">The target collider.</param>
    private void RotatePlayerTowardsTarget(Collider hit)
    {
        Vector3 direction = (hit.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.DORotateQuaternion(lookRotation, 0.3f);
    }

    /// <summary>
    /// Handles sprinting logic, including cooldowns and FOV adjustments.
    /// </summary>
    private void HandleSprint()
    {
        if (_isSprinting && !_isSprintCooldown)
        {
            if (_isSprintingSoundCooldown)
            {
                _sprintingSoundTimer += Time.deltaTime;
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
            _sprintRemaining = Mathf.Clamp(_sprintRemaining += 1 * Time.deltaTime, 0, sprintDuration);
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, fov, zoomStepTime * Time.deltaTime);
        }

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

    /// <summary>
    /// Handles player movement, including walking and sprinting.
    /// </summary>
    private void HandleMovement()
    {
        Vector3 targetVelocity = new Vector3(_movementInput.x, 0, Mathf.Max(0, _movementInput.y));
        targetVelocity = transform.TransformDirection(targetVelocity);

        if (targetVelocity.x != 0 || targetVelocity.z != 0)
        {
            _isWalking = true;

            if (_isWalkingSoundCooldown)
            {
                _walkingSoundTimer += Time.fixedDeltaTime;
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

        if (targetVelocity != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
        }
    }
}