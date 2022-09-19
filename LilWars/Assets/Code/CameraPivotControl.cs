using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPivotControl : MonoBehaviour
{
    public float rotatingSpeed = 90;
    public float movementSpeed = 100;
    

    private SpriteController[] spriteControllers;
    [HideInInspector] public bool adjustSprite = false;

    // Start is called before the first frame update
    void Start()
    {
        // FACK: Tendremos que hacerlos varias veces
        // Al menos hasta que las pool esten ehchas del todo
        // TODO: Mejor que lo haga cada sprite controller chequeando a la cámara
        spriteControllers = FindObjectsOfType<SpriteController>();
    }

    // Update is called once per frame
    void Update()
    {
        //
        adjustSprite = false;
        //
        float dt = Time.deltaTime;
        //if (Time.timeScale == 0)
            dt = 0.016f;
        //
        float horizontalAxis = Input.GetAxis("Horizontal");
        float verticalAxis = Input.GetAxis("Vertical");
        float rightMouseAxis = Input.GetAxis("Fire2");
        float mouseHorizontalAxis = Input.GetAxis("Mouse X");
        //
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0;
        cameraForward = cameraForward.normalized;
        //
        Vector3 cameraRight = Camera.main.transform.right;
        cameraRight.y = 0;
        cameraRight = cameraRight.normalized;
        //
        //transform.Translate(cameraForward * verticalAxis * movementSpeed * dt, Space.World);
        transform.position += cameraForward * verticalAxis * movementSpeed * dt;
        //
        if (rightMouseAxis != 0)
        {
            transform.Rotate(Vector3.up * mouseHorizontalAxis * rotatingSpeed * dt);
            // TODO: Que roten los sprites también
            adjustSprite = true;
        }
            
        //else
        transform.Translate(cameraRight * horizontalAxis * movementSpeed * dt, Space.World);
    }
}
