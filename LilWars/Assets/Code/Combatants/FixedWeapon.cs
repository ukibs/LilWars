using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedWeapon : BaseCombatant
{
    public WeaponStats weaponStats;
    public Transform shootPoint;

    private Building building;
    private float attackCooldown;
    private TrooperController currentObjective;
    private LevelController levelController;

    public Building WeaponBuilding { get { return building; } }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        building = GetComponentInParent<Building>();
        levelController = FindObjectOfType<LevelController>();
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        if(building.CurrentOwner != Owner.None)
        {
            //
            if(currentObjective == null || currentObjective.isActiveAndEnabled)
            {
                // Primero en el propio edificio
                CheckEnemiesInBuilding();
                // Luego en campo abierto
                CheckEnemiesInOpenField();
                // Por ultimo en vecinos cercanos
            }
            
            //
            if (currentObjective != null)
                UpdateFiring(dt);
        }
        
    }

    //
    void CheckEnemiesInBuilding()
    {
        //
        if (currentObjective != null) return;
        //
        switch (building.CurrentOwner)
        {
            case Owner.Blue:
                if(building.CurrentRedTroopers.Count > 0)
                {
                    currentObjective = building.GetNearestTrooper(building.CurrentOwner, transform.position);
                }
                break;
            case Owner.Red:
                if (building.CurrentBlueTroopers.Count > 0)
                {
                    currentObjective = building.GetNearestTrooper(building.CurrentOwner, transform.position);
                }
                break;
        }
    }

    //
    void CheckEnemiesInOpenField()
    {
        //
        if (currentObjective != null) return;
        //
        //switch (building.CurrentOwner)
        //{
        //    case Owner.Blue:
        //        if (levelController..Count > 0)
        //        {
        //            currentObjective = building.GetNearestTrooper(building.CurrentOwner, transform.position);
        //        }
        //        break;
        //    case Owner.Red:
        //        if (building.CurrentBlueTroopers.Count > 0)
        //        {
        //            currentObjective = building.GetNearestTrooper(building.CurrentOwner, transform.position);
        //        }
        //        break;
        //}
        //currentObjective = levelController.GetNearestTrooper(building.CurrentOwner, transform.position);
        currentObjective = levelController.GetFirstTrooperInRange(building.CurrentOwner, transform.position, weaponStats);
    }

    //
    void UpdateFiring(float dt)
    {
        attackCooldown += dt;
        //
        if(attackCooldown >= weaponStats.attackRate)
        {

            attackCooldown -= weaponStats.attackRate;
            //TODO: Casos para enemigo en edicio y fortificado
            float chanceToHit = 0.75f;
            float attackRoll = Random.Range(0f, 1f);
            bool attackHits = false;
            if (attackRoll >= chanceToHit)
            {
                //
                currentObjective.ReceiveAttack(this);
                attackHits = true;
            }

            //
            PlayClipWithoutOverlapping(weaponStats.attackClip, weaponStats.clipVolume);
            //
            if (weaponStats.fireArm && currentObjective != null)
            {
                //
                transform.LookAt(currentObjective.transform);
                //fireArmPS.Play();
                //
                if (weaponStats.weaponProyectileMaterial != null)
                {
                    GameObject newProyectile = Instantiate(weaponStats.proyectilePrefab, transform.position, Quaternion.identity);
                    newProyectile.transform.LookAt(currentObjective.transform);
                    BulletBehaviour bulletBehaviour = newProyectile.GetComponent<BulletBehaviour>();
                    bulletBehaviour.InitializeWeaponBullet(currentObjective.transform.position, weaponStats.weaponProyectileMaterial, attackHits,
                        currentObjective, weaponStats, this);
                }
            }
        }
    }
}
