using UnityEngine;

/// <summary>
/// O corpo do Wendy (GDD §5.1, §5.2, §5.3).
///
/// Uma criança de 9 anos, franzina e desarmada: anda devagar, corre pouco e
/// paga caro por isso. Três velocidades e nada mais — não há pulo, não há
/// esquiva, não há golpe. Todo o "poder" do jogador está em escolher QUAL das
/// três usar e quando (§1.3, pilar da vulnerabilidade absoluta).
///
/// O fôlego é uma barra INVISÍVEL (§9.1): o jogador só descobre que acabou
/// porque o Wendy arfa alto — e arfar alto dobra o raio de ruído (§5.3).
/// Correr é sempre uma dívida.
///
/// Este script controla:
///   · o CharacterController (movimento e gravidade);
///   · a ALTURA do colisor ao agachar;
///   · a ALTURA local do CameraRig (1,20 m em pé — escala de criança, §5.1).
/// Ele NÃO controla o pitch da câmera (PlayerLook) nem o head-bob (CameraBob),
/// para que os três possam escrever em transforms diferentes sem brigar:
///   Player      → posição (aqui)
///   CameraRig   → altura (aqui) + pitch (PlayerLook)
///   PlayerCamera→ offset de bob (CameraBob)
///
/// Anexar ao GameObject "Player", junto com o CharacterController.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("O CameraRig — filho do Player. É a altura DELE que sobe e desce " +
             "entre ficar em pé e agachar. A câmera propriamente dita é filha " +
             "do rig e fica livre para o head-bob.")]
    [SerializeField] private Transform cameraRig;

    [Header("Velocidades (§5.2)")]
    [Tooltip("Caminhada — a velocidade padrão do jogo (m/s).")]
    [SerializeField] private float walkSpeed = 2.2f;
    [Tooltip("Corrida — consome fôlego e é MUITO barulhenta (m/s).")]
    [SerializeField] private float runSpeed = 3.6f;
    [Tooltip("Agachado/rastejando — quase silencioso; é a rota das passagens " +
             "de criança (m/s).")]
    [SerializeField] private float crouchSpeed = 1.1f;
    [Tooltip("Suavização entre velocidades (m/s²). Alto demais deixa o " +
             "movimento 'de patins'; baixo demais e a criança vira caminhão.")]
    [SerializeField] private float acceleration = 14f;

    [Header("Fôlego (§5.3) — invisível, sempre")]
    [Tooltip("Segundos de corrida contínua até o fôlego acabar.")]
    [SerializeField] private float runSeconds = 6f;
    [Tooltip("Segundos para recuperar o fôlego inteiro PARADO.")]
    [SerializeField] private float recoverIdleSeconds = 8f;
    [Tooltip("Segundos para recuperar o fôlego inteiro ANDANDO.")]
    [SerializeField] private float recoverWalkSeconds = 12f;
    [Tooltip("Fração de fôlego necessária para poder correr DE NOVO depois de " +
             "zerar. Existe para impedir o 'corre-para-corre-para' que " +
             "burlaria o custo da corrida: enquanto não chegar aqui, o Wendy " +
             "continua arfando alto e não obedece ao Shift.")]
    [Range(0.05f, 1f)][SerializeField] private float minStaminaToRunAgain = 0.25f;

    [Header("Corpo de criança")]
    [Tooltip("Altura do colisor em pé (m). O Wendy tem ~1,30 m.")]
    [SerializeField] private float standingHeight = 1.3f;
    [Tooltip("Altura do colisor agachado (m) — o que passa por vãos e dutos.")]
    [SerializeField] private float crouchingHeight = 0.75f;
    [Tooltip("Altura da câmera em pé (m). O GDD crava 1,20: maçanetas na " +
             "linha dos olhos e todo adulto 'para cima' (§5.1).")]
    [SerializeField] private float cameraStandHeight = 1.2f;
    [Tooltip("Altura da câmera agachado (m).")]
    [SerializeField] private float cameraCrouchHeight = 0.62f;
    [Tooltip("Velocidade da transição de altura ao agachar/levantar.")]
    [SerializeField] private float heightChangeSpeed = 8f;

    [Header("Gravidade")]
    [Tooltip("Gravidade aplicada (m/s²). Mais forte que a real porque queda " +
             "flutuante estraga o peso de um corpo pequeno.")]
    [SerializeField] private float gravity = -14f;
    [Tooltip("Força para baixo aplicada quando já está no chão — mantém o " +
             "CharacterController colado em rampas e degraus.")]
    [SerializeField] private float groundedStick = -3f;

    [Header("Teclas (§5.2 — Input legado por ora)")]
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [Tooltip("Marcado (padrão, §5.2): Ctrl ALTERNA agachar. Desmarcado: " +
             "segurar para ficar agachado.")]
    [SerializeField] private bool crouchIsToggle = true;

    [Header("Ruído (tabela 6.1) — ainda não consumido por ninguém")]
    [Tooltip("Raio de ruído rastejando/agachado (m).")]
    [SerializeField] private float noiseRadiusCrouch = 3f;
    [Tooltip("Raio de ruído andando (m).")]
    [SerializeField] private float noiseRadiusWalk = 9f;
    [Tooltip("Raio de ruído correndo (m).")]
    [SerializeField] private float noiseRadiusRun = 16f;
    [Tooltip("Multiplicador do raio enquanto o Wendy arfa sem fôlego (§5.3).")]
    [SerializeField] private float windedNoiseMultiplier = 2f;

    private CharacterController controller;
    private Vector3 horizontalVelocity;      // velocidade no plano XZ
    private float verticalVelocity;          // gravidade acumulada
    private float stamina = 1f;              // 0..1, invisível
    private bool winded;                     // sem fôlego: arfa alto e não corre
    private bool crouching;
    private bool running;
    private float currentHeight;             // altura interpolada do colisor
    private bool cameraHeightOverridden;     // o roteiro assumiu a altura da câmera
    private float overriddenCameraHeight;

    /// <summary>
    /// Trava a corrida. É a regra do §6.2: quando o Peter Pan está em cena, o
    /// input de corrida some — o corpo do jogador obedece antes do jogador.
    /// </summary>
    public bool RunLocked { get; set; }

    /// <summary>
    /// Tira o movimento do jogador (despertar, diálogo, cutscene). A gravidade
    /// continua: travado não é flutuando.
    /// </summary>
    public bool MoveLocked { get; set; }

    /// <summary>True enquanto o Shift está surtindo efeito de verdade.</summary>
    public bool IsRunning => running;

    /// <summary>True enquanto agachado — o estado das passagens de criança.</summary>
    public bool IsCrouching => crouching;

    /// <summary>True quando há deslocamento real no plano.</summary>
    public bool IsMoving => horizontalVelocity.sqrMagnitude > 0.01f;

    /// <summary>
    /// True enquanto o fôlego está no fundo: o Wendy arfa ALTO e o ruído dobra
    /// (§5.3). Sai deste estado ao recuperar 'minStaminaToRunAgain'.
    /// </summary>
    public bool IsWinded => winded;

    /// <summary>Fôlego de 0 a 1. NUNCA vai para a tela (§9.1) — é para o áudio.</summary>
    public float StaminaNormalized => stamina;

    /// <summary>
    /// A altura da câmera em pé (§5.1: 1,20 m). Fonte única da estatura do
    /// Wendy — quem precisa levantá-lo do chão lê daqui em vez de repetir o
    /// número.
    /// </summary>
    public float CameraStandHeight => cameraStandHeight;

    /// <summary>Velocidade atual como fração da corrida — para o head-bob.</summary>
    public float SpeedNormalized =>
        runSpeed > 0f ? Mathf.Clamp01(horizontalVelocity.magnitude / runSpeed) : 0f;

    /// <summary>
    /// Raio de ruído que o Wendy emite AGORA, em metros (tabela 6.1). Parado é
    /// zero. Ainda ninguém escuta — a Sininho entra na noite —, mas o valor já
    /// existe para o áudio de passos e para a IA achar pronto quando chegar.
    /// </summary>
    public float CurrentNoiseRadius
    {
        get
        {
            if (!IsMoving)
                return 0f;

            float radius = crouching ? noiseRadiusCrouch
                         : running ? noiseRadiusRun
                         : noiseRadiusWalk;

            return winded ? radius * windedNoiseMultiplier : radius;
        }
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        currentHeight = standingHeight;
        ApplyHeight(standingHeight);

        if (cameraRig != null)
            SetCameraRigHeight(cameraStandHeight);
        else
            Debug.LogWarning("[PlayerMotor] Sem CameraRig: a câmera não vai " +
                             "agachar junto com o corpo.", this);
    }

    private void Update()
    {
        ReadCrouchInput();
        UpdateStamina();
        Move();
        UpdateHeights();
    }

    // Ctrl alterna (ou segura) — mas levantar só acontece se houver teto para isso.
    private void ReadCrouchInput()
    {
        if (MoveLocked)
            return;

        bool wants = crouchIsToggle
            ? (Input.GetKeyDown(crouchKey) ? !crouching : crouching)
            : Input.GetKey(crouchKey);

        if (!wants && crouching && !CanStandUp())
            wants = true; // há teto: continua agachado, e o jogador entende por quê

        crouching = wants;
    }

    private void UpdateStamina()
    {
        // Correr só drena de fato quando há deslocamento: parar com Shift
        // apertado não custa nada.
        if (running && IsMoving)
        {
            stamina -= Time.deltaTime / Mathf.Max(0.01f, runSeconds);

            if (stamina <= 0f)
            {
                stamina = 0f;
                winded = true;
            }
        }
        else
        {
            float seconds = IsMoving ? recoverWalkSeconds : recoverIdleSeconds;
            stamina += Time.deltaTime / Mathf.Max(0.01f, seconds);
            stamina = Mathf.Clamp01(stamina);

            if (winded && stamina >= minStaminaToRunAgain)
                winded = false;
        }
    }

    private void Move()
    {
        Vector3 wish = Vector3.zero;

        if (!MoveLocked)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            wish = transform.right * x + transform.forward * z;
            if (wish.sqrMagnitude > 1f)
                wish.Normalize(); // diagonal não é mais rápida
        }

        // Correr exige: tecla, fôlego, não estar agachado e o roteiro permitir.
        running = !MoveLocked
                  && !RunLocked
                  && !crouching
                  && !winded
                  && Input.GetKey(runKey)
                  && wish.sqrMagnitude > 0.01f;

        float targetSpeed = crouching ? crouchSpeed
                          : running ? runSpeed
                          : walkSpeed;

        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity, wish * targetSpeed, acceleration * Time.deltaTime);

        // Gravidade: colado no chão quando há chão, acumulando quando não há.
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = groundedStick;
        else
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 motion = horizontalVelocity;
        motion.y = verticalVelocity;
        controller.Move(motion * Time.deltaTime);
    }

    // Colisor e câmera perseguem a altura-alvo no mesmo ritmo, para que agachar
    // seja um movimento só e não duas coisas fora de sincronia.
    private void UpdateHeights()
    {
        float targetHeight = crouching ? crouchingHeight : standingHeight;
        currentHeight = Mathf.MoveTowards(
            currentHeight, targetHeight, heightChangeSpeed * Time.deltaTime);
        ApplyHeight(currentHeight);

        if (cameraRig == null)
            return;

        if (cameraHeightOverridden)
        {
            SetCameraRigHeight(overriddenCameraHeight);
            return;
        }

        float targetCamera = crouching ? cameraCrouchHeight : cameraStandHeight;
        float camera = Mathf.MoveTowards(
            cameraRig.localPosition.y, targetCamera, heightChangeSpeed * Time.deltaTime);

        SetCameraRigHeight(camera);
    }

    private void ApplyHeight(float height)
    {
        controller.height = height;
        controller.center = new Vector3(0f, height * 0.5f, 0f);
    }

    private void SetCameraRigHeight(float height)
    {
        Vector3 local = cameraRig.localPosition;
        local.y = height;
        cameraRig.localPosition = local;
    }

    // Só levanta se houver espaço acima: senão o CharacterController atravessa
    // o teto e o Wendy fica com a cabeça dentro do duto.
    private bool CanStandUp()
    {
        float radius = controller.radius * 0.95f;
        Vector3 bottom = transform.position + Vector3.up * (radius + 0.05f);
        float distance = standingHeight - crouchingHeight;

        return !Physics.SphereCast(bottom, radius, Vector3.up, out _, distance,
                                   ~0, QueryTriggerInteraction.Ignore);
    }

    /// <summary>
    /// Entrega a altura da câmera ao roteiro — é o que o despertar usa para
    /// deitar o Wendy no campo (§2.3). Enquanto durar, agachar não mexe na
    /// câmera.
    /// </summary>
    public void OverrideCameraHeight(float height)
    {
        cameraHeightOverridden = true;
        overriddenCameraHeight = height;
    }

    /// <summary>Devolve a altura da câmera ao corpo.</summary>
    public void ClearCameraHeightOverride()
    {
        cameraHeightOverridden = false;
    }

    /// <summary>
    /// Teleporta o Wendy (spawn, checkpoint). Desliga o CharacterController no
    /// caminho porque ele ignora mudanças diretas de transform.
    /// </summary>
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        if (controller != null)
            controller.enabled = false;

        transform.SetPositionAndRotation(position, rotation);
        horizontalVelocity = Vector3.zero;
        verticalVelocity = 0f;

        if (controller != null)
            controller.enabled = true;
    }
}
