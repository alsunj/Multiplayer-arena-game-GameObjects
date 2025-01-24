using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    private const string IS_WALKING = "IsWalking";
    private const string IS_RUNNING = "IsRunning";
    private const string IS_INTERACTING = "Interact";
    private const string IS_ATTACKING = "Attack";

    private PlayerEvents _playerEvents;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }


    private void OnDisable()
    {
        if (_playerEvents != null)
        {
            _playerEvents.onPlayerWalk -= SetPlayerWalkBool;
            _playerEvents.onPlayerRun -= SetPlayerRunBool;
            _playerEvents.onPlayerInteract -= SetPlayerInteract;
            _playerEvents.onPlayerAttack -= SetPlayerAttack;
        }
    }

    public void InitializeEvents(PlayerEvents playerEvents)
    {
        this._playerEvents = playerEvents;
        if (_playerEvents != null)
        {
            _playerEvents.onPlayerWalk += SetPlayerWalkBool;
            _playerEvents.onPlayerRun += SetPlayerRunBool;
            _playerEvents.onPlayerInteract += SetPlayerInteract;
            _playerEvents.onPlayerAttack += SetPlayerAttack;
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
    }

    private void SetPlayerRunBool(bool obj)
    {
        _animator.SetBool(IS_RUNNING, obj);
    }

    private void SetPlayerWalkBool(bool obj)
    {
        _animator.SetBool(IS_WALKING, obj);
    }

    private void SetPlayerInteract()
    {
        _animator.CrossFade(IS_INTERACTING, 0.1f, -1, 0, 1f);
    }

    private void SetPlayerAttack()
    {
        _animator.CrossFade(IS_ATTACKING, 0.1f, -1, 0, 1f);
    }
}