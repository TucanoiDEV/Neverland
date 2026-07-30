using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Legendas no padrão Puppet Combo (GDD §9.1, §9.4, §10).
///
/// A caixa é sempre a mesma: um retângulo escuro semitransparente, o nome de
/// quem fala numa cor própria em cima, e a fala sendo DATILOGRAFADA embaixo,
/// caractere a caractere, com um blip a cada poucas letras. Nada de animação
/// de entrada elaborada — a caixa aparece, escreve e some. É uma legenda de
/// fita VHS, não uma UI de RPG.
///
/// Regras de ouro do sistema:
///   · A caixa NUNCA muda de tamanho enquanto escreve. O texto é medido inteiro
///     de uma vez e só é REVELADO aos poucos (maxVisibleCharacters). Se fosse
///     por substring, a caixa cresceria letra a letra — o erro clássico.
///   · As linhas entram numa FILA. Duas falas disparadas juntas não se
///     atropelam: a segunda espera a primeira acabar.
///   · Entre falas encadeadas a caixa NÃO pisca — ela só some quando a fila
///     esvazia.
///   · Tudo roda em tempo NÃO escalado (unscaledDeltaTime). O áudio ignora
///     Time.timeScale; a legenda também tem que ignorar, senão uma câmera lenta
///     dessincroniza a fala da voz.
///   · Legenda é opcional (§9.1): desligada, o sistema inteiro vira no-op.
///
/// Como usar, de qualquer script:
///     Subtitles.Instance?.Show("Mãe", "Pai nosso que estais no céu...");
///     Subtitles.Instance?.ShowWithAudio(motherVoice, "Mãe", "Santificado seja...");
///     Subtitles.Instance?.Hint("[sino à esquerda]");
///
/// Anexar ao GameObject do painel da legenda, dentro do Canvas.
/// </summary>
public class Subtitles : MonoBehaviour
{
    /// <summary>Acesso rápido de qualquer script da cena. Pode ser nulo — sempre use `?.`.</summary>
    public static Subtitles Instance { get; private set; }

    [Header("Referências de UI")]
    [Tooltip("CanvasGroup do painel escuro (a caixa). É por ele que a legenda aparece e some.")]
    [SerializeField] private CanvasGroup panel;
    [Tooltip("Texto do nome de quem fala. Fica escondido nas falas sem dono (ambiência, sussurro).")]
    [SerializeField] private TMP_Text speakerLabel;
    [Tooltip("Texto da fala — é ele que é datilografado.")]
    [SerializeField] private TMP_Text lineLabel;
    [Tooltip("Opcional: a setinha '▼' que pisca quando a fala terminou de escrever e " +
             "está só esperando. Aparece só em falas que o jogador pode pular.")]
    [SerializeField] private GameObject continueIndicator;

    [Header("Ligar/desligar (§9.1 — legendas são opcionais)")]
    [Tooltip("Desmarcado: nenhuma legenda aparece e as chamadas viram no-op. " +
             "É este campo que a opção de acessibilidade do menu vai controlar.")]
    [SerializeField] private bool subtitlesEnabled = true;

    [Header("Datilografia")]
    [Tooltip("Caracteres por segundo. Puppet Combo escreve rápido: 35–60 é a faixa boa. " +
             "Valores baixos demais fazem o jogador ler mais devagar que a voz.")]
    [SerializeField] private float charactersPerSecond = 45f;

    [Header("Quanto tempo a fala fica na tela")]
    [Tooltip("Segundos parados depois de a fala terminar de escrever (base fixa).")]
    [SerializeField] private float holdSeconds = 1.2f;
    [Tooltip("Segundos EXTRA por caractere — falas longas ficam mais tempo. " +
             "Ignorado quando a fala segue um AudioSource ou recebe duração explícita.")]
    [SerializeField] private float holdSecondsPerCharacter = 0.035f;

    [Header("Aparecer e sumir")]
    [Tooltip("Fade da caixa ao aparecer. 0 = pop seco (mais fiel ao Puppet Combo).")]
    [SerializeField] private float fadeInDuration = 0.12f;
    [Tooltip("Fade da caixa ao sumir, quando a fila esvazia.")]
    [SerializeField] private float fadeOutDuration = 0.25f;

    [Header("Quem fala")]
    [Tooltip("Sufixo colado no nome. Deixe ':' para o visual clássico ('Wendy:').")]
    [SerializeField] private string speakerSuffix = ":";
    [Tooltip("Cor de quem não estiver na tabela abaixo.")]
    [SerializeField] private Color defaultSpeakerColor = new Color(0.45f, 0.65f, 1f);
    [Tooltip("Marcado: a fala é pintada da mesma cor do nome (é assim na referência " +
             "que originou esta caixa). Desmarcado: a fala fica na cor autorada no TMP.")]
    [SerializeField] private bool tintLineWithSpeakerColor = true;
    [Tooltip("Uma cor por personagem — Wendy, Mãe, Sininho, Peter Pan. " +
             "O nome tem que bater com o passado no Show() (maiúsculas não importam).")]
    [SerializeField] private SpeakerStyle[] speakerStyles;

    [Header("Indicador direcional (§9.1, §9.4)")]
    [Tooltip("Cor das pistas de som direcionais — o '[sino à esquerda]'. " +
             "Mais apagada que uma fala: é informação de acessibilidade, não diálogo.")]
    [SerializeField] private Color hintColor = new Color(0.75f, 0.75f, 0.75f);
    [Tooltip("Segundos que uma pista de som fica na tela. Ela não é datilografada: " +
             "o som está acontecendo AGORA, a pista tem que ser instantânea.")]
    [SerializeField] private float hintDuration = 2f;

    [Header("Blip da máquina de escrever")]
    [Tooltip("AudioSource do blip. Deixe 2D (Spatial Blend 0) e num grupo de mixer de UI.")]
    [SerializeField] private AudioSource typeAudio;
    [Tooltip("O clipe do blip — curtíssimo. Vazio: a legenda escreve muda.")]
    [SerializeField] private AudioClip typeClip;
    [Tooltip("Toca um blip a cada N caracteres. 1 = a cada letra (barulhento demais).")]
    [Min(1)][SerializeField] private int blipEveryCharacters = 2;
    [Tooltip("Variação aleatória de pitch do blip — sem ela vira metralhadora.")]
    [SerializeField] private Vector2 blipPitchRange = new Vector2(0.94f, 1.06f);

    [Header("Pular")]
    [Tooltip("Marcado: a tecla abaixo termina a datilografia na hora e, apertada de " +
             "novo, dispensa a fala. Falas presas a um AudioSource NUNCA são " +
             "dispensadas assim — só têm a escrita acelerada — senão a voz " +
             "continuaria tocando sem legenda.")]
    [SerializeField] private bool allowSkip = true;
    [Tooltip("Tecla de pular/avançar.")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    // Uma fala na fila. 'duration' negativa = calcular pelo tamanho do texto.
    private struct Line
    {
        public string speaker;
        public string text;
        public float duration;
        public AudioSource follow;  // enquanto este source tocar, a legenda fica
        public bool instant;        // sem datilografia (pistas de som)
        public Color? colorOverride;
    }

    private readonly Queue<Line> queue = new Queue<Line>();
    private Coroutine runner;
    private bool skipRequested;
    private int lastBlipStep;
    private Color authoredLineColor = Color.white;  // a cor posta no TMP no Inspector

    /// <summary>True enquanto houver alguma fala escrevendo ou na tela.</summary>
    public bool IsShowing => runner != null;

    /// <summary>Legendas ligadas? Desligar limpa o que estiver na tela (§9.1).</summary>
    public bool Enabled
    {
        get => subtitlesEnabled;
        set
        {
            subtitlesEnabled = value;
            if (!value)
                Clear();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[Subtitles] Já existe um Subtitles nesta cena. " +
                             "Removendo o componente duplicado deste objeto.", this);
            Destroy(this);
            return;
        }

        Instance = this;

        if (panel == null)
            Debug.LogWarning("[Subtitles] Sem CanvasGroup no painel: a caixa não " +
                             "consegue aparecer nem sumir.", this);

        if (lineLabel == null)
            Debug.LogWarning("[Subtitles] Sem TMP_Text da fala: as legendas não " +
                             "vão a lugar nenhum.", this);
        else
            authoredLineColor = lineLabel.color;

        if (panel != null)
        {
            // A legenda é enfeite: nunca pode comer clique de UI.
            panel.blocksRaycasts = false;
            panel.interactable = false;
            panel.alpha = 0f;
        }

        if (continueIndicator != null)
            continueIndicator.SetActive(false);

        ClearLabels();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnValidate()
    {
        charactersPerSecond = Mathf.Max(1f, charactersPerSecond);
        holdSeconds = Mathf.Max(0f, holdSeconds);
        hintDuration = Mathf.Max(0f, hintDuration);
    }

    private void Update()
    {
        // A tecla é lida aqui, e não dentro da coroutine, para que um toque valha
        // no mesmo frame em que aconteceu, seja qual for o estágio da fala.
        if (allowSkip && runner != null && Input.GetKeyDown(skipKey))
            skipRequested = true;
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Enfileira uma fala. A duração é calculada pelo tamanho do texto.
    /// 'speaker' vazio = fala sem dono (ambiência, sussurro) e o nome nem aparece.
    /// </summary>
    public void Show(string speaker, string text)
    {
        Enqueue(new Line { speaker = speaker, text = text, duration = -1f });
    }

    /// <summary>
    /// Enfileira uma fala que ocupa exatamente 'duration' segundos de tela — TEMPO
    /// TOTAL, datilografia inclusa. É isso que permite encadear falas marcadas
    /// contra um clipe de áudio sem que elas atrasem umas às outras: o tempo de
    /// escrita sai de dentro da duração, não se soma a ela.
    /// </summary>
    public void Show(string speaker, string text, float duration)
    {
        Enqueue(new Line { speaker = speaker, text = text, duration = Mathf.Max(0f, duration) });
    }

    /// <summary>
    /// Enfileira uma fala amarrada a uma voz: ela fica na tela enquanto o
    /// AudioSource estiver tocando. Chame no MESMO instante em que der Play —
    /// se a voz já tiver acabado quando a fala chegar na vez dela, o sistema cai
    /// no tempo calculado pelo texto.
    /// </summary>
    public void ShowWithAudio(AudioSource source, string speaker, string text)
    {
        Enqueue(new Line { speaker = speaker, text = text, duration = -1f, follow = source });
    }

    /// <summary>
    /// Pista de som direcional para acessibilidade (§9.1, §9.4) — ex.: "[sino à esquerda]".
    /// Aparece inteira de uma vez, sem datilografia e sem nome: o som está
    /// acontecendo agora, e uma pista que se escreve devagar chega tarde demais.
    /// </summary>
    public void Hint(string text)
    {
        Enqueue(new Line
        {
            speaker = null,
            text = text,
            duration = hintDuration,
            instant = true,
            colorOverride = hintColor
        });
    }

    /// <summary>Apaga a fala atual e esvazia a fila na hora, sem fade. Use em corte de cena.</summary>
    public void Clear()
    {
        queue.Clear();

        if (runner != null)
        {
            StopCoroutine(runner);
            runner = null;
        }

        if (panel != null)
            panel.alpha = 0f;

        if (continueIndicator != null)
            continueIndicator.SetActive(false);

        ClearLabels();
    }

    // ─── Motor ────────────────────────────────────────────────────────────────

    private void Enqueue(Line line)
    {
        if (!subtitlesEnabled || lineLabel == null || string.IsNullOrEmpty(line.text))
            return;

        queue.Enqueue(line);

        if (runner == null)
            runner = StartCoroutine(RunQueue());
    }

    private IEnumerator RunQueue()
    {
        // A caixa sobe uma vez só, no começo da fila, e desce quando ela acaba:
        // falas encadeadas não podem fazer o painel piscar entre uma e outra.
        yield return FadePanel(1f, fadeInDuration);

        while (queue.Count > 0)
            yield return PlayLine(queue.Dequeue());

        yield return FadePanel(0f, fadeOutDuration);

        ClearLabels();
        runner = null;
    }

    private IEnumerator PlayLine(Line line)
    {
        skipRequested = false;
        lastBlipStep = 0;

        // Marcado ANTES da datilografia: uma duração explícita conta a partir daqui.
        float startedAt = Time.unscaledTime;

        Color color = ResolveColor(line);
        ApplySpeaker(line.speaker, color);

        lineLabel.text = line.text;

        // Sempre reatribuída a partir da cor autorada, nunca da cor que ficou da
        // linha anterior — senão uma pista cinza pintaria toda a conversa seguinte.
        lineLabel.color = line.colorOverride
            ?? (tintLineWithSpeakerColor ? color : authoredLineColor);

        // Mede o texto INTEIRO antes de revelar nada: é isso que trava o tamanho
        // da caixa e dá o contador real de caracteres (tags de rich text não contam).
        lineLabel.maxVisibleCharacters = 0;
        lineLabel.ForceMeshUpdate();
        int total = lineLabel.textInfo.characterCount;

        if (line.instant)
        {
            lineLabel.maxVisibleCharacters = total;
        }
        else
        {
            float revealed = 0f;
            while (revealed < total && !skipRequested)
            {
                revealed += charactersPerSecond * Time.unscaledDeltaTime;
                int visible = Mathf.Min(total, Mathf.FloorToInt(revealed));

                if (visible != lineLabel.maxVisibleCharacters)
                {
                    lineLabel.maxVisibleCharacters = visible;
                    PlayBlip(visible);
                }

                yield return null;
            }

            // Seja por chegar ao fim, seja por pulo: a fala termina completa.
            lineLabel.maxVisibleCharacters = total;
            skipRequested = false;
        }

        yield return HoldLine(line, total, startedAt);

        if (continueIndicator != null)
            continueIndicator.SetActive(false);
    }

    // A fala já está escrita — agora é só o tempo de leitura.
    private IEnumerator HoldLine(Line line, int characterCount, float startedAt)
    {
        // Presa a uma voz: quem manda no tempo é o áudio, não o relógio.
        if (line.follow != null && line.follow.isPlaying)
        {
            while (line.follow != null && line.follow.isPlaying)
                yield return null;

            yield break;
        }

        // Duração explícita conta o tempo TOTAL de tela: desconta o que a
        // datilografia já consumiu. Sem isso, falas encadeadas contra um clipe
        // de áudio atrasariam um pouco a cada linha até perder a voz de vista.
        float hold = line.duration >= 0f
            ? line.duration - (Time.unscaledTime - startedAt)
            : holdSeconds + characterCount * holdSecondsPerCharacter;

        // A setinha só pisca onde o jogador realmente pode avançar.
        bool dismissable = allowSkip && !line.instant;
        if (continueIndicator != null)
            continueIndicator.SetActive(dismissable);

        float elapsed = 0f;
        while (elapsed < hold)
        {
            if (dismissable && skipRequested)
            {
                skipRequested = false;
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void PlayBlip(int visibleCount)
    {
        if (typeAudio == null || typeClip == null || visibleCount <= 0)
            return;

        // Um blip a cada N caracteres, e nunca em espaço em branco: o silêncio
        // entre palavras é o que faz o som parecer digitação e não ruído.
        int step = visibleCount / blipEveryCharacters;
        if (step == lastBlipStep)
            return;

        lastBlipStep = step;

        TMP_TextInfo info = lineLabel.textInfo;
        if (visibleCount - 1 < info.characterCount &&
            char.IsWhiteSpace(info.characterInfo[visibleCount - 1].character))
            return;

        typeAudio.pitch = Random.Range(blipPitchRange.x, blipPitchRange.y);
        typeAudio.PlayOneShot(typeClip);
    }

    private Color ResolveColor(Line line)
    {
        if (line.colorOverride.HasValue)
            return line.colorOverride.Value;

        if (!string.IsNullOrEmpty(line.speaker) && speakerStyles != null)
        {
            foreach (SpeakerStyle style in speakerStyles)
            {
                if (!string.IsNullOrEmpty(style.speaker) &&
                    string.Equals(style.speaker, line.speaker, System.StringComparison.OrdinalIgnoreCase))
                    return style.color;
            }
        }

        return defaultSpeakerColor;
    }

    private void ApplySpeaker(string speaker, Color color)
    {
        if (speakerLabel == null)
            return;

        bool hasSpeaker = !string.IsNullOrEmpty(speaker);

        // Esconder o objeto (e não só limpar o texto) faz o layout fechar o
        // buraco: fala sem dono não deixa linha vazia em cima.
        speakerLabel.gameObject.SetActive(hasSpeaker);

        if (!hasSpeaker)
            return;

        speakerLabel.text = speaker + speakerSuffix;
        speakerLabel.color = color;
    }

    private void ClearLabels()
    {
        if (lineLabel != null)
        {
            lineLabel.text = string.Empty;
            lineLabel.maxVisibleCharacters = 0;
        }

        if (speakerLabel != null)
        {
            speakerLabel.text = string.Empty;
            speakerLabel.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadePanel(float target, float duration)
    {
        if (panel == null)
            yield break;

        float from = panel.alpha;

        if (duration <= 0f)
        {
            panel.alpha = target;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            panel.alpha = Mathf.Lerp(from, target, elapsed / duration);
            yield return null;
        }

        panel.alpha = target;
    }

    /// <summary>Uma cor por personagem — é o que dá a "Katz:" azul da referência.</summary>
    [System.Serializable]
    private struct SpeakerStyle
    {
        [Tooltip("Nome exatamente como é passado no Show() (maiúsculas não importam).")]
        public string speaker;
        public Color color;
    }
}
