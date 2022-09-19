using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrooperGalleryManager : MonoBehaviour
{
    //
    public TrooperStats[] statsToShow;
    public GUISkin uISkin;
    //
    private MenuTrooperController[] menuTroopers;
    TrooperStats currentTrooperStats;

    // Start is called before the first frame update
    void Start()
    {
        menuTroopers = FindObjectsOfType<MenuTrooperController>();
        //
        ChangeTroopersMaterial(0);
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}

    private void OnGUI()
    {
        // Aqui mostraremos los stats
        ShowTrooperButtons();
        //
        ShowTrooperStats();
    }

    void ShowTrooperStats()
    {
        // Name
        GUI.Label(new Rect(Screen.width / 2 - 100, 100, 300, 50), currentTrooperStats.name, uISkin.label);
        // And attributes
        // Weapon
        GUI.Label(new Rect(Screen.width - 300, 200, 300, 50), 
            "Attack: " + currentTrooperStats.weaponStats.attack, uISkin.label);
        GUI.Label(new Rect(Screen.width - 300, 250, 300, 50), 
            "Attack rate: " + currentTrooperStats.weaponStats.attackRate, uISkin.label);
        GUI.Label(new Rect(Screen.width - 300, 300, 300, 50), 
            "Attack reach: " + currentTrooperStats.weaponStats.attackReach, uISkin.label);
        // Proper trooper
        GUI.Label(new Rect(Screen.width - 300, 350, 300, 50),
            "Health: " + currentTrooperStats.initialHealth, uISkin.label);
        GUI.Label(new Rect(Screen.width - 300, 400, 300, 50),
            "Armor: " + currentTrooperStats.armor, uISkin.label);
        GUI.Label(new Rect(Screen.width - 300, 450, 300, 50),
            "Speed: " + currentTrooperStats.movementSpeed, uISkin.label);
        // And cost
        GUI.Label(new Rect(Screen.width - 300, 500, 300, 50),
            "Cost: " + currentTrooperStats.cost, uISkin.label);
    }

    /// <summary>
    /// 
    /// </summary>
    void ShowTrooperButtons()
    {
        //
        for(int i = 0; i < statsToShow.Length; i++)
        {
            //
            Rect buttonCoordinates = new Rect(100, 100 * i + 100, 200, 50);
            //
            if (GUI.Button(buttonCoordinates, statsToShow[i].name))
            {
                ChangeTroopersMaterial(i);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    void ChangeTroopersMaterial(int index)
    {
        //
        for (int i = 0; i < menuTroopers.Length; i++)
        {
            
            //
            MeshRenderer meshRenderer = menuTroopers[i].GetComponentInChildren<MeshRenderer>();
            //
            if(i % 2 == 0)
            {
                meshRenderer.material = statsToShow[index].trooperMaterials.redMaterial;
            }
            else
            {
                meshRenderer.material = statsToShow[index].trooperMaterials.blueMaterial;
            }
        }
        //
        currentTrooperStats = statsToShow[index];
    }

    public void Return()
    {
        SceneManager.LoadScene(0);
    }
}
