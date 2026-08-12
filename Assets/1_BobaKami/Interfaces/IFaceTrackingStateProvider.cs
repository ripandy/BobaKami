namespace BobaKami.Interfaces
{
    /// <summary>
    /// Port telling the domain whether the run being played is a face-tracking run, so a finished
    /// score can be filed against its own kind in <see cref="HighScoreTable"/>.
    /// <para>
    /// Deliberately a bool rather than an input-mode enum: the concrete modes (pointer, key,
    /// gamepad, Auto and how Auto resolves per platform) are adapter concerns, and the domain only
    /// needs the one distinction that changes how a score should be ranked.
    /// </para>
    /// </summary>
    public interface IFaceTrackingStateProvider
    {
        bool IsFaceTracking { get; }
    }
}
