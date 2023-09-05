using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region public field

    

    #endregion
    #region private field

    [SerializeField]
    private CinemachineVirtualCamera CMCamera;
    Cinemachine3rdPersonFollow CM3rdComponent;
    [SerializeField]
    private float WheelSensitivity = 50f;

    CinemachineVirtualCamera rallcamera;
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        CMCamera = GetComponent<CinemachineVirtualCamera>();
        CM3rdComponent = CMCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        //rallcamera = GameObject.Find("RallCam").GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        float zoomSpeed = Input.GetAxis("Mouse ScrollWheel") * WheelSensitivity;

        CMCamera.m_Lens.FieldOfView = Mathf.Clamp(CMCamera.m_Lens.FieldOfView + zoomSpeed, 30, 90);
        if (CMCamera.m_Lens.FieldOfView < 60)
        {
            CM3rdComponent.VerticalArmLength = .8f+ (1.2f * (CMCamera.m_Lens.FieldOfView-30) / 30);
        }
        else
        {
            CM3rdComponent.VerticalArmLength = 2;
        }

    }
}
