using Godot;
using System;

public partial class DemoHandler : Node3D
{
	public bool _marthaDone = false;
	public bool _lukasDone = false;
	public bool _dillonDone = false;
	private bool _done = false;
	[Export]
	public string _endDemoText;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (_marthaDone && _lukasDone && _dillonDone && !_done)
        {
            _done = true;
			GetNode<Ui>("Player_3d/UI")._loadingGoal = -1;
			GetNode<Label>("Player_3d/UI/Loading/Text").Text = _endDemoText;
			GetNode<Player3d>("Player_3d")._dead = true;
			GetNode<Label>("Player_3d/UI/Loading/Q").Visible = true;
        }
    }
}
