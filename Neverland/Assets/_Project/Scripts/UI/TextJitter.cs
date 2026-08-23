using TMPro;
using UnityEngine;

/// <summary>
/// Tremulação de fita: sacode cada caractere de um texto TMP por frações de
/// pixel, sorteando posições novas algumas dezenas de vezes por segundo
/// (GDD §9.3 — "leve tremor/estática de VHS"; §10.1 — vertex snapping/jitter).
///
/// O tremor é aplicado nos VÉRTICES da malha do texto, não no Transform. Sacudir
/// o objeto inteiro faria a caixa da legenda balançar junto, como um letreiro
/// pendurado; aqui cada letra treme por conta própria, que é o que dá a
/// sensação de fita gasta — a imagem não se move, ela é que está instável.
///
/// Regras de ouro:
///   · A malha é reconstruída pelo TMP a cada mudança de texto e a cada letra
///     revelada pela datilografia. Por isso guardamos uma cópia LIMPA da
///     geometria e trememos sempre a partir dela — nunca do quadro anterior,
///     senão o deslocamento acumularia e o texto sairia voando da caixa.
///   · O tremor é sorteado em DEGRAUS (refreshRate), não interpolado. Um seno
///     suave vira ondulação de RPG; a fita treme aos solavancos.
///   · Só caracteres realmente visíveis tremem — espaços não têm geometria, e
///     o que a datilografia ainda não revelou fica quieto.
///   · Tempo NÃO escalado, como todo o resto do sistema de legendas: uma câmera
///     lenta não pode acalmar a fita.
///
/// Anexar a qualquer objeto com TMP_Text — a fala, o nome de quem fala, um
/// título de menu. Amplitude 0 desliga o efeito sem precisar remover o
/// componente (é por aí que a opção de acessibilidade do §9.4 vai passar).
/// </summary>
[RequireComponent(typeof(TMP_Text))]
[DisallowMultipleComponent]
public class TextJitter : MonoBehaviour
{
    [Header("Tremulação")]
    [Tooltip("Deslocamento máximo de cada caractere, nas unidades locais do texto — " +
             "as mesmas do 'font size'. Pense em porcentagem do corpo da fonte: " +
             "2–5% treme como fita (numa fonte 64, algo entre 1.5 e 3); acima " +
             "disso o texto ferve e fica ilegível. CUIDADO com a escala do pai: " +
             "se ele achata o texto — a caixa da legenda achata —, o tremor é " +
             "achatado junto, e o eixo encolhido precisa de mais amplitude para " +
             "aparecer igual. Zero nos dois eixos = efeito desligado.")]
    [SerializeField] private Vector2 amplitude = new Vector2(1.5f, 1.5f);

    [Tooltip("Quantas vezes por segundo cada caractere sorteia uma posição nova. " +
             "10–15 é a faixa de 'fita ruim'. Muito alto vira borrão (o olho " +
             "perde os degraus); muito baixo vira pulinho ritmado.")]
    [SerializeField] private float refreshRate = 12f;

    private TMP_Text text;

    // A geometria como o TMP a gerou, sem tremor nenhum. É daqui que sai todo
    // quadro — o que está na tela nunca serve de base para o quadro seguinte.
    private TMP_MeshInfo[] pristine;
    private bool hasPristine;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        // O TMP avisa toda vez que reconstrói uma malha — inclusive quando quem
        // reconstrói é outro (mudança de resolução, layout, fonte). Sem escutar
        // isso, a cópia limpa envelheceria e o texto voltaria a um estado antigo.
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnAnyTextChanged);
        hasPristine = false;
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnAnyTextChanged);
        RestorePristine();
    }

    private void OnValidate()
    {
        amplitude.x = Mathf.Max(0f, amplitude.x);
        amplitude.y = Mathf.Max(0f, amplitude.y);
        refreshRate = Mathf.Max(1f, refreshRate);
    }

    private void OnAnyTextChanged(Object changed)
    {
        if (changed == text)
            CachePristine();
    }

    private void LateUpdate()
    {
        if (text == null)
            return;

        // Aqui está o pulo do gato da ordem de execução: o TMP só reconstruiria
        // a malha no fim do frame, DEPOIS deste LateUpdate — e a reconstrução
        // apagaria o tremor que acabamos de aplicar, fazendo o efeito sumir
        // justamente nos frames em que a datilografia revela uma letra nova
        // (ou seja, quase todos). Forçar a atualização agora põe a malha limpa
        // na nossa frente: o TMP escreve, a gente treme por cima, ninguém
        // reescreve depois.
        if (text.havePropertiesChanged || !hasPristine)
        {
            text.ForceMeshUpdate();
            CachePristine();
        }

        TMP_TextInfo info = text.textInfo;
        if (!hasPristine || info == null || info.characterCount == 0)
            return;

        if (amplitude.x <= 0f && amplitude.y <= 0f)
            return;

        // O degrau atual do sorteio. Enquanto ele não vira, todo caractere fica
        // parado no mesmo lugar — é o que faz o tremor ter "quadros", como fita.
        int step = Mathf.FloorToInt(Time.unscaledTime * refreshRate);

        // Não adianta tremer o que a datilografia ainda não revelou: a geometria
        // desses caracteres está zerada e o deslocamento viraria um cisco na tela.
        int visibleLimit = Mathf.Min(info.characterCount, text.maxVisibleCharacters);

        bool touched = false;

        for (int i = 0; i < visibleLimit; i++)
        {
            TMP_CharacterInfo character = info.characterInfo[i];

            // Espaço, tabulação e quebra de linha não têm quatro vértices.
            if (!character.isVisible)
                continue;

            int mesh = character.materialReferenceIndex;
            int vertex = character.vertexIndex;

            if (mesh >= pristine.Length || mesh >= info.meshInfo.Length)
                continue;

            Vector3[] clean = pristine[mesh].vertices;
            Vector3[] live = info.meshInfo[mesh].vertices;

            // Cópia limpa defasada (a malha cresceu neste frame e o evento ainda
            // não chegou): melhor pular este caractere do que ler fora do array.
            if (vertex + 3 >= clean.Length || vertex + 3 >= live.Length)
                continue;

            // Os quatro vértices recebem o MESMO deslocamento: o caractere se
            // move inteiro. Vértice a vértice ele se deformaria, e letra torta
            // não é tremor de fita, é gelatina.
            Vector3 offset = new Vector3(
                Noise(i, step, 0) * amplitude.x,
                Noise(i, step, 1) * amplitude.y,
                0f);

            live[vertex] = clean[vertex] + offset;
            live[vertex + 1] = clean[vertex + 1] + offset;
            live[vertex + 2] = clean[vertex + 2] + offset;
            live[vertex + 3] = clean[vertex + 3] + offset;

            touched = true;
        }

        if (touched)
            PushGeometry();
    }

    private void CachePristine()
    {
        TMP_TextInfo info = text != null ? text.textInfo : null;

        if (info == null || info.meshInfo == null)
        {
            hasPristine = false;
            return;
        }

        pristine = info.CopyMeshInfoVertexData();
        hasPristine = true;
    }

    // Devolve o texto parado. Sem isso, desligar o componente deixaria na tela o
    // último quadro tremido, congelado e torto.
    private void RestorePristine()
    {
        TMP_TextInfo info = text != null ? text.textInfo : null;

        if (!hasPristine || info == null || info.meshInfo == null)
            return;

        int count = Mathf.Min(pristine.Length, info.meshInfo.Length);

        for (int i = 0; i < count; i++)
        {
            Vector3[] clean = pristine[i].vertices;
            Vector3[] live = info.meshInfo[i].vertices;

            if (clean == null || live == null || clean.Length != live.Length)
                continue;

            System.Array.Copy(clean, live, clean.Length);
        }

        PushGeometry();
    }

    private void PushGeometry()
    {
        TMP_TextInfo info = text.textInfo;

        for (int i = 0; i < info.meshInfo.Length; i++)
        {
            Mesh mesh = info.meshInfo[i].mesh;

            if (mesh == null)
                continue;

            mesh.vertices = info.meshInfo[i].vertices;
            text.UpdateGeometry(mesh, i);
        }
    }

    /// <summary>
    /// Ruído de -1 a 1, sorteado a partir do caractere, do degrau de tempo e do
    /// eixo. É uma função pura de propósito: nada de estado por caractere, nada
    /// de array para manter em dia quando a fala muda de tamanho — o mesmo
    /// caractere no mesmo degrau sempre cai no mesmo lugar, e é isso que segura
    /// a letra parada entre um sorteio e outro.
    /// </summary>
    private static float Noise(int character, int step, int axis)
    {
        unchecked
        {
            uint hash = (uint)(character * 73856093) ^
                        (uint)(step * 19349663) ^
                        (uint)(axis * 83492791);

            hash ^= hash >> 13;
            hash *= 1274126177u;
            hash ^= hash >> 16;

            return (hash & 0xFFFFu) / 32767.5f - 1f;
        }
    }
}
