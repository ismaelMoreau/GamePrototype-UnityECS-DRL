using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(TransformSystemGroup))]
public partial class QlearningSystemGroup : ComponentSystemGroup
{
    
}