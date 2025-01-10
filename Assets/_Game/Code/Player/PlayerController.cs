using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : NetworkBehaviour
{
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

    private void UpdateMovementBooleans()
    {
    }

    public bool IsWalking()
    {
        return _isWalking;
    }
}