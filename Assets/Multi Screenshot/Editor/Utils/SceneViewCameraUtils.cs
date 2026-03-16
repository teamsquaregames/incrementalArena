using UnityEditor;
using UnityEngine;

namespace Dalak.Screenshot
{
    public static class SceneViewCameraUtils
    {
        public static void GetPositionAndRotation(out Vector3 pos, out Quaternion rotation)
        {
            var view = SceneView.lastActiveSceneView;
            var camTransform = view.camera.transform;
            pos = camTransform.position;
            rotation = camTransform.rotation;
        }
        
        public static float FOV()
        {
            var view = SceneView.lastActiveSceneView;
            return view.camera.fieldOfView;
        }
        
        public static Vector3 GetOrthogonal(Vector3 vec)
        {
            var v1 = new Vector3(vec.z, vec.z, -vec.x - vec.y).normalized;
            var v2 = new Vector3(-vec.y - vec.z, vec.x, vec.x).normalized;
            if (v1.magnitude > Mathf.Epsilon)
            {
                return v1;
            }

            return v2;
        }
    }
}