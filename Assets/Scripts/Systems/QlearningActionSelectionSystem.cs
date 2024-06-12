using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Random = Unity.Mathematics.Random;

[UpdateAfter(typeof(QlearningInitSystem))]
public partial struct QlearningActionSelectionSystem : ISystem
{
    public Random random;
    public void OnCreate(ref SystemState state)
    {
        random = new Random(123);
        state.RequireForUpdate<ConfigQlearn>();
        state.RequireForUpdate<EnemyActionComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var qlearnConfig = SystemAPI.GetSingleton<ConfigQlearn>();
        var gridConfig = SystemAPI.GetSingleton<ConfigQlearnGrid>();

        NativeList<float3x3> qTables = new NativeList<float3x3>(Allocator.TempJob);

        foreach (var QtableComponent in SystemAPI.Query<RefRO<QtableComponent>>())
        {
            qTables.Add(new float3x3(
                QtableComponent.ValueRO.forward,
                QtableComponent.ValueRO.backward,
                QtableComponent.ValueRO.stepRight,
                QtableComponent.ValueRO.stepLeft,
                QtableComponent.ValueRO.dash,
                QtableComponent.ValueRO.block,
                QtableComponent.ValueRO.heal,
                QtableComponent.ValueRO.jump,
                QtableComponent.ValueRO.stay
            ));
        }

        foreach (var (enemy, enemyActiontimer, enemyEpsilon, actionBuffer, possibleActions) in SystemAPI.Query<RefRW<EnemyActionComponent>, RefRW<EnemyActionTimerComponent>, 
            RefRW<EnemyEpsilonComponent>, DynamicBuffer<EnemyActionBufferElement>, RefRO<EnemyPossibleActionComponent>>())
        {
            if (!enemy.ValueRO.isDoingAction)
            {
                if (enemy.ValueRO.currentActionIndex >= actionBuffer.Length - 1)
                {
                    // Initialize a new sequence of 5 different actions
                    actionBuffer.Clear();
                    enemy.ValueRW.currentTotalQvalue = 0f;
                    int indexGridPosition = enemy.ValueRO.gridFlatenPosition;;
                   
                    for (int i = 0; i < 5; i++)
                    {   
                        
                        var (chosenAction, actionValue, nextChosenGridIndex) = SelectAction(indexGridPosition, enemyEpsilon.ValueRO.epsilon, 
                            qTables[indexGridPosition], gridConfig.width, gridConfig.height, possibleActions.ValueRO);
                        enemy.ValueRW.currentTotalQvalue += actionValue;
                        actionBuffer.Add(new EnemyActionBufferElement { 
                            action = chosenAction,
                            actionValue = actionValue
                        });
                        indexGridPosition = nextChosenGridIndex;
                    }
                    enemy.ValueRW.currentActionIndex = 0;
                }

                int currentAction = actionBuffer[enemy.ValueRO.currentActionIndex].action;
                
                int nextActionIndexFlattenPosition = GetNeighborIndices(enemy.ValueRO.gridFlatenPosition, currentAction, gridConfig.width, gridConfig.height);

                enemy.ValueRW.chosenAction = actionBuffer[enemy.ValueRO.currentActionIndex].action;
                //enemy.ValueRW.chosenActionQvalue = actionBuffer[enemy.ValueRO.currentActionIndex].actionValue;
                enemy.ValueRW.numberOfSteps = math.min(enemy.ValueRO.numberOfSteps + 1, 100);
                enemyEpsilon.ValueRW.epsilon = math.max(enemyEpsilon.ValueRO.epsilon - 0.001f, 0.01f);
                enemy.ValueRW.isDoingAction = true;
                //enemy.ValueRW.chosenNextAction = actionBuffer[enemy.ValueRO.currentActionIndex + 1].action;
                //enemy.ValueRW.chosenNextActionQvalue = actionBuffer[enemy.ValueRO.currentActionIndex + 1].actionValue;
                enemy.ValueRW.nextActionGridFlatenPosition = nextActionIndexFlattenPosition;

                enemyActiontimer.ValueRW.actionDuration = 0.5f; // Adjust based on chosen action

                // Move to the next action in the sequence
                enemy.ValueRW.currentActionIndex++;
            }
        }

        qTables.Dispose();
    }

    private bool IsActionPossible(EnemyPossibleActionComponent possibleActions, int action)
    {
        switch ((EnemyActionEnum)action)
        {
            case EnemyActionEnum.foward:
                return possibleActions.canForward;
            case EnemyActionEnum.backward:
                return possibleActions.canBackward;
            case EnemyActionEnum.stepRight:
                return possibleActions.canStepRight;
            case EnemyActionEnum.stepLeft:
                return possibleActions.canStepLeft;
            case EnemyActionEnum.dash:
                return possibleActions.canDash;
            case EnemyActionEnum.block:
                return possibleActions.canBlock;
            case EnemyActionEnum.heal:
                return possibleActions.canHeal;
            case EnemyActionEnum.jump:
                return possibleActions.canJump;
            case EnemyActionEnum.stay:  
                return possibleActions.canStay;
            default:
                return false;
        }
    }

    public int GetNeighborIndices(int index, int chosenAction, int width, int height)
    {
        // Calculate grid center (player's position)
        int centerRow = height / 2;
        int centerCol = width / 2;

        // Calculate current enemy's row and column
        int row = index / width;
        int col = index % width;

        // Compute the direction vector from the enemy to the center
        int rowDirection = (int)math.sign(centerRow - row);
        int colDirection = (int)math.sign(centerCol - col);

        // Compute potential indices for each movement direction
        int moveIndex = -1;

        switch ((EnemyActionEnum)chosenAction)
        {
            case EnemyActionEnum.foward:
                // Moving toward the center
                moveIndex = index + (rowDirection * width) + colDirection;
                break;
            case EnemyActionEnum.backward:
                // Moving away from the center
                moveIndex = index - (rowDirection * width) - colDirection;
                break;
            case EnemyActionEnum.stepRight:
                // Moving perpendicular to the left of the center
                moveIndex = index + (-colDirection * width) + rowDirection;
                break;
            case EnemyActionEnum.stepLeft:
                // Moving perpendicular to the right of the center
                moveIndex = index + (colDirection * width) - rowDirection;
                break;
            case EnemyActionEnum.dash:
                // Dash towards the center, move further than forward
                moveIndex = index + 2 * ((rowDirection * width) + colDirection);
                break;
            case EnemyActionEnum.block:
            case EnemyActionEnum.heal:
            case EnemyActionEnum.jump:
            case EnemyActionEnum.stay:
                // These actions do not involve moving on the grid
                moveIndex = index;
                break;
        }

        // Ensure that the calculated move is within the grid bounds
        if (moveIndex < 0 || moveIndex >= width * height)
        {
            return -1; // Return an invalid index if out of bounds
        }

        return moveIndex;
    }

    private (int action, float value,int nextGridIndex) SelectAction(int index, float epsilon, float3x3 possibleMovAction, int width, int height, EnemyPossibleActionComponent possibleActions)
    {
        NativeList<int> validActions = new NativeList<int>(Allocator.Temp);

        // Consider all actions valid except the inverse of the previous action
        // This simplistic approach does not account for "invalid" actions like moving into walls
        for (int i = 0; i <= 8; i++)
        {
            if (GetNeighborIndices(index, i, width, height) != -1 && IsActionPossible(possibleActions, i)) // i != oldAction &&
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
        var nextGridIndex = GetNeighborIndices(index, chosenAction, width, height);
        return (chosenAction, actionValue, nextGridIndex);
    }

    private float ExtractActionValue(float3x3 qMatrix, int action)
    {
        switch (action)
        {
            case 0: return qMatrix.c0.x;
            case 1: return qMatrix.c0.y;
            case 2: return qMatrix.c0.z;
            case 3: return qMatrix.c1.x;
            case 4: return qMatrix.c1.y;
            case 5: return qMatrix.c1.z;
            case 6: return qMatrix.c2.x;
            case 7: return qMatrix.c2.y;
            case 8: return qMatrix.c2.z;
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
        foward,
        backward,
        stepRight,
        stepLeft,
        dash,
        block,
        heal,
        jump,
        stay
    }
}
