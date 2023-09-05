using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField]
    private bool followOnStart = false;

    public GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        Camera camera = Camera.main;
        //target = GameObject.Find("UniChan");
    }

    // Update is called once per frame
    void LateUpdate ()
    {
        if (target==null)
        {
            return;
        }

        transform.position = target.transform.position - target.transform.forward * 4 + target.transform.up * 2;

        RaycastHit hit;

        Debug.DrawRay(target.transform.position, transform.position);

        if(Physics.Raycast(target.transform.position, transform.position, out hit, 2f, 1 << LayerMask.NameToLayer("Wall")))
        {
            Debug.Log(hit.collider.ClosestPoint(transform.position));
            transform.position = hit.collider.ClosestPoint(transform.position);
        }
        transform.LookAt(target.transform);

    }
}
