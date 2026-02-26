using Godot;
using System;
using Godot.Collections;

public partial class vCultist : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private float _distance;
	private bool _smashAnim = false;
	private bool _attack2Anim = false;
	private float _charge = 0f;
	private ShaderMaterial _auraShader;
	private MeshInstance3D _leftAura;
	private MeshInstance3D _rightAura;
	private Area3D _pushBox;
	public override void _Ready()
	{
		// -- Variables -- //
		Chaser = true;              // If this monster chasing the player or finds a point within a range of the player
		MoveWhileAttack = true;     // Can this monster move while attacking
		Flying = false;              // Should gravity be applied to this monster
		Stationery = false;          // If the monster shouldnt move at all
		BaseDamage = 12.0f;         // Base damage of the monster
		//BaseDamage = new Godot.Collections.Array<float>{15f, 25, 40};
		AttackSpeed = 0.5f;         // The time between its attacks
		AttackRange = 1.5f;           // The distance the monster gets from the player before stopping and attacking
		MaxHealth = 65.0f;         // Maximum monster health
		WanderRange = 35;           // The range the monster can wander from its spawn point
		AgroFOV = 7f;          	// The vision FOV of the monster
		AgroLength = 6.5f;          // The detection length of the monsters vision
		WalkRange = 4.5f;	         	// The noise range monsters hear the player walking
		WalkSpeed = 1.5f;             // Movement speed when they are wandering
		RunSpeed = 4.5f;              // Movement speed when they are chasing the player

		// -- Other -- //
		Monster = this;
		Initialization();
		_leftAura = GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/forearm_L/Cube_004");
		_rightAura = GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/forearm_R/Cube_001");
		_auraShader = _leftAura.MaterialOverride as ShaderMaterial;
		_pushBox = GetNode<Area3D>("PushBox");
	}

	private void ToggleAura(bool toggle)
	{
		_leftAura.Visible = toggle;
		_rightAura.Visible = toggle;
	}
	private void ToggleParticles(bool toggle)
	{
		_leftAura.GetNode<GpuParticles3D>("Magic").Emitting = toggle; 
		_rightAura.GetNode<GpuParticles3D>("Magic").Emitting = toggle;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EveryFrame(delta);
		_auraShader.SetShaderParameter("fade_softness", Mathf.Lerp((float)_auraShader.GetShaderParameter("fade_softness"), _charge, (float)delta));
		if (_charge >= 1){ToggleAura(true); ToggleParticles(true);}
		if (_charge <= 0){ToggleAura(false); ToggleParticles(false);}else{ToggleAura(true);}
		if (_stunned){_charge = 0;}
		if (_health <= 0)
		{
			_player.MonsterKilled("vCultist", Biome);
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
			GD.Print(_charge);
			if (_charge < 1){_charge += 0.34f;}
			_player.Damaged((BaseDamage + _damageOffset)*(1+(_charge/1.5f)), this, "None");
			_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
			_hasHit = true;
		}
	}
	public void _on_push_box_area_entered(Node3D body)
	{
		if (body.IsInGroup("Player") && _hasHit == false && body.Name == "Hurtbox")
		{
			GD.Print(_charge);
			_player.Damaged(0f, this, "Push");
			_pushBox.SetDeferred("monitoring", false);
			_hasHit = true;
		}
	}

	public async void Attack()
	{
		_canAttack = false;
		if (_charge < 0.66)
		{
			BasicAttack();
			_attackAnim = true;
			await ToSignal(GetTree().CreateTimer(0.65f), "timeout");
			_attackAnim = false;
			BasicAttack();
			_attack2Anim = true;
			await ToSignal(GetTree().CreateTimer(0.7f), "timeout");
			_attack2Anim = false;
			await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
			_canAttack = true;
		}
		else if (_charge < 1)
		{
			SuperPunch();
			_attackAnim = true;
			await ToSignal(GetTree().CreateTimer(1.25f), "timeout");
			_attackAnim = false;
			await ToSignal(GetTree().CreateTimer(AttackSpeed+0.1f), "timeout");
			_canAttack = true;
		}
		else
		{
			MoveWhileAttack = false;
			await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
			Smash();
			_smashAnim = true;
			_attackAnim = true;
			await ToSignal(GetTree().CreateTimer(1.4f), "timeout");
			_smashAnim = false;
			_attackAnim = false;
			MoveWhileAttack = true;
			await ToSignal(GetTree().CreateTimer(AttackSpeed*2), "timeout");
			_canAttack = true;
		}
	}

	private async void BasicAttack()
	{
		_hasHit = false;
		await ToSignal(GetTree().CreateTimer(0.55), "timeout");
		_attackBox.GetParent<Area3D>().SetDeferred("monitoring", true);
		await ToSignal(GetTree().CreateTimer(0.1), "timeout");
		_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
	}
	private async void SuperPunch()
	{
		_hasHit = false;
		await ToSignal(GetTree().CreateTimer(1.14), "timeout");
		_attackBox.GetParent<Area3D>().SetDeferred("monitoring", true);
		await ToSignal(GetTree().CreateTimer(0.1), "timeout");
		_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
	}
	private async void Smash()
	{
		_hasHit = false;
		await ToSignal(GetTree().CreateTimer(1.06), "timeout");
		_attackBox.GetParent<Area3D>().SetDeferred("monitoring", true);
		_pushBox.SetDeferred("monitoring", true);
		GetNode<GpuParticles3D>("Push").Emitting = true;
		_charge = 0f;
		await ToSignal(GetTree().CreateTimer(0.3), "timeout");
		_pushBox.SetDeferred("monitoring", false);
		_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
	}
}
