using Godot;
using System;

public partial class TheHollow : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private float _distance;
	public override void _Ready()
	{
		Speed = 2.5f;             // Movement speed
		MaxHealth = 100.0f;         // Maximum monster health
		Range = 25.0f;            // Detection range for chasing
		SpawnDistance = 100;    // Distance from player before despawning
		BaseDamage = 45.0f;
		WanderRange = 50;
		AttackSpeed = 2.5f;
		AttackRange = 2f;
		Monster = this;
		Chaser = true;
		Initialization();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EveryFrame(delta);
		_distance = (_player.GlobalPosition - GlobalPosition).Length();
		if (_distance < 5 && _attackException == false)
		{
			_speedOffset = 2.5f;
		}
		else if (_attackException == false)
		{
			_speedOffset = 0f;
		}
		if (_health <= 0)
		{
			_player.MonsterKilled("TheHollow");
			QueueFree(); // Destroy monster when health hits zero
		}
	}

	public void _on_hurtbox_area_entered(Area3D body)
	{
		Damaged(body);
	}

	public void _on_attackbox_area_entered(Node3D body)
	{
		if (body.IsInGroup("Player") && _hasHit == false && body.Name == "Hurtbox")
		{
			_player.Damaged(BaseDamage + _damageOffset, this as Monster3d);
			_attackBox.Disabled = true;
			_hasHit = true;
		}
	}

	public async void Attack()
	{
		_hasHit = false;
		_attackAnim = true;
		_targetVelocity = Vector3.Zero;
		await ToSignal(GetTree().CreateTimer(1.7), "timeout");
		_speedOffset = 2.5f;
		_attackBox.GetParent<Area3D>().Monitoring = true;
        await ToSignal(GetTree().CreateTimer(0.2), "timeout");
		_attackBox.GetParent<Area3D>().Monitoring = false;
		_canAttack = false;
		_attackException = false;
		await ToSignal(GetTree().CreateTimer(0.7), "timeout");
		_attackAnim = false;
		_targetVelocity = Vector3.Zero;
        await ToSignal(GetTree().CreateTimer(AttackSpeed), "timeout");
        _canAttack = true;
	}
}
