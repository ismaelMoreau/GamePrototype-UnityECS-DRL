using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using Unity.Entities;
using System.Collections.Generic;


public class UIController : MonoBehaviour
{
    public UIDocument uiDocument; // Assign your UIDocument in the Inspector
    private VisualElement tableContainer;
    private VisualElement tableContentContainer; // Container for table content
    private Button exportButton;
    private Label scoreLabel;
    private World defaultWorld;
    private VisualElement graphContainer;
    private List<float> cumulativeRewards = new List<float>();
    public int maxPoints = 100;
    public float xSpacing = 1.0f;
    public float yScale = 1f;

    void Update()
    {
        if (defaultWorld != null && defaultWorld.IsCreated)
        {
            var scoreEntity = defaultWorld.EntityManager.CreateEntityQuery(typeof(ScoreComponent)).GetSingletonEntity();
            var score = defaultWorld.EntityManager.GetComponentData<ScoreComponent>(scoreEntity).Value;
            scoreLabel.text = $"Score: {score}";
        }
        // // var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        // // var performanceMetricEntity = entityManager.CreateEntityQuery(typeof(PerformanceMetricComponent)).GetSingletonEntity();
        // // var performanceMetric = entityManager.GetComponentData<PerformanceMetricComponent>(performanceMetricEntity);

        // // // Add the latest cumulative reward to the list
        // // cumulativeRewards.Add(performanceMetric.TotalCumulativeReward);

        // // // Maintain the maximum number of points
        // // if (cumulativeRewards.Count > maxPoints)
        // // {
        // //     cumulativeRewards.RemoveAt(0);
        // // }

        // // // Clear previous graph
        // // graphContainer.Clear();

        // // // Calculate the zero line (midpoint of the container height)
        // // float zeroLine = graphContainer.resolvedStyle.height / 2;

        // // // Draw new graph
        // // for (int i = 0; i < cumulativeRewards.Count; i++)
        // // {
        // //     float x = i * xSpacing;
        // //     float y = cumulativeRewards[i] * yScale;

        // //     // Ensure x is within the bounds of the graph container
        // //     x = Mathf.Clamp(x, 0, graphContainer.resolvedStyle.width);

        // //     // Adjust y to be relative to the zero line
        // //     y = zeroLine + y;

        // //     // Create a new visual element for each point
        // //     var point = new VisualElement();
        // //     point.style.width = 2;
        // //     point.style.height = 2;
        // //     point.style.backgroundColor = new StyleColor(Color.green);
        // //     point.style.position = Position.Absolute;
        // //     point.style.left = x;
        // //     point.style.bottom = y;

        // //     // Add the point to the graph container
        // //     graphContainer.Add(point);
        // }
    }
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        exportButton = root.Q<Button>("exportButton");
        exportButton.clicked += OnExportButtonClick;
    }
    void OnDisable()
    {
        exportButton.clicked -= OnExportButtonClick;
    }
    
    private void OnExportButtonClick()
    {
        // Create an entity with the ExportRequest component to trigger the export in the ECS system
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;
        var exportRequestEntity = entityManager.CreateEntity();
        entityManager.AddComponentData(exportRequestEntity,new paramsExportRequestTag());
        entityManager.AddComponentData(exportRequestEntity,new DestroyTag());
        entityManager.SetComponentEnabled<DestroyTag>(exportRequestEntity,false);
        Debug.Log(exportRequestEntity.Index);
        Debug.Log("Export request created");
    }
    private void Start()
    {
        var rootVisualElement = uiDocument.rootVisualElement;
        tableContainer = rootVisualElement.Q<VisualElement>("tableContainer");

        scoreLabel = rootVisualElement.Q<Label>("ScoreLabel");
        defaultWorld = World.DefaultGameObjectInjectionWorld;

        // Create and add the table content container
        tableContentContainer = new VisualElement();
        tableContentContainer.name = "tableContentContainer";
        tableContainer.Add(tableContentContainer);

          // Get the graph container element
        graphContainer = rootVisualElement.Q<VisualElement>("GraphContainer");

        // // Style the graph container (Optional: move this to USS)
        // graphContainer.style.width = new StyleLength(new Length(20, LengthUnit.Percent));
        // graphContainer.style.height = 50;
       // graphContainer.style.backgroundColor = new StyleColor(Color.black);
    }

    public void GenerateTable()
    {
        // Ensure the table content container is clear before generating the table
        tableContentContainer.Clear();

        var table = new VisualElement();
        table.name = "dynamicTable"; // Name the table for easier querying
        table.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);

        for (int i = 0; i < 20; i++)
        {
            var row = new VisualElement();
            row.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            row.style.height = 20;

            for (int j = 0; j < 20; j++)
            {
                var cell = new Label($"{i + 1},{j + 1}");
                cell.name = $"cell_{i}_{j}"; // Naming each cell for easy access
                cell.style.flexGrow = 1;
                //cell.style.borderLeftWidth = cell.style.borderTopWidth = 1;
                cell.style.borderLeftColor = cell.style.borderTopColor = new StyleColor(Color.black);
                cell.style.paddingLeft = cell.style.paddingTop = 5;
                row.Add(cell);
            }
            table.Add(row);
        }

        tableContentContainer.Add(table);
    }

    public void ClearTable()
    {
        // Clears all children from the table content container
        tableContentContainer.Clear();
    }

    public void SetCellContent(int rowIndex, int columnIndex, string content)
    {
        var cellName = $"cell_{rowIndex}_{columnIndex}";
        var cell = tableContentContainer.Q<Label>(cellName);
        if (cell != null)
        {
            cell.text = content;
        }
        else
        {
            Debug.LogError($"Cell at [{rowIndex}, {columnIndex}] not found.");
        }
    }

    public void SetCellContentFlat(int index, string content)
    {
        if (index < 0 || index >= 400) // Ensure the index is within bounds
        {
            Debug.LogError($"UIcontroller Index out of bounds: {index}");
            return;
        }

        int row = index / 20;
        int col = index % 20;
        SetCellContent(row, col, content); // Utilize the existing method for setting content
    }

    public void SetTitle(string titleText)
    {
        // Remove any existing title
        var existingTitle = tableContainer.Q<Label>("tableTitle");
        if (existingTitle != null)
        {
            tableContainer.Remove(existingTitle);
        }

        var title = new Label(titleText);
        title.name = "tableTitle"; // Name the title for easier querying
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.fontSize = 20;
        title.style.marginBottom = 10;
        title.style.unityTextAlign = TextAnchor.MiddleCenter;

        // Insert the title at the top
        tableContainer.Insert(0, title);
    }
}
