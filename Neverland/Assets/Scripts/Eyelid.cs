using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Desenha uma "pálpebra": uma faixa preta que cobre a partir de uma borda da
/// tela (topo ou base) até uma linha CURVA. A curvatura só aparece durante a
/// piscada (some quando os olhos estão totalmente abertos ou fechados), dando o
/// formato de amêndoa de um olho de verdade.
///
/// Coloque este componente num objeto de UI esticado (stretch) cobrindo a tela
/// inteira. A cor da forma é a cor deste Graphic (deixe preta no Inspector).
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class Eyelid : Graphic
{
    [Tooltip("Marque para a pálpebra de CIMA; desmarque para a de BAIXO.")]
    public bool isTop = true;

    [Tooltip("0 = olhos abertos, 1 = totalmente fechados.")]
    [Range(0f, 1f)] public float closeAmount = 0f;

    [Tooltip("Quanto a borda se curva (0 = reta, maior = mais arredondada).")]
    [Range(0f, 0.5f)] public float curveDepth = 0.14f;

    [Tooltip("Suavidade da curva (mais segmentos = curva mais lisa).")]
    [Min(2)] public int segments = 32;

    /// <summary>Atualiza o quanto o olho está fechado e redesenha.</summary>
    public void SetClose(float amount)
    {
        closeAmount = Mathf.Clamp01(amount);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect r = GetPixelAdjustedRect();
        float t = closeAmount;

        // A curvatura nasce e some ao longo da piscada (máxima na metade).
        float curve = curveDepth * r.height * (t * (1f - t));

        // Borda externa fixa (topo ou base) e a linha reta-base da borda interna.
        float outerY = isTop ? r.yMax : r.yMin;
        float straightY = isTop
            ? Mathf.Lerp(r.yMax, r.center.y, t)   // desce do topo até o centro
            : Mathf.Lerp(r.yMin, r.center.y, t);  // sobe da base até o centro

        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;

        // Gera pares de vértices (externo + interno) ao longo da largura.
        for (int i = 0; i <= segments; i++)
        {
            float fx = (float)i / segments;                 // 0..1 na largura
            float x = Mathf.Lerp(r.xMin, r.xMax, fx);
            float gx = 1f - (2f * fx - 1f) * (2f * fx - 1f); // 1 no centro, 0 nas pontas

            // No centro a pálpebra recua (deixa o olho mais aberto no meio).
            float edgeY = isTop ? straightY + curve * gx
                                : straightY - curve * gx;

            vert.position = new Vector3(x, outerY);
            vh.AddVert(vert);
            vert.position = new Vector3(x, edgeY);
            vh.AddVert(vert);
        }

        // Liga os pares formando a faixa preenchida.
        for (int i = 0; i < segments; i++)
        {
            int o0 = i * 2;
            int in0 = o0 + 1;
            int o1 = o0 + 2;
            int in1 = o0 + 3;

            vh.AddTriangle(o0, in0, in1);
            vh.AddTriangle(o0, in1, o1);
        }
    }
}
