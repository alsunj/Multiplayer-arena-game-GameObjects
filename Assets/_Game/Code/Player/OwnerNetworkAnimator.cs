using Unity.Netcode.Components;

/// <summary>
/// A custom implementation of NetworkAnimator that allows client authority over animation synchronization.
/// </summary>
public class OwnerNetworkAnimator : NetworkAnimator
{
    /// <summary>
    /// Determines whether the server is authoritative for this animator. 
    /// This implementation makes the owner client authoritative instead of the server.
    /// </summary>
    /// <returns>False, indicating that the owner client is authoritative.</returns>
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}