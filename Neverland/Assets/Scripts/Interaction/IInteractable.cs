using UnityEngine;

/// <summary>
/// Tudo com que o Wendy pode mexer (GDD §5.2).
///
/// A interação tem duas naturezas, e a diferença entre elas é o núcleo do
/// stealth do jogo:
///   · RÁPIDA (tocar E) — abre, puxa, gira na hora. Faz RUÍDO (tabela 6.1:
///     porta ou gaveta rápida = 10 m).
///   · LENTA (segurar E) — a mesma ação, devagar, sem ruído nenhum.
/// Nem tudo aceita a versão lenta: um vidro que se quebra ou uma almofada que
/// se rasga só existem no barulho (§7.3-D). É para isso que serve
/// 'SupportsSlowInteract' — o objeto é quem decide, não o jogador.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Verbo curto, do jeito que uma criança diria: "Abrir", "Pegar",
    /// "Empurrar". O caderninho (§9.1) e o retículo leem daqui.
    /// </summary>
    string Prompt { get; }

    /// <summary>
    /// False esconde o objeto do retículo e recusa a interação — porta ainda
    /// trancada, item já pego. O jogador não deve nem ver o ponto acender.
    /// </summary>
    bool CanInteract { get; }

    /// <summary>
    /// True se segurar E faz sentido aqui (abrir devagar, sem ruído). False:
    /// só existe a versão rápida, e ela custa barulho.
    /// </summary>
    bool SupportsSlowInteract { get; }

    /// <summary>
    /// Executa a interação. 'slow' true = o jogador segurou até o fim.
    /// </summary>
    void Interact(bool slow);

    /// <summary>
    /// O Transform do objeto — para o retículo saber a distância e para o
    /// futuro sistema de ruído saber de ONDE o som saiu.
    /// </summary>
    Transform Transform { get; }
}
