using BobaKami.Interfaces;

namespace BobaKami.Tests
{
    /// <summary>
    /// Face-tracking state double. Lets a test say which kind of run finished, so high-score
    /// submissions can be asserted against the table they belong in.
    /// </summary>
    internal class ScriptedFaceTrackingState : IFaceTrackingStateProvider
    {
        public ScriptedFaceTrackingState(bool isFaceTracking = true)
        {
            IsFaceTracking = isFaceTracking;
        }

        public bool IsFaceTracking { get; set; }
    }
}
