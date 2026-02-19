using Godot;
using Godot.Collections;
using System;

public partial class vCultist : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private float _distance;
	private int _attackAnimSwitch = 1;
	private bool _smashAnim = false;
	private float _charge = 0f;
	private ShaderMaterial _auraShader;
	private MeshInstance3D _leftAura;
	private MeshInstance3D _rightAura;
	public override void _Ready()
	{
		// -- Variables -- //
		Chaser = true;              // If this monster chasing the player or finds a point within a range of the player
		MoveWhileAttack = true;     // Can this monster move while attacking
		Flying = false;              // Should gravity be applied to this monster
		Stationery = false;          // If the monster shouldnt move at all
		BaseDamage = 15.0f;         // Base damage of the monster
		AttackSpeed = 1.2f;         // The time between its attacks
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
		_leftAura = GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/forearm_L/arur_l");
		_rightAura = GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/forearm_R/arua_r");
		_auraShader = _leftAura.MaterialOverride as ShaderMaterial;

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
			_player.Damaged((BaseDamage + _damageOffset)*(1+(_charge/1.5f)), this as Monster3d, "None");

			_attackBox.Disabled = true;
			_hasHit = true;
		}
	}

	public async void Attack()
	{
		if (_attackAnimSwitch == 1)
		{
			_attackAnimSwitch = 2;
		}
		else
		{
			_attackAnimSwitch = 1;
		}
		if (_charge < 1)
        {
            _charge += 0.34f;
			_hasHit = false;
			_attackAnim = true;
			await ToSignal(GetTree().CreateTimer(0.48), "timeout");
			_attackBox.GetParent<Area3D>().SetDeferred("monitoring", true);
			await ToSignal(GetTree().CreateTimer(0.2), "timeout");
			_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
			_canAttack = false;
			await ToSignal(GetTree().CreateTimer(0.1), "timeout");
			_attackAnim = false;
			await ToSignal(GetTree().CreateTimer(AttackSpeed-0.1f), "timeout");
			_canAttack = true;
        }
        else
        {
			_hasHit = false;
			_smashAnim = true;
			await ToSignal(GetTree().CreateTimer(0.48), "timeout");
			_attackBox.GetParent<Area3D>().SetDeferred("monitoring", true);
			_charge = 0f;
			_leftAura.GetNode<GpuParticles3D>("Magic").Emitting = false; 
			_rightAura.GetNode<GpuParticles3D>("Magic").Emitting = false;
			await ToSignal(GetTree().CreateTimer(0.2), "timeout");
			_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
			_canAttack = false;
			await ToSignal(GetTree().CreateTimer(0.5), "timeout");
			_smashAnim = false;
			await ToSignal(GetTree().CreateTimer(AttackSpeed-0.1f), "timeout");
			_canAttack = true;
        }
	}
}
