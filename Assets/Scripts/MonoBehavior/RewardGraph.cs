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
    public float xSpacing = 10.0f;
    public float yScale = 1f;

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
        var performanceMetricEntity = entityManager.CreateEntityQuery(typeof(PerformanceMetricComponent)).GetSingletonEntity();
        var performanceMetric = entityManager.GetComponentData<PerformanceMetricComponent>(performanceMetricEntity);

        // Add the latest cumulative reward to the list
        cumulativeRewards.Add(performanceMetric.TotalCumulativeReward);

        // Maintain the maximum number of points
        if (cumulativeRewards.Count > maxPoints)
        {
            cumulativeRewards.RemoveAt(0);
        }

        // Update the UILineRenderer with the new points
        uiLineRenderer.Points.Clear();
       

        float maxCumulativeReward = Mathf.Max(cumulativeRewards.ToArray());
        float minCumulativeReward = Mathf.Min(cumulativeRewards.ToArray());
      
        for (int i = 0; i < cumulativeRewards.Count; i++)
        {
            float x = i * xSpacing;
            float y = cumulativeRewards[i] * yScale;
            
            uiLineRenderer.Points.Add(new Vector2(x, y));
        }
        uiLineRenderer.SetVerticesDirty();

        // Update scale texts
        UpdateScaleTexts(maxCumulativeReward,minCumulativeReward);

        
    }
    void UpdateScaleTexts(float maxValue, float minValue)
    {
      
        scaleTexts[0].text = $"max :{maxValue.ToString("F1")}";
        
        scaleTexts[1].text = "0";
        
        scaleTexts[2].text = $"min :{minValue.ToString("F1")}";
        
    }


}
