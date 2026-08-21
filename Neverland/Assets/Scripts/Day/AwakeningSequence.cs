using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// O despertar no Campo do Despertar — a primeira coisa que acontece no Ato I
/// (GDD §2.3, §4.2).
///
/// O prólogo termina com o Wendy fechando os olhos para fugir. Esta cena
/// começa com eles ainda fechados — e é o JOGADOR quem os abre. A mesma tecla,
/// a mesma mão, o sentido invertido: no quarto real, Espaço era fuga; aqui,
/// Espaço é chegar. É a mesma inversão que o §5.6 promete para a noite,
/// apresentada de graça no primeiro minuto da ilha.
///
/// Ordem dos beats:
///   1. ESCURO — olhos travados fechados, corpo travado, câmera deitada no
///      capim, apontada para o céu impossível. Só o vento e a caixinha de
///      música (§11.1, estado CALMARIA) entrando de longe.
///   2. A ESPERA — por 'minDarkSeconds' a tecla não faz nada. O menino ainda
///      está atravessando.
///   3. ABRIR — o jogador aperta (ou, passado 'autoWakeSeconds', o jogo abre
///      por ele). As pálpebras sobem.
///   4. LEVANTAR — a câmera se ergue do chão até 1,20 m enquanto o olhar cai
///      do céu para o horizonte. É aqui que a ilha aparece inteira, e é lento
///      de propósito.
///   5. ENTREGA — controle devolvido e 'onAwakeningComplete' disparado. É
///      nesse gancho que a Sininho entra ("Você finalmente acordou!").
///
/// Anexar a um GameObject vazio da cena (ex.: "DayDirector").
/// </summary>
public class AwakeningSequence : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("As pálpebras — o mesmo BlinkMechanic do prólogo, no Canvas.")]
    [SerializeField] private BlinkMechanic blink;
    [Tooltip("O corpo do Wendy.")]
    [SerializeField] private PlayerMotor motor;
    [Tooltip("O olhar do Wendy.")]
    [SerializeField] private PlayerLook look;
    [Tooltip("A mão do Wendy — fica travada até ele estar de pé.")]
    [SerializeField] private PlayerInteractor interactor;

    [Header("Deitado no campo")]
    [Tooltip("Altura da câmera com o Wendy caído no capim (m).")]
    [SerializeField] private float lyingCameraHeight = 0.28f;
    [Tooltip("Para onde ele olha ao acordar. Negativo = para cima; -70 põe o " +
             "céu impossível ocupando a tela quase inteira (§2.3).")]
    [SerializeField] private float lyingPitch = -70f;

    [Header("Ritmo")]
    [Tooltip("Segundos no escuro antes de a tecla passar a funcionar. O menino " +
             "ainda está chegando — não zere.")]
    [SerializeField] private float minDarkSeconds = 2.5f;
    [Tooltip("Segundos após os quais o jogo abre os olhos sozinho, caso o " +
             "jogador não aperte nada. 0 desliga (a espera vira infinita).")]
    [SerializeField] private float autoWakeSeconds = 12f;
    [Tooltip("Quanto tempo o Wendy leva para se levantar (segundos). Lento de " +
             "propósito: é a primeira vez que o jogador vê a ilha.")]
    [SerializeField] private float riseDuration = 4f;
    [Tooltip("Curva do levantar. Começa devagar (o corpo custa) e assenta no " +
             "fim, sem freada brusca.")]
    [SerializeField] private AnimationCurve riseEase =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Pausa entre o Wendy estar de pé e o controle voltar. É o respiro " +
             "antes de a ilha ser 'jogável'.")]
    [SerializeField] private float delayBeforeControl = 0.5f;

    [Header("Áudio · a ilha entrando")]
    [Tooltip("A caixinha de música em dó maior, levemente desafinada — o " +
             "leitmotiv do jogo (§11.1). Loop, 2D.")]
    [SerializeField] private AudioSource musicCalmaria;
    [Tooltip("Volume final da música.")]
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.5f;
    [Tooltip("Ambiência do campo: vento no capim alto, insetos. Loop, 2D.")]
    [SerializeField] private AudioSource ambience;
    [Tooltip("Volume final da ambiência.")]
    [Range(0f, 1f)][SerializeField] private float ambienceVolume = 0.6f;
    [Tooltip("Fade de entrada dos dois (segundos). Longo: a ilha não 'liga', " +
             "ela vaza para dentro do escuro.")]
    [SerializeField] private float audioFadeIn = 4f;

    [Header("Ganchos")]
    [Tooltip("Disparado no instante em que os olhos começam a abrir.")]
    [SerializeField] private UnityEvent onEyesOpen;
    [Tooltip("Disparado quando o controle volta para o jogador. É aqui que a " +
             "Sininho entra em cena (§2.3).")]
    [SerializeField] private UnityEvent onAwakeningComplete;

    private bool woke;   // a abertura já disparou (só acontece uma vez)

    private void Awake()
    {
        // Tudo que trava tem que travar ANTES do primeiro frame: um único
        // frame com o campo dourado na tela estragaria a chegada.
        // As pálpebras são a exceção e ficam no Start — ver o comentário lá.
        if (motor != null)
        {
            motor.MoveLocked = true;
            motor.OverrideCameraHeight(lyingCameraHeight);
        }

        if (look != null)
        {
            look.LookLocked = true;
            look.SetPitch(lyingPitch);
        }

        if (interactor != null)
            interactor.InteractionLocked = true;
    }

    private void Start()
    {
        // As pálpebras são fechadas aqui, e não no Awake, por causa da ordem de
        // execução: o Awake do BlinkMechanic reescreve closeAmount a partir do
        // 'startClosed' dele, e se ele rodasse DEPOIS deste script a cena
        // abriria com um piscar rápido no primeiro frame. Todo Awake acontece
        // antes de qualquer Start, e nenhum frame é desenhado no meio — então
        // aqui a pose é definitiva. (Marcar 'startClosed' no BlinkMechanic da
        // cena continua sendo o certo; isto só garante que não dependa disso.)
        if (blink != null)
        {
            blink.SnapEyes(true);
            blink.LockEyes(true);
        }
        else
        {
            Debug.LogWarning("[AwakeningSequence] Sem BlinkMechanic: a cena " +
                             "vai abrir com os olhos já abertos.", this);
        }

        StartCoroutine(FadeIn(musicCalmaria, musicVolume));
        StartCoroutine(FadeIn(ambience, ambienceVolume));
        StartCoroutine(RunAwakening());
    }

    private IEnumerator RunAwakening()
    {
        // Beat 1 e 2 — o escuro e a espera. A tecla ainda não é dele.
        yield return new WaitForSeconds(minDarkSeconds);

        float waited = 0f;
        KeyCode key = blink != null ? blink.BlinkKey : KeyCode.Space;

        while (!woke)
        {
            // GetKey e não GetKeyDown: quem chegou do prólogo segurando a
            // tecla não precisa soltar e apertar de novo para acordar.
            if (Input.GetKey(key))
                break;

            waited += Time.deltaTime;
            if (autoWakeSeconds > 0f && waited >= autoWakeSeconds)
                break;

            yield return null;
        }

        woke = true;

        // Beat 3 — as pálpebras sobem. Ficam TRAVADAS ABERTAS durante o
        // levantar: se devolvêssemos o controle agora, o jogador que ainda
        // estivesse com a tecla apertada manteria os olhos fechados e perderia
        // exatamente a imagem que a cena inteira existe para mostrar.
        if (blink != null)
            blink.LockEyes(false);

        onEyesOpen?.Invoke();

        while (blink != null && blink.CloseAmount > 0.001f)
            yield return null;

        // Beat 4 — o corpo se levanta e o olhar desce do céu para a ilha.
        yield return Rise();

        yield return new WaitForSeconds(delayBeforeControl);

        // Beat 5 — a ilha é dele.
        if (blink != null)
            blink.UnlockEyes();

        if (motor != null)
        {
            motor.ClearCameraHeightOverride();
            motor.MoveLocked = false;
        }

        if (look != null)
            look.LookLocked = false;

        if (interactor != null)
            interactor.InteractionLocked = false;

        onAwakeningComplete?.Invoke();
    }

    private IEnumerator Rise()
    {
        float elapsed = 0f;

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float k = riseEase.Evaluate(Mathf.Clamp01(elapsed / riseDuration));

            if (motor != null)
                motor.OverrideCameraHeight(
                    Mathf.Lerp(lyingCameraHeight, StandingCameraHeight(), k));

            if (look != null)
                look.SetPitch(Mathf.Lerp(lyingPitch, 0f, k));

            yield return null;
        }

        if (motor != null)
            motor.OverrideCameraHeight(StandingCameraHeight());

        if (look != null)
            look.SetPitch(0f);
    }

    // A altura de pé é do PlayerMotor (§5.1 crava 1,20 m); ler de lá evita
    // duas fontes de verdade sobre a estatura do menino.
    private float StandingCameraHeight()
    {
        return motor != null ? motor.CameraStandHeight : 1.2f;
    }

    private IEnumerator FadeIn(AudioSource source, float target)
    {
        if (source == null)
            yield break;

        source.loop = true;
        source.volume = 0f;

        if (!source.isPlaying)
            source.Play();

        float elapsed = 0f;
        while (elapsed < audioFadeIn)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, target, elapsed / Mathf.Max(0.01f, audioFadeIn));
            yield return null;
        }

        source.volume = target;
    }
}
