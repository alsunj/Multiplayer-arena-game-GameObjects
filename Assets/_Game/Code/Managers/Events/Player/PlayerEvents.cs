using System;

public class PlayerEvents
{
    public event Action<bool> onPlayerWalk;
    public event Action<bool> onPlayerRun;
    public event Action<bool> onPlayerDefence;
    public event Action onPlayerInteract;
    public event Action onPlayerAttack;


    public void PlayerDefence(bool state)
    {
        if (onPlayerDefence != null)
        {
            onPlayerDefence(state);
        }
    }

    public void PlayerWalk(bool state)
    {
        if (onPlayerWalk != null)
        {
            onPlayerWalk(state);
        }
    }

    public void PlayerRun(bool state)
    {
        if (onPlayerRun != null)
        {
            onPlayerRun(state);
        }
    }

    public void PlayerInteract()
    {
        if (onPlayerInteract != null)
        {
            onPlayerInteract();
        }
    }

    public void PlayerAttack()
    {
        if (onPlayerAttack != null)
        {
            onPlayerAttack();
        }
    }
}