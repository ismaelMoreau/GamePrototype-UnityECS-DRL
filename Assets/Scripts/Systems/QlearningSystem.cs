// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;
// using Unity.Collections;
// using Unity.Burst;

// public partial struct QlearningSystem : ISystem
// {
    
//     [BurstCompile]    
//     public void OnCreate(ref SystemState state){
//         state.RequireForUpdate<QtableComponent>();
//         for (int x = 0; x < 20; x++)
//         {
//             for (int y = 0; y < 20; y++)
//             {
//                 state.EntityManager.AddComponent<QtableComponent>(state.SystemHandle);
//             }
//         }
//         foreach(var Qtable  in SystemAPI.Query<RefRW<QtableComponent>>()){
//            Qtable.ValueRW.up = 0;
//            Qtable.ValueRW.down = 0;
//            Qtable.ValueRW.left = 0;
//            Qtable.ValueRW.right = 0;
//         }
//     }
//     [BurstCompile]
//     public void OnUpdate(ref SystemState state)
//     {
//     //     SystemAPI.SetComponent(state.SystemHandle, new PlayerInputData {
//     //         AxisX = [...read controller data],
//     //         AxisY = [...read controller data]
//     //     });
//         foreach(var Qtable  in SystemAPI.Query<RefRW<QtableComponent>>()){
            
//         }
//     }
// }
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Random= Unity.Mathematics.Random;
using Unity.VisualScripting;
using Unity.Scenes;

[UpdateAfter(typeof(EnemyMovementSystem))]
public partial struct QlearningSystem : ISystem
{
    public Random random; 
    [BurstCompile]    
    public void OnCreate(ref SystemState state){
       
        //var gridconfig =  SystemAPI.GetSingleton<ConfigQlearnGrid>();
        random = new Random(123);
       
        int centerX = 10;
        int centerZ = 10;
        
        float maxDistance = math.abs(centerX - 0) + math.abs(centerZ - 0);
        
        float maxReward = 10.0f; // Maximum reward at the center
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
                
               
                var rewardComponent = new QtableRewardComponent { reward = reward };
                state.EntityManager.AddComponentData(Qtable, rewardComponent);
            }
        }
 
        foreach(var QtablePossibleAction  in SystemAPI.Query<RefRW<QtableComponent>>()){
            QtablePossibleAction.ValueRW.up = random.NextFloat(1000.0f, 1000.1f);
            QtablePossibleAction.ValueRW.down = random.NextFloat(1000.0f, 1000.1f);
            QtablePossibleAction.ValueRW.left = random.NextFloat(1000.0f, 1000.1f);
            QtablePossibleAction.ValueRW.right = random.NextFloat(1000.0f, 1000.1f);
        }
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {   
        var qlearnconfig =  SystemAPI.GetSingleton<ConfigQlearn>();
        var gridconfig =  SystemAPI.GetSingleton<ConfigQlearnGrid>();
        NativeList<float> listOfFlatenIndexAgentPositionOnGrid = new NativeList<float>(Allocator.TempJob);
        foreach(var enemy in SystemAPI.Query<RefRO<EnemyGridPositionComponent>>()){
            listOfFlatenIndexAgentPositionOnGrid.Add(enemy.ValueRO.gridFlatenPosition);
           
        }
       
        NativeList<float4> qtables = new NativeList<float4>(Allocator.TempJob);
        NativeList<float> qreward = new NativeList<float>(Allocator.TempJob);
        
        foreach((var QtableComponent,var reward)  in SystemAPI.Query<RefRW<QtableComponent>,RefRO<QtableRewardComponent>>()){
           qtables.Add(new float4(QtableComponent.ValueRO.up,QtableComponent.ValueRO.down,QtableComponent.ValueRO.right,QtableComponent.ValueRO.left));
           qreward.Add(reward.ValueRO.reward);
        }
        int indexAgentPositionOnGrid = 0;
        int numberOfAgentActive = 0;
        foreach((var Qtable,var reward)  in SystemAPI.Query<RefRW<QtableComponent>,RefRO<QtableRewardComponent>>()){
            
            if(ContainsValue(listOfFlatenIndexAgentPositionOnGrid,(float)indexAgentPositionOnGrid)){
            // Determine the action using epsilon-greedy policy
             
            float action = SelectAction(indexAgentPositionOnGrid, qlearnconfig.epsilon,qtables[indexAgentPositionOnGrid]);
            if (action == -1){return;}
            // // Update position based on action (this is a simplified representation)
            // UpdatePosition(ref position, action);
            

            float nextActionindex = GetNeighborIndices(indexAgentPositionOnGrid,action,gridconfig.width,gridconfig.height);
            
            // Invalid action
            if (nextActionindex == -1){return;}
            

            // // Update Q-value using the Q-learning formula
            float oldQValue =0;
            switch (action)
            {
                case 0: 
                    oldQValue=Qtable.ValueRO.up;
                    break;
                case 1:
                    oldQValue=Qtable.ValueRO.down;
                    break;
                case 2:
                    oldQValue=Qtable.ValueRO.right;
                    break;
                case 3:
                    oldQValue=Qtable.ValueRO.left;
                    break;
            }
            //Debug.Log("oldQValue"+oldQValue.ToString());
            float maxNextQValue = math.cmax(qtables[(int)nextActionindex]); 
            //Debug.Log("maxNextQValue"+maxNextQValue.ToString());

            switch (action)
            {
                case 0: 
                    Qtable.ValueRW.up = oldQValue + qlearnconfig.alpha * (qreward[(int)nextActionindex] + qlearnconfig.gamma * maxNextQValue - oldQValue);
                    break;
                case 1:
                    Qtable.ValueRW.down = oldQValue + qlearnconfig.alpha * (qreward[(int)nextActionindex] + qlearnconfig.gamma * maxNextQValue - oldQValue);
                    break;
                case 2:
                    Qtable.ValueRW.right = oldQValue + qlearnconfig.alpha * (qreward[(int)nextActionindex] + qlearnconfig.gamma * maxNextQValue - oldQValue);
                    break;
                case 3:
                    Qtable.ValueRW.left = oldQValue + qlearnconfig.alpha * (qreward[(int)nextActionindex] + qlearnconfig.gamma * maxNextQValue - oldQValue);
                    break;
            }
            listOfFlatenIndexAgentPositionOnGrid[numberOfAgentActive]=action; // reusing the list with the chosen action 
            numberOfAgentActive++;
            }
            indexAgentPositionOnGrid++;
        }
        int countEnemiesAction = 0;
        foreach(var enemy in SystemAPI.Query<RefRW<EnemyGridPositionComponent>>()){
            enemy.ValueRW.chosenAction = listOfFlatenIndexAgentPositionOnGrid[countEnemiesAction];
            enemy.ValueRW.isDoingAction = true;
            countEnemiesAction++;
        }
        listOfFlatenIndexAgentPositionOnGrid.Dispose();
        qtables.Dispose();
        qreward.Dispose();
      
    }
    bool ContainsValue(NativeList<float> list, float value) {
    for (int i = 0; i < list.Length; i++) {
        if (list[i] == value) {
            return true; // Found the value
        }
    }
    return false; // Value not found
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

    private float SelectAction(float index, float epsilon,float4 possibleMovAction) {
        if (random.NextFloat(0f, 1f) < epsilon) {
                // Return a random action
                return random.NextInt(0, 4);
        } else { 
                return GetMaxValueAction(possibleMovAction);
        }
    }
    private float GetMaxValueAction(float4 possibleMovAction){
        float highestValue = math.cmax(possibleMovAction); 
        float action = highestValue == possibleMovAction.x ? 1 :
            highestValue == possibleMovAction.y ? 2 :
            highestValue == possibleMovAction.z ? 3 : 4;
        return action;
    }
    
}
