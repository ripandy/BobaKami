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
        [Tooltip("When true, the input vector's x directly selects Left/Forward/Right by threshold (absolute).\nWhen false, x nudges the direction incrementally. Pointer/keyboard/gamepad input requires absolute: centered positions contribute 0 in incremental mode, so Forward would be unreachable. Incremental suits relative face-tracking input on non-iOS.")]
        [SerializeField] private bool absoluteDirection = true;

        private DirectionEnum currentDirection;

        private bool UseAbsolute
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                return true;
#else
                return absoluteDirection;
#endif
            }
        }

        public async ValueTask<DirectionEnum> WaitForDirectionInput(CancellationToken cancellationToken = default)
        {
            // Sample the held value first: position controls emit nothing while at rest, so a
            // transition that happened while no one was awaiting (e.g. a fast trackpad swipe
            // crossing the center band) would otherwise never be observed.
            if (UseAbsolute)
            {
                var sampled = ConvertToDirectionEnum(Value);
                if (sampled != currentDirection)
                {
                    currentDirection = sampled;
                    await UniTask.Yield();
                    return currentDirection;
                }
            }

            currentDirection = await AsObservable().Select(ConvertToDirectionEnum)
                .FirstOrDefaultAsync(direction => direction != currentDirection, cancellationToken: cancellationToken);
            await UniTask.Yield();
            return currentDirection;
        }

        private DirectionEnum ConvertToDirectionEnum(Vector2 directionVector)
        {
            if (UseAbsolute)
            {
                return directionVector.x >= faceLookThreshold ? DirectionEnum.Right :
                    directionVector.x <= -faceLookThreshold ? DirectionEnum.Left :
                    DirectionEnum.Forward;
            }

            var current = (int)currentDirection;
            current += directionVector.x >= faceLookThreshold ? 1 :
                directionVector.x <= -faceLookThreshold ? -1 :
                0;
            current = Mathf.Clamp(current, -1, 1);
            return (DirectionEnum)current;
        }
    }
}
