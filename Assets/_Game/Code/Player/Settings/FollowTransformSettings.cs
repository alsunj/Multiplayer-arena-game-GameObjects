using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "FollowTransformSettings", menuName = "Scriptable Objects/FollowTransformSettings")]
public class FollowTransformSettings : ScriptableObject
{
    public Vector3 targetRotationOffset = new Vector3(180, 0, 0);
    public Vector3 targetPositionOffset = new Vector3(0f, 0, 0);
}