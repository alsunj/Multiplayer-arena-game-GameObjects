using UnityEngine;

[CreateAssetMenu(fileName = "FollowTransformSettings", menuName = "Scriptable Objects/FollowTransformSettings")]
public class FollowTransformSettings : ScriptableObject
{
    public Vector3 keyRotation = new Vector3(180, 180, 0);
}