using System;
using MyBox;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private Camera mainCam;
    
    public Camera MainCam => mainCam;

    public void RegisterMainCamera(Camera camera)
    {
        Destroy(mainCam.gameObject);
        mainCam = camera;
        mainCam.transform.SetParent(transform);
    }
}