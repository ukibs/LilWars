using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnTroopsAbility", menuName = "ScriptableObjects/SpawnTroopsAbility", order = 1)]
public class SpawnTroopsAbility : BaseCommanderAbility
{
    //
    public int troopAmount = 5;

    //
    public override void UseAbility(DataForAbility dataForAbility)
    {
        base.UseAbility(dataForAbility);
        // TODO: Spamear aqui a las tropas
        // Con requisito
        if(dataForAbility.objectiveBuilding != null && dataForAbility.objectiveBuilding.CurrentOwner == dataForAbility.userTeam)
        {
            // TODO: Dar puntos de producción en vez de tropas brutas
            //for(int i = 0; i < troopAmount; i++)
            //{
            //    dataForAbility.objectiveBuilding.SpawnTroop();
            //}
            dataForAbility.objectiveBuilding.AddProductionFromAbility(troopAmount);
        }
        else
        {
            Debug.Log("Error, objective building of your team required");
        }
    }
}
