using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine.UI;

public class TotalLossGraph : MonoBehaviour
{
    public UILineRenderer uiLineRenderer;
    public RectTransform panelRectTransform;
    public TMP_Text titleText;
    public TMP_Text[] scaleTexts;
    public string title = "Performance Graph";
    public int maxPoints = 100;


    private List<float> totalLoss = new List<float>();

    void Start()
    {
        // Initialize UILineRenderer
        uiLineRenderer.Points = new List<Vector2>();

        // Set the title
        titleText.text = title;
    }

   void Update()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var performanceMetricEntity = entityManager.CreateEntityQuery(typeof(LossMetricComponent)).GetSingletonEntity();
        var performanceMetric = entityManager.GetComponentData<LossMetricComponent>(performanceMetricEntity);

        float rawLoss = performanceMetric.totalLoss;

        // Update the smoothed loss
        float smoothedLoss = rawLoss;
        if (totalLoss.Count > 0)
        {
            smoothedLoss = 0.9f * totalLoss[^1] + 0.1f * rawLoss;
        }

        totalLoss.Add(smoothedLoss);

        if (totalLoss.Count > maxPoints)
        {
            totalLoss.RemoveAt(0);
        }
 

        //Debug.Log($"Graph Range: Max = {maxTotalLoss}, Min = {minTotalLoss}, Panel Height = {panelRectTransform.rect.height}");

        UpdateGraph(uiLineRenderer, totalLoss, panelRectTransform);
        UpdateScaleTexts();
    }
    void UpdateGraph(UILineRenderer lineRenderer, List<float> values, RectTransform panelRect)
    {
        lineRenderer.Points.Clear();

        float maxValue = Mathf.Max(values.ToArray());
        float minValue = Mathf.Min(values.ToArray());

        // Prevent division by zero if all values are the same
        if (Mathf.Approximately(maxValue, minValue))
        {
            maxValue += 1f; // Add a small delta to create a visible range
        }

        float panelWidth = panelRect.rect.width;
        float panelHeight = panelRect.rect.height;

        for (int i = 0; i < values.Count; i++)
        {
            float normalizedX = i / (float)(values.Count - 1);
            float normalizedY = (values[i] - minValue) / (maxValue - minValue);

            float x = panelWidth * normalizedX;
            float y = panelHeight * normalizedY;

            lineRenderer.Points.Add(new Vector2(x, y));
        }

        lineRenderer.SetVerticesDirty();
    }
    void UpdateScaleTexts()
    {
        float maxTotalLoss = Mathf.Max(totalLoss.ToArray());
        float minTotalLoss = Mathf.Min(totalLoss.ToArray());

        scaleTexts[0].text = $"max :{maxTotalLoss.ToString("F1")}";
        scaleTexts[1].text = "0";
        scaleTexts[2].text = $"min :{minTotalLoss.ToString("F1")}";
    }
}
