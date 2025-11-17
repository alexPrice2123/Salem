using Godot;
using System;

public partial class underBrush : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private float _distance;
	private float _attackOffset = 0.437f;
	public float _currentAttackOffset = 0f;
	private float _countDown = 5f;
	public override void _Ready()
	{
		Speed = 5f;             // Movement speed
		MaxHealth = 75.0f;         // Maximum monster health
		Range = 30.0f;            // Detection range for chasing
		SpawnDistance = 100;    // Distance from player before despawning
		BaseDamage = 10.0f;
		WanderRange = 50;
		AttackSpeed = 1.33f;
		AttackRange = 1f;
		Monster = this;
		Chaser = true;
		MoveWhileAttack = true;
		Initialization();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EveryFrame(delta);
		if (_health <= 0)
		{
			_player.MonsterKilled("underBrush", Biome);
			if (Debug == true)
			{
				if (GetParent().GetParent() is DebugHut dh) { dh._shouldSpawn = true; }
			}
			QueueFree(); // Destroy monster when health hits zero
		}
		_countDown -= (float)delta;
		if (_countDown <= 0f)
        {
			_currentAttackOffset = 0f;
        }
		RotateFunc(delta);
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
			_player.Damaged(BaseDamage + _damageOffset, this as Monster3d, "StaminaDrain");
			_currentAttackOffset += _attackOffset;
			if (_player._blocking == false)
            {
				_countDown = 5f;
				if (_currentAttackOffset >= 1.311f) { _currentAttackOffset = 1.311f; }   
            }
			_attackBox.Disabled = true;
			_hasHit = true;
		}
	}

	public async void Attack()
	{
		_hasHit = false;
		_attackAnim = true;
		await ToSignal(GetTree().CreateTimer(1.6), "timeout");
		_speedOffset = 2.5f;
		_attackBox.GetParent<Area3D>().Monitoring = true;
        await ToSignal(GetTree().CreateTimer(0.2), "timeout");
		_attackBox.GetParent<Area3D>().Monitoring = false;
		_canAttack = false;
		await ToSignal(GetTree().CreateTimer(0.7), "timeout");
		_attackAnim = false;
		await ToSignal(GetTree().CreateTimer(AttackSpeed - _currentAttackOffset), "timeout");
        _canAttack = true;
	}
}
