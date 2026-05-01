using Godot;
using System;
using System.Collections.Generic;

public partial class PauseMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public Player3d _player;
	private Dictionary<int,DisplayServer.WindowMode> windowMode;
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_quit_pressed()
    {
		GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/titlescreen.tscn");
		QueueFree();
    }
	private void _on_resume_pressed()
    {
		GetTree().Paused = false;
		QueueFree();
		_player.UnPause();
    }

	private void _on_windowMode_up()
	{
		DisplayServer.WindowSetMode(windowMode[GetNode<OptionButton>("OptionButton").Selected]);
	}
}
