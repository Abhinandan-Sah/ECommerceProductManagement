namespace Shared.Messaging
{
    /// <summary>
    /// Event published when Identity changes the total user count.
    /// </summary>
    public class UserCountChangedEvent
    {
        /// <summary>
        /// Change in user count to apply to the next reporting dashboard snapshot.
        /// </summary>
        public int Delta { get; set; }
    }
}
