using Godot;
using System;
using System.Collections.Generic;
using nWins.Lib.Core;
using nWins.Lib.Settings;

public class CreateGameSidebar : Node2D
{
	private static readonly Dictionary<GameSide, string> _sideSelectionLabels = new Dictionary<GameSide, string>()
	{
		{ GameSide.None,   "Random"     },
		{ GameSide.SideA,  "1st Action" },
		{ GameSide.SideB,  "2nd Action" },
	};

	private static readonly Dictionary<AgentType, string> _opponentSelectionLabels = new Dictionary<AgentType, string>()
	{
		{ AgentType.Random,      "Random"             },
		{ AgentType.SimpleQL,    "Naive Q-Learning"   },
		{ AgentType.DoubleQL,    "Double Q-Learning"  },
		{ AgentType.DynaQL,      "Dynamic Q-Learning" },
		{ AgentType.SarsaLambda, "SARSA Lambda"       },
	};

	private GameSide DEFAULT_SIDE = GameSide.SideA;
	private AgentType DEFAULT_OPPONENT = AgentType.Random;

	public GameSide PreferredSide => (GameSide?)_sideSelection?.Selected ?? DEFAULT_SIDE;
	public AgentType SelectedOpponent => (AgentType?)_opponentSelection?.Selected ?? DEFAULT_OPPONENT;

	private OptionButton _sideSelection;
	private OptionButton _opponentSelection;

	public event EventHandler NewGameStarted;

	private void onOkClicked() => NewGameStarted?.Invoke(null, null);

	private void onDefaultClicked()
	{
		_sideSelection.Select((int)DEFAULT_SIDE);
		_opponentSelection.Select((int)DEFAULT_OPPONENT);
	}

	public override void _Draw()
	{
		// create a side selection dropdown menu
		_sideSelection = new OptionButton();
		foreach (var side in _sideSelectionLabels.Keys) { _sideSelection.AddItem(_sideSelectionLabels[side], (int)side); }
		_sideSelection.SetPosition(new Vector2(200, 20));
		_sideSelection.Select((int)DEFAULT_SIDE);

		var sideSelectionLabel = new Label() { Text = "Preferred Side:" };
		sideSelectionLabel.SetPosition(new Vector2(20, 20));

		AddChild(sideSelectionLabel);
		AddChild(_sideSelection);

		// create a opponent selection dropdown menu
		_opponentSelection = new OptionButton();
		foreach (var opp in _opponentSelectionLabels.Keys) { _opponentSelection.AddItem(_opponentSelectionLabels[opp], (int)opp); }
		_opponentSelection.SetPosition(new Vector2(200, 60));
		_opponentSelection.Select((int)DEFAULT_OPPONENT);

		var opponentSelectionLabel = new Label() { Text = "AI Opponent:" };
		opponentSelectionLabel.SetPosition(new Vector2(20, 60));

		AddChild(opponentSelectionLabel);
		AddChild(_opponentSelection);

		// create OK / Default buttons and bind event handlers
		var okButton = new Button() { Text = "New Game" };
		okButton.SetPosition(new Vector2(120, 100));
		okButton.Connect("pressed", this, nameof(onOkClicked));

		var defaultButton = new Button() { Text = "Default" };
		defaultButton.SetPosition(new Vector2(220, 100));
		defaultButton.Connect("pressed", this, nameof(onDefaultClicked));

		AddChild(okButton);
		AddChild(defaultButton);
	}
}
