using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Utils
{
    public class AnimatorRootMotion : MonoBehaviour
    {
        [TitleGroup("Dependencies")]
        [Required]
        [SerializeField] Rigidbody _rigidbody = null;
        [Required]
        [SerializeField] EntityMovementModule m_movementEM;

        [TitleGroup("Settings")]
        public bool ApplyRootMotion = true;

        Animator _anim = null;

        // Start is called before the first frame update
        void Start()
        {
            _anim = GetComponent<Animator>();
        }

        void OnAnimatorMove()
        {
            if (ApplyRootMotion && !m_movementEM.MoveSpeed.Equals(0f))
            {
                MoveRootMotion();
            }
        }

        void MoveRootMotion()
        {
            Vector3 newPosition = transform.position + _anim.deltaPosition;
            _rigidbody.MovePosition(newPosition);

            Quaternion newRotation = _rigidbody.rotation * _anim.deltaRotation;
            _rigidbody.MoveRotation(newRotation);
        }
    }
}
