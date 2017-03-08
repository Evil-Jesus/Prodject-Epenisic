using UnityEngine;
using System.Collections;

namespace TileEditor
{
    public class Tile : MonoBehaviour
    {
        #region Public Fields
        [HideInInspector]
        public CollisionType Collision;
        public int pathCost = 0;
        #endregion
    }
}