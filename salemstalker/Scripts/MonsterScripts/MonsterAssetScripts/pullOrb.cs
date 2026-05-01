using Godot;
using System;
using System.Collections;

public partial class pullOrb : RigidBody3D
{
	// Called when the node enters the scene tree for the first time.
	public Player3d _playerOrb;
	public float _damageOrb;
	private double _count = 0;
	private Vector3 _goalSize = new Vector3(9, 0.3f, 9);
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		_count += delta;
		GetNode<MeshInstance3D>("Orb").Scale = GetNode<MeshInstance3D>("Orb").Scale.Lerp(_goalSize, (float)delta*3);
		if (_count > 5)
        {
			Close();
        }
    }

	public async void _on_attackbox_area_entered(Node3D body)
	{
		if (body.IsInGroup("PlayerHurtbox"))
        {
         	_playerOrb._pullLocation = GlobalPosition;
			_count = 0;
        }
	}

	public async void _on_attackbox_area_exited(Node3D body)
	{
		if (body.IsInGroup("PlayerHurtbox"))
        {
         	_playerOrb._pullLocation = new Vector3(0,-67,0);
        }
	}

	private async void Close()
    {
		_goalSize = new Vector3(0, 0, 0);
		GetNode<Area3D>("Attackbox").SetDeferred("monitoring", false);
		_playerOrb.RangedDamaged(0, this, "PullOrbOff");
		await ToSignal(GetTree().CreateTimer(2.5), "timeout");
		QueueFree();
    }
}
