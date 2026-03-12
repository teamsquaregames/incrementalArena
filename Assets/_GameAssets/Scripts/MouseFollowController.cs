using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Déplace un personnage vers la position de la souris dans le monde 3D.
/// Compatible Unity 6 + New Input System.
/// </summary>
[AddComponentMenu("Movement/Mouse Follow Controller")]
public class MouseFollowController : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Caméra utilisée pour le raycasting. Laissez vide pour utiliser Camera.main.")]
    public Camera targetCamera;

    [Tooltip("Layer(s) sur lesquels le raycast détecte la position souris (sol, terrain…).")]
    public LayerMask groundLayerMask = ~0;
    [Header("Zone morte")]
    [Tooltip("Distance minimale (en unités) entre le personnage et la cible pour que le déplacement s'active.")]
    [Min(0f)] public float deadZoneRadius = 1.5f;

    [Tooltip("Affiche la zone morte sous forme de Gizmo dans la scène.")]
    public bool showDeadZoneGizmo = true;

    [Tooltip("Couleur du Gizmo de zone morte.")]
    public Color deadZoneGizmoColor = new Color(1f, 0.3f, 0.3f, 0.4f);

    [Header("Mouvement")]
    [Tooltip("Vitesse maximale de déplacement (unités/seconde).")]
    [Min(0f)] public float moveSpeed = 5f;

    [Tooltip("Accélération : rapidité à atteindre la vitesse maximale (0 = instantané).")]
    [Min(0f)] public float acceleration = 10f;

    [Tooltip("Décélération : rapidité à freiner quand dans la zone morte (0 = instantané).")]
    [Min(0f)] public float deceleration = 15f;

    [Tooltip("Axe de déplacement ignoré. 'Y' pour un jeu vue de dessus, 'None' pour un jeu 2.5D.")]
    public LockedAxis lockedAxis = LockedAxis.Y;

    [Header("Rotation")]
    [Tooltip("Le personnage pivote-t-il pour regarder la cible ?")]
    public bool faceTarget = true;

    [Tooltip("Vitesse de rotation en degrés/seconde (0 = instantanée).")]
    [Min(0f)] public float rotationSpeed = 720f;

    [Tooltip("Axe de rotation du personnage.")]
    public RotationAxis rotationAxis = RotationAxis.Y;

    [Header("Lissage de la cible")]
    [Tooltip("Lisse la position cible pour éviter les à-coups (0 = pas de lissage).")]
    [Range(0f, 1f)] public float targetSmoothing = 0f;

    [Header("Debug")]
    public bool showDebugLogs = false;
    public bool showTargetGizmo = true;
    public Color targetGizmoColor = new Color(0.2f, 1f, 0.4f, 0.8f);

    private Vector3             m_CurrentVelocity;
    private Vector3             m_SmoothedTarget;
    private bool                m_HasTarget;
    private Rigidbody           m_Rb;
    private CharacterController m_Cc;
    private Mouse               m_Mouse;
    
    public enum LockedAxis   { None, X, Y, Z }
    public enum RotationAxis { X, Y, Z }

    public Vector3 CurrentVelocity => m_CurrentVelocity;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        m_Rb             = GetComponent<Rigidbody>();
        m_Cc             = GetComponent<CharacterController>();
        m_SmoothedTarget = transform.position;
        m_Mouse          = Mouse.current;

    }

    private void OnEnable()
    {
        // Re-cache au cas où un périphérique serait reconnecté
        m_Mouse = Mouse.current;
    }

    private void Update()
    {
        // Rafraîchit si la souris était absente au démarrage
        if (m_Mouse == null)
        {
            m_Mouse = Mouse.current;
            if (m_Mouse == null) return;
        }

        if (!TryGetMouseWorldPosition(out Vector3 rawTarget)) return;

        // ── Lissage de la cible ──────────────────────────────────
        m_SmoothedTarget = targetSmoothing > 0f
            ? Vector3.Lerp(m_SmoothedTarget, rawTarget, 1f - targetSmoothing)
            : rawTarget;

        Vector3 target   = ApplyLockedAxis(m_SmoothedTarget);
        Vector3 origin   = ApplyLockedAxis(transform.position);
        Vector3 toTarget = target - origin;
        float   distance = toTarget.magnitude;

        bool inDeadZone = distance <= deadZoneRadius;
        m_HasTarget = !inDeadZone;

        if (showDebugLogs)
            Debug.Log($"[MouseFollow] Distance: {distance:F2} | InDeadZone: {inDeadZone} | Speed: {m_CurrentVelocity.magnitude:F2}");

        // ── Vitesse désirée ──────────────────────────────────────
        Vector3 desiredVelocity = inDeadZone
            ? Vector3.zero
            : toTarget.normalized * moveSpeed;

        float accel = inDeadZone ? deceleration : acceleration;

        m_CurrentVelocity = accel <= 0f
            ? desiredVelocity
            : Vector3.MoveTowards(m_CurrentVelocity, desiredVelocity, accel * Time.deltaTime);

        // ── Déplacement ──────────────────────────────────────────
        if (m_Rb != null && !m_Rb.isKinematic)
        {
            Vector3 vel = m_Rb.linearVelocity;
            vel.x = m_CurrentVelocity.x;
            if (lockedAxis != LockedAxis.Y) vel.y = m_CurrentVelocity.y;
            vel.z = m_CurrentVelocity.z;
            m_Rb.linearVelocity = vel;
        }
        else if (m_Cc != null)
        {
            m_Cc.Move(m_CurrentVelocity * Time.deltaTime);
        }
        else
        {
            transform.position += m_CurrentVelocity * Time.deltaTime;
        }

        // ── Rotation ─────────────────────────────────────────────
        if (faceTarget && !inDeadZone && toTarget.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = ComputeLookRotation(toTarget);
            transform.rotation = rotationSpeed <= 0f
                ? lookRot
                : Quaternion.RotateTowards(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
        }
    }



    /// <summary>Convertit la position écran de la souris en position monde via raycast.</summary>
    private bool TryGetMouseWorldPosition(out Vector3 worldPos)
    {
        worldPos = Vector3.zero;
        if (targetCamera == null) return false;

        // ← Correction : New Input System à la place de Input.mousePosition
        Vector2 screenPos = m_Mouse.position.ReadValue();
        Ray ray = targetCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
        {
            worldPos = hit.point;
            return true;
        }

        // Fallback : plan passant par le personnage
        Plane plane = GetFallbackPlane();
        if (plane.Raycast(ray, out float enter))
        {
            worldPos = ray.GetPoint(enter);
            return true;
        }

        return false;
    }

    private Plane GetFallbackPlane()
    {
        return lockedAxis switch
        {
            LockedAxis.Y => new Plane(Vector3.up,      transform.position),
            LockedAxis.X => new Plane(Vector3.right,   transform.position),
            LockedAxis.Z => new Plane(Vector3.forward, transform.position),
            _            => new Plane(Vector3.up,      transform.position),
        };
    }

    /// <summary>Fige l'axe verrouillé sur la valeur du personnage.</summary>
    private Vector3 ApplyLockedAxis(Vector3 v)
    {
        return lockedAxis switch
        {
            LockedAxis.X => new Vector3(transform.position.x, v.y, v.z),
            LockedAxis.Y => new Vector3(v.x, transform.position.y, v.z),
            LockedAxis.Z => new Vector3(v.x, v.y, transform.position.z),
            _            => v,
        };
    }

    private Quaternion ComputeLookRotation(Vector3 direction)
    {
        direction = rotationAxis switch
        {
            RotationAxis.Y => new Vector3(direction.x, 0f,          direction.z),
            RotationAxis.X => new Vector3(0f,          direction.y, direction.z),
            RotationAxis.Z => new Vector3(direction.x, direction.y, 0f),
            _              => direction,
        };

        if (direction == Vector3.zero) return transform.rotation;
        return Quaternion.LookRotation(direction);
    }

    private void OnDrawGizmosSelected()
    {
        if (showDeadZoneGizmo)
        {
            Gizmos.color = deadZoneGizmoColor;
            Gizmos.DrawSphere(transform.position, deadZoneRadius);
            Gizmos.color = new Color(deadZoneGizmoColor.r, deadZoneGizmoColor.g, deadZoneGizmoColor.b, 1f);
            Gizmos.DrawWireSphere(transform.position, deadZoneRadius);
        }

        if (showTargetGizmo && m_HasTarget)
        {
            Gizmos.color = targetGizmoColor;
            Gizmos.DrawSphere(m_SmoothedTarget, 0.15f);
            Gizmos.DrawLine(transform.position, m_SmoothedTarget);
        }
    }
}