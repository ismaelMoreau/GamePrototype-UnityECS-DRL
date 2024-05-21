using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    public UIDocument uiDocument; // Assign your UIDocument in the Inspector
    private VisualElement tableContainer;
    private VisualElement tableContentContainer; // Container for table content

    private void Start()
    {
        var rootVisualElement = uiDocument.rootVisualElement;
        tableContainer = rootVisualElement.Q<VisualElement>("tableContainer");

        // Create and add the table content container
        tableContentContainer = new VisualElement();
        tableContentContainer.name = "tableContentContainer";
        tableContainer.Add(tableContentContainer);
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
                cell.style.borderLeftWidth = cell.style.borderTopWidth = 1;
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
