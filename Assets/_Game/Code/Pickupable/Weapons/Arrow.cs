using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Represents an arrow that can follow a target's position and rotation.
/// </summary>
public class Arrow : MonoBehaviour
{
    private Transform _targetTransform; // The transform of the target the arrow will follow.

    /// <summary>
    /// Updates the arrow's position and rotation to follow the target.
    /// </summary>
    private void Update()
    {
        FollowTarget();
    }

    /// <summary>
    /// Makes the arrow follow the target's position and rotation.
    /// </summary>
    private void FollowTarget()
    {
        if (_targetTransform != null)
        {
            gameObject.transform.position = _targetTransform.position;
            gameObject.transform.rotation = _targetTransform.rotation;
        }
    }

    /// <summary>
    /// Sets the target transform for the arrow to follow.
    /// </summary>
    /// <param name="targetTransform">The transform of the target.</param>
    public void SetTargetTransform(Transform targetTransform)
    {
        _targetTransform = targetTransform;
    }

    /// <summary>
    /// Removes the target transform, stopping the arrow from following it.
    /// </summary>
    public void RemoveTargetTransform()
    {
        _targetTransform = null;
    }
}