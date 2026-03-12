using Lean.Pool;
using UnityEngine;

[CreateAssetMenu(fileName = "GameObjectPoolRef", menuName = "ScriptableObjects/GameObjectPoolRef")]
public class GameObjectPoolRef : ScriptableObject
{
    public LeanGameObjectPool pool;
}