using System.Threading;
using System.Threading.Tasks;
using BobaKami.Interfaces;
using Soar.Events;

namespace BobaKami.Gameplay
{
    public class BittenBobaGameEvent : GameEvent<int>, IPlayerBiteInputProvider
    {
        public ValueTask<int> WaitForBite(CancellationToken cancellationToken = default)
        {
            return EventAsync(cancellationToken);
        }
    }
}