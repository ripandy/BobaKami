using System.Threading;
using System.Threading.Tasks;
using BobaKami.Interfaces;

namespace BobaKami
{
    public struct Bean
    {
        internal static int id;
        
        public int Id { get; }
        public DirectionEnum ThrowDirection { get; }
        
        public Bean(DirectionEnum throwDirection)
        {
            Id = id++;
            ThrowDirection = throwDirection;
        }
    }

    public interface IBeanPresenter
    {
        ValueTask<bool> Show(int id, DirectionEnum throwDirection, CancellationToken cancellationToken = default);
        void Hide(int id);
    }
}