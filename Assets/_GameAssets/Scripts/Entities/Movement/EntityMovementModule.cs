using Stats;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EntityMovementModule : EntityModule
{
    [Header("References")]
    [SerializeField] private Rigidbody m_rigidbody;

    [Header("Movement")]
    [SerializeField] private float m_fallbackMoveSpeed = 10f;
    [SerializeField] private bool m_faceVelocity = false;
    [SerializeField, Min(0f)] private float m_rotationSpeed = 20f;

    public Vector3 MoveInput { get; private set; }
    public float CurrentVelocity => m_rigidbody != null ? m_rigidbody.linearVelocity.magnitude : 0;

    private float m_moveSpeed;

    private void Reset() => CacheReferences();

    private void FixedUpdate()
    {
        if (m_rigidbody == null) return;

        RefreshMoveSpeed();
        ApplyMovement();

        if (m_faceVelocity)
            ApplyVelocityRotation();
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

    private void ApplyVelocityRotation()
    {
        Vector3 flatVelocity = new Vector3(m_rigidbody.linearVelocity.x, 0f, m_rigidbody.linearVelocity.z);
        if (flatVelocity.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(flatVelocity);
        Owner.transform.rotation = Quaternion.RotateTowards(Owner.transform.rotation, targetRotation, m_rotationSpeed * Time.fixedDeltaTime);
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
}