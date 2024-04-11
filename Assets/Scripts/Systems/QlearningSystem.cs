using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Random= Unity.Mathematics.Random;
using Unity.VisualScripting;
using Unity.Scenes;
using System.Threading.Tasks;

[UpdateAfter(typeof(EnemyMovementSystem))]
public partial struct QlearningSystem : ISystem
{
    public Random random; 
    [BurstCompile]    
    public void OnCreate(ref SystemState state){
       
        //var gridconfig =  SystemAPI.GetSingleton<ConfigQlearnGrid>();
        random = new Random(123);
       
        int centerX = 9;
        int centerZ = 9;
        int regionSize = 1; 
        float maxDistance = math.abs(centerX - 0) + math.abs(centerZ - 0);
        
        float maxReward = 0f; // Maximum reward at the center
        float minReward = 0f; 

        for (int x = 0; x < 20; x++)
        {
            for (int z = 0; z < 20; z++)
            {
                Entity Qtable = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponent<QtableComponent>(Qtable);
                
                // Calculate Manhattan distance from the center
                float distanceFromCenter = math.abs(x - centerX) + math.abs(z - centerZ);
                
                
                float reward = minReward + (maxReward - minReward) * ((float)(maxDistance - distanceFromCenter) / maxDistance);
                 if (x >= (centerX - regionSize) && x <= (centerX + regionSize) && 
                    z >= (centerZ - regionSize) && z <= (centerZ + regionSize)) {
                    reward = 1000f; // Set reward to 100 for cells within the defined square
                }
               
                var rewardComponent = new QtableRewardComponent { reward = reward };
                state.EntityManager.AddComponentData(Qtable, rewardComponent);
            }
        }
 
        foreach(var QtablePossibleAction  in SystemAPI.Query<RefRW<QtableComponent>>()){
            QtablePossibleAction.ValueRW.up = 0.1f;// random.NextFloat(0.0f, 0.001f);
            QtablePossibleAction.ValueRW.down = 0.1f;// random.NextFloat(0.0f, 0.001f);
            QtablePossibleAction.ValueRW.left = 0.1f;// random.NextFloat(0.0f, 0.001f);
            QtablePossibleAction.ValueRW.right = 0.1f;// random.NextFloat(0.0f, 0.001f);
        }
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {   
        var qlearnconfig =  SystemAPI.GetSingleton<ConfigQlearn>();
        var gridconfig =  SystemAPI.GetSingleton<ConfigQlearnGrid>();
      
        NativeList<float4> qtables = new NativeList<float4>(Allocator.TempJob);
        NativeList<float> qreward = new NativeList<float>(Allocator.TempJob);
        
        foreach((var QtableComponent,var reward)  in SystemAPI.Query<RefRO<QtableComponent>,RefRO<QtableRewardComponent>>()){
           qtables.Add(new float4(QtableComponent.ValueRO.up,QtableComponent.ValueRO.down,QtableComponent.ValueRO.right,QtableComponent.ValueRO.left));
           qreward.Add(reward.ValueRO.reward);
        }
        
        foreach(var enemy in SystemAPI.Query<RefRW<EnemyGridPositionComponent>>()){
            if(!enemy.ValueRO.isDoingAction){
              
                var x = qtables[(int)enemy.ValueRO.gridFlatenPosition].x;
                var y = qtables[(int)enemy.ValueRO.gridFlatenPosition].y;
                var z = qtables[(int)enemy.ValueRO.gridFlatenPosition].z;
                var w = qtables[(int)enemy.ValueRO.gridFlatenPosition].w;

                var positionInitial =  enemy.ValueRO.gridFlatenPosition;
                // Determine the action using epsilon-greedy policy
                var (chosenAction, actionValue) = SelectAction(positionInitial, qlearnconfig.epsilon,qtables[(int)positionInitial]
                    ,enemy.ValueRO.chosenAction,gridconfig.width,gridconfig.height);
               
                float nextActionIndexPosition = GetNeighborIndices(positionInitial,chosenAction,gridconfig.width,gridconfig.height);
       
                var (chosenNextAction, nextActionValue) = SelectAction(nextActionIndexPosition, qlearnconfig.epsilon,qtables[(int)nextActionIndexPosition]
                    ,chosenAction,gridconfig.width,gridconfig.height);


                enemy.ValueRW.chosenAction = chosenAction;
                var numberOfSteps = enemy.ValueRW.numberOfSteps++;
                enemy.ValueRW.isDoingAction = true;

                switch (chosenAction)
                {
                    case 0: 
                        x = actionValue + qlearnconfig.alpha * (qreward[(int)nextActionIndexPosition]+(numberOfSteps*-0.01f) + qlearnconfig.gamma * nextActionValue - actionValue);
                        break;
                    case 1:
                        y = actionValue + qlearnconfig.alpha * (qreward[(int)nextActionIndexPosition]+(numberOfSteps*-0.01f) + qlearnconfig.gamma * nextActionValue - actionValue);
                        break;
                    case 2:
                        z = actionValue + qlearnconfig.alpha * (qreward[(int)nextActionIndexPosition]+(numberOfSteps*-0.01f) + qlearnconfig.gamma * nextActionValue - actionValue);
                        break;
                    case 3:
                        w = actionValue + qlearnconfig.alpha * (qreward[(int)nextActionIndexPosition]+(numberOfSteps*-0.01f) + qlearnconfig.gamma * nextActionValue - actionValue);
                        break;
                }
                qtables[(int)enemy.ValueRO.gridFlatenPosition] = new float4(x,y,z,w);
            }
        }
        int indexqtable = 0;
        foreach(var qtableComponent  in SystemAPI.Query<RefRW<QtableComponent>>()){
            qtableComponent.ValueRW.up=qtables[indexqtable].x;
            qtableComponent.ValueRW.down=qtables[indexqtable].y;
            qtableComponent.ValueRW.right=qtables[indexqtable].z;
            qtableComponent.ValueRW.left=qtables[indexqtable].w;
            indexqtable++;
        }
   
        qtables.Dispose();
        qreward.Dispose();
      
    }
    public float GetNeighborIndices(float index, float chosenAction, float width, float height)
    {
        // Calculate row and column from index
        int row = (int)index / (int)width;
        int col = (int)index % (int)width;

        // Calculate neighbor indices
        float up = row > 0 ? index - width : -1; // Move up if not in the first row
        float down = row < height - 1 ? index + width : -1; // Move down if not in the last row
        float right = col < width - 1 ? index + 1 : -1; // Move right if not in the last column
        float left = col > 0 ? index - 1 : -1; // Move left if not in the first column

        // Return the index based on the chosen action
        switch (chosenAction)
        {
            case 0: return up;
            case 1: return down;
            case 2: return right;
            case 3: return left;
            default: return -1; // Invalid action
        }
    }

    private (float action, float value) SelectAction(float index, float epsilon, float4 possibleMovAction, float oldAction, float width, float height) {
       
        NativeList<float> validActions = new NativeList<float>(Allocator.Temp);

        for (int i = 0; i < 4; i++) {
            if (i != oldAction && GetNeighborIndices(index, i, width, height) != -1) {
                validActions.Add(i);
            }
        }

        float chosenAction;
        float actionValue;

        if (random.NextFloat(0f, 1f) < epsilon || validActions.Length == 0) {
            
            int randomIndex = random.NextInt(0, validActions.Length); 
            chosenAction = validActions[randomIndex];
            
            actionValue = chosenAction switch {
                0 => possibleMovAction.x,
                1 => possibleMovAction.y,
                2 => possibleMovAction.z,
                3 => possibleMovAction.w,
                _ => 0 // Default case, shouldn't be hit
            };
        } else {
            // Choose the best action from validActions
            var maxAction = GetMaxValueActionFromValidActions(possibleMovAction, validActions);
            chosenAction = validActions[maxAction.index];
            actionValue = maxAction.value;
        }

        validActions.Dispose();
        return (chosenAction, actionValue);
    }

    private (int index, float value) GetMaxValueActionFromValidActions(float4 possibleMovAction, NativeList<float> validActions) {
        float maxActionValue = float.MinValue;
        int maxActionIndex = 0;

        for (int i = 0; i < validActions.Length; i++) {
            float actionValue = 0;
            switch ((int)validActions[i]) {
                case 0: actionValue = possibleMovAction.x; break;
                case 1: actionValue = possibleMovAction.y; break;
                case 2: actionValue = possibleMovAction.z; break;
                case 3: actionValue = possibleMovAction.w; break;
            }

            if (actionValue > maxActionValue) {
                maxActionValue = actionValue;
                maxActionIndex = i;
            }
        }

        return (maxActionIndex, maxActionValue);
    }
}
