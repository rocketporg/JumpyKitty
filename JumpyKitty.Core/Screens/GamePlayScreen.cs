using JumpyKitty.Core.Platforms;
using JumpyKitty.Core.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ECS;
using MonoGame.Extended.Input;
using MonoGame.Extended.Screens;

namespace JumpyKitty.Core.Screens;

internal class GamePlayScreen : GameScreen
{
    private readonly CameraSystem _cameraSystem = default!;
    private readonly PauseScreen _pauseScreen = default!;
    private readonly PlatformSystem _platformSystem = default!;
    private readonly ScreenManager _screenManager = default!;
    private readonly SpriteDrawingSystem _spriteDrawingSystem = default!;
    private World? _world;

    public GamePlayScreen(
        CameraSystem cameraSystem,
        Game game,
        PauseScreen pauseScreen,
        PlatformSystem platformSystem,
        ScreenManager screenManager,
        SpriteDrawingSystem spriteDrawingSystem) : base(game)
    {
        _cameraSystem = cameraSystem;
        _pauseScreen = pauseScreen;
        _platformSystem = platformSystem;
        _screenManager = screenManager;
        _spriteDrawingSystem = spriteDrawingSystem;
    }

    public override void Draw(GameTime gameTime) => _world?.Draw(gameTime);

    public override void LoadContent()
    {
        // Add systems        
        _world = new WorldBuilder()

            .AddSystem(_platformSystem)
            .AddSystem(_spriteDrawingSystem)

            // Add the camera system
            .AddSystem(_cameraSystem)

            // Build the ECS world ;-)
            .Build();

        base.LoadContent();
    }

    public override void UnloadContent()
    {
        // We need to reset the physics world and dispose of the ECS world when we unload the
        // content for this screen, otherwise when we come back to this screen the physics world
        // will still have all the bodies in it...
        //_physicsService.ResetWorld();

        // ...and the ECS world will still have all the entities
        // in it which will cause all sorts of weird issues!
        _world?.Dispose();

        base.UnloadContent();
    }

    public override void Update(GameTime gameTime)
    {
        // Update the world, this will run all the systems in the world which
        // will update the game state and do all the drawing
        _world?.Update(gameTime);

        // Check to see if the pause button was pressed, if it was we
        // show the pause screen (which will pause the game until it's closed)
        var keyboardState = KeyboardExtended.GetState();

        if (keyboardState.WasKeyPressed(Keys.Space))
        {
            // When the game is paused, we still draw the gameplay screen
            // but we don't update anything (i.e. paused ;-)
            DrawWhenInactive = true;
            UpdateWhenInactive = false;

            // Now show the pause screen...
            _screenManager.ShowScreen(_pauseScreen);
        }
    }
}
