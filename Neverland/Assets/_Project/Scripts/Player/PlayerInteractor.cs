using UnityEngine;

/// <summary>
/// A mão do Wendy (GDD §5.2, §9.1).
///
/// Um raycast saindo da câmera, e a regra que decide o jogo inteiro de furtivo:
///   · TOCAR E   → interação rápida. Acontece na hora e FAZ RUÍDO.
///   · SEGURAR E → interação lenta. Demora, e é silenciosa.
/// Por isso a rápida só dispara ao SOLTAR a tecla antes do tempo: até o botão
/// subir, o jogo ainda não sabe qual das duas o jogador quis. Meio segundo de
/// espera para o retículo começar a encher — e é o próprio jogador que
/// descobre, na primeira gaveta, que a pressa custa caro.
///
/// O alcance tem dois raios (§9.1): o de DETECÇÃO, em que o retículo vira um
/// pontinho avisando "há algo ali", e o de MÃO, mais curto, em que a interação
/// de fato existe.
///
/// Anexar ao GameObject "Player".
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("A câmera de onde sai o raycast. Vazio: procura na hierarquia.")]
    [SerializeField] private Camera playerCamera;

    [Header("Alcance (§9.1)")]
    [Tooltip("Distância em que o retículo ACENDE, avisando que há algo " +
             "interativo à frente (m).")]
    [SerializeField] private float detectRange = 5f;
    [Tooltip("Distância em que o Wendy alcança de verdade (m). Braço de " +
             "criança: mantenha curto.")]
    [SerializeField] private float interactRange = 1.8f;
    [Tooltip("Raio do 'dedo': um SphereCast perdoa a mira tremida sem virar " +
             "um ímã. 0 = raio fino e exato.")]
    [SerializeField] private float castRadius = 0.08f;
    [Tooltip("O que o raio pode acertar. Deixe de fora a própria camada do " +
             "Player para o raycast não bater no corpo do Wendy.")]
    [SerializeField] private LayerMask mask = ~0;

    [Header("Interação lenta (§5.2)")]
    [Tooltip("Segundos segurando a tecla até a interação lenta e silenciosa " +
             "completar. É a moeda do stealth: caro o bastante para doer " +
             "com a Sininho por perto, curto o bastante para não irritar.")]
    [SerializeField] private float slowInteractSeconds = 1.4f;
    [Tooltip("Tempo segurando a partir do qual o jogo entende que o jogador " +
             "QUER a versão lenta. Soltar antes disso executa a rápida.")]
    [SerializeField] private float holdRecognitionSeconds = 0.18f;

    [Header("Teclas")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [Tooltip("Alternativa (§5.2: 'E ou clique esquerdo').")]
    [SerializeField] private KeyCode altInteractKey = KeyCode.Mouse0;

    private IInteractable target;      // o que está sob a mira agora
    private float targetDistance;
    private float holdTime;
    private bool holdConsumed;         // a lenta já completou nesta segurada

    /// <summary>O interativo sob a mira, ou null. O retículo lê daqui.</summary>
    public IInteractable Target => target;

    /// <summary>True quando o alvo está perto o bastante para ser tocado.</summary>
    public bool TargetInReach => target != null && targetDistance <= interactRange;

    /// <summary>
    /// Progresso da interação lenta, de 0 a 1 — o preenchimento do retículo.
    /// </summary>
    public float SlowProgress =>
        slowInteractSeconds > 0f ? Mathf.Clamp01(holdTime / slowInteractSeconds) : 0f;

    /// <summary>True enquanto a segurada já foi reconhecida como "lenta".</summary>
    public bool IsHoldingSlow =>
        !holdConsumed && holdTime >= holdRecognitionSeconds && TargetInReach
        && target != null && target.SupportsSlowInteract;

    /// <summary>
    /// Tira a interação do jogador — cutscenes, diálogo, e a fuga final do
    /// clímax, que roda "sem inventário, sem interações" (§7.4).
    /// </summary>
    public bool InteractionLocked { get; set; }

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
            Debug.LogWarning("[PlayerInteractor] Sem câmera: nada será " +
                             "detectado.", this);
    }

    private void Update()
    {
        if (InteractionLocked)
        {
            target = null;
            ResetHold();
            return;
        }

        ScanForTarget();
        ReadInput();
    }

    private void ScanForTarget()
    {
        target = null;
        targetDistance = float.MaxValue;

        if (playerCamera == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        bool hitSomething = castRadius > 0f
            ? Physics.SphereCast(ray, castRadius, out hit, detectRange, mask,
                                 QueryTriggerInteraction.Collide)
            : Physics.Raycast(ray, out hit, detectRange, mask,
                              QueryTriggerInteraction.Collide);

        if (!hitSomething)
            return;

        // GetComponentInParent para que um colisor filho (a maçaneta, a gaveta)
        // encontre o script no objeto pai.
        IInteractable found = hit.collider.GetComponentInParent<IInteractable>();

        if (found == null || !found.CanInteract)
            return;

        target = found;
        targetDistance = hit.distance;
    }

    private void ReadInput()
    {
        bool held = Input.GetKey(interactKey) || Input.GetKey(altInteractKey);
        bool released = Input.GetKeyUp(interactKey) || Input.GetKeyUp(altInteractKey);

        // Soltar a tecla ou perder o alvo zera a segurada — mirar para longe no
        // meio de abrir uma porta devagar cancela a ação, de propósito.
        if (!held || !TargetInReach)
        {
            if (released && !holdConsumed && holdTime > 0f && holdTime < holdRecognitionSeconds
                && TargetInReach)
            {
                // Toque curto: a versão rápida, a barulhenta.
                target.Interact(false);
            }

            ResetHold();
            return;
        }

        if (holdConsumed)
            return;

        holdTime += Time.deltaTime;

        // Objeto que não aceita a versão lenta: segurar não adianta nada, e a
        // ação sai assim que o tempo de reconhecimento passa, para o jogador
        // não ficar segurando um botão morto.
        if (!target.SupportsSlowInteract)
        {
            if (holdTime >= holdRecognitionSeconds)
            {
                target.Interact(false);
                holdConsumed = true;
            }

            return;
        }

        if (holdTime >= slowInteractSeconds)
        {
            target.Interact(true);
            holdConsumed = true;
        }
    }

    private void ResetHold()
    {
        holdTime = 0f;
        holdConsumed = false;
    }
}
