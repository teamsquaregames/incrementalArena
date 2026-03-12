using Lean.Pool;
using UnityEngine;

public class ComponentPoolRefSetter<TComponent, TPool> : MonoBehaviour
    where TComponent : Component
    where TPool : LeanComponentPool<TComponent>
{
    [SerializeField] protected ComponentPoolRef<TComponent, TPool> poolRef;

    private void Awake() => poolRef.pool = GetComponent<TPool>();
}
