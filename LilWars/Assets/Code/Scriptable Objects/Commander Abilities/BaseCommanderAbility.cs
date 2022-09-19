using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CommanderAbility", menuName = "ScriptableObjects/CommanderAbility", order = 1)]
public class BaseCommanderAbility : ScriptableObject
{
    public Texture abilityIcon;
    public float cost;
    public float cooldown;
    public float range;
    public AudioClip clip;

    //private LevelController levelController;

    public virtual void UseAbility(DataForAbility dataForAbility)
    {
        //Debug.Log("Using ability: " + this.name);
        dataForAbility.levelController.PlayAbilityClip(clip);
    }

    protected List<TrooperController> GetPossibleAffectedTroopers(LevelController levelController, Vector3 abilityEpicenter)
    {
        //
        List<TrooperController> affectedTroopers = new List<TrooperController>();
        // Pasamos por las bases
        Vector3 buildingDistance;
        Vector3 trooperDistance;
        for (int i = 0; i < levelController.LevelBuildings.Length; i++)
        {
            //
            buildingDistance = levelController.LevelBuildings[i].transform.position - abilityEpicenter;
            if (buildingDistance.magnitude < range + 50)
            {
                // 
                for (int j = 0; j < levelController.LevelBuildings[i].CurrentBlueTroopers.Count; j++)
                {
                    trooperDistance = levelController.LevelBuildings[i].CurrentBlueTroopers[j].transform.position - abilityEpicenter;
                    if (trooperDistance.sqrMagnitude < range * range)
                        affectedTroopers.Add(levelController.LevelBuildings[i].CurrentBlueTroopers[j]);

                }
                for (int j = 0; j < levelController.LevelBuildings[i].CurrentRedTroopers.Count; j++)
                {
                    trooperDistance = levelController.LevelBuildings[i].CurrentRedTroopers[j].transform.position - abilityEpicenter;
                    if (trooperDistance.sqrMagnitude < range * range)
                        affectedTroopers.Add(levelController.LevelBuildings[i].CurrentRedTroopers[j]);
                }
            }
        }

        // Y por la zona abierta
        for (int i = 0; i < levelController.currentBlueTroopers.Count; i++)
        {
            trooperDistance = levelController.currentBlueTroopers[i].transform.position - abilityEpicenter;
            if (trooperDistance.sqrMagnitude < range * range)
                affectedTroopers.Add(levelController.currentBlueTroopers[i]);
        }

        for (int i = 0; i < levelController.currentRedTroopers.Count; i++)
        {
            trooperDistance = levelController.currentRedTroopers[i].transform.position - abilityEpicenter;
            if (trooperDistance.sqrMagnitude < range * range)
                affectedTroopers.Add(levelController.currentRedTroopers[i]);
        }
        //
        Debug.Log("Affected troppers " + affectedTroopers.Count);
        return affectedTroopers;
    }
}

public class DataForAbility
{

    public LevelController levelController;
    public Vector3 abilityEpicenter;
    public Owner userTeam;
    public Building objectiveBuilding;

    public DataForAbility(LevelController levelController, Vector3 abilityEpicenter, Owner userTeam, Building objectiveBuilding)
    {
        this.levelController = levelController;
        this.abilityEpicenter = abilityEpicenter;
        this.userTeam = userTeam;
        this.objectiveBuilding = objectiveBuilding;
    }
}