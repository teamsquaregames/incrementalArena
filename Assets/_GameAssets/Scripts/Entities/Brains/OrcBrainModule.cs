using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;

public class OrcBrainModule : EntityBrainModule
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float m_stopRadius = 0.2f;

    [ReadOnly]
    [SerializeField] private bool m_isMoving;

    protected override void Think()
    {
        if (!Owner.TryGetModule(out EntityAbilityModule abilityModule)) return;
        if (Owner.IsStaggered)
        {
            StopMovement();
            return;
        }

        // While a non-auto ability is animating, block input.
        if (abilityModule.IsUsingAbility) return;

        Entity player = EntityManager.Instance.Player;
        if (player == null) return;

        Vector3 playerPos = player.transform.position;

        float distanceToPlayer = Vector3.Distance(
            new Vector3(Owner.transform.position.x, 0f, Owner.transform.position.z),
            new Vector3(playerPos.x, 0f, playerPos.z));

        bool inAttackRange = distanceToPlayer <= abilityModule.AutoAttack.range;
        
        //MoveToward(playerPos);
        
        if (inAttackRange)
        {
            m_isMoving = false;
            StopMovement();
            FacePosition(playerPos);
            TryAutoAttack(playerPos.SetY(0f));
        }
        else
        {
            abilityModule.CancelEverything();
            MoveToward(playerPos);
        }
    }

    private void MoveToward(Vector3 worldTarget)
    {
        Vector3 delta = worldTarget - Owner.transform.position;
        Vector2 flatDelta = new Vector2(delta.x, delta.z);

        if (flatDelta.sqrMagnitude > m_stopRadius * m_stopRadius)
        {
            m_isMoving = true;
            SetMoveInput(flatDelta.normalized);
        }
        else
        {
            m_isMoving = false;
            SetMoveInput(Vector2.zero);
        }
    }
}
