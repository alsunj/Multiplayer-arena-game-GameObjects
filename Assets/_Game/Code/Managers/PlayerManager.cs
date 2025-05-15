using UnityEngine;

/// <summary>
/// Manages player-related functionality, including input handling and player events.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    /// <summary>
    /// Handles player input and provides input data to other systems.
    /// </summary>
    public InputReader inputReader;

    /// <summary>
    /// Handles events related to player actions such as movement, interaction, and combat.
    /// </summary>
    public PlayerEvents playerEvents;

    /// <summary>
    /// Initializes the PlayerManager by setting up input handling and player events.
    /// </summary>
    public void Initialize()
    {
        inputReader.InitializeInput();
        playerEvents = new PlayerEvents();
    }
}