using System.Collections;
using UnityEngine;

/// <summary>
/// Orquestra o final do prólogo — O Quarto Real (GDD §2.3, §4.1).
/// Por ora cobre apenas dois beats, em ordem, via coroutine:
///   1. A MÃE SAI — a silhueta (cápsula de greybox) caminha da beira da cama
///      até a porta.
///   2. A PORTA FECHA — gira até fechar e a fresta de luz se apaga, deixando o
///      quarto no breu (§4.1: "a fresta da porta que se apaga quando ela se fecha").
/// Áudio (oração, briga) e o handoff para o BlinkMechanic entram em passos
/// futuros — deixei o gancho comentado no fim da sequência.
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

    private Quaternion doorOpenRotation;   // pose "aberta" em mundo
    private Vector3 doorOpenPosition;
    private Vector3 hingeWorldPos;
    private Vector3 hingeAxis = Vector3.up;
    private float gapLightBaseIntensity;

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
    }

    private void Start()
    {
        StartCoroutine(RunPrologue());
    }

    private IEnumerator RunPrologue()
    {
        yield return new WaitForSeconds(delayBeforeMotherLeaves);

        yield return MotherLeaves();

        yield return new WaitForSeconds(delayBeforeDoorCloses);

        yield return DoorCloses();

        // Próximos passos (áudio da briga, fechar os olhos → memórias → ilha)
        // entram aqui, encadeados após a porta fechar.
    }

    // Beat 1 — a mãe caminha até a porta e sai de cena.
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

    // Beat 2 — a porta gira até fechar; a fresta de luz apaga junto.
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

        if (doorGapLight != null)
        {
            doorGapLight.intensity = 0f;
            doorGapLight.enabled = false; // breu total
        }
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
