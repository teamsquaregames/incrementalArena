using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Utils
{
    public class AnimatorRootMotion : MonoBehaviour
    {
        [TitleGroup("Dependencies")]
        [SerializeField, Required] private Rigidbody m_rigidbody = null;
        [SerializeField, Required] private EntityMovementModule m_movementEM;
        [SerializeField, Required] private Entity m_owner;

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
            Vector3 newPosition = transform.position + m_owner.Animator.deltaPosition;
            m_rigidbody.MovePosition(newPosition);

            Quaternion newRotation = m_rigidbody.rotation * m_owner.Animator.deltaRotation;
            m_rigidbody.MoveRotation(newRotation);
        }
    }
}
