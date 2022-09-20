using SRML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpawnerUpgrade
{
    internal class SlimeSpawnerRunner : MonoBehaviour
    {
        internal GameObject slimeNode;
        internal DirectedActorSpawner.SpawnConstraint[] constraints;
        internal CellDirector cellDir;

        public void Awake()
        {
            cellDir = GetComponentInParent<CellDirector>();
            #region SPAWNER
            constraints = new DirectedActorSpawner.SpawnConstraint[]
            {
                new DirectedActorSpawner.SpawnConstraint()
                {
                    window = new DirectedActorSpawner.TimeWindow()
                    {
                        timeMode = DirectedActorSpawner.TimeMode.ANY
                    },
                    slimeset = new SlimeSet()
                    {
                        members = Main.slimeMembers
                    },
                    weight = 0.5f
                }
            };

            slimeNode = GameObjectUtils.InstantiateInactive(GameObject.Find("zoneQUARRY/cellQuarry_Entrance/Sector/Slimes/nodeSlime"));
            slimeNode.transform.parent = base.transform;
            slimeNode.transform.position = base.transform.position;
            slimeNode.transform.rotation = Quaternion.identity;
            slimeNode.RemoveComponent<DirectedSlimeSpawner>();
            DirectedSlimeSpawner nodeSpawner = slimeNode.AddComponent<DirectedSlimeSpawner>();
            nodeSpawner.constraints = constraints;
            nodeSpawner.allowDirectedSpawns = true;
            nodeSpawner.enableToteming = true;
            nodeSpawner.radius = 1f;
            nodeSpawner.slimeSpawnFX = GameObject.Find("zoneQUARRY/cellQuarry_Entrance/Sector/Slimes/nodeSlime").GetComponent<DirectedSlimeSpawner>().slimeSpawnFX;
            nodeSpawner.spawnFX = GameObject.Find("zoneQUARRY/cellQuarry_Entrance/Sector/Slimes/nodeSlime").GetComponent<DirectedSlimeSpawner>().spawnFX;
            #endregion
            slimeNode.SetActive(true);
        }

        public void OnDestroy()
        {
            cellDir.spawners.Remove(slimeNode.GetComponent<DirectedSlimeSpawner>());
        }
    }
}
