using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    private const string IS_WALKING = "IsWalking";
    [SerializeField] private PlayerController _playerController;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        _animator.SetBool(IS_WALKING, _playerController.IsWalking());
    }
}