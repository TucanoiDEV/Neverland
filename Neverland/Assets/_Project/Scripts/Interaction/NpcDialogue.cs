using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Um NPC com quem o Wendy conversa: tocar E e a fala sobe na caixa de legenda
/// (GDD §9.3, §5.9).
///
/// É um IInteractable como qualquer outro — quem detecta, acende o retículo e
/// lê a tecla continua sendo o PlayerInteractor. Este script só decide O QUE é
/// dito e em que ordem, e entrega para o Subtitles.
///
/// As falas saem em SEQUÊNCIA, uma por interação: é assim que a Sininho
/// apresenta a ilha e que o Peter Pan entrega a lista de brinquedos (§2.3) sem
/// precisar de árvore de diálogo. Terminada a última, o NPC ou repete a
/// despedida ou cala de vez, conforme 'loopLastLine'.
///
/// NÃO existe versão lenta: segurar E é o gesto de fazer algo em silêncio
/// (§5.2), e não há como falar em silêncio. Por isso SupportsSlowInteract é
/// sempre false — o anel de progresso nem chega a aparecer sobre um NPC.
///
/// Anexar a um GameObject COM COLISOR, na layer Interactable.
/// </summary>
public class NpcDialogue : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public struct Line
    {
        [Tooltip("O que ele diz. Uma frase por vez — a caixa não rola.")]
        [TextArea(2, 4)] public string text;

        [Tooltip("Segundos na tela depois de terminar de escrever. 0 = deixa o " +
                 "Subtitles calcular pelo tamanho da fala.")]
        public float duration;
    }

    [Header("Identidade")]
    [Tooltip("Nome que aparece na legenda. Tem que bater com a tabela de cores " +
             "do Subtitles — 'Sininho', 'Peter Pan' — senão sai na cor padrão.")]
    [SerializeField] private string speaker = "Sininho";

    [Tooltip("Verbo que o retículo e o prompt mostram. Para gente, 'Falar'.")]
    [SerializeField] private string prompt = "Falar";

    [Header("O que ele diz")]
    [Tooltip("As falas, na ordem. Cada toque em E avança uma.")]
    [SerializeField] private Line[] lines;

    [Tooltip("Marcado: chegando ao fim, a última fala se repete para sempre — " +
             "o NPC nunca fica mudo. Desmarcado: ele para de responder e some " +
             "do retículo, e é o jogador quem entende que aquele assunto " +
             "acabou.")]
    [SerializeField] private bool loopLastLine = true;

    [Header("Não falar por cima")]
    [Tooltip("Marcado: enquanto uma legenda ainda estiver na tela, apertar E " +
             "não faz nada. Sem isto, o jogador martelando a tecla corta a " +
             "própria fala do NPC e nunca lê nada inteiro.")]
    [SerializeField] private bool ignoreWhileSpeaking = true;

    [Header("Ganchos")]
    [Tooltip("Disparado a cada fala, com o índice dela.")]
    [SerializeField] private UnityEvent onLineSpoken;

    [Tooltip("Disparado uma única vez, quando a ÚLTIMA fala é dita. É por aqui " +
             "que a Sininho manda o jogador seguir para a Roda das Crianças.")]
    [SerializeField] private UnityEvent onDialogueFinished;

    private int next;              // índice da próxima fala
    private bool finished;         // a última já foi dita

    public string Prompt => prompt;
    public Transform Transform => transform;

    // Falar nunca tem versão silenciosa.
    public bool SupportsSlowInteract => false;

    /// <summary>
    /// O NPC some do retículo quando não tem mais nada a dizer. Enquanto está
    /// falando ele continua "interagível" — recusar aqui apagaria o retículo no
    /// meio da conversa, e o jogador leria isso como "acabou".
    /// </summary>
    public bool CanInteract => lines != null && lines.Length > 0
                               && (loopLastLine || !finished);

    /// <summary>Índice da próxima fala — para um save saber onde a conversa parou.</summary>
    public int NextLineIndex => next;

    public void Interact(bool slow)
    {
        if (!CanInteract)
            return;

        // Deixa ele terminar de falar antes de aceitar o próximo toque.
        if (ignoreWhileSpeaking && Subtitles.Instance != null
            && Subtitles.Instance.IsShowing)
            return;

        int index = Mathf.Min(next, lines.Length - 1);
        Line line = lines[index];

        if (line.duration > 0f)
            Subtitles.Instance?.Show(speaker, line.text, line.duration);
        else
            Subtitles.Instance?.Show(speaker, line.text);

        onLineSpoken?.Invoke();

        // 'finished' dispara na última fala, não depois dela: quem repete a
        // despedida (loopLastLine) nunca sairia deste método de outro jeito.
        if (index >= lines.Length - 1)
        {
            if (!finished)
            {
                finished = true;
                onDialogueFinished?.Invoke();
            }
        }
        else
        {
            next = index + 1;
        }
    }

    /// <summary>
    /// Volta a conversa para o começo — o gancho de "a Sininho tem assunto novo
    /// depois que você achou o item".
    /// </summary>
    public void ResetDialogue()
    {
        next = 0;
        finished = false;
    }
}
