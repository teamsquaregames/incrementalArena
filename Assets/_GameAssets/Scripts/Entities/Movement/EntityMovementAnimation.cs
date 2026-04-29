using UnityEngine;

public class EntityMovementAnimation : EntityModule
{
    [Header("Animation Parameters")]
    [SerializeField] private string m_speedParameterName = "Speed";

    private int m_speedParameterHash;

    private void Reset() => CacheReferences();

    private void Update()
    {
        if (Owner.Animator == null) return;

        if (Owner.TryGetModule(out EntityMovementModule movementModule))
        {
            Owner.Animator.SetFloat(m_speedParameterHash, movementModule.CurrentVelocity);
        }
    }

    protected override void OnInitialize()
    {
        m_speedParameterHash = Animator.StringToHash(m_speedParameterName);
    }
}