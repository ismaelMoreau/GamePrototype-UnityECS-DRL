using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class EnemyAuthoring : MonoBehaviour
{

    public float speed = 2f;

    public float jumpForce = 5f;

    public int numberOfSteps = 0;
    public float earnReward= 0;
    public float actionTimer= 0;
    public float actionDuration= 0;
    public float epsilon= 0.3f;
    public int currentActionIndex = 0;
    public float actionCooldownTimer = 0;
    public bool canForward, canBackward, canStepRight, canStepLeft, canDash, canBlock, canHeal, canJump, canStay;

    public float cooldownDashDuration=0f, cooldownBlockDuration=2f, cooldownHealDuration=3f, cooldownJumpDuration=2f, cooldownStayDuration=0.5f;
    public float cooldownDashTimer=0f, cooldownBlockTimer=0f, cooldownHealTimer=0f, cooldownJumpTimer=0f, cooldownStayTimer=0f;

    private class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyMovementComponent
            {
                speed = authoring.speed,
                jumpForce = authoring.jumpForce,
                isGrounded = false,
                isCooldownDashActive = false,
                isCooldownBlockActive = false,
                isCooldownHealActive = false,
                isCooldownJumpActive = false,
                isCooldownStayActive = false
            });
            AddComponent(entity,new DestroyTag{});
            SetComponentEnabled<DestroyTag>(entity, false);
            AddComponent(entity,new EnemyActionComponent{
                isDoingAction = false,
                numberOfSteps = authoring.numberOfSteps,
                //IsReadyToUpdateQtable = false,
                //currentActionIndex = authoring.currentActionIndex
            });
            SetComponentEnabled<EnemyActionComponent>(entity, true);
            AddComponent(entity,new EnemyRewardComponent{
                earnReward= authoring.earnReward
            });
            AddComponent(entity,new EnemyActionTimerComponent{
                actionTimer = authoring.actionTimer,
                actionDuration = authoring.actionDuration,
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
            AddComponent(entity, new EnemyStateComponent{
                playerDistance = 1,
                playerHealth = 1,
                ownPositionX = 0,
                ownPositionY = 0,
                firstEnemyHealth = 1,
                secondEnemyHealth = 1,
                playerOrientationX = 0,
                playerOrientationZ = 0,
                enemiesSharedReward = 0,
                velocity = 0
            });
            AddComponent(entity,new EnemyPreviousStateComponent{});
               AddComponent(entity, new EnemyActionsCooldownComponent{
                cooldownDashDuration = authoring.cooldownDashDuration,
                cooldownBlockDuration = authoring.cooldownBlockDuration,
                cooldownHealDuration = authoring.cooldownHealDuration,
                cooldownJumpDuration = authoring.cooldownJumpDuration,
                cooldownStayDuration = authoring.cooldownStayDuration,
                cooldownDashTimer = authoring.cooldownDashTimer,
                cooldownBlockTimer = authoring.cooldownBlockTimer,
                cooldownHealTimer = authoring.cooldownHealTimer,
                cooldownJumpTimer = authoring.cooldownJumpTimer,
                cooldownStayTimer = authoring.cooldownStayTimer
            });
        }
    }
}