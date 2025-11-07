using Godot;
using System;

public partial class weepingSpine : Monster3d
{
	// Called when the node enters the scene tree for the first time.
	private PackedScene _poisonBall = GD.Load<PackedScene>("res://Scenes/Monsters/MonsterAssets/poisonBall.tscn"); // Scene reference to the dark orb
	private float _distance;
	private Node3D _spawn;
    private float _projectileSpeed = 25f;
	public override void _Ready()
	{
        Speed = 4.5f;             // Movement speed
        MaxHealth = 50.0f;         // Maximum monster health
        Range = 55.0f;            // Detection range for chasing
        SpawnDistance = 100;    // Distance from player before despawning
        BaseDamage = 25.0f;
        WanderRange = 50;
        AttackSpeed = 3f;
        AttackRange = 15f;
        Monster = this;
		Stationery = true;
		Initialization();
		 _spawn = GetNode<Node3D>("Spawn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EveryFrame(delta);
		if (_health <= 0)
		{
			_player.MonsterKilled("weepingSpine");
			if (Debug == true)
            {
				if (GetParent().GetParent() is DebugHut dh){ dh._shouldSpawn = true; }
            }
			QueueFree(); // Destroy monster when health hits zero
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
			_player.Damaged(BaseDamage + _damageOffset, this as Monster3d, "None");
			_attackBox.Disabled = true;
			_hasHit = true;
		}
	}

	public async void Attack()
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
}
