using Godot;
using System;

public partial class hollowBrute : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private float _distance;
	public override void _Ready()
	{
		Speed = 2.5f;             // Movement speed
		MaxHealth = 100.0f;         // Maximum monster health
		Range = 30.0f;            // Detection range for chasing
		SpawnDistance = 100;    // Distance from player before despawning
		BaseDamage = 35.0f;
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
		if (_health <= 0)
		{
			_player.MonsterKilled("hollowBrute", Biome);
			if (Debug == true)
            {
				if (GetParent().GetParent() is DebugHut dh){ dh._shouldSpawn = true; }
            }
			QueueFree(); // Destroy monster when health hits zero
		}
		if (_attackAnim == false) { RotateFunc(delta); }
		else { _targetVelocity = Vector3.Zero; }
	}

	private void RotateFunc(double delta)
    {
        if (Mathf.RadToDeg(_lookDirection.GlobalRotation.Y) >= 175 || Mathf.RadToDeg(_lookDirection.GlobalRotation.Y) <= -175)
        {
            GlobalRotation = new Vector3(GlobalRotation.X, _lookDirection.GlobalRotation.Y, GlobalRotation.Z);
        }
        else
        {
            float newRotation = Mathf.Lerp(GlobalRotation.Y, _lookDirection.GlobalRotation.Y, (float)delta * 10f);
            GlobalRotation = new Vector3(GlobalRotation.X, newRotation, GlobalRotation.Z);
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
			_player.Damaged(BaseDamage + _damageOffset, this as Monster3d, "None");
			_attackBox.Disabled = true;
			_hasHit = true;
		}
	}

	public async void Attack()
	{
		_hasHit = false;
		_attackAnim = true;
		_targetVelocity = Vector3.Zero;
		await ToSignal(GetTree().CreateTimer(1.5), "timeout");
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
