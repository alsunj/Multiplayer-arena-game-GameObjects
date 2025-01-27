using UnityEngine;

public class Shield : MonoBehaviour
{
    private MeshCollider _shieldCollider;
    void Start()
    {
        _shieldCollider = GetComponent<MeshCollider>();
    }

}
