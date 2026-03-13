using UnityEngine;

public class EntityMovementAnimation : EntityModule
{
    [Header("References")]
    [SerializeField] private Animator m_animator;

    [Header("Animation Parameters")]
    [SerializeField] private string m_speedParameterName = "Speed";

    private int m_speedParameterHash;

    private void Reset() => CacheReferences();

    private void Update()
    {
        if (m_animator == null) return;

        if (Owner.TryGetModule(out EntityMovementModule movementModule))
        {
            m_animator.SetFloat(m_speedParameterHash, movementModule.CurrentVelocity);
        }
    }

    public override void CacheReferences()
    {
        base.CacheReferences();
        m_animator = GetComponentInChildren<Animator>();
    }

    protected override void OnInitialize()
    {
        m_speedParameterHash = Animator.StringToHash(m_speedParameterName);
    }
}