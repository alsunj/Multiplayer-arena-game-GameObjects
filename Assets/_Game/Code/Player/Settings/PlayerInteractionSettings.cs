using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInteractionSettings", menuName = "Scriptable Objects/PlayerInteractionSettings")]
public class PlayerInteractionSettings : ScriptableObject
{
    public LayerMask interactableLayer;
    public LayerMask destructableLayer;
    public LayerMask pickupableLayer;
    public float interactableRadius = 0.2f;
}