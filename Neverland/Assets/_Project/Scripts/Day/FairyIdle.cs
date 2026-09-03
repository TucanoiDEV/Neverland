using UnityEngine;

/// <summary>
/// A Sininho parada no ar (GDD §3.2, §2.3, §10.3, §10.4).
///
/// A forma-fada não anda, não persegue e não tem IA: ela FICA. Vinte e cinco
/// centímetros de luz âmbar pairando na frente do Wendy, perto demais, o tempo
/// todo virada para ele. Todo o "estar viva" dela cabe em quatro coisas que
/// este script faz e nada mais:
///   · o voo parado — sobe e desce, balança de lado e vai e vem, cada eixo com
///     um período diferente, para que o desenho nunca feche um ciclo visível;
///   · o encarar — ela gira em torno do próprio eixo para ficar de frente para
///     o menino, sempre, de onde quer que ele olhe (§3.2: "sempre por perto
///     demais");
///   · as asas — batem rápido, em torno do repouso que você deu a elas na cena;
///   · a luz — âmbar pulsante, mais o card de brilho que a acompanha. É o
///     ÚNICO bloom do jogo (§10.4), e ele morre quando ela vira monstro.
///
/// **Marionete a 12 fps (§10.3).** A pose inteira só é recalculada 12 vezes
/// por segundo, e entre um quadro e outro o script sai do Update na segunda
/// linha. Não é economia: é a direção de arte — os NPCs do jogo animam a 12
/// quadros interpolados, e só a Sininho MONSTRO anima fluida, o que é
/// exatamente o que a torna errada. Rodando a 60 fps, quatro de cada cinco
/// Updates aqui não fazem conta nenhuma.
///
/// **A chegada (§2.3).** O Campo liga este objeto no fim do despertar (o
/// gancho 'onAwakeningComplete' do AwakeningSequence). Ligar um objeto o faz
/// aparecer do nada no meio da tela — então, ao ser ligada, ela entra voando
/// de 'arrivalOffset' até o ponto onde você a deixou na cena, com a luz
/// subindo do zero. "Uma fada se aproxima." Zere 'arrivalSeconds' se quiser
/// que ela simplesmente já esteja lá.
///
/// Custo: nenhuma alocação por quadro (o MaterialPropertyBlock do brilho nasce
/// uma vez no Awake e é reescrito), nenhuma física, nenhum GetComponent fora
/// do Awake.
///
/// Anexar à raiz do NPC_Sininho — a MESMA raiz que carrega o colisor e o
/// NpcConversation, para que a conversa siga o corpo dela.
/// </summary>
[DisallowMultipleComponent]
public class FairyIdle : MonoBehaviour
{
    private const float Tau = Mathf.PI * 2f;

    [Header("Quem ela encara")]
    [Tooltip("O alvo do olhar dela — a câmera do Wendy. Vazio: procura a " +
             "câmera principal na cena.")]
    [SerializeField] private Transform target;

    [Header("Voo parado")]
    [Tooltip("Quanto ela sobe e desce a partir do ponto onde está na cena (m).")]
    [SerializeField] private float bobHeight = 0.055f;
    [Tooltip("Segundos de um sobe-desce completo.")]
    [SerializeField] private float bobSeconds = 2.7f;
    [Tooltip("Quanto ela balança para os lados (m).")]
    [SerializeField] private float swayWidth = 0.07f;
    [Tooltip("Segundos de um vai-e-vem lateral. Deixe PRIMO em relação ao " +
             "sobe-desce: períodos diferentes é o que impede o olho de " +
             "perceber o loop.")]
    [SerializeField] private float swaySeconds = 4.3f;
    [Tooltip("Quanto ela chega e afasta o rosto (m). Pequeno: ela não pode " +
             "sair do alcance da mão do Wendy (1,8 m no PlayerInteractor).")]
    [SerializeField] private float driftDepth = 0.035f;
    [Tooltip("Segundos de um chega-afasta.")]
    [SerializeField] private float driftSeconds = 5.9f;
    [Tooltip("Quanto ela se inclina para dentro da curva ao balançar (graus). " +
             "É o que faz o voo parecer voo, e não um objeto num trilho.")]
    [SerializeField] private float tiltDegrees = 7f;

    [Header("Encarar o Wendy")]
    [Tooltip("Desmarcado: ela fica na rotação em que você a deixou na cena.")]
    [SerializeField] private bool facePlayer = true;
    [Tooltip("Velocidade do giro (graus por segundo). Alto demais e ela vira " +
             "uma torre de vigia; baixo demais e ela perde o menino de vista.")]
    [SerializeField] private float turnSpeed = 160f;

    [Header("Asas")]
    [Tooltip("O pivô da asa esquerda. Vazio: ela não bate essa asa.")]
    [SerializeField] private Transform leftWing;
    [Tooltip("O pivô da asa direita.")]
    [SerializeField] private Transform rightWing;
    [Tooltip("Batidas por segundo. A 12 fps, valores acima de 6 viram " +
             "tremulação (o efeito da roda de carroça no cinema) — o que, " +
             "para asa de fada, funciona.")]
    [SerializeField] private float wingBeatsPerSecond = 4.5f;
    [Tooltip("Abertura da batida, para cada lado do repouso (graus).")]
    [SerializeField] private float wingDegrees = 26f;

    [Header("A luz âmbar (§10.4)")]
    [Tooltip("A luz filha — um CONE apontado para baixo, com o ápice logo " +
             "abaixo da saia. Tem que ser cone: uma point light dentro de um " +
             "corpo de 25 cm fica a 4 cm da própria pele dela e, com a queda " +
             "por 1/d², estoura a fada inteira em branco. Assim ela fica de " +
             "fora do próprio facho e o que acende é o capim embaixo dela. " +
             "Vazio: ela brilha só pelo material e pelo halo.")]
    [SerializeField] private Light glow;
    [Tooltip("Intensidade no pico do pulso.")]
    [SerializeField] private float glowIntensity = 2.4f;
    [Tooltip("Quanto a luz cai no vale do pulso (0 = luz fixa, 1 = apaga).")]
    [Range(0f, 1f)][SerializeField] private float glowPulse = 0.3f;
    [Tooltip("Segundos de um pulso completo. Perto do ritmo de uma respiração " +
             "calma — ela é bonita antes de ser assustadora.")]
    [SerializeField] private float glowPulseSeconds = 1.9f;

    [Header("O card de brilho")]
    [Tooltip("O quad aditivo que faz o halo. Vazio: não há halo.")]
    [SerializeField] private Renderer halo;
    [Tooltip("Tamanho do halo no pico do pulso (m).")]
    [SerializeField] private float haloSize = 0.44f;
    [Tooltip("Quanto o halo encolhe no vale do pulso (0 = tamanho fixo).")]
    [Range(0f, 1f)][SerializeField] private float haloShrink = 0.18f;

    [Header("A chegada (§2.3)")]
    [Tooltip("Segundos que ela leva para entrar voando até o lugar dela. " +
             "0: ela já começa parada no ponto.")]
    [SerializeField] private float arrivalSeconds = 2f;
    [Tooltip("De onde ela vem, em metros a partir do ponto dela, no espaço do " +
             "mundo. O padrão a traz de cima e de longe, pela direita.")]
    [SerializeField] private Vector3 arrivalOffset = new Vector3(1.6f, 2.2f, 3.4f);

    [Header("Marionete (§10.3)")]
    [Tooltip("Quadros por segundo da animação. 12 é o padrão dos NPCs do " +
             "jogo; 0 desliga a quantização e ela passa a animar fluida — o " +
             "que é o privilégio da forma MONSTRO, não desta.")]
    [SerializeField] private float animationFps = 12f;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private Vector3 anchor;              // o ponto da cena, em torno do qual ela paira
    private Quaternion rest;             // a rotação em que ela foi deixada na cena
    private Quaternion leftWingRest;
    private Quaternion rightWingRest;

    private MaterialPropertyBlock haloBlock;
    private Color haloColor = Color.white;

    private Vector3 facingOverride;      // para onde quem guia mandou ela olhar
    private bool hasFacingOverride;

    private float age;                   // segundos desde que ela foi ligada
    private int lastFrame = int.MinValue;

    /// <summary>
    /// O ponto em torno do qual ela paira, em coordenadas de mundo. Trocar
    /// isto é como se move a Sininho: quem a leva pela trilha (o FairyGuide)
    /// só empurra este ponto, e ela continua pairando — não existe navegação
    /// nem física por baixo, e nunca vai existir.
    /// </summary>
    public Vector3 Anchor
    {
        get => anchor;
        set => anchor = value;
    }

    /// <summary>
    /// Manda o rosto dela para uma direção do mundo, no lugar de encarar o
    /// Wendy — é o que a faz olhar para onde está indo enquanto guia. O eixo
    /// vertical é ignorado: ela nunca inclina a cabeça.
    /// </summary>
    public void FaceDirection(Vector3 worldDirection)
    {
        facingOverride = worldDirection;
        hasFacingOverride = true;
    }

    /// <summary>Desfaz o FaceDirection: ela volta a encarar o menino.</summary>
    public void FaceThePlayer()
    {
        hasFacingOverride = false;
    }

    private void Awake()
    {
        anchor = transform.position;
        rest = transform.rotation;

        if (leftWing != null)
            leftWingRest = leftWing.localRotation;

        if (rightWing != null)
            rightWingRest = rightWing.localRotation;

        if (halo != null)
        {
            haloBlock = new MaterialPropertyBlock();

            if (halo.sharedMaterial != null && halo.sharedMaterial.HasProperty(BaseColorId))
                haloColor = halo.sharedMaterial.GetColor(BaseColorId);
        }

        if (target == null)
        {
            PlayerLook look = FindObjectOfType<PlayerLook>();

            if (look != null && look.PlayerCamera != null)
                target = look.PlayerCamera.transform;
            else if (Camera.main != null)
                target = Camera.main.transform;
        }
    }

    // Ligar o objeto recomeça a chegada — é o que o AwakeningSequence dispara
    // no fim do despertar, e é o que faz ela ENTRAR em vez de aparecer.
    private void OnEnable()
    {
        age = 0f;
        lastFrame = int.MinValue;
        Pose(0f, 0f);
    }

    private void Update()
    {
        age += Time.deltaTime;

        if (animationFps <= 0f)
        {
            Pose(age, Time.deltaTime);
            return;
        }

        // O relógio de 12 quadros: dentro do mesmo quadro não há nada a fazer,
        // e é aqui que quatro de cada cinco Updates terminam.
        int frame = Mathf.FloorToInt(age * animationFps);

        if (frame == lastFrame)
            return;

        int elapsed = lastFrame == int.MinValue ? 1 : frame - lastFrame;
        lastFrame = frame;

        Pose(frame / animationFps, elapsed / animationFps);
    }

    /// <summary>
    /// Escreve a pose inteira dela no instante 't'. 'step' é quanto tempo se
    /// passou desde a pose anterior — é por ele que o giro avança, para que a
    /// velocidade de virar seja a mesma com ou sem quantização.
    /// </summary>
    private void Pose(float t, float step)
    {
        // As três ondas do voo parado. Períodos diferentes, nenhum múltiplo do
        // outro: o desenho só se repetiria depois de minutos.
        float sway = Mathf.Sin(Tau * t / Mathf.Max(0.01f, swaySeconds));
        float bob = Mathf.Sin(Tau * t / Mathf.Max(0.01f, bobSeconds));
        float drift = Mathf.Sin(Tau * t / Mathf.Max(0.01f, driftSeconds));

        // A chegada: 0 no instante em que ela é ligada, 1 quando já está no
        // lugar. O smoothstep tira a freada seca da aterrissagem.
        float arrival = arrivalSeconds > 0f ? Mathf.Clamp01(t / arrivalSeconds) : 1f;
        arrival = arrival * arrival * (3f - 2f * arrival);

        // 1. Para onde ela olha. O giro é do CORPO inteiro, só na horizontal:
        //    uma fada que se inclina para olhar o menino de cima já seria a
        //    outra personagem. Quem guia (o FairyGuide) toma a direção
        //    emprestada enquanto anda, e devolve o rosto ao menino ao parar.
        Vector3 toward = Vector3.zero;
        bool wantsToTurn = false;

        if (hasFacingOverride)
        {
            toward = facingOverride;
            wantsToTurn = true;
        }
        else if (facePlayer && target != null)
        {
            toward = target.position - anchor;
            wantsToTurn = true;
        }

        Quaternion facing = rest;

        if (wantsToTurn)
        {
            toward.y = 0f;

            // Direção degenerada (o menino exatamente embaixo dela): mantém o
            // giro que já havia, sem a inclinação do quadro anterior.
            facing = StripTilt(transform.rotation);

            if (toward.sqrMagnitude > 0.0001f)
            {
                Quaternion want = Quaternion.LookRotation(toward.normalized, Vector3.up);

                // No primeiro quadro ela já nasce virada; depois, ela vira.
                facing = lastFrame == int.MinValue || step <= 0f
                    ? want
                    : Quaternion.RotateTowards(facing, want, turnSpeed * step);
            }
        }

        // 2. Onde ela está. O balanço é nos eixos DELA (o lado dela, a frente
        //    dela), e não nos do mundo — assim o vai-e-vem continua sendo
        //    lateral depois que ela gira.
        Vector3 hover = anchor + facing * new Vector3(sway * swayWidth,
                                                      bob * bobHeight,
                                                      drift * driftDepth);

        transform.position = arrival >= 1f
            ? hover
            : Vector3.Lerp(anchor + arrivalOffset, hover, arrival);

        // 3. A inclinação para dentro da curva, por cima do giro.
        transform.rotation = facing * Quaternion.Euler(0f, 0f, -sway * tiltDegrees);

        // 4. As asas, em torno do repouso que elas têm na cena.
        float flap = Mathf.Sin(Tau * t * wingBeatsPerSecond) * wingDegrees;

        if (leftWing != null)
            leftWing.localRotation = leftWingRest * Quaternion.Euler(0f, flap, 0f);

        if (rightWing != null)
            rightWing.localRotation = rightWingRest * Quaternion.Euler(0f, -flap, 0f);

        // 5. A luz e o halo, no mesmo pulso — a luz é a personagem (§10.4).
        float pulse = 0.5f + 0.5f * Mathf.Sin(Tau * t / Mathf.Max(0.01f, glowPulseSeconds));

        if (glow != null)
            glow.intensity = glowIntensity * Mathf.Lerp(1f - glowPulse, 1f, pulse) * arrival;

        if (halo != null)
        {
            float size = haloSize * Mathf.Lerp(1f - haloShrink, 1f, pulse) * arrival;
            halo.transform.localScale = new Vector3(size, size, 1f);

            // O card é chapado: sem virar para a câmera, o halo desapareceria
            // de perfil. O material é sem culling, então o lado não importa.
            if (target != null)
                halo.transform.rotation =
                    Quaternion.LookRotation(halo.transform.position - target.position, Vector3.up);

            Color c = haloColor;
            c.a *= Mathf.Lerp(1f - haloShrink, 1f, pulse) * arrival;

            haloBlock.SetColor(BaseColorId, c);
            halo.SetPropertyBlock(haloBlock);
        }
    }

    /// <summary>
    /// Devolve só o giro horizontal de uma rotação — a inclinação do quadro
    /// anterior não pode entrar na conta do giro do quadro seguinte, ou ela
    /// se acumularia até a fada acabar de cabeça para baixo.
    /// </summary>
    private static Quaternion StripTilt(Quaternion rotation)
    {
        Vector3 forward = rotation * Vector3.forward;
        forward.y = 0f;

        return forward.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(forward.normalized, Vector3.up)
            : rotation;
    }

#if UNITY_EDITOR
    // O ponto onde ela vai parar, desenhado na cena: com o objeto desligado
    // (que é como o Campo começa) não há nada para ver de outro jeito.
    private void OnDrawGizmosSelected()
    {
        Vector3 point = Application.isPlaying ? anchor : transform.position;

        Gizmos.color = new Color(1f, 0.72f, 0.3f, 0.9f);
        Gizmos.DrawWireSphere(point, 0.12f);

        if (arrivalSeconds > 0f)
        {
            Gizmos.color = new Color(1f, 0.72f, 0.3f, 0.35f);
            Gizmos.DrawLine(point + arrivalOffset, point);
        }
    }
#endif
}
