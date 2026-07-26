using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BobaKami.Interfaces;

namespace BobaKami.Tests
{
    /// <summary>
    /// Deterministic boba presenter double. With AutoDrop, every shown boba "drops" immediately
    /// (damage path). Otherwise bobas stay in flight until the test calls DropBoba(id) or the
    /// game bites them (Hide resolves the boba as eaten). WaitForShown lets tests synchronize
    /// on a specific boba being launched. Cancellation propagates as OperationCanceledException.
    /// </summary>
    internal class ScriptedBobaPresenter : IBobaPresenter
    {
        private readonly Dictionary<int, TaskCompletionSource<bool>> inFlight = new();
        private readonly Dictionary<int, TaskCompletionSource<bool>> shownSignals = new();
        private readonly List<int> hiddenBobas = new();

        public bool AutoDrop { get; set; }
        public IReadOnlyList<int> HiddenBobas => hiddenBobas;

        public async ValueTask<bool> Show(int id, DirectionEnum throwDirection, CancellationToken cancellationToken = default)
        {
            GetSignal(shownSignals, id).TrySetResult(true);

            if (AutoDrop)
            {
                await Task.Yield();
                return true;
            }

            var tcs = GetSignal(inFlight, id);
            using var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            return await tcs.Task;
        }

        public void Hide(int id)
        {
            hiddenBobas.Add(id);
            GetSignal(inFlight, id).TrySetResult(false); // eaten/hidden -> not dropped
        }

        public void DropBoba(int id)
        {
            GetSignal(inFlight, id).TrySetResult(true);
        }

        public Task WaitForShown(int id)
        {
            return GetSignal(shownSignals, id).Task;
        }

        private static TaskCompletionSource<bool> GetSignal(Dictionary<int, TaskCompletionSource<bool>> map, int id)
        {
            if (map.TryGetValue(id, out var tcs)) return tcs;
            tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            map.Add(id, tcs);
            return tcs;
        }
    }
}
