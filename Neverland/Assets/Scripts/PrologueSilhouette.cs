using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A silhueta no canto do quarto — o primeiro Peter Pan (GDD §2.3, §3.3, §6.2).
///
/// Ela existe numa janela muito estreita: **depois de a briga começar e antes de
/// Wendy fechar os olhos**. É o único momento do prólogo em que o quarto deixa
/// de ser só um quarto.
///
/// Regras que este script não quebra (§6.2, §3.3):
///   · Ela **não persegue, não anda, não ameaça** — não há IA nenhuma aqui.
///     Aparece, fica parada no canto onde você a posicionou, e some. Encenação
///     audiovisual pura; o medo vem do reconhecimento, não de uma mecânica.
///   · Ela **não interrompe nada** — a briga continua, o relógio do
///     PrologueEscape continua, o jogador continua livre para olhar ou não.
///     Dá para terminar o prólogo sem nunca ter virado a cabeça para o canto.
///
/// O truque de encenação é 'onlyAppearWhenUnseen': a silhueta nunca "pisca" na
/// tela. Ela espera o canto sair do enquadramento (ou o jogador piscar) e só
/// então passa a existir — quando o olhar volta, ela já estava lá. É a diferença
/// entre um susto e a sensação de ter perdido alguma coisa.
///
/// No breu do prólogo (a fresta da porta já se apagou) uma silhueta preta em
/// quarto preto não se lê. Ela precisa de um contorno: uma luz fraca de canto,
/// um relâmpago da chuva, ou um material levemente mais claro que a parede. Use
/// 'onAppear' para disparar esse revelador — o script não presume qual é.
///
/// Anexar ao GameObject da silhueta (a cápsula/mesh no canto do quarto).
/// </summary>
public class PrologueSilhouette : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("O diretor do prólogo — é dele que vem o relógio da briga.")]
    [SerializeField] private PrologueDirector director;
    [Tooltip("A câmera do jogador (Main Camera). Serve para saber se a silhueta " +
             "está no enquadramento — ela não aparece na cara de ninguém.")]
    [SerializeField] private Camera playerCamera;
    [Tooltip("Opcional. Com o BlinkMechanic ligado, um piscar do jogador também " +
             "conta como 'não estou vendo' — e é o melhor momento para ela chegar.")]
    [SerializeField] private BlinkMechanic blink;

    [Header("Quando ela chega")]
    [Tooltip("Segundos de briga antes de a silhueta poder aparecer. Contados a " +
             "partir das vozes, não do início da cena. Mantenha confortavelmente " +
             "abaixo do 'minListenSeconds' do PrologueEscape (padrão 15) — senão " +
             "o jogador fecha os olhos antes de ela existir.")]
    [SerializeField] private float appearAfterFightSeconds = 6f;

    [Header("Ela nunca aparece sendo vista")]
    [Tooltip("Marcado (recomendado): espera o canto sair do enquadramento — ou o " +
             "jogador piscar — antes de existir. Desmarcado: aparece na hora, " +
             "doa a quem doer.")]
    [SerializeField] private bool onlyAppearWhenUnseen = true;
    [Tooltip("Fração de olhos fechados que já conta como 'não estou vendo'. " +
             "Só usado se houver um BlinkMechanic ligado acima.")]
    [Range(0f, 1f)][SerializeField] private float blinkCountsAsUnseenAt = 0.6f;
    [Tooltip("Teto de espera, em segundos. Se o jogador encarar o canto sem piscar " +
             "esse tempo todo, ela aparece assim mesmo — no limite, ver a coisa " +
             "chegar também serve. 0 desliga o teto (ela espera o que for preciso).")]
    [SerializeField] private float maxWaitForUnseen = 8f;

    [Header("Encenação")]
    [Tooltip("Ao aparecer, gira no eixo Y para encarar o jogador. Ela não faz mais " +
             "nada além disso: uma vez virada, fica parada.")]
    [SerializeField] private bool facePlayerOnAppear = true;
    [Tooltip("Fade-in, em segundos. 0 = surge inteira (o padrão — combina com " +
             "'só aparece sem ser vista'). Qualquer valor acima de 0 exige que o " +
             "material dela seja Surface Type: Transparent, senão nada acontece.")]
    [SerializeField] private float fadeInDuration = 0f;
    [Tooltip("Fade-out ao sumir. 0 = some de uma vez. Como ela some atrás das " +
             "pálpebras fechando, 0 costuma bastar.")]
    [SerializeField] private float fadeOutDuration = 0f;

    [Header("Áudio")]
    [Tooltip("AudioSource do momento da chegada. Deixe FORA da hierarquia da " +
             "silhueta se ela puder sumir no meio do som.")]
    [SerializeField] private AudioSource appearAudio;
    [Tooltip("O som da chegada. Vazio: ela chega em silêncio — que é uma escolha " +
             "legítima, e talvez a melhor.")]
    [SerializeField] private AudioClip appearClip;

    [Header("Ganchos")]
    [Tooltip("Disparado no frame em que ela passa a existir. É aqui que se " +
             "pendura o revelador: o relâmpago, a luz de canto, a camada grave " +
             "no mixer (§12.3).")]
    [SerializeField] private UnityEvent onAppear;
    [Tooltip("Disparado quando ela some.")]
    [SerializeField] private UnityEvent onVanish;

    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private Coroutine fadeRoutine;
    private bool hasAppeared = false;   // ela só chega uma vez
    private float currentAlpha = 1f;    // último alpha aplicado (o bloco não se lê de volta)

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor"); // URP
    private static readonly int ColorId = Shader.PropertyToID("_Color");         // built-in

    /// <summary>True enquanto a silhueta está em cena.</summary>
    public bool IsVisible { get; private set; }

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        propertyBlock = new MaterialPropertyBlock();

        if (director == null)
            Debug.LogWarning("[PrologueSilhouette] Sem PrologueDirector: não há relógio da " +
                             "briga, então ela nunca aparece.", this);

        if (playerCamera == null)
            playerCamera = Camera.main;

        SetRenderersEnabled(false);
        IsVisible = false;
    }

    private void Update()
    {
        if (hasAppeared || director == null)
            return;

        float listened = director.SecondsSinceFightStarted;

        // Antes das vozes o prólogo ainda é só um quarto — ela não tem lugar ali.
        if (listened < appearAfterFightSeconds)
            return;

        if (!onlyAppearWhenUnseen || IsUnseen())
        {
            Appear();
            return;
        }

        // O jogador não desgruda o olho do canto: passado o teto, ela chega mesmo assim.
        float waiting = listened - appearAfterFightSeconds;
        if (maxWaitForUnseen > 0f && waiting >= maxWaitForUnseen)
            Appear();
    }

    /// <summary>
    /// Põe a silhueta em cena agora, ignorando relógio e enquadramento. Pública
    /// para quem quiser chamá-la de um Timeline ou de outro gatilho.
    /// </summary>
    public void Appear()
    {
        if (hasAppeared)
            return;

        hasAppeared = true;
        IsVisible = true;

        if (facePlayerOnAppear && playerCamera != null)
        {
            Vector3 dir = playerCamera.transform.position - transform.position;
            dir.y = 0f;   // ela encara, não olha para cima nem para baixo
            if (dir.sqrMagnitude > 0.0000001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        SetRenderersEnabled(true);

        if (fadeInDuration > 0f)
            StartFade(0f, 1f, fadeInDuration);
        else
            SetAlpha(1f);

        if (appearAudio != null && appearClip != null)
            appearAudio.PlayOneShot(appearClip);

        onAppear?.Invoke();
    }

    /// <summary>
    /// Tira a silhueta de cena. Ligar no 'onEscapeBegin' do PrologueEscape: no
    /// instante em que Wendy fecha os olhos para valer, o canto volta a ser um
    /// canto — e o jogo nunca confirma que havia alguém ali (§2.4).
    /// </summary>
    public void Vanish()
    {
        if (!IsVisible)
            return;

        IsVisible = false;
        // Impede que ela reapareça se o Update ainda estiver rodando.
        hasAppeared = true;

        if (fadeOutDuration > 0f)
        {
            StartFade(currentAlpha, 0f, fadeOutDuration, disableAtEnd: true);
        }
        else
        {
            SetAlpha(0f);
            SetRenderersEnabled(false);
        }

        onVanish?.Invoke();
    }

    // "Não estou vendo": ou as pálpebras estão fechadas o bastante, ou nenhum
    // pedaço dela cabe no frustum da câmera.
    private bool IsUnseen()
    {
        if (blink != null && blink.CloseAmount >= blinkCountsAsUnseenAt)
            return true;

        if (playerCamera == null || renderers == null || renderers.Length == 0)
            return false;

        Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(playerCamera);

        foreach (Renderer r in renderers)
        {
            if (r == null)
                continue;

            if (GeometryUtility.TestPlanesAABB(frustum, r.bounds))
                return false;
        }

        return true;
    }

    private void SetRenderersEnabled(bool enabled)
    {
        if (renderers == null)
            return;

        foreach (Renderer r in renderers)
            if (r != null)
                r.enabled = enabled;
    }

    private void StartFade(float from, float to, float duration, bool disableAtEnd = false)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(Fade(from, to, duration, disableAtEnd));
    }

    private IEnumerator Fade(float from, float to, float duration, bool disableAtEnd)
    {
        float elapsed = 0f;
        SetAlpha(from);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        SetAlpha(to);

        if (disableAtEnd)
            SetRenderersEnabled(false);

        fadeRoutine = null;
    }

    // Alpha por MaterialPropertyBlock:: não instancia material nem suja o asset.
    // Só tem efeito visível em material Transparent — em Opaque o alpha é ignorado
    // pelo shader, e é por isso que o fade vem desligado por padrão.
    private void SetAlpha(float alpha)
    {
        currentAlpha = alpha;

        if (renderers == null)
            return;

        foreach (Renderer r in renderers)
        {
            if (r == null)
                continue;

            r.GetPropertyBlock(propertyBlock);

            int id = r.sharedMaterial != null && r.sharedMaterial.HasProperty(BaseColorId)
                ? BaseColorId
                : ColorId;

            Color c = r.sharedMaterial != null && r.sharedMaterial.HasProperty(id)
                ? r.sharedMaterial.GetColor(id)
                : Color.white;

            c.a = alpha;
            propertyBlock.SetColor(id, c);
            r.SetPropertyBlock(propertyBlock);
        }
    }
}
