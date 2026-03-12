using Stats;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EntityMovementModule : EntityModule
{
    [Header("References")]
    [SerializeField] private Rigidbody m_rigidbody;

    [Header("Movement")]
    [SerializeField] private float m_fallbackMoveSpeed = 10f;

    public Vector3 MoveInput { get; private set; }
    public Vector3 CurrentVelocity => m_rigidbody != null ? m_rigidbody.linearVelocity : Vector3.zero;

    private float m_moveSpeed;

    private void Reset() => CacheReferences();

    private void FixedUpdate()
    {
        if (m_rigidbody == null) return;

        RefreshMoveSpeed();
        ApplyMovement();
    }

    public void SetMoveInput(Vector2 flatInput)
    {
        MoveInput = Vector3.ClampMagnitude(new Vector3(flatInput.x, 0f, flatInput.y), 1f);
    }

    public void AddImpulse(Vector3 impulse)
    {
        m_rigidbody?.AddForce(impulse, ForceMode.VelocityChange);
    }

    protected virtual void OnAfterMovementApplied() { }

    private void ApplyMovement()
    {
        Vector3 targetXZ = MoveInput * m_moveSpeed;
        m_rigidbody.linearVelocity = new Vector3(targetXZ.x, m_rigidbody.linearVelocity.y, targetXZ.z);

        OnAfterMovementApplied();
    }

    private void RefreshMoveSpeed()
    {
        m_moveSpeed = m_fallbackMoveSpeed;
        if (Owner.TryGetModule(out EntityStatModule statModule))
            m_moveSpeed = statModule.GetValue(StatType.MoveSpeed);
    }

    public override void CacheReferences()
    {
        base.CacheReferences();
        m_rigidbody = GetComponent<Rigidbody>();
    }

    protected override void OnInitialize()
    {
        if (m_rigidbody == null) return;
        m_rigidbody.freezeRotation = true;
        m_rigidbody.useGravity = true;
    }
}