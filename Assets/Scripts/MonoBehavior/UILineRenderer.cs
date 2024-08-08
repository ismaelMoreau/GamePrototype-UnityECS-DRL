using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UILineRenderer : MaskableGraphic
{
    public List<Vector2> Points;
    public float LineThickness = 2f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (Points == null || Points.Count < 2)
            return;

        float width = LineThickness / 2;

        for (int i = 0; i < Points.Count - 1; i++)
        {
            Vector2 pointA = Points[i];
            Vector2 pointB = Points[i + 1];

            Vector2 normal = new Vector2(pointB.y - pointA.y, pointA.x - pointB.x).normalized * width;
            Vector2 v1 = pointA - normal;
            Vector2 v2 = pointA + normal;
            Vector2 v3 = pointB + normal;
            Vector2 v4 = pointB - normal;

            vh.AddVert(v1, color, new Vector2(0, 0));
            vh.AddVert(v2, color, new Vector2(0, 1));
            vh.AddVert(v3, color, new Vector2(1, 1));
            vh.AddVert(v4, color, new Vector2(1, 0));

            int idx = i * 4;
            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx + 2, idx + 3, idx);
        }
    }
}
