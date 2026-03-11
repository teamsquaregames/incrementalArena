using Lean.Pool;
using UnityEngine;

public class GenericGaugePoolRefSetter : MonoBehaviour
{
	[SerializeField] private GenericGaugePoolRef poolRef;

    private void Awake()
    {
        poolRef.pool = GetComponent<LeanGenericGaugePool>();
    }
}