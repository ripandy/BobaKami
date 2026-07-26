using BobaKami.Gameplay.HUD;
using Doinject;
using BobaKami.GameStates;
using BobaKami.Interfaces;
using UnityEngine;

namespace BobaKami.Gameplay
{
    public class GameplayBindingInstaller : MonoBehaviour, IBindingInstaller
    {
        [SerializeField] private PlayerData playerData;
        [SerializeField] private BobaLauncherData bobaLauncherData;

        [SerializeField] private HealthPercentageVariable healthPercentageVariable;
        [SerializeField] private GameStatsVariable gameStatsVariable;
        
        [SerializeField] private BobaPresenter bobaPresenter;
        [SerializeField] private IntroPresenter introPresenter;
        [SerializeField] private GameOverPresenter gameOverPresenter;

        [SerializeField] private PlayerDirectionVariable playerDirectionVariable;
        [SerializeField] private FaceDirectionConverterVectorVariable faceDirectionConverterVectorVariable;
        [SerializeField] private BittenBobaGameEvent bittenBobaGameEvent;

        public void Install(DIContainer container, IContextArg contextArg)
        {
            // Domain
            container.BindFromInstance(playerData.Value);
            container.BindFromInstance(bobaLauncherData.Value);
            container.BindSingleton<IntroGameState>();
            container.BindSingleton<PlayGameState>();
            container.BindSingleton<GameOverGameState>();

            // Presenters
            container.BindFromInstance<IPlayerHealthPresenter>(healthPercentageVariable);
            container.BindFromInstance<IPlayerStatsPresenter>(gameStatsVariable);
            container.BindFromInstance<IPlayerDirectionPresenter>(playerDirectionVariable);
            container.BindFromInstance<IBobaPresenter>(bobaPresenter);
            container.BindFromInstance<IIntroPresenter>(introPresenter);
            container.BindFromInstance<IGameOverPresenter>(gameOverPresenter);

            // Input Providers
            container.BindFromInstance<IPlayerDirectionInputProvider>(faceDirectionConverterVectorVariable);
            container.BindFromInstance<IPlayerBiteInputProvider>(bittenBobaGameEvent);
        }
    }
}