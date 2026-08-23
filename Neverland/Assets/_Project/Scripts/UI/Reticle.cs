using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// O único elemento permanente da tela (GDD §9.1).
///
/// A filosofia da interface é "o mínimo de tela possível", e o retículo é o
/// caso-limite dela: na maior parte do jogo ele NÃO EXISTE. Só aparece quando
/// há algo com que mexer, e em três estados:
///   · nada à frente        → tela limpa;
///   · algo interativo longe→ um ponto;
///   · algo interativo perto→ o ícone de mão;
/// e, enquanto o jogador segura E, um anel que enche mostrando a interação
/// lenta (§5.2). O anel é a ÚNICA barra de progresso do jogo — e ela existe
/// porque abrir uma porta em silêncio precisa de um relógio visível, senão o
/// jogador solta a tecla cedo demais e não entende o barulho que fez.
///
/// Sem fôlego, sem vida, sem munição, sem radar: se aparecer algo além disto
/// na tela, está errado.
///
/// Anexar ao objeto "Reticle" dentro do Canvas do HUD.
/// </summary>
public class Reticle : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("O PlayerInteractor do Wendy. Vazio: procura na cena.")]
    [SerializeField] private PlayerInteractor interactor;

    [Header("Estados")]
    [Tooltip("O ponto de 2 px — 'há algo ali'.")]
    [SerializeField] private Graphic dot;
    [Tooltip("O ícone de mão — 'você alcança'.")]
    [SerializeField] private Graphic hand;
    [Tooltip("Anel de preenchimento da interação lenta. Image com Image Type " +
             "= Filled, Fill Method = Radial 360.")]
    [SerializeField] private Image slowFill;

    [Header("Aparência")]
    [Tooltip("Segundos do fade entre estados. Curto: o retículo aparece, não " +
             "'anima'.")]
    [SerializeField] private float fadeSeconds = 0.12f;
    [Tooltip("Opacidade máxima. O GDD pede discrição — 1 costuma ser demais.")]
    [Range(0f, 1f)][SerializeField] private float maxAlpha = 0.75f;

    private float dotAlpha;
    private float handAlpha;

    private void Awake()
    {
        if (interactor == null)
            interactor = FindObjectOfType<PlayerInteractor>();

        if (interactor == null)
            Debug.LogWarning("[Reticle] Sem PlayerInteractor: o retículo fica " +
                             "apagado o jogo inteiro.", this);

        SetAlpha(dot, 0f);
        SetAlpha(hand, 0f);

        if (slowFill != null)
        {
            slowFill.fillAmount = 0f;
            SetAlpha(slowFill, 0f);
        }
    }

    private void LateUpdate()
    {
        bool hasTarget = interactor != null && interactor.Target != null;
        bool inReach = interactor != null && interactor.TargetInReach;

        // Longe: o ponto. Perto: a mão. Nunca os dois ao mesmo tempo.
        float dotTarget = hasTarget && !inReach ? maxAlpha : 0f;
        float handTarget = inReach ? maxAlpha : 0f;

        float step = fadeSeconds > 0f ? Time.deltaTime / fadeSeconds : 1f;

        dotAlpha = Mathf.MoveTowards(dotAlpha, dotTarget, step);
        handAlpha = Mathf.MoveTowards(handAlpha, handTarget, step);

        SetAlpha(dot, dotAlpha);
        SetAlpha(hand, handAlpha);

        if (slowFill == null)
            return;

        // O anel só existe durante a segurada; fora dela, some por completo em
        // vez de ficar um círculo vazio na tela.
        bool holding = interactor != null && interactor.IsHoldingSlow;

        slowFill.fillAmount = holding ? interactor.SlowProgress : 0f;
        SetAlpha(slowFill, holding ? maxAlpha : 0f);
    }

    private void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null)
            return;

        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;

        // Desligar o objeto quando invisível evita que ele continue custando
        // draw call no orçamento apertado do §12.3.
        if (graphic.gameObject.activeSelf != (alpha > 0.001f))
            graphic.gameObject.SetActive(alpha > 0.001f);
    }
}
