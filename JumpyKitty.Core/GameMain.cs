using JumpyKitty.Core.Extensions;
using JumpyKitty.Core.Player;
using JumpyKitty.Core.Screens;
using JumpyKitty.Core.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;
using System;
using System.Reflection;

namespace JumpyKitty.Core;

public class GameMain : Game
{
    // Since we're using 16 bit pixel style graphics we'll use kind
    // 16 bit style 'virtual' resolution which we'll scale later to
    // whatever screen size
    private const int _virtualResolutionWidth = 1080, _virtualResolutionHeight = 1920;

    private CustomRenderTarget _customRenderTarget = default!;
    private readonly GraphicsDeviceManager _graphics;
    private readonly ScreenManager _screenManager;
    private IServiceProvider _serviceProvider = default!;

    public GameMain()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Set initial video config
        _graphics.PreferredBackBufferWidth = _virtualResolutionWidth;
        _graphics.PreferredBackBufferHeight = _virtualResolutionHeight;

        // If we want a different target fps from the default (which in Monogame is 60), then
        // we need to set the target 'time elapsed' we want for the specified target fps        
        TargetElapsedTime = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond / 119));
        IsFixedTimeStep = true;
        InactiveSleepTime = TimeSpan.Zero;

        // No vsync
        _graphics.SynchronizeWithVerticalRetrace = false;

        // Apply changes
        _graphics.ApplyChanges();

        // Add the Monogame.Extended screen manager as per normal...
        _screenManager = new ScreenManager();
        Components.Add(_screenManager);
    }

    private ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // The Monogame Extended screen manager needs the actual 'Game' object, so
        // we'll register it here so we can inject it later on
        services.AddSingleton<Game>(this);

        // We're just going to use a single content manager and inject it as a service
        // whenever we need it. Since we won't use multiple content managers, this way
        // it will cache assets and we just write 'load' as normal to load textures for
        // example and if we load an existing texture, we'll get the cached version ;-)
        services.AddSingleton(Content);

        // Register the graphics device too, so we can also register sprite batch
        // and the container will inject the graphics device for us ;-)
        services.AddSingleton(GraphicsDevice);

        // We're going to inject the SpriteBatch service into various other classes
        // eventually via constructor
        services.AddSingleton<SpriteBatch>();

        // Add the Monogame Extended screen manager too
        services.AddSingleton(_screenManager);

        // Some other services to help with dealing with the player and game state
        services.AddSingleton<GameStateService>();
        services.AddSingleton<PlayerService>();

        // We'll add our custom render target service so we can use our virtual resolution
        // but scale correctly to all different screen sizes easily        
        services.AddSingleton<CustomRenderTarget>(options =>
        {
            var service = new CustomRenderTarget(GraphicsDevice, options.GetRequiredService<SpriteBatch>());
            service.InitialiseRenderDestination(_virtualResolutionWidth, _virtualResolutionHeight);

            return service;
        });

        // Setup our camera and viewport, see link to documentation below
        // https://www.monogameextended.net/docs/features/camera/orthographic-camera/
        services.AddSingleton<OrthographicCamera>(options =>
        {
            // Setup a viewport adapter to handle different screen sizes/aspect ratios
            var viewportAdapter = new BoxingViewportAdapter(
                Window,
                GraphicsDevice,
                _virtualResolutionWidth,
                _virtualResolutionHeight);

            return new OrthographicCamera(viewportAdapter);
        });

        // Add our ECS world        
        services.AddSingleton<WorldBuilder>();

        // This adds all our Monogame.Extended ECS systems (which are in this assembly)
        services.AddAllImplementationsAsSelf<ISystem>(ServiceLifetime.Singleton, Assembly.GetExecutingAssembly());

        // Now add all our screens (which are in this assembly)
        services.AddAllImplementationsAsSelf<Screen>(ServiceLifetime.Singleton, Assembly.GetExecutingAssembly());

        return services.BuildServiceProvider();
    }

    protected override void Draw(GameTime gameTime)
    {
        // Tell the system we want to render to the custom render target
        _customRenderTarget.Begin();

        // Draw all registered drawable game components
        base.Draw(gameTime);

        // Now draw everything as normal        
        //_debuggingService.Draw();

        // Finally, draw the render target to the screen
        _customRenderTarget.Draw();
    }

    protected override void Initialize()
    {
        // Create service collection (not using the Monogame 'container' as it cannot do constructor
        // injection), so instead we're using the standard Microsoft container ;-)        
        _serviceProvider = ConfigureServices();

        // Initialize the screen manager with the service provider so it can resolve screens
        base.Initialize();

        // Now we can use the screen manager to load the first game screen                
        _customRenderTarget = _serviceProvider.GetRequiredService<CustomRenderTarget>();
        //_debuggingService = _serviceProvider.GetRequiredService<DebuggingService>();
    }

    protected override void LoadContent()
    {
        var startingScreen = _serviceProvider.GetRequiredService<GamePlayScreen>();
        _screenManager.ShowScreen(startingScreen);
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardExtended.Update();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // All update logic is now handled by the screen management service        
        base.Update(gameTime);
    }
}
