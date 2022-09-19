using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private float minCameraDistanceToPivot = 20;
    private float maxCameraDistanceToPivot = 100;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        float currentDistanceToPivot = transform.localPosition.magnitude;
        //
        float wheelAxis = Input.GetAxis("Mouse ScrollWheel");
        // transform.Translate(Vector3.forward * 1000 * wheelAxis * dt);
        currentDistanceToPivot -= wheelAxis * 10000 * dt;
        currentDistanceToPivot = Mathf.Clamp(currentDistanceToPivot, minCameraDistanceToPivot, maxCameraDistanceToPivot);
        //
        //transform.localPosition = Vector3.ClampMagnitude(transform.localPosition, currentDistanceToPivot);
        transform.localPosition = transform.localPosition.normalized * currentDistanceToPivot;
    }
}
