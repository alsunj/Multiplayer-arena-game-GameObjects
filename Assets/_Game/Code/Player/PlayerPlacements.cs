using UnityEngine;

public class PlayerPlacements : MonoBehaviour
{
    public GameObject playerRightHand;
    public GameObject playerLeftHand;

    void Start()
    {
        playerRightHand = transform.Find("PlayerVisual/Skeleton_Minion/Rig/root/handIK.r/EquippedItemR")
            ?.gameObject;
        if (playerRightHand == null)
        {
            Debug.LogError("EquippedItemR not found in player rig");
        }

        playerLeftHand = transform.Find("PlayerVisual/Skeleton_Minion/Rig/root/handIK.l/EquippedItemL")
            ?.gameObject;
        if (playerLeftHand == null)
        {
            Debug.LogError("EquippedItemL not found in player rig");
        }
    }
}