using Godot;
using System;

public partial class Cutscene1 : Node3D
{
	// Called when the node enters the scene tree for the first time.
	private bool _fading = true;
	private bool _ready = false;
	[Export] public PackedScene TargetScene { get; set; }
	public async override void _Ready()
    {
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		_fading = false;
		_ready = true;
        GetNode<AnimationPlayer>("AnimationPlayer2").Play("CameraAction");
		GetNode<AnimationPlayer>("AnimationPlayer").Play("CubeAction");
		await ToSignal(GetTree().CreateTimer(10), "timeout");
		_fading = true;
		GetNode<Control>("Fade/Control").Visible = true;
		
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (_fading){GetNode<ColorRect>("Fade").Modulate = GetNode<ColorRect>("Fade").Modulate.Lerp(new Color(1,1,1,1), (float)delta*2);}
		else{GetNode<ColorRect>("Fade").Modulate = GetNode<ColorRect>("Fade").Modulate.Lerp(new Color(1,1,1,0), (float)delta/2);}
		if (_ready && _fading && GetNode<ColorRect>("Fade").Modulate.A > 0.95f)
		{
			_ready = false;
			if (TargetScene != null)
			{
				// Change the scene in the SceneTree
				GetTree().ChangeSceneToPacked(TargetScene);
			}
		}
		if (Input.IsActionJustPressed("dash") && !_fading)
        {
            _fading = true;
			GetNode<Control>("Fade/Control").Visible = true;
        }
    }

	
}
