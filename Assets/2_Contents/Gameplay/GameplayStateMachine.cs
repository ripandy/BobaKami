using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Doinject;
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
            // Captured once: destroyCancellationToken's getter throws MissingReferenceException
            // once the MonoBehaviour is destroyed, and this loop resumes from an await *after*
            // teardown on PlayMode stop. CancellationToken is a struct, so the copy stays valid.
            var cancellationToken = destroyCancellationToken;

            var activeState = initialState;
            while (activeState != GameStateEnum.None && !cancellationToken.IsCancellationRequested)
            {
                activeState = await gameStates[activeState].Running(cancellationToken);
            }

            // Don't reload the root scene on the way out of PlayMode / a destroyed machine —
            // only when a state genuinely returned None.
            if (cancellationToken.IsCancellationRequested) return;

            // Deliberately no token, despite the overload: Command.ExecuteAsync only uses it for
            // one ThrowIfCancellationRequested before a synchronous Execute(), so passing ours
            // would just turn the guard above into an OCE thrown out of this async UniTaskVoid
            // (i.e. unobserved-exception spam). It already links Application.exitCancellationToken
            // internally, and this call loads Core in Single mode — destroying this very
            // component — so destroyCancellationToken is the wrong lifetime to govern it.
            await resetAppCommand.ExecuteAsync();
        }
    }
}