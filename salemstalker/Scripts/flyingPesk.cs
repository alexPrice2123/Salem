using Godot;
using System;

public partial class flyingPesk : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private float _distance;
	private float _attackingRange = 1.2f;
	private float _attackingDistance = 10f;
	private bool _fleeing = false;
	private int _cooldown = 5;
	private int _currentCooldown = 0;
	public override void _Ready()
	{
		Speed = 7f;             // Movement speed
		MaxHealth = 30.0f;         // Maximum monster health
		Range = 50.0f;            // Detection range for chasing
		SpawnDistance = 100;    // Distance from player before despawning
		BaseDamage = 5.0f;
		WanderRange = 50;
		AttackSpeed = 20f;
		AttackRange = 40f;
		Monster = this;
		Chaser = false;
		Flying = true;
		Initialization();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EveryFrame(delta);
		_distance = (_player.GlobalPosition - GlobalPosition).Length();
		if (_distance <= _attackingDistance && _fleeing == false)
		{
			if (_currentCooldown <= 0)
			{
				_attacking = true;
				_rangedPosition = _player.GlobalPosition;
			}
            else
            {
                _fleeing = true;
                RandomRangedPosition();
            }
		}
		GD.Print(_distance);
		if (_distance <= _attackingRange && _attacking == true && _fleeing == false)
		{
			if (_currentCooldown <= 0)
			{
				Attack();
			}
        }
		if (_health <= 0)
		{
			_player.MonsterKilled("flyingPesk", Biome);
			if (Debug == true)
            {
				if (GetParent().GetParent() is DebugHut dh){ dh._shouldSpawn = true; }
            }
			QueueFree(); // Destroy monster when health hits zero
		}
		if (_attacking == true)
        {
            Vector3 playerPos = _player.GlobalPosition;
            _lookDirection.LookAt(new Vector3(playerPos.X, GlobalPosition.Y, playerPos.Z), Vector3.Up);
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

	public void Fly()
	{
		if (_attacking == false)
		{
			RandomRangedPosition();
			_currentCooldown -= 1;
		}
		_fleeing = false;
	}

	public async void Attack()
	{
		GD.Print("PESK ATTACK");
		_currentCooldown = _cooldown;
		_attackAnim = true;
		_attacking = false;
		_targetVelocity = Vector3.Zero;
		Velocity = _targetVelocity;

		_player.Damaged(BaseDamage + _damageOffset, this as Monster3d, "Hallucinate");

		// await ToSignal(GetTree().CreateTimer(1f), "timeout");
		_fleeing = true;
		_attackAnim = false;
		RandomRangedPosition();
	}
}
