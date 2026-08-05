using UnityEngine;

/// <summary>
/// Ao segurar a tecla Espaço o jogador "fecha os olhos": duas pálpebras curvas,
/// uma vinda de cima e outra de baixo, deslizam em direção ao centro até se
/// encontrarem (formato de olho). Ao soltar, elas recuam e os olhos "abrem".
/// </summary>
public class BlinkMechanic : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Pálpebra ancorada no TOPO da tela.")]
    [SerializeField] private Eyelid topEyelid;

    [Tooltip("Pálpebra ancorada na BASE da tela.")]
    [SerializeField] private Eyelid bottomEyelid;

    [Header("Configuração")]
    [Tooltip("Tecla usada para fechar os olhos.")]
    [SerializeField] private KeyCode blinkKey = KeyCode.Space;

    [Tooltip("Quão rápido os olhos fecham (fração por segundo).")]
    [SerializeField] private float closeSpeed = 2f;

    [Tooltip("Quão rápido os olhos abrem (fração por segundo).")]
    [SerializeField] private float openSpeed = 6f;

    [Tooltip("Marcado: a cena ABRE com os olhos já fechados, sem animação. É o " +
             "estado do James chegando na ilha — ele fechou os olhos no quarto " +
             "real e é o jogador quem os abre do outro lado. Sem isso, o campo " +
             "dourado piscaria na tela por meio segundo antes de as pálpebras " +
             "encostarem, e a manhã começaria pelo fim.")]
    [SerializeField] private bool startClosed = false;

    // 0 = olhos totalmente abertos, 1 = totalmente fechados.
    private float closeAmount = 0f;

    // Enquanto travado, o input é ignorado e as pálpebras obedecem lockedClosed.
    private bool locked = false;
    private bool lockedClosed = true;

    void Awake()
    {
        // A pose inicial é resolvida no Awake, e não no Start, para que qualquer
        // diretor de cena possa mandar nas pálpebras no próprio Start dele sem
        // depender de quem roda primeiro.
        closeAmount = startClosed ? 1f : 0f;
    }

    void Start()
    {
        ApplyEyelids();
    }

    void Update()
    {
        // Segurando a tecla: fecha. Soltando: abre.
        // Travado: o roteiro manda, não o jogador.
        bool isBlinking = locked ? lockedClosed : Input.GetKey(blinkKey);
        float target = isBlinking ? 1f : 0f;
        float speed = isBlinking ? closeSpeed : openSpeed;

        closeAmount = Mathf.MoveTowards(closeAmount, target, speed * Time.deltaTime);
        ApplyEyelids();
    }

    /// <summary>Repassa o estado atual para as duas pálpebras.</summary>
    private void ApplyEyelids()
    {
        if (topEyelid != null)
            topEyelid.SetClose(closeAmount);

        if (bottomEyelid != null)
            bottomEyelid.SetClose(closeAmount);
    }

    /// <summary>True quando as pálpebras estão totalmente fechadas.</summary>
    public bool EyesClosed => closeAmount >= 1f;

    /// <summary>Quanto os olhos estão fechados: 0 = abertos, 1 = fechados.</summary>
    public float CloseAmount => closeAmount;

    /// <summary>
    /// A tecla de fechar os olhos, para quem precisa reagir a ela sem duplicar
    /// a configuração — ex.: o PrologueEscape, que trata o primeiro toque
    /// depois da briga como definitivo.
    /// </summary>
    public KeyCode BlinkKey => blinkKey;

    /// <summary>
    /// Tira o controle das pálpebras do jogador e as leva para fechadas (ou
    /// abertas). Para quando o roteiro assume a cena — ex.: a travessia para o
    /// Dia, em que os olhos precisam ficar fechados durante o escuro.
    /// </summary>
    public void LockEyes(bool closed)
    {
        locked = true;
        lockedClosed = closed;
    }

    /// <summary>Devolve as pálpebras ao jogador.</summary>
    public void UnlockEyes()
    {
        locked = false;
    }

    /// <summary>
    /// Põe as pálpebras na pose pedida NA HORA, sem animação — e sem travá-las.
    /// Para corte de cena e para spawn: a manhã na ilha começa com os olhos
    /// fechados, e o jogador é quem os abre.
    /// </summary>
    public void SnapEyes(bool closed)
    {
        closeAmount = closed ? 1f : 0f;
        ApplyEyelids();
    }
}
