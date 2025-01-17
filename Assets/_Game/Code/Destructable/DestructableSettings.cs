using UnityEngine;

[CreateAssetMenu(fileName = "DestructableSettings", menuName = "Scriptable Objects/DestructableSettings")]
public class DestructableSettings : ScriptableObject
{
    public float health = 20f;
    public float hitDelay = 1f;
    public Vector3 shakeOffset = new Vector3(0.2f, 0, 0.2f);
}