using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using BobaKami.Interfaces;
using R3;
using Soar.Variables;
using UnityEngine;

namespace BobaKami.Gameplay
{
    [CreateAssetMenu(fileName = "FaceDirectionConverterVectorVariable", menuName = "BobaKami/FaceDirectionConverterVectorVariable")]
    public class FaceDirectionConverterVectorVariable : Variable<Vector2>, IPlayerDirectionInputProvider
    {
        [SerializeField] private float faceLookThreshold = 0.3f;
        
        private DirectionEnum currentDirection;
        
        public async ValueTask<DirectionEnum> WaitForDirectionInput(CancellationToken cancellationToken = default)
        {
            currentDirection = await AsObservable().Select(ConvertToDirectionEnum)
                .FirstOrDefaultAsync(direction => direction != currentDirection, cancellationToken: cancellationToken);
            await UniTask.Yield();
            return currentDirection;
        }
        
        private DirectionEnum ConvertToDirectionEnum(Vector2 directionVector)
        {
#if UNITY_IOS && !UNITY_EDITOR
            return directionVector.x >= faceLookThreshold ? DirectionEnum.Right :
                directionVector.x <= -faceLookThreshold ? DirectionEnum.Left :
                DirectionEnum.Forward;
#else
            var current = (int)currentDirection;
            current += directionVector.x >= faceLookThreshold ? 1 :
                directionVector.x <= -faceLookThreshold ? -1 :
                0;
            current = Mathf.Clamp(current, -1, 1);
            return (DirectionEnum)current;
#endif
        }
    }
}