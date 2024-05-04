using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Random= Unity.Mathematics.Random;

[UpdateAfter(typeof(QlearningRewardSystem))]
public partial struct QlearningActionSelectionSystem : ISystem
{
    public Random random;
    public void OnCreate(ref SystemState state)
    {
        random = new Random(123);
        state.RequireForUpdate<ConfigQlearn>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var qlearnConfig = SystemAPI.GetSingleton<ConfigQlearn>();
        var gridConfig = SystemAPI.GetSingleton<ConfigQlearnGrid>();

        NativeList<float3x3> qTables = new NativeList<float3x3>(Allocator.TempJob);

        foreach (var QtableComponent in SystemAPI.Query<RefRO<QtableComponent>>())
        {
            qTables.Add(new float3x3(QtableComponent.ValueRO.up,QtableComponent.ValueRO.down,QtableComponent.ValueRO.right,QtableComponent.ValueRO.left,
            QtableComponent.ValueRO.upRight,QtableComponent.ValueRO.upLeft,QtableComponent.ValueRO.downRight,QtableComponent.ValueRO.downLeft,QtableComponent.ValueRO.stay));
        }

        foreach ((var enemy,var enemyActiontimer) in SystemAPI.Query<RefRW<EnemyActionComponent>,RefRW<EnemyActionTimerComponent>>())
        {
            if (!enemy.ValueRO.isDoingAction)
            {
                var positionInitial = enemy.ValueRO.gridFlatenPosition;
                // Determine the action using epsilon-greedy policy
                var (chosenAction, actionValue) = SelectAction(positionInitial, qlearnConfig.epsilon, qTables[positionInitial], enemy.ValueRO.chosenAction, gridConfig.width, gridConfig.height);

                int nextActionIndexFlattenPosition = GetNeighborIndices(positionInitial, chosenAction, gridConfig.width, gridConfig.height);

                var (chosenNextAction, nextActionValue) = SelectAction(nextActionIndexFlattenPosition, qlearnConfig.epsilon, qTables[nextActionIndexFlattenPosition], chosenAction, gridConfig.width, gridConfig.height);

                enemy.ValueRW.chosenAction = chosenAction;
                enemy.ValueRW.chosenActionQvalue = actionValue;
                enemy.ValueRW.numberOfSteps++;
                enemy.ValueRW.isDoingAction = true;
                enemy.ValueRW.chosenNextAction = chosenNextAction;
                enemy.ValueRW.chosenNextActionQvalue = nextActionValue;
                enemy.ValueRW.nextActionGridFlatenPosition = nextActionIndexFlattenPosition;
                
                enemyActiontimer.ValueRW.actionDuration=1;// TODO: only move action for now , got to adjust on chosen action 
            }
        }

        qTables.Dispose();
    }

    public int GetNeighborIndices(int index, int chosenAction, int width, int height)
{
    int row = index / width;
    int col = index % width;

    int up = row > 0 ? index - width : -1;
    int down = row < height - 1 ? index + width : -1;
    int right = col < width - 1 ? index + 1 : -1;
    int left = col > 0 ? index - 1 : -1;
    int upLeft = (row > 0 && col > 0) ? index - width - 1 : -1;
    int upRight = (row > 0 && col < width - 1) ? index - width + 1 : -1;
    int downLeft = (row < height - 1 && col > 0) ? index + width - 1 : -1;
    int downRight = (row < height - 1 && col < width - 1) ? index + width + 1 : -1;

    switch (chosenAction)
    {
        case 0: return up;
        case 1: return down;
        case 2: return right;
        case 3: return left;
        case 4: return upRight;
        case 5: return upLeft;
        case 6: return downRight;
        case 7: return downLeft;
        case 8: return index; // Stay action returns the same index
        default: return -1;
    }
}

private (int action, float value) SelectAction(int index, float epsilon, float3x3 possibleMovAction, int oldAction, int width, int height)
{
    NativeList<int> validActions = new NativeList<int>(Allocator.Temp);

    // Consider all actions valid except the inverse of the previous action
    // This simplistic approach does not account for "invalid" actions like moving into walls
    for (int i = 0; i <= 8; i++)
    {
        if (i != oldAction && GetNeighborIndices(index, i, width, height) != -1)
        {
            validActions.Add(i);
        }
    }

    int chosenAction;
    float actionValue;

    if (random.NextFloat(0f, 1f) < epsilon || validActions.Length == 0)
    {
        int randomIndex = random.NextInt(0, validActions.Length);
        chosenAction = validActions[randomIndex];
        actionValue = ExtractActionValue(possibleMovAction, chosenAction);
    }
    else
    {
        // Choose the best action from validActions
        var maxAction = GetMaxValueActionFromValidActions(possibleMovAction, validActions);
        chosenAction = validActions[maxAction.index];
        actionValue = maxAction.value;
    }

    validActions.Dispose();
    return (chosenAction, actionValue);
}

private float ExtractActionValue(float3x3 qMatrix, int action)
{
    switch (action)
    {
        case 0: return qMatrix.c0.x; // Up
        case 1: return qMatrix.c0.y; // Down
        case 2: return qMatrix.c0.z; // Left
        case 3: return qMatrix.c1.x; // Right
        case 4: return qMatrix.c1.y; // UpRight
        case 5: return qMatrix.c1.z; // UpLeft
        case 6: return qMatrix.c2.x; // DownRight
        case 7: return qMatrix.c2.y; // DownLeft
        case 8: return qMatrix.c2.z; // Stay
        default: return 0f; // Should not happen
    }
}

private (int index, float value) GetMaxValueActionFromValidActions(float3x3 qMatrix, NativeList<int> validActions)
{
    float maxActionValue = float.MinValue;
    int maxActionIndex = 0;

    for (int i = 0; i < validActions.Length; i++)
    {
        float actionValue = ExtractActionValue(qMatrix, validActions[i]);
        if (actionValue > maxActionValue)
        {
            maxActionValue = actionValue;
            maxActionIndex = i;
        }
    }

    return (maxActionIndex, maxActionValue);
}

    public enum EnemyActionEnum
    {
        Up,
        Down,
        Left,
        Right,
        UpRight,
        UpLeft,
        DownRight,
        DownLeft,
        Stay,
    }
}
