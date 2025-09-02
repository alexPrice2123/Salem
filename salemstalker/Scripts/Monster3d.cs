using Godot;
using System;

public partial class Monster3d : CharacterBody3D
{
	public const float Speed = 10.0f; //Player speed
	public const float JumpVelocity = 6.5f; //Jump power

	public override void _Ready()
	{
		//
	}

	public override void _PhysicsProcess(double delta) //Event tick; happens every frame
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
		Velocity = velocity;
		MoveAndSlide();
	}
}
