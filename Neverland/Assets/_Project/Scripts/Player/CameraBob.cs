using UnityEngine;

/// <summary>
/// Head-bob sutil ligado ao fôlego (GDD §5.1).
///
/// Duas camadas somadas:
///   · PASSOS — oscilação em 8 conforme o Wendy anda; intensifica ao correr.
///   · RESPIRAÇÃO — um balanço lento e mínimo que NUNCA para, nem parado. É o
///     que impede a câmera de virar um tripé e lembra que há uma criança
///     respirando ali.
/// Quando o fôlego acaba, a respiração cresce: a câmera arfa junto com o som
/// (§9.1 — o fôlego não tem barra, então ele precisa ser sentido).
///
/// Escreve APENAS no localPosition da própria câmera. A altura mora no
/// CameraRig (PlayerMotor) e o pitch também (PlayerLook) — assim nada aqui
/// disputa transform com ninguém.
///
/// Anexar ao GameObject "PlayerCamera".
/// </summary>
public class CameraBob : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("O PlayerMotor — de onde vêm velocidade e fôlego.")]
    [SerializeField] private PlayerMotor motor;

    [Header("Passos")]
    [Tooltip("Amplitude vertical do bob na velocidade de caminhada (m).")]
    [SerializeField] private float walkAmplitude = 0.035f;
    [Tooltip("Amplitude vertical do bob na corrida (m).")]
    [SerializeField] private float runAmplitude = 0.07f;
    [Tooltip("Passos por segundo na caminhada.")]
    [SerializeField] private float walkFrequency = 1.7f;
    [Tooltip("Passos por segundo na corrida.")]
    [SerializeField] private float runFrequency = 2.6f;
    [Tooltip("Quanto do movimento vertical vaza para o lado (0 = puro sobe-e-desce).")]
    [Range(0f, 1f)][SerializeField] private float lateralRatio = 0.5f;

    [Header("Respiração")]
    [Tooltip("Amplitude do balanço de respiração com fôlego cheio (m).")]
    [SerializeField] private float breathAmplitude = 0.006f;
    [Tooltip("Amplitude do balanço quando o Wendy está arfando (m).")]
    [SerializeField] private float windedBreathAmplitude = 0.022f;
    [Tooltip("Ciclos de respiração por segundo em repouso.")]
    [SerializeField] private float breathFrequency = 0.28f;
    [Tooltip("Ciclos de respiração por segundo arfando.")]
    [SerializeField] private float windedBreathFrequency = 1.1f;

    [Header("Geral")]
    [Tooltip("Multiplicador global — é o que a opção 'reduzir head-bob' da " +
             "acessibilidade (§9.4) vai mexer. 0 desliga por completo.")]
    [Range(0f, 1f)][SerializeField] private float intensity = 1f;
    [Tooltip("Suavização da volta ao centro quando o Wendy para.")]
    [SerializeField] private float damping = 8f;

    private Vector3 restPosition;   // a pose autorada da câmera na cena
    private float stepPhase;        // avança com a velocidade, não com o tempo
    private float breathPhase;
    private Vector3 currentOffset;

    private void Awake()
    {
        restPosition = transform.localPosition;

        if (motor == null)
        {
            motor = GetComponentInParent<PlayerMotor>();

            if (motor == null)
                Debug.LogWarning("[CameraBob] Sem PlayerMotor: só a respiração " +
                                 "vai acontecer.", this);
        }
    }

    private void LateUpdate()
    {
        float speed = motor != null ? motor.SpeedNormalized : 0f;
        bool winded = motor != null && motor.IsWinded;
        bool moving = motor == null || motor.IsMoving;

        // A fase dos passos anda proporcional à velocidade: parar congela o
        // ciclo em vez de continuar balançando no lugar.
        float frequency = Mathf.Lerp(walkFrequency, runFrequency, speed);
        if (moving)
            stepPhase += Time.deltaTime * frequency * Mathf.PI * 2f * Mathf.Max(speed, 0.35f);

        float amplitude = Mathf.Lerp(walkAmplitude, runAmplitude, speed) * speed;

        Vector3 step = new Vector3(
            Mathf.Sin(stepPhase * 0.5f) * amplitude * lateralRatio,
            Mathf.Sin(stepPhase) * amplitude,
            0f);

        // A respiração nunca para — e cresce quando o fôlego acaba.
        float breathFreq = winded ? windedBreathFrequency : breathFrequency;
        float breathAmp = winded ? windedBreathAmplitude : breathAmplitude;
        breathPhase += Time.deltaTime * breathFreq * Mathf.PI * 2f;

        Vector3 breath = new Vector3(
            0f,
            Mathf.Sin(breathPhase) * breathAmp,
            0f);

        Vector3 target = (step + breath) * intensity;

        currentOffset = Vector3.Lerp(
            currentOffset, target, 1f - Mathf.Exp(-damping * Time.deltaTime));

        transform.localPosition = restPosition + currentOffset;
    }

    /// <summary>
    /// Intensidade global do head-bob, de 0 a 1 — o gancho da opção de
    /// acessibilidade "reduzir head-bob" (§9.4).
    /// </summary>
    public void SetIntensity(float value)
    {
        intensity = Mathf.Clamp01(value);
    }
}
