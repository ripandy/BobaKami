using BobaKami;
using Soar.Variables;
using UnityEngine;

namespace BobaKami.Gameplay
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "BobaKami/PlayerData")]
    public class PlayerData : JsonableVariable<Player>
    {
    }
}