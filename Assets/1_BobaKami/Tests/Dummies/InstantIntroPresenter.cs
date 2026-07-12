using System.Threading;
using System.Threading.Tasks;
using BobaKami.Interfaces;

namespace BobaKami.Tests
{
    internal class InstantIntroPresenter : IIntroPresenter
    {
        public int ShowCount { get; private set; }

        public ValueTask Show(CancellationToken cancellationToken = default)
        {
            ShowCount++;
            return default;
        }
    }
}
