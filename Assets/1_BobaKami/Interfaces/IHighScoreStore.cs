namespace BobaKami.Interfaces
{
    /// <summary>
    /// Port for persisting the high-score table. The domain decides *when* a save is
    /// meaningful; the adapter owns *how* it is written.
    /// </summary>
    public interface IHighScoreStore
    {
        void Save(HighScoreTable table);
    }
}
