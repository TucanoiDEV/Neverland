using System.Collections;
using UnityEngine;

/// <summary>
/// Orquestra o prólogo — O Quarto Real (GDD §2.3, §4.1).
/// Beats, em ordem, via coroutine:
///   0. A CHUVA e A MÚSICA — dois loops presentes desde o primeiro frame. Não
///      são beats: são o chão em que os beats acontecem. Ambos recuam e sobem
///      conforme a cena pede, e ambos morrem no fade final.
///   1. A ORAÇÃO — a mãe recita o Pai-Nosso em primeiro plano (§4.1).
///   2. A MÃE SAI — a silhueta (cápsula de greybox) caminha da beira da cama
///      até a porta.
///   3. A PORTA FECHA — gira até fechar, o baque seco toca e a fresta de luz
///      se apaga, deixando o quarto no breu (§4.1); a chuva sobe, porque no
///      escuro ela é tudo que sobra.
///   4. A BRIGA — depois de um silêncio deliberado, as vozes atrás da parede
///      entram e a chuva recua para que elas cortem por cima.
/// Os AudioSources são referências de cena: espacialização, grupo de mixer e o
/// AudioLowPassFilter da briga ficam a seu critério no Inspector (§12.1).
/// Todo slot de áudio é opcional — sem clipe, o beat passa direto.
/// O handoff para o BlinkMechanic entra em um passo futuro.
/// Anexar a um GameObject vazio da cena (ex.: "PrologueDirector").
/// </summary>
public class PrologueDirector : MonoBehaviour
{
    [Header("Ritmo")]
    [Tooltip("Espera antes de a mãe começar a se levantar/sair (segundos).")]
    [SerializeField] private float delayBeforeMotherLeaves = 1.5f;
    [Tooltip("Pausa entre a mãe alcançar a porta e a porta começar a fechar.")]
    [SerializeField] private float delayBeforeDoorCloses = 0.3f;

    [Header("A mãe (cápsula) saindo")]
    [Tooltip("Transform da mãe — a cápsula de greybox.")]
    [SerializeField] private Transform mother;
    [Tooltip("Ponto na porta até onde a mãe caminha antes de sair de cena.")]
    [SerializeField] private Transform motherExitPoint;
    [Tooltip("Velocidade de caminhada da mãe (m/s).")]
    [SerializeField] private float motherWalkSpeed = 1.2f;
    [Tooltip("Se marcado, a cápsula gira para encarar a direção do movimento.")]
    [SerializeField] private bool motherFacesMovement = true;
    [Tooltip("Distância (m) do ponto de saída em que a mãe é considerada 'na porta'.")]
    [SerializeField] private float arriveThreshold = 0.05f;
    [Tooltip("Desativar a mãe ao terminar de sair (ela some ao fechar a porta).")]
    [SerializeField] private bool disableMotherOnExit = true;

    [Header("A porta fechando")]
    [Tooltip("O painel da porta — pode ser um cubo comum, pivô no centro. Não precisa parentear nada.")]
    [SerializeField] private Transform door;
    [Tooltip("Empty posicionado na aresta da dobradiça. A porta gira em torno dele. " +
             "Se vazio, a porta gira em torno do próprio pivô (fallback).")]
    [SerializeField] private Transform doorHinge;
    [Tooltip("Marque se você posicionou a porta JÁ FECHADA na cena (encaixada no vão): " +
             "o script a abre sozinho antes do primeiro frame e a fecha no beat. " +
             "Desmarcado: a pose da cena é a ABERTA.")]
    [SerializeField] private bool startFromClosedPose = false;
    [Tooltip("Ângulo, em graus, entre aberta e fechada.")]
    [SerializeField] private float doorCloseAngle = 90f;
    [Tooltip("Quanto tempo a porta leva para fechar (segundos).")]
    [SerializeField] private float doorCloseDuration = 0.6f;
    [Tooltip("Curva do fechamento (o baque seco pede um fim abrupto).")]
    [SerializeField] private AnimationCurve doorCloseEase =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("A fresta de luz")]
    [Tooltip("Luz da fresta da porta — some conforme a porta fecha (§4.1).")]
    [SerializeField] private Light doorGapLight;

    [Header("Áudio · a chuva (ambiência)")]
    [Tooltip("AudioSource em loop com a chuva. Ela emoldura o prólogo inteiro — " +
             "mas nunca deve competir com o baque, com o silêncio ou com as vozes (§11.4).")]
    [SerializeField] private AudioSource rainLoop;
    [Tooltip("Volume da chuva enquanto a mãe ainda está no quarto.")]
    [Range(0f, 1f)][SerializeField] private float rainVolumeStart = 0.35f;
    [Tooltip("Volume da chuva depois de a porta fechar: no breu, ela é tudo que sobra.")]
    [Range(0f, 1f)][SerializeField] private float rainVolumeInTheDark = 0.6f;
    [Tooltip("Volume da chuva durante a briga: recua para as vozes cortarem por cima.")]
    [Range(0f, 1f)][SerializeField] private float rainVolumeUnderFight = 0.4f;
    [Tooltip("Duração de cada transição de volume da chuva (segundos).")]
    [SerializeField] private float rainFadeDuration = 1.5f;

    [Header("Áudio · a música de fundo")]
    [Tooltip("AudioSource em loop com a música do prólogo. Como a chuva, ela nasce " +
             "junto com a cena — mas mora embaixo de tudo: o baque, o silêncio e as " +
             "vozes vêm primeiro (§11.4). Deixe 2D (Spatial Blend 0) e, de " +
             "preferência, num grupo de mixer separado do ambiente.")]
    [SerializeField] private AudioSource musicLoop;
    [Tooltip("Volume da música enquanto a mãe ainda está no quarto.")]
    [Range(0f, 1f)][SerializeField] private float musicVolumeStart = 0.25f;
    [Tooltip("Volume da música depois de a porta fechar — o breu é dela e da chuva.")]
    [Range(0f, 1f)][SerializeField] private float musicVolumeInTheDark = 0.3f;
    [Tooltip("Volume da música durante a briga: recua junto com a chuva para as " +
             "vozes cortarem por cima.")]
    [Range(0f, 1f)][SerializeField] private float musicVolumeUnderFight = 0.15f;
    [Tooltip("Duração de cada transição de volume da música (segundos).")]
    [SerializeField] private float musicFadeDuration = 1.5f;
    [Tooltip("Marcado (padrão): a música NÃO morre junto com o quarto no fade final — " +
             "ela atravessa o escuro e segura o sussurro por baixo. É o certo quando " +
             "a música pertence à memória (o chiado de VHS) e não ao quarto: o que " +
             "afunda no silêncio é a chuva, a briga e a mãe. Quem a desliga passa a " +
             "ser o corte de cena. Desmarcado: some junto com o resto.")]
    [SerializeField] private bool musicSurvivesRoomFade = true;

    [Header("Áudio · a oração da mãe")]
    [Tooltip("AudioSource da mãe — em primeiro plano (§4.1). Pode ser filho da cápsula.")]
    [SerializeField] private AudioSource motherVoice;
    [Tooltip("A oração do Pai-Nosso. Vazio: o prólogo segue direto para a espera abaixo.")]
    [SerializeField] private AudioClip prayerClip;

    [Header("Áudio · o baque da porta")]
    [Tooltip("AudioSource posicionado na porta.")]
    [SerializeField] private AudioSource doorAudio;
    [Tooltip("O baque seco (§2.3) — toca no instante exato em que a porta encosta.")]
    [SerializeField] private AudioClip doorSlamClip;

    [Header("Áudio · a briga atrás da parede")]
    [Tooltip("AudioSource posicionado ALÉM da parede/porta. É nele que vai o " +
             "AudioLowPassFilter: a briga NUNCA pode ser inteligível (§4.1, §2.2).")]
    [SerializeField] private AudioSource fightAudio;
    [Tooltip("As vozes alteradas. Vazio: o beat é pulado.")]
    [SerializeField] private AudioClip fightClip;
    [Tooltip("O silêncio entre o baque da porta e o primeiro grito. É ele que faz o " +
             "susto existir — não zere sem pensar (§11.4).")]
    [SerializeField] private float delayBeforeFight = 3f;
    [Tooltip("Fade-in da briga: as vozes sobem, não aparecem do nada.")]
    [SerializeField] private float fightFadeIn = 1f;

    private Quaternion doorOpenRotation;   // pose "aberta" em mundo
    private Vector3 doorOpenPosition;
    private Vector3 hingeWorldPos;
    private Vector3 hingeAxis = Vector3.up;
    private float gapLightBaseIntensity;
    private float fightBaseVolume = 1f;    // volume autorado da briga, alvo do fade-in
    private Coroutine rainFadeRoutine;     // só um fade de chuva por vez
    private Coroutine musicFadeRoutine;    // idem para a música
    private float fightStartTime = -1f;    // Time.time em que as vozes entraram

    /// <summary>True a partir do instante em que a briga começa (§2.3).</summary>
    public bool FightHasStarted => fightStartTime >= 0f;

    /// <summary>
    /// Segundos de briga já escutados — o relógio do prólogo. Retorna -1 antes
    /// de as vozes entrarem. O PrologueEscape só arma a fuga a partir de um
    /// mínimo: Wendy (e o jogador) tem que ficar ali, ouvindo.
    /// </summary>
    public float SecondsSinceFightStarted =>
        FightHasStarted ? Time.time - fightStartTime : -1f;

    private void Awake()
    {
        if (door != null)
        {
            if (doorHinge != null)
            {
                hingeWorldPos = doorHinge.position;
                hingeAxis = doorHinge.up; // permite dobradiça inclinada, se preciso
            }

            // Se a porta foi autorada FECHADA, gira -ângulo para descobrir a pose
            // aberta e já deixa a porta aberta desde o começo (sem flash de frame).
            if (startFromClosedPose)
            {
                if (doorHinge != null)
                    door.RotateAround(hingeWorldPos, hingeAxis, -doorCloseAngle);
                else
                    door.rotation = door.rotation * Quaternion.AngleAxis(-doorCloseAngle, Vector3.up);
            }

            doorOpenRotation = door.rotation;
            doorOpenPosition = door.position;
        }

        if (doorGapLight != null)
            gapLightBaseIntensity = doorGapLight.intensity;

        // A chuva já está caindo quando o jogo abre — ela antecede a cena.
        if (rainLoop != null)
        {
            rainLoop.loop = true;
            rainLoop.volume = rainVolumeStart;
            if (!rainLoop.isPlaying)
                rainLoop.Play();
        }

        // A música entra junto, no mesmo primeiro frame: ela não é um beat, é o
        // chão em que os beats acontecem.
        if (musicLoop != null)
        {
            musicLoop.loop = true;
            musicLoop.volume = musicVolumeStart;
            if (!musicLoop.isPlaying)
                musicLoop.Play();
        }

        // Guarda o volume autorado da briga: é o alvo do fade-in, não 1.
        if (fightAudio != null)
            fightBaseVolume = fightAudio.volume;
    }

    private void Start()
    {
        StartCoroutine(RunPrologue());
    }

    private IEnumerator RunPrologue()
    {
        yield return Prayer();

        yield return new WaitForSeconds(delayBeforeMotherLeaves);

        yield return MotherLeaves();

        yield return new WaitForSeconds(delayBeforeDoorCloses);

        yield return DoorCloses();

        // A chuva sobe DURANTE o silêncio que se segue ao baque — por isso não
        // esperamos o fade terminar aqui. A música acompanha o mesmo movimento.
        FadeRainTo(rainVolumeInTheDark);
        FadeMusicTo(musicVolumeInTheDark);

        yield return Fight();

        // Próximo passo (fechar os olhos → memórias → ilha) entra aqui,
        // encadeado depois de a briga começar.
    }

    // Beat 1 — a mãe recita o Pai-Nosso, em primeiro plano (§4.1).
    private IEnumerator Prayer()
    {
        if (motherVoice == null || prayerClip == null)
            yield break;

        motherVoice.clip = prayerClip;
        motherVoice.Play();

        while (motherVoice.isPlaying)
            yield return null;
    }

    // Beat 2 — a mãe caminha até a porta e sai de cena.
    private IEnumerator MotherLeaves()
    {
        if (mother == null || motherExitPoint == null)
            yield break;

        Vector3 target = motherExitPoint.position;

        while ((mother.position - target).sqrMagnitude > arriveThreshold * arriveThreshold)
        {
            Vector3 next = Vector3.MoveTowards(
                mother.position, target, motherWalkSpeed * Time.deltaTime);

            if (motherFacesMovement)
            {
                Vector3 dir = next - mother.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0000001f)
                    mother.rotation = Quaternion.LookRotation(dir);
            }

            mother.position = next;
            yield return null;
        }

        if (disableMotherOnExit)
            mother.gameObject.SetActive(false);
    }

    // Beat 3 — a porta gira até fechar; o baque seco toca e a fresta de luz apaga junto.
    private IEnumerator DoorCloses()
    {
        if (door == null)
            yield break;

        float elapsed = 0f;
        while (elapsed < doorCloseDuration)
        {
            elapsed += Time.deltaTime;
            float k = doorCloseEase.Evaluate(Mathf.Clamp01(elapsed / doorCloseDuration));

            ApplyDoorAngle(doorCloseAngle * k);

            if (doorGapLight != null)
                doorGapLight.intensity = Mathf.Lerp(gapLightBaseIntensity, 0f, k);

            yield return null;
        }

        ApplyDoorAngle(doorCloseAngle);

        // O baque seco cai no frame em que a porta encosta — nem antes, nem depois.
        if (doorAudio != null && doorSlamClip != null)
            doorAudio.PlayOneShot(doorSlamClip);

        if (doorGapLight != null)
        {
            doorGapLight.intensity = 0f;
            doorGapLight.enabled = false; // breu total
        }
    }

    // Beat 4 — depois do silêncio, as vozes atrás da parede (§2.3).
    private IEnumerator Fight()
    {
        yield return new WaitForSeconds(delayBeforeFight);

        // O relógio da escuta começa aqui, com ou sem clipe: em greybox mudo o
        // prólogo ainda precisa poder terminar.
        fightStartTime = Time.time;

        if (fightAudio == null || fightClip == null)
            yield break;

        // Chuva e música recuam no mesmo movimento em que a briga sobe.
        FadeRainTo(rainVolumeUnderFight);
        FadeMusicTo(musicVolumeUnderFight);

        fightAudio.clip = fightClip;
        fightAudio.volume = 0f;
        fightAudio.Play();

        float elapsed = 0f;
        while (elapsed < fightFadeIn)
        {
            elapsed += Time.deltaTime;
            fightAudio.volume = Mathf.Lerp(0f, fightBaseVolume, elapsed / fightFadeIn);
            yield return null;
        }

        fightAudio.volume = fightBaseVolume;
    }

    /// <summary>
    /// Afunda o quarto no silêncio — chuva, briga e voz da mãe — em 'duration'
    /// segundos. É o que o PrologueEscape chama quando Wendy fecha os olhos
    /// para valer: o mundo real some antes da ilha aparecer (§2.3).
    /// A música só vai junto se 'musicSurvivesRoomFade' estiver desmarcado;
    /// marcado, ela continua tocando por baixo do escuro e do sussurro.
    /// Encerra a sequência do prólogo: depois daqui não há mais beats.
    /// </summary>
    public void FadeEverythingOut(float duration)
    {
        StopAllCoroutines();   // a sequência acabou; nenhum beat pode voltar
        rainFadeRoutine = null;
        musicFadeRoutine = null;
        StartCoroutine(FadeOutAll(duration));
    }

    private IEnumerator FadeOutAll(float duration)
    {
        // A música só entra na lista se ela for do quarto. Sendo da memória, ela
        // fica tocando por baixo do escuro — e é o corte de cena que a encerra.
        AudioSource[] sources = musicSurvivesRoomFade
            ? new[] { rainLoop, fightAudio, motherVoice }
            : new[] { rainLoop, musicLoop, fightAudio, motherVoice };
        float[] from = new float[sources.Length];

        for (int i = 0; i < sources.Length; i++)
            from[i] = sources[i] != null ? sources[i].volume : 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float k = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;

            for (int i = 0; i < sources.Length; i++)
                if (sources[i] != null)
                    sources[i].volume = Mathf.Lerp(from[i], 0f, k);

            yield return null;
        }

        foreach (AudioSource src in sources)
        {
            if (src == null)
                continue;

            src.volume = 0f;
            src.Stop();
        }
    }

    // Troca o volume da chuva sem travar a sequência, cancelando um fade anterior
    // (dois fades simultâneos brigariam pelo mesmo volume).
    private void FadeRainTo(float target)
    {
        if (rainLoop == null)
            return;

        if (rainFadeRoutine != null)
            StopCoroutine(rainFadeRoutine);

        rainFadeRoutine = StartCoroutine(FadeVolume(rainLoop, target, rainFadeDuration));
    }

    // Mesma ideia, para a música: cada uma tem seu próprio fade em curso, para
    // que um beat que mexe só na chuva não interrompa o fade da música.
    private void FadeMusicTo(float target)
    {
        if (musicLoop == null)
            return;

        if (musicFadeRoutine != null)
            StopCoroutine(musicFadeRoutine);

        musicFadeRoutine = StartCoroutine(FadeVolume(musicLoop, target, musicFadeDuration));
    }

    private IEnumerator FadeVolume(AudioSource source, float target, float duration)
    {
        float from = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(from, target, elapsed / duration);
            yield return null;
        }

        source.volume = target;
    }

    // Reposiciona a porta na pose "aberta" e a gira 'angle' graus a partir dela.
    // Com dobradiça: orbita o ponto da dobradiça (arco exato, sem encolher).
    // Sem dobradiça: gira em torno do próprio pivô (fallback).
    private void ApplyDoorAngle(float angle)
    {
        door.rotation = doorOpenRotation;
        door.position = doorOpenPosition;

        if (doorHinge != null)
            door.RotateAround(hingeWorldPos, hingeAxis, angle);
        else
            door.rotation = doorOpenRotation * Quaternion.AngleAxis(angle, Vector3.up);
    }
}
