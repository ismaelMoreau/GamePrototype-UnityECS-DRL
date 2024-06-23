using Google.Protobuf.WellKnownTypes;
using Unity.Entities;

public struct EnemyMovementComponent : IComponentData
{
    public float speed;
    public float jumpForce;
}

public struct EnemyActionComponent : IComponentData, IEnableableComponent
{
    //public int gridFlatenPosition;
    
    //public bool IsReadyToUpdateQtable;
    public bool isDoingAction;

    public int chosenAction;
    public int numberOfSteps;

    // public float chosenActionQvalue;

    // public int chosenNextAction;
    // public float chosenNextActionQvalue;
    
    //public int nextActionGridFlatenPosition;

    //public int currentActionIndex;
    //public float currentTotalQvalue;

    
}
public struct EnemyRewardComponent : IComponentData, IEnableableComponent
{
    public float earnReward;
}
public struct EnemyActionTimerComponent : IComponentData 
{
    
    public float actionTimer;
    public float actionDuration;
    
}
public struct EnemyEpsilonComponent : IComponentData 
{
    public float epsilon;
}
public struct DestroyTag : IComponentData, IEnableableComponent{ 
  
}
public struct EnemyActionBufferElement : IBufferElementData
{
    public int action;
    public float actionValue;
}
public struct EnemyPossibleActionComponent : IComponentData
{
    public bool canForward, canBackward, canStepRight, canStepLeft, canDash, canBlock, canHeal, canJump, canStay;
}
public struct EnemyActionsCooldownComponent : IComponentData
{
    public float cooldownDashTimer, cooldownBlockTimer, cooldownHealTimer, cooldownJumpTimer, cooldownStayTimer;
    public float cooldownDashDuration, cooldownBlockDuration, cooldownHealDuration, cooldownJumpDuration, cooldownStayDuration;
   
}
public struct EnemyPreviousStateComponent : IComponentData
{
    public EnemyStateComponent previousState;
}
public struct EnemyStateComponent : IComponentData 
{
    public float playerDistance;
    public float playerHealth;
    public float ownPositionX;
    public float ownPositionY;
    public float firstEnemyHealth;
    public float secondEnemyHealth;
    public float playerOrientationX;
    public float playerOrientationZ;
    public float enemiesSharedReward;

    public float velocity;
}