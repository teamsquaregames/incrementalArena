using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

public class ColliderRomover : MonoBehaviour
{
    [Button]
    public void RemoveColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            this.Log($"Removing collider: {col.name} from {col.gameObject.name}");
            DestroyImmediate(col);
        }
    }
}
