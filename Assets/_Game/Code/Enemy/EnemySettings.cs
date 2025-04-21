using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "Scriptable Objects/EnemySettings")]
public class EnemySettings : ScriptableObject
{
    public LayerMask targetLayer;
    public float detectionRange = 7f;
    public float shootingRange = 20f;
    public float shootingDelay = 1f;
}