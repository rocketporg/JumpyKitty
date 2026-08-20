namespace JumpyKitty.Core.Shared;

internal class EntityStateComponent
{
    public bool IsAlive => State == EntityState.Alive || State == EntityState.Invunerable;

    public EntityState State = EntityState.Alive;
}
