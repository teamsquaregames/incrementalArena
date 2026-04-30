using UnityEngine;
using Sirenix.OdinInspector;
using Utils;
using System.Collections.Generic;

namespace Utils.RendererEffect
{
    public class _RendererEffect : MonoBehaviour
    {
        [TitleGroup("Dependencies")]
        [SerializeField, Required] protected Renderer[] m_renderers;

        [Button]
        protected virtual void FillRenderers()
        {
            m_renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        }
    }
}
