using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;

public class RewardObject : MonoBehaviour, IPoolable
{
    [SerializeField] private RewardConfig m_rewardConfig;
    [SerializeField] private Rigidbody m_rb;
    [SerializeField] private List<Collider> m_colliders;
    [SerializeField, FoldoutGroup("Jump Tween")] private float m_jumpPower = 2f;
    [SerializeField, FoldoutGroup("Jump Tween")] private float m_jumpDuration = 0.4f;

    private double m_value;
    private bool m_isBeingPickedUp;

    public void SetValue(double val)
    {
        m_value = val;
    }

    public void PickUp()
    {
        if (m_isBeingPickedUp) return;
        m_isBeingPickedUp = true;
                
        m_rb.isKinematic = true;
        foreach (Collider col in m_colliders)
            col.enabled = false;

        StartCoroutine(JumpToPlayerCoroutine());
    }

    private IEnumerator JumpToPlayerCoroutine()
    {
        float elapsed = 0f;
        Vector3 flatPosition = transform.position;

        while (elapsed < m_jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / m_jumpDuration);

            if (EntityManager.Instance?.Player != null)
            {
                float remaining = m_jumpDuration - elapsed + Time.deltaTime;
                float lerpFactor = remaining > 0f ? Time.deltaTime / remaining : 1f;
                flatPosition = Vector3.Lerp(flatPosition, EntityManager.Instance.Player.transform.position.OffsetY(1), lerpFactor);
            }

            float arc = Mathf.Sin(t * Mathf.PI) * m_jumpPower;
            transform.position = new Vector3(flatPosition.x, flatPosition.y + arc, flatPosition.z);

            yield return null;
        }

        if (m_rewardConfig != null && m_rewardConfig.CurrencyAsset != null)
            GameData.Instance.AddCurrency(m_rewardConfig.CurrencyAsset, m_value);

        LeanPool.Despawn(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (EntityManager.Instance == null || EntityManager.Instance.Player == null) return;

        if (other == EntityManager.Instance.Player.Collider)
            PickUp();
    }

    public void OnSpawn()
    {
        m_isBeingPickedUp = false;
        
        m_rb.isKinematic = false;
        foreach (Collider col in m_colliders)
            col.enabled = true;

        LevelManager.Instance?.RegisterReward(this);
    }

    public void OnDespawn()
    {
        LevelManager.Instance?.UnregisterReward(this);
    }
}
