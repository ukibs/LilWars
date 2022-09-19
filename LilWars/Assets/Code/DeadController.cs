using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadController : MonoBehaviour
{
    public DeadMaterials deadMaterials;
    public float heightFromAliveOne = -0.3f;

    //private List<Vector3> positionsToCheck;

    //private float heightToCheck = 5;
    //private float heightOffest = 0.2f;

    //private bool overFloor = false;

    // Start is called before the first frame update
    void Start()
    {
        // Para que se queden a la altura que corresponde
        //
        //positionsToCheck = new List<Vector3>(4);
        //positionsToCheck.Add(new Vector3(1, 0, 0));
        //positionsToCheck.Add(new Vector3(-1, 0, 0));
        //positionsToCheck.Add(new Vector3(0, 0, 1));
        //positionsToCheck.Add(new Vector3(0, 0, -1));
    }

    //private void OnGUI()
    //{
    //    //
    //    //Debug.DrawLine(transform.position, transform.position + (Vector3.down * heightOffest), Color.blue);
    //    //Debug.DrawLine(transform.position + (Vector3.down * heightOffest), transform.position + (Vector3.down * heightToCheck), Color.green);
    //}

    public void InitiateBody()
    {
        // Para que se queden a la altura que corresponde
        transform.position = new Vector3(transform.position.x, transform.position.y + heightFromAliveOne, transform.position.z);
        // Antes esto, si no mal
        //overFloor = false;
        //
        // StartCoroutine(PeriodicHeightAndSorroundingsCheck());
        // CheckHeight();
        
    }

    //
    public void SetMaterial(Owner owner)
    {
        //
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        //
        switch (owner)
        {
            case Owner.Blue: meshRenderer.material = deadMaterials.blueMaterial; break;
            case Owner.Red:  meshRenderer.material = deadMaterials.redMaterial; break;
        }
    }

    //IEnumerator PeriodicHeightAndSorroundingsCheck()
    //{
    //    //
    //    while (!overFloor)
    //    {
    //        yield return new WaitForSeconds(1);
    //        CheckHeight();
    //        //CheckSorroundings();
    //    }
    //    //
    //    //Debug.Log("Over floor, checkings stopped");
    //}

    /// <summary>
    /// 
    /// </summary>
    //void CheckHeight()
    //{
    //    //
    //    //float heightToCheck = 1;
    //    //float heightOffest = 0.15f;
    //    //
    //    Vector3 rayOrigin = transform.position - (Vector3.up * heightOffest);
    //    if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, heightToCheck))
    //    {
    //        // 0.2 es la altura normal del moñaco sobre el suelo
    //        transform.position = hitInfo.point + (Vector3.up * (heightOffest + 0.01f));
    //        //Debug.Log("Ray origin y: " + rayOrigin.y + ", Hit point: " + hitInfo.point.y + ", New height: " + transform.position.y);
    //        //Debug.Log("Hit in " + hitInfo.collider);
    //        //
    //        if (hitInfo.transform.tag == "Corpse")
    //        {
    //            //CheckSorroundings();
    //        }
    //        //else
    //        overFloor = true;
    //        //    Debug.Log();
    //    }
    //    // Ñapaaaa
    //    else
    //    {
    //        transform.position += Vector3.down * heightToCheck;
    //    }
    //}

    /// <summary>
    /// 
    /// </summary>
    //void CheckSorroundings()
    //{
    //    //
    //    bool somethingInSorroundings = false;
    //    //
    //    for(int i = 0; i < positionsToCheck.Count; i++)
    //    {
    //        if (Physics.Raycast(transform.position + positionsToCheck[i] + (Vector3.up * 0.5f), 
    //            Vector3.down, out RaycastHit hitInfo, 0.5f))
    //        {
    //            //transform.position += positionsToCheck[i];
    //            somethingInSorroundings = true;
    //            // Haremos que pare y siga, asi aligeramos
    //            break;
    //        }
    //    }

    //    //
    //    if(!somethingInSorroundings) transform.position += positionsToCheck[0];

    //    // TODO: Rotar el orden
    //    Vector3 positionToRotate = positionsToCheck[0];
    //    positionsToCheck.RemoveAt(0);
    //    positionsToCheck.Add(positionToRotate);
    //}


    [System.Serializable]
    public class DeadMaterials
    {
        public Material greyMaterial;
        public Material redMaterial;
        public Material blueMaterial;
    }
}
