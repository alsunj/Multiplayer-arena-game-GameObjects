using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public EnemyEvents enemyEvents;

    public void Initialize()
    {
        enemyEvents = new EnemyEvents();
    }
}