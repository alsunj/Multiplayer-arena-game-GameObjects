using System;
using UnityEngine;
using UnityEngine.Serialization;

public class FollowTransform : MonoBehaviour
{
    private Transform targetTransform;

    [SerializeField] private FollowTransformSettings followTransformSettings;

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

        transform.position = targetTransform.position + followTransformSettings.targetPositionOffset;
        Vector3 targetEulerAngles = targetTransform.rotation.eulerAngles + followTransformSettings.targetRotationOffset;
        transform.rotation = Quaternion.Euler(targetEulerAngles);
    }
}