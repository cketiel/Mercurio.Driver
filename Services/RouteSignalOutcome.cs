namespace Raphael.Driver.Services
{
    /// <summary>
    /// What the screen on top did with a route signal.
    /// </summary>
    public enum RouteSignalOutcome
    {
        /// <summary>
        /// The screen showed it and acted on it. The signal has done its job and is deleted.
        /// </summary>
        Handled,

        /// <summary>
        /// The screen saw it and it does not concern what is on it — a trip it is not
        /// showing. Also deleted: every screen reloads its data when the driver opens it, so
        /// keeping the signal around would only make it interrupt somebody later for a change
        /// they already have.
        /// </summary>
        NotRelevant,

        /// <summary>
        /// Not now. Kept, and offered again when the driver reaches a screen that can take it.
        /// </summary>
        /// <remarks>
        /// ⚠️ This exists for signature capture. Interrupting a driver holding a patient's
        /// finger on the screen loses the signature, and a signature is the proof the trip
        /// happened.
        /// </remarks>
        Deferred
    }
}
