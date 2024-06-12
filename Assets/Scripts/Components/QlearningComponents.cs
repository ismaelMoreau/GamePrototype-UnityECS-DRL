using Unity.Entities;

public struct QtableComponent: IComponentData
{

    public float forward, backward, stepRight, stepLeft, dash, block, heal, jump, stay;
    public int indexOfQtableComponent;

}
public struct QtableRewardComponent: IComponentData
{

    public float reward;
}