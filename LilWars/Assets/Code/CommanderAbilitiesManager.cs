using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommanderAbilitiesManager : MonoBehaviour
{
    //
    public GameObject explosionPrefab;
    public GameObject bombPrefab;

    private LevelController levelController;

    // Start is called before the first frame update
    void Start()
    {
        levelController = FindObjectOfType<LevelController>();
    }

    // Update is called once per frame
    void Update()
    {
        //
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //
            //CreateExplosion();
            
        }
    }

    //
    public void StartArtilleryBarrage(int numberOfBombs, Vector3 abilityEpicenter, float barrageRadius, int damageInEpicenter)
    {
        //
        Debug.Log("Starting artillery barrage");
        //
        StartCoroutine(BombBarrage(numberOfBombs, abilityEpicenter, barrageRadius, damageInEpicenter));
    }

    //
    IEnumerator BombBarrage(int numberOfBombs, Vector3 abilityEpicenter, float barrageRadius, int damageInEpicenter)
    {
        for (int i = 0; i < numberOfBombs; i++)
        {
            //
            //Debug.Log("Launching " + i + "bomb");
            //
            float angle = Random.Range(0, 360);
            float distance = Random.Range(0, barrageRadius);
            //
            float xCoords = Mathf.Cos(angle) * distance;
            float zCoords = Mathf.Sin(angle) * distance;
            //
            Vector3 nextBombSpawnPosition = abilityEpicenter + new Vector3(xCoords, 100, zCoords);
            //
            GameObject newBomb = Instantiate(bombPrefab, nextBombSpawnPosition, Quaternion.identity);
            BombController bombController = newBomb.GetComponent<BombController>();
            bombController.InitiateBomb(damageInEpicenter);
            //
            yield return new WaitForSeconds(0.3f);
        }
    }
}

