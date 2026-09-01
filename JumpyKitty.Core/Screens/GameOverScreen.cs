using JumpyKitty.Core.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Screens;

namespace JumpyKitty.Core.Screens;

internal class GameOverScreen : GameScreen
{
    private readonly ContentManager _contentManager;
    private SpriteFont _font = default!;
    private readonly GameStateService _gameStateService;
    private readonly ScreenManager _screenManager;
    private readonly SpriteBatch _spriteBatch;

    public GameOverScreen(
        Game game,
        ScreenManager screenManager,
        ContentManager contentManager,
        SpriteBatch spriteBatch,
        GameStateService gameStateService) : base(game)
    {
        _screenManager = screenManager;
        _contentManager = contentManager;
        _spriteBatch = spriteBatch;
        _gameStateService = gameStateService;
    }

    public override void Draw(GameTime gameTime)
    {
        var gameOverText = "Game Over";
        var gameOverTextSize = _font.MeasureString(gameOverText);
        var gameOverTextOrigin = new Vector2(gameOverTextSize.X / 2, gameOverTextSize.Y / 2);
        var gameOverTextPosition = new Vector2(_spriteBatch.GraphicsDevice.Viewport.Width / 2, _spriteBatch.GraphicsDevice.Viewport.Height / 2 - 150);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _spriteBatch.DrawString(
            spriteFont: _font,
            text: gameOverText,
            position: gameOverTextPosition,
            color: Color.Black,
            rotation: 0f,
            origin: gameOverTextOrigin,
            scale: 8f,
            effects: SpriteEffects.None,
            layerDepth: 1
        );

        var scoreText = "Your score: ";// + _scoreSystem.LastScore.ToString();
        var scoreTextSize = _font.MeasureString(scoreText);
        var scoreTextOrigin = new Vector2(scoreTextSize.X / 2, scoreTextSize.Y / 2);
        var scoreTextPosition = new Vector2(gameOverTextPosition.X, gameOverTextPosition.Y + 175);

        _spriteBatch.DrawString(
            spriteFont: _font,
            text: scoreText,
            position: scoreTextPosition,
            color: Color.White,
            rotation: 0f,
            origin: scoreTextOrigin,
            scale: 4f,
            effects: SpriteEffects.None,
            layerDepth: 1
        );

        var highScoreText = "High score: ";// + _scoreSystem.HighScore.ToString();
        var highScoreTextSize = _font.MeasureString(highScoreText);
        var highScoreTextOrigin = new Vector2(highScoreTextSize.X / 2, highScoreTextSize.Y / 2);
        var highScoreTextPosition = new Vector2(scoreTextPosition.X, scoreTextPosition.Y + 75);

        _spriteBatch.DrawString(
            spriteFont: _font,
            text: highScoreText,
            position: highScoreTextPosition,
            color: Color.White,
            rotation: 0f,
            origin: highScoreTextOrigin,
            scale: 2.5f,
            effects: SpriteEffects.None,
            layerDepth: 1
        );

        var tapToPlayText = "Tap to play again...";
        var tapToPlayTextSize = _font.MeasureString(tapToPlayText);
        var tapToPlayTextOrigin = new Vector2(tapToPlayTextSize.X / 2, tapToPlayTextSize.Y / 2);
        var tapToPlayTextPosition = new Vector2(highScoreTextPosition.X, highScoreTextPosition.Y + 225);

        _spriteBatch.DrawString(
            spriteFont: _font,
            text: tapToPlayText,
            position: tapToPlayTextPosition,
            color: Color.White,
            rotation: 0f,
            origin: tapToPlayTextOrigin,
            scale: 4f,
            effects: SpriteEffects.None,
            layerDepth: 1
        );

        _spriteBatch.End();
    }

    public override void LoadContent()
    {
        _font = _contentManager.Load<SpriteFont>("Fonts/Font");

        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        var keyboardState = KeyboardExtended.GetState();

        // If the restart button was pressed, we return to the gameplay screen and reset the game state
        if (keyboardState.WasKeyPressed(Keys.Space))
        {
            // Reset the game state and respawn the player            
            _gameStateService.RestartGame();

            // Close the Game Over screen...
            _screenManager.CloseScreen();
        }
    }
}
