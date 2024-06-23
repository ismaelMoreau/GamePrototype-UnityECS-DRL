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
        state.RequireForUpdate<Grounded>();
       
    }

    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var CollisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        
        foreach (var (grounded, localTransform) in SystemAPI.Query<RefRW<Grounded>, RefRW<LocalTransform>>())
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
                localTransform.ValueRW.Position.y = hit.Position.y;
                grounded.ValueRW.IsGrounded = true;
            }
            else
            {
                grounded.ValueRW.IsGrounded = false;
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
