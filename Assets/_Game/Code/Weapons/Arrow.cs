using UnityEngine;
using UnityEngine.UIElements;

public class Arrow : MonoBehaviour
{
    private BoxCollider _boxCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
    }
}