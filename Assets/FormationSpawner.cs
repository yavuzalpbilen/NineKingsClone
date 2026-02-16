using System.Collections.Generic;
using UnityEngine;

public static class FormationSpawner
{
    // Spawn merkezinin right/forward yönlerine göre grid üretir (board dönükse bozulmaz)
    public static List<Vector3> MakeGridOriented(
        Vector3 center,
        Vector3 right,
        Vector3 forward,
        int count,
        float spacing,
        float jitter = 0.08f)
    {
        var result = new List<Vector3>(count);
        if (count <= 0) return result;

        // sadece XZ düzleminde çalış
        right.y = 0f;
        forward.y = 0f;
        right = right.sqrMagnitude < 0.0001f ? Vector3.right : right.normalized;
        forward = forward.sqrMagnitude < 0.0001f ? Vector3.forward : forward.normalized;

        int cols = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rows = Mathf.CeilToInt(count / (float)cols);

        float w = (cols - 1) * spacing;
        float h = (rows - 1) * spacing;

        int i = 0;
        for (int r = 0; r < rows && i < count; r++)
        {
            for (int c = 0; c < cols && i < count; c++)
            {
                float ox = (c * spacing) - w * 0.5f;
                float oz = (r * spacing) - h * 0.5f;

                Vector2 j = Random.insideUnitCircle * jitter;

                Vector3 p = center
                            + right * (ox + j.x)
                            + forward * (oz + j.y);

                result.Add(p);
                i++;
            }
        }

        return result;
    }
}
