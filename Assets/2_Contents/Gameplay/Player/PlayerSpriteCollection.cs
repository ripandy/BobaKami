using System;
using Soar.Collections;
using UnityEngine;

namespace BobaKami.Gameplay
{
    public class PlayerSpriteCollection : SoarDictionary<PlayerSpriteEnum, Sprite>
    {
    }
    
    [Flags]
    public enum PlayerSpriteEnum
    {
        None = 0,
        Idle = 1 << 0,
        Blink = 1 << 1,
        MouthClose = 1 << 2,
        MouthOpen = 1 << 3,
        Left = 1 << 4,
        Right = 1 << 5,
        Bite = 1 << 6,
        IdleMouthClosed = Idle | MouthClose,
        BlinkMouthClosed = Blink | MouthClose,
        IdleMouthOpen = Idle | MouthOpen,
        BlinkMouthOpen = Blink | MouthOpen,
        IdleMouthClosedLeft = IdleMouthClosed | Left,
        BlinkMouthClosedLeft = BlinkMouthClosed | Left,
        IdleMouthOpenLeft = IdleMouthOpen | Left,
        BlinkMouthOpenLeft = BlinkMouthOpen | Left,
        IdleMouthClosedRight = IdleMouthClosed | Right,
        BlinkMouthClosedRight = BlinkMouthClosed | Right,
        IdleMouthOpenRight = IdleMouthOpen | Right,
        BlinkMouthOpenRight = BlinkMouthOpen | Right,
        BiteLeft = Bite | Left,
        BiteRight = Bite | Right,
    }
}