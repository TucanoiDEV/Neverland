using UnityEngine;

/// <summary>
/// O sussurro que abre o Dia. Assim que a cena carrega, a voz entra — ela
/// não espera nada nem ninguém: é a primeira coisa que o jogador escuta.
/// </summary>
public class DaySusurro : MonoBehaviour
{
    [Header("Áudio · o sussurro")]
    [Tooltip("AudioSource que carrega o sussurro. Se vazio, tenta pegar um " +
             "AudioSource no próprio objeto.")]
    [SerializeField] private AudioSource susurroAudio;
    [Tooltip("O clipe do sussurro. Vazio: toca o que já estiver no AudioSource.")]
    [SerializeField] private AudioClip susurroClip;

    [Tooltip("Silêncio antes do sussurro entrar (segundos). 0 = toca no mesmo " +
             "instante em que a cena abre.")]
    [SerializeField] private float delayBeforeSusurro = 0f;

    private void Awake()
    {
        // Se ninguém ligou um AudioSource no Inspector, usa o do próprio objeto.
        if (susurroAudio == null)
            susurroAudio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (susurroAudio == null)
        {
            Debug.LogWarning("DaySusurro: nenhum AudioSource atribuído — o sussurro não vai tocar.", this);
            return;
        }

        // A cena abriu: o sussurro entra. Com atraso, se autorado; senão, já.
        susurroAudio.playOnAwake = false;

        if (delayBeforeSusurro > 0f)
            Invoke(nameof(PlaySusurro), delayBeforeSusurro);
        else
            PlaySusurro();
    }

    private void PlaySusurro()
    {
        if (susurroClip != null)
            susurroAudio.PlayOneShot(susurroClip);
        else
            susurroAudio.Play();
    }
}
