using Lean.Pool;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityPoolRef", menuName = "Pool System/EntityPoolRef")]
public class EntityPoolRef : ComponentPoolRef<Entity, LeanEntityPool> { }