using Godot;
using System;

public partial class vineTangler : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private float _distance;
	private Node3D _spawn;
	private PackedScene _undergroundVine = GD.Load<PackedScene>("res://Scenes/Monsters/MonsterAssets/vineUnderground.tscn");
	public bool _hasVine = false;
	private Node _holder;
	private bool _readyToAttack = true;
	private bool _meleeAnim = false;
	public override void _Ready()
	{
		Speed = 4.6f;             // Movement speed
		MaxHealth = 45.0f;         // Maximum monster health
		Range = 30.0f;            // Detection range for chasing
		SpawnDistance = 100;    // Distance from player before despawning
		BaseDamage = 2.5f;
		WanderRange = 50;
		AttackSpeed = 1.5f;
		AttackRange = 10f;
		Monster = this;
		Stationery = true;
		Initialization();

		_spawn = GetNode<Node3D>("Spawn");
		_holder = _player.GetParent();	
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EveryFrame(delta);
		_distance = (GlobalPosition - _player.GlobalPosition).Length();
		if (_health <= 0)
		{
			_player.MonsterKilled("vineTangler", Biome);
			if (Debug == true)
            {
				if (GetParent().GetParent() is DebugHut dh){ dh._shouldSpawn = true; }
            }
			QueueFree(); // Destroy monster when health hits zero
		}
		RotateFunc(delta);
	}

	public async void VineDied()
	{
		_hasVine = false;
		await ToSignal(GetTree().CreateTimer(1.3), "timeout");
		_readyToAttack = true;
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

	public async void _on_attackbox_area_entered(Node3D body)
	{
		if (body.IsInGroup("Player") && _hasHit == false && body.Name == "Hurtbox")
		{
			_player.Damaged(BaseDamage + _damageOffset, this as Monster3d, "None");
			_attackBox.GetParent<Area3D>().Monitoring = false;
			_hasHit = true;
			await ToSignal(GetTree().CreateTimer(0.4), "timeout");
			_player.Damaged(BaseDamage + _damageOffset, this as Monster3d, "None");
		}
	}

	public async void Attack()
	{
		if (_readyToAttack == false)
        {
			return;
        }
		if (_distance < 2.5)
        {
			_hasHit = false;
			_meleeAnim = true;
			await ToSignal(GetTree().CreateTimer(0.5), "timeout");
			_attackBox.GetParent<Area3D>().Monitoring = true;
			await ToSignal(GetTree().CreateTimer(0.2), "timeout");
			_attackBox.GetParent<Area3D>().Monitoring = false;
			await ToSignal(GetTree().CreateTimer(0.2), "timeout");
			_attackBox.GetParent<Area3D>().Monitoring = true;
			await ToSignal(GetTree().CreateTimer(0.2), "timeout");
			_attackBox.GetParent<Area3D>().Monitoring = false;
			_canAttack = false;
			await ToSignal(GetTree().CreateTimer(0.1), "timeout");
			_meleeAnim = false;
			await ToSignal(GetTree().CreateTimer(AttackSpeed-0.1f), "timeout");
			_canAttack = true;
		}
        else
		{
			_hasHit = false;
			_canAttack = false;
			_attackAnim = true;
			_hasVine = true;
			_readyToAttack = false;
			await ToSignal(GetTree().CreateTimer(1.5), "timeout");
			CharacterBody3D projectileInstance = _undergroundVine.Instantiate<CharacterBody3D>(); // Create monster instance
			projectileInstance.GlobalPosition = _spawn.GlobalPosition;
			_holder.AddChild(projectileInstance);                                             // Add monster to holder node
			if (projectileInstance is vineUnderground vu)
			{
				vu._player = _player;
				vu._monster = this;
			}
			await ToSignal(GetTree().CreateTimer(0.7), "timeout");
			_attackAnim = false;
			await ToSignal(GetTree().CreateTimer(AttackSpeed), "timeout");
			_canAttack = true;
		}
	}
}
