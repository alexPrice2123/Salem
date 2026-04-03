using Godot;
using System;
public partial class theCoiledOne : Monster3d
{
	private float _distance;
	private int _attackAnimSwitch = 1;
	public Godot.Collections.Array<Node3D> _resinArray { get; set; } = []; 
	private int _resinCount = 0;
	private MeshInstance3D _roots;
	public override void _Ready()
	{
		// -- Variables -- //
		Chaser = true;              // If this monster chasing the player or finds a point within a range of the player
		MoveWhileAttack = false;     // Can this monster move while attacking
		IsObject = true;              // Should gravity be applied to this monster
		Stationery = true;          // If the monster shouldnt move at all
		BaseDamage = 12.5f;         // Base damage of the monster000000
		AttackSpeed = 2.5f;         // The time between its attacks
		AttackRange = 1.5f;           // The distance the monster gets from the player before stopping and attacking
		MaxHealth = 100.0f;         // Maximum monster health
		WanderRange = 35;           // The range the monster can wander from its spawn point
		AgroFOV = 7.0f;          	// The vision FOV of the monster
		AgroLength = 5.5f;          // The detection length of the monsters vision
		WalkRange = 3.5f;	         	// The noise range monsters hear the player walking
		WalkSpeed = 1f;             // Movement speed when they are wandering
		RunSpeed = 3.5f;              // Movement speed when they are chasing the player

		// -- Other -- //
		Monster = this;
		Initialization();

		foreach (Node3D roots in GetParent().GetParent().GetChildren())
        {
            if (((string)roots.Name).Contains("RootWall"))
            {
                foreach (Node3D resin in roots.GetChildren())
                {
					if (((string)resin.Name).Contains("Resin"))
                    {
                        _resinArray.Add(resin);
                    }
                }
            }
        }
		SpawnResin();
		_roots = GetNode<MeshInstance3D>("Roots");
	}

	public async void ResinBroken()
    {
		_resinCount -= 1;
		GD.Print(_resinCount+" Resin Left");
		if (_resinCount <= 0)
        {
			await ToSignal(GetTree().CreateTimer(1), "timeout");
			_roots.Visible = false;
            //SpawnResin();
        }
    }

	private void SpawnResin()
    {
		_resinCount = 0;
        for (int i = 0; i < 6; i++)
        {
            if (_resinArray.PickRandom() is Resin resInst)
            {
                resInst.Grow(this);
				_resinCount += 1;
            }
        }
    }

	public override void _Process(double delta)
	{
		EveryFrame(delta);
		if (_health <= 0)
		{
			_player.MonsterKilled("theCoiledOne", Biome);
			if (Debug == true)
			{
				if (GetParent().GetParent() is DebugHut dh) { dh._shouldSpawn = true; }
			}
			int i = 0;
			if(Cutscene)
			{
				foreach (Monster3d monst in GetParent().GetChildren())
				{
					if (monst != this)
					{
						monst.ForceSeePlayer();
						i++;
					}
				}
				if (i == 0)
				{
					_player.GetParent().GetNode<Cutscene3>("Cutscene3").StartCut(_player);
				}
			}
			if(!Cutscene)
			{
				//_itemDropper.Drop("deadooze", 0.5f, 3, GlobalPosition);
				//_itemDropper.Drop("woundedooze", 0.25f, 2, GlobalPosition);
			}
			QueueFree();
		}
		if (Velocity.LengthSquared() > 0.01f)
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

	public void _on_hurtbox_area_entered(Area3D body){if(!_roots.Visible){Damaged(body);}}

	public void _on_attackbox_area_entered(Node3D body)
	{
		TryHitPlayer(body);
	}

	public async void Attack()
	{
		if (_attackAnimSwitch == 1) { _attackAnimSwitch = 2; }
		else { _attackAnimSwitch = 1; }

		_hasHit = false;
		_attackAnim = true;
		await ToSignal(GetTree().CreateTimer(0.48), "timeout");
		_attackBox.GetParent<Area3D>().SetDeferred("monitoring", true);
		await ToSignal(GetTree().CreateTimer(0.2), "timeout");
		_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
		_canAttack = false;
		await ToSignal(GetTree().CreateTimer(0.1), "timeout");
		_attackAnim = false;
		await ToSignal(GetTree().CreateTimer(AttackSpeed - 0.1f), "timeout");
		_canAttack = true;
	}
}
