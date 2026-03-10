using Lean.Pool;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityPoolRef", menuName = "ScriptableObjects/EntityPoolRef")]
public class EntityPoolRef : ScriptableObject
{
    public LeanEntityPool pool;
}