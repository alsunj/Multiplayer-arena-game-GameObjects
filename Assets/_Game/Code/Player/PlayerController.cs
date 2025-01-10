using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed = 2.0f;
    private Rigidbody _rb;

    private bool _isWalking;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized * _speed * Time.deltaTime;
        if (movement != Vector3.zero)
        {
            _rb.MovePosition(transform.position + movement);
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _speed);
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