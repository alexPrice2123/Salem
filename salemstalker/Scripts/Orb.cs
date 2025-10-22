using Godot;
using System;
using System.Collections;

public partial class Orb : RigidBody3D
{
	// Called when the node enters the scene tree for the first time.
	public Player3d _playerOrb;
	public float _damageOrb;
	private int _count = 0;
	public void Shoot(float speed)
	{
		LookAt(new Vector3(_playerOrb.GlobalPosition.X, GlobalPosition.Y, _playerOrb.GlobalPosition.Z), Vector3.Up);
		ApplyCentralImpulse(-GlobalTransform.Basis.Z.Normalized() * speed);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		_count += 1;
		if (_count > 250)
        {
			QueueFree();
        }
    }

	public void _on_attackbox_area_entered(Node3D body)
	{
		if (body.IsInGroup("Player") && body.Name == "Hurtbox")
		{
			_playerOrb.RangedDamaged(_damageOrb, this);
			//GetNode<CollisionShape3D>("Area3D/CollisionShape3D").Disabled = true;
			LinearVelocity = Vector3.Zero;
			AngularVelocity = Vector3.Zero;
			GetNode<GpuParticles3D>("Boom").Emitting = true;
			GetNode<GpuParticles3D>("Magic").Emitting = false;
			GetNode<MeshInstance3D>("Orb").Visible = false;
		}
	}
}
