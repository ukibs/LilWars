using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    private TrooperController trooperController;
    private MenuTrooperController menuTrooperController;
    private float timeGap;
    private CameraPivotControl cameraPivotControl;

    // Start is called before the first frame update
    void Start()
    {
        trooperController = GetComponentInParent<TrooperController>();
        menuTrooperController = GetComponentInParent<MenuTrooperController>();
        cameraPivotControl = FindObjectOfType<CameraPivotControl>();
        //
        float currentTime = Time.time;
        timeGap = currentTime - (int)currentTime;
        //
        AdjustToCameraForward();
    }

    // Update is called once per frame
    void Update()
    {
        //
        UpdateBouncingSprite();
        //
        if (cameraPivotControl && cameraPivotControl.adjustSprite)
        {
            AdjustToCameraForward();
        }
    }

    public void AdjustToCameraForward()
    {
        //
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0;
        transform.LookAt(transform.position + cameraForward);
    }

    void UpdateBouncingSprite()
    {
        //
        if (trooperController != null)
        {
            float currentY = 0;
            switch (trooperController.CurrentStatus)
            {
                case TrooperController.Status.Idle:
                    transform.localPosition = Vector3.zero;
                    break;
                case TrooperController.Status.Wandering:
                case TrooperController.Status.MovingToDestination:
                case TrooperController.Status.MovingInFight:
                    currentY = Mathf.Abs(Mathf.Sin((Time.time - timeGap) * 15)) * 0.5f;
                    transform.localPosition = new Vector3(0, currentY, 0);
                    break;
                case TrooperController.Status.Fighting:
                    currentY = Mathf.Abs(Mathf.Sin((Time.time - timeGap) * 20)) * 0.5f;
                    transform.localPosition = new Vector3(0, currentY, 0);
                    break;
            }
        }
        //
        if (menuTrooperController != null)
        {
            float currentY = 0;
            switch (menuTrooperController.CurrentStatus)
            {
                case MenuTrooperController.Status.Idle:
                    transform.localPosition = Vector3.zero;
                    break;
                case MenuTrooperController.Status.Wandering:
                    currentY = Mathf.Abs(Mathf.Sin((Time.time - timeGap) * 20)) * 0.5f;
                    transform.localPosition = new Vector3(0, currentY, 0);
                    break;
            }
        }
    }
}
