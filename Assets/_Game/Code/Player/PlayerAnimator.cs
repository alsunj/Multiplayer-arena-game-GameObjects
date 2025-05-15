using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles player animation states and synchronizes them with player events.
/// </summary>
public class PlayerAnimator : NetworkBehaviour
{
    private const string IS_WALKING = "IsWalking"; // Animation parameter for walking state.
    private const string IS_RUNNING = "IsRunning"; // Animation parameter for running state.
    private const string IS_INTERACTING = "Interact"; // Animation trigger for interaction.
    private const string IS_ATTACKING = "Attack"; // Animation trigger for attacking.
    private const string IS_DEFENDING = "IsDefending"; // Animation parameter for defending state.

    private PlayerEvents _playerEvents; // Reference to player events for subscribing to animation triggers.
    private Animator _animator; // Reference to the Animator component.

    /// <summary>
    /// Initializes the Animator component.
    /// </summary>
    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Unsubscribes from player events when the object is disabled.
    /// </summary>
    private void OnDisable()
    {
        if (_playerEvents != null)
        {
            _playerEvents.onPlayerWalk -= SetPlayerWalkBool;
            _playerEvents.onPlayerRun -= SetPlayerRunBool;
            _playerEvents.onPlayerInteract -= SetPlayerInteract;
            _playerEvents.onPlayerAttack -= SetPlayerAttack;
            _playerEvents.onPlayerDefence -= SetPlayerDefence;
        }
    }

    /// <summary>
    /// Subscribes to player events to handle animation state changes.
    /// </summary>
    /// <param name="playerEvents">The PlayerEvents instance to subscribe to.</param>
    public void InitializeEvents(PlayerEvents playerEvents)
    {
        this._playerEvents = playerEvents;
        if (_playerEvents != null)
        {
            _playerEvents.onPlayerWalk += SetPlayerWalkBool;
            _playerEvents.onPlayerRun += SetPlayerRunBool;
            _playerEvents.onPlayerInteract += SetPlayerInteract;
            _playerEvents.onPlayerAttack += SetPlayerAttack;
            _playerEvents.onPlayerDefence += SetPlayerDefence;
        }
    }

    /// <summary>
    /// Sets the running animation state.
    /// </summary>
    /// <param name="state">True if the player is running, false otherwise.</param>
    private void SetPlayerRunBool(bool state)
    {
        _animator.SetBool(IS_RUNNING, state);
    }

    /// <summary>
    /// Sets the walking animation state.
    /// </summary>
    /// <param name="state">True if the player is walking, false otherwise.</param>
    private void SetPlayerWalkBool(bool state)
    {
        _animator.SetBool(IS_WALKING, state);
    }

    /// <summary>
    /// Triggers the interaction animation.
    /// </summary>
    private void SetPlayerInteract()
    {
        _animator.CrossFade(IS_INTERACTING, 0.1f, -1, 0, 1f);
    }

    /// <summary>
    /// Triggers the attack animation.
    /// </summary>
    private void SetPlayerAttack()
    {
        _animator.CrossFade(IS_ATTACKING, 0.1f, -1, 0, 1f);
    }

    /// <summary>
    /// Sets the defending animation state.
    /// </summary>
    /// <param name="state">True if the player is defending, false otherwise.</param>
    private void SetPlayerDefence(bool state)
    {
        _animator.SetBool(IS_DEFENDING, state);
    }
}