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
    private readonly DeathSystem _deathSystem;
    private readonly GameOverScreen _gameOverScreen;
    private readonly GameStateService _gameStateService;
    private readonly JumpSystem _jumpSystem;
    private readonly PauseScreen _pauseScreen;
    private readonly PhysicsSystem _physicsSystem;
    private readonly PlatformSystem _platformSystem;
    private readonly PlayerControlSystem _playerControlSystem;
    private readonly PlayerService _playerService;
    private readonly PlayerSpawnSystem _playerSpawnSystem;
    private readonly ScreenManager _screenManager;
    private readonly SpriteDrawingSystem _spriteDrawingSystem;
    private readonly WaterSystem _waterSystem;
    private World? _world;

    public GamePlayScreen(
        BackgroundSystem backgroundSystem,
        BoundingBoxSystem boundingBoxSystem,
        CameraSystem cameraSystem,
        CollisionSystem collisionSystem,
        DeathSystem deathSystem,
        Game game,
        GameOverScreen gameOverScreen,
        GameStateService gameStateService,
        JumpSystem jumpSystem,
        PauseScreen pauseScreen,
        PhysicsSystem physicsSystem,
        PlatformSystem platformSystem,
        PlayerControlSystem playerControlSystem,
        PlayerService playerService,
        PlayerSpawnSystem playerSpawnSystem,
        ScreenManager screenManager,
        SpriteDrawingSystem spriteDrawingSystem,
        WaterSystem waterSystem) : base(game)
    {
        _backgroundSystem = backgroundSystem;
        _boundingBoxSystem = boundingBoxSystem;
        _cameraSystem = cameraSystem;
        _collisionSystem = collisionSystem;
        _deathSystem = deathSystem;
        _gameOverScreen = gameOverScreen;
        _gameStateService = gameStateService;
        _jumpSystem = jumpSystem;
        _pauseScreen = pauseScreen;
        _physicsSystem = physicsSystem;
        _platformSystem = platformSystem;
        _playerControlSystem = playerControlSystem;
        _playerService = playerService;
        _playerSpawnSystem = playerSpawnSystem;
        _screenManager = screenManager;
        _spriteDrawingSystem = spriteDrawingSystem;
        _waterSystem = waterSystem;
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
            .AddSystem(_deathSystem)

            // Add the bounding box system and collision system next so that
            // collisions are handled after the physics system has run and
            // after the platform system has updated the platforms
            .AddSystem(_boundingBoxSystem)
            .AddSystem(_collisionSystem)

            // Add the background drawing system and sprite drawing system last
            // so that they run after the other systems. Note that we place the
            // water system last, so that the water is drawn 'over' the other
            // background items
            .AddSystem(_backgroundSystem)
            .AddSystem(_spriteDrawingSystem)
            .AddSystem(_waterSystem)

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
        
        // If the player has requested a restart, we need to reset the game state and systems
        if (_gameStateService.RestartRequested)
        {
            // Flag to indicate that the player has restarted the game, so we can reset the game state
            _gameStateService.HasRestarted();
            _playerService.Respawn();

            // Reset the game state and systems
            UnloadContent();
            LoadContent();            
        }

        // Has the player died?
        else if (_playerService.HasDied)
        {
            // Still draw the gameplay screen, but don't update anything during game over
            DrawWhenInactive = true;
            UpdateWhenInactive = false;

            // Show the game over screen, which will allow the player to restart the game
            _screenManager.ShowScreen(_gameOverScreen);
        }
    }
}
