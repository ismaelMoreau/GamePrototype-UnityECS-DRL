// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;
// using Unity.Collections;
// using Unity.Burst;
// using Random= Unity.Mathematics.Random;
// using System;

// [UpdateBefore(typeof(QlearningActionSelectionSystem))]
// [UpdateBefore(typeof(TransformSystemGroup))]
// [UpdateInGroup(typeof(QlearningSystemGroup))]
// public partial struct QlearningInitSystem : ISystem
// {
//     public void OnCreate(ref SystemState state){
            
//         state.Enabled = false;// using drl system instead 
         
//          state.RequireForUpdate<ConfigQlearn>();
//     }
//     [BurstCompile]
//     public void OnUpdate(ref SystemState state){
        

//         //this ensure only one update
//         state.Enabled = false;
        
//         var configQlearn =  SystemAPI.GetSingleton<ConfigQlearn>();
        
//         var configQlearnGrid =  SystemAPI.GetSingleton<ConfigQlearnGrid>();
        

//         int centerX = 9;
//         int centerZ = 9;
//         int regionSize = 1; 
//         float maxDistance = math.abs(centerX - 0) + math.abs(centerZ - 0);
        
//         float maxReward = 0f; // Maximum reward at the center
//         float minReward = 0f; 

//         for (int x = 0; x < configQlearnGrid.width; x++)
//         {
//             for (int z = 0; z < configQlearnGrid.height; z++)
//             {
//                 Entity Qtable = state.EntityManager.CreateEntity();
//                 state.EntityManager.AddComponent<QtableComponent>(Qtable);
                
//                 // Calculate Manhattan distance from the center
//                 float distanceFromCenter = math.abs(x - centerX) + math.abs(z - centerZ);
                
                
//                 float reward = minReward + (maxReward - minReward) * ((float)(maxDistance - distanceFromCenter) / maxDistance);
//                  if (x >= (centerX - regionSize) && x <= (centerX + regionSize) && 
//                     z >= (centerZ - regionSize) && z <= (centerZ + regionSize)) {
//                    reward = 1f; // Set reward to 1 for cells within the defined square
//                 }
               
//                 var rewardComponent = new QtableRewardComponent { reward = reward };
//                 state.EntityManager.AddComponentData(Qtable, rewardComponent);
//             }
//         }
//         int indexOfQtableComponent = 0;
//         foreach (var QtablePossibleAction in SystemAPI.Query<RefRW<QtableComponent>>())
//         {
//             // Original directions
//             QtablePossibleAction.ValueRW.forward = 0.1f; // random.NextFloat(0.0f, 0.001f);
//             QtablePossibleAction.ValueRW.backward = 0.1f; // random.NextFloat(0.0f, 0.001f);
//             QtablePossibleAction.ValueRW.stepLeft = 0.1f; // random.NextFloat(0.0f, 0.001f);
//             QtablePossibleAction.ValueRW.stepRight = 0.1f; // random.NextFloat(0.0f, 0.001f);

//             // Diagonal directions
//             QtablePossibleAction.ValueRW.dash = 0.1f; // random.NextFloat(0.0f, 0.001f);
//             QtablePossibleAction.ValueRW.block = 0.1f; // random.NextFloat(0.0f, 0.001f);
//             QtablePossibleAction.ValueRW.heal = 0.1f; // random.NextFloat(0.0f, 0.001f);
//             QtablePossibleAction.ValueRW.jump = 0.1f; // random.NextFloat(0.0f, 0.001f);

//             // Stay in place
//             QtablePossibleAction.ValueRW.stay = 0.1f; // random.NextFloat(0.0f, 0.001f);
//             QtablePossibleAction.ValueRW.indexOfQtableComponent = indexOfQtableComponent;
//             indexOfQtableComponent++;
//         }
      
       
//         // foreach (EnemyRelativePositionFromPlayer position in Enum.GetValues(typeof(EnemyRelativePositionFromPlayer)))
//         // {
//         //     foreach (EnemyRelativeDirectionFromPLayer direction in Enum.GetValues(typeof(EnemyRelativeDirectionFromPLayer)))
//         //     {
//         //         Entity Qtable = state.EntityManager.CreateEntity();
//         //         state.EntityManager.AddComponent<QtableComponent>(Qtable);
//         //     }
//         // }
//     }
// }
// // public enum EnemyRelativePositionFromPlayer
// // {
// //     Near,
// //     Medium,
// //     Far
// // }

// // public enum EnemyRelativeDirectionFromPLayer
// // {
// //     Front,
// //     LeftFlank,
// //     RightFlank,
// //     Back
// // }

// // public enum EnemyActions
// // {
// //     RightSideStep,
// //     LeftSideStep,
// //     MoveFoward,
// //     Attack,
// //     Block,
// //     jump,
// //     Dash,
// //     Heal,
// //     Throw,
// //     Retreat
// // }
