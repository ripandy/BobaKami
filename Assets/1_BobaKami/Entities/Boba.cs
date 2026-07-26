using System.Threading;
using System.Threading.Tasks;
using BobaKami.Interfaces;

namespace BobaKami
{
    public struct Boba
    {
        public int Id { get; }
        public DirectionEnum ThrowDirection { get; }

        public Boba(int id, DirectionEnum throwDirection)
        {
            Id = id;
            ThrowDirection = throwDirection;
        }
    }

    public interface IBobaPresenter
    {
        ValueTask<bool> Show(int id, DirectionEnum throwDirection, CancellationToken cancellationToken = default);
        void Hide(int id);
    }
}