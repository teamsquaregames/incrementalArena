using System.Collections;
using Sirenix.OdinInspector;
using Stats;
using UnityEngine;
using Utils;


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

    [SerializeField, ReadOnly]private float m_moveSpeed;
    public float MoveSpeed => m_moveSpeed;

    private Coroutine m_dashCoroutine;

    private void Reset() => CacheReferences();

    private void FixedUpdate()
    {
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
        // this.Log($"Adding impulse {impulse}");
        m_rigidbody?.AddForce(impulse, ForceMode.Impulse);
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

    
    public void DashToPosition(Vector3 position, float duration)
    {
        this.Log($"Dashing to {position} over {duration} seconds");

        if (m_dashCoroutine != null)
        {
            StopCoroutine(m_dashCoroutine);
            m_dashCoroutine = null;
        }

        if  (duration <= 0f)
        {
            Owner.transform.position = position;
            return;
        }
        m_dashCoroutine = StartCoroutine(DashCR(position, duration));
    }

    private IEnumerator DashCR(Vector3 position, float duration)
    {
        float elapsed = 0f;
        Vector3 startPosition = Owner.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector3 newPosition = Vector3.Lerp(startPosition, position, t);
            transform.position = newPosition;
            // this.Log($"Dashing... {t * 100f:0.0}% / {newPosition}");
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        
    }
}