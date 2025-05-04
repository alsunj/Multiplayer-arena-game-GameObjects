using System;
using Unity.Netcode;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    public int DetectionRadius { get; set; }
    public Transform Target { get; set; }
    public LayerMask TargetLayerMask { get; set; }

    protected virtual void InitializeEnemy(int detectionRange, LayerMask targetLayerMask)
    {
        DetectionRadius = detectionRange;
        TargetLayerMask = targetLayerMask;
        Target = null;
    }

    // public override void OnDestroy()
    // {
    //     TargetingManager.Instance.RemoveEnemyFromTargetingList(this);
    // }
}