using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CoinSpawner : MonoBehaviour
{
    float coinSpawnTimer = 0;
    [SerializeField] float coinSpawnTime;

    [SerializeField] GameObject coinPrefab;

    [SerializeField] Transform left, right;

    Vector3 leftToRight;

    private void Start()
    {
        coinSpawnTimer = 0;

        leftToRight = right.position - left.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        coinSpawnTimer += Time.deltaTime;
        if (coinSpawnTimer >= coinSpawnTime)
        {
            coinSpawnTimer = 0;

            //spawn coins
            Vector3 targetPosition = left.position + Random.value * leftToRight;
            PhotonNetwork.Instantiate(coinPrefab.name, targetPosition, Quaternion.identity);
        }
    }
}
