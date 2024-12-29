using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Entities;
using UnityEngine.UI;

public class LossGraph : MonoBehaviour
{
    public UILineRenderer lossLineRenderer;
    public RectTransform panelRectTransform;
    public TMP_Text titleText;
    public TMP_Text[] scaleTexts;
    public string title = "Loss Graph";
 

    private List<float> keyLossValues = new List<float>();
    private int bucketSize = 100; // Adjustable size for bucketing
    private int frameCounter = 0;
    private float aggregatedLoss = 0;
    private float emaLoss = 0;
    private float smoothingFactor = 0.1f; // Adjustable for EMA

    void Start()
    {
        // Initialize UILineRenderer
        lossLineRenderer.Points = new List<Vector2>();

        // Set the title
        titleText.text = title;
    }

    void Update()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Query LossMetricComponent
        var lossMetricEntity = entityManager.CreateEntityQuery(typeof(LossMetricComponent)).GetSingletonEntity();
        var lossMetric = entityManager.GetComponentData<LossMetricComponent>(lossMetricEntity);

        // Update EMA
        emaLoss = smoothingFactor * lossMetric.totalLoss + (1 - smoothingFactor) * emaLoss;

        // Add to current bucket
        aggregatedLoss += lossMetric.totalLoss;
        frameCounter++;

        // At the end of each bucket, store aggregated data
        if (frameCounter >= bucketSize)
        {
            float averageLoss = aggregatedLoss / frameCounter;

            // Store key points
            if (keyLossValues.Count == 0 || averageLoss < keyLossValues[^1]) // Track minimum
            {
                keyLossValues.Add(averageLoss);
            }

            // Always add EMA as a key point
            keyLossValues.Add(emaLoss);

            // Reset bucket
            frameCounter = 0;
            aggregatedLoss = 0;
        }

        // Update Graph with keyLossValues
        UpdateGraph(lossLineRenderer, keyLossValues, panelRectTransform);

        // Update scale texts
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
        float maxLoss = Mathf.Max(keyLossValues.ToArray());
        float minLoss = Mathf.Min(keyLossValues.ToArray());

        scaleTexts[0].text = $"Loss Max: {maxLoss:F1}";
        scaleTexts[1].text = $"Loss Min: {minLoss:F1}";
    }
}
