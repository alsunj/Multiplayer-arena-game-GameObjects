using DG.Tweening;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    private ISwitchPlayerMap _targetPlayerControls;
    private Transform _targetTransform;
    private Quaternion _startingRotation;


    [SerializeField] private FollowTransformSettings followTransformSettings;

    private void Start()
    {
        _startingRotation = transform.rotation;
    }

    public void SetTargetPlayerControls(ISwitchPlayerMap targetPlayerControls)
    {
        this._targetPlayerControls = targetPlayerControls;
    }

    public void SetTargetTransform(Transform targetTransform)
    {
        if (targetTransform != null)
        {
            _targetPlayerControls.TurnOffPlayerControls();
            transform.DOMove(targetTransform.position, followTransformSettings.pickupDuration).OnComplete(() =>
            {
                this._targetTransform = targetTransform;
                _targetPlayerControls.TurnOnPlayerControls();
            });
        }
    }

    public void RemoveTargetTransform(Vector3 putDownPosition)
    {
        _targetPlayerControls.TurnOffPlayerControls();
        transform.DOMove(putDownPosition, followTransformSettings.putDownDuration).OnComplete(() =>
        {
            transform.rotation = _startingRotation;
            _targetPlayerControls.TurnOnPlayerControls();
            SetTargetPlayerControls(null);
            _targetTransform = null;
        });
    }

    private void LateUpdate()
    {
        if (_targetTransform == null)
        {
            return;
        }

        transform.position = _targetTransform.position + followTransformSettings.targetPositionOffset;
        Vector3 targetEulerAngles =
            _targetTransform.rotation.eulerAngles + followTransformSettings.targetRotationOffset;
        transform.rotation = Quaternion.Euler(targetEulerAngles);
    }
}