using Soar.Collections;
using UnityEngine;

namespace BobaKami.Gameplay
{
    [CreateAssetMenu(fileName = "BobaCollection", menuName = "BobaKami/BobaCollection")]
    public class BobaCollection : SoarDictionary<int, GameObject>
    {
    }
}