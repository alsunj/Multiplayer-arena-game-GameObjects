using System;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    private Transform targetTransform;

    [SerializeField] private FollowTransformSettings _followTransformSettings;

    public void SetTargetTransform(Transform targetTransform)
    {
        this.targetTransform = targetTransform;
    }

    private void LateUpdate()
    {
        if (targetTransform == null)
        {
            return;
        }

        transform.position = targetTransform.position;
        Vector3 targetEulerAngles = targetTransform.rotation.eulerAngles + _followTransformSettings.keyRotation;
        transform.rotation = Quaternion.Euler(targetEulerAngles);
    }
}