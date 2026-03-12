using Lean.Pool;
using UnityEngine;

public class GameObjectPoolRefSetter : MonoBehaviour
{
	[SerializeField] private GameObjectPoolRef poolRef;

    private void Awake()
    {
        poolRef.pool = GetComponent<LeanGameObjectPool>();
    }
}