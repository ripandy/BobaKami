using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BobaKami.Interfaces;

namespace BobaKami.Tests
{
    /// <summary>
    /// Deterministic input double. Tests push directions/bites explicitly; waits honor
    /// cancellation by throwing OperationCanceledException, mirroring the SOAR port contract
    /// (GameEvent.EventAsync). CancelPendingWaits simulates Application.exitCancellationToken
    /// firing while the game state's own token is still alive.
    /// </summary>
    internal class ScriptedInputProvider : IPlayerDirectionInputProvider, IPlayerBiteInputProvider
    {
        private readonly AsyncValueSource<DirectionEnum> directions = new();
        private readonly AsyncValueSource<int> bites = new();

        public void PushDirection(DirectionEnum direction) => directions.Push(direction);
        public void PushBite(int bobaId) => bites.Push(bobaId);

        public void CancelPendingWaits()
        {
            directions.CancelWaiter();
            bites.CancelWaiter();
        }

        public ValueTask<DirectionEnum> WaitForDirectionInput(CancellationToken cancellationToken = default)
        {
            return directions.WaitAsync(cancellationToken);
        }

        public ValueTask<int> WaitForBite(CancellationToken cancellationToken = default)
        {
            return bites.WaitAsync(cancellationToken);
        }

        private class AsyncValueSource<T>
        {
            private readonly Queue<T> pending = new();
            private TaskCompletionSource<T> waiter;

            public void Push(T value)
            {
                var current = waiter;
                if (current != null)
                {
                    waiter = null;
                    current.TrySetResult(value);
                }
                else
                {
                    pending.Enqueue(value);
                }
            }

            public void CancelWaiter()
            {
                var current = waiter;
                waiter = null;
                current?.TrySetCanceled();
            }

            public async ValueTask<T> WaitAsync(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (pending.Count > 0) return pending.Dequeue();

                var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                waiter = tcs;
                using var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
                return await tcs.Task;
            }
        }
    }
}
