using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [System.Serializable]
    public class Stage
    {
        public GameObject StagePrefab;
        private int bestMoves;

        public Vector2 GetInitialPlayerPosition()
        {
            Transform ippTransform = StagePrefab.transform.Find("InitialPlayerPosition");
            Vector2 ipp = new Vector2(ippTransform.position.x, ippTransform.position.z);
            return ipp;
        }
    }
}
