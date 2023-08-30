using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{

    GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("UniChan");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.transform.position - target.transform.forward*4 + target.transform.up *2;
        transform.LookAt(target.transform);
    }
}
