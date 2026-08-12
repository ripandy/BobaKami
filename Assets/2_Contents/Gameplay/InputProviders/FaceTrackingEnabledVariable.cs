using BobaKami.Interfaces;
using Soar.Variables;

namespace BobaKami.Gameplay
{
    /// <summary>
    /// Whether face tracking is both available on this device and selected by the current input
    /// mode. Doubles as the domain's <see cref="IFaceTrackingStateProvider"/> port, the same way
    /// <see cref="HUD.HealthPercentageVariable"/> doubles as IPlayerHealthPresenter, so the
    /// high-score table can file a finished run against face or touch without the domain ever
    /// learning what an input mode is.
    /// <para>
    /// <see cref="FaceTrackingAdapter"/> remains the single writer — it lives in the Core scene
    /// and so stays loaded on every scene. This type only adds a reader.
    /// </para>
    /// </summary>
    public class FaceTrackingEnabledVariable : Variable<bool>, IFaceTrackingStateProvider
    {
        public bool IsFaceTracking => Value;
    }
}
