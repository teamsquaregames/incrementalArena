using Lean.Pool;
using UnityEngine;

public class ComponentPoolRef<TComponent, TPool> : ScriptableObject
    where TComponent : Component
    where TPool : LeanComponentPool<TComponent>
{
    public TPool pool;
}
