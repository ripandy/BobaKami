using BobaKami.Interfaces;
using Soar.Variables;

namespace BobaKami.Gameplay
{
    public class PlayerDirectionVariable : Variable<DirectionEnum>, IPlayerDirectionPresenter
    {
        public void Show(DirectionEnum direction)
        {
            Value = direction;
        }
    }
}