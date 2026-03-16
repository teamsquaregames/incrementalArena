using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using Utils;

[Serializable]
public struct ParticleSystems
{
    public ParticleSystem[] particleSystems;
    public float[] delay;
}

public class AttackAnimEM //: AnimEM
{
    // public Action onAttackActive;
    //
    // // [TitleGroup("Dependencies"), Required, Space(10)]
    // // [SerializeField] protected AttackingEM m_attackingEM;
    //
    // [TitleGroup("Dependencies")]
    // [SerializeField] private TrailRenderer m_weaponTrail;
    // // [TitleGroup("Dependencies")]
    // // [SerializeField] protected AbilitiesEm m_abilitiesEM;
    //
    // [TitleGroup("Settings")]
    // [SerializeField] private int m_attackVarAmount = 1;
    // [SerializeField] private float m_comboWindow = 3;
    // [SerializeField] private ParticleSystems[] m_attackVfx;
    //
    //
    // protected bool m_isAttacking;
    // private int m_animHashAttackCancel;
    // private int m_currentAttackVar;
    // private float m_comboWindowTime;
    //
    // private string m_isAttackingHash = "IsAttacking";
    //
    //
    // // protected override void OnModuleInit()
    // // {
    // //     // this.Log($"attack anim init");
    // //     base.OnModuleInit();
    //
    //
    // //     if (m_attackingEM)
    // //     {
    // //         m_attackingEM.onAttack += OnAttack;
    // //         m_attackingEM.onAttackCancel += OnAttackCancel;
    // //         m_animHashAttackCancel = Animator.StringToHash("AttackCancel");
    // //     }
    //
    // //     if (m_abilitiesEM)
    // //     {
    // //         m_abilitiesEM.OnAbilityCast += OnAbilityCast;
    // //     }
    //
    // //     if (m_weaponTrail)
    // //     {
    // //         m_weaponTrail.emitting = false;
    // //     }
    // // }
    //
    // protected override void Update()
    // {
    //     base.Update();
    //
    //     MouseFollowController mouseFollow = GetComponent<MouseFollowController>();
    //     if (mouseFollow)
    //     {
    //         Vector3 localVel = transform.InverseTransformDirection(mouseFollow.CurrentVelocity);
    //         if (localVel.z == 0)
    //             m_animator.SetBool(m_isAttackingHash, true);
    //         else
    //         {
    //             m_animator.SetBool(m_isAttackingHash, false);
    //             m_currentAttackVar = 0;
    //         }
    //
    //     }
    // }
    //
    // [Button]
    // private void OnAttack()
    // {
    //     m_isAttacking = true;
    //
    //     if (m_comboWindowTime > Time.time)
    //         m_currentAttackVar = (m_currentAttackVar + 1) % m_attackVarAmount;
    //     else
    //         m_currentAttackVar = 0;
    //
    //     m_animator.Play($"Attack {m_currentAttackVar}");
    //     m_comboWindowTime = Time.time + m_comboWindow;
    // }
    //
    // public void AttackStart()
    // {
    //     this.Log($"AttackStart. Curnent attack var: {m_currentAttackVar}");
    //
    //     foreach (var vfx in m_attackVfx[m_currentAttackVar].particleSystems)
    //     {
    //         PlayVFXWithDelay(vfx, m_attackVfx[m_currentAttackVar].delay[Array.IndexOf(m_attackVfx[m_currentAttackVar].particleSystems, vfx)]);
    //         this.Log($"Play vfx {vfx.name} with delay {m_attackVfx[m_currentAttackVar].delay[Array.IndexOf(m_attackVfx[m_currentAttackVar].particleSystems, vfx)]}");
    //     }
    //
    //     m_currentAttackVar++;
    // }
    //
    //
    // public void AttackActive()
    // {
    //     if (m_isAttacking)
    //     {
    //         this.Log("AttackActive");
    //         // m_isAttacking = false;
    //         // onAttackActive?.Invoke();
    //     }
    //     else
    //         this.LogWarning("Attack Active, but is not attacking");
    // }
    //
    // public void AttackAnimEnd() { }
    //
    // private void OnAttackCancel()
    // {
    //     if (m_isAttacking)
    //     {
    //         // this.Log($"on attack ccancel");
    //         m_isAttacking = false;
    //         m_animator.SetTrigger(m_animHashAttackCancel);
    //         EndTrail();
    //     }
    // }
    //
    // private void OnAbilityCast(int index)
    // {
    //     // this.Log($"on abilitty cast {index} ");
    //     m_animator.Play($"Ability {index}");
    // }
    //
    //
    // protected override void OnDisplacement(Vector3 velocity)
    // {
    //     ///not opti ?
    //     // if (m_movementPlugin.IsMoving)
    //     // {
    //     //     OnAttackCancel();
    //     // }
    //
    //
    //     base.OnDisplacement(velocity);
    // }
    //
    // protected override void OnDeath()
    // {
    //     // this.Log("On death");
    //     if (m_weaponTrail)
    //         m_weaponTrail.emitting = false;
    //
    //     base.OnDeath();
    // }
    //
    //
    // public void StartTrail()
    // {
    //     if (m_weaponTrail)
    //         m_weaponTrail.emitting = true;
    // }
    //
    // private void EndTrail()
    // {
    //     if (m_weaponTrail)
    //         m_weaponTrail.emitting = false;
    // }
    //
    // private async void PlayVFXWithDelay(ParticleSystem vfx, float delay)
    // {
    //     await Task.Delay(Mathf.RoundToInt(delay * 1000f));
    //     vfx.Play();
    // }
}

