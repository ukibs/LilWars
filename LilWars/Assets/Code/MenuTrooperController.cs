using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuTrooperController : MonoBehaviour
{
    public enum Status
    {
        Idle,
        Wandering
    }

    private Vector3 currentDestination;
    private float idleCounter = 0;
    private Status status = Status.Idle;

    public Status CurrentStatus { get { return status; } }

    // Start is called before the first frame update
    void Start()
    {
        idleCounter = Random.Range(0, 1);
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        UpdateBehaviour(dt);
    }

    void UpdateBehaviour(float dt)
    {
        //
        switch (status)
        {
            case Status.Idle:
                idleCounter += dt;
                if (idleCounter >= 1)
                {
                    DecideNextWanderDestination();
                    status = Status.Wandering;
                    idleCounter -= 1;
                }
                break;

            case Status.Wandering:
                Move(dt);
                if ((transform.position - currentDestination).magnitude < 0.5f)
                    status = Status.Idle;
                break;
        }
    }

    void Move(float dt)
    {
        Vector3 direction = (currentDestination - transform.position).normalized;
        transform.Translate(direction * 10 * dt);
        //
        //PlayClipWithoutOverlapping(trooperStats.movingClip);
    }

    void DecideNextWanderDestination()
    {
        //
        float radius = Random.Range(0, 10);
        float angle = Random.Range(0, 360);
        //
        float xPos = radius * Mathf.Cos(angle);
        float zPos = radius * Mathf.Sin(angle);
        
        //
        currentDestination = new Vector3(xPos, 0, zPos);
        currentDestination.y = transform.position.y;
    }
}
