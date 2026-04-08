using System.Threading.Tasks;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;


public class NpcBrainModule : EntityBrainModule
{
    protected enum NpcState
    {
        None,
        Idle,
        Agressive,
        Fleeing
    }

    [TitleGroup("Dependencies")]
    [SerializeField, Required] private Animator m_animator;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float m_stopRadius = 0.2f;

    [Header("Behaviour")]
    [SerializeField] private float m_averageIdleDuration = 5f;

    [Header("Debug")]
    [SerializeField, ReadOnly] private bool m_isMoving;
    [SerializeField, ReadOnly] private NpcState m_currentState;


    public override void CacheReferences()
    {
        m_animator = GetComponentInChildren<Animator>();
    }


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

        switch (m_currentState)
        {
            case NpcState.None:
                RandomState();
                break;
            case NpcState.Idle:
                FacePosition(playerPos);
                break;
            case NpcState.Agressive:
                AgressiveBehaviour(distanceToPlayer, playerPos, abilityModule);
                break;
            case NpcState.Fleeing:
                // MoveToward(Owner.transform.position - (playerPos - Owner.transform.position));
                break;
        }
    }

    private void AgressiveBehaviour(float distanceToPlayer, Vector3 playerPos, EntityAbilityModule abilityModule)
    {
        bool inAttackRange = distanceToPlayer <= abilityModule.AutoAttack.range.y;

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

    private void RandomState()
    {
        /// select random stat of the enum
        m_currentState = (NpcState)Random.Range(0, System.Enum.GetValues(typeof(NpcState)).Length);

        switch (m_currentState)
        {
            case NpcState.Idle:
                EnterIdleState();
                break;
            case NpcState.Agressive:
                EnterAgressiveState();
                break;
            case NpcState.Fleeing:
                /// do nothing, fleeing will be handled in the Think method
                RandomState();
                break;
        }
    }

    private async Task EnterIdleState()
    {
        this.Log("Entering Idle State");
        m_currentState = NpcState.Idle;

        StopMovement();
        m_animator.SetTrigger("Rallying");

        float idleDuration = CusMath.RngGaussian() * (m_averageIdleDuration * 2f);
        await Task.Delay((int)(idleDuration * 1000));
        RandomState();
    }


    private async Task EnterAgressiveState()
    {
        m_currentState = NpcState.Agressive;

        StopMovement();
        float idleDuration = CusMath.RngGaussian() * (m_averageIdleDuration * 2f);
        await Task.Delay((int)(idleDuration * 1000));
        RandomState();
    }
}
