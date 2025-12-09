using Godot;
using System;

public partial class PauseMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public Player3d _player;
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
}
