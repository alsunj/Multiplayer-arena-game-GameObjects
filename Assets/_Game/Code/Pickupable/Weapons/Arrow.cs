using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Arrow : MonoBehaviour
{
    private Transform _targetTransform;

    private void Update()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        if (_targetTransform != null)
        {
            gameObject.transform.position = _targetTransform.position;
            gameObject.transform.rotation = _targetTransform.rotation;
        }
    }

    public void SetTargetTransform(Transform targetTransform)
    {
        _targetTransform = targetTransform;
    }

    public void RemoveTargetTransform()
    {
        _targetTransform = null;
    }
}