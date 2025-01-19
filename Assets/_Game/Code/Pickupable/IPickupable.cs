using UnityEngine;

public interface IPickupable
{
    public void Pickup(GameObject targetobject);
    public void PutDown(Vector3 position);
}