using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;
using Unity.Transforms;
using System.IO;
using System.Resources;
using Unity.Collections;
using Unity.VisualScripting;
using System;
using System.Reflection;
using System.Collections.Generic;


[UpdateAfter(typeof(TransformSystemGroup))]
[CreateAfter(typeof(TransformSystemGroup))]
public partial struct DebugToolSystem : ISystem
{


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GamePlayingTag>();
        // state.RequireForUpdate<PlayerMovementComponent>();
        // state.RequireForUpdate<QtableComponent>();
        // state.RequireForUpdate<ConfigQlearnGrid>();
        // initialized = false;

    }

    public enum debugEnemyActionEnum
    {
        forward = 1,
        backward = 2,
        stepRight = 3,
        stepLeft = 4,
        dash = 5
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (request, entity) in SystemAPI.Query<RefRO<paramsExportRequestTag>>().WithEntityAccess())
        {
            Debug.Log("in export request");
            ExportParameters();

            // Remove the ExportRequest component to reset the state
            state.EntityManager.SetComponentEnabled<DestroyTag>(entity,true);
        }
        // foreach (var (neuralNetworkParameters,targetNetworkParameters) in SystemAPI.Query<RefRO<NeuralNetworkParametersComponent>,RefRO<TargetNeuralNetworkParametersComponent>>())
        // {
        //     float[] array = neuralNetworkParameters.ValueRO.inputWeights.ToArray();
        //     float[] array2 = targetNetworkParameters.ValueRO.inputWeights.ToArray();

           
        //     // Log the array
        //     Debug.Log("NN: " + string.Join(", ", array));
        //     Debug.Log("targetNN: " + string.Join(", ", array2));
        // }
        // var config = SystemAPI.GetSingleton<ConfigQlearnGrid>();
        // var configEntity = SystemAPI.GetSingletonEntity<ConfigQlearnGrid>();
        // var configManaged = state.EntityManager.GetComponentObject<ConfigManaged>(configEntity);
        // bool shouldBreak = false;
        // foreach (debugEnemyActionEnum action in System.Enum.GetValues(typeof(debugEnemyActionEnum)))
        // {

        //     if (Input.GetKey(KeyCode.Alpha0 + (int)action))
        //     {
        //         if (initialized) { configManaged.UIController.ClearTable(); }

        //         HandleEnemyAction(action, configManaged, ref state);
        //         shouldBreak = true;
        //         break;
        //         // Only handle one action per frame
        //     }
        //     if (shouldBreak) { break; }


        // }
        // // if (Input.GetKey(KeyCode.Alpha2))//((int)action - 1)))
        // //     {
        // //         HandleEnemyAction(debugEnemyActionEnum.forward,configManaged,ref state);

        // //     }
        // // if (Input.GetKey(KeyCode.A)) {


        // //     if (!initialized)
        // //     {
        // //         initialized = true;


        // //         configManaged.UIController = GameObject.FindObjectOfType<UIController>();
        // //         configManaged.UIController.GenerateTable();
        // //         int index = 0;
        // //         foreach(var Qtable in SystemAPI.Query<RefRO<QtableComponent>>()){
        // //             configManaged.UIController.SetCellContentFlat(index,index.ToString()+": "+ Qtable.ValueRO.forward.ToString("F2")+"/");
        // //             // Qtable.ValueRO.down.ToString("F2")+"/"+
        // //             // Qtable.ValueRO.right.ToString("F2")+"/"+
        // //             // Qtable.ValueRO.left.ToString("F2"));
        // //             index++;
        // //         }
        // //     }
        // // }
        // if (Input.GetKey(KeyCode.P))
        // {

        //     if (!initialized)
        //     {
        //         initialized = true;


        //         configManaged.UIController = GameObject.FindObjectOfType<UIController>();
        //         configManaged.UIController.GenerateTable();
        //         int index = 0;
        //         foreach (var Qtable in SystemAPI.Query<RefRO<QtableRewardComponent>>())
        //         {
        //             configManaged.UIController.SetCellContentFlat(index, Qtable.ValueRO.reward.ToString());
        //             index++;
        //         }
        //     }

        // }
        // else if (Input.GetKey(KeyCode.D))
        // {

        //     float3 centerPosition = new float3(0, 0, 0); // Default center position
        //     foreach ((var transforms, var playerMovementComponent) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<PlayerMovementComponent>>())
        //     {
        //         centerPosition = transforms.ValueRO.Position;
        //     }

        //     // Use config values for grid size, spacing, and center position setup
        //     int gridSizeX = config.width;
        //     int gridSizeZ = config.height;
        //     float cellSize = config.cellSize; // Assuming each cell in the grid is defined by cellSize units
        //     float3 bottomLeftPosition = centerPosition - new float3(gridSizeX * cellSize / 2, 0, gridSizeZ * cellSize / 2);
        //     float y = 0.1f; // To ensure visibility above ground

        //     // Line visibility duration
        //     float lineDuration = SystemAPI.Time.DeltaTime; // Adjust as needed

        //     // Drawing the grid, ensuring to include the closing lines
        //     for (int x = 0; x <= gridSizeX; x++)
        //     {
        //         for (int z = 0; z <= gridSizeZ; z++)
        //         {
        //             // Vertical lines
        //             Debug.DrawLine(
        //                 new float3(x * cellSize, y, 0) + bottomLeftPosition,
        //                 new float3(x * cellSize, y, gridSizeZ * cellSize) + bottomLeftPosition,
        //                 Color.white, lineDuration);

        //             // Horizontal lines
        //             if (z < gridSizeZ) // To avoid drawing an extra line at the end
        //             {
        //                 Debug.DrawLine(
        //                     new float3(0, y, z * cellSize) + bottomLeftPosition,
        //                     new float3(gridSizeX * cellSize, y, z * cellSize) + bottomLeftPosition,
        //                     Color.white, lineDuration);
        //             }
        //         }
        //     }

        //     float3 gridOrigin = centerPosition - new float3(gridSizeX / 2 * cellSize, 0, gridSizeZ / 2 * cellSize); // Adjust grid origin based on player position

        //     foreach ((RefRO<LocalToWorld> localTransform, RefRW<EnemyMovementComponent> enemy, Entity entity)
        //                 in SystemAPI.Query<RefRO<LocalToWorld>, RefRW<EnemyMovementComponent>>().WithEntityAccess())
        //     {
        //         float3 enemyPosition = localTransform.ValueRO.Position - gridOrigin; // Relative position to the moving grid origin

        //         // Calculate the grid cell index
        //         float2 gridPos = new float2(math.floor(enemyPosition.x / cellSize), math.floor(enemyPosition.z / cellSize));

        //         // Ensure the enemy is within the grid bounds
        //         if (gridPos.x >= 0 && gridPos.x < gridSizeX && gridPos.y >= 0 && gridPos.y < gridSizeZ)
        //         {
        //             // Calculate the world position of the grid cell's bottom-left corner
        //             float3 cellBottomLeft = gridOrigin + new float3(gridPos.x * cellSize, 0, gridPos.y * cellSize);

        //             // Calculate other corners based on the bottom-left
        //             float3 cellTopLeft = cellBottomLeft + new float3(0, 0, cellSize);
        //             float3 cellTopRight = cellBottomLeft + new float3(cellSize, 0, cellSize);
        //             float3 cellBottomRight = cellBottomLeft + new float3(cellSize, 0, 0);

        //             // Draw the highlighted square
        //             Color highlightColor = Color.red; // Color for highlighting
        //             Debug.DrawLine(cellBottomLeft, cellTopLeft, highlightColor, lineDuration);
        //             Debug.DrawLine(cellTopLeft, cellTopRight, highlightColor, lineDuration);
        //             Debug.DrawLine(cellTopRight, cellBottomRight, highlightColor, lineDuration);
        //             Debug.DrawLine(cellBottomRight, cellBottomLeft, highlightColor, lineDuration);
        //         }
        //     }
        // }
        // // else{
        // //     if(initialized){
        // if (Input.GetKey(KeyCode.Delete))
        // {
        //     configManaged.UIController.ClearTable();
        //     initialized = false;
        // }
        // // }

    
    }
    // private void HandleEnemyAction(debugEnemyActionEnum action, ConfigManaged configManaged, ref SystemState state)
    // {
    //     initialized = true;
    //     configManaged.UIController = GameObject.FindObjectOfType<UIController>();
    //     configManaged.UIController.SetTitle(action.ToString());
    //     configManaged.UIController.GenerateTable();
    //     int index = 0;
    //     foreach (var Qtable in SystemAPI.Query<RefRO<QtableComponent>>())
    //     {
    //         string content = GetActionContent(action, Qtable.ValueRO);

    //         configManaged.UIController.SetCellContentFlat(index, index.ToString() + ": " + content);
    //         index++;
    //     }


    // }

    // private string GetActionContent(debugEnemyActionEnum action, QtableComponent qTable)
    // {
    //     // // Get the name of the enum value
    //     // string actionName = Enum.GetName(typeof(debugEnemyActionEnum), action);

    //     // // Use reflection to dynamically generate content based on the enum value name
    //     // PropertyInfo property = typeof(QtableComponent).GetProperty(actionName.ToLower());
    //     // if (property != null)
    //     // {
    //     //     float value = (float)property.GetValue(qTable, null);
    //     //     float indexQtables = (float)property.GetValue(index, null);
    //     //     return value.ToString("F2") + "/";
    //     // }
    //     // else
    //     // {
    //     //     // Handle the case where the property corresponding to the enum value doesn't exist
    //     //     return "Content not available";
    //     // }
    //     string value = "";
    //     switch (action)
    //     {
    //         case debugEnemyActionEnum.forward:
    //             value = qTable.forward.ToString("F2") + "/";
    //             break;
    //         case debugEnemyActionEnum.backward:
    //             value = qTable.backward.ToString("F2") + "/";
    //             break;
    //         case debugEnemyActionEnum.stepRight:
    //             value = qTable.stepRight.ToString("F2") + "/";
    //             break;
    //         case debugEnemyActionEnum.stepLeft:
    //             value = qTable.stepLeft.ToString("F2") + "/";
    //             break;
    //         case debugEnemyActionEnum.dash:
    //             value = qTable.dash.ToString("F2") + "/";
    //             break;
    //         default:
    //             value = "";
    //             break;
    //     }
    //     return value;//+ "  i="+qTable.indexOfQtableComponent;
    // }
    public void ExportParameters()
    {

        // Get the neural network parameters component
        var targetNN = SystemAPI.GetSingletonEntity<TargetNeuralNetworkParametersComponent>();
        var neuralNetworkParameters = SystemAPI.GetSingleton<NeuralNetworkParametersComponent>();
        var neuralNetwork = SystemAPI.GetSingleton<NeuralNetworkComponent>();
        var filePath = Path.Combine(Application.persistentDataPath, "parameters.json");
        var targetNNfilePath = Path.Combine(Application.persistentDataPath, "targetNNparameters.json");

        ParameterExporter.ExportParameters(neuralNetworkParameters,neuralNetwork, filePath);
        //ParameterExporter.ExportParameters(neuralNetworkParameters,neuralNetwork, targetNNfilePath);

        Debug.Log("Parameters exported to " + filePath);
    }
   
}
