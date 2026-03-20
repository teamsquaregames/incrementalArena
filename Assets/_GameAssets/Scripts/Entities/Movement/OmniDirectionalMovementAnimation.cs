using UnityEngine;
using Utils;

/// <summary>
/// Drives an omni-directional (strafe-aware) movement animation by decomposing
/// the character's world-space velocity into local Forward / Right components,
/// then feeding them into the Animator as normalised [-1, 1] floats.
///
/// Use this instead of <see cref="EntityMovementAnimation"/> on entities whose
/// facing (rotation) is controlled independently of their movement direction —
/// i.e. Active and Passive brain modes.
///
/// Animator parameters expected
/// ────────────────────────────
///   Float  "XVelocity"  — lateral  component (-1 = full strafe-left,  +1 = full strafe-right)
///   Float  "ZVelocity"  — forward  component (-1 = full walk-backward, +1 = full walk-forward)
///
/// All values are smoothed each frame via <see cref="m_smoothTime"/> to avoid
/// jerky blendtree transitions.
/// </summary>
public class OmniDirectionalMovementAnimation : EntityModule
{
    [Header("References")]
    [SerializeField] private Animator m_animator;

    [Header("Parameter names")]
    [SerializeField] private string m_velocityXName = "XVelocity";
    [SerializeField] private string m_velocityYName = "ZVelocity";

    [Header("Smoothing")]
    [Tooltip("Lower = snappier, higher = smoother. 0.05–0.15 works well.")]
    [SerializeField, Min(0f)] private float m_smoothTime = 0.08f;

    // Animator parameter hashes
    private int m_hashVelocityX;
    private int m_hashVelocityY;

    // Current smoothed values
    private float m_currentX;
    private float m_currentY;

    // SmoothDamp velocities
    private float m_velX;
    private float m_velY;

    // ─── EntityModule ─────────────────────────────────────────────────────────

    private void Reset() => CacheReferences();

    public override void CacheReferences()
    {
        base.CacheReferences();
        m_animator = GetComponentInChildren<Animator>();
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        m_hashVelocityX = Animator.StringToHash(m_velocityXName);
        m_hashVelocityY = Animator.StringToHash(m_velocityYName);
    }

    // ─── Update ───────────────────────────────────────────────────────────────

    private void Update()
    {
        if (m_animator == null) return;
        if (!Owner.TryGetModule(out EntityMovementModule movementModule)) return;

        // Project world-space velocity onto the entity's local axes
        // so that strafing left/right and moving backward are all correctly encoded.
        Vector3 worldVelocity = movementModule.MoveInput;
        Vector3 localVelocity = Owner.transform.InverseTransformDirection(worldVelocity);

        float targetX = Mathf.Clamp(localVelocity.x, -1f, 1f);
        float targetY = Mathf.Clamp(localVelocity.z, -1f, 1f);

        m_currentX = Mathf.SmoothDamp(m_currentX, targetX, ref m_velX, m_smoothTime);
        m_currentY = Mathf.SmoothDamp(m_currentY, targetY, ref m_velY, m_smoothTime);

        m_animator.SetFloat(m_hashVelocityX, m_currentX);
        m_animator.SetFloat(m_hashVelocityY, m_currentY);

        this.Log($"[OmniMovAnim] X={m_currentX:F2}  Y={m_currentY:F2}");
    }
}