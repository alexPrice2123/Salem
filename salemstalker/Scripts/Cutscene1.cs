using Godot;
using System;

public partial class Cutscene1 : Node3D
{
	// Called when the node enters the scene tree for the first time.
	private bool _fading = true;
	private bool _ready = false;
	public async override void _Ready()
    {
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		_fading = false;
		_ready = true;
        GetNode<AnimationPlayer>("AnimationPlayer2").Play("CameraAction");
		GetNode<AnimationPlayer>("AnimationPlayer").Play("CubeAction");
		await ToSignal(GetTree().CreateTimer(10), "timeout");
		_fading = true;
		
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (_fading){GetNode<ColorRect>("Fade").Color = GetNode<ColorRect>("Fade").Color.Lerp(new Color(0,0,0,1), (float)delta*2);}
		else{GetNode<ColorRect>("Fade").Color = GetNode<ColorRect>("Fade").Color.Lerp(new Color(0,0,0,0), (float)delta/2);}
		if (_ready && _fading && GetNode<ColorRect>("Fade").Color.A > 0.95f)
		{
			_ready = false;
			GetTree().ChangeSceneToFile("res://Scenes/newWorld.tscn");
		}
		if (Input.IsActionJustPressed("dash") && !_fading)
        {
            _fading = true;
        }
    }
}
