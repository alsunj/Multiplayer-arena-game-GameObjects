using DG.Tweening;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    private ISwitchPlayerMap _targetPlayerControls;
    private Transform targetTransform;
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
                this.targetTransform = targetTransform;
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
            targetTransform = null;
            _targetPlayerControls.TurnOnPlayerControls();
        });

        SetTargetPlayerControls(null);
    }

    private void LateUpdate()
    {
        if (targetTransform == null)
        {
            return;
        }

        transform.position = targetTransform.position + followTransformSettings.targetPositionOffset;
        Vector3 targetEulerAngles = targetTransform.rotation.eulerAngles + followTransformSettings.targetRotationOffset;
        transform.rotation = Quaternion.Euler(targetEulerAngles);
    }
}