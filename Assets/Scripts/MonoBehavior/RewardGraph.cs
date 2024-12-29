using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine.UI;

public class RewardGraph : MonoBehaviour
{
    public UILineRenderer uiLineRenderer;
    public RectTransform panelRectTransform;
    public TMP_Text titleText;
    public TMP_Text[] scaleTexts;
    public string title = "Performance Graph";
    public int maxPoints = 100;

    private List<float> cumulativeRewards = new List<float>();

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
        var performanceMetricEntity = entityManager.CreateEntityQuery(typeof(RewardMetricComponent)).GetSingletonEntity();
        var performanceMetric = entityManager.GetComponentData<RewardMetricComponent>(performanceMetricEntity);

        // Add the latest cumulative reward to the list
        float smoothedReward = 0.9f * (cumulativeRewards.Count > 0 ? cumulativeRewards[^1] : 0) + 0.1f * performanceMetric.TotalCumulativeReward;
        cumulativeRewards.Add(smoothedReward);

        // Maintain the maximum number of points
        if (cumulativeRewards.Count > maxPoints)
        {
            cumulativeRewards.RemoveAt(0);
        }

        // // Update the UILineRenderer with the new points
        UpdateGraph(uiLineRenderer, cumulativeRewards, panelRectTransform);
        // uiLineRenderer.Points.Clear();
       

        // float maxCumulativeReward = Mathf.Max(cumulativeRewards.ToArray());
        // float minCumulativeReward = Mathf.Min(cumulativeRewards.ToArray());
      
        // for (int i = 0; i < cumulativeRewards.Count; i++)
        // {
        //     float x = i * xSpacing;
        //     float y = cumulativeRewards[i] * yScale;
            
        //     uiLineRenderer.Points.Add(new Vector2(x, y));
        // }
        // uiLineRenderer.SetVerticesDirty();

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
        float maxCumulativeReward = Mathf.Max(cumulativeRewards.ToArray());
        float minCumulativeReward = Mathf.Min(cumulativeRewards.ToArray());
      
        scaleTexts[0].text = $"max :{maxCumulativeReward.ToString("F1")}";
        
        scaleTexts[1].text = "0";
        
        scaleTexts[2].text = $"min :{minCumulativeReward.ToString("F1")}";
        
    }


}
