using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float spawnRate = 2f;
    public float heightOffset = 2f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnObstacle), 1f, spawnRate);
    }

    void SpawnObstacle()
    {
        float y = Random.Range(-heightOffset, heightOffset);
        Instantiate(obstaclePrefab, new Vector3(10f, y, 0), Quaternion.identity);
    }
}
