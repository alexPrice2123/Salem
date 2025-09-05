using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class Monster3d : CharacterBody3D
{
	public const float Speed = 5.0f; //Player speed
	public const float JumpVelocity = 6.5f; //Jump power
	public const float MaxHealth = 5.0f; //Max Health
	public const float Range = 25.0f; //Range where it can detect the player

	private Player3d _player;
	private StandardMaterial3D _material;
	public float _health = MaxHealth;
    private Vector3 _knockbackVelocity = Vector3.Zero;
	private Vector3 _wanderPos;
	private float _count;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	private NavigationAgent3D _navAgent;
	private Vector3 startPos;

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
		_rng.Randomize();
		_navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		startPos = GlobalPosition;
	}

	private async void _on_hitbox_area_entered(Area3D body)
	{
		GD.Print(body);
		if (body.IsInGroup("Player"))
		{
			float _damage = _player._damage;
			Color basecolor = _material.AlbedoColor;
			basecolor.A = 0.5f;
			Vector3 knockbackDirection = (GlobalPosition - _player.Position).Normalized();
        	_knockbackVelocity = knockbackDirection * _player._knockbackStrength;
			_material.AlbedoColor = basecolor;
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			basecolor.A = 0.0f;
			float randZ = GlobalPosition.Z + _rng.RandiRange(-50, 50);
			float randX = GlobalPosition.X + _rng.RandiRange(-50, 50);
			_wanderPos = new Vector3(randX, 0f, randZ);
			_material.AlbedoColor = basecolor;
			_health -= _damage;
			if (_health <= 0){
				QueueFree();
			}
		}
	}

	public override void _PhysicsProcess(double delta) //Event tick; happens every frame
	{
		_count += 1;
		if (_count >= 250)
		{
			_count = _rng.RandiRange(-100, 50);
			float randZ = startPos.Z + _rng.RandiRange(-50, 50);
			float randX = startPos.X + _rng.RandiRange(-50, 50);
			_wanderPos = new Vector3(randX, 0f, randZ);
		}
		float distance = (_player.GlobalPosition - GlobalPosition).Length();
		if (distance <= Range)
		{
			_navAgent.TargetPosition = _player.GlobalPosition;
			Vector3 nextPoint = _navAgent.GetNextPathPosition();
			Velocity = ((nextPoint - GlobalTransform.Origin).Normalized() * Speed)+_knockbackVelocity;
			Vector3 playerPos = _player.GlobalPosition;
			LookAt(new Vector3(playerPos.X, GlobalPosition.Y, playerPos.Z), Vector3.Up);
		}
		else
		{
			_navAgent.TargetPosition = _wanderPos;
			Vector3 nextPoint = _navAgent.GetNextPathPosition();
			Velocity = ((nextPoint - GlobalTransform.Origin).Normalized() * Speed)+_knockbackVelocity;
			LookAt(new Vector3(_wanderPos.X, GlobalPosition.Y, _wanderPos.Z), Vector3.Up);
		}
		MoveAndSlide();
		_knockbackVelocity = _knockbackVelocity.Lerp(Vector3.Zero, (float)delta * 5.0f);
	}
}


