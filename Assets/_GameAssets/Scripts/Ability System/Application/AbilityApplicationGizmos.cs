using UnityEngine;
using Sirenix.OdinInspector;


public class AbilityApplicationGizmos : MonoBehaviour
{
    [TitleGroup("Debug Visualization")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] public AbilityApplicationInfo applicationInfo;
    [SerializeField] public TargetingInfo targetingInfo;
    [SerializeField] private Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color wireColor = Color.red;
    [SerializeField] public Vector3 position;
    
    private void OnDrawGizmos()
    {
        if (!showGizmos || applicationInfo == null) return;
        
        
        // Dessine selon le type d'application
        switch (applicationInfo.effectZoneType)
        {
            case AbilityApplicationInfo.Type.Direct:
                DrawDirectGizmo(position);
                break;
            case AbilityApplicationInfo.Type.Aoe:
                DrawAoeGizmo(position);
                break;
            case AbilityApplicationInfo.Type.Projectile:
                DrawProjectileGizmo(position);
                break;
            case AbilityApplicationInfo.Type.Custom:
                DrawCustomGizmo(position);
                break;
        }
    }
    
    private void DrawDirectGizmo(Vector3 position)
    {
        // Ligne vers la cible
        Gizmos.color = wireColor;
        Gizmos.DrawLine(transform.position, position);
        
        // Petite sphère à la position cible
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(position, 0.3f);
        
        Gizmos.color = wireColor;
        Gizmos.DrawWireSphere(position, 0.3f);
    }
    
    private void DrawAoeGizmo(Vector3 position)
    {
        if (applicationInfo.aoeInfo == null) return;

        Quaternion rotation = transform != null ? transform.rotation : Quaternion.identity;
        Vector3 rotatedOffset = rotation * applicationInfo.aoeInfo.offset;
        Vector3 center = position + rotatedOffset;
        
        switch (applicationInfo.aoeInfo.effectShape)
        {
            case AoEInfo.Shape.Circle:
                DrawCircleAoe(center);
                break;
            case AoEInfo.Shape.Rect:
                DrawRectAoe(center, rotation);
                break;
            case AoEInfo.Shape.Cone:
                DrawConeAoe(center, rotation);
                break;
        }
    }
    
    private void DrawCircleAoe(Vector3 position)
    {
        float radius = applicationInfo.aoeInfo.Radius();
        
        // Sphère pleine semi-transparente
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(position, radius);
        
        // Contour wire
        Gizmos.color = wireColor;
        Gizmos.DrawWireSphere(position, radius);
        
        // Cercle au sol (plus visible)
        DrawCircleOnGround(position, radius, wireColor);
    }
    
    private void DrawRectAoe(Vector3 position, Quaternion rotation)
    {
        Vector3 size = applicationInfo.aoeInfo.scale;

        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);

        // Cube plein semi-transparent
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(Vector3.zero, size);

        // Contour wire
        Gizmos.color = wireColor;
        Gizmos.DrawWireCube(Vector3.zero, size);

        Gizmos.matrix = previousMatrix;
    }
    
    private void DrawConeAoe(Vector3 position, Quaternion rotation)
    {
        float radius = applicationInfo.aoeInfo.Radius();
        float angle = applicationInfo.aoeInfo.angle;
        Vector3 forward = rotation * Vector3.forward;
        
        // Point de départ du cône
        Gizmos.color = wireColor;
        
        // Dessine les bords du cône
        int segments = 20;
        Vector3 previousPoint = Vector3.zero;
        
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = (i / (float)segments) * angle - angle / 2f;
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 endPoint = position + direction * radius;
            
            // Ligne depuis le centre
            Gizmos.DrawLine(position, endPoint);
            
            // Arc à l'extrémité
            if (i > 0)
            {
                Gizmos.DrawLine(previousPoint, endPoint);
            }
            
            previousPoint = endPoint;
        }
        
        // Zone semi-transparente
        Gizmos.color = gizmoColor;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * angle - angle / 2f;
            float angle2 = ((i + 1) / (float)segments) * angle - angle / 2f;
            
            Vector3 dir1 = Quaternion.Euler(0, angle1, 0) * forward;
            Vector3 dir2 = Quaternion.Euler(0, angle2, 0) * forward;
            
            Vector3 p1 = position + dir1 * radius;
            Vector3 p2 = position + dir2 * radius;
            
            // Triangle du cône (simplifié)
            Gizmos.DrawLine(position, p1);
            Gizmos.DrawLine(position, p2);
            Gizmos.DrawLine(p1, p2);
        }
    }
    
    private void DrawProjectileGizmo(Vector3 position)
    {
        // Ligne de trajectoire
        Gizmos.color = wireColor;
        Gizmos.DrawLine(transform.position, position);
        
        // Flèche à la fin
        DrawArrow(transform.position, position, wireColor);
        
        // Petite sphère au départ et à l'arrivée
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.2f);
        Gizmos.DrawSphere(position, 0.2f);
    }
    
    private void DrawCustomGizmo(Vector3 position)
    {
        // Gizmo par défaut pour custom
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(position, 1f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, 1f);
    }
    
    // Helpers
    private void DrawCircleOnGround(Vector3 center, float radius, Color color)
    {
        int segments = 32;
        Vector3 previousPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            
            Gizmos.color = color;
            Gizmos.DrawLine(previousPoint, newPoint);
            previousPoint = newPoint;
        }
    }
    
    private void DrawArrow(Vector3 from, Vector3 to, Color color)
    {
        Vector3 direction = (to - from).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
        
        float arrowHeadLength = 0.5f;
        float arrowHeadAngle = 20f;
        
        Vector3 arrowTip = to;
        Vector3 arrowBase = to - direction * arrowHeadLength;
        
        Vector3 arrowSide1 = arrowBase + Quaternion.Euler(0, arrowHeadAngle, 0) * -direction * arrowHeadLength * 0.5f;
        Vector3 arrowSide2 = arrowBase + Quaternion.Euler(0, -arrowHeadAngle, 0) * -direction * arrowHeadLength * 0.5f;
        
        Gizmos.color = color;
        Gizmos.DrawLine(arrowTip, arrowSide1);
        Gizmos.DrawLine(arrowTip, arrowSide2);
    }
}