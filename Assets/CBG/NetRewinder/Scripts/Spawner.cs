using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {
    [SerializeField]
    GameObject prefab;
    [SerializeField]
    int replicationCount;
    [SerializeField]
    Vector3 spacing = Vector3.one;
    Vector3 initialPosition;

	// Use this for initialization
	void Awake () {
        initialPosition = transform.position - new Vector3(((Mathf.Sqrt(replicationCount)-1)*spacing.x)/2, 0, ((Mathf.Sqrt(replicationCount)-1)*spacing.z)/2);
        Vector3 spawnPos = initialPosition;
        int rows = (int)(Mathf.Sqrt(replicationCount)+.999f);
        for (int spawned = 0; spawned < replicationCount; spawned++) {
            if (spawned > 0 && spawned % rows == 0) {
                spawnPos.x = initialPosition.x;
                spawnPos.z += spacing.z;
            }
            GameObject.Instantiate(prefab, spawnPos, Quaternion.identity);
            spawnPos.x += spacing.x;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}


