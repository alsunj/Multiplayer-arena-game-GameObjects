using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerEvents playerEvents;

    public void Initialize()
    {
        playerEvents = new PlayerEvents();
    }
}