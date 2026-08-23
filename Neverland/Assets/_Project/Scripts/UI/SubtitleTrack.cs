using System.Collections;
using UnityEngine;

/// <summary>
/// Uma fala marcada num ponto do clipe. O tempo é medido DENTRO do áudio
/// (segundo 0 = começo do clipe), não a partir do início da cena.
/// </summary>
[System.Serializable]
public struct SubtitleCue
{
    [Tooltip("Segundo do CLIPE em que esta fala entra. As marcações têm que estar " +
             "em ordem crescente.")]
    public float time;
    [Tooltip("Quem fala. Vazio: legenda sem dono (ambiência, sussurro).")]
    public string speaker;
    [TextArea(2, 4)]
    [Tooltip("O texto. Quebras de linha manuais são respeitadas.")]
    public string text;
    [Tooltip("Segundos na tela. Deixe 0 para o automático: até a próxima marcação, " +
             "ou — na última — o tempo de leitura calculado pelo tamanho do texto. " +
             "Preencha quando a fala for solta no meio de um clipe longo, como um " +
             "pensamento por cima de uma ambiência de 1 minuto.")]
    public float duration;
}

/// <summary>
/// Legenda uma gravação longa — a oração da mãe (GDD §4.1), um monólogo, um
/// colecionável em VHS — dividindo-a em falas marcadas por tempo.
///
/// O ponto inteiro deste script é NÃO ter relógio próprio. Ele lê
/// 'AudioSource.time', o cabeçote de leitura do próprio áudio, e dispara cada
/// fala quando o áudio passa pela marcação. Um travamento de carregamento, um
/// frame longo, um Time.timeScale alterado — nada disso desloca a legenda,
/// porque ela não está contando nada: está OLHANDO onde a voz está.
///
/// Um contador próprio (`elapsed += Time.deltaTime`) pareceria funcionar no
/// editor e desandaria na build, um pouquinho a cada engasgo. É o erro que
/// custa uma tarde de "por que a legenda atrasa só às vezes?".
///
/// Não é preciso ninguém avisar quando começar: o track fica de olho no
/// AudioSource e se arma sozinho no instante em que ele começa a tocar. Por
/// isso o PrologueDirector não precisa saber que legendas existem — os dois
/// scripts obedecem ao mesmo relógio, sem conversar.
///
/// Só funciona com áudio tocado por 'Play()' (com clipe atribuído ao source).
/// 'PlayOneShot' não move o AudioSource.time — para esses, use
/// Subtitles.Instance.Show() direto, na hora do disparo.
///
/// Anexar ao mesmo GameObject do AudioSource que ele legenda.
/// </summary>
public class SubtitleTrack : MonoBehaviour
{
    [Header("A voz")]
    [Tooltip("O AudioSource que este track segue. Vazio: procura um no próprio objeto.")]
    [SerializeField] private AudioSource source;

    [Header("As falas")]
    [Tooltip("As marcações, em ordem crescente de tempo. Cada fala fica na tela " +
             "até a próxima começar — a última fica até o clipe acabar.")]
    [SerializeField] private SubtitleCue[] cues;

    [Header("Ajuste fino")]
    [Tooltip("Deslocamento global, em segundos, aplicado a TODAS as marcações. " +
             "Negativo faz a legenda entrar um pouco antes da voz, que é como " +
             "legenda de cinema funciona: ela espera você, você não espera ela.")]
    [SerializeField] private float offset = -0.15f;

    private Coroutine runner;

    private void Reset()
    {
        source = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (source == null)
            source = GetComponent<AudioSource>();

        if (source == null)
        {
            Debug.LogWarning("[SubtitleTrack] Sem AudioSource: não há relógio para " +
                             "seguir, então este track nunca dispara.", this);
            enabled = false;
            return;
        }

        if (cues == null || cues.Length == 0)
            Debug.LogWarning("[SubtitleTrack] Nenhuma marcação preenchida.", this);
    }

    private void OnEnable()
    {
        runner = StartCoroutine(WatchSource());
    }

    private void OnDisable()
    {
        if (runner != null)
        {
            StopCoroutine(runner);
            runner = null;
        }
    }

    // Fica de olho na voz e se arma sozinho quando ela começa — inclusive se o
    // clipe for tocado de novo mais tarde.
    private IEnumerator WatchSource()
    {
        while (true)
        {
            while (source == null || !source.isPlaying)
                yield return null;

            yield return DispatchCues();

            // Só rearma depois de a voz calar de vez, senão o track dispararia
            // as marcações outra vez no mesmo Play.
            while (source != null && source.isPlaying)
                yield return null;
        }
    }

    private IEnumerator DispatchCues()
    {
        if (cues == null)
            yield break;

        int next = 0;

        while (next < cues.Length)
        {
            // A voz parou antes do fim (corte de cena, fade do Director): o que
            // sobrou de legenda morre com ela.
            if (source == null || !source.isPlaying)
                yield break;

            if (source.time >= cues[next].time + offset)
            {
                SubtitleCue cue = cues[next];
                float duration = DurationOf(next);

                // Negativo não pode ir para a sobrecarga com duração: ela trava em
                // zero. O automático mora na sobrecarga sem o parâmetro.
                if (duration < 0f)
                    Subtitles.Instance?.Show(cue.speaker, cue.text);
                else
                    Subtitles.Instance?.Show(cue.speaker, cue.text, duration);

                next++;
                continue;   // marcações coladas disparam no mesmo frame
            }

            yield return null;
        }
    }

    // Duração explícita ganha de tudo. Sem ela, a fala vive até a próxima
    // marcação — e a ÚLTIMA vive o tempo de leitura do próprio texto, não o
    // resto do clipe: numa ambiência longa (a briga atrás da parede dura
    // minutos), esticar a última fala até o fim deixaria uma frase parada na
    // tela a cena inteira.
    private float DurationOf(int index)
    {
        if (cues[index].duration > 0f)
            return cues[index].duration;

        if (index + 1 < cues.Length)
            return Mathf.Max(0f, cues[index + 1].time - cues[index].time);

        return -1f;   // negativo = o Subtitles calcula pelo tamanho do texto
    }
}
