using MyBox;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private Camera mainCam;
    
    public Camera MainCam => mainCam;
}