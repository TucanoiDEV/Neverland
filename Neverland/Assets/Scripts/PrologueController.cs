using UnityEngine;

/// <summary>
/// Controlador do prólogo — O Quarto Real (GDD §4.1).
/// Cobre apenas os dois verbos essenciais deste momento: OLHAR (free-look
/// limitado — o Wendy está deitado na cama, não anda) e ENCOLHER-SE NA CAMA
/// (segurar para recolher o enquadramento e estreitar o campo de visão).
/// Sem locomoção, fôlego ou inventário: nada disso pertence ao prólogo.
/// Anexar à Main Camera.
/// </summary>
public class PrologueController : MonoBehaviour
{
    [Header("Olhar (free-look limitado)")]
    [SerializeField] private float lookSensitivity = 2f;
    [Tooltip("Arco horizontal em graus para cada lado a partir do enquadramento inicial.")]
    [SerializeField] private float yawLimit = 55f;
    [Tooltip("Arco vertical em graus para cima/baixo a partir do enquadramento inicial.")]
    [SerializeField] private float pitchLimit = 40f;

    [Header("Encolher-se na cama")]
    [SerializeField] private KeyCode huddleKey = KeyCode.LeftControl;
    [Tooltip("Quanto a cabeça desce (em metros) ao se encolher.")]
    [SerializeField] private float huddleDrop = 0.18f;
    [Tooltip("Arco horizontal reduzido enquanto encolhido.")]
    [SerializeField] private float huddleYawLimit = 20f;
    [Tooltip("Arco vertical reduzido enquanto encolhido.")]
    [SerializeField] private float huddlePitchLimit = 18f;
    [Tooltip("Velocidade da transição entre deitado e encolhido.")]
    [SerializeField] private float transitionSpeed = 4f;

    private float yaw;    // deslocamento relativo ao enquadramento inicial
    private float pitch;
    private Quaternion baseRotation;
    private Vector3 baseLocalPosition;
    private float huddleBlend; // 0 = deitado normal, 1 = totalmente encolhido

    private void Awake()
    {
        baseRotation = transform.localRotation;
        baseLocalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        UpdateHuddle();
        UpdateLook();
    }

    // ENCOLHER-SE: segurar a tecla recolhe a cabeça e estreita o olhar.
    private void UpdateHuddle()
    {
        float target = Input.GetKey(huddleKey) ? 1f : 0f;
        huddleBlend = Mathf.MoveTowards(huddleBlend, target, transitionSpeed * Time.deltaTime);
        transform.localPosition = baseLocalPosition + Vector3.down * (huddleDrop * huddleBlend);
    }

    // OLHAR: free-look com clamp; o arco encolhe conforme o Wendy se encolhe.
    private void UpdateLook()
    {
        yaw += Input.GetAxis("Mouse X") * lookSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;

        float currentYawLimit = Mathf.Lerp(yawLimit, huddleYawLimit, huddleBlend);
        float currentPitchLimit = Mathf.Lerp(pitchLimit, huddlePitchLimit, huddleBlend);

        yaw = Mathf.Clamp(yaw, -currentYawLimit, currentYawLimit);
        pitch = Mathf.Clamp(pitch, -currentPitchLimit, currentPitchLimit);

        transform.localRotation = baseRotation * Quaternion.Euler(pitch, yaw, 0f);
    }
}
