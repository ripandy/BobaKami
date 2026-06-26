using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Doinject;
using BobaKami;
using BobaKami.GameStates;
using Soar.Commands;
using UnityEngine;

namespace BobaKami.Gameplay
{
    public class GameplayStateMachine : MonoBehaviour, IInjectableComponent
    {
        [SerializeField] private Command resetAppCommand;
        [SerializeField] private GameStateEnum initialState = GameStateEnum.Intro;
        
        private IReadOnlyDictionary<GameStateEnum, IGameState> gameStates;
        
        [Inject]
        public void Construct(IntroGameState introGameState, PlayGameState playGameState, GameOverGameState gameOverGameState)
        {
            gameStates = new Dictionary<GameStateEnum, IGameState>
            {
                { introGameState.Id, introGameState },
                { playGameState.Id, playGameState },
                { gameOverGameState.Id, gameOverGameState }
            };
        }
        
        private void Start() => Run().Forget();

        private async UniTaskVoid Run()
        {
            var activeState = initialState;
            while (activeState != GameStateEnum.None && !destroyCancellationToken.IsCancellationRequested)
            {
                Debug.Log($"Running {activeState}");
                activeState = await gameStates[activeState].Running(destroyCancellationToken);
            }
            
            await resetAppCommand.ExecuteAsync();
        }
    }
}