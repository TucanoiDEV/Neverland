using UnityEngine;

/// <summary>
/// O contador de Lucidez (GDD §5.9).
///
/// Cada fala de LUCIDEZ que o jogador escolhe soma +1; falas de ENTREGA não
/// tiram nada — a Lucidez NUNCA cai, porque perceber a verdade é
/// irreversível. O número não aparece em lugar nenhum da tela: quem conta ao
/// jogador que ele está mais lúcido é a ilha (o tom do Peter Pan, o silêncio
/// da Sininho, §5.9), nunca uma barra.
///
/// É uma classe ESTÁTICA e não um MonoBehaviour de propósito: não existe
/// GameObject "Lucidez" para alguém esquecer de arrastar numa cena, não há
/// Update rodando por trás, e o valor atravessa a troca de cena (Dia → Casa)
/// sem DontDestroyOnLoad. Custo em runtime: um int.
///
/// Persistência: PlayerPrefs por enquanto — é o suficiente para testar o Dia
/// inteiro. Quando o save de verdade existir (§12), só o par
/// Load()/Persist() muda; ninguém mais toca em PlayerPrefs.
///
/// Uso:
///     if (Lucidez.AtLeast(2)) ...        // ramo de diálogo no limiar
///     Lucidez.Add();                     // uma fala [L] foi escolhida
/// </summary>
public static class Lucidez
{
    /// <summary>Chave no PlayerPrefs. Trocar isto zera a Lucidez de quem já jogou.</summary>
    public const string SaveKey = "neverland.lucidez";

    /// <summary>
    /// Disparado sempre que a Lucidez sobe, já com o valor novo. É o gancho
    /// para a encenação do §5.9 (a Sininho que passa a vigiar, o Peter Pan que
    /// endurece) sem que ninguém precise ficar lendo o contador todo frame.
    /// </summary>
    public static event System.Action<int> Changed;

    private static int value;
    private static bool loaded;

    /// <summary>Quanto o Wendy já entendeu. Só leitura — subir é por Add().</summary>
    public static int Value
    {
        get
        {
            if (!loaded)
                Load();

            return value;
        }
    }

    /// <summary>
    /// True quando a Lucidez alcançou o limiar — a condição dos ramos de
    /// diálogo ("com L ≥ 2, Wendy pode dizer 'sinto saudade da minha mãe'").
    /// </summary>
    public static bool AtLeast(int threshold)
    {
        return Value >= threshold;
    }

    /// <summary>
    /// Soma à Lucidez. Valor negativo é ignorado de propósito: a regra do
    /// §5.9 é que ela nunca desce, e um bug de sinal em qualquer chamada não
    /// pode ser capaz de desfazer o que o menino já viu.
    /// </summary>
    public static void Add(int amount = 1)
    {
        if (amount <= 0)
            return;

        if (!loaded)
            Load();

        value += amount;
        Persist();
        Changed?.Invoke(value);
    }

    /// <summary>Volta ao começo — jogo novo, ou teste do Dia do zero.</summary>
    public static void Reset()
    {
        loaded = true;
        value = 0;
        Persist();
        Changed?.Invoke(value);
    }

    /// <summary>
    /// Recarrega do disco quando o jogo entra em Play.
    ///
    /// Existe por causa do editor: com o "Domain Reload" desligado (que é o
    /// que deixa o Play começar rápido), o estado estático SOBREVIVE ao Stop —
    /// sem isto, a Lucidez de um teste vazaria para o teste seguinte e ninguém
    /// entenderia por que o Peter Pan já estava frio na primeira fala.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReloadOnPlay()
    {
        loaded = false;
        Changed = null;
    }

    private static void Load()
    {
        value = PlayerPrefs.GetInt(SaveKey, 0);
        loaded = true;
    }

    private static void Persist()
    {
        PlayerPrefs.SetInt(SaveKey, value);
    }
}
