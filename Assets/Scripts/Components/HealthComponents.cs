using Unity.Entities;

public struct HealthComponent : IComponentData 
{
    public float currentHealth;
    public float maxHealth; 
}
