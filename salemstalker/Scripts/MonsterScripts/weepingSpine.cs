using Godot;
using System;

public partial class weepingSpine : Monster3d
{
	// Called when the node enters the scene tree for the first time.
	private PackedScene _poisonBall = GD.Load<PackedScene>("res://Scenes/Monsters/MonsterAssets/poisonBall.tscn"); // Scene reference to the dark orb
	private float _distance;
	private Node3D _spawn;
	private float _projectileSpeed = 25f;
	private float _meleeRange = 2f;
	private float _meleeDamage = 10f;
	public bool _hushSpawned = false;
	public override void _Ready()
	{
       	// -- Variables -- //
		Chaser = true;              // If this monster chasing the player or finds a point within a range of the player
		MoveWhileAttack = true;     // Can this monster move while attacking
		Flying = false;              // Should gravity be applied to this monster
		Stationery = false;          // If the monster shouldnt move at all
		BaseDamage = 10.0f;         // Base damage of the monster
		AttackSpeed = 0.5f;         // The time between its attacks
		AttackRange = 10f;           // The distance the monster gets from the player before stopping and attacking
		MaxHealth = 100.0f;         // Maximum monster health
		WanderRange = 10;           // The range the monster can wander from its spawn point
		AgroFOV = 5.0f;          	// The vision FOV of the monster
		AgroLength = 5.0f;          // The detection length of the monsters vision
		WalkSpeed = 2f;             // Movement speed when they are wandering
		RunSpeed = 3f;              // Movement speed when they are chasing the player

		// -- Other -- //
		Monster = this;
		Initialization();
		_spawn = GetNode<Node3D>("Spawn");
		if (_hushSpawned == true)
        {
            _damageOffset += BaseDamage*-0.25f;
        }
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EveryFrame(delta);
		if (this.GetParent().GetParent().GetParent().GetParent().GetNode<Node3D>("HushedTrees") is theHushedBark thb1 && _hushSpawned == true)
		{
			if (thb1._dead == true)
            {
                _health = 0;
            }
		}
		if (_health <= 0)
		{
			_player.MonsterKilled("weepingSpine", Biome);
			if (Debug == true)
			{
				if (GetParent().GetParent() is DebugHut dh) { dh._shouldSpawn = true; }
			}
			if (this.GetParent().GetParent().GetParent().GetParent().GetNode<Node3D>("HushedTrees") is theHushedBark thb && _hushSpawned == true)
            {
                thb._weepingCount -= 1;
            }
			QueueFree(); // Destroy monster when health hits zero
		}
		_distance = (_player.GlobalPosition - GlobalPosition).Length();
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
			_player.Damaged(_meleeDamage + _damageOffset, this as Monster3d, "None");
			_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
			_hasHit = true;
		}
	}

	public async void Attack()
	{
		if (_distance > _meleeRange)
		{
			_hasHit = false;
			_attackAnim = true;
			_canAttack = false;
			await ToSignal(GetTree().CreateTimer(1.6), "timeout");
			RigidBody3D projectileInstance = _poisonBall.Instantiate<RigidBody3D>(); // Create monster instance
			_player.GetParent().AddChild(projectileInstance);                                             // Add monster to holder node
			projectileInstance.GlobalPosition = _spawn.GlobalPosition;
			if (projectileInstance is poisonBall ball)
			{
				ball._playerOrb = _player;
				ball._damageOrb = BaseDamage + _damageOffset;
				ball.Shoot(_projectileSpeed);
			}
			_attackAnim = false;
			await ToSignal(GetTree().CreateTimer(AttackSpeed), "timeout");
			_canAttack = true;
		}
        else
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
			await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
			_canAttack = true;
        }
	}
}
