using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMFreeLookSetting : MonoBehaviour
{
    void Awake()
    {
        CinemachineCore.GetInputAxis = clickControl;
    }

    public float clickControl(string axis)
    {
        if (Input.GetMouseButton(2))
        {
            Debug.Log(axis);

            return UnityEngine.Input.GetAxis(axis);
        }

        return 0;
    }
}
