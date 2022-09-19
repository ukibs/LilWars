using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
    //
    public GameObject explosionPrefab;
    public GameObject afterburnPrefab;

    //
    private LevelController levelController;
    private int damageInEpicenter;

    //
    private void Start()
    {
        levelController = FindObjectOfType<LevelController>();
    }

    //
    private void OnCollisionEnter(Collision collision)
    {
        Explode(collision.contacts[0].point);
    }

    //
    public void Explode(Vector3 explosionPoint)
    {
        //
        Debug.Log("Creating explosion");
        //
        CreateExplosion();
        //
        GameObject explosionAfterBurn = Instantiate(afterburnPrefab,
            explosionPoint + (Vector3.up * 0.01f), afterburnPrefab.transform.rotation);
        explosionAfterBurn.transform.position = new Vector3(explosionAfterBurn.transform.position.x,
            0.01f, explosionAfterBurn.transform.position.z);
        //
        Destroy(gameObject);
    }

    //
    public void InitiateBomb(int damage)
    {
        damageInEpicenter = damage;
    }

    //
    void CreateExplosion()
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //if (Physics.Raycast(ray, out RaycastHit hitInfo))
        //{
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            // Primero a ver que edificios están cerca
            for (int i = 0; i < levelController.LevelBuildings.Length; i++)
            {
                // Por lo pronto vamos a pillar los edificios que estén a 50 metros o menos
                Vector3 buildingDistance = transform.position - levelController.LevelBuildings[i].transform.position;
                if (buildingDistance.sqrMagnitude < Mathf.Pow(50, 2))
                {
                    AffectTroopsWithExplosion(levelController.LevelBuildings[i].CurrentBlueTroopers, transform.position);
                    AffectTroopsWithExplosion(levelController.LevelBuildings[i].CurrentRedTroopers, transform.position);
                }
            }
            // Y luego los de la zona abierta
            AffectTroopsWithExplosion(levelController.currentBlueTroopers, transform.position);
            AffectTroopsWithExplosion(levelController.currentRedTroopers, transform.position);
       // }
       
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="troopersToCheck"></param>
    /// <param name="epicenter"></param>
    void AffectTroopsWithExplosion(List<TrooperController> troopersToCheck, Vector3 epicenter)
    {
        //
        //float damageInEpicenter = 20;
        //
        for (int i = 0; i < troopersToCheck.Count; i++)
        {
            //
            if (troopersToCheck[i] == null)
                continue;
            //
            Vector3 distanceToEpicenter = epicenter - troopersToCheck[i].transform.position;
            float pureDistanceToEpicenter = distanceToEpicenter.magnitude;

            // Multiplicamos por dos imaginando que la fuerza se reparte en una semiesfera 
            //int damageToApply = (int)(forceInEpicenter * 2 / distanceToEpicenter.sqrMagnitude );

            // Vamos a no ponernos tan realistas
            //int damageToApply = (int)(damageInEpicenter / pureDistanceToEpicenter);
            int damageToApply = (int)(damageInEpicenter - (pureDistanceToEpicenter * 1));
            //
            if (damageToApply > 0)
            {
                //Debug.Log("Distance " + pureDistanceToEpicenter + ", sending " + damageToApply + " explosion damage");
                troopersToCheck[i].ReceiveAttack(damageToApply);
            }
        }
    }
}
