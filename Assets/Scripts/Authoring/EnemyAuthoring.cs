using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class EnemyAuthoring : MonoBehaviour
{

    public float speed = 2f;


    public int numberOfSteps = 0;
    public float earnReward= 0;
    public float actionTimer= 0;
    public float actionDuration= 0;
    public float epsilon= 0.3f;
    public int currentActionIndex = 0;
    public bool canForward, canBackward, canStepRight, canStepLeft, canDash, canBlock, canHeal, canJump, canStay;
    
    private class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyMovementComponent
            {
                speed = authoring.speed
            });
            AddComponent(entity,new DestroyTag{});
            SetComponentEnabled<DestroyTag>(entity, false);
            AddComponent(entity,new EnemyActionComponent{
                isDoingAction = false,
                numberOfSteps = authoring.numberOfSteps,
                IsReadyToUpdateQtable = false,
                currentActionIndex = authoring.currentActionIndex
            });
            SetComponentEnabled<EnemyActionComponent>(entity, false);
            AddComponent(entity,new EnemyRewardComponent{
                earnReward= authoring.earnReward
            });
            AddComponent(entity,new EnemyActionTimerComponent{
                actionTimer = authoring.actionTimer,
                actionDuration = authoring.actionDuration
            });
            AddComponent(entity,new EnemyEpsilonComponent{
                epsilon= authoring.epsilon
            });
            AddBuffer<EnemyActionBufferElement>(entity);
            AddComponent(entity,new EnemyPossibleActionComponent{
                canForward = authoring.canForward,
                canBackward = authoring.canBackward,
                canStepRight = authoring.canStepRight,
                canStepLeft = authoring.canStepLeft,
                canDash = authoring.canDash,
                canBlock = authoring.canBlock,
                canHeal = authoring.canHeal,
                canJump = authoring.canJump,
                canStay = authoring.canStay
            });
        }
    }
}