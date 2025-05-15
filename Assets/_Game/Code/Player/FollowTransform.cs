using DG.Tweening;
using UnityEngine;

/// <summary>
/// Handles the movement and rotation of a GameObject to follow a target transform or player controls.
/// </summary>
public class FollowTransform : MonoBehaviour
{
    private ISwitchPlayerMap _targetPlayerControls; // Interface for managing player control states.
    private Transform _targetTransform; // The transform to follow.
    private Quaternion _startingRotation; // The initial rotation of the GameObject.

    [SerializeField]
    private FollowTransformSettings followTransformSettings; // Settings for movement and rotation behavior.

    /// <summary>
    /// Initializes the starting rotation of the GameObject.
    /// </summary>
    private void Start()
    {
        _startingRotation = transform.rotation;
    }

    /// <summary>
    /// Sets the target player controls to manage during the follow process.
    /// </summary>
    /// <param name="targetPlayerControls">The player controls to manage.</param>
    public void SetTargetPlayerControls(ISwitchPlayerMap targetPlayerControls)
    {
        this._targetPlayerControls = targetPlayerControls;
    }

    /// <summary>
    /// Sets the target transform for the GameObject to follow.
    /// </summary>
    /// <param name="targetTransform">The transform to follow.</param>
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

    /// <summary>
    /// Removes the target transform and moves the GameObject to a specified position.
    /// </summary>
    /// <param name="putDownPosition">The position to move the GameObject to.</param>
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

    /// <summary>
    /// Updates the GameObject's position and rotation to match the target transform.
    /// </summary>
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