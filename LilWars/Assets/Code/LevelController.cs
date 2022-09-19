using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    public Texture selectedBuildingIcon;
    public float timeBetweenChecks = 0.5f;
    public GUISkin uISkin;
    public AudioClip errorClip;

    public float playerBaseResourceRate = 1;
    public float aiBaseResourceRate = 0.9f;

    // provisional
    //public int maxTroopersPerTeam = 200;
    public GameObject deadBodyPrefab;
    public GameObject bulletPrefab;
    public GameObject trooperPrefab;

    [HideInInspector] public static LevelController instance;

    //
    private Building[] levelBuildings;
    //
    private List<Building> redBuildings;
    private List<Building> blueBuildings;
    private List<Building> greyBuildings;

    private Building selectedBuilding;
    private Building preSelectedBuilding;

    private bool matchEnded = false;
    private bool playerWon = false;

    //
    private float currentPlayerResourceRate;
    private float currentAiResourceRate;

    //private GameManager gameManager;
    private MatchStatistics matchStatistics;

    // Para la zona general
    public List<TrooperController> currentBlueTroopers;
    public List<TrooperController> currentRedTroopers;
    public List<Platoon> platoons;

    // Pool de troopers
    private List<TrooperController> trooperPool;
    // Pool de disparos
    private List<BulletBehaviour> bulletPool;

    private List<AttackDamageInfo> attackDamageInfoList;

    private List<DeadController> deadBodiesPool;
    private List<DeadController> activeDeadBodies;

    // De momento lo manejamos aqui
    private AudioSource audioSource;

    // Puntos de mando, de momento los gestionamos asi
    private float playerCommandPoints;
    private float aiCommandPoints;

    private int maxActiveTroopers;

    private float currentTimeScale = 1;

    #region Properties

    public Building SelectedBuilding
    {
        get { return selectedBuilding; }
        set { selectedBuilding = value; }
    }

    public Building PreSelectedBuilding
    {
        get { return preSelectedBuilding; }
        set { preSelectedBuilding = value; }
    }

    public Building[] LevelBuildings { get { return levelBuildings; } }
    public List<Building> RedBuildings { get { return redBuildings; } }
    public List<Building> BlueBuildings { get { return blueBuildings; } }
    public List<Building> GreyBuildings { get { return greyBuildings; } }

    public bool MatchEnded { get { return matchEnded; } }

    public float PlayerResourceRate { get { return currentPlayerResourceRate; } }
    public float AiResourceRate { get { return currentAiResourceRate; } }

    public float PlayerCommandPoints { 
        get { return playerCommandPoints; }
        set { playerCommandPoints = value; }
    }
    public float AiCommandPoints { 
        get { return aiCommandPoints; }
        set { aiCommandPoints = value; }
    }

    public int MaxTroopersForBlueTeam
    {
        get
        {
            //
            int maxTrooperForBlueTeam = 0;
            //
            for(int i = 0; i < blueBuildings.Count; i++)
            {
                maxTrooperForBlueTeam += blueBuildings[i].PopulationCapacity;
            }
            //
            return maxTrooperForBlueTeam;
        }
    }

    public int MaxTroopersForRedTeam
    {
        get
        {
            //
            int maxTrooperForRedTeam = 0;
            //
            for (int i = 0; i < redBuildings.Count; i++)
            {
                maxTrooperForRedTeam += redBuildings[i].PopulationCapacity;
            }
            //
            return maxTrooperForRedTeam;
        }
    }

    public MatchStatistics MatchStatisticsRef
    {
        get { return matchStatistics; }
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //
        instance = this;
        //
        levelBuildings = FindObjectsOfType<Building>();
        //
        maxActiveTroopers = levelBuildings.Length * 30;
        //
        redBuildings = new List<Building>(5);
        blueBuildings = new List<Building>(5);
        greyBuildings = new List<Building>(5);
        //
        StartCoroutine(CheckBuildingOwnership());
        //
        if(GameManager.instance != null)
        {
            aiBaseResourceRate = GameManager.instance.difficulty;
        }
        //
        InitiateMatchStatistics();
        //
        StablishBuildlessZone();
        //
        attackDamageInfoList = new List<AttackDamageInfo>(100);
        //        
        InitiateDeadBodies();
        //
        InitiateBulletPool();
        //
        InitiateTrooperPool();
        //
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        //
        float dt = Time.deltaTime;
        // Para volver al menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //
            if (matchEnded)
            {
                Time.timeScale = 1;
                SceneManager.LoadScene(0);
            }
            //
            else if (Time.timeScale != 0) Time.timeScale = 0;
            else Time.timeScale = currentTimeScale;
        }
        // Para volver al menu
        if (Input.GetKeyDown(KeyCode.Return) && Time.timeScale == 0)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(0);
        }
        //
        UpdateDamageInfo(Time.deltaTime);
        // Checko de cafdavertes
        if (Input.GetKeyDown(KeyCode.M))
        {
            DeactivateActiveBodies();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            ReactivateActiveBodies();
        }
        //
        if (!matchEnded)
        {
            UpdateCommandPoints(dt);
        }        
    }

    private void OnGUI()
    {
        //
        if(selectedBuilding != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(selectedBuilding.transform.position);
            GUI.DrawTexture(new Rect(screenPosition.x - 50, Screen.height - screenPosition.y - 50, 100, 100), selectedBuildingIcon);
        }
        // TODO: De momento tabulador a lo guarro
        if (matchEnded || Input.GetKey(KeyCode.Tab))
        {
            if (playerWon)
                GUI.Label(new Rect(Screen.width * 1/4, Screen.height * 1/3, Screen.width * 1/2, Screen.height * 1/3), "BLUE WINS", uISkin.label);
            else if(matchEnded)
                GUI.Label(new Rect(Screen.width * 1 / 4, Screen.height * 1 / 3, Screen.width * 1 / 2, Screen.height * 1 / 3), "RED WINS", uISkin.label);
            //
            GUI.Label(new Rect(Screen.width * 1 / 4, Screen.height * 2 / 3, Screen.width * 1 / 2, Screen.height * 1 / 3), 
                "Press Esc to return", uISkin.label);
            //
            ShowMatchStatistics();
        }
        // Lista de daños
        //for (int i = 0; i < attackDamageInfoList.Count; i++)
        //{
        //    Vector3 screenPosition = Camera.main.WorldToScreenPoint(attackDamageInfoList[i].damageWorldPosition);
        //    screenPosition.y += attackDamageInfoList[i].activeTime * 100;
        //    GUI.Label(new Rect(screenPosition.x - 20, Screen.height - screenPosition.y - 15, 40, 30),
        //        attackDamageInfoList[i].damageDone + "", uISkin.label);
        //}
        //
        if(Time.timeScale == 0)
        {
            GUI.Label(new Rect(Screen.width * 1 / 4, Screen.height * 1 / 2 - 50, Screen.width * 1 / 2, 100), "PAUSED", uISkin.label);
        }
        //Mostraremos velocidad aqui de momento
        GUI.Label(new Rect(Screen.width * 1 / 4, /*Screen.height * 1 / 2 - 50*/0, Screen.width * 1 / 2, 100), 
            "SPEED: " + currentTimeScale.ToString("0.0"), uISkin.label);

        // Vamos a mostrar la cantidad de soldados por bando
        int totalBlueTroopers = GetTotalActiveTroopersOfTeam(Owner.Blue);
        int totalRedTroopers = GetTotalActiveTroopersOfTeam(Owner.Red);
        GUI.Label(new Rect(Screen.width - 100, 50, 100, 50), totalBlueTroopers + "/" + MaxTroopersForBlueTeam, uISkin.customStyles[0]);
        GUI.Label(new Rect(20, 50, 100, 50), totalRedTroopers + "/" + MaxTroopersForRedTeam, uISkin.customStyles[1]);
        // Y la capacidad de producción de cada uno
        GUI.Label(new Rect(Screen.width - 100, 100, 100, 50), "Power: " + currentPlayerResourceRate, uISkin.customStyles[0]);
        GUI.Label(new Rect(20, 100, 100, 50), "Power: " + currentAiResourceRate + "", uISkin.customStyles[1]);

        //Y los puntos de mando
        GUI.Label(new Rect(Screen.width - 100, 150, 100, 50), "CP: " + (int)playerCommandPoints, uISkin.customStyles[0]);
        GUI.Label(new Rect(20, 150, 100, 50), "CP: " + (int)aiCommandPoints + "", uISkin.customStyles[1]);
    }

    #region Dead Bodies Pool Methods

    //
    void InitiateDeadBodies()
    {
        // Haremos que sea 10 veces la cantidad de tropas
        int maxDeadBodies = maxActiveTroopers * 5 * 2;
        deadBodiesPool = new List<DeadController>(maxDeadBodies);
        activeDeadBodies = new List<DeadController>(maxDeadBodies);
        //
        for (int i = 0; i < maxDeadBodies; i++)
        {
            DeadController deadController = Instantiate(deadBodyPrefab, Vector3.zero, Quaternion.identity).GetComponent<DeadController>();
            deadController.gameObject.SetActive(false);
            deadBodiesPool.Add(deadController);
        }
    }

    //
    public DeadController GetDeadBody()
    {
        DeadController deadBodyToReturn = null;
        // Si quedan cadaveres en el pool los sacamos
        if(deadBodiesPool.Count > 0)
        {
            deadBodyToReturn = deadBodiesPool[0];
            //deadBodiesPool.RemoveAt(0);
            deadBodiesPool.Remove(deadBodyToReturn);
            activeDeadBodies.Add(deadBodyToReturn);
            deadBodyToReturn.gameObject.SetActive(true);
        } // Si no vamos cogiendo los mas viejos de la activa
        else
        {
            deadBodyToReturn = activeDeadBodies[0];
            activeDeadBodies.Remove(deadBodyToReturn);
            activeDeadBodies.Add(deadBodyToReturn);
            deadBodyToReturn.gameObject.SetActive(true);
            Debug.Log("Recicling corpse");
        }

        return deadBodyToReturn;
    }

    //
    public void DestroyBody(DeadController bodyToDestroy)
    {
        deadBodiesPool.Add(bodyToDestroy);
        activeDeadBodies.Remove(bodyToDestroy);
        bodyToDestroy.gameObject.SetActive(false);
    }

    // Para testeo
    void DeactivateActiveBodies()
    {
        //
        for (int i = 0; i < activeDeadBodies.Count; i++)
        {
            activeDeadBodies[i].gameObject.SetActive(false);
        }
    }

    void ReactivateActiveBodies()
    {
        //
        for (int i = 0; i < activeDeadBodies.Count; i++)
        {
            activeDeadBodies[i].gameObject.SetActive(true);
        }
    }

    #endregion

    #region Bullet Pool Methods

    void InitiateBulletPool()
    {
        // Haremos que sea 10 veces la cantidad de tropas
        int maxBullets = maxActiveTroopers * 2 * 2;
        bulletPool = new List<BulletBehaviour>(maxBullets);
        //
        for (int i = 0; i < maxBullets; i++)
        {
            BulletBehaviour newBullet = Instantiate(bulletPrefab, Vector3.zero, Quaternion.identity).GetComponent<BulletBehaviour>();
            newBullet.gameObject.SetActive(false);
            bulletPool.Add(newBullet);
        }
    }

    public BulletBehaviour GetBullet()
    {
        // Debug.Log("Getting bullet");
        BulletBehaviour bullet = null;
        if(bulletPool.Count > 0)
        {
            bullet = bulletPool[0];
            if (bullet.gameObject.activeSelf)
                Debug.LogError("Error, bullet already active");
            bullet.gameObject.SetActive(true);
            bulletPool.Remove(bullet);
        }
        else
        {
            Debug.Log("No more bullets available");
        }
        return bullet;
    }

    public void ReturnBullet(BulletBehaviour bulletBehaviour)
    {
        // Debug.Log("Returning bullet");
        bulletPool.Add(bulletBehaviour);
        bulletBehaviour.gameObject.SetActive(false);
    }

    #endregion

    #region Trooper Pool Methods

    void InitiateTrooperPool()
    {
        // 
        trooperPool = new List<TrooperController>(maxActiveTroopers);
        //
        for (int i = 0; i < maxActiveTroopers; i++)
        {
            TrooperController newTrooper = Instantiate(trooperPrefab, Vector3.zero, Quaternion.identity).GetComponent<TrooperController>();
            newTrooper.gameObject.SetActive(false);
            trooperPool.Add(newTrooper);
        }
    }

    public TrooperController GetTrooper()
    {
        // Debug.Log("Getting bullet");
        TrooperController trooper = null;
        if (trooperPool.Count > 0)
        {
            trooper = trooperPool[0];
            if (trooper.gameObject.activeSelf)
                Debug.LogError("Error, bullet already active");
            trooper.gameObject.SetActive(true);
            trooperPool.Remove(trooper);
        }
        else
        {
            Debug.Log("No more troopers available");
        }
        return trooper;
    }

    public void ReturnTrooper(TrooperController trooperController)
    {
        // Debug.Log("Returning bullet");
        trooperPool.Add(trooperController);
        trooperController.gameObject.SetActive(false);
    }

    #endregion

    //
    void UpdateCommandPoints(float dt)
    {
        //
        playerCommandPoints += dt;
        aiCommandPoints += dt;
        //
        playerCommandPoints = Mathf.Min(100, playerCommandPoints);
        aiCommandPoints = Mathf.Min(100, aiCommandPoints);
    }

    //
    void UpdateBuildingsOwnership()
    {
        //
        redBuildings.Clear();
        blueBuildings.Clear();
        greyBuildings.Clear();
        //
        currentPlayerResourceRate = playerBaseResourceRate;
        currentAiResourceRate = aiBaseResourceRate;
        //
        for (int i = 0; i < levelBuildings.Length; i++)
        {
            switch (levelBuildings[i].CurrentOwner)
            {
                case Owner.Red:
                    redBuildings.Add(levelBuildings[i]);
                    currentAiResourceRate += levelBuildings[i].productionBonus;
                    break;
                case Owner.Blue:
                    blueBuildings.Add(levelBuildings[i]);
                    currentPlayerResourceRate += levelBuildings[i].productionBonus;
                    break;
                case Owner.None: greyBuildings.Add(levelBuildings[i]); break;
            }
        }
        // TODO: Actualizar efectos a nivel de jugador
    }

    public void ChangeGameSpeed(int direction)
    {
        currentTimeScale += direction * 0.2f;
        currentTimeScale = Mathf.Clamp(currentTimeScale, 0.2f, 3);
        //
        Time.timeScale = currentTimeScale;
        Time.fixedDeltaTime = 0.02f * currentTimeScale;
    }

    void CheckVictory()
    {
        if(redBuildings.Count == 0) { matchEnded = true; playerWon = true; }
        if(blueBuildings.Count == 0) { matchEnded = true;  }
    }

    public int GetMaxActiveTroopers(Owner team)
    {
        switch (team)
        {
            case Owner.Blue: return MaxTroopersForBlueTeam;
            case Owner.Red: return MaxTroopersForRedTeam;
        }
        //
        return 0;
    }
    #region Order Methods
    /// <summary>
    /// Gestiona la orden dada por el player
    /// </summary>
    /// <param name="senderBuilding"></param>
    public bool ManageOrder(Building senderBuilding)
    {
        //
        Debug.Log("Managing order");
        if (preSelectedBuilding != null && preSelectedBuilding != senderBuilding)
        {
            //
            if (CanSendThere(preSelectedBuilding, senderBuilding))
            {
                // TODO: Gestinar aqui el arrastrado
                // Colocar el line renderer
                preSelectedBuilding.SupplyTo = senderBuilding;
                //
                preSelectedBuilding.SupplyLineRenderer.SetPosition(1, senderBuilding.transform.position);
                //
                preSelectedBuilding = null;
                selectedBuilding = null;
            }
            else
            {
                //
                preSelectedBuilding.SupplyLineRenderer.SetPosition(1, preSelectedBuilding.transform.position);
                //
                preSelectedBuilding = null;
                selectedBuilding = null;
                // Y sondio de rror
                audioSource.clip = errorClip;
                audioSource.Play();
                
            }
        }
        //
        else if(selectedBuilding != null)
        {
            //
            //Debug.Log("Selection status: PS " + preSelectedBuilding + ", S " + selectedBuilding + ", Se " + senderBuilding);

            if (CanSendThere(selectedBuilding, senderBuilding))
            {
                //
                selectedBuilding.SendTroops(senderBuilding);
                selectedBuilding = null;
            }
        }
        //
        return true;
    }

    //
    public bool CanSendThere(Building startBuidling, Building endBuilding)
    {
        //
        // TODO: Que ignore la layer de los cadáveres
        int layerMask = 1 << 9;
        layerMask = ~layerMask;
        //
        Vector3 checkDirection = endBuilding.transform.position - startBuidling.transform.position;
        //
        if (Physics.Raycast(startBuidling.transform.position, checkDirection, out RaycastHit hitInfo, checkDirection.magnitude, layerMask))
        {
            //Debug.Log("Hit in: " + hitInfo.collider.name);
            Building impactedBuilding = hitInfo.collider.GetComponent<Building>();
            if (impactedBuilding == endBuilding)
            {
                //
                return true;
            }
            //else
            //{
            //    Debug.Log("Hit in: " + hitInfo.collider);
            //}
        }
        // Si llega hasta aqui rertorna false y ya
        return false;
    }

    #endregion

    // Estableceremos zonas de conexion para que se enfrenten en 
    //void StablishConnectionZones()
    //{

    //}

    #region General Zone Methods
    // O zona general para todo bicho que no esté en un edificio
    void StablishBuildlessZone()
    {
        //
        currentBlueTroopers = new List<TrooperController>(maxActiveTroopers);
        currentRedTroopers = new List<TrooperController>(maxActiveTroopers);
    }

    public void EnterBuildlessZone(TrooperController enteringTrooper)
    {
        switch (enteringTrooper.Team)
        {
            case Owner.Blue: currentBlueTroopers.Add(enteringTrooper); break;
            case Owner.Red: currentRedTroopers.Add(enteringTrooper); break;
        }
    }

    public void ExitBuildlessZone(TrooperController exitingTrooper)
    {
        switch (exitingTrooper.Team)
        {
            case Owner.Blue: currentBlueTroopers.Remove(exitingTrooper); break;
            case Owner.Red: currentRedTroopers.Remove(exitingTrooper); break;
        }
    }

    /// <summary>
    /// Para la zona abierta
    /// </summary>
    /// <param name="trooperWhoSearch"></param>
    /// <returns></returns>
    public TrooperController GetNearestTrooper(Owner team, Vector3 searcherPosition)
    {
        //
        TrooperController nearestTrooper = null;
        List<TrooperController> troopersToSearch = null;
        float closestDistance = Mathf.Infinity;
        //
        switch (team)
        {
            case Owner.Blue: troopersToSearch = currentRedTroopers; break;
            case Owner.Red: troopersToSearch = currentBlueTroopers; break;
        }
        //
        for (int i = 0; i < troopersToSearch.Count; i++)
        {
            //
            float currentTrooperDistance = (searcherPosition - troopersToSearch[i].transform.position).sqrMagnitude;
            //
            if (nearestTrooper == null || currentTrooperDistance < closestDistance)
            {
                // Vamos a ahacer aqui la coña
                // Con esto debería variar la selección de objetivos cercanos
                // Vamos a darle un margen de un metro
                //if (Mathf.Abs(currentTrooperDistance - closestDistance) < 5)
                //{
                //    float pitoPito = Random.value;
                //    if (pitoPito < 0.8)
                //        continue;
                //}
                //
                nearestTrooper = troopersToSearch[i];
                closestDistance = currentTrooperDistance;
            }
        }

        return nearestTrooper;
    }

    // Todo: Buscar con parámetro de alcance del trooper
    public TrooperController GetFirstTrooperInRange(Owner team, Vector3 searcherPosition, WeaponStats weaponStats)
    {
        //
        TrooperController nearestTrooper = null;
        List<TrooperController> troopersToSearch = null;
        //
        switch (team)
        {
            case Owner.Blue: troopersToSearch = currentRedTroopers; break;
            case Owner.Red: troopersToSearch = currentBlueTroopers; break;
        }
        //
        for (int i = 0; i < troopersToSearch.Count; i++)
        {
            //
            float currentTrooperDistance = (searcherPosition - troopersToSearch[i].transform.position).sqrMagnitude;
            //
            if (currentTrooperDistance < Mathf.Pow(weaponStats.attackReach, 2))
            {
                // 
                return troopersToSearch[i];
            }
        }

        return nearestTrooper;
    }

    #endregion

    public int GetTotalActiveTroopersOfTeam(Owner owner)
    {
        int totalActiveTroopers = 0;
        //
        switch (owner)
        {
            case Owner.Blue:
                // Primero de la zona general
                totalActiveTroopers += currentBlueTroopers.Count;
                // Y por cada edificio
                for(int i = 0; i < levelBuildings.Length; i++)
                {
                    totalActiveTroopers += levelBuildings[i].CurrentBlueTroopers.Count;
                }
                break;
            case Owner.Red:
                // Primero de la zona general
                totalActiveTroopers += currentRedTroopers.Count;
                // Y por cada edificio
                for (int i = 0; i < levelBuildings.Length; i++)
                {
                    totalActiveTroopers += levelBuildings[i].CurrentRedTroopers.Count;
                }
                break;
        }
        //
        return totalActiveTroopers;
    }

    #region Coroutines

    //
    IEnumerator CheckBuildingOwnership()
    {
        //
        // TODO: QUe pare cuando acabe la partida
        while (!matchEnded)
        {
            yield return new WaitForSeconds(timeBetweenChecks);
            UpdateBuildingsOwnership();
            CheckVictory();
        }
    }

    #endregion

    #region Damage Info Methods

    public void SendDamageInfo(AttackDamageInfo newAttackDamageInfo)
    {
        attackDamageInfoList.Add(newAttackDamageInfo);
    }

    void UpdateDamageInfo(float dt)
    {
        //
        for (int i = 0; i < attackDamageInfoList.Count; i++)
        {
            attackDamageInfoList[i].activeTime += dt;
            if (attackDamageInfoList[i].activeTime > 2)
                attackDamageInfoList.RemoveAt(i);
        }
    }

    #endregion

    public void PlayAbilityClip(AudioClip clip)
    {
        if(clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    #region Statistics Methods

    //
    void InitiateMatchStatistics()
    {
        //
        matchStatistics = new MatchStatistics();
        matchStatistics.producedTroopersInMatch = new List<TrooperStats>(5);
        // Pasamos por todos los edificios para ver las diferentes tropas que entran en partida
        for(int i = 0; i < levelBuildings.Length; i++)
        {
            if (!matchStatistics.producedTroopersInMatch.Contains(levelBuildings[i].producedTrooper) &&
                levelBuildings[i].producedTrooper != null)
                matchStatistics.producedTroopersInMatch.Add(levelBuildings[i].producedTrooper);
        }
        // Debugueo
        //for (int i = 0; i < matchStatistics.producedTroopersInMatch.Count; i++)
        //{
        //    if (matchStatistics.producedTroopersInMatch[i].icon != null)
        //        Debug.Log(matchStatistics.producedTroopersInMatch[i]);
        //    else
        //        Debug.Log("Algo mal");
        //}

        //
        matchStatistics.deathStatsPerTroop = 
            new int[matchStatistics.producedTroopersInMatch.Count][];
        //
        matchStatistics.deathStatsPerWeapon =
            new int[matchStatistics.producedTroopersInMatch.Count];
        //
        for (int i = 0; i < matchStatistics.deathStatsPerTroop.Length; i++){
            matchStatistics.deathStatsPerTroop[i] =
            new int[matchStatistics.producedTroopersInMatch.Count];
        }
        //
        matchStatistics.generalStatsPerPlayer = new StatsPerPlayer[2];
        matchStatistics.generalStatsPerPlayer[0] = new StatsPerPlayer();
        matchStatistics.generalStatsPerPlayer[1] = new StatsPerPlayer();
    }

    //
    public void AnotateProduction(TrooperController producedTrooper)
    {
        switch (producedTrooper.Team)
        {
            case Owner.Blue: matchStatistics.generalStatsPerPlayer[0].producedTroops++; break;
            case Owner.Red: matchStatistics.generalStatsPerPlayer[1].producedTroops++;  break;
        }
    }

    /// <summary>
    /// En principio la victima tiene que ser trooper si o si
    /// </summary>
    /// <param name="killer"></param>
    /// <param name="victim"></param>
    public void AnotateKill(/*TrooperController killer*/ int killerType, TrooperController victim)
    {
        //
        //if(killer != null)
        //{
            // int killerIndex = matchStatistics.producedTroopersInMatch.IndexOf(killer.trooperStats);
            int killerIndex = killerType;
            int victimIndex = matchStatistics.producedTroopersInMatch.IndexOf(victim.trooperStats);
            //
            if (killerIndex >= 0 && victimIndex >= 0)
            {
                //
                matchStatistics.deathStatsPerTroop[killerIndex][victimIndex]++;
            }
            
        //}
        
        // TODO: Cuando haya mas equipos habrá qye desarrollar esto
        switch (victim.Team)
        {
            case Owner.Red:
                matchStatistics.generalStatsPerPlayer[0].kills++;
                matchStatistics.generalStatsPerPlayer[1].losses++;
                break;
            case Owner.Blue:
                matchStatistics.generalStatsPerPlayer[1].kills++;
                matchStatistics.generalStatsPerPlayer[0].losses++;
                break;
        }
    }

    public void AnotateKill(FixedWeapon killer, TrooperController victim)
    {
        // TODO: Esto lo abordaremos mas tarde
        if (killer != null)
        {
            //int killerIndex = matchStatistics.producedTroopersInMatch.IndexOf(killer.trooperStats);
            int victimIndex = matchStatistics.producedTroopersInMatch.IndexOf(victim.trooperStats);
            //
            if(victimIndex >= 0)
                matchStatistics.deathStatsPerWeapon/*[killerIndex]*/[victimIndex]++;
        }

        //
        switch (killer.WeaponBuilding.CurrentOwner)
        {
            case Owner.Blue:
                matchStatistics.generalStatsPerPlayer[0].kills++;
                matchStatistics.generalStatsPerPlayer[1].losses++;
                break;
            case Owner.Red:
                matchStatistics.generalStatsPerPlayer[1].kills++;
                matchStatistics.generalStatsPerPlayer[0].losses++;
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="victim"></param>
    public void AnotateExplosionKill(TrooperController victim)
    {
        //
        switch (victim.Team)
        {
            case Owner.Red:
                matchStatistics.generalStatsPerPlayer[0].kills++;
                matchStatistics.generalStatsPerPlayer[1].losses++;
                break;
            case Owner.Blue:
                matchStatistics.generalStatsPerPlayer[1].kills++;
                matchStatistics.generalStatsPerPlayer[0].losses++;
                break;
        }
    }

    //
    void ShowMatchStatistics()
    {
        //
        GUI.Label(new Rect(200, 100, 100, 60), "Prod", uISkin.label);
        GUI.Label(new Rect(260,  100, 100, 60), "Kill", uISkin.label);
        GUI.Label(new Rect(320, 100, 100, 60), "Loss", uISkin.label);
        //
        for (int i = 0; i < matchStatistics.generalStatsPerPlayer.Length; i++)
        {
            GUI.Label(new Rect(100, i * 60 + 160, 100, 60), "Player " + (i+1), uISkin.label);
            GUI.Label(new Rect(200, i * 60 + 160, 60, 60), matchStatistics.generalStatsPerPlayer[i].producedTroops+"", uISkin.label);
            GUI.Label(new Rect(260, i * 60 + 160, 60, 60), matchStatistics.generalStatsPerPlayer[i].kills + "", uISkin.label);
            GUI.Label(new Rect(320, i * 60 + 160, 60, 60), matchStatistics.generalStatsPerPlayer[i].losses + "", uISkin.label);
        }
        //
        for (int i = 0; i < matchStatistics.producedTroopersInMatch.Count; i++)
        {
            GUI.DrawTexture(new Rect(Screen.width / 2 + (60 * i) + 200, 100, 60, 60), matchStatistics.producedTroopersInMatch[i].icon);
        }
        //
        for (int i = 0; i < matchStatistics.deathStatsPerTroop.Length; i++)
        {
            //
            if(i < matchStatistics.producedTroopersInMatch.Count)
                GUI.DrawTexture(new Rect((Screen.width / 2) + 100, 60 * i + 200, 60, 60), matchStatistics.producedTroopersInMatch[i].icon);
            //
            for (int j = 0; j < matchStatistics.deathStatsPerTroop[i].Length; j++)
            {
                GUI.Label(new Rect((Screen.width / 2) + (60 * i) + 200, 60 * j + 200, 60, 60), 
                    matchStatistics.deathStatsPerTroop[i][j]+"", uISkin.label);
            }
        }
        // Extra para las armas fijas
        //GUI.DrawTexture(new Rect((Screen.width / 2) + 100, 60 * i + 200, 60, 60), );
        float fixedWeaponHeight = matchStatistics.deathStatsPerTroop.Length;
        for (int i = 0; i < matchStatistics.deathStatsPerWeapon.Length; i++)
        {
            GUI.Label(new Rect((Screen.width / 2) + (60 * i) + 200, 60 * fixedWeaponHeight + 200, 60, 60),
                    matchStatistics.deathStatsPerWeapon[i] + "", uISkin.label);
        }
    }

    #endregion
}

public class MatchStatistics
{
    // TODO: Meteremos esto para que se vean los resultados de la batalla
    public int[][] deathStatsPerTroop;
    public int[] deathStatsPerWeapon; // De momento solo una dimensión
    public StatsPerPlayer[] generalStatsPerPlayer;

    public List<TrooperStats> producedTroopersInMatch;
}

public class StatsPerPlayer
{
    public int producedTroops;
    public int kills;
    public int losses;
}

public class AttackDamageInfo
{
    public Vector3 damageWorldPosition;
    public int damageDone;

    public float activeTime;

    public AttackDamageInfo(Vector3 damageWorldPosition, int damageDone)
    {
        this.damageWorldPosition = damageWorldPosition;
        this.damageDone = damageDone;
    }
}

public class Platoon
{
    public Owner team;
    public List<TrooperController> troopers;

    public Platoon(Owner team, List<TrooperController> troopers)
    {
        this.team = team;
        this.troopers = troopers;
    }
}