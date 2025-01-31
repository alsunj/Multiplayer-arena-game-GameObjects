using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class Explosive : Pickupable, IExplosive
{
    [SerializeField] private ExplosiveSettings explosiveSettings;
    private BoxCollider _boxCollider;
    private Rigidbody _rigidbody;

    private void Start()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _rigidbody = GetComponent<Rigidbody>();
    }


    public void Explode()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosiveSettings.explosionRadius, explosiveSettings.explosionLayerMask);
        foreach (var hitCollider in hitColliders)
        {
            
        }
    }

    public void Throw(Vector3 throwDirection)
    {
        _rigidbody.isKinematic = false;
        _rigidbody.AddForce(throwDirection * explosiveSettings.throwForce, ForceMode.VelocityChange);
    }

    public override void Pickup()
    {
        _boxCollider.isTrigger = true;
    }

    public override void PutDown()
    {
        _boxCollider.isTrigger = false;
        //  transform.position = position;
    }
}