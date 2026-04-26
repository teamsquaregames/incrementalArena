using System;
using UnityEngine;

public class CameraRegister : MonoBehaviour
{
    [SerializeField] private Camera camera;
    private void Awake()
    {
        CameraManager.Instance.RegisterMainCamera(camera);
    }

    private void Reset()
    {
        camera = GetComponent<Camera>();
    }
}