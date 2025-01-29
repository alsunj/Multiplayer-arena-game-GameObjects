using UnityEngine;

public interface IExplosive
{
    public void Explode();
    public void Throw(Vector3 throwDirection);
}