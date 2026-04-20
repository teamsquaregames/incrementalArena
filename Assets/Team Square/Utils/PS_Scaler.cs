using System;
using UnityEngine;
using Utils;

public class PS_Scaler : MonoBehaviour
{
    private enum ScaleType
    {
        Scale,
        Rate,
        Burst,
    }


    [SerializeField] private SerializableDictionary<ParticleSystem, ScaleType[]> particleSystemScales;


    public void ScaleParticleSystems(float scaleFactor)
    {
        foreach (var kvp in particleSystemScales)
        {
            ParticleSystem ps = kvp.Key;
            ScaleType[] scalesToApply = kvp.Value;

            if (ps == null)
            {
                this.LogWarning("One of the ParticleSystem references is null. Skipping.");
                continue;
            }

            var mainModule = ps.main;
            var emissionModule = ps.emission;
            var burstModule = emissionModule.burstCount > 0 ? emissionModule.GetBurst(0) : default;

            foreach (var scaleType in scalesToApply)
            {
                switch (scaleType)
                {
                    case ScaleType.Scale:
                        mainModule.startSizeMultiplier *= scaleFactor;
                        break;
                    case ScaleType.Rate:
                        emissionModule.rateOverTimeMultiplier *= scaleFactor;
                        break;
                    case ScaleType.Burst:
                        if (emissionModule.burstCount > 0)
                        {
                            // burstModule.count *= (short)scaleFactor;
                            emissionModule.SetBurst(0, burstModule);
                        }
                        break;
                }
            }
        }
    }
}
