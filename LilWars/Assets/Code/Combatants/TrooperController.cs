using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TrooperController : BaseCombatant
{
    public enum Status
    {
        Idle,
        Wandering,
        MovingToDestination,
        MovingInFight,
        Fighting
    }

    public TrooperStats trooperStats;

    //public GameObject proyectilePrefab;

    public Texture greenTexture;
    public Texture redTexture;
    public Texture moralIcon;

    public AudioClip[] deathClips;

    private Transform currentBuilding;
    private Transform objectiveBuilding;
    private TrooperController objectiveTrooper = null;

    private Vector3 currentDestination;
    private float idleCounter = 0;
    private Status status = Status.Idle;
    private Owner team;
    private Building currentBuildingBehaviour;
    private Building objectiveBuildingBehaviour;
    private float attackCooldown;
    private float currentHealth;
    private LevelController levelController;
    private ParticleSystem fireArmPS;

    // Para el bonus de moral
    private float bonusSpeed;
    private float bonusAttackSpeed;
    private float bonusDuration;        // Este lo aplicaremos un poco mas tarde
    

    public Transform CurrentBuilding
    {
        get { return currentBuilding; }
        set {
            //
            currentBuilding = value;
            //
            if (currentBuilding != null)
            {
                currentBuildingBehaviour = currentBuilding.GetComponent<Building>();
                currentBuildingBehaviour.ArriveTroop(this);
                //
                if(levelController != null)
                    levelController.ExitBuildlessZone(this);
            }
            //
            else
                currentBuildingBehaviour = null;
            
        }
    }

    public Transform ObjectiveBuilding
    {
        get { return objectiveBuilding; }
        set {
            objectiveBuilding = value;
            if (objectiveBuilding != null)
                objectiveBuildingBehaviour = objectiveBuilding.GetComponent<Building>();
            else
                objectiveBuildingBehaviour = null;
            //Debug.Log(objectiveBuildingBehaviour);
        }
    }

    public Status CurrentStatus { get { return status; } }
    public Owner Team
    {
        get { return team; }
        set { team = value; }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        //
        base.Start();
        //
        idleCounter = UnityEngine.Random.Range(0,1);
        
        levelController = FindObjectOfType<LevelController>();
        fireArmPS = GetComponentInChildren<ParticleSystem>();
        
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        UpdateBehaviour(dt);
        
    }

    private void OnEnable()
    {
        // Para cuando tiremos de pool
        bonusAttackSpeed = 0;
        bonusSpeed = 0;
        
    }

    private void OnDisable()
    {
        //
        CurrentBuilding = null;
        objectiveBuilding = null;
        status = Status.Idle;
        team = Owner.None;
    }

    private void OnGUI()
    {
        //
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        //
        if(currentHealth < trooperStats.initialHealth)
        {
            GUI.DrawTexture(new Rect(screenPosition.x - 10, Screen.height - screenPosition.y - 20, 20, 6), redTexture);

            GUI.DrawTexture(new Rect(screenPosition.x - 10, Screen.height - screenPosition.y - 20,
                currentHealth / trooperStats.initialHealth * 20, 6), greenTexture);
        }
        
        //
        if(bonusSpeed > 0)
        {
            GUI.DrawTexture(new Rect(screenPosition.x - 10, Screen.height - screenPosition.y - 60, 40, 40), moralIcon);
        }

        // Debugueo
        //string textToUse = "";
        //if (currentBuildingBehaviour != null) textToUse = "B";
        //else textToUse = "O";
        //textToUse += ",";
        ////
        //switch (status)
        //{
        //    case Status.Idle: textToUse += "I"; break;
        //    case Status.Wandering: textToUse += "W"; break;
        //    case Status.MovingToDestination: textToUse += "MD"; break;
        //    case Status.MovingInFight: textToUse += "MF"; break;
        //    case Status.Fighting: textToUse += "F"; break;
        //}
        ////
        //GUI.Label(new Rect(screenPosition.x - 20, Screen.height - screenPosition.y + 10, 40, 20), textToUse);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="trooperStats"></param>
    public void Initiate(TrooperStats trooperStats)
    {
        //
        this.trooperStats = trooperStats;
        currentHealth = this.trooperStats.initialHealth;
        //
        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        switch (team)
        {
            case Owner.Blue: meshRenderer.material = this.trooperStats.trooperMaterials.blueMaterial; break;
            case Owner.Red: meshRenderer.material = this.trooperStats.trooperMaterials.redMaterial; break;
        }
        //
        StartCoroutine(PeriodicCheckOfFloorHeight());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dt"></param>
    void UpdateBehaviour(float dt)
    {
        //
        Building candidateNeighbour = null;
        //
        if (objectiveTrooper != null && !objectiveTrooper.isActiveAndEnabled)
            objectiveTrooper = null;
        //
        //if (status != Status.Fighting && CheckEnemyPresence(currentBuildingBehaviour))
        //    EngageIntoCombat();
        //
        switch (status)
        {
            case Status.Idle:
                // TODO: Revisar como manejamos esto
                if (CheckCombatInOpenZone())
                    return;
                //
                if (CheckEnemyPresence(currentBuildingBehaviour))
                    EngageIntoCombatInBuilding(currentBuildingBehaviour);
                else if ((candidateNeighbour = CheckEnemyPresenceInNearBuildings(currentBuildingBehaviour)) != null)
                {
                    // TODO: A ver como gestionamos esto
                    EngageIntoCombatInBuilding(candidateNeighbour);
                }
                else
                {
                    idleCounter += dt;
                    if (idleCounter >= 1)
                    {
                        DecideNextWanderDestination();
                        status = Status.Wandering;
                        idleCounter -= 1;
                        // Que se curen poco a poco si descansan
                        if (currentHealth < trooperStats.initialHealth)
                            currentHealth++;
                    }
                }                
                break;

            case Status.Wandering:
                // TODO: Revisar como manejamos esto
                if (CheckCombatInOpenZone())
                    return;
                //
                if (CheckEnemyPresence(currentBuildingBehaviour))
                    EngageIntoCombatInBuilding(currentBuildingBehaviour);
                else if ((candidateNeighbour = CheckEnemyPresenceInNearBuildings(currentBuildingBehaviour)) != null)
                {
                    // TODO: A ver como gestionamos esto
                    EngageIntoCombatInBuilding(candidateNeighbour);
                }
                else
                {
                    Move(dt);
                    // Ignoramos la diferencia en altura
                    Vector3 destinationDistance = transform.position - currentDestination;
                    destinationDistance.y = 0;
                    //
                    if (destinationDistance.magnitude < 0.5f)
                        status = Status.Idle;
                }                
                break;

            case Status.MovingToDestination:
                Move(dt);
                //
                Vector3 objectiveBuildingDistance = transform.position - objectiveBuilding.position;
                objectiveBuildingDistance.y = 0;
                // TODO: Revisar como manejamos esto
                if (CheckCombatInOpenZone())
                    return;
                // TODO: Lo quitamos de momento
                //if (CheckEnemiesInObjectiveBuilding())
                //    return;
                // Sobre todo para los de largo alcance
                // Ver si detectan enemigos de camino al edificio objetivo
                // TODO: Revisar esta condicion
                //if (CheckEnemyPresence(objectiveBuildingBehaviour)//)
                //    && (objectiveBuildingBehaviour.GetNearestTrooper(team, transform.position).transform.position
                //        - transform.position).magnitude < trooperStats.weaponStats.attackReach)
                //{
                //    CurrentBuilding = objectiveBuilding;
                //    ObjectiveBuilding = null;
                //    EngageIntoCombat();
                //    //
                //    //currentBuildingBehaviour.ArriveTroop(this);
                //}
                // TODO: Ajustar la distancia para favorecer a los breachers
                else if (objectiveBuildingDistance.sqrMagnitude < Mathf.Pow(15, 2))
                {
                    // Lo hacemos con propiedad para que pille también el behaviour
                    CurrentBuilding = objectiveBuilding;
                    ObjectiveBuilding = null;
                    //
                    if (!CheckEnemyPresence(currentBuildingBehaviour)) status = Status.Idle;
                    else EngageIntoCombatInBuilding(currentBuildingBehaviour);
                    //
                    //currentBuildingBehaviour.ArriveTroop(this);
                }
                break;

            case Status.MovingInFight:
                // Vmoa a ver si va esto
                //if((transform.position - currentBuilding.position).sqrMagnitude > Mathf.Pow(10,2))
                //{
                //    currentDestination = currentBuilding.position;
                //}
                //
                Move(dt);
                EngageIntoCombatInBuilding(currentBuildingBehaviour);
                break;

            case Status.Fighting:
                //
                if(objectiveTrooper == null || !objectiveTrooper.isActiveAndEnabled)
                {
                    // Lo reseteamos al morir el objetivo
                    // attackCooldown = 0;
                    //
                    if (currentBuildingBehaviour != null)
                    {
                        if (!CheckEnemyPresence(currentBuildingBehaviour)) status = Status.Idle;
                        else EngageIntoCombatInBuilding(currentBuildingBehaviour);
                    }
                    // TODO: Revisar este caso
                    else status = Status.MovingToDestination;
                }
                else
                {
                    //
                    attackCooldown += dt;
                    //
                    if (attackCooldown >= (trooperStats.weaponStats.attackRate - bonusAttackSpeed))
                    {
                        //attackCooldown -= (trooperStats.weaponStats.attackRate - bonusAttackSpeed);
                        attackCooldown = 0;

                        //TODO: Casos para enemigo en edicio y fortificado
                        float chanceToHit = 0.75f;
                        // Si el ataque es producido desde otro edificio (o campo abierto)
                        // Así favorecemos a tropas de asalto en este aspecto
                        //if (currentBuildingBehaviour != objectiveTrooper.currentBuildingBehaviour)
                        // Cambio: Si el objetivo está defendiendo su edificio
                        if (objectiveTrooper.currentBuildingBehaviour != null && 
                            objectiveTrooper.currentBuildingBehaviour.CurrentOwner == objectiveTrooper.team)
                        {
                            // Si el objetivo esta en campo abierto acierto normal
                            if(objectiveTrooper.currentBuildingBehaviour != null)
                            {
                                // Muy jodido si tiene barricadas
                                if(objectiveTrooper.currentBuildingBehaviour.DefenseBonus > 0)
                                {
                                    chanceToHit = 0.25f;
                                }
                                // No tanto si no las tiene
                                else
                                {
                                    chanceToHit = 0.5f;
                                }
                            }
                        }
                        // Debug.Log("Chance to hit: " + chanceToHit);
                        //
                        float attackRoll = UnityEngine.Random.Range(0f,1f);
                        bool attackHits = false;
                        if(attackRoll >= chanceToHit)
                        {
                            //
                            // objectiveTrooper.ReceiveAttack(this);
                            attackHits = true;
                        }                        
                        
                        //
                        PlayClipWithoutOverlapping(trooperStats.weaponStats.attackClip, trooperStats.weaponStats.clipVolume);
                        //
                        if (trooperStats.weaponStats.fireArm && (objectiveTrooper != null && objectiveTrooper.isActiveAndEnabled))
                        {
                            //
                            //fireArmPS.transform.LookAt(objectiveTrooper.transform);
                            //fireArmPS.Play();
                            //
                            if(trooperStats.weaponStats.weaponProyectileMaterial != null)
                            {
                                //GameObject newProyectile = Instantiate(trooperStats.weaponStats.proyectilePrefab, 
                                //    transform.position, Quaternion.identity);
                                //newProyectile.transform.LookAt(objectiveTrooper.transform);
                                //BulletBehaviour bulletBehaviour = newProyectile.GetComponent<BulletBehaviour>();
                                //bulletBehaviour.InitializeTrooperBullet(objectiveTrooper.transform.position, 
                                //    trooperStats.weaponStats.weaponProyectileMaterial, attackHits, objectiveTrooper, 
                                //    trooperStats.weaponStats, this);
                                //
                                BulletBehaviour bulletBehaviour1 = levelController.GetBullet();
                                bulletBehaviour1.transform.position = transform.position;
                                bulletBehaviour1.transform.LookAt(objectiveTrooper.transform);
                                bulletBehaviour1.InitializeTrooperBullet(objectiveTrooper.transform.position,
                                    trooperStats.weaponStats.weaponProyectileMaterial, attackHits, objectiveTrooper,
                                    trooperStats.weaponStats, this);
                            }
                        }
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    bool CheckEnemyPresence(Building buildingToCheckBehaviour)
    {
        // Chequeamos primero si toca pelear
        if (buildingToCheckBehaviour != null)
        {            
            // Primero chequeamos enemigos en el propio edificio
            switch (team)
            {
                //
                case Owner.Blue:
                    if (buildingToCheckBehaviour.CurrentRedTroopers.Count > 0)
                        return true;
                    break;
                case Owner.Red:
                    if (buildingToCheckBehaviour.CurrentBlueTroopers.Count > 0)
                        return true;
                    break;
            }            
        }
        return false;
    }

    Building CheckEnemyPresenceInNearBuildings(Building buildingToCheckBehaviour)
    {
        // Chequeamos primero si toca pelear
        if (buildingToCheckBehaviour != null)
        {
            // Y después, en caso negativo, los vecinos
            for (int i = 0; i < buildingToCheckBehaviour.BuildingsOnSight.Count; i++)
            {
                Building neighbour = buildingToCheckBehaviour.BuildingsOnSight[i];
                Vector3 buildingsDistance = buildingToCheckBehaviour.transform.position - neighbour.transform.position;
                switch (team)
                {
                    // TODO: Chequear también distancia
                    // TODO: Hacer este chequeo en el edificio al arrancar y a correr
                    case Owner.Blue:
                        // TODO: Revisar este hardcodeado
                        if (neighbour.CurrentRedTroopers.Count > 0 &&
                            buildingsDistance.sqrMagnitude < Mathf.Pow(trooperStats.weaponStats.attackReach + 10, 2))
                            return neighbour;
                        break;
                    case Owner.Red:
                        if (neighbour.CurrentBlueTroopers.Count > 0 &&
                            buildingsDistance.sqrMagnitude < Mathf.Pow(trooperStats.weaponStats.attackReach + 10, 2))
                            return neighbour;
                        break;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    bool CheckCombatInOpenZone()
    {
        //
        //objectiveTrooper = levelController.GetNearestTrooper(team, transform.position);
        objectiveTrooper = levelController.GetFirstTrooperInRange(team, transform.position, trooperStats.weaponStats);
        if (objectiveTrooper != null && objectiveTrooper.isActiveAndEnabled)
        {
            Vector3 distance = transform.position - objectiveTrooper.transform.position;
            //
            if (distance.magnitude < trooperStats.weaponStats.attackReach)
            {
                status = Status.Fighting;
                return true;
            }
        }
        // Lo vovlemos a poner a null en caso negativo para no liar
        else
            objectiveTrooper = null;
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    bool CheckEnemiesInObjectiveBuilding()
    {
        //
        //objectiveTrooper = objectiveBuildingBehaviour.GetNearestTrooper(team, transform.position);
        objectiveTrooper = objectiveBuildingBehaviour.GetRandomTrooper(team);
        if (objectiveTrooper != null && objectiveTrooper.isActiveAndEnabled)
        {
            Vector3 distance = transform.position - objectiveTrooper.transform.position;
            //
            if (distance.sqrMagnitude < Mathf.Pow(trooperStats.weaponStats.attackReach, 2))
            {
                status = Status.Fighting;
                return true;
            }
        }
        // Lo vovlemos a poner a null en caso negativo para no liar
        else
            objectiveTrooper = null;
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    void EngageIntoCombatInBuilding(Building building)
    {
        //
        //objectiveTrooper = building.GetNearestTrooper(team, transform.position);
        objectiveTrooper = building.GetRandomTrooper(team);
        // TODO: Meterlo bien
        // TrooperController possibleObjectiveTrooper = levelController.GetNearestTrooper(team, transform.position);
        //
        if (objectiveTrooper == null || !objectiveTrooper.isActiveAndEnabled)
            status = Status.Idle;
        else if ((transform.position - objectiveTrooper.transform.position).magnitude < trooperStats.weaponStats.attackReach)
        {
            status = Status.Fighting;
        }
        else
        {
            status = Status.MovingInFight;
            currentDestination = objectiveTrooper.transform.position;
        }
            
    }

    /// <summary>
    /// 
    /// </summary>
    void DecideNextWanderDestination()
    {
        //
        float radius = UnityEngine.Random.Range(0, 10);
        float angle = UnityEngine.Random.Range(0, 360);
        //
        float xPos = radius * Mathf.Cos(angle);
        float zPos = radius * Mathf.Sin(angle);
        //
        if(currentBuilding != null)
        {
            //
            currentDestination = currentBuilding.position + new Vector3(xPos, 0, zPos);
            currentDestination.y = transform.position.y;
        }
        else
        {
            Debug.Log("Current building not assigned, aplying correction");
            GetNearestBuilding();
        }
    }

    /// <summary>
    /// Para cuando pierden la referencia del edificio
    /// NOTA: Esto es un apaño, no debería pasar por aqui
    /// </summary>
    void GetNearestBuilding()
    {
        // Cogemos la mas cercana
        float closestDistance = Mathf.Infinity;
        Building nearestBuilding = null;
        for (int i = 0; i < levelController.LevelBuildings.Length; i++)
        {
            //
            Vector3 distance = transform.position - levelController.LevelBuildings[i].transform.position;
            //
            if (nearestBuilding == null || distance.magnitude < closestDistance)
            {
                closestDistance = distance.magnitude;
                nearestBuilding = levelController.LevelBuildings[i];
            }
        }
        //
        //nearestBuilding.ArriveTroop(this);
        CurrentBuilding = nearestBuilding.transform;
    }

    public void SetNewDestination(Building objectiveBuilding)
    {
        // Testeo
        if (objectiveBuilding == null || currentBuilding == null)
        {
            // TODO: Plantear qué hacer en estos casos
            // Falla current building, lidiaremos con eso
            Debug.LogError("Destiantion error: " + objectiveBuilding + ", " + currentBuilding);
            return;
        }
            
        //
        currentDestination = objectiveBuilding.transform.position + (currentBuilding.position - transform.position);
        currentDestination.y = transform.position.y;
        // Lo hacemos con la propiedad para qye haga las gestiones automaticas
        CurrentBuilding = null;
        this.ObjectiveBuilding = objectiveBuilding.gameObject.transform;
        //
        if(levelController != null)
            levelController.EnterBuildlessZone(this);
        //
        status = Status.MovingToDestination;
        
    }

    // De momento con valores preestablecidos
    public void ReceiveMoralBuff()
    {
        bonusAttackSpeed = trooperStats.weaponStats.attackRate * 0.25f;
        bonusSpeed = trooperStats.movementSpeed * 0.25f;
        // TODO: Duracion bufo
        StartCoroutine(WaitAndEndBuff());
    }

    IEnumerator WaitAndEndBuff()
    {
        yield return new WaitForSeconds(10);
        bonusAttackSpeed = 0;
        bonusSpeed = 0;
    }

    void Move(float dt)
    {
        Vector3 direction = (currentDestination - transform.position).normalized;
        transform.Translate(direction * (trooperStats.movementSpeed + bonusSpeed) * dt);
        //
        //PlayClipWithoutOverlapping(trooperStats.movingClip);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="attacker"></param>
    public void ReceiveAttack(/*TrooperController attacker, */int attackDamage, int attackerType)
    {
        // Los troopers que defiendan un edificio con puntos defensivos
        int totalDefense = trooperStats.armor;
        // Armadura extra
        // Esto lo hacemos ahora con probabilidad de impacto
        //if (currentBuildingBehaviour != null && currentBuildingBehaviour.CurrentOwner == team)
        //    totalDefense += currentBuildingBehaviour.DefenseBonus;
        //
        int sufferedDamage = Mathf.Max(attackDamage - totalDefense, 0);
        currentHealth -= sufferedDamage;
        //
        AttackDamageInfo attackDamageInfo = new AttackDamageInfo(transform.position, sufferedDamage);
        levelController.SendDamageInfo(attackDamageInfo);
        // Hacemos el chequeo asi para que solo salte una vez y no explote
        if(currentHealth <= 0 && currentHealth + sufferedDamage > 0)
        {
            //levelController.AnotateKill(attacker, this);
            levelController.AnotateKill(attackerType, this);
            Die();
        }
    }

    /// <summary>
    /// Variante para armas fijas
    /// </summary>
    /// <param name="attacker"></param>
    public void ReceiveAttack(FixedWeapon attacker)
    {

        // Los troopers que defiendan un edificio con puntos defensivos
        int totalDefense = trooperStats.armor;
        if (currentBuildingBehaviour != null && currentBuildingBehaviour.CurrentOwner == team)
            totalDefense += currentBuildingBehaviour.DefenseBonus;
        //
        int sufferedDamage = Mathf.Max(attacker.weaponStats.attack - totalDefense, 0);
        currentHealth -= sufferedDamage;
        //
        AttackDamageInfo attackDamageInfo = new AttackDamageInfo(transform.position, sufferedDamage);
        levelController.SendDamageInfo(attackDamageInfo);
        // Hacemos el chequeo asi para que solo salte una vez y no explote
        if (currentHealth <= 0 && currentHealth + sufferedDamage > 0)
        {
            levelController.AnotateKill(attacker, this);
            Die();
        }
    }

    /// <summary>
    /// Ataques que no provienen de un atacante específico
    /// </summary>
    /// <param name="damage"></param>
    public void ReceiveAttack(int damage)
    {

        // Los troopers que defiendan un edificio con puntos defensivos
        int totalDefense = trooperStats.armor;
        if (currentBuildingBehaviour != null && currentBuildingBehaviour.CurrentOwner == team)
            totalDefense += currentBuildingBehaviour.DefenseBonus;
        //
        int sufferedDamage = Mathf.Max(damage - totalDefense, 0);
        currentHealth -= sufferedDamage;
        //
        AttackDamageInfo attackDamageInfo = new AttackDamageInfo(transform.position, sufferedDamage);
        levelController.SendDamageInfo(attackDamageInfo);
        // Hacemos el chequeo asi para que solo salte una vez y no explote
        if (currentHealth <= 0 && currentHealth + sufferedDamage > 0)
        {
            levelController.AnotateExplosionKill(this);
            Die();
        }
    }

    void Die()
    {
        // Vamos a ponerlo aqui
        CheckFloorHeight();
        //
        //GameObject deadBody = Instantiate(trooperStats.deadBodyPrefab, transform.position, Quaternion.identity);
        //DeadController deadController = deadBody.GetComponent<DeadController>();
        //deadController.SetMaterial(team);
        DeadController deadController = levelController.GetDeadBody();
        deadController.SetMaterial(team);
        // TODO: Hardocedado. Hacerlo bien
        deadController.transform.position = transform.position + new Vector3(0, -0.3f, 0);
        deadController.InitiateBody();
        //
        GameObject deathEffect = Instantiate(trooperStats.deathEffectPrefab, transform.position, Quaternion.identity);
        AudioSource audioSource = deathEffect.GetComponent<AudioSource>();
        int deathClipToUse = UnityEngine.Random.Range(0, deathClips.Length);
        audioSource.clip = deathClips[deathClipToUse];
        audioSource.Play();
        //
        if (currentBuildingBehaviour != null)
            currentBuildingBehaviour.RemoveTrooper(this);
        else
            levelController.ExitBuildlessZone(this);
        //Destroy(gameObject);
        levelController.ReturnTrooper(this);
    }

    

    IEnumerator PeriodicCheckOfFloorHeight()
    {
        //
        while (true)
        {
            //
            yield return new WaitForSeconds(0.2f);
            //
            //Debug.Log("Checking floor height in coroutine, " + team);
            CheckFloorHeight();
        }
    }

    //
    void CheckFloorHeight()
    {
        //
        //Debug.Log("Checking floor height");
        // TODO: Trabajar mejor los parámetros
        Vector3 rayOrigin = transform.position + (Vector3.up * 10);
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, 30))
        {
            // 0.5 es la altura normal del moñaco sobre el suelo
            transform.position = hitInfo.point + (Vector3.up * 1f);
            //Debug.Log("Ray origin y: " + rayOrigin.y + ", Hit point: " + hitInfo.point.y + ", New height: " + transform.position.y);
        }
    }

    [System.Serializable]
    public class TrooperMaterials
    {
        public Material greyMaterial;
        public Material redMaterial;
        public Material blueMaterial;
    }
}
