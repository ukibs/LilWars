using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponStats", menuName = "ScriptableObjects/WeaponStats", order = 1)]
public class WeaponStats : ScriptableObject
{
    public float attackReach = 1;
    public float attackRate = 0.5f;
    public int attack = 4;

    public GameObject proyectilePrefab;
    public Material weaponProyectileMaterial;

    public bool fireArm = true;

    public bool isExplosive = false;
    public float explosiveRadius = 0;

    public AudioClip attackClip;
    public float clipVolume = 0.5f;
}
