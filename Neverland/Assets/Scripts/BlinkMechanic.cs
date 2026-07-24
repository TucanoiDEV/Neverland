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

    // 0 = olhos totalmente abertos, 1 = totalmente fechados.
    private float closeAmount = 0f;

    void Start()
    {
        closeAmount = 0f;
        ApplyEyelids();
    }

    void Update()
    {
        // Segurando a tecla: fecha. Soltando: abre.
        bool isBlinking = Input.GetKey(blinkKey);
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
}
