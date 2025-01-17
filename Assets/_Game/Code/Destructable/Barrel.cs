using System;
using DG.Tweening;
using UnityEngine;

public class Barrel : MonoBehaviour, IDestrucable
{
    [SerializeField] private DestructableSettings _destructableSettings;

    private float _health;


    private void Start()
    {
        _health = _destructableSettings.health;
    }


    public void TakeDamage(float damage)
    {
        _health -= damage;
        Debug.Log(_health);
        if (_health <= 0)
        {
            transform.DOScale(Vector3.zero, 0.5f).SetDelay(_destructableSettings.hitDelay)
                .OnComplete(() => gameObject.SetActive(false));
        }
        else
        {
            transform.DOShakePosition(0.5f, _destructableSettings.shakeOffset, 10, 90, false, true)
                .SetDelay(_destructableSettings.hitDelay);
        }
    }
}