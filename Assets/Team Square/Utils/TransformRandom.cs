using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class TransformRandom : MonoBehaviour
{
    [Title("Position Settings")]
    [SerializeField] private Vector3 m_positionMin = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 m_positionMax = new Vector3(0, 0, 0);

    [TitleGroup("Rotation Settings")]
    [SerializeField] private Vector3 m_rotationMin = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 m_rotationMax = new Vector3(0, 360, 0);
    [SerializeField] private Vector3 m_rotationSpeedMin = new Vector3(0, 90, 0);
    [SerializeField] private Vector3 m_rotationSpeedMax = new Vector3(0, 180, 0);

    [TitleGroup("Scale Settings")]
    [SerializeField] private Vector3 m_scaleMin = new Vector3(.9f, .9f, .9f);
    [SerializeField] private Vector3 m_scaleMax = new Vector3(1.1f, 1.1f, 1.1f);

    [Title("Debug")]
    [SerializeField] private bool m_updateInEditor = false;

    private Vector3 m_rotationSpeed;
    
    [Button("Randomize Transform")]
    void Start()
    {
        transform.localPosition = Vector3.Lerp(m_positionMin, m_positionMax, Random.value);

        transform.localRotation = Quaternion.Euler(Vector3.Lerp(m_rotationMin, m_rotationMax, Random.value));
        m_rotationSpeed = Vector3.Lerp(m_rotationSpeedMin, m_rotationSpeedMax, Random.value);

        transform.localScale = Vector3.Lerp(m_scaleMin, m_scaleMax, Random.value);
    }


    void FixedUpdate()
    {
        // Ne mettre à jour que si on est en jeu OU en editeur avec le flag activé
        if (!Application.isPlaying && !m_updateInEditor)
            return;

        /// rotation
        var rot = transform.localRotation.eulerAngles;
        rot += m_rotationSpeed * Time.fixedDeltaTime;
        transform.localRotation = Quaternion.Euler(rot);
    }
}
