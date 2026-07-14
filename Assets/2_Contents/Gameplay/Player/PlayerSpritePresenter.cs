using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using BobaKami.Interfaces;
using R3;
using Soar.Events;
using Soar.Variables;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BobaKami.Gameplay
{
    public class PlayerSpritePresenter : MonoBehaviour
    {
        [SerializeField] private GameEvent<DirectionEnum> playerDirection;
        [SerializeField] private GameEvent<bool> mouthOpenEvent;
        [SerializeField] private GameEvent<bool> isEyeBlinkEvent;
        [SerializeField] private Variable<float> mouthColliderEnableDuration;
        [SerializeField] private PlayerSpriteCollection playerSpriteCollection;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private readonly ReactiveProperty<PlayerSpriteEnum> currentSprite = new();
        private IDisposable subscription;
        private bool isMouthOpen;

        private void Start()
        {
            var s1 = playerDirection.Subscribe(OnPlayerDirection);
            // The bite animation is uninterruptible (Drop), but the logical mouth state is
            // tracked separately so the bite's end-state can respect a press that happened
            // mid-animation (open instead of idle-closed).
            var s2 = mouthOpenEvent.Subscribe(open => isMouthOpen = open);
            var s2Await = mouthOpenEvent.AsObservable().SubscribeAwait(OnMouthOpen, AwaitOperation.Drop);
            
#if UNITY_IOS && !UNITY_EDITOR
            var s3 = isEyeBlinkEvent.Subscribe(OnBlink);
#else
            var s3 = Observable.Interval(TimeSpan.FromSeconds(3))
                .SubscribeAwait(async (_, token) => await TryUpdateAutoBlink(token));
#endif

            var s4 = currentSprite.Subscribe(UpdateSprite);
            
            subscription = new CompositeDisposable(s1, s2, s2Await, s3, s4);
            
            currentSprite.Value = PlayerSpriteEnum.IdleMouthClosed;
        }

        private void OnPlayerDirection(DirectionEnum direction)
        {
            var newSprite = currentSprite.Value & ~(PlayerSpriteEnum.Left | PlayerSpriteEnum.Right);
            newSprite |= direction switch
            {
                DirectionEnum.Left => PlayerSpriteEnum.Left,
                DirectionEnum.Right => PlayerSpriteEnum.Right,
                _ => PlayerSpriteEnum.None
            };
            currentSprite.Value = newSprite;
        }
        
        private async ValueTask OnMouthOpen(bool opened, CancellationToken cancellationToken)
        {
            if (opened)
            {
                currentSprite.Value = ToMouthOpenSprite(currentSprite.Value);
                return;
            }

            currentSprite.Value = (currentSprite.Value &
                                   (PlayerSpriteEnum.Left | PlayerSpriteEnum.Right))
                                  | PlayerSpriteEnum.Bite;

            await UniTask.Delay(TimeSpan.FromSeconds(mouthColliderEnableDuration), cancellationToken: cancellationToken);

            // The bite window played out in full; land on whatever the mouth currently is.
            // A press that arrived mid-bite was dropped by the animation handler, but
            // isMouthOpen remembers it, so the sprite reopens instead of idling closed.
            currentSprite.Value = isMouthOpen
                ? ToMouthOpenSprite(currentSprite.Value)
                : (currentSprite.Value & ~PlayerSpriteEnum.Bite) | PlayerSpriteEnum.IdleMouthClosed;
        }

        private static PlayerSpriteEnum ToMouthOpenSprite(PlayerSpriteEnum current)
        {
            // Clear Bite/MouthClose and guarantee an Idle/Blink base flag so the result
            // always maps to a valid sprite in the collection.
            var newSprite = (current & ~(PlayerSpriteEnum.MouthClose | PlayerSpriteEnum.Bite)) | PlayerSpriteEnum.MouthOpen;
            if (!newSprite.HasFlag(PlayerSpriteEnum.Idle) && !newSprite.HasFlag(PlayerSpriteEnum.Blink))
            {
                newSprite |= PlayerSpriteEnum.Idle;
            }
            return newSprite;
        }

        private async ValueTask TryUpdateAutoBlink(CancellationToken token = default)
        {
            const float blinkChance = 0.4f;
            if (Random.value > blinkChance || currentSprite.Value.HasFlag(PlayerSpriteEnum.Bite)) return;
            
            OnBlink(true);
            
            const float blinkDuration = 0.2f;
            await UniTask.Delay(TimeSpan.FromSeconds(blinkDuration), cancellationToken: token);
            
            OnBlink(false);
        }

        private void OnBlink(bool blink)
        {
            var newSprite = currentSprite.Value;
            newSprite &= blink ? ~PlayerSpriteEnum.Idle : ~PlayerSpriteEnum.Blink;
            newSprite |= blink ? PlayerSpriteEnum.Blink : PlayerSpriteEnum.Idle;
            currentSprite.Value = newSprite;
        }

        private void UpdateSprite(PlayerSpriteEnum newSpriteFlag)
        {
            if (!playerSpriteCollection.TryGetValue(newSpriteFlag, out var sprite)) return;
            spriteRenderer.sprite = sprite;
        }

        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}