using Unity.Entities;

    public struct Target: IComponentData
    {
        public bool isJumpTarget;
        public bool isAoeTarget;
    }
