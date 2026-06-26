using System.Threading;
using System.Threading.Tasks;
using BobaKami.DataTransferObjects;
using BobaKami.Interfaces;
using UnityEngine;

namespace BobaKami.Tests
{
    public class DummyGameOverPresenter : IGameOverPresenter
    {
        public async ValueTask<bool> Show(GameStatsDto stats, CancellationToken cancellationToken = default)
        {
            Debug.Log("Game Over!");
            await Task.Delay(1000, cancellationToken);
            Debug.Log($"Beans Eaten: {stats.Score}");
            await Task.Delay(500, cancellationToken);
            Debug.Log($"Last Combo: {stats.Combo}");
            await Task.Delay(500, cancellationToken);
            
            return stats.Combo != 99; // For test purpose, return false (exit game) if combo is 99.
        }
    }
}