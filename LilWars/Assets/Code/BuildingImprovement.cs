using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ImprovementEffect
{
    Invalid = -1,

    None,               //Mejoras como arma fija, que funciona a su marcha
    ExtraDefense,
    ExtraHealing,
    ExtraEnergy,        // Solo para el edificio
    ExtraStorage,        // Aumenta cantidad general de tropas
    ExtraCommand,       // Aumenta la subida de CP gloables

    Count
}

/// <summary>
/// 
/// </summary>
public class BuildingImprovement : MonoBehaviour
{
    //
    public enum Status
    {
        NotConstructed,
        OnConstruction,
        Active,

        Count
    }

    //
    public Texture icon;
    public int cost = 10;
    public ImprovementEffect improvementEffect;
    //
    private Building building;
    private int progression = 0;
    // TODO: Progreso de construcción y si se ha ordenado construirlo
    private Status status;

    public int Progression
    {
        get { return progression; }
        set { progression = value; }
    }

    public Status ConstructionStatus { get { return status; } }

    // Start is called before the first frame update
    void Start()
    {
        //
        building = GetComponentInParent<Building>();
        //
        Initiate();
    }

    void Initiate()
    {
        building.AddImprovement(this);
        gameObject.SetActive(false);
    }

    public void StartConstruction()
    {
        status = Status.OnConstruction;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        status = Status.Active;
    }
}
