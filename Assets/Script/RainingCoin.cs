using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RainingCoin : MonoBehaviour
{
    [SerializeField] float lifespan;
    [SerializeField] float startFadeTime;
    float aliveTimer;

    SpriteRenderer sr;

    bool destroyed;

    // Start is called before the first frame update
    void Start()
    {
        aliveTimer = 0;

        sr = GetComponent<SpriteRenderer>();

        destroyed = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        aliveTimer += Time.deltaTime;

        if (aliveTimer >= startFadeTime)
        {
            float percentageFade = 1 - (aliveTimer - startFadeTime) / (lifespan - startFadeTime);

            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, percentageFade);
        }

        if (aliveTimer >= lifespan && !destroyed)
        {
            PhotonNetwork.Destroy(gameObject);
            destroyed = true;
        }
    }
}
