using JumpyKitty.Core.Background;
using JumpyKitty.Core.Platforms;
using JumpyKitty.Core.Player;
using JumpyKitty.Core.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ECS;
using MonoGame.Extended.Input;
using MonoGame.Extended.Screens;

namespace JumpyKitty.Core.Screens;

internal class GamePlayScreen : GameScreen
{
    private readonly BackgroundSystem _backgroundSystem;
    private readonly BoundingBoxSystem _boundingBoxSystem;
    private readonly CameraSystem _cameraSystem;
    private readonly CollisionSystem _collisionSystem;
    private readonly JumpSystem _jumpSystem;
    private readonly PauseScreen _pauseScreen;
    private readonly PhysicsSystem _physicsSystem;
    private readonly PlatformSystem _platformSystem;
    private readonly PlayerControlSystem _playerControlSystem;
    private readonly PlayerSpawnSystem _playerSpawnSystem;
    private readonly ScreenManager _screenManager;
    private readonly SpriteDrawingSystem _spriteDrawingSystem;
    private World? _world;

    public GamePlayScreen(
        BackgroundSystem backgroundSystem,
        BoundingBoxSystem boundingBoxSystem,
        CameraSystem cameraSystem,
        CollisionSystem collisionSystem,
        Game game,
        JumpSystem jumpSystem,
        PauseScreen pauseScreen,
        PhysicsSystem physicsSystem,
        PlatformSystem platformSystem,
        PlayerControlSystem playerControlSystem,
        PlayerSpawnSystem playerSpawnSystem,
        ScreenManager screenManager,
        SpriteDrawingSystem spriteDrawingSystem) : base(game)
    {
        _backgroundSystem = backgroundSystem;
        _boundingBoxSystem = boundingBoxSystem;
        _cameraSystem = cameraSystem;
        _collisionSystem = collisionSystem;
        _jumpSystem = jumpSystem;
        _pauseScreen = pauseScreen;
        _physicsSystem = physicsSystem;
        _platformSystem = platformSystem;
        _playerControlSystem = playerControlSystem;
        _playerSpawnSystem = playerSpawnSystem;
        _screenManager = screenManager;
        _spriteDrawingSystem = spriteDrawingSystem;
    }

    public override void Draw(GameTime gameTime) => _world?.Draw(gameTime);

    public override void LoadContent()
    {
        // Add systems        
        _world = new WorldBuilder()

            // Add the player spawn system first so that
            // it initialises the player before the other systems
            .AddSystem(_playerSpawnSystem)

            // Add the physics system and player control system next
            // so that they run before the other systems            
            .AddSystem(_playerControlSystem)
            .AddSystem(_jumpSystem)
            .AddSystem(_physicsSystem)
            .AddSystem(_platformSystem)

            // Add the bounding box system and collision system next so that
            // collisions are handled after the physics system has run and
            // after the platform system has updated the platforms
            .AddSystem(_boundingBoxSystem)
            .AddSystem(_collisionSystem)

            // Add the background drawing system and sprite drawing
            // system last so that they run after the other systems
            .AddSystem(_backgroundSystem)
            .AddSystem(_spriteDrawingSystem)

            // Add the camera system
            .AddSystem(_cameraSystem)

            // Build the ECS world ;-)
            .Build();

        base.LoadContent();
    }

    public override void UnloadContent()
    {
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

        if (keyboardState.WasKeyPressed(Keys.P))
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
