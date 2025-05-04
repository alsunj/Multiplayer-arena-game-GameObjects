using UnityEngine;

[CreateAssetMenu(fileName = "DamageCollisionSettings", menuName = "Scriptable Objects/DamageCollisionSettings")]
public class DamageCollisionSettings : ScriptableObject
{
    public int damageAmount;
    public LayerMask targetLayer;
}