using UnityEngine;

/// <summary>
/// Os passos do Wendy (GDD §11.2 — "o som diegético é o verdadeiro HUD").
///
/// O §9.1 tira da tela tudo o que puder viver no mundo: não há barra de fôlego,
/// não há barra de vida. O que sobra para o jogador sentir o próprio corpo é o
/// som — e o passo é a batida desse corpo. Por isso este script não é enfeite:
/// é a metade audível do §5.3, o par do head-bob que o CameraBob já faz.
///
/// O áudio de passo do projeto (`SFX_PlayerFootsteps_01`) é uma CAMA CONTÍNUA
/// de caminhada, de quase um minuto — não um passo isolado. Então o script não
/// dispara um som por passada: ele mantém o loop tocando enquanto o Wendy anda
/// e o silencia quando ele para. Tentar picar essa cama em passadas seria
/// empilhar cópias dela por cima de si mesma.
///
/// A cadência vem do PITCH. Um take de caminhada tem o andar de quem gravou;
/// acelerar a leitura aproxima as pisadas e afinar as afasta, que é a maneira
/// barata e antiga de fazer um take só servir para andar, correr e se esgueirar.
/// É também coerente com o §5.4: agachado o som não fica só mais baixo, fica
/// mais LENTO — o corpo escolhendo onde pisar.
///
/// O volume sai da mesma tabela de ruído do §6.1 que o PlayerMotor já publica
/// em 'CurrentNoiseRadius'. A escuta ainda não existe (a Sininho só caça na
/// noite, §6.1), mas o que o jogador ouve e o que a caçadora vai ouvir já são o
/// mesmo número — quando a IA entrar, não vai precisar de um segundo conceito
/// de "barulho".
///
/// Anexar ao MESMO GameObject do PlayerMotor (o 'Player').
/// </summary>
[RequireComponent(typeof(PlayerMotor))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("De onde o som sai. Deixe vazio para o script criar um AudioSource " +
             "no próprio Player. 3D (spatialBlend 1): o passo nasce no corpo, " +
             "não dentro da mixagem.")]
    [SerializeField] private AudioSource source;

    [Tooltip("A cama de caminhada, em loop. Um take contínuo de passos — não um " +
             "passo isolado.")]
    [SerializeField] private AudioClip walkLoop;

    [Header("Volume por marcha")]
    [Tooltip("Volume agachado — o andar de quem não quer ser ouvido (§5.4).")]
    [Range(0f, 1f)][SerializeField] private float volumeCrouch = 0.16f;

    [Tooltip("Volume andando.")]
    [Range(0f, 1f)][SerializeField] private float volumeWalk = 0.42f;

    [Tooltip("Volume correndo.")]
    [Range(0f, 1f)][SerializeField] private float volumeRun = 0.7f;

    [Tooltip("Multiplicador sem fôlego (§5.3): ele pisa mais pesado porque já " +
             "não controla o corpo. Espelha o 'windedNoiseMultiplier' do motor.")]
    [SerializeField] private float windedVolumeMultiplier = 1.3f;

    [Header("Cadência (pitch)")]
    [Tooltip("Pitch agachado. Abaixo de 1 as pisadas se afastam: passo lento e " +
             "cuidadoso.")]
    [SerializeField] private float pitchCrouch = 0.78f;

    [Tooltip("Pitch andando. 1 = a cadência original da gravação. Se o take " +
             "soar rápido ou lento demais para uma criança, é ESTE número que " +
             "se ajusta — os outros dois são relativos a ele.")]
    [SerializeField] private float pitchWalk = 1f;

    [Tooltip("Pitch correndo. Acima de 1 as pisadas se aproximam.")]
    [SerializeField] private float pitchRun = 1.4f;

    [Header("Transições")]
    [Tooltip("Velocidade do fade de volume ao começar e parar de andar. Alto " +
             "demais corta seco; baixo demais deixa um rastro de passos depois " +
             "de o Wendy já ter parado.")]
    [SerializeField] private float fadeSpeed = 9f;

    [Tooltip("Velocidade da mudança de cadência ao trocar de marcha. Trocar de " +
             "pitch num frame só soa como fita acelerando.")]
    [SerializeField] private float pitchSmoothing = 7f;

    [Tooltip("Abaixo deste volume o loop é pausado, em vez de seguir tocando " +
             "mudo. Pausar (e não parar) faz o take retomar de onde estava: " +
             "recomeçar sempre do mesmo sample denuncia a gravação.")]
    [SerializeField] private float silenceThreshold = 0.003f;

    private PlayerMotor motor;
    private CharacterController controller;
    private float volume;
    private float pitch = 1f;
    private bool paused;

    private void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        controller = GetComponent<CharacterController>();

        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
            source.spatialBlend = 1f;   // 3D: o passo é do corpo
            source.minDistance = 1f;
            source.maxDistance = 25f;
        }

        source.playOnAwake = false;
        source.loop = true;
        source.volume = 0f;
        pitch = pitchWalk;
        source.pitch = pitch;

        if (walkLoop != null)
            source.clip = walkLoop;
        else
            Debug.LogWarning("[PlayerFootsteps] Sem cama de caminhada: o Wendy " +
                             "vai andar em silêncio absoluto.", this);
    }

    private void Update()
    {
        if (source == null || source.clip == null)
            return;

        bool grounded = controller == null || controller.isGrounded;
        bool moving = motor.IsMoving && grounded;

        // Volume-alvo: zero parado, a marcha atual em movimento.
        float target = 0f;
        if (moving)
        {
            target = motor.IsCrouching ? volumeCrouch
                   : motor.IsRunning ? volumeRun
                   : volumeWalk;

            if (motor.IsWinded)
                target *= windedVolumeMultiplier;

            target = Mathf.Clamp01(target);
        }

        volume = Mathf.MoveTowards(volume, target, fadeSpeed * Time.deltaTime);
        source.volume = volume;

        // A cadência só muda enquanto ele anda. Parado, o pitch congela onde
        // estava: reajustá-lo no silêncio faria a próxima arrancada começar
        // com a cadência errada por uma fração de segundo.
        if (moving)
        {
            float targetPitch = motor.IsCrouching ? pitchCrouch
                              : motor.IsRunning ? pitchRun
                              : pitchWalk;

            pitch = Mathf.MoveTowards(pitch, targetPitch, pitchSmoothing * Time.deltaTime);
            source.pitch = pitch;
        }

        if (volume > silenceThreshold)
        {
            if (!source.isPlaying)
            {
                // UnPause e não Play: Play() rebobina o take para o começo, e
                // toda arrancada sairia do mesmo sample — o ouvido pega isso na
                // terceira vez. UnPause retoma de onde a caminhada parou.
                if (paused)
                {
                    source.UnPause();
                    paused = false;
                }
                else
                {
                    source.Play();
                }
            }
        }
        else if (source.isPlaying)
        {
            source.Pause();
            paused = true;
        }
    }
}
