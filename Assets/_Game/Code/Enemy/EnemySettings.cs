using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "Scriptable Objects/EnemySettings")]
public class EnemySettings : ScriptableObject
{
    public LayerMask targetLayer;
    public int detectionRange = 7;
    public int shootingRange = 20;
    public float shootingDelay = 1f;
    public int speed = 5;
}