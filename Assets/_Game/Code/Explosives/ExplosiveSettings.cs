using UnityEngine;

[CreateAssetMenu(fileName = "ExplosiveSettings", menuName = "Scriptable Objects/ExplosiveSettings")]
public class ExplosiveSettings : ScriptableObject
{
    public float explosionRadius = 3f;
    public float explosionForce = 10f;
    public float explosionDamage = 10f;
    public float throwForce = 10f;
    public LayerMask explosionLayerMask;
}
