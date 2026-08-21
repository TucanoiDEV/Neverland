using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Interativo de greybox: um IInteractable genérico que dispara UnityEvents
/// (GDD §12.4 — prototipar em greybox ANTES de qualquer arte).
///
/// Serve para pendurar comportamento em um cubo enquanto os itens de verdade
/// (§7.1) não existem: um brinquedo da lista do Peter Pan, uma porta, uma
/// gaveta. Quando o sistema definitivo daquele objeto nascer, ele implementa
/// IInteractable por conta própria e este script sai de cena.
///
/// Anexar a qualquer GameObject COM COLISOR.
/// </summary>
public class SimpleInteractable : MonoBehaviour, IInteractable
{
    [Header("Identidade")]
    [Tooltip("Verbo curto que aparece para o jogador: 'Abrir', 'Pegar'…")]
    [SerializeField] private string prompt = "Usar";
    [Tooltip("Desmarcado: o objeto some do retículo e não responde — porta " +
             "trancada, item já pego.")]
    [SerializeField] private bool canInteract = true;
    [Tooltip("Marcado: segurar E faz a versão lenta e silenciosa. Desmarcado: " +
             "só existe a versão rápida (e barulhenta) — o caso do vidro que " +
             "quebra e da almofada que rasga.")]
    [SerializeField] private bool supportsSlowInteract = true;

    [Header("Uma vez só")]
    [Tooltip("Marcado: depois da primeira interação o objeto se desliga " +
             "sozinho — o caso de pegar um item.")]
    [SerializeField] private bool singleUse = false;

    [Header("O que acontece")]
    [Tooltip("Disparado em QUALQUER interação, rápida ou lenta.")]
    [SerializeField] private UnityEvent onInteract;
    [Tooltip("Só na interação rápida — a barulhenta. É aqui que o ruído de " +
             "10 m (tabela 6.1) vai se pendurar quando o sistema existir.")]
    [SerializeField] private UnityEvent onFastInteract;
    [Tooltip("Só na interação lenta — a silenciosa.")]
    [SerializeField] private UnityEvent onSlowInteract;

    public string Prompt => prompt;
    public bool CanInteract => canInteract;
    public bool SupportsSlowInteract => supportsSlowInteract;
    public Transform Transform => transform;

    public void Interact(bool slow)
    {
        if (!canInteract)
            return;

        if (slow)
            onSlowInteract?.Invoke();
        else
            onFastInteract?.Invoke();

        onInteract?.Invoke();

        if (singleUse)
            canInteract = false;
    }

    /// <summary>
    /// Liga ou desliga o objeto por script — o gancho de "a chave destrancou
    /// esta porta" (§4.3, progressão por fechaduras).
    /// </summary>
    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }
}
