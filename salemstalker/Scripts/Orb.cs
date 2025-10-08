using Godot;
using System;

public partial class Orb : RigidBody3D
{
	// Called when the node enters the scene tree for the first time.
	public void Shoot(Vector3 direction)
	{
		ApplyCentralImpulse(direction * 5f);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
