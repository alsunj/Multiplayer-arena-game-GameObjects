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

    private bool _isWalking;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

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
            _isWalking = true;
        }
        else
        {
            _isWalking = false;
        }

        UpdateMovementBooleans();
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