using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private float spawnTime;

    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            Debug.Log("Waiting to spawn enemy");
            SpawnEnemy();
        }
        else
        {
            Debug.Log("Client, not spawning enemy");
        }
    }

    private IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(spawnTime);

        GameObject enemy =  Instantiate(enemyPrefab, enemyParent);
        enemy.transform.position = transform.position;
    }
}
