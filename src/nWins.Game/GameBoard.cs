using Godot;
using System;
using System.Linq;
using nWins.Lib.Core;
using nWins.Lib.Settings;
using nWins.Lib.Session;
using nWins.Lib.Agent;
using nWins.Lib.Factory;

public class GameBoard : Node2D
{
	// define height and width of the game board
	private const int ROWS = 4;
	private const int COLUMNS = 5;

    // define color brushes
	private static readonly Color BOARD_BACKGROUND = new Color("#0000FF");
	private static readonly Color STONE_WHITE = new Color("#FFFFFF");
	private static readonly Color STONE_SIDE_A = new Color("#D50B30");
	private static readonly Color STONE_SIDE_B = new Color("#F7F00D");

	private static readonly CreateGameSidebar _sidebar = new CreateGameSidebar();

    // initialize game settings
    private static readonly GameSettings _settings = new GameSettings() {
		Rows = ROWS,
		Columns = COLUMNS,
		StonesToConnect = 4
	};

	// define game session
	private OnlineGameSession _session;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// create async player to put human actions
		onNewGame(null, null);
	}

	private void onColumnSelected(int column)
	{
		// ignore actions when it's not the user's turn or the game is already over
		if (_session.Game.ActingSide != _session.PlayerSide || _session.IsGameOver) { return; }

		// create the action to be applied and make sure that it is valid (otherwise abort)
		var action = new GameAction((uint)column, _session.PlayerSide);
		if (!_session.Game.PossibleActions.Contains(action)) { return; }

		// apply the player's action and the AI opponent's action (if the player's action was non-terminal)
		var log = _session.ApplyPlayerAction(action);

		// repaint the game board
		Update();

        // check if the game is over (either by the own or the opponent's action)
		if (_session.IsGameOver)
		{
			// show the game result as popup window
			var result = _session.Game.Result;
			var dlg = new AcceptDialog();
			dlg.DialogText = result.AsText();
			AddChild(dlg);
			dlg.PopupCentered(new Vector2(150, 70));
		}
	}

	private void onNewGame(object sender, EventArgs e)
	{
		// create a simple QL agent from settings
		var sessionSettings = new GameSessionSettings() {
			GameConfig = _settings,
			AIOpponent = _sidebar.SelectedOpponent,
			PreferredSide = _sidebar.PreferredSide
		};

		// initialize game session
		_session = new OnlineGameSession(sessionSettings);

		// repaint the game board
		Update();
	}

	public override void _Draw()
	{
        // draw parameters
		const int STONE_SIZE = 100;
		const int STONE_DIAMETER = 40;
		const int PADDING = 20;
		const int X_STONES_OFFSET = 60;
		const int Y_STONES_OFFSET = 60;
		const int BUTTON_PADDING = 5;
		const int X_BUTTON_OFFSET = 26;

		// display the n-wins game board as rectangle
		int boardX = COLUMNS * STONE_SIZE + PADDING;
		int boardY = ROWS * STONE_SIZE + PADDING;
		DrawRect(new Rect2(0, 0, boardX, boardY), BOARD_BACKGROUND);

		// draw n-wins stones (colored circles)
		for (int column = 0; column < COLUMNS; column++)
		{
			for (int row = 0; row < ROWS; row++)
			{
				// determine the stone's color
				int fieldIndex = (ROWS - row - 1) * COLUMNS + column;
				var field = _session.Game.CurrentState.Fields[fieldIndex];
				var color = field == GameSide.None ? STONE_WHITE : (field == GameSide.SideA ? STONE_SIDE_A : STONE_SIDE_B);

                // display the stone as circle
				int circleX = X_STONES_OFFSET + column * STONE_SIZE;
				int circleY = Y_STONES_OFFSET + row * STONE_SIZE;
				DrawCircle(new Vector2(circleX, circleY), STONE_DIAMETER, color);
			}
		}

        // put buttons for column selection
        for (int column = 0; column < COLUMNS; column++)
		{
			// determine button position
			int buttonX = X_BUTTON_OFFSET + column * STONE_SIZE;
			int buttonY = boardY + PADDING + BUTTON_PADDING;

            // create the button, spawn it at its position and register a click event
			var button = new Button() { Text = "Put Stone" };
			button.SetPosition(new Vector2(buttonX, buttonY));
			button.Connect("pressed", this, nameof(onColumnSelected), new Godot.Collections.Array() { column });

            // apply the button to the node tree
			AddChild(button);
		}

		// create game creation sidebar
		_sidebar.GlobalPosition = new Vector2(570, 20);
		_sidebar.NewGameStarted += onNewGame;
		AddChild(_sidebar);
	}
}
