using Unity.Entities;
using Random= Unity.Mathematics.Random;
public struct SkillsConfig: IComponentData
{
    public Entity PrefabAoe;
    public float EffectRadius;
    public float Damage;
    public double LastUsedTime;
    public float Cooldown;

}
public class ConfigManaged : IComponentData
{
    public UIController UIController;
}
public struct ConfigQlearnGrid : IComponentData
{
    public int height;
    public int width;

    public float cellSize;
}
public struct ConfigQlearn : IComponentData
{
    public float sartingEpsilon;
    public float alpha;
    public float gamma;

    public Random random;

    public bool isInitQlearning;

}
public struct HitTriggerConfigComponent : IComponentData
{
    public float triggerCooldownTimer;
    public float hitTriggerCooldownDuration;
}