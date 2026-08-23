using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Fecha o prólogo e leva Wendy para o Dia (GDD §2.3, §8.1, §12.2).
///
/// A regra central é uma só: **a briga tem que ser escutada**. Fechar os olhos
/// é o ritual de fuga do Wendy (§2.1), mas ele não funciona no primeiro
/// segundo — o jogador fica ali, no escuro, ouvindo o que acontece atrás da
/// parede, por um tempo mínimo, antes de a fuga ser possível. É o que dá peso
/// ao prólogo: a impotência vem de não poder sair na hora em que se quer.
///
/// Cumprida a escuta, a janela de escolha é curta e tem uma só saída:
///   · Apertar a tecla fecha os olhos **para sempre** — não há como reabrir.
///     O ritual não é um piscar; é uma decisão.
///   · Não apertar não é uma opção: passado 'autoCloseSeconds' de briga, o
///     Wendy fecha os olhos sozinho. A criança não aguenta continuar ouvindo,
///     e o jogo não pergunta.
///
/// Sequência, quando a fuga finalmente dispara:
///   1. As pálpebras travam fechadas — o jogador não reabre mais os olhos.
///   2. O quarto real afunda no silêncio (o Director faz o fade de tudo).
///   3. O sussurro (§2.3) toca no escuro.
///   4. Corte: a cena do Dia carrega, e Wendy abre os olhos em outro lugar.
///
/// Os flashes de memória em VHS entram no gancho 'onEscapeBegin' quando esse
/// sistema existir — este script só precisa saber que o escuro dura o tempo
/// configurado.
///
/// Anexar ao mesmo GameObject do PrologueDirector (ou a um vazio da cena).
/// </summary>
public class PrologueEscape : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("O diretor do prólogo — é dele que vem o relógio da briga.")]
    [SerializeField] private PrologueDirector director;
    [Tooltip("O BlinkMechanic das pálpebras (normalmente no Canvas).")]
    [SerializeField] private BlinkMechanic blink;

    [Header("A escuta obrigatória")]
    [Tooltip("Segundos de briga que o jogador PRECISA escutar antes de fechar " +
             "os olhos poder tirá-lo do quarto. Contados a partir do instante " +
             "em que as vozes entram, não do início da cena.")]
    [SerializeField] private float minListenSeconds = 15f;
    [Tooltip("Desmarcado (padrão): as pálpebras respondem desde sempre — o ritual " +
             "existe, só não salva ainda. Marcado: antes do tempo mínimo a tecla " +
             "não move nada, e o jogador sente a mão do jogo segurando a dele.")]
    [SerializeField] private bool blockBlinkBeforeGate = false;

    [Header("O gatilho")]
    [Tooltip("Segundos de briga após os quais o Wendy fecha os olhos SOZINHO, " +
             "se o jogador não tiver apertado a tecla. Mesmo relógio do tempo " +
             "mínimo acima: 15 abre a janela de escolha, 25 a fecha. " +
             "0 desliga o fecho automático (a espera vira infinita).")]
    [SerializeField] private float autoCloseSeconds = 25f;

    [Header("A travessia")]
    [Tooltip("Nome da cena do Dia, exatamente como está em File > Build Settings.")]
    [SerializeField] private string daySceneName = "Island_Day";
    [Tooltip("Fade do áudio do quarto (chuva, briga, mãe) até o silêncio.")]
    [SerializeField] private float audioFadeOut = 2f;
    [Tooltip("Quanto tempo a tela fica no breu antes de a cena trocar. É o espaço " +
             "das memórias em VHS e do sussurro — não zere.")]
    [SerializeField] private float secondsInTheDark = 3f;

    [Header("O sussurro (§2.3)")]
    [Tooltip("AudioSource da voz que sussurra no escuro. Opcional.")]
    [SerializeField] private AudioSource whisperAudio;
    [Tooltip("O clipe do sussurro. Vazio: o beat é pulado.")]
    [SerializeField] private AudioClip whisperClip;
    [Tooltip("Espera, dentro do escuro, antes de o sussurro entrar.")]
    [SerializeField] private float delayBeforeWhisper = 1f;

    [Header("Gancho")]
    [Tooltip("Disparado no instante em que a fuga começa (olhos travados). " +
             "É aqui que os flashes de memória em VHS vão se pendurar.")]
    [SerializeField] private UnityEvent onEscapeBegin;

    private bool gateOpen = false;    // a escuta mínima já foi cumprida
    private bool escaping = false;    // a travessia já começou (só acontece uma vez)

    private void OnValidate()
    {
        // O fecho automático depois da janela de escolha, nunca antes dela.
        if (autoCloseSeconds > 0f && autoCloseSeconds < minListenSeconds)
            autoCloseSeconds = minListenSeconds;
    }

    private void Awake()
    {
        if (director == null)
            Debug.LogWarning("[PrologueEscape] Sem PrologueDirector: não há relógio da " +
                             "briga, então a fuga nunca arma.", this);

        if (blink == null)
            Debug.LogWarning("[PrologueEscape] Sem BlinkMechanic: não há como saber que " +
                             "o Wendy fechou os olhos.", this);

        // Avisa cedo, no play, em vez de falhar só no fim do prólogo.
        if (string.IsNullOrEmpty(daySceneName) ||
            !Application.CanStreamedLevelBeLoaded(daySceneName))
        {
            Debug.LogWarning(
                $"[PrologueEscape] A cena '{daySceneName}' não está em Build Settings. " +
                "O prólogo vai rodar até o escuro e parar ali.", this);
        }
    }

    private void Start()
    {
        // Antes da escuta cumprida, opcionalmente as pálpebras nem obedecem.
        if (blockBlinkBeforeGate && blink != null)
            blink.LockEyes(false);
    }

    private void Update()
    {
        if (escaping)
            return;

        // O relógio só corre depois de a briga começar; antes disso o prólogo
        // ainda está na oração ou no silêncio.
        float listened = director != null ? director.SecondsSinceFightStarted : -1f;

        if (!gateOpen)
        {
            if (listened < minListenSeconds)
                return;

            gateOpen = true;
            if (blockBlinkBeforeGate && blink != null)
                blink.UnlockEyes();
        }

        if (blink == null)
            return;

        // Escuta cumprida: a tecla deixa de ser um piscar e vira uma decisão.
        // GetKey, e não GetKeyDown, para pegar também quem já estava segurando
        // quando a janela abriu — ninguém precisa soltar e apertar de novo.
        if (Input.GetKey(blink.BlinkKey))
        {
            StartCoroutine(Escape());
            return;
        }

        // Ninguém apertou: o Wendy fecha sozinho. Ouvir também tem limite.
        if (autoCloseSeconds > 0f && listened >= autoCloseSeconds)
            StartCoroutine(Escape());
    }

    private IEnumerator Escape()
    {
        escaping = true;

        // Daqui em diante os olhos são do roteiro: fecham e não abrem mais.
        if (blink != null)
            blink.LockEyes(true);

        onEscapeBegin?.Invoke();

        if (director != null)
            director.FadeEverythingOut(audioFadeOut);

        // O breu só começa a contar quando as pálpebras terminam de encostar —
        // senão o tempo de escuro engoliria a própria animação de fechar.
        while (blink != null && !blink.EyesClosed)
            yield return null;

        yield return new WaitForSeconds(delayBeforeWhisper);

        // A voz que sussurra, no breu, logo antes de ele abrir os olhos (§2.3).
        if (whisperAudio != null && whisperClip != null)
            whisperAudio.PlayOneShot(whisperClip);

        float remaining = secondsInTheDark - delayBeforeWhisper;
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        if (string.IsNullOrEmpty(daySceneName) ||
            !Application.CanStreamedLevelBeLoaded(daySceneName))
        {
            // Sem cena para ir, o prólogo devolve os olhos em vez de travar o
            // jogo no escuro — é o comportamento útil em greybox. 'escaping'
            // continua ligado de propósito: sem isso o fecho automático
            // dispararia de novo no frame seguinte, em loop.
            Debug.LogError(
                $"[PrologueEscape] Não consegui carregar '{daySceneName}': adicione a cena " +
                "em File > Build Settings. Devolvendo o controle das pálpebras.", this);

            if (blink != null)
                blink.UnlockEyes();

            yield break;
        }

        SceneManager.LoadSceneAsync(daySceneName);
    }
}
