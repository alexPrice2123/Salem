using Godot;
using System;

public partial class NpcVillager : CharacterBody3D
{
	public const float speed = 5.0f;


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
	
	private void _on_hitbox_body_entered(Node3D body)
	{
		if (body.IsInGroup("Monster"))
		{
			
		}
	}
}
