using BobaKami.Interfaces;
using Soar;
using Soar.Variables;
using UnityEngine;

namespace BobaKami.Gameplay
{
    /// <summary>
    /// Persisted high-score table. Doubles as the domain's <see cref="IHighScoreStore"/> port,
    /// the same way <see cref="HUD.GameStatsVariable"/> doubles as IPlayerStatsPresenter.
    /// The asset must have autoResetValue off, or PlayMode exit wipes the loaded table.
    /// </summary>
    [CreateAssetMenu(fileName = "HighScoreData", menuName = "BobaKami/HighScoreData")]
    public class HighScoreData : JsonableVariable<HighScoreTable>, IHighScoreStore
    {
        private static string Directory => Application.persistentDataPath;

        public void Save(HighScoreTable table)
        {
            // Raise rather than assign Value: the domain mutates the table in place, so the
            // setter's equality check would swallow the notification.
            Raise(table);
            this.SaveToJson(Directory, name);
        }

        /// <summary>
        /// Reads the saved table, or starts an empty one. Must run before the value is bound
        /// into the DI container — FromJsonString replaces the instance rather than filling it.
        /// </summary>
        public void Load()
        {
            // Guarded: LoadFromJson logs an error when the file is missing, which is the
            // normal case on first launch.
            if (this.IsJsonFileExist(Directory, name))
                this.LoadFromJson(Directory, name);

            var table = Value ?? new HighScoreTable();
            table.Normalize();
            Raise(table);
        }
    }
}
