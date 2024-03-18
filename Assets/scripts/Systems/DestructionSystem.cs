using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
// [UpdateInGroup(typeof(SimulationSystemGroup))]
// [UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))] // Ensure it runs after command buffers are played back
public partial class DestructionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityQuery query = GetEntityQuery(ComponentType.ReadOnly<DestroyTag>());

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        ecb.DestroyEntity(query,EntityQueryCaptureMode.AtPlayback);
        ecb.Playback(EntityManager);

        // You are responsible for disposing of any ECB you create.
        ecb.Dispose();
    }
}
