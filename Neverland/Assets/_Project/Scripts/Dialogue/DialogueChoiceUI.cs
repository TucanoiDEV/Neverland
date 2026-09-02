using TMPro;
using UnityEngine;

/// <summary>
/// A caixa de escolha de fala (GDD §5.9, §9.1).
///
/// Duas ou três frases empilhadas logo acima da legenda, na mesma família
/// visual: sem moldura, sem botão, sem ícone. A escolhida é a que tem o "›"
/// na frente. Navega com W/S (ou as setas) e confirma com E — a mesma tecla
/// de interagir, porque foi ela que abriu a conversa e a mão do Wendy não
/// muda de função no meio de uma frase. O F de pular legenda (Subtitles)
/// continua livre e não conflita.
///
/// Decisões de custo, porque isto vive dentro do HUD que roda o jogo inteiro:
///   · Sem Button, sem EventSystem, sem Image de fundo — nada aqui é alvo de
///     raycast de UI e nada acrescenta draw call quando fechado.
///   · O componente se AUTODESLIGA quando a caixa fecha (enabled = false):
///     com ele desligado a Unity nem chama o Update. Fora da conversa o
///     custo é exatamente zero.
///   · Os TMP das opções não usadas ficam desativados — TMP desativado não
///     remonta malha nem entra no batch do Canvas.
///   · O texto só é escrito na abertura e a cada troca de seleção, nunca por
///     frame: o Canvas só é remontado quando o jogador de fato mexe.
///
/// Quem pergunta é o NpcConversation; esta classe não sabe o que são nós,
/// Lucidez ou NPC — ela mostra frases e devolve um índice.
///
/// Anexar ao "Painel_Escolhas", dentro do Canvas do HUD. O GameObject fica
/// SEMPRE ativo (é o CanvasGroup que esconde): desativado, o Awake não
/// rodaria e o Instance nunca existiria.
/// </summary>
public class DialogueChoiceUI : MonoBehaviour
{
    /// <summary>Acesso de qualquer conversa da cena. Pode ser nulo — use `?.`.</summary>
    public static DialogueChoiceUI Instance { get; private set; }

    [Header("Referências de UI")]
    [Tooltip("CanvasGroup do painel — é por ele que a caixa aparece e some.")]
    [SerializeField] private CanvasGroup panel;

    [Tooltip("Os textos das opções, na ordem de cima para baixo. Três bastam: " +
             "o §5.9 escreve escolhas de 1–2 linhas, e uma criança de 9 anos " +
             "não tem quatro caminhos na cabeça ao mesmo tempo.")]
    [SerializeField] private TMP_Text[] options;

    [Header("Aparência")]
    [Tooltip("Vem na frente da opção sob o cursor.")]
    [SerializeField] private string selectedPrefix = "› ";

    [Tooltip("Vem na frente das outras. Espaços do mesmo tamanho evitam que a " +
             "linha 'pule' para o lado quando a seleção muda.")]
    [SerializeField] private string unselectedPrefix = "   ";

    [Tooltip("Cor da opção sob o cursor.")]
    [SerializeField] private Color selectedColor = Color.white;

    [Tooltip("Cor das demais. Apagada, não invisível: o jogador precisa ler " +
             "todas antes de escolher.")]
    [SerializeField] private Color dimColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    [Tooltip("Fade da caixa. Curto — ela aparece, não 'anima' (§10).")]
    [SerializeField] private float fadeSeconds = 0.1f;

    [Header("Teclas")]
    [SerializeField] private KeyCode upKey = KeyCode.W;
    [SerializeField] private KeyCode upAltKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode downKey = KeyCode.S;
    [SerializeField] private KeyCode downAltKey = KeyCode.DownArrow;

    [Tooltip("Confirmar. Mesma tecla de interagir, de propósito.")]
    [SerializeField] private KeyCode confirmKey = KeyCode.E;
    [SerializeField] private KeyCode confirmAltKey = KeyCode.Return;

    [Header("Som (opcional)")]
    [Tooltip("Fonte 2D dos bipes da caixa. Vazio: a caixa é muda.")]
    [SerializeField] private AudioSource sfx;
    [Tooltip("Bipe ao mudar de opção.")]
    [SerializeField] private AudioClip moveClip;
    [Tooltip("Bipe ao confirmar.")]
    [SerializeField] private AudioClip confirmClip;

    private int count;          // quantas opções estão na tela agora
    private int index;          // qual está sob o cursor
    private int result = -1;    // índice escolhido, ou -1 enquanto ninguém escolheu
    private bool armed;         // a tecla de confirmar já foi solta desde a abertura
    private float alpha;

    /// <summary>True enquanto a caixa está pedindo uma escolha.</summary>
    public bool IsOpen => count > 0;

    /// <summary>True quando o jogador confirmou; leia então o Result.</summary>
    public bool HasResult => result >= 0;

    /// <summary>A opção escolhida, ou -1. Índice DENTRO da lista mostrada.</summary>
    public int Result => result;

    /// <summary>Quantos textos a caixa aguenta mostrar de uma vez.</summary>
    public int Capacity => options != null ? options.Length : 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[DialogueChoiceUI] Já existe uma caixa de escolha " +
                             "nesta cena. Removendo o componente duplicado.", this);
            Destroy(this);
            return;
        }

        Instance = this;

        if (panel != null)
        {
            // A caixa é leitura, não clique: nunca pode comer o mouse.
            panel.blocksRaycasts = false;
            panel.interactable = false;
            panel.alpha = 0f;
        }

        HideAll();

        // Fechada: sem Update, sem custo.
        enabled = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Mostra as escolhas e passa a esperar. 'texts' pode ser um buffer maior
    /// que 'howMany' — só os primeiros 'howMany' são lidos, para que quem
    /// chama reaproveite o mesmo array a conversa inteira e não gere lixo.
    /// </summary>
    public void Open(string[] texts, int howMany)
    {
        if (options == null || options.Length == 0 || texts == null)
        {
            Debug.LogWarning("[DialogueChoiceUI] Sem textos de opção ligados: a " +
                             "escolha não tem como aparecer.", this);
            return;
        }

        count = Mathf.Clamp(howMany, 0, options.Length);
        index = 0;
        result = -1;

        // Se o jogador ainda está com o E afundado (foi ele que abriu a
        // conversa), a caixa só passa a aceitar confirmação depois que a tecla
        // subir — senão a primeira opção seria escolhida no mesmo frame em que
        // apareceu, sem ninguém ler nada.
        armed = !Input.GetKey(confirmKey) && !Input.GetKey(confirmAltKey);

        for (int i = 0; i < options.Length; i++)
        {
            bool used = i < count;
            options[i].gameObject.SetActive(used);

            if (!used)
                continue;

            bool selected = i == index;
            options[i].text = (selected ? selectedPrefix : unselectedPrefix) + texts[i];
            options[i].color = selected ? selectedColor : dimColor;
        }

        enabled = true;
    }

    /// <summary>Fecha a caixa e volta a custar zero.</summary>
    public void Close()
    {
        count = 0;
        result = -1;
        HideAll();
    }

    private void Update()
    {
        if (count == 0)
        {
            // Só continua ligado enquanto o fade de saída ainda tem o que fazer.
            if (Fade(0f))
                enabled = false;

            return;
        }

        Fade(1f);

        if (!armed)
        {
            armed = !Input.GetKey(confirmKey) && !Input.GetKey(confirmAltKey);
            return;
        }

        if (result >= 0)
            return;

        int step = 0;

        if (Input.GetKeyDown(downKey) || Input.GetKeyDown(downAltKey))
            step = 1;
        else if (Input.GetKeyDown(upKey) || Input.GetKeyDown(upAltKey))
            step = -1;

        if (step != 0 && count > 1)
        {
            int previous = index;
            index = (index + step + count) % count;
            Repaint(previous);
            Play(moveClip);
        }

        if (Input.GetKeyDown(confirmKey) || Input.GetKeyDown(confirmAltKey))
        {
            result = index;
            Play(confirmClip);
        }
    }

    /// <summary>
    /// Reescreve só as duas linhas que mudaram de estado. Trocar o texto de um
    /// TMP marca o Canvas para remontar; mexer nas outras à toa seria remontar
    /// de graça.
    /// </summary>
    private void Repaint(int previous)
    {
        WritePrefix(previous, false);
        WritePrefix(index, true);
    }

    private void WritePrefix(int slot, bool selected)
    {
        TMP_Text label = options[slot];
        string prefix = selected ? selectedPrefix : unselectedPrefix;
        string old = selected ? unselectedPrefix : selectedPrefix;

        if (!string.IsNullOrEmpty(old) && label.text.StartsWith(old))
            label.text = prefix + label.text.Substring(old.Length);

        label.color = selected ? selectedColor : dimColor;
    }

    private void HideAll()
    {
        if (options == null)
            return;

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null)
                options[i].gameObject.SetActive(false);
        }
    }

    /// <summary>Aproxima o alfa do alvo. Devolve true quando já chegou.</summary>
    private bool Fade(float target)
    {
        if (panel == null)
            return true;

        // Tempo não escalado, como a legenda: uma câmera lenta não pode
        // atrasar a caixa que está esperando o jogador.
        alpha = fadeSeconds > 0f
            ? Mathf.MoveTowards(alpha, target, Time.unscaledDeltaTime / fadeSeconds)
            : target;

        panel.alpha = alpha;
        return Mathf.Approximately(alpha, target);
    }

    private void Play(AudioClip clip)
    {
        if (sfx != null && clip != null)
            sfx.PlayOneShot(clip);
    }
}
