using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInterface : MonoBehaviour
{
    //
    //public Texture[] abilityIcons;
    public BaseCommanderAbility[] commanderAbilities;
    //public Texture[] buildingOptionIcons;

    public GUISkin uISkin;

    public Texture selectionTexture;
    public Texture readinessTexture;
    public Texture builtOptionTexture;
    public Texture onConstructionTexture;
    public Texture changeIconTexture;

    public GameObject abilityMarkerObject;
    public AudioClip errorClip;

    //
    private LevelController levelController;
    private float buttonSize = 100;

    private int selectedOne = -1;
    private bool abilitySelected = false;

    //
    //private Build
    private bool showingTroopsToProduce = false;

    public bool AbilitySelected { get { return abilitySelected;  } }
    
    // Start is called before the first frame update
    void Start()
    {
        levelController = FindObjectOfType<LevelController>();
    }

    // Update is called once per frame
    void Update()
    {
        //
        CheckAbilityUse();
        
        //
        UpdateMouse();
        //
        //if (Input.GetMouseButtonUp(0))
        //{
        //    CheckMouseUp();
        //}

        //
        CheckAndChangeGameSpeed();
    }

    private void OnGUI()
    {
        //
        if (levelController.SelectedBuilding != null)
        {
            ShowBuildingOptionIcons();
        }
        else
        {
            ShowAbilityIcons();
        }
    }
    
    private void CheckMouseUp()
    {
        //
        //Debug.Log("Checking mouse click");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            //
            
            Building building = hitInfo.collider.GetComponent<Building>();
            if(building == null)
            {
                levelController.SelectedBuilding = null;
            }
        }
    }

    void CheckAndChangeGameSpeed()
    {
        if (Input.GetKeyUp(KeyCode.KeypadPlus))
        {
            levelController.ChangeGameSpeed(1);
        }
        if (Input.GetKeyUp(KeyCode.KeypadMinus))
        {
            levelController.ChangeGameSpeed(-1);
        }
    }

    void UpdateMouse()
    {
        // TODO: 
        if (levelController.MatchEnded/* || playerInterface.AbilitySelected*/) return;

        //
        if (abilitySelected)
        {
            UpdateAbilityMarker();
        }
            
        // AL pulsar el ziqueirdo del raton
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                //
                Building objetiveBuilding = hitInfo.collider.GetComponent<Building>();
                // Si tenemos una habildiad seleccionada hacemo esot
                if (abilitySelected)
                {
                    //
                    if (levelController.PlayerCommandPoints > commanderAbilities[selectedOne].cost)
                    {
                        
                        DataForAbility dataForAbility = new DataForAbility(levelController, hitInfo.point, Owner.Blue, objetiveBuilding);
                        commanderAbilities[selectedOne].UseAbility(dataForAbility);
                        levelController.PlayerCommandPoints -= commanderAbilities[selectedOne].cost;
                    }
                    else
                    {
                        // TODO: Ruidito de no listo
                        levelController.PlayAbilityClip(errorClip);
                    }
                    abilityMarkerObject.SetActive(false);
                    abilitySelected = false;
                    selectedOne = -1;
                }

                //
                else
                {
                    // Si no
                    if (objetiveBuilding != null && objetiveBuilding.CurrentOwner == Owner.Blue)
                    {
                        levelController.PreSelectedBuilding = objetiveBuilding;
                    }
                }
            }
        }

        // Al soltarlo
        if (Input.GetMouseButtonUp(0))
        {
            
            // Que lo sleccione según la cosa
            // TODO: Que ignore la layer de los cadáveres
            //int layerMask = 1 << 9;
            //layerMask = ~layerMask;
            //
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Building hitBuilding = hitInfo.collider.GetComponent<Building>();

                if (hitBuilding)
                {
                    if (hitBuilding.CurrentOwner == Owner.Blue)
                    {
                        // Para la orden de supply
                        if (hitBuilding != null && hitBuilding != levelController.PreSelectedBuilding)
                        {
                            //Debug.Log("Estableciendo linea de suministro");
                            levelController.ManageOrder(hitBuilding);
                            return;
                        }
                        // Selecciona edifioc
                        if (levelController.SelectedBuilding == null)
                        {
                            levelController.SelectedBuilding = levelController.PreSelectedBuilding;
                            levelController.SelectedBuilding.ShowNeighbourLines(true);
                        }
                            
                        // Deselecciona edificio
                        else if (levelController.SelectedBuilding == levelController.PreSelectedBuilding)
                        {
                            levelController.SelectedBuilding.ShowNeighbourLines(false);
                            levelController.SelectedBuilding = null;                            
                        }
                            
                        // Manda orden al levelcontroler
                        else
                        {
                            if (levelController.SelectedBuilding)
                                levelController.SelectedBuilding.ShowNeighbourLines(false);
                            levelController.ManageOrder(levelController.PreSelectedBuilding);
                        }
                            
                    }
                    else
                    {
                        if(levelController.SelectedBuilding)
                            levelController.SelectedBuilding.ShowNeighbourLines(false);
                        levelController.ManageOrder(hitBuilding);
                    }
                }
                else
                {
                    //
                    if (levelController.PreSelectedBuilding &&
                        levelController.PreSelectedBuilding.SupplyTo != null)
                    {
                        levelController.PreSelectedBuilding.SupplyTo = null;

                        //levelController.PreSelectedBuilding.SupplyLineRenderer.SetPosition(1,
                        //    levelController.SelectedBuilding.transform.position);
                    }

                    //
                    if (levelController.SelectedBuilding)
                    {
                        levelController.SelectedBuilding.ShowNeighbourLines(false);
                        levelController.SelectedBuilding = null;
                    }
                }

                // 
                if (levelController.PreSelectedBuilding &&
                    levelController.PreSelectedBuilding.SupplyTo == null)
                    levelController.PreSelectedBuilding.SupplyLineRenderer.SetPosition(1,
                        levelController.PreSelectedBuilding.transform.position);
                //
                levelController.PreSelectedBuilding = null;
            }
        }
    }

    void CheckAbilityUse()
    {
        //
        if (Input.GetKeyDown(KeyCode.Q))
        {
            showingTroopsToProduce = !showingTroopsToProduce;
        }
        //
        if (levelController.SelectedBuilding != null)
        {
            //
            if (!showingTroopsToProduce)
            {
                //
                for (int i = 0; i < levelController.SelectedBuilding.BuildingImprovementList.Count; i++)
                {
                    // 49 - Tecla 1
                    if (Input.GetKeyDown((KeyCode)(49 + i)))
                    {
                        if (i != selectedOne)
                        {
                            levelController.SelectedBuilding.StartImprovementConstruction(i);
                        }
                    }
                }
            }
            else
            {
                //
                for (int i = 0; i < levelController.SelectedBuilding.possibleTroopsToProduce.Length; i++)
                {
                    // 49 - Tecla 1
                    if (Input.GetKeyDown((KeyCode)(49 + i)))
                    {
                        if (i != selectedOne)
                        {
                            levelController.SelectedBuilding.producedTrooper =
                                levelController.SelectedBuilding.possibleTroopsToProduce[i];
                        }
                    }
                }
            }
        }
        else
        {
            // Seleccionamos habilidades
            for (int i = 0; i < commanderAbilities.Length; i++)
            {
                // 49 - Tecla 1
                if (Input.GetKeyDown((KeyCode)(49 + i)))
                {
                    if (i == selectedOne)
                    {
                        selectedOne = -1;
                        abilitySelected = false;
                        //
                        abilityMarkerObject.SetActive(false);
                    } // No dejamos seleccionarla si no tiene suficientes puntos
                    else if (commanderAbilities[i].cost > levelController.PlayerCommandPoints)
                    {
                        // Sonidito de error
                        levelController.PlayAbilityClip(errorClip);
                    }
                    else
                    {
                        selectedOne = i;
                        abilitySelected = true;
                        //
                        abilityMarkerObject.SetActive(true);
                        // Recordemos radio vs diametro
                        abilityMarkerObject.transform.localScale = Vector3.one * commanderAbilities[i].range * 2;
                    }
                }
            }
        }
    }

    //
    void UpdateAbilityMarker()
    {
        //
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            abilityMarkerObject.transform.position = hitInfo.point + (Vector3.up * 0.01f);
            //Debug.Log("Setting ability marker object");
        }
    }

    //
    void ShowAbilityIcons()
    {
        //
        float buttonListStart = Screen.width / 2 - (commanderAbilities.Length * buttonSize / 2);
        //
        for (int i = 0; i < commanderAbilities.Length; i++)
        {
            // TODO: Mostrar también cuanto falta para usarla
            float abilityReadiness = levelController.PlayerCommandPoints / commanderAbilities[i].cost;
            abilityReadiness = Mathf.Clamp01(abilityReadiness);
            GUI.DrawTexture(new Rect(i * buttonSize + buttonListStart, Screen.height - buttonSize, 
                buttonSize, abilityReadiness * buttonSize), readinessTexture);

            //
            if (selectedOne != -1)
            {
                GUI.DrawTexture(new Rect(selectedOne * buttonSize + buttonListStart, Screen.height - buttonSize,
                    buttonSize, buttonSize), selectionTexture);
            }

            GUI.DrawTexture(new Rect(i * buttonSize + buttonListStart, Screen.height - buttonSize, buttonSize, buttonSize), 
                commanderAbilities[i].abilityIcon);
            GUI.Label(new Rect(i * buttonSize + buttonListStart, Screen.height - buttonSize, buttonSize, buttonSize), 
                (i+1) + "", uISkin.customStyles[2]);
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    void ShowBuildingOptionIcons()
    {
        
        //
        List<BuildingImprovement> currentBuildingImprovements = levelController.SelectedBuilding.BuildingImprovementList;
        //
        float buttonListStart = Screen.width / 2 - (currentBuildingImprovements.Count * buttonSize / 2);
        
        // Este lo colocamos un poc mas alante
        if (levelController.SelectedBuilding.producedTrooper != null &&
            levelController.SelectedBuilding.possibleTroopsToProduce.Length > 0)
        {
            GUI.DrawTexture(new Rect(buttonListStart - buttonSize, Screen.height - buttonSize, buttonSize, buttonSize),
                    levelController.SelectedBuilding.producedTrooper.icon);
            GUI.DrawTexture(new Rect(buttonListStart - buttonSize, Screen.height - buttonSize, buttonSize, buttonSize),
                    changeIconTexture);
            GUI.Label(new Rect(buttonListStart - buttonSize, Screen.height - buttonSize, buttonSize, buttonSize),
                    "Q", uISkin.customStyles[2]);
        }

        //
        //if (selectedOne != -1)
        //{
        //    GUI.DrawTexture(new Rect(selectedOne * buttonSize + buttonListStart, Screen.height - buttonSize,
        //        buttonSize, buttonSize), builtOptionTexture);
        //}

        //
        if (!showingTroopsToProduce)
        {
            //
            for (int i = 0; i < currentBuildingImprovements.Count; i++)
            {
                // TODO: Mostrar texura detrás si esta activo
                if (currentBuildingImprovements[i].gameObject.activeSelf)
                {
                    GUI.DrawTexture(new Rect(i * buttonSize + buttonListStart, Screen.height - buttonSize, buttonSize, buttonSize),
                        builtOptionTexture);
                }
                else if (currentBuildingImprovements[i].ConstructionStatus == BuildingImprovement.Status.OnConstruction)
                {
                    //
                    float constructionProgress = (float)currentBuildingImprovements[i].Progression /
                        (float)currentBuildingImprovements[i].cost;
                    constructionProgress = Mathf.Clamp01(constructionProgress);
                    //
                    GUI.DrawTexture(new Rect(i * buttonSize + buttonListStart, Screen.height - buttonSize,
                        buttonSize, buttonSize), onConstructionTexture);
                    GUI.DrawTexture(new Rect(i * buttonSize + buttonListStart, Screen.height - buttonSize,
                        buttonSize, buttonSize * constructionProgress), readinessTexture);
                }

                //
                GUI.DrawTexture(new Rect(i * buttonSize + buttonListStart, Screen.height - buttonSize, buttonSize, buttonSize),
                    currentBuildingImprovements[i].icon);
                GUI.Label(new Rect(i * buttonSize + buttonListStart, Screen.height - buttonSize, buttonSize, buttonSize), 
                    (i + 1) + "", uISkin.customStyles[2]);
            }
        }
        else
        {
            //
            for (int i = 0; i < levelController.SelectedBuilding.possibleTroopsToProduce.Length; i++)
            {
                //
                if(levelController.SelectedBuilding.possibleTroopsToProduce[i] == levelController.SelectedBuilding.producedTrooper)
                    GUI.DrawTexture(new Rect(i * buttonSize + buttonListStart, Screen.height - buttonSize,
                        buttonSize, buttonSize), onConstructionTexture);
                //
                GUI.DrawTexture(new Rect(i * buttonSize + buttonListStart, Screen.height - buttonSize,
                        buttonSize, buttonSize), levelController.SelectedBuilding.possibleTroopsToProduce[i].icon);
                GUI.Label(new Rect(i * buttonSize + buttonListStart, Screen.height - buttonSize, buttonSize, buttonSize), 
                    (i + 1) + "", uISkin.customStyles[2]);
            }
        }
    }

    //
    void ShowTroopsToProduceIcons()
    {

    }
}
