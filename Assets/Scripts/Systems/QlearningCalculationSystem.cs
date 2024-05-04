using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Random = Unity.Mathematics.Random;

[UpdateAfter(typeof(QlearningActionSelectionSystem))]
public partial struct QlearningCalculationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ConfigQlearn>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var qlearnConfig = SystemAPI.GetSingleton<ConfigQlearn>();

        NativeList<float3x3> qTables = new NativeList<float3x3>(Allocator.TempJob);
        NativeList<float> qRewards = new NativeList<float>(Allocator.TempJob);

        // Populate qTables and qRewards
        foreach ((var QtableComponent,var QtableRewardComponent) in SystemAPI.Query<RefRO<QtableComponent>, RefRO<QtableRewardComponent>>())
        {
            qTables.Add(new float3x3(QtableComponent.ValueRO.up,QtableComponent.ValueRO.down,QtableComponent.ValueRO.right,QtableComponent.ValueRO.left,
                QtableComponent.ValueRO.upRight,QtableComponent.ValueRO.upLeft,QtableComponent.ValueRO.downRight,QtableComponent.ValueRO.downLeft,QtableComponent.ValueRO.stay));
        
            qRewards.Add(QtableRewardComponent.ValueRO.reward);
        }

        // Calculate new Q-values
        foreach ((var enemyActionComponent, var enemyRewardComponent) in SystemAPI.Query<RefRW<EnemyActionComponent>, RefRW<EnemyRewardComponent>>())
        {
            if (enemyActionComponent.ValueRO.IsReadyToUpdateQtable){
                var qTable = qTables[enemyActionComponent.ValueRO.gridFlatenPosition];
                var reward = qRewards[enemyActionComponent.ValueRO.gridFlatenPosition];//+ enemyRewardComponent.ValueRO.earnReward;
                if(enemyRewardComponent.ValueRO.earnReward != 0){
                    enemyRewardComponent.ValueRW.earnReward= 0;
                }
                var discountFactor = qlearnConfig.gamma;
                var learningRate = qlearnConfig.alpha;
                var penalty = enemyActionComponent.ValueRO.numberOfSteps * -0.1f; 
                
                // Q-Learning formula
                var oldValue = ExtractActionValue(qTable, enemyActionComponent.ValueRO.chosenAction);
                var newValue = oldValue + learningRate * (reward + penalty + discountFactor * enemyActionComponent.ValueRO.chosenNextActionQvalue - oldValue);

                // Update the Q-value in the matrix
                qTable = UpdateActionValue(qTable, enemyActionComponent.ValueRO.chosenAction, newValue);
                qTables[enemyActionComponent.ValueRO.gridFlatenPosition] = qTable;

                enemyActionComponent.ValueRW.IsReadyToUpdateQtable = false;
            }
        }

        // Assign updated Q-values back to QtableComponents
        int indexQtable = 0;
        foreach (var qtableComponent in SystemAPI.Query<RefRW<QtableComponent>>())
        {
            var qTable = qTables[indexQtable];
            qtableComponent.ValueRW.up = qTable.c0.x;
            qtableComponent.ValueRW.down = qTable.c0.y;
            qtableComponent.ValueRW.right = qTable.c0.z;
            qtableComponent.ValueRW.left = qTable.c1.x;
            qtableComponent.ValueRW.upRight = qTable.c1.y;
            qtableComponent.ValueRW.upLeft = qTable.c1.z;
            qtableComponent.ValueRW.downRight = qTable.c2.x;
            qtableComponent.ValueRW.downLeft = qTable.c2.y;
            qtableComponent.ValueRW.stay = qTable.c2.z;

            indexQtable++;
        }

        qTables.Dispose();
        qRewards.Dispose();
    }

    private float ExtractActionValue(float3x3 qMatrix, int action)
    {
    
            switch (action)
            {
                case 0: return qMatrix.c0.x; // Up
                case 1: return qMatrix.c0.y; // Down
                case 2: return qMatrix.c0.z; // Right
                case 3: return qMatrix.c1.x; // Left
                case 4: return qMatrix.c1.y; // UpRight
                case 5: return qMatrix.c1.z; // UpLeft
                case 6: return qMatrix.c2.x; // DownRight
                case 7: return qMatrix.c2.y; // DownLeft
                case 8: return qMatrix.c2.z; // Stay
                default: throw new System.ArgumentOutOfRangeException("action", "Invalid action index");
            }
        

    }

    private float3x3 UpdateActionValue(float3x3 qMatrix, int action, float newValue)
    {
        switch (action)
        {
            case 0: // Up
                qMatrix.c0.x = newValue;
                break;
            case 1: // Down
                qMatrix.c0.y = newValue;
                break;
            case 2: // Right
                qMatrix.c0.z = newValue;
                break;
            case 3: // Left
                qMatrix.c1.x = newValue;
                break;
            case 4: // UpRight
                qMatrix.c1.y = newValue;
                break;
            case 5: // UpLeft
                qMatrix.c1.z = newValue;
                break;
            case 6: // DownRight
                qMatrix.c2.x = newValue;
                break;
            case 7: // DownLeft
                qMatrix.c2.y = newValue;
                break;
            case 8: // Stay
                qMatrix.c2.z = newValue;
                break;
            default:
                throw new System.ArgumentOutOfRangeException("action", "Invalid action index");
        }
        return qMatrix;
    }

}
