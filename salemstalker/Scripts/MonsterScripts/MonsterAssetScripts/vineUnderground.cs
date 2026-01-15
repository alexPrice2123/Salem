using Godot;
using System;

public partial class vineUnderground : CharacterBody3D
{
	private NavigationAgent3D _navAgent;
	public Player3d _player;
	public bool _charging = false;
	private bool _attack = false;
	private float _count = 1.5f;
	private float _health = 2f;
	private float _vineCount = 0f;
	public vineTangler _monster;
	private bool _hit = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float distance = (_player.GlobalPosition - GlobalPosition).Length();
		if (_charging == false && _attack == false)
		{
			_navAgent.TargetPosition = _player.GlobalPosition;
			Vector3 nextPoint = _navAgent.GetNextPathPosition();
			Velocity = (nextPoint - GlobalTransform.Origin).Normalized() * 5f;
			if (!IsOnFloor())
			{
				Velocity = new Vector3(Velocity.X, -9.8f, Velocity.Z);
			}
		}
		if ((distance <= 2f || _charging == true) && _attack == false)
		{
			_charging = true;
			Velocity = Vector3.Zero;
			GetNode<GpuParticles3D>("Dirt").Emitting = false;
			GetNode<GpuParticles3D>("Before").Emitting = true;
		}
		if (_charging == true)
        {
            _count -= (float)delta;
        }
		if (_count <= 0 && _attack == false)
		{
			Attack();
		}
		if ((_attack == true && distance <= 2.1f && _vineCount <= 0f) || _hit == true)
		{
			GetNode<Node3D>("VinePoint").LookAt(_player.GlobalPosition, Vector3.Up);
			float vineRange = Mathf.Clamp(distance, 0.05f, 2f);
			if (_vineCount <= 0f)
			{
				_hit = true;
			}
			if (distance > 2f)
			{
				Vector3 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
				_player.GlobalPosition = GlobalPosition + direction * 2f;
				_player.Velocity = Vector3.Zero;
			}
			GetNode<Node3D>("VinePoint/Vine").Scale = new Vector3(0.126f, vineRange * 2.1f, 0.126f);
		}
		else if (_attack == true && _charging == true && _vineCount > 2f && _hit == false)
		{
			GetNode<Node3D>("VinePoint/Vine").Scale = new Vector3(0.126f, GetNode<Node3D>("VinePoint/Vine").Scale.Y - 0.2f, 0.126f);
		}
		if (_attack == true)
		{
			_vineCount += (float)delta;
		}
		if ((GetNode<Node3D>("VinePoint/Vine").Scale.Y <= 0f && _hit == false) || _health <= 0f)
		{
			_monster.VineDied();
			QueueFree();
		}
		GetNode<Node3D>("VinePoint/HitFX").Scale = GetNode<Node3D>("VinePoint/Vine").Scale;
		MoveAndSlide();
	}
	
	private async void _on_area_3d_area_entered(Area3D body)
    {
        if (body.IsInGroup("Weapon"))
        {
			GetNode<Node3D>("VinePoint/HitFX").Visible = true;
			GetNode<Node3D>("VinePoint/Vine").Visible = false;
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

			// Reduce health
			_health -= 1f;

			GetNode<Node3D>("VinePoint/HitFX").Visible = false;
			GetNode<Node3D>("VinePoint/Vine").Visible = true;
        }
    }
	
	private void Attack()
    {
		_attack = true;
		GetNode<GpuParticles3D>("Before").Emitting = false;
		GetNode<GpuParticles3D>("VinePoint/Attack").Emitting = true;
		GetNode<AnimationPlayer>("AnimationPlayer").Play("Attack");
		GetNode<Node3D>("VinePoint").LookAt(_player.GlobalPosition, Vector3.Up);
    }
}
