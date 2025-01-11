using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private PlayerInteractionSettings _playerInteractionSettings;
    [SerializeField] private float _speed = 2f;
    private Rigidbody _rb;

    private Camera _camera;
    public Vector3 offset = new Vector3(0, 7.4f, -6.4f);
    public Vector3 eulerAngles = new Vector3(40.45f, 0, 0);

    private bool _isWalking;


    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // if (!IsOwner)
        // {
        //     return;
        // }

        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckForInteractableCollision();
        }

        UpdateMovement();
    }

    private void UpdateMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized * _speed * Time.fixedDeltaTime;
        if (movement != Vector3.zero)
        {
            _rb.MovePosition(transform.position + movement);
            Debug.Log(movement);
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _speed);
            _camera.transform.rotation = Quaternion.Slerp(_camera.transform.rotation,
                targetRotation * Quaternion.Euler(eulerAngles), _speed);
            _isWalking = true;
        }
        else
        {
            _isWalking = false;
        }

        UpdateCamera();
        UpdateMovementBooleans();
    }

    private void UpdateCamera()
    {
        _camera.transform.position = gameObject.transform.position + offset;
    }

    private void CheckForInteractableCollision()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position,
            _playerInteractionSettings.interactableRadius,
            _playerInteractionSettings.interactableLayer);
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