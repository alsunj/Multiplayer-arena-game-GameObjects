using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "Scriptable Objects/EnemySettings")]
public class EnemySettings : ScriptableObject
{
    public LayerMask targetLayer;
    public float detectionRange = 7f;
    public float shootingRange = 10f;
    public float shootingDelay = 1f;
}