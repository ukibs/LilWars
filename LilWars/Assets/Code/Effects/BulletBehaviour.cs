using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    public float lifeTime = 0.1f;

    private Vector3 initialPosition;
    private Renderer meshRenderer;
    private Vector3 finalPosition;
    private float timeAlive = 0;
    private float lifeTimeToUse;
    private LevelController levelController;
    private FixedWeapon attackerWeapon;
    private TrooperController attackerTrooper;
    private TrooperController objectiveTrooper;
    private WeaponStats weaponStats;
    private BombController bombController;
    private bool attackHits;
    private int attackerType = -1;

    // Provisional
    private Material materialToUse;

    // Start is called before the first frame update
    void Start()
    {
        //
        levelController = FindObjectOfType<LevelController>();
        //
        initialPosition = transform.position;
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        //
        bombController = GetComponent<BombController>();
        if (bombController)
            bombController.InitiateBomb(weaponStats.attack);
        //Debug.Log(meshRenderer);
        meshRenderer.material = materialToUse;
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        timeAlive += dt;
        // TODO: Revisar como gestionamos esto
        transform.position = Vector3.Lerp(initialPosition, finalPosition, timeAlive / lifeTimeToUse);
        //
        if (timeAlive > lifeTimeToUse)
        {
            // De momento que explote si o si
            // Aunque sea en cuenca
            if (weaponStats.isExplosive)
            {
                Debug.Log("Bomb explodes");
                // TODO: Habrá que meter unas cuantas cosas para las estadísticas
                bombController.Explode(transform.position);
            }
            // Y no explosiva a ver si da
            else if (attackHits)
            {
                // TODO: Chequear que este vivo
                if (objectiveTrooper)
                {
                    // 
                    if (attackerType != -1)
                        objectiveTrooper.ReceiveAttack(weaponStats.attack, attackerType);
                    // TODO: Mirar si surgen casos de arma fija que pueda ser destruida
                    else if (attackerWeapon)
                        objectiveTrooper.ReceiveAttack(attackerWeapon);
                }                
            }
            //
            //Destroy(gameObject);
            levelController.ReturnBullet(this);
        }            
    }

    private void OnDrawGizmosSelected()
    {
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="finalPosition"></param>
    /// <param name="materialToUse"></param>
    /// <param name="attackHits"></param>
    /// <param name="objectiveTrooper"></param>
    /// <param name="weaponStats"></param>
    /// <param name="attacker"></param>
    public void InitializeTrooperBullet(Vector3 finalPosition, Material materialToUse, bool attackHits, TrooperController objectiveTrooper,
        WeaponStats weaponStats, TrooperController attacker)
    {
        attackerType = FindObjectOfType<LevelController>().MatchStatisticsRef.producedTroopersInMatch.IndexOf(attacker.trooperStats);
        this.attackerTrooper = attacker;
        InitiliazeBullet(finalPosition, materialToUse, attackHits, objectiveTrooper, weaponStats);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="finalPosition"></param>
    /// <param name="materialToUse"></param>
    /// <param name="attackHits"></param>
    /// <param name="objectiveTrooper"></param>
    /// <param name="weaponStats"></param>
    /// <param name="attacker"></param>
    public void InitializeWeaponBullet(Vector3 finalPosition, Material materialToUse, bool attackHits, TrooperController objectiveTrooper,
        WeaponStats weaponStats, FixedWeapon attacker)
    {
        this.attackerWeapon = attacker;
        InitiliazeBullet(finalPosition, materialToUse, attackHits, objectiveTrooper, weaponStats);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="finalPosition"></param>
    /// <param name="materialToUse"></param>
    public void InitiliazeBullet(Vector3 finalPosition, Material materialToUse, bool attackHits, TrooperController objectiveTrooper, 
        WeaponStats weaponStats)
    {
        //
        timeAlive = 0;
        // TODO: Hacer la función de daño con esto al impactar
        this.objectiveTrooper = objectiveTrooper;
        this.weaponStats = weaponStats;
        this.attackHits = attackHits;
        //
        initialPosition = transform.position;
        //
        if (attackHits)
        {
            this.finalPosition = finalPosition;
            lifeTimeToUse = lifeTime;
        }            
        else
        {
            this.finalPosition = transform.position + ((finalPosition - transform.position) * 3f);
            lifeTimeToUse = lifeTime * 3;
        }
        //
        if (meshRenderer != null)
            meshRenderer.material = materialToUse;
        this.materialToUse = materialToUse;
        //
        // transform.LookAt(finalPosition);
    }
}
