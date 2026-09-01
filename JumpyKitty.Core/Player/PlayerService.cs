namespace JumpyKitty.Core.Player;

internal class PlayerService
{
    public bool HasDied { get; private set; } = false;    

    public void Die() => HasDied = true;
    public void Respawn() => HasDied = false;
}
