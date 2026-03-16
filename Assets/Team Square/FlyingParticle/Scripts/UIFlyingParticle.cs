using System;
using AssetKits.ParticleImage;
using DG.Tweening;
using Lean.Pool;
using UnityEngine;

public class UIFlyingParticle : MonoBehaviour
{
    [SerializeField] private ParticleImage particleImage;
    [SerializeField] private float logBase = 10f;
    [SerializeField] private float logMultiplier = 5f;
    [SerializeField] private float burstInterval = .1f;

    private Vector3 m_controlPos;
    
    private Action m_callback;

    public async void Initialize(Transform target, Sprite sprite, float duration, Action callback = null, double burstCount = 1)
    {
        particleImage.Stop();
        particleImage.particles.Clear();

        particleImage.attractorTarget = target;
        particleImage.lifetime = duration;

        int scaledBurstCount = ApplyLogScaling(burstCount);
        particleImage.SetBurst(0, 0, 1);

        m_callback = callback;

        if (particleImage != null && sprite != null)
            particleImage.sprite = sprite;

        for (int i = 0; i < scaledBurstCount; i++)
        {
            particleImage.Play();
            await System.Threading.Tasks.Task.Delay((int)(burstInterval * 1000));
        }
    }

    private int ApplyLogScaling(double originalCount)
    {
        if (originalCount <= 0)
            return 0;
        float scaledValue = Mathf.Log((float)originalCount + 7.5f, logBase) * logMultiplier - 23.4f;
        return Mathf.Max(1, Mathf.RoundToInt(scaledValue));
    }

    public void OnFirstParticleArrived()
    {
        if (m_callback != null)
            m_callback?.Invoke();
    }

    public void OnParticleArrived()
    {
        //Debug.Log("Particle arrived at target!");
        SoundManager.Instance.PlaySound(SoundKeys.ui_currency_gain);
    }

    public void OnLastParticleArrived()
    {
        //Debug.Log("Last particle arrived at target!");
        particleImage.Stop();
        LeanPool.Despawn(this);
    }
}