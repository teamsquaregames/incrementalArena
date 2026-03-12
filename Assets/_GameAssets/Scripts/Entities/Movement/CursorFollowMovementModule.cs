using UnityEngine;

public class CursorFollowMovementModule : EntityMovementModule
{
    private void Update()
    {
        if (CursorManager.Instance == null) return;

        Vector3 target = CursorManager.Instance.MouseWorldPosition;
        Vector3 delta = target - transform.position;
        Vector2 flatDelta = new Vector2(delta.x, delta.z);

        float stopRadius = 0.2f;
        SetMoveInput(flatDelta.sqrMagnitude > stopRadius * stopRadius ? flatDelta.normalized : Vector2.zero);
    }
}