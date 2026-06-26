using BobaKami.Interfaces;
using Soar.Variables;

namespace BobaKami.Gameplay.HUD
{
    public class HealthPercentageVariable : Variable<float>, IPlayerHealthPresenter
    {
        public void Show(float healthPercentage)
        {
            Value = healthPercentage;
        }
    }
}