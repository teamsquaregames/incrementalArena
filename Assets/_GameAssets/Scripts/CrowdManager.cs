using System;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class CrowdManager : MonoBehaviour
{
    [SerializeField] private Animator[] m_crowdAnimators;

    [SerializeField] private float m_cheerMaxDelay = 0.5f;
    [SerializeField] private int m_cheerVarAmount = 1;
    [SerializeField] private float m_cheerCrossFadeDuration = 0.1f;

     void Start()
    {
        m_crowdAnimators = GetComponentsInChildren<Animator>();
    }

    [Button]
    public void CrowdCheer()
    {
        foreach (var animator in m_crowdAnimators)
        {
            float randomDelay = UnityEngine.Random.Range(0f, m_cheerMaxDelay);
            PlayCheer(animator, randomDelay);
        }
    }

    private async void PlayCheer(Animator animator, float delay = 0f)
    {
        if (delay > 0f)
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(delay));

        int cheerIndex = UnityEngine.Random.Range(0, m_cheerVarAmount);
        animator.CrossFade($"Cheer {cheerIndex}", m_cheerCrossFadeDuration);
    }

    [Button]
    public void CrowdIdle()
    {
        foreach (var animator in m_crowdAnimators)
        {
            float randomDelay = UnityEngine.Random.Range(0f, m_cheerMaxDelay);
            PlayIdle(animator, randomDelay);
        }
    }

    private async void PlayIdle(Animator animator, float delay = 0f)
    {
        if (delay > 0f)
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(delay));

        animator.SetTrigger("Idle");
    }
}
