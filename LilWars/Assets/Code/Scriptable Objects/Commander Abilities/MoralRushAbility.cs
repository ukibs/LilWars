using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MoralRushAbility", menuName = "ScriptableObjects/MoralRushAbility", order = 1)]
public class MoralRushAbility : BaseCommanderAbility
{
    //

    //
    public override void UseAbility(DataForAbility dataForAbility)
    {
        base.UseAbility(dataForAbility);
        //
        List<TrooperController> affectedTroopers = 
            GetPossibleAffectedTroopers(dataForAbility.levelController, dataForAbility.abilityEpicenter);
        //
        for(int i = 0; i < affectedTroopers.Count; i++)
        {
            //
            //Debug.Log(affectedTroopers[i].gameObject + ", " + affectedTroopers[i].Team + "posibly affected");
            // TODO: Diferenciar equipo
            if (affectedTroopers[i].Team == dataForAbility.userTeam && affectedTroopers[i].enabled)
            {
                //
                //Debug.Log(affectedTroopers[i].gameObject + " recevinf moral buff");
                //
                affectedTroopers[i].ReceiveMoralBuff();
            }
        }
    }
}
