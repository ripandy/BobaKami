using System.Threading;
using System.Threading.Tasks;

namespace BobaKami.Interfaces
{
    public interface IIntroPresenter
    {
        ValueTask Show(CancellationToken cancellationToken = default);
    }
}