using Unity.Netcode;
using UnityEngine;

public class Explosive : Pickupable, IExplosive
{
    private ExplosiveSettings _explosiveSettings;
    private BoxCollider _boxCollider;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }


    public void Explode()
    {
        throw new System.NotImplementedException();
    }

    public override void Pickup(GameObject pickupingTarget)
    {
        _boxCollider.isTrigger = true;
    }

    public override void PutDown(Vector3 position)
    {
        _boxCollider.isTrigger = false;
        //  transform.position = position;
    }
}