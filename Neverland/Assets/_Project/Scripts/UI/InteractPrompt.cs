using TMPro;
using UnityEngine;

/// <summary>
/// O "Pressione E" que aparece quando o Wendy chega perto de algo com que dá
/// para mexer.
///
/// O GDD §9.1 pede o mínimo de tela possível, e no jogo final quem diz "dá para
/// mexer aqui" é o retículo virando mão — sem palavra nenhuma. Este texto
/// existe porque ninguém nasce sabendo que a tecla é E: ele é o tutorial
/// diegético do Ato I (§2.3, "mini-interações que ensinam os controles"), e a
/// intenção é que ele se apague sozinho depois das primeiras vezes. É para isso
/// que serve 'maxShows'.
///
/// Não lê o teclado e não decide nada: só observa o PlayerInteractor e escreve.
/// A tecla vem de lá (InteractKey), então remapear a tecla reescreve o texto
/// sozinho.
///
/// Anexar ao objeto de texto do prompt, dentro do Canvas do HUD.
/// </summary>
public class InteractPrompt : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("O PlayerInteractor do Wendy. Vazio: procura na cena.")]
    [SerializeField] private PlayerInteractor interactor;

    [Tooltip("O texto do prompt. Vazio: pega o TMP_Text deste objeto.")]
    [SerializeField] private TMP_Text label;

    [Header("O que escrever")]
    [Tooltip("{0} vira a tecla ('E'), {1} vira o verbo do objeto ('Falar', " +
             "'Abrir'). Ex.: 'Pressione {0} para {1}'.")]
    [SerializeField] private string format = "Pressione {0}";

    [Header("Aparência")]
    [Tooltip("Segundos do fade. Curto — o prompt aparece, não 'anima'.")]
    [SerializeField] private float fadeSeconds = 0.12f;

    [Tooltip("Opacidade máxima. Discreto: ele não é o assunto da tela.")]
    [Range(0f, 1f)][SerializeField] private float maxAlpha = 0.8f;

    [Header("Sumir com o tempo (tutorial)")]
    [Tooltip("Quantas vezes o prompt pode aparecer antes de se aposentar de " +
             "vez. 0 = para sempre, que é o certo enquanto vocês testam.")]
    [SerializeField] private int maxShows = 0;

    [Tooltip("Marcado: some enquanto uma legenda está na tela. É honestidade — " +
             "durante a fala o E não faz nada (ver NpcDialogue), então " +
             "continuar pedindo a tecla seria mentira.")]
    [SerializeField] private bool hideWhileSpeaking = true;

    private float alpha;
    private bool wasVisible;
    private int shows;

    private void Awake()
    {
        if (interactor == null)
            interactor = FindObjectOfType<PlayerInteractor>();

        if (label == null)
            label = GetComponent<TMP_Text>();

        if (interactor == null)
            Debug.LogWarning("[InteractPrompt] Sem PlayerInteractor: o prompt " +
                             "nunca aparece.", this);

        if (label == null)
            Debug.LogWarning("[InteractPrompt] Sem TMP_Text: não há onde " +
                             "escrever.", this);

        SetAlpha(0f);
    }

    private void LateUpdate()
    {
        if (label == null)
            return;

        bool visible = ShouldShow();

        // Conta uma "aparição" por vez que o prompt nasce, não por frame.
        if (visible && !wasVisible)
        {
            shows++;

            if (maxShows > 0 && shows > maxShows)
                visible = false;
        }

        wasVisible = visible;

        if (visible)
            label.text = string.Format(format, KeyLabel(), interactor.Target.Prompt);

        float step = fadeSeconds > 0f ? Time.deltaTime / fadeSeconds : 1f;
        SetAlpha(Mathf.MoveTowards(alpha, visible ? maxAlpha : 0f, step));
    }

    private bool ShouldShow()
    {
        if (interactor == null || interactor.InteractionLocked)
            return false;

        if (!interactor.TargetInReach || !interactor.Target.CanInteract)
            return false;

        if (hideWhileSpeaking && Subtitles.Instance != null
            && Subtitles.Instance.IsShowing)
            return false;

        return true;
    }

    /// <summary>
    /// O nome da tecla como uma criança escreveria: 'E', não 'KeyCode.E'. Os
    /// botões de mouse não têm nome bonito no KeyCode, então ganham o deles.
    /// </summary>
    private string KeyLabel()
    {
        switch (interactor.InteractKey)
        {
            case KeyCode.Mouse0: return "clique esquerdo";
            case KeyCode.Mouse1: return "clique direito";
            case KeyCode.Space:  return "Espaço";
            default:             return interactor.InteractKey.ToString();
        }
    }

    private void SetAlpha(float value)
    {
        alpha = value;

        Color color = label.color;
        color.a = alpha;
        label.color = color;

        // Mesma economia do Reticle (§12.3): invisível não custa draw call.
        // Mas só quando o texto é OUTRO objeto — desligar o objeto onde este
        // script mora mataria o LateUpdate e o prompt nunca mais acenderia.
        // Nesse caso basta desligar o componente de texto.
        if (label.gameObject != gameObject)
        {
            if (label.gameObject.activeSelf != (alpha > 0.001f))
                label.gameObject.SetActive(alpha > 0.001f);
        }
        else if (label.enabled != (alpha > 0.001f))
        {
            label.enabled = alpha > 0.001f;
        }
    }
}
