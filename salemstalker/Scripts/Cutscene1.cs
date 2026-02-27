using Godot;
using System;

public partial class Cutscene1 : Node3D
{
	// Called when the node enters the scene tree for the first time.
	private bool _fading = true;
	public async override void _Ready()
    {
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		_fading = false;
        GetNode<AnimationPlayer>("AnimationPlayer2").Play("CameraAction");
		GetNode<AnimationPlayer>("AnimationPlayer").Play("CubeAction");
		await ToSignal(GetTree().CreateTimer(10), "timeout");
		_fading = true;
		await ToSignal(GetTree().CreateTimer(3), "timeout");
		GetTree().ChangeSceneToFile("res://Scenes/newWorld.tscn");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (_fading){GetNode<ColorRect>("Fade").Color = GetNode<ColorRect>("Fade").Color.Lerp(new Color(0,0,0,1), (float)delta*2);}
		else{GetNode<ColorRect>("Fade").Color = GetNode<ColorRect>("Fade").Color.Lerp(new Color(0,0,0,0), (float)delta/2);}
    }
}
