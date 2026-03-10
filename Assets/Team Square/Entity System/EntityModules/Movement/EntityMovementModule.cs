using System;
using Stats;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class EntityMovementModule : EntityModule
{
    [Header("References")]
    [SerializeField] private Rigidbody m_rigidbody;

    private Vector2 m_inputDirection;

    #region MonoBehaviour Methods

    private void Reset()
    {
        CacheReferences();
    }
    
    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float y = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        m_inputDirection = new Vector2(x, y);
    }

    private void FixedUpdate()
    {
        if (m_rigidbody == null) return;
        
        Vector3 moveDir = new Vector3(m_inputDirection.x, 0f, m_inputDirection.y).normalized;
        float moveSpeed = 10;
        
        if (Owner.TryGetModule(out EntityStatModule statModule))
        {
            moveSpeed = statModule.GetValue(StatType.MoveSpeed);
        }
        
        Vector3 targetVelocity = moveDir * moveSpeed;
        m_rigidbody.linearVelocity = new Vector3(targetVelocity.x, m_rigidbody.linearVelocity.y, targetVelocity.z);
    }

    #endregion
    

    #region EntityModule Methods

    public override void CacheReferences()
    {
        base.CacheReferences();
        m_rigidbody = GetComponent<Rigidbody>();
    }

    protected override void OnInitialize()
    {
        if (m_rigidbody != null)
        {
            m_rigidbody.freezeRotation = true;
            m_rigidbody.useGravity = true;
        }
    }

    #endregion
}