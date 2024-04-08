using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    public UIDocument uiDocument; // Assign your UIDocument in the Inspector
    private VisualElement tableContainer;

    private void Start()
    {
        var rootVisualElement = uiDocument.rootVisualElement;
        tableContainer = rootVisualElement.Q<VisualElement>("tableContainer");  
    }

    public void GenerateTable()
    {
        // Ensure the container is clear before generating the table
        tableContainer.Clear();

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
                cell.style.borderLeftWidth = cell.style.borderTopWidth = 1;
                cell.style.borderLeftColor = cell.style.borderTopColor = new StyleColor(Color.black);
                cell.style.paddingLeft = cell.style.paddingTop = 5;
                row.Add(cell);
            }
            table.Add(row);
        }

        tableContainer.Add(table);
    }
    public void ClearTable()
    {
        // Clears all children from the table container
        tableContainer.Clear();
    }
   
    public void SetCellContent(int rowIndex, int columnIndex, string content)
    {
        var cellName = $"cell_{rowIndex}_{columnIndex}";
        var cell = tableContainer.Q<Label>(cellName);
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
            Debug.LogError($"Index out of bounds: {index}");
            return;
        }

        int row = index / 20;
        int col = index % 20;
        SetCellContent(row, col, content); // Utilize the existing method for setting content
    }

}
