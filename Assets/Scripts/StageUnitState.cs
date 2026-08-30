using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class StageUnitState
    {
        public GameObject normalTile;
        public GameObject weakTile;
        public GameObject switchX;
        public GameObject switchO;
        public GameObject holeTile;
        public GameObject toggleTile;
        public GameObject star;

        public StageUnitState(Transform transform)
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, 5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
            foreach (RaycastHit hit in hits)
            {
                string hitTag = hit.transform.tag;
                switch (hitTag)
                {
                    case "NormalTile":
                        normalTile = hit.transform.gameObject;
                        break;
                    case "WeakTile":
                        weakTile = hit.transform.gameObject;
                        break;
                    case "SwitchX":
                        switchX = hit.transform.gameObject;
                        break;
                    case "SwitchO":
                        switchO = hit.transform.gameObject;
                        break;
                    case "HoleTile":
                        holeTile = hit.transform.gameObject;
                        break;
                    case "ToggleTile":
                        toggleTile = hit.transform.gameObject;
                        break;
                    case "Star":
                        star = hit.transform.gameObject;
                        break;
                    default:
                        //Debug.Log("Raycast hit smth else: " + hit.transform.name);
                        break;
                }
            }
        }
    }
}
