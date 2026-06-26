using System;
using BobaKami.Interfaces;
using Soar.Events;
using UnityEngine;

namespace BobaKami.Gameplay
{
    public class PlayerDirectionPresenter : MonoBehaviour
    {
        [SerializeField] private GameEvent<DirectionEnum> playerDirection;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private float lookDistance = 1.5f;

        private IDisposable subscription;
        
        private Vector3 defaultPosition;

        private void Start()
        {
            defaultPosition = playerRoot.localPosition;
            subscription = playerDirection.Subscribe(OnPlayerEvent);
        }

        private void OnPlayerEvent(DirectionEnum direction)
        {
            var position = defaultPosition;
            position.x += direction switch
            {
                DirectionEnum.Forward => 0,
                DirectionEnum.Left => -lookDistance,
                DirectionEnum.Right => lookDistance,
                _ => 0
            };
            playerRoot.localPosition = position;
        }
        
        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}