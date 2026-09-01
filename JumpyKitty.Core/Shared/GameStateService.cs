namespace JumpyKitty.Core.Shared;

internal class GameStateService
{
    public bool RestartRequested { get; private set; } = false;

    public void HasRestarted() => RestartRequested = false;
    public void RestartGame() => RestartRequested = true;
}
