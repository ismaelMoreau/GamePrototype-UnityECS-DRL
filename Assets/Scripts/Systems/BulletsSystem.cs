// using Unity.Burst;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;
// using Unity.Collections;
// using Unity.Jobs;
// using Unity.Physics;
// using UnityEngine;





// public partial struct BulletstSystem : ISystem
// {
   

//     public void OnCreate(ref SystemState state)
//     {
        
//     }

//     [BurstCompile] 
//     public void OnUpdate(ref SystemState state)
//     {
//          if (!Input.GetKey(KeyCode.B)){return;}
//         PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
     
//         float3 playerPosition = new float3(0, 0, 0);
//         foreach ((RefRW<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed) 
//             in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovementComponent>>())
//         {
//             playerPosition = localTransform.ValueRO.Position; 
//         }
        
        
//         float nearestDistanceEnemy = 10000;
//         float3 nearestEnemyPosition = new float3(0, 0, 0);
//         NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.TempJob);
//         foreach ((var localTransform, var enemy,Entity e) 
//             in SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyMovementComponent>>().WithEntityAccess())
//         {
//             float distance = math.distance(playerPosition, localTransform.ValueRW.Position);
//             if (distance < nearestDistanceEnemy) {
//                 nearestDistanceEnemy = distance;
//                 nearestEnemyPosition = localTransform.ValueRW.Position;
//             }
            
//             bool isHits= physicsWorld.SphereCastAll(localTransform.ValueRO.Position,1,localTransform.ValueRO.Position,1,ref hits,new CollisionFilter
//                 {
//                     BelongsTo = (uint)CollisionLayer.enemies, // Ray belongs to all layers (use a specific mask if needed)
//                     CollidesWith = (uint)CollisionLayer.bullets,  // Only collide with ground layer
//                     GroupIndex = 0
//                 });
//             if(isHits){state.EntityManager.SetComponentEnabled<DestroyTag>(e,true);}
//         }
//         foreach(var hit in hits)
//         {
//             if (state.EntityManager.HasComponent<DestroyTag>(hit.Entity))
//             {
//                 state.EntityManager.SetComponentEnabled<DestroyTag>(hit.Entity,true);
//             }
//         }
//         hits.Dispose();

//         var job = new BulletMoveTowardsEnnemyJob
//         {
//             nearestEnemyPosition = nearestEnemyPosition,
//             DeltaTime = SystemAPI.Time.DeltaTime,
//         };        
//         state.Dependency = job.Schedule(state.Dependency);
//         state.Dependency.Complete();        
//     }
// }
// // Job to move enemies towards the player
// [BurstCompile] 
// public partial struct BulletMoveTowardsEnnemyJob : IJobEntity
// {
    
//     public float3 nearestEnemyPosition;
//     public float DeltaTime;


    

//     public void Execute(Entity entity,[ChunkIndexInQuery]int index, ref LocalTransform localTransform, in BulletsMouvementComponent bullets)
//     {
//         float3 direction = math.normalize(nearestEnemyPosition - localTransform.Position);
//         float distance = math.distance(nearestEnemyPosition, localTransform.Position);
//         localTransform.Position += direction * bullets.speed * DeltaTime;
      
//     }
// } 
