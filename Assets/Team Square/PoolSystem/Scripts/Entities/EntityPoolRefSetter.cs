using Lean.Pool;
using UnityEngine;

public class EntityPoolRefSetter : MonoBehaviour
{
	[SerializeField] private EntityPoolRef poolRef;

    private void Awake()
    {
        poolRef.pool = GetComponent<LeanEntityPool>();
    }
}