using Godot;
using System;
using System.Collections;

public partial class pullOrb : RigidBody3D
{
	// Called when the node enters the scene tree for the first time.
	public Player3d _playerOrb;
	public float _damageOrb;
	private double _count = 0;
	private Vector3 _goalSize = new Vector3(0, 0, 0);
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public async override void _Ready()
    {
        GetNode<GpuParticles3D>("HitRange").Emitting = true;
		await ToSignal(GetTree().CreateTimer(0.5), "timeout");
		_goalSize = new Vector3(9, 0.3f, 9);
		GetNode<GpuParticles3D>("Orb/Magic").Emitting = true;
		await ToSignal(GetTree().CreateTimer(0.25), "timeout");
		GetNode<Area3D>("Attackbox").SetDeferred("monitoring", true);
    }
	public override void _Process(double delta)
    {
		_count += delta;
		GetNode<MeshInstance3D>("Orb").Scale = GetNode<MeshInstance3D>("Orb").Scale.Lerp(_goalSize, (float)delta*3);
		if (_count > 5)
        {
			Close();
        }
		_playerOrb._pullSpeed = (5-(float)_count)*2;
    }

	public async void _on_attackbox_area_entered(Node3D body)
	{
		if (body.IsInGroup("PlayerHurtbox"))
        {
         	_playerOrb._pullLocation = GlobalPosition;
			_count = 0;
			_playerOrb._pullSpeed = 10;
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
		await ToSignal(GetTree().CreateTimer(0.5), "timeout");
		GetNode<GpuParticles3D>("Orb/Magic").Emitting = false;
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		QueueFree();
    }
}
