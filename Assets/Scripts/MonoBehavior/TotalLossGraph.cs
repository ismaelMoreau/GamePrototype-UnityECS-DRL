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
    public float xSpacing = 10.0f;
    public float yScale = 1f;

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
        var performanceMetricEntity = entityManager.CreateEntityQuery(typeof(PerformanceMetricComponent)).GetSingletonEntity();
        var performanceMetric = entityManager.GetComponentData<PerformanceMetricComponent>(performanceMetricEntity);

        // Add the latest cumulative reward to the list
        if(performanceMetric.totalLoss != 0){
            totalLoss.Add(performanceMetric.totalLoss);
        }

        // Maintain the maximum number of points
        if (totalLoss.Count > maxPoints)
        {
            totalLoss.RemoveAt(0);
        }

        // Update the UILineRenderer with the new points
        uiLineRenderer.Points.Clear();
       

        float maxtotalLoss = Mathf.Max(totalLoss.ToArray());
        float mintotalLoss = Mathf.Min(totalLoss.ToArray());
      
        for (int i = 0; i < totalLoss.Count; i++)
        {
            float x = i * xSpacing;
            float y = totalLoss[i] * yScale;
            
            uiLineRenderer.Points.Add(new Vector2(x, y));
        }
        uiLineRenderer.SetVerticesDirty();

        // Update scale texts
        UpdateScaleTexts(maxtotalLoss,mintotalLoss);

        
    }

    void UpdateScaleTexts(float maxValue, float minValue)
    {
      
        scaleTexts[0].text = $"max :{maxValue.ToString("F1")}";
        
        scaleTexts[1].text = "0";
        
        scaleTexts[2].text = $"min :{minValue.ToString("F1")}";
        
    }


}
