﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace App.Player
{
    public class SpawnPhysicsObject : NetworkBehaviour
    {
        /// <summary>
        /// Singleton
        /// </summary>
        public static SpawnPhysicsObject Instance;

        /// <summary>
        /// PhysicsObject
        /// </summary>
        [SerializeField]
        private GameObject _PhysicsObject;

        /// <summary>
        /// Spawn Points where physic objects spawn
        /// </summary>
        [SerializeField]
        private List<GameObject> _SpawnPoints = new List<GameObject>();

        /// <summary>
        /// List of objects spawned
        /// </summary>
        private List<GameObject> _SpawnObjectList = new List<GameObject>();

        /// <summary>
        /// Is spawning the physical objects
        /// </summary>
        public bool IsSpawning = false;

        /// <summary>
        /// Inbetween time
        /// </summary>
        public float InbetweenTime = 1.0f;

        /// <summary>
        /// Awake this instance
        /// </summary>
        private void Awake()
        {
            if (SpawnPhysicsObject.Instance == null)
            {
                SpawnPhysicsObject.Instance = this;
                DontDestroyOnLoad(this);
            }
        }

        /// <summary>
        /// Remove existing spawned Objects, and spawn new ones
        /// </summary>
        public void SpawnObjects()
        {
            if (this.IsSpawning)
                return;
            this.IsSpawning = true;
            StartCoroutine(SpawningObject());
        }

        /// <summary>
        /// Spawning Object
        /// </summary>
        /// <returns></returns>
        private IEnumerator SpawningObject()
        {
            for (int i = 0; i < this._SpawnObjectList.Count; i++)
            {
                if (this._SpawnObjectList[i] == null)
                    continue;
                NetworkServer.Destroy(this._SpawnObjectList[i]);
            }
            this._SpawnObjectList = new List<GameObject>();
            //Spawn physics objects
            for (int i = 0; i < this._SpawnPoints.Count; i++)
            {
                GameObject obj = (GameObject)Instantiate(this._PhysicsObject);
                obj.transform.position = this._SpawnPoints[i].transform.position;
                if (i > 2)
                    obj.GetComponent<SphereCollider>().material = null;
                HUDManager.Instance.RpcSpawnMessage();
                NetworkServer.Spawn(obj);
                this._SpawnObjectList.Add(obj);
                yield return new WaitForSeconds(this.InbetweenTime);
            }
            this.IsSpawning = false;
        }
    }
}

