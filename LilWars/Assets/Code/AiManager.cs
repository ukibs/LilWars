using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oponent IA, controla a al equipo rojo
/// </summary>
public class AiManager : MonoBehaviour
{
    //
    public enum Attitude
    {
        Invalid = -1,

        Expanding,
        Offensive,
        Defensive,

        Count
    }
    //
    public int minTroopsToAttack = 5;
    public float timeBetweenChecks = 1;
    // TODO: Lo controlaremos en un sitio mas centralizado
    public BaseCommanderAbility[] commanderAbilities;

    //
    private LevelController levelController;
    //private CommanderAbilitiesManager abilitiesManager;
    private Attitude attitude;
    //
    private bool upgradePerformedThisStep = false;

    // Start is called before the first frame update
    void Start()
    {
        //
        levelController = FindObjectOfType<LevelController>();
        //abilitiesManager = FindObjectOfType<CommanderAbilitiesManager>();
        //
        StartCoroutine(CheckAndExecute());
    }

    //
    void CheckOwnedBuildings()
    {
        //
        if (levelController.RedBuildings == null) return;
        // Reseteamos este valor en cada step
        upgradePerformedThisStep = false;
        //
        for (int i = 0; i < levelController.RedBuildings.Count; i++)
        {
            //
            CheckForBuildingUpdates(levelController.RedBuildings[i]);
            //
            if(levelController.RedBuildings[i].CurrentRedTroopers.Count > minTroopsToAttack &&
                levelController.RedBuildings[i].CurrentBlueTroopers.Count == 0)
            {
                DecideBestObjective(levelController.RedBuildings[i]);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckAbilities()
    {
        // Aprovecharemos que usa una lista diferente a la del player para jugar con las prioridades
        for(int i = 0; i < commanderAbilities.Length; i++)
        {
            //
            if(levelController.AiCommandPoints > commanderAbilities[i].cost)
            {
                CheckForSuitableAbilityObjecive(commanderAbilities[i]);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="abilityToCheck"></param>
    void CheckForSuitableAbilityObjecive(BaseCommanderAbility abilityToCheck)
    {
        //
        //BombardmentAbility bombardmentAbility = (BombardmentAbility)abilityToCheck;
        //MoralRushAbility moralRushAbility = (MoralRushAbility)abilityToCheck;
        //
        Building bestObjectiveBuilding = null;
        // Si la habilidad es de tipo bombardeo...
        if (abilityToCheck.GetType() == typeof(BombardmentAbility))
        {
            // Cogemos el edificio con mas azules que tenga 10 o mas
            for (int i = 0; i < levelController.LevelBuildings.Length; i++)
            {
                if(levelController.LevelBuildings[i].CurrentBlueTroopers.Count >= 10 && (bestObjectiveBuilding == null
                    || levelController.LevelBuildings[i].CurrentBlueTroopers.Count > bestObjectiveBuilding.CurrentBlueTroopers.Count))
                {
                    bestObjectiveBuilding = levelController.LevelBuildings[i];
                }
            }
        }
        // Si la habilidad es de tipo moral rush...
        else if (abilityToCheck.GetType() == typeof(MoralRushAbility))
        {
            // Cogemos el edificio con mas rojos que tenga 10 o mas
            for (int i = 0; i < levelController.LevelBuildings.Length; i++)
            {
                if (levelController.LevelBuildings[i].CurrentRedTroopers.Count > 10 && (bestObjectiveBuilding == null
                    || levelController.LevelBuildings[i].CurrentRedTroopers.Count > bestObjectiveBuilding.CurrentRedTroopers.Count))
                {
                    bestObjectiveBuilding = levelController.LevelBuildings[i];
                }
            }
        }
        // Si la habilidad es de tipo spawn troops...
        else if (abilityToCheck.GetType() == typeof(SpawnTroopsAbility))
        {
            // Si tiene mas de la mitad que pase para usar otras habilidades
            if (levelController.GetTotalActiveTroopersOfTeam(Owner.Red) > levelController.MaxTroopersForRedTeam / 2)
                return;
            // Cogemos un edificio rojo que pueda porducir tropas
            for (int i = 0; i < levelController.LevelBuildings.Length; i++)
            {
                //
                float strongestTrooperCost = 0;
                if (levelController.LevelBuildings[i].CurrentOwner == Owner.Red && levelController.LevelBuildings[i].producedTrooper != null &&
                    levelController.LevelBuildings[i].producedTrooper.cost > strongestTrooperCost)
                {
                    bestObjectiveBuilding = levelController.LevelBuildings[i];
                    // Creo que no uso esto de momento proque aun esta muy rota
                    strongestTrooperCost = levelController.LevelBuildings[i].producedTrooper.cost;
                }
            }
        }
        else
        {
            Debug.Log("Algo estamos chequeando mal");
        }

        //
        if(bestObjectiveBuilding != null)
        {
            levelController.AiCommandPoints -= abilityToCheck.cost;
            DataForAbility dataForAbility = new DataForAbility(levelController, bestObjectiveBuilding.transform.position, Owner.Red, bestObjectiveBuilding);
            abilityToCheck.UseAbility(dataForAbility);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="attackerBuilding"></param>
    void DecideBestObjective(Building attackerBuilding)
    {
        //
        Building objectiveBuilding = null;
        //List<Vector3> bluePositions = new List<Vector3>();
        // Primero los grises, la presa facil
        if(levelController.GreyBuildings.Count > 0)
        {
            // Cogemos la mas cercana
            float closestDistance = Mathf.Infinity;
            Building nearestBuilding = null;
            for(int i = 0; i < levelController.GreyBuildings.Count; i++)
            {
                //
                Vector3 distance = attackerBuilding.transform.position - levelController.GreyBuildings[i].transform.position;
                //
                if((nearestBuilding == null || distance.magnitude < closestDistance) && 
                    levelController.CanSendThere(attackerBuilding, levelController.GreyBuildings[i]))
                {
                    closestDistance = distance.magnitude;
                    nearestBuilding = levelController.GreyBuildings[i];
                }
            }
            //
            objectiveBuilding = nearestBuilding;
        }
        else
        {
            // Cogemos la mas debil
            int lesserTroops = 10000;
            Building weakestBuilding = null;
            for (int i = 0; i < levelController.BlueBuildings.Count; i++)
            {
                //
                //bluePositions.Add(levelController.BlueBuildings[i].transform.position);
                //
                int buildingStength = levelController.BlueBuildings[i].CurrentBlueTroopers.Count;
                // TODO: Revisar el comprobador de obstáculo
                // En teoria ya está, asegurarse
                if ((weakestBuilding == null || buildingStength < lesserTroops) &&
                    levelController.CanSendThere(attackerBuilding, levelController.BlueBuildings[i]))
                {
                    lesserTroops = buildingStength;
                    weakestBuilding = levelController.BlueBuildings[i];
                }
            }
            // Solo pasa al ataque si está a la ofensiva
            // NOPE: Lo unico que hace es perder oportunidades
            // if(attitude == Attitude.Offensive)
            objectiveBuilding = weakestBuilding;
        }
        // Que los mande a la roja mas cercana
        // A la que le deje mas cerca de los azules
        if (objectiveBuilding == null)
        {
            // 
            if (levelController.RedBuildings.Count > 0)
            {
                // Cogemos la mas cercana
                float closestDistance = Mathf.Infinity;
                Building nearestBuilding = null;
                // Empezamos chequenado la del propio edifio, por si ya erea el más cercano
                Vector3 distanceToNearestBlue = Vector3.positiveInfinity;
                for (int j = 0; j < levelController.BlueBuildings.Count; j++)
                {
                    Vector3 distanceToThatBlue = levelController.BlueBuildings[j].transform.position -
                        attackerBuilding.transform.position;
                    //
                    if (distanceToThatBlue.sqrMagnitude < distanceToNearestBlue.sqrMagnitude)
                        distanceToNearestBlue = distanceToThatBlue;
                }
                // Guardamoes esta distancia y el propio edificio como referente
                closestDistance = distanceToNearestBlue.magnitude;
                nearestBuilding = attackerBuilding;
                //
                for (int i = 0; i < levelController.RedBuildings.Count; i++)
                {
                    //
                    //Vector3 distance = attackerBuilding.transform.position - levelController.RedBuildings[i].transform.position;
                    //
                    distanceToNearestBlue = Vector3.positiveInfinity;
                    for(int j = 0; j < levelController.BlueBuildings.Count; j++)
                    {
                        Vector3 distanceToThatBlue = levelController.BlueBuildings[j].transform.position - 
                            levelController.RedBuildings[i].transform.position;
                        //
                        if (distanceToThatBlue.sqrMagnitude < distanceToNearestBlue.sqrMagnitude)
                            distanceToNearestBlue = distanceToThatBlue;
                    }
                    //
                    if ((nearestBuilding == null || distanceToNearestBlue.magnitude < closestDistance) &&
                        levelController.CanSendThere(attackerBuilding, levelController.RedBuildings[i]))
                    {
                        closestDistance = distanceToNearestBlue.magnitude;
                        nearestBuilding = levelController.RedBuildings[i];
                    }
                }
                //
                objectiveBuilding = nearestBuilding;
            }
        }

        // Vamos a hacerlo un poco mejor
        // Que ataque si lo ve claro
        // Y si el mas apto es el propio edificiopues que tampoco envie
        if(objectiveBuilding != attackerBuilding &&
            attackerBuilding.CurrentRedTroopers.Count > objectiveBuilding.CurrentBlueTroopers.Count)
            attackerBuilding.SendTroops(objectiveBuilding);
        //Si no a atrincherarse
    }

    /// <summary>
    /// Vamos a ahcer que solo haga una por tirada
    /// Para que no se pase y se quede sin tropas de repente
    /// </summary>
    void CheckForBuildingUpdates(Building building)
    {
        // Vamos a probar con esto aqui a ver si asi no gasta tanto
        // Así nos aseguramos de que solo hace una en el turno
        if (levelController.GetTotalActiveTroopersOfTeam(Owner.Red) < levelController.MaxTroopersForRedTeam * 1 / 2
            || upgradePerformedThisStep)
        {
            //Debug.Log("Not enough troopers, upgrades will be not applied");
            return;
        }
            
        // Primero chequeamos si el edificio está en zona de peligro
        bool enemyBuildingsOnSight = false;
        for(int i = 0; i < building.BuildingsOnSight.Count; i++)
        {
            // Vamos a diferencairlo mejor
            if(building.BuildingsOnSight[i].CurrentOwner == Owner.Blue/* ||
                building.BuildingsOnSight[i].CurrentOwner == Owner.None*/)
            {
                enemyBuildingsOnSight = true;
                break;
            }
        }

        bool neutralBuildingsOnSight = false;
        for (int i = 0; i < building.BuildingsOnSight.Count; i++)
        {
            if (building.BuildingsOnSight[i].CurrentOwner == Owner.None)
            {
                neutralBuildingsOnSight = true;
                break;
            }
        }

        // Aqui decidiremos por unas mejoras u otras
        if (enemyBuildingsOnSight)
        {
            // Aqui montamos las defensivas
            for (int i = 0; i < building.BuildingImprovementList.Count; i++)
            { 
                if((building.BuildingImprovementList[i].improvementEffect == ImprovementEffect.ExtraDefense ||
                    building.BuildingImprovementList[i].improvementEffect == ImprovementEffect.None) &&
                    building.BuildingImprovementList[i].ConstructionStatus == BuildingImprovement.Status.NotConstructed)
                {
                    building.StartImprovementConstruction(i);
                    upgradePerformedThisStep = true;
                    return;
                }
            }
        }
        else if(!neutralBuildingsOnSight)
        {
            // Aqui las de producción
            // Aqui montamos las productivas
            for (int i = 0; i < building.BuildingImprovementList.Count; i++)
            {
                //
                if (building.BuildingImprovementList[i].improvementEffect == ImprovementEffect.ExtraEnergy &&
                    building.BuildingImprovementList[i].ConstructionStatus == BuildingImprovement.Status.NotConstructed)
                {
                    building.StartImprovementConstruction(i);
                    upgradePerformedThisStep = true;
                    return;
                }
                // Para que no se sature tanto
                if (building.BuildingImprovementList[i].improvementEffect == ImprovementEffect.ExtraStorage &&
                    levelController.GetTotalActiveTroopersOfTeam(Owner.Red) > levelController.MaxTroopersForRedTeam * 3/4 &&
                    building.BuildingImprovementList[i].ConstructionStatus == BuildingImprovement.Status.NotConstructed)
                {
                    building.StartImprovementConstruction(i);
                    upgradePerformedThisStep = true;
                    return;
                }
            }
        }
    }

    void EvaluateGeneralSituation()
    {
        // En la fase de expansion que vaya tirando con la actitud por defencto
        if (levelController.GreyBuildings.Count > 0 && attitude == Attitude.Expanding)
            return;
        // Cuando se anima la cosa pasamos a evaluar la situiación general
        float trooperPowerRatio = levelController.GetTotalActiveTroopersOfTeam(Owner.Red) / 
            levelController.GetTotalActiveTroopersOfTeam(Owner.Blue);
        float buildingPowerRatio = levelController.RedBuildings.Count / levelController.BlueBuildings.Count;
        // De momento nos basamos solo en la cantidad de tropas
        if (trooperPowerRatio > 1)
            attitude = Attitude.Offensive;
        else
            attitude = Attitude.Defensive;
    }

    //
    IEnumerator CheckAndExecute()
    {
        //
        // TODO: QUe pare cuando acabe la partida
        while (!levelController.MatchEnded)
        {
            //
            yield return new WaitForSeconds(timeBetweenChecks);
            CheckOwnedBuildings();
            CheckAbilities();
            EvaluateGeneralSituation();
        }
    }
}
