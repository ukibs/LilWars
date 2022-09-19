using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public enum Owner
{
    None,
    Red,
    Blue,

    Count
}

/// <summary>
/// 
/// </summary>
public class Building : MonoBehaviour
{
    public Owner initialOwner;
    public Transform spawnPoint;
    //public GameObject troopPrefab;
    //public float timeToSpawnTroop = 1;
    public float productionBonus = 0;
    public int baseDefenseBonus = 0;
    public int basePopulationCapacity = 20;
    
    public BuildingMaterials buildingMaterials;
    public Flags flags;

    public TrooperStats producedTrooper;
    public TrooperStats[] possibleTroopsToProduce;
    public Texture productionIcon;
    public Texture defenseIcon;
    public Texture greenTexture;
    public GUISkin uISkin;

    public Material neighbourMaterial;

    private float currentTimeToSpawnTroop = 0;
    private LevelController levelController;
    private PlayerInterface playerInterface;

    private Building supplyTo;

    //
    private List<TrooperController> currentBlueTroopers;
    private List<TrooperController> currentRedTroopers;
    private Owner currentOwner;
    private Possesion possesion;

    //private GameObject supplyLine;
    private LineRenderer supplyLineRenderer;

    //
    private List<BuildingImprovement> buildingImprovements;

    //
    private List<Building> buildingsOnSight;
    private List<GameObject> buildingsOnSightLineRenderers;

    public List<TrooperController> CurrentBlueTroopers
    {
        get { return currentBlueTroopers; }
    }
    public List<TrooperController> CurrentRedTroopers
    {
        get { return currentRedTroopers; }
    }

    public List<BuildingImprovement> BuildingImprovementList { get { return buildingImprovements; } }

    public Owner CurrentOwner { get { return currentOwner; } }

    public Building SupplyTo { 
        set { supplyTo = value; } 
        get { return supplyTo; }
    }

    //public GameObject SupplyLine {
    //    set { supplyLine = value; }
    //    get { return supplyLine; }
    //}

    public LineRenderer SupplyLineRenderer
    {
        set { supplyLineRenderer = value; }
        get { return supplyLineRenderer; }
    }

    // TODO: Añadir chequeo y suma de defensa extra
    public int DefenseBonus { 
        get {
            int extraDefenseBonus = CheckIfActiveImprovement(ImprovementEffect.ExtraDefense) ? 1 : 0;
            return baseDefenseBonus + extraDefenseBonus; 
        } 
    }

    public int PopulationCapacity
    {
        get
        {
            int extraPopulationCapacity = CheckIfActiveImprovement(ImprovementEffect.ExtraStorage) ? 10 : 0;
            return basePopulationCapacity + extraPopulationCapacity;
        }
    }

    public List<Building> BuildingsOnSight { get { return buildingsOnSight; } }

    // Start is called before the first frame update
    void Start()
    {
        levelController = FindObjectOfType<LevelController>();
        playerInterface = FindObjectOfType<PlayerInterface>();

        currentBlueTroopers = new List<TrooperController>(100);
        currentRedTroopers = new List<TrooperController>(100);

        possesion = new Possesion();

        //
        supplyLineRenderer = GetComponentInChildren<LineRenderer>();
        supplyLineRenderer.positionCount = 2;
        supplyLineRenderer.SetPosition(0, transform.position);
        supplyLineRenderer.SetPosition(1, transform.position);

        InitializeBuilding();

        // TODO: Poner los marcadores de vecinos
    }

    // Update is called once per frame
    void Update()
    {
        //
        if (levelController.MatchEnded) return;
        //
        float dt = Time.deltaTime;
        //
        UpdateProduction(dt);

        //
        UpdateStatus(dt);
        //
        if (levelController.PreSelectedBuilding != null && levelController.PreSelectedBuilding == this)
        {
            //
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Building building = hitInfo.transform.GetComponent<Building>();
                if(building == null || building != this)
                    supplyLineRenderer.SetPosition(1, hitInfo.point);
            }            
        }
        //
        //if(troopPrefab == null && supplyTo != null)
        //{
        //    switch (currentOwner)
        //    {

        //    }
        //}

        // Vamos a manejar aqui la construccion
        ManageConstructionOfImprovements();

        // Que mande tropas al abastecido
        // Recuerda que son solo azules con esto
        if(supplyTo != null && currentBlueTroopers.Count > 0)
        {
            SendTroops(supplyTo);
        }
    }

    // Para el player
    // TODO: Mover a player interface
    //private void OnMouseDown()
    //{
    //    // Opción para generar lineas de abastecimiento y cosas
    //    // De momento solo entre azules
    //    if (currentOwner == Owner.Blue)
    //    {
    //        levelController.PreSelectedBuilding = this;
    //    }

    //}

    // TODO: Mover a player interface
    //private void OnMouseUp()
    //{
    //    // TODO: 
    //    if (levelController.MatchEnded/* || playerInterface.AbilitySelected*/) return;
    //    // Que lo sleccione según la cosa
    //    if (currentOwner == Owner.Blue)
    //    {
    //        // TODO: Que ignore la layer de los cadáveres
    //        int layerMask = 1 << 9;
    //        layerMask = ~layerMask;
    //        //
    //        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //        if (Physics.Raycast(ray, out RaycastHit hitInfo))
    //        {
    //            Building hitBuilding = hitInfo.collider.GetComponent<Building>();
    //            if (hitBuilding != null && hitBuilding != this)
    //            {
    //                //Debug.Log("Estableciendo linea de suministro");
    //                levelController.ManageOrder(hitBuilding);
    //                return;
    //            }
    //            //
    //            else
    //            {
    //                supplyTo = null;
    //                supplyLineRenderer.SetPosition(1, transform.position);
    //            }
    //        }

    //        //Debug.Log("Proccesing selection of " + gameObject.name);
    //        if (levelController.SelectedBuilding == null) levelController.SelectedBuilding = this;
    //        else if (levelController.SelectedBuilding == this) levelController.SelectedBuilding = null;
    //        else levelController.ManageOrder(this);
    //    }
    //    else
    //    {
    //        levelController.ManageOrder(this);
    //    }

    //    // Se cual sea el caso pre seleccionado fuera
    //    //if (levelController.PreSelectedBuilding == this) levelController.PreSelectedBuilding = null;
    //    levelController.PreSelectedBuilding = null;
    //    //
    //    if (supplyTo == null)
    //        supplyLineRenderer.SetPosition(1, transform.position);
    //}

    //
    private void OnGUI()
    {
        //
        Vector3 positionInScreen = Camera.main.WorldToScreenPoint(transform.position);
        //
        if(producedTrooper != null)
        {
            //Debug.Log(producedTrooper);
            GUI.DrawTexture(new Rect(positionInScreen.x - 25, Screen.height - positionInScreen.y - 75, 50, 50), producedTrooper.icon);
            // Progreso de producción
            GUI.DrawTexture(new Rect(positionInScreen.x - 25, Screen.height - positionInScreen.y - 25,
                currentTimeToSpawnTroop / producedTrooper.cost * 50, 10), greenTexture);
        }
        //
        if (productionBonus > 0)
        {
            GUI.DrawTexture(new Rect(positionInScreen.x - 25, Screen.height - positionInScreen.y - 75, 50, 50), productionIcon);
            GUI.Label(new Rect(positionInScreen.x - 25, Screen.height - positionInScreen.y - 25, 50, 50), productionBonus+"", uISkin.label);
        }
        //
        GUI.Label(new Rect(positionInScreen.x - 50, Screen.height - positionInScreen.y - 100, 50, 50), 
            currentBlueTroopers.Count + "", uISkin.customStyles[0]);
        GUI.Label(new Rect(positionInScreen.x + 0, Screen.height - positionInScreen.y - 100, 50, 50),
            currentRedTroopers.Count + "", uISkin.customStyles[1]);
        // Ahora va de otra forma
        //if(DefenseBonus > 0)
        //{
        //    GUI.DrawTexture(new Rect(positionInScreen.x - 25, Screen.height - positionInScreen.y - 125, 50, 50), defenseIcon);
        //    GUI.Label(new Rect(positionInScreen.x - 25, Screen.height - positionInScreen.y - 125, 50, 50), DefenseBonus + "", uISkin.label);
        //}
        
    }

    // TODO: Revisar esto
    private void OnDrawGizmos()
    {
        //
        if (Application.isEditor)
        {
            //
            Gizmos.color = Color.blue;
            //
            for (int i = 0; i < currentBlueTroopers.Count; i++)
            {
                Gizmos.DrawLine(transform.position, currentBlueTroopers[i].transform.position);
            }

            //
            Gizmos.color = Color.red;
            //
            for (int i = 0; i < currentRedTroopers.Count; i++)
            {
                Gizmos.DrawLine(transform.position, currentRedTroopers[i].transform.position);
            }
        }
       }

        //
        void InitializeBuilding()
    {
        //
        currentOwner = initialOwner;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        //
        switch (initialOwner)
        {
            case Owner.None:
                meshRenderer.material = buildingMaterials.greyMaterial;
                flags.redFlag.localPosition = new Vector3(0, -0.51f, 0.25f);
                flags.blueFlag.localPosition = new Vector3(0, -0.51f, -0.25f);
                break;
            case Owner.Blue:
                meshRenderer.material = buildingMaterials.blueMaterial;
                flags.redFlag.localPosition = new Vector3(0, -0.51f, 0.25f);
                possesion.blue = 100;
                break;
            case Owner.Red:
                meshRenderer.material = buildingMaterials.readMaterial;
                flags.blueFlag.localPosition = new Vector3(0, -0.51f, -0.25f);
                possesion.red = 100;
                break;
        }

        // Marcadores de posibles vecinos
        buildingsOnSight = new List<Building>(5);
        buildingsOnSightLineRenderers = new List<GameObject>(5);
        for(int i = 0; i < levelController.LevelBuildings.Length; i++)
        {
            if (levelController.CanSendThere(this, levelController.LevelBuildings[i]))
            {
                // TODO: Mirar también a ver si filtramos aparte los que están lo bastante cerca
                buildingsOnSight.Add(levelController.LevelBuildings[i]);
                GameObject neighbourhoodLineRendererObject = Instantiate(supplyLineRenderer.gameObject, transform.position, Quaternion.identity);
                LineRenderer neighbourhoodLineRenderer = neighbourhoodLineRendererObject.GetComponent<LineRenderer>();
                neighbourhoodLineRenderer.material = neighbourMaterial;
                neighbourhoodLineRenderer.positionCount = 2;
                neighbourhoodLineRenderer.SetPosition(0, transform.position);
                neighbourhoodLineRenderer.SetPosition(1, levelController.LevelBuildings[i].transform.position);
                buildingsOnSightLineRenderers.Add(neighbourhoodLineRendererObject);
                //
                neighbourhoodLineRendererObject.SetActive(false);
            } 
        }
    }

    #region Improvement Methods

    public void AddImprovement(BuildingImprovement buildingImprovement)
    {
        //
        if (buildingImprovements == null)
            buildingImprovements = new List<BuildingImprovement>(5);
        // TODO: Cehquear el tope de mejoras
        // De momento 3
        //
        buildingImprovements.Add(buildingImprovement);
    }

    void ActivateImprovement(int index)
    {
        buildingImprovements[index].Activate();
    }

    public void StartImprovementConstruction(int index)
    {
        buildingImprovements[index].StartConstruction();
    }

    bool CheckIfActiveImprovement(ImprovementEffect improvementEffect)
    {
        //bool hasActiveImprovement = false;

        //
        for(int i = 0; i < buildingImprovements.Count; i++)
        {
            if (buildingImprovements[i].improvementEffect == improvementEffect &&
                buildingImprovements[i].gameObject.activeSelf)
                return true;
        }

        return false;
    }

    void ManageConstructionOfImprovements()
    {
        //
        int currentTroopers = 0;
        switch (currentOwner)
        {
            case Owner.Blue: currentTroopers = currentBlueTroopers.Count; break;
            case Owner.Red: currentTroopers = currentRedTroopers.Count; break;
        }
        if (currentTroopers == 0)
            return;
        //
        for(int i = 0; i < buildingImprovements.Count; i++)
        {
            if(buildingImprovements[i].ConstructionStatus == BuildingImprovement.Status.OnConstruction)
            {
                buildingImprovements[i].Progression++;
                TrooperController trooperToUse;
                //
                switch (CurrentOwner)
                {
                    case Owner.Blue:
                        //Destroy(currentBlueTroopers[0].gameObject);
                        trooperToUse = currentBlueTroopers[0];
                        levelController.ReturnTrooper(trooperToUse);
                        currentBlueTroopers.Remove(trooperToUse);                        
                        break;
                    case Owner.Red:
                        //Destroy(currentRedTroopers[0].gameObject);
                        trooperToUse = currentRedTroopers[0];
                        levelController.ReturnTrooper(trooperToUse);
                        currentRedTroopers.Remove(trooperToUse);                        
                        break;
                }
                //
                if (buildingImprovements[i].Progression >= buildingImprovements[i].cost)
                {
                    ActivateImprovement(i);
                }
                // Una vez acabamos la pasada paramos
                // Solo una pasada por tic
                return;
            }
        }
    }

    #endregion

    void GetXCheapestOnes(int amount){}

    /// <summary>
    /// Este lo llamaremos desde el player interface
    /// </summary>
    /// <param name="yesNo"></param>
    public void ShowNeighbourLines(bool yesNo)
    {
        //
        for(int i = 0; i < buildingsOnSightLineRenderers.Count; i++)
        {
            buildingsOnSightLineRenderers[i].SetActive(yesNo);
        }
    }

    void UpdateProduction(float dt)
    {
        //
        if (currentOwner != Owner.None && producedTrooper != null && !CheckIfEnemyTroops())
        {
            //
            float ownerMultiplier = 1;
            switch (currentOwner)
            {
                case Owner.Blue: ownerMultiplier = levelController.PlayerResourceRate; break;
                case Owner.Red: ownerMultiplier = levelController.AiResourceRate; break;
            }
            //
            float productionBonus = CheckIfActiveImprovement(ImprovementEffect.ExtraEnergy) ? 0.2f : 0;
            // TODO: Chequear y sumar bonus de energía
            currentTimeToSpawnTroop += dt * (ownerMultiplier + productionBonus);
            if (currentTimeToSpawnTroop >= producedTrooper.cost)
            {
                // 
                int currentAmountOfTroops = levelController.GetTotalActiveTroopersOfTeam(currentOwner);
                if (currentAmountOfTroops < levelController.GetMaxActiveTroopers(currentOwner))
                {
                    SpawnTroop();
                }
                else
                {
                    //Debug.Log("Limite de tropas alcanzado - " + currentOwner + ", " + currentAmountOfTroops);
                    //Debug.Log("Limite de tropas alcanzado - " + currentOwner);
                }
                //
                currentTimeToSpawnTroop -= producedTrooper.cost;
            }
        }
    }

    void UpdateStatus(float dt)
    {
        // TODO: Hacer que se tengan que acercar para toamrlo

        //
        switch (currentOwner)
        {
            case Owner.None:
                //
                if (currentRedTroopers.Count > 0 && currentBlueTroopers.Count == 0)
                {
                    //
                    if(possesion.blue > 0)
                        possesion.blue -= TroopersInTakingRange(Owner.Red) * dt;
                    //
                    else
                    {
                        possesion.red += TroopersInTakingRange(Owner.Red) * dt;
                        if (possesion.red >= 100) ChangeOwner(Owner.Red);
                    }
                    
                }
                //
                if (currentBlueTroopers.Count > 0 && currentRedTroopers.Count == 0)
                {
                    //
                    if (possesion.red > 0)
                        possesion.red -= TroopersInTakingRange(Owner.Blue) * dt;
                    //
                    else
                    {
                        possesion.blue += TroopersInTakingRange(Owner.Blue) * dt;
                        if (possesion.blue >= 100) ChangeOwner(Owner.Blue);
                    }
                }
                break;
            case Owner.Blue:
                if(currentRedTroopers.Count > 0 && currentBlueTroopers.Count == 0)
                {
                    possesion.blue -= TroopersInTakingRange(Owner.Red) * dt;
                    //
                    if (possesion.blue <= 0) ChangeOwner(Owner.None);
                }
                break;
            case Owner.Red:
                if (currentBlueTroopers.Count > 0 && currentRedTroopers.Count == 0)
                {
                    possesion.red -= TroopersInTakingRange(Owner.Blue) * dt;
                    //
                    if (possesion.red <= 0) ChangeOwner(Owner.None);
                }
                break;
        }
        // Clampeamos aqui
        possesion.red = Mathf.Clamp(possesion.red, 0, 100);
        possesion.blue = Mathf.Clamp(possesion.blue, 0, 100);
        // Altura banderas
        flags.redFlag.localPosition  = new Vector3(0, -(100 - possesion.red)  / 200, 0.25f);
        flags.blueFlag.localPosition = new Vector3(0, -(100 - possesion.blue) / 200, -0.25f);
    }

    /// <summary>
    /// Bonus a producción desde habilidad
    /// En vez de spameo bruto de tropas
    /// Para no abusar del spameo de elites
    /// </summary>
    /// <param name="amount"></param>
    public void AddProductionFromAbility(float amount)
    {
        currentTimeToSpawnTroop += amount;
    }

    /// <summary>
    /// Lo hacemos publixo para poder ser llamado por una habilidad
    /// </summary>
    public void SpawnTroop()
    {
        //
        float radius = Random.Range(0, 10);
        float angle = Random.Range(0, 360);
        //
        float xPos = radius * Mathf.Cos(angle);
        float zPos = radius * Mathf.Sin(angle);
        //
        //GameObject newTroop = Instantiate(producedTrooper.prefab, 
        //    transform.position + new Vector3(xPos, 0, zPos)/*spawnPoint.position*/, Quaternion.identity);
        //TrooperController trooperController = newTroop.GetComponent<TrooperController>();
        //trooperController.CurrentBuilding = transform;
        //trooperController.Team = currentOwner;
        //trooperController.Initiate(producedTrooper);
        //
        TrooperController newTrooper = levelController.GetTrooper();
        //
        if (!newTrooper)
            return;
        //
        newTrooper.transform.position = transform.position + new Vector3(xPos, 0, zPos);
        newTrooper.CurrentBuilding = transform;
        newTrooper.Team = currentOwner;
        newTrooper.Initiate(producedTrooper);
        //
        switch (currentOwner)
        {
            case Owner.Blue: currentBlueTroopers.Add(newTrooper); break;
            case Owner.Red: currentRedTroopers.Add(newTrooper); break;
        }
        // Esto lo gestionamos ahora en otro lado
        //if (supplyTo != null)
        //{
        //    SendTroops(supplyTo);
        //}
        //
        levelController.AnotateProduction(newTrooper);
    }

    public void SendTroops(Building objectiveBuilding)
    {
        //
        List<TrooperController> troopersToSend = new List<TrooperController>();
        switch (currentOwner)
        {
            case Owner.Blue: troopersToSend = currentBlueTroopers; break;
            case Owner.Red:  troopersToSend = currentRedTroopers;  break;
        }
        // Generamos nuevo peloton
        // TODO: Mirar cuando hay que destruirlo
        //levelController.platoons.Add(new Platoon(currentOwner, troopersToSend));
        //
        for(int i = 0; i < troopersToSend.Count; i++)
        {
            troopersToSend[i].SetNewDestination(objectiveBuilding);
        }
        //
        troopersToSend.Clear();
    }

    public void ArriveTroop(TrooperController trooperController)
    {
        //
        switch (trooperController.Team)
        {
            case Owner.Blue: currentBlueTroopers.Add(trooperController); break;
            case Owner.Red:  currentRedTroopers.Add(trooperController);  break;
        }
    }

    void ChangeOwner(Owner newOwner)
    {
        currentOwner = newOwner;
        // TODO: Que cambie al material adecuado
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        switch (currentOwner)
        {
            case Owner.None: meshRenderer.material = buildingMaterials.greyMaterial; break;
            case Owner.Blue: meshRenderer.material = buildingMaterials.blueMaterial; break;
            case Owner.Red:  meshRenderer.material = buildingMaterials.readMaterial; break;
        }
        //
        supplyTo = null;
        supplyLineRenderer.SetPosition(1, transform.position);
    }

    public bool CheckIfEnemyTroops()
    {
        //
        switch (currentOwner)
        {
            case Owner.Blue: if (currentRedTroopers.Count > 0) return true; break;
            case Owner.Red: if (currentBlueTroopers.Count > 0) return true; break;
        }
        //
        return false;
    }

    /// <summary>
    /// El de edificio
    /// </summary>
    /// <param name="trooperWhoSearch"></param>
    /// <returns></returns>
    //public TrooperController GetNearestTrooper(BaseCombatant trooperWhoSearch)
    public TrooperController GetNearestTrooper(Owner searcherTeam, Vector3 searcherPosition)
    {
        //
        TrooperController nearestTrooper = null;
        List<TrooperController> troopersToSearch = null;
        float closestDistance = Mathf.Infinity;
        //
        switch (searcherTeam)
        {
            case Owner.Blue: troopersToSearch = currentRedTroopers; break;
            case Owner.Red: troopersToSearch = currentBlueTroopers; break;
        }
        // TODO: Mirar si hacemos que busque aqui de los demás edificios
        //
        for(int i = 0; i < troopersToSearch.Count; i++)
        {
            //
            float currentTrooperDistance = (searcherPosition - troopersToSearch[i].transform.position).sqrMagnitude;
            //
            if (nearestTrooper == null || currentTrooperDistance < closestDistance) {
                // Vamos a ahacer aqui la coña
                // Con esto debería variar la selección de objetivos cercanos
                // Vamos a darle un margen de un metro
                if (Mathf.Abs(currentTrooperDistance - closestDistance) < 5)
                {
                    float pitoPito = Random.value;
                    if (pitoPito < 0.8)
                        continue;
                }
                nearestTrooper = troopersToSearch[i];
                closestDistance = currentTrooperDistance;
            }
        }

        return nearestTrooper;
    }

    public TrooperController GetRandomTrooper(Owner searcherTeam)
    {
        //
        List<TrooperController> troopersToSearch = null;
        //
        switch (searcherTeam)
        {
            case Owner.Blue: troopersToSearch = currentRedTroopers; break;
            case Owner.Red: troopersToSearch = currentBlueTroopers; break;
        }
        // TODO: Mirar si hacemos que busque aqui de los demás edificios
        if (troopersToSearch.Count == 0)
            return null;
        else
            return troopersToSearch[Random.Range(0, troopersToSearch.Count)];
    }

    // TODO: Sacar primero en rango de arma

    public void RemoveTrooper(TrooperController trooperToRemove)
    {
        switch (trooperToRemove.Team)
        {
            case Owner.Blue: currentBlueTroopers.Remove(trooperToRemove); break;
            case Owner.Red: currentRedTroopers.Remove(trooperToRemove); break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    int TroopersInTakingRange(Owner owner)
    {
        int troopersInTakingRange = 0;
        List<TrooperController> troopersToCheck = new List<TrooperController>();
        //
        switch (owner)
        {
            case Owner.Blue: troopersToCheck = currentBlueTroopers; break;
            case Owner.Red: troopersToCheck = currentRedTroopers; break;
        }
        //
        for(int i = 0; i < troopersToCheck.Count; i++)
        {
            // Vamos a tomar 10 como distancia para tomar el edificio
            if((transform.position - troopersToCheck[i].transform.position).sqrMagnitude < 10 * 10)
            {
                troopersInTakingRange++;
            }
        }
        //
        return troopersInTakingRange;
    }

    #region Material Class

    [System.Serializable]
    public class BuildingMaterials
    {
        public Material greyMaterial;
        public Material readMaterial;
        public Material blueMaterial;
    }

    [System.Serializable]
    public class Flags
    {
        public Transform redFlag;
        public Transform blueFlag;
    }

    public class Possesion
    {
        public float blue;
        public float red;
    }

    #endregion
}
