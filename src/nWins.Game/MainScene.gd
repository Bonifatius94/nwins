extends Node2D

func _ready():

	OS.set_window_size(Vector2(950, 520))

	# load game board and show it
	var script_game_board = load("res://GameBoard.cs")
	var game_board = script_game_board.new()
	game_board.set_position(Vector2(20, 20))
	add_child(game_board)
