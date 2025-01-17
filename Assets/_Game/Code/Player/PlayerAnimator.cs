using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    private const string IS_WALKING = "IsWalking";
    private const string IS_RUNNING = "IsRunning";
    private const string IS_INTERACTING = "Interact";

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
        _animator.SetTrigger(IS_INTERACTING);
    }
}