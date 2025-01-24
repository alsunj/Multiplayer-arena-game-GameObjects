using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerManager : MonoBehaviour
{
    public InputReader inputReader;
    public PlayerEvents playerEvents;

    public void Initialize()
    {
        inputReader.InitializeInput();
        playerEvents = new PlayerEvents();
    }
}