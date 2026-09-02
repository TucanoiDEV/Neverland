using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Uma conversa com escolha de fala (GDD §5.9, Apêndice C).
///
/// É o irmão mais velho do NpcDialogue: aquele serve para quem só tem falas
/// em fila (um NPC de ambiente); este serve para os momentos em que o jogador
/// ESCOLHE o que o Wendy responde — a chegada da Sininho, a apresentação do
/// Peter Pan, a lista de brinquedos, a ordem de dormir.
///
/// A conversa é um grafo pequeno, autorado no Inspector: uma lista de NÓS, e
/// cada nó tem as falas do NPC e até três escolhas. Cada escolha diz qual é o
/// próximo nó (-1 encerra), se soma +1 de [[Lucidez]] e a partir de que
/// limiar ela sequer aparece. Nada de árvore gigante nem de ScriptableObject:
/// o §5.9 pede "máquina de nós simples", e o Dia inteiro cabe em quatro
/// conversas de 2 a 4 nós.
///
/// Custo: ZERO fora da conversa. Não existe Update aqui — só uma coroutine
/// que nasce quando o jogador aperta E e morre no fim da fala. Os buffers de
/// escolha são alocados uma vez, no Awake, e reaproveitados: uma conversa
/// inteira não gera lixo para o GC.
///
/// Durante a conversa o Wendy PARA de andar mas continua olhando: tirar a
/// câmera dele seria uma cutscene, e o §9 é claro que o jogo não tira a
/// câmera da mão do jogador sem necessidade. A mão (PlayerInteractor) é
/// travada para que a mesma tecla E não abra uma segunda conversa por cima.
///
/// Anexar a um GameObject COM COLISOR, na layer Interactable.
/// </summary>
public class NpcConversation : MonoBehaviour, IInteractable
{
    /// <summary>Uma fala do jogador, e para onde ela leva.</summary>
    [System.Serializable]
    public class Choice
    {
        [Tooltip("O que aparece na caixa. 1–2 linhas, lidas em voz alta em " +
                 "menos de 4 s, na boca de uma criança de 9 anos (§5.9).")]
        [TextArea(1, 3)] public string text;

        [Tooltip("O que o Wendy DIZ ao escolher, se for diferente do que está " +
                 "escrito na caixa. Vazio: ele diz exatamente a opção.")]
        [TextArea(1, 3)] public string spoken;

        [Tooltip("Marcado: é uma fala de LUCIDEZ [L] e soma +1. Desmarcado: " +
                 "fala de ENTREGA [E], que não tira nada — a Lucidez nunca cai.")]
        public bool raisesLucidez;

        [Tooltip("Lucidez mínima para esta opção sequer aparecer. 0 = sempre. " +
                 "É o ramo de limiar do §5.9 (ex.: 2 para 'sinto saudade da " +
                 "minha mãe').")]
        public int minLucidez;

        [Tooltip("Índice do nó que vem depois. -1 encerra a conversa.")]
        public int nextNode = -1;

        [Tooltip("Disparado quando esta escolha é feita.")]
        public UnityEvent onChosen;
    }

    /// <summary>Um trecho de conversa: o que o NPC diz e o que se pode responder.</summary>
    [System.Serializable]
    public class Node
    {
        [Tooltip("Só para você se achar na lista — não aparece no jogo.")]
        public string nickname;

        [Tooltip("As falas do NPC neste nó, na ordem. Elas entram na fila da " +
                 "legenda de uma vez: a caixa não pisca entre uma e outra.")]
        [TextArea(1, 4)] public string[] lines;

        [Tooltip("As respostas do Wendy. Vazio: o nó só fala e segue para o " +
                 "'nextNode'.")]
        public Choice[] choices;

        [Tooltip("Para onde ir quando não há escolha nenhuma disponível. " +
                 "-1 encerra a conversa.")]
        public int nextNode = -1;

        [Tooltip("Disparado ao entrar neste nó, antes da primeira fala.")]
        public UnityEvent onEnter;
    }

    [Header("Identidade")]
    [Tooltip("Nome na legenda. Tem que bater com a tabela de cores do " +
             "Subtitles — 'Sininho', 'Peter Pan'.")]
    [SerializeField] private string speaker = "Sininho";

    [Tooltip("Verbo do retículo e do prompt. Para gente, 'Falar'.")]
    [SerializeField] private string prompt = "Falar";

    [Tooltip("Nome que aparece quando é o Wendy que fala.")]
    [SerializeField] private string playerSpeaker = "Wendy";

    [Header("A conversa")]
    [Tooltip("Os nós. O índice de cada um é a posição nesta lista — é ele que " +
             "'nextNode' aponta.")]
    [SerializeField] private Node[] nodes;

    [Tooltip("Por qual nó a conversa começa.")]
    [SerializeField] private int startNode;

    [Tooltip("Nó em que a conversa recomeça depois de terminada — o 'ele ainda " +
             "responde alguma coisa' quando o jogador volta a falar. -1: o NPC " +
             "sai do retículo e fica mudo.")]
    [SerializeField] private int repeatNode = -1;

    [Header("O corpo do Wendy durante a conversa")]
    [Tooltip("O PlayerMotor. Vazio: procura na cena no Awake.")]
    [SerializeField] private PlayerMotor motor;

    [Tooltip("O PlayerInteractor. Vazio: procura na cena no Awake.")]
    [SerializeField] private PlayerInteractor interactor;

    [Tooltip("Marcado: o Wendy fica parado enquanto conversa. Desmarcado: ele " +
             "pode se afastar no meio da fala (útil para conversas de " +
             "ambiente, nunca para as do Apêndice C).")]
    [SerializeField] private bool holdPlayerStill = true;

    [Header("Sem legenda")]
    [Tooltip("Segundos por fala quando as legendas estão desligadas (§9.1) ou " +
             "não há Subtitles na cena. Sem isto a conversa passaria inteira " +
             "em um frame.")]
    [SerializeField] private float fallbackLineSeconds = 2.5f;

    [Header("Ganchos")]
    [Tooltip("Disparado quando a conversa começa.")]
    [SerializeField] private UnityEvent onConversationStarted;

    [Tooltip("Disparado quando a conversa chega ao fim — é por aqui que o " +
             "Dia avança (a Sininho manda seguir para a Roda, o Peter Pan " +
             "abre o passeio).")]
    [SerializeField] private UnityEvent onConversationFinished;

    // Buffers reaproveitados: 'texts' vai para a caixa, 'map' lembra qual
    // escolha do nó corresponde a cada linha mostrada (as escondidas por
    // limiar de Lucidez abrem buracos na numeração).
    private string[] choiceTexts;
    private int[] choiceMap;

    private bool running;
    private bool finished;

    public string Prompt => prompt;
    public Transform Transform => transform;

    /// <summary>Conversa nunca tem versão silenciosa (§5.2).</summary>
    public bool SupportsSlowInteract => false;

    public bool CanInteract =>
        !running && nodes != null && nodes.Length > 0 && (!finished || repeatNode >= 0);

    /// <summary>True enquanto esta conversa está no ar.</summary>
    public bool IsRunning => running;

    private void Awake()
    {
        if (motor == null)
            motor = FindObjectOfType<PlayerMotor>();

        if (interactor == null)
            interactor = FindObjectOfType<PlayerInteractor>();
    }

    public void Interact(bool slow)
    {
        if (!CanInteract)
            return;

        StartCoroutine(Run());
    }

    /// <summary>
    /// Volta a conversa ao começo — "a Sininho tem assunto novo agora".
    /// </summary>
    public void ResetConversation()
    {
        finished = false;
    }

    private IEnumerator Run()
    {
        running = true;
        HoldPlayer(true);
        onConversationStarted?.Invoke();

        int index = finished && repeatNode >= 0 ? repeatNode : startNode;

        while (index >= 0 && index < nodes.Length)
        {
            Node node = nodes[index];
            node.onEnter?.Invoke();

            yield return SpeakLines(node);

            if (node.choices == null || node.choices.Length == 0)
            {
                index = node.nextNode;
                continue;
            }

            int shown = CollectChoices(node);

            if (shown == 0)
            {
                index = node.nextNode;
                continue;
            }

            int picked = 0;

            DialogueChoiceUI ui = DialogueChoiceUI.Instance;

            if (ui == null)
            {
                // Sem caixa na cena a conversa não pode simplesmente travar:
                // ela segue pela primeira opção e avisa quem montou a cena.
                Debug.LogWarning("[NpcConversation] Sem DialogueChoiceUI na " +
                                 "cena: seguindo pela primeira escolha.", this);
            }
            else
            {
                // Enquanto a caixa está aberta, pular legenda é desligado: se a
                // tecla de pular for a mesma de confirmar (o Enter é o caso
                // clássico), um único toque pularia a fala e escolheria a opção
                // no mesmo frame — o jogador teria decidido sem ler.
                Subtitles subs = Subtitles.Instance;
                bool skipWas = subs != null && subs.SkipEnabled;

                if (subs != null)
                    subs.SkipEnabled = false;

                ui.Open(choiceTexts, shown);

                while (!ui.HasResult)
                    yield return null;

                picked = Mathf.Clamp(ui.Result, 0, shown - 1);
                ui.Close();

                if (subs != null)
                    subs.SkipEnabled = skipWas;
            }

            Choice choice = node.choices[choiceMap[picked]];

            string said = string.IsNullOrEmpty(choice.spoken) ? choice.text : choice.spoken;
            yield return Say(playerSpeaker, said);

            // A Lucidez sobe DEPOIS da fala: o nó seguinte já lê o valor novo,
            // que é o que faz "o tom muda se L ≥ 1" (Apêndice C) funcionar
            // dentro da mesma conversa.
            if (choice.raisesLucidez)
                Lucidez.Add();

            choice.onChosen?.Invoke();

            index = choice.nextNode;
        }

        finished = true;

        // Só devolve a mão depois que o jogador soltar a tecla: se ele
        // terminasse a conversa com o E ainda afundado, o PlayerInteractor
        // leria isso como um toque novo e a conversa recomeçaria sozinha.
        if (interactor != null)
        {
            while (Input.GetKey(interactor.InteractKey))
                yield return null;
        }

        HoldPlayer(false);
        running = false;

        onConversationFinished?.Invoke();
    }

    /// <summary>Enfileira todas as falas do nó e espera a legenda esvaziar.</summary>
    private IEnumerator SpeakLines(Node node)
    {
        if (node.lines == null || node.lines.Length == 0)
            yield break;

        Subtitles subs = Subtitles.Instance;

        if (subs == null || !subs.Enabled)
        {
            yield return new WaitForSeconds(fallbackLineSeconds * node.lines.Length);
            yield break;
        }

        for (int i = 0; i < node.lines.Length; i++)
            subs.Show(speaker, node.lines[i]);

        while (subs.IsShowing)
            yield return null;
    }

    private IEnumerator Say(string who, string what)
    {
        if (string.IsNullOrEmpty(what))
            yield break;

        Subtitles subs = Subtitles.Instance;

        if (subs == null || !subs.Enabled)
        {
            yield return new WaitForSeconds(fallbackLineSeconds);
            yield break;
        }

        subs.Show(who, what);

        while (subs.IsShowing)
            yield return null;
    }

    /// <summary>
    /// Copia para os buffers as escolhas que o Wendy pode fazer AGORA, com a
    /// Lucidez que ele tem agora. Devolve quantas são.
    /// </summary>
    private int CollectChoices(Node node)
    {
        // Os buffers nascem do tamanho da caixa que existe na cena, e só uma
        // vez: daqui para a frente a conversa inteira roda sem alocar nada.
        // Ficam aqui, e não no Awake, porque a caixa pode ainda não ter
        // acordado quando este script acorda — a ordem de Awake é indefinida.
        int slots = DialogueChoiceUI.Instance != null
            ? Mathf.Max(1, DialogueChoiceUI.Instance.Capacity)
            : 3;

        if (choiceTexts == null || choiceTexts.Length != slots)
        {
            choiceTexts = new string[slots];
            choiceMap = new int[slots];
        }

        int shown = 0;

        for (int i = 0; i < node.choices.Length && shown < choiceTexts.Length; i++)
        {
            Choice choice = node.choices[i];

            if (!Lucidez.AtLeast(choice.minLucidez))
                continue;

            choiceTexts[shown] = choice.text;
            choiceMap[shown] = i;
            shown++;
        }

        return shown;
    }

    private void HoldPlayer(bool hold)
    {
        if (holdPlayerStill && motor != null)
            motor.MoveLocked = hold;

        if (interactor != null)
            interactor.InteractionLocked = hold;
    }
}
