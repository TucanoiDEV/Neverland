using UnityEngine;

/// <summary>
/// O olhar do Wendy (GDD §5.1, §9.4).
///
/// Divisão de trabalho: o eixo horizontal gira o CORPO (o Player), o vertical
/// gira só o CameraRig. É o que faz o movimento do PlayerMotor sair sempre na
/// direção para onde o menino está olhando, sem que olhar para cima o faça
/// andar para o céu.
///
/// O FOV padrão é 70° e é ajustável de 60 a 90 nas opções (§9.4) — o campo
/// existe aqui para que a tela de opções tenha onde escrever quando existir.
///
/// Anexar ao GameObject "Player" (o mesmo do PlayerMotor).
/// </summary>
public class PlayerLook : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("O CameraRig — recebe o pitch (olhar para cima/baixo).")]
    [SerializeField] private Transform cameraRig;
    [Tooltip("A câmera do jogador — é nela que o FOV é aplicado.")]
    [SerializeField] private Camera playerCamera;

    [Header("Sensibilidade")]
    [Tooltip("Graus por unidade de movimento do mouse.")]
    [SerializeField] private float sensitivity = 2.2f;
    [Tooltip("Inverter o eixo vertical (§9.4, acessibilidade).")]
    [SerializeField] private bool invertY = false;
    [Tooltip("Suavização do olhar. 0 = cru e imediato. Um toque de suavização " +
             "combina com a estética PS1, mas exagerar dá sensação de gelo.")]
    [Range(0f, 0.3f)][SerializeField] private float smoothing = 0.03f;

    [Header("Limites")]
    [Tooltip("Quanto o Wendy pode olhar para cima (graus).")]
    [SerializeField] private float maxLookUp = 85f;
    [Tooltip("Quanto o Wendy pode olhar para baixo (graus).")]
    [SerializeField] private float maxLookDown = 85f;

    [Header("Campo de visão (§9.4)")]
    [Tooltip("FOV padrão do jogo. O GDD crava 70°, ajustável 60–90 nas opções.")]
    [Range(60f, 90f)][SerializeField] private float fieldOfView = 70f;

    [Header("Cursor")]
    [Tooltip("Prender e esconder o cursor ao iniciar.")]
    [SerializeField] private bool lockCursor = true;

    private float yaw;      // rotação do corpo
    private float pitch;    // rotação do rig
    private Vector2 smoothedDelta;
    private Vector2 deltaVelocity;

    /// <summary>
    /// Tira o olhar do jogador. O despertar e as cutscenes de câmera fixa
    /// (§5.1) ligam isto para que a mão do jogador não puxe o enquadramento.
    /// </summary>
    public bool LookLocked { get; set; }

    /// <summary>Pitch atual em graus (negativo = olhando para cima).</summary>
    public float Pitch => pitch;

    /// <summary>A câmera do jogador, para quem precisa dela (raycast, cutscene).</summary>
    public Camera PlayerCamera => playerCamera;

    private void Awake()
    {
        yaw = transform.eulerAngles.y;

        if (cameraRig != null)
        {
            // Respeita a pose autorada na cena em vez de zerar o rig no play.
            float authored = cameraRig.localEulerAngles.x;
            pitch = authored > 180f ? authored - 360f : authored;
        }
        else
        {
            Debug.LogWarning("[PlayerLook] Sem CameraRig: não há como olhar " +
                             "para cima ou para baixo.", this);
        }

        if (playerCamera != null)
            playerCamera.fieldOfView = fieldOfView;
    }

    private void Start()
    {
        if (lockCursor)
            SetCursorLocked(true);
    }

    private void Update()
    {
        if (!LookLocked)
            ReadMouse();

        ApplyRotation();
    }

    private void ReadMouse()
    {
        Vector2 raw = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y"));

        // O mouse já é um delta por frame: multiplicar por deltaTime aqui
        // deixaria a mira dependente do frame rate. Só a suavização usa tempo.
        smoothedDelta = smoothing > 0f
            ? Vector2.SmoothDamp(smoothedDelta, raw, ref deltaVelocity, smoothing)
            : raw;

        yaw += smoothedDelta.x * sensitivity;

        float vertical = smoothedDelta.y * sensitivity * (invertY ? 1f : -1f);
        pitch = Mathf.Clamp(pitch + vertical, -maxLookUp, maxLookDown);
    }

    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        if (cameraRig != null)
            cameraRig.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    /// <summary>
    /// Aponta o olhar na hora, sem animação. O despertar usa para começar a
    /// cena encarando o céu impossível (§2.3).
    /// </summary>
    public void SetPitch(float degrees)
    {
        pitch = Mathf.Clamp(degrees, -maxLookUp, maxLookDown);
        ApplyRotation();
    }

    /// <summary>Gira o corpo na hora (spawn, checkpoint, cutscene).</summary>
    public void SetYaw(float degrees)
    {
        yaw = degrees;
        ApplyRotation();
    }

    /// <summary>Troca o FOV — o gancho da tela de opções (§9.4).</summary>
    public void SetFieldOfView(float fov)
    {
        fieldOfView = Mathf.Clamp(fov, 60f, 90f);

        if (playerCamera != null)
            playerCamera.fieldOfView = fieldOfView;
    }

    /// <summary>Prende ou solta o cursor — para menus e para o editor.</summary>
    public void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
