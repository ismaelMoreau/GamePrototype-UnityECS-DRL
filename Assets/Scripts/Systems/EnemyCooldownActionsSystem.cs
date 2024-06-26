using Unity.Entities;
using Unity.Burst;

[BurstCompile]
public partial struct EnemyActionsCooldownSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
          state.RequireForUpdate<GamePlayingTag>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        // No specific cleanup needed
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (cooldown,enemyMovementComponent) in SystemAPI.Query<RefRW<EnemyActionsCooldownComponent>,RefRW<EnemyMovementComponent>>())
        {
            UpdateCooldown(ref cooldown.ValueRW.cooldownDashTimer, cooldown.ValueRW.cooldownDashDuration, ref enemyMovementComponent.ValueRW.isCooldownDashActive, deltaTime);
            UpdateCooldown(ref cooldown.ValueRW.cooldownBlockTimer, cooldown.ValueRW.cooldownBlockDuration, ref enemyMovementComponent.ValueRW.isCooldownBlockActive, deltaTime);
            UpdateCooldown(ref cooldown.ValueRW.cooldownHealTimer, cooldown.ValueRW.cooldownHealDuration, ref enemyMovementComponent.ValueRW.isCooldownHealActive, deltaTime);
            UpdateCooldown(ref cooldown.ValueRW.cooldownJumpTimer, cooldown.ValueRW.cooldownJumpDuration, ref enemyMovementComponent.ValueRW.isCooldownJumpActive, deltaTime);
            UpdateCooldown(ref cooldown.ValueRW.cooldownStayTimer, cooldown.ValueRW.cooldownStayDuration, ref enemyMovementComponent.ValueRW.isCooldownStayActive, deltaTime);
        }
    }

    private void UpdateCooldown(ref float timer, float duration, ref bool isActive, float deltaTime)
    {
        if (isActive)
        {
            timer -= deltaTime;
            if (timer <= 0)
            {
                timer = duration;
                isActive = false;
            }
        }
        // else
        // {
        //     timer = duration;
        //     isActive = true;
        // }
    }
}