using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TrooperStats", menuName = "ScriptableObjects/TrooperStats", order = 1)]
public class TrooperStats : ScriptableObject
{
    public Texture icon;
    public GameObject prefab;

    public float movementSpeed = 10;

    public WeaponStats weaponStats;

    //public float attackReach = 1;
    //public float attackRate = 0.5f;
    //public int attack = 4;
    public int armor = 0;
    public int initialHealth = 10;

    public float cost = 1;

    public TrooperController.TrooperMaterials trooperMaterials;

    //public GameObject proyectilePrefab;
    //public Material weaponProyectileMaterial;

    //public bool fireArm = true;

    public GameObject deadBodyPrefab;

    public AudioClip movingClip;

    public GameObject deathEffectPrefab;
}
