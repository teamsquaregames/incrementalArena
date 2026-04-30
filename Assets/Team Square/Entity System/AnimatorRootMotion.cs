using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Utils
{
    public class AnimatorRootMotion : MonoBehaviour
    {
        [TitleGroup("Dependencies")]
        [SerializeField, Required] Rigidbody m_rigidbody = null;
        [SerializeField, Required] EntityMovementModule m_movementEM;
        [SerializeField, Required]private Entity Owner;

        [TitleGroup("Settings")]
        public bool ApplyRootMotion = true;


        void OnAnimatorMove()
        {
            if (ApplyRootMotion && !m_movementEM.MoveSpeed.Equals(0f))
            {
                MoveRootMotion();
            }
        }

        void MoveRootMotion()
        {
            Vector3 newPosition = transform.position + Owner.Animator.deltaPosition;
            m_rigidbody.MovePosition(newPosition);

            Quaternion newRotation = m_rigidbody.rotation * Owner.Animator.deltaRotation;
            m_rigidbody.MoveRotation(newRotation);
        }
    }
}
