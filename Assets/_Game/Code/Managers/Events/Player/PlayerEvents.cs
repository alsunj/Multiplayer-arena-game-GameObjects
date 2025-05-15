using System;

/// <summary>
/// Manages events related to player actions such as walking, running, defending, interacting, and attacking.
/// Provides methods to trigger these events and allows other components to subscribe to them.
/// </summary>
public class PlayerEvents
{
    /// <summary>
    /// Event triggered when the player starts or stops walking.
    /// The boolean parameter indicates whether the player is walking (true) or not (false).
    /// </summary>
    public event Action<bool> onPlayerWalk;

    /// <summary>
    /// Event triggered when the player starts or stops running.
    /// The boolean parameter indicates whether the player is running (true) or not (false).
    /// </summary>
    public event Action<bool> onPlayerRun;

    /// <summary>
    /// Event triggered when the player starts or stops defending.
    /// The boolean parameter indicates whether the player is defending (true) or not (false).
    /// </summary>
    public event Action<bool> onPlayerDefence;

    /// <summary>
    /// Event triggered when the player interacts with an object.
    /// </summary>
    public event Action onPlayerInteract;

    /// <summary>
    /// Event triggered when the player performs an attack.
    /// </summary>
    public event Action onPlayerAttack;

    /// <summary>
    /// Triggers the onPlayerDefence event to notify subscribers that the player has started or stopped defending.
    /// </summary>
    /// <param name="state">Indicates whether the player is defending (true) or not (false).</param>
    public void PlayerDefence(bool state)
    {
        if (onPlayerDefence != null)
        {
            onPlayerDefence(state);
        }
    }

    /// <summary>
    /// Triggers the onPlayerWalk event to notify subscribers that the player has started or stopped walking.
    /// </summary>
    /// <param name="state">Indicates whether the player is walking (true) or not (false).</param>
    public void PlayerWalk(bool state)
    {
        if (onPlayerWalk != null)
        {
            onPlayerWalk(state);
        }
    }

    /// <summary>
    /// Triggers the onPlayerRun event to notify subscribers that the player has started or stopped running.
    /// </summary>
    /// <param name="state">Indicates whether the player is running (true) or not (false).</param>
    public void PlayerRun(bool state)
    {
        if (onPlayerRun != null)
        {
            onPlayerRun(state);
        }
    }

    /// <summary>
    /// Triggers the onPlayerInteract event to notify subscribers that the player has interacted with an object.
    /// </summary>
    public void PlayerInteract()
    {
        if (onPlayerInteract != null)
        {
            onPlayerInteract();
        }
    }

    /// <summary>
    /// Triggers the onPlayerAttack event to notify subscribers that the player has performed an attack.
    /// </summary>
    public void PlayerAttack()
    {
        if (onPlayerAttack != null)
        {
            onPlayerAttack();
        }
    }
}