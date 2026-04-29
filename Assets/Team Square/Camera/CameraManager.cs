using System;
using MyBox;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private CinemachineCamera m_vCam;
    
    public Camera MainCam => mainCam;

    public void RegisterMainCamera(Camera camera)
    {
        if (mainCam != null)
        {
            Destroy(mainCam.gameObject);
        }

        m_vCam.transform.position = camera.transform.position;
        m_vCam.transform.rotation = camera.transform.rotation;
        
        mainCam = camera;
        mainCam.transform.SetParent(transform);
    }
}