using System.Threading;
using System.Threading.Tasks;
using BobaKami.Interfaces;

namespace BobaKami.GameStates
{
    public class IntroGameState : IGameState
    {
        private readonly Player player;
        private readonly BeanLauncher beanLauncher;
        private readonly IPlayerHealthPresenter playerHealthPresenter;
        private readonly IPlayerStatsPresenter playerStatsPresenter;
        private readonly IIntroPresenter introPresenter;
        
        public GameStateEnum Id => GameStateEnum.Intro;
        
        public IntroGameState(
            Player player,
            BeanLauncher beanLauncher,
            IPlayerHealthPresenter playerHealthPresenter,
            IPlayerStatsPresenter playerStatsPresenter,
            IIntroPresenter introPresenter)
        {
            this.player = player;
            this.beanLauncher = beanLauncher;
            this.playerHealthPresenter = playerHealthPresenter;
            this.playerStatsPresenter = playerStatsPresenter;
            this.introPresenter = introPresenter;
        }
        
        public async ValueTask<GameStateEnum> Running(CancellationToken cancellationToken = default)
        {
            var showIntroTask = introPresenter.Show(cancellationToken);
            
            player.Initialize();
            beanLauncher.Initialize();
            
            playerHealthPresenter.Show(player.HealthPercentage);
            playerStatsPresenter.Show(player.GameStats);
            
            await showIntroTask;
            return GameStateEnum.GamePlay;
        }
    }

    
}