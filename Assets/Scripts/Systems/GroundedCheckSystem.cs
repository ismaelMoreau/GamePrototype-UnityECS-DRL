using Unity.Burst;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;



[BurstCompile]
public partial struct  GroundCheckSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyMovementComponent>();
          state.RequireForUpdate<GamePlayingTag>();
       
    }

    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var CollisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        
        foreach (var (grounded, localTransform, physicsVelocity) in SystemAPI.Query<RefRW<EnemyMovementComponent>, RefRW<LocalTransform>, RefRW<PhysicsVelocity>>())
        {
            var point = localTransform.ValueRO.Position;
            var direction = new float3(0, -0.5f, 0);
            var rayInput = new RaycastInput()
            {
                Start = point,
                End = point + direction,
                Filter = new CollisionFilter()
                {
                    BelongsTo = (uint)1,
                    CollidesWith = (uint)4,
                    GroupIndex = 0
                }
            };
            //UnityEngine.Debug.Log($"Ray Start: {rayInput.Start}, Ray End: {rayInput.End}");

            if (CollisionWorld.CastRay(rayInput, out Unity.Physics.RaycastHit hit))
            {
                //UnityEngine.Debug.Log($"Hit: {hit.Position}, Distance: {hit.Fraction}");
                //localTransform.ValueRW.Position.y = hit.Position.y;

                grounded.ValueRW.isGrounded = true;
                // physicsVelocity.ValueRW.Linear.y = 0;
                // physicsVelocity.ValueRW.Angular = float3.zero;
            }
            else
            {
                grounded.ValueRW.isGrounded = false;
            }
        }
    }

    // [BurstCompile]
    // public partial struct GroundCheckJob : IJobEntity
    // {
    //     [ReadOnly] public PhysicsWorld PhysicsWorld;
       

    //     public void Execute(ref Grounded grounded, in LocalTransform translation)
    //     {
            
    //     }
    // }
    
}
