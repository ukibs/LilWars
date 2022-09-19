using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BombardmentAbility", menuName = "ScriptableObjects/BombardmentAbility", order = 1)]
public class BombardmentAbility : BaseCommanderAbility
{
    //
    public int strength;
    public int numberOfBombs;
    //public GameObject bombPrefab;
    //
    public override void UseAbility(DataForAbility dataForAbility)
    {
        base.UseAbility(dataForAbility);
        //
        //StartCoroutine(BombBarrage);
        //
        CommanderAbilitiesManager commanderAbilitiesManager = dataForAbility.levelController.GetComponent<CommanderAbilitiesManager>();
        commanderAbilitiesManager.StartArtilleryBarrage(numberOfBombs, dataForAbility.abilityEpicenter, range, strength);
    }

    
}
