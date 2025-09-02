using Godot;
using System;

public partial class Monster3d : CharacterBody3D
{
	public const float Speed = 5.0f; //Player speed
	public const float JumpVelocity = 6.5f; //Jump power

	private Player3d _player;


	public override void _Ready()
	{
		_player = this.GetParent().GetNode<Player3d>("Player_3d");
		StandardMaterial3D material = GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverlay as StandardMaterial3D;
		GetNode<MeshInstance3D>("MeshInstance3D").SetSurfaceOverrideMaterial(0, material);
		material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
		material.EmissionEnabled = true;
		Color basecolor = material.AlbedoColor;
		basecolor.A = 1.0f;
		material.AlbedoColor = basecolor;
	}

	public override void _PhysicsProcess(double delta) //Event tick; happens every frame
	{

		Vector3 direction = (_player.GlobalPosition - GlobalPosition).Normalized();

		direction.Y = 0;

		

		// Add the gravity.
		if (!IsOnFloor())
		{
			direction += GetGravity() * (float)delta;
		}
		Velocity = direction * Speed;
		MoveAndSlide();
	}
}
