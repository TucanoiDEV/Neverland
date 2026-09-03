using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A Sininho levando o Wendy pela trilha (GDD §2.3, §4.2).
///
/// Terminada a primeira conversa, ela vira e sai andando na frente: "vem, tem
/// tanta coisa que eu quero te mostrar". O passeio até a Roda das Crianças é o
/// tutorial diegético do jogo — é andando atrás dela que o menino aprende o
/// tamanho da ilha, e é ela que decide o ritmo.
///
/// **A coleira.** Ela nunca some de vista. Se o Wendy ficar mais de
/// 'waitDistance' atrás, ela PARA e fica pairando, virada para ele, até que
/// ele chegue a 'resumeDistance' — e só então retoma. As duas distâncias são
/// diferentes de propósito: com uma só, o menino andando exatamente no limite
/// faria a fada piscar entre andar e parar a cada passo. É a "guia que chama
/// de volta" do §4.2, sem parede invisível nenhuma: o limite do mundo é ela.
///
/// A coleira só conta quando o menino está ATRÁS dela. Se ele correr na
/// frente, ela continua andando em vez de congelar esperando alguém que já
/// passou — quem manda no passeio é ela, mas ela não é boba.
///
/// **Ela não navega.** Não há NavMesh, não há física, não há pathfinding: este
/// script empurra o ponto em torno do qual o [[FairyIdle]] faz ela pairar, de
/// um waypoint para o outro, e o resto (o voo, o balanço, as asas, o giro do
/// rosto) continua sendo do FairyIdle. Custo: um Update que só existe entre o
/// fim da conversa e a chegada — antes disso e depois dela o componente se
/// desliga sozinho, e a Unity nem chama o Update.
///
/// Anexar ao MESMO GameObject do FairyIdle, e ligar o
/// 'onConversationFinished' do NpcConversation dela em Begin().
/// </summary>
[RequireComponent(typeof(FairyIdle))]
[DisallowMultipleComponent]
public class FairyGuide : MonoBehaviour
{
    [Header("A rota")]
    [Tooltip("Os pontos por onde ela passa, na ordem. O último é onde ela " +
             "para — perto do Peter Pan, na Roda das Crianças.")]
    [SerializeField] private Transform[] waypoints;

    [Tooltip("Velocidade dela (m/s). O Wendy anda a 2,2 e corre a 3,6: " +
             "mantenha ABAIXO de 2,2, ou seguir a fada vira uma corrida.")]
    [SerializeField] private float speed = 1.7f;

    [Tooltip("A que distância do waypoint ela o considera alcançado (m).")]
    [SerializeField] private float arriveRadius = 0.3f;

    [Header("A coleira (§4.2)")]
    [Tooltip("Se o Wendy ficar mais longe que isto, ela para e espera (m).")]
    [SerializeField] private float waitDistance = 4f;

    [Tooltip("E só volta a andar quando ele chegar a esta distância (m). " +
             "Tem que ser MENOR que a de cima — é a folga que impede a fada " +
             "de tremer entre andar e parar no limite exato.")]
    [SerializeField] private float resumeDistance = 3f;

    [Header("Chamar de volta")]
    [Tooltip("O que ela diz enquanto espera, em rodízio. Vazio: ela espera " +
             "calada — o que também funciona, e é mais assustador.")]
    [TextArea(1, 2)][SerializeField] private string[] callLines;

    [Tooltip("Segundos entre uma chamada e a seguinte.")]
    [SerializeField] private float callEverySeconds = 7f;

    [Tooltip("Nome dela na legenda. Tem que bater com o do NpcConversation.")]
    [SerializeField] private string speaker = "Sininho";

    [Header("Referências")]
    [Tooltip("O corpo do Wendy, para medir a distância. Vazio: procura o " +
             "PlayerMotor na cena.")]
    [SerializeField] private Transform player;

    [Tooltip("A conversa dela. Enquanto ela estiver falando o passeio para " +
             "— ninguém guia e conversa ao mesmo tempo. Vazio: procura no " +
             "próprio objeto.")]
    [SerializeField] private NpcConversation conversation;

    [Header("Ganchos")]
    [Tooltip("Disparado no instante em que ela vira e sai andando.")]
    [SerializeField] private UnityEvent onDeparted;

    [Tooltip("Disparado quando ela chega ao último ponto. É por aqui que o " +
             "Peter Pan entra (§2.3).")]
    [SerializeField] private UnityEvent onArrived;

    private FairyIdle idle;

    private int index;             // o waypoint que ela persegue agora
    private bool guiding;
    private bool arrived;
    private bool waiting;          // parada, esperando o menino
    private float callTimer;
    private int nextCall;

    /// <summary>True enquanto ela está levando o menino pela trilha.</summary>
    public bool IsGuiding => guiding;

    /// <summary>True depois que ela chegou ao fim da rota.</summary>
    public bool HasArrived => arrived;

    /// <summary>True enquanto ela está parada esperando o Wendy alcançar.</summary>
    public bool IsWaiting => waiting;

    private void Awake()
    {
        idle = GetComponent<FairyIdle>();

        if (conversation == null)
            conversation = GetComponent<NpcConversation>();

        if (player == null)
        {
            PlayerMotor motor = FindObjectOfType<PlayerMotor>();

            if (motor != null)
                player = motor.transform;
        }

        // Fora do passeio este componente não custa nada: desligado, a Unity
        // não chama o Update dele.
        enabled = false;
    }

    /// <summary>
    /// "Vem, tem tanta coisa que eu quero te mostrar." Ligue isto no
    /// 'onConversationFinished' do NpcConversation dela. Chamar de novo com o
    /// passeio em andamento (o jogador falou com ela no meio do caminho) não
    /// faz nada — ela não recomeça a rota.
    /// </summary>
    public void Begin()
    {
        if (guiding || arrived || waypoints == null || waypoints.Length == 0)
            return;

        guiding = true;
        waiting = false;
        index = 0;
        enabled = true;

        onDeparted?.Invoke();
    }

    /// <summary>
    /// Para o passeio onde ele está — para as cutscenes e para o momento em
    /// que a ilha tira o passo do menino.
    /// </summary>
    public void Halt()
    {
        guiding = false;
        enabled = false;
        idle.FaceThePlayer();
    }

    private void Update()
    {
        if (!guiding)
            return;

        // Ninguém guia e conversa ao mesmo tempo: se o Wendy a chamou no meio
        // do caminho, ela para de andar até terminar de responder.
        if (conversation != null && conversation.IsRunning)
        {
            idle.FaceThePlayer();
            return;
        }

        Transform point = NextPoint();

        if (point == null)
        {
            Arrive();
            return;
        }

        Vector3 anchor = idle.Anchor;
        Vector3 toPoint = point.position - anchor;

        // A coleira. 'ahead' é para onde ela vai; se o menino está do lado
        // contrário, ele ficou para trás — e é só nesse caso que faz sentido
        // ela esperar.
        if (player != null)
        {
            Vector3 toPlayer = player.position - anchor;
            toPlayer.y = 0f;

            Vector3 ahead = toPoint;
            ahead.y = 0f;

            float gap = toPlayer.magnitude;
            bool behind = Vector3.Dot(toPlayer, ahead) <= 0f;

            if (waiting)
            {
                if (gap <= resumeDistance)
                    waiting = false;
            }
            else if (gap > waitDistance && behind)
            {
                waiting = true;
                // A primeira chamada não sai no mesmo instante em que ela
                // para: ela olha para trás, espera um pouco, e só então chama.
                callTimer = 0f;
            }
        }

        if (waiting)
        {
            idle.FaceThePlayer();
            CallTheBoy();
            return;
        }

        // Andar é só empurrar o ponto em torno do qual ela paira; o voo, o
        // balanço e as asas continuam sendo do FairyIdle.
        anchor = Vector3.MoveTowards(anchor, point.position, speed * Time.deltaTime);
        idle.Anchor = anchor;

        Vector3 look = toPoint;
        look.y = 0f;

        if (look.sqrMagnitude > 0.0001f)
            idle.FaceDirection(look);

        if ((point.position - anchor).sqrMagnitude <= arriveRadius * arriveRadius)
            index++;
    }

    /// <summary>O próximo ponto válido da rota, ou null se a rota acabou.</summary>
    private Transform NextPoint()
    {
        while (index < waypoints.Length && waypoints[index] == null)
            index++;

        return index < waypoints.Length ? waypoints[index] : null;
    }

    private void Arrive()
    {
        guiding = false;
        arrived = true;
        waiting = false;
        enabled = false;

        idle.FaceThePlayer();
        onArrived?.Invoke();
    }

    /// <summary>"Vem, bobinho!" — a fala que ela repete enquanto espera.</summary>
    private void CallTheBoy()
    {
        if (callLines == null || callLines.Length == 0 || callEverySeconds <= 0f)
            return;

        callTimer += Time.deltaTime;

        if (callTimer < callEverySeconds)
            return;

        callTimer = 0f;

        Subtitles subs = Subtitles.Instance;

        // Se já há legenda na tela, ela espera a próxima volta em vez de
        // empilhar frase em cima de frase.
        if (subs == null || !subs.Enabled || subs.IsShowing)
            return;

        subs.Show(speaker, callLines[nextCall]);
        nextCall = (nextCall + 1) % callLines.Length;
    }

#if UNITY_EDITOR
    // A rota desenhada na cena: sem isto, editar um passeio de 90 metros feito
    // de objetos vazios seria adivinhação.
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Gizmos.color = new Color(1f, 0.72f, 0.3f, 0.85f);

        Vector3 previous = transform.position;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
                continue;

            Vector3 here = waypoints[i].position;

            Gizmos.DrawLine(previous, here);
            Gizmos.DrawWireSphere(here, arriveRadius);

            previous = here;
        }
    }
#endif
}
