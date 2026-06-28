using UnityEngine;
using UnityEngine.UI;

public class JudgementUI : BaseMeshEffect
{
    public float JudgementType_Float;
    public float NormalizedProgress;
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }

        int vertexCount = vh.currentVertCount;

        float judgement;

        if (MathHelper.IsTwoFloatsEqualWithEpsilion(JudgementType_Float, 0f)) judgement = 0f;
        else if (MathHelper.IsTwoFloatsEqualWithEpsilion(JudgementType_Float, 1f)) judgement = 0.5f;
        else judgement = 1f;
        for (int i = 0; i < vertexCount; i++)
        {
            int index = i;
            UIVertex vertex = UIVertex.simpleVert;
            vh.PopulateUIVertex(ref vertex, index);

            vertex.uv2 = new Vector4(judgement, NormalizedProgress, 0f, 0f);

            vh.SetUIVertex(vertex, index);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        UpdateJudgementMarker();
    }
#endif
    public void UpdateJudgementMarker()
    {
        graphic.SetAllDirty();
    }
}
