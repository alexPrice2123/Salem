using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class Monster3d : CharacterBody3D
{
	public const float Speed = 5.0f; //Player speed
	public const float JumpVelocity = 6.5f; //Jump power
	public const float MaxHealth = 5.0f; //Max Health

	private Player3d _player;
	private StandardMaterial3D _material;
	public float _health = MaxHealth;


	public override void _Ready()
	{
		_player = this.GetParent().GetNode<Player3d>("Player_3d");
		_material = GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverlay as StandardMaterial3D;
		GetNode<MeshInstance3D>("MeshInstance3D").SetSurfaceOverrideMaterial(0, _material);
		_material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
		_material.EmissionEnabled = true;
		Color basecolor = _material.AlbedoColor;
		basecolor.A = 0.0f;
		_material.AlbedoColor = basecolor;
	}

	private async void _on_hitbox_area_entered(Area3D body)
	{
		GD.Print(body);
		if (body.IsInGroup("Player"))
		{
			float _damage = _player._damage;
			Color basecolor = _material.AlbedoColor;
			basecolor.A = 0.5f;
			_material.AlbedoColor = basecolor;
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			basecolor.A = 0.0f;
			_material.AlbedoColor = basecolor;
			GD.Print(_damage);
			_health -= _damage;
			if (_health <= 0){
				QueueFree();
			}
		}
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
	//Color basecolor = _material.AlbedoColor;
	//	basecolor.A = 1.0f;
	//	_material.AlbedoColor = basecolor;