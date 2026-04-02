using Godot;
using System;
using System.IO;
using System.Linq;

public partial class Monster3d : CharacterBody3D
{
	//---- THIS IS THE BASE MONSTER SCRIPT ALL VARIABLES WILL BE CHANGED IN THE INDIVIDUAL MONSTER SCRIPTS

	// --- CONSTANTS ---
	protected float RunSpeed = 3f;
	protected float WalkSpeed = 2f;
	public float MaxHealth = 100.0f;
	protected float WalkRange = 3.0f;
	protected float AgroFOV = 5.0f;
	protected float AgroLength = 5.0f;
	protected double SpawnDistance = 100;
	protected float BaseDamage = 10.0f;
	protected int WanderRange = 10;
	protected float AttackSpeed = 0.5f;
	protected float AttackRange = 1f;
	protected float AttackRangeSqr = 1f;        // Cached squared attack range (avoids sqrt each frame)
	protected CharacterBody3D Monster;
	protected bool Chaser = false;
	protected bool MoveWhileAttack = false;
	protected bool Flying = false;
	public bool Debug = false;
	public bool Shadow = false;
	public bool Stationery = false;
	public string Biome = "Plains";
	public bool Fleeing = false;
	public float SpawnRange = 50f;
	public float SpawnRangeSqr = 2500f;         // Cached squared spawn range
	public float MaxLookTime = 1f;
	public Node3D MultBodyRef = null;
	public Node3D MultHitRef = null;
	public bool DebugShapes = false;
	public bool Disabled = false;               // When true, monster stays idle and ignores everything
	public bool Cutscene = false;

	// --- NODE REFERENCES ---
	protected Player3d _player;
	protected NavigationAgent3D _navAgent;
	protected Node3D _hitFX;
	protected Node3D _body;
	protected CollisionShape3D _attackBox;
	protected Node3D _lookDirection;
	protected Area3D _walkArea;
	protected Area3D _runArea;
	protected Area3D _agroArea;
	protected ItemDropper _itemDropper;

	// --- VARIABLES ---
	public float _health;
	protected Vector3 _knockbackVelocity = Vector3.Zero;
	protected Vector3 _wanderPos;
	protected float _count;
	protected RandomNumberGenerator _rng = new();
	public Vector3 _startPos;
	public bool _canBeHit = true;
	protected Vector3 _currentRot;
	public bool _attacking = false;
	public bool _canAttack = true;
	protected bool _hasHit = false;
	protected float _speedOffset = 0f;
	protected float _damageOffset = 0f;
	protected bool _justSpawned = true;
	public bool _attackAnim = false;
	public bool _attackException = false;
	protected bool _stunned = false;
	protected Vector3 _targetVelocity;
	protected float _dashVelocity = 1f;
	protected Vector3 _rangedPosition;
	protected float _veloThreshold = -5f;
	protected bool _dashAnim = false;
	protected bool _canSeePlayer = false;
	public float _currentSpawnRange;
	protected bool _retreating = false;
	protected bool _justWandered = true;
	protected float _agroChangeCooldown = 0f;
	protected bool _playerRunning = false;
	protected string _playerBiome = "None";
	protected bool _quitePlayerInRange = false;
	protected bool _playerInVisionRange = false;
	protected bool _looking = false;
	protected float _lookingTimer = 0f;
	protected bool _playerInWalkRange = false;

	// --- Cached/precomputed state to avoid per-frame allocations ---
	private bool _cachedInVillage = false;
	private bool _cachedPlayerDead = false;
	private Vector3 _cachedPlayerPos = Vector3.Zero;
	private float _distanceSqr = 0f;           // Squared distance (avoids sqrt)
	private float _spawnDistanceSqr = 0f;
	private float _playerSpawnDistanceSqr = 0f;
	private float _navUpdateTimer = 0f;         // Throttles nav target updates so pathfinding isn't recalculated every frame
	private const float NavUpdateInterval = 0.15f; // Seconds between nav target updates (tweak 0.1-0.25 to taste)


	// --- READY --- //
	public async void Initialization()
	{
		GD.Print(Disabled);
		

		_player = GetParent().GetParent().GetParent().GetParent().GetNode<Player3d>("Player_3d");
		_rng.Randomize();
		_navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

		if (!Disabled)
		{
			float randZ = _startPos.Z + _rng.RandiRange(-WanderRange, WanderRange);
			float randX = _startPos.X + _rng.RandiRange(-WanderRange, WanderRange);
			_wanderPos = new Vector3(randX, 0f, randZ);
		}

		if (MultBodyRef != null) { _hitFX = MultHitRef; _body = MultBodyRef; }
		else { _hitFX = GetNode<Node3D>("HitFX"); _body = GetNode<Node3D>("Body"); }

		_currentRot = GlobalRotation;
		_attackBox = GetNode<CollisionShape3D>("Attackbox/CollisionShape3D");
		_health = MaxHealth;
		_lookDirection = GetNode<Node3D>("Direction");
		_walkArea = GetNode<Area3D>("WalkRange");
		_runArea = GetNode<Area3D>("RunRange");
		_agroArea = GetNode<Area3D>("AgroRange");
		_itemDropper = _player.GetParent().GetNode<ItemDropper>("MonstItemDropper");
		
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		SetUpRanges();
	}


	// --- DAMAGE SYSTEM --- //
	public void Damaged(Area3D body)
	{
		GD.Print("I was hit");
		GD.Print(_canBeHit);
		if (body.IsInGroup("Weapon") && _canBeHit)
		{
			GD.Print("By a player weapon!");
			GD.Print(_player._damage);
			DamageHandler(false, _player._damage);
		}
		else if (body.IsInGroup("PlayerProj") && _canBeHit)
		{
			float damage = MaxHealth * (float)body.GetParent().GetMeta("DamagePer");
			if (body.GetParent() is StakeBullet sb)
				sb.CountPierce();
			DamageHandler(false, damage);
		}
	}

	private async void DamageHandler(bool knockBack, float damage)
	{
		if (_stunned) { damage *= 1.3f; }
		if (knockBack) { ApplyKnockback(); }

		if (HasChildWithName(_hitFX, "AnimationPlayer"))
			_hitFX.GetNode<AnimationPlayer>("AnimationPlayer").Play("idle");

		_hitFX.Visible = true;
		_body.Visible = false;
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		GD.Print(damage);
		_health -= damage;
		_hitFX.Visible = false;
		_body.Visible = true;
	}

	public async void Bleed(float bleedDamage, float bleedLength) { }

	public bool HasChildWithName(Node node, string childName)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child.Name == childName)
				return true;
		}
		return false;
	}


	// --- CORE MONSTER AI LOOP --- //
	public void EveryFrame(double delta)
	{
		// Early-out: disabled, debugging, or player dead — go fully idle
		if (/*Disabled ||*/ Debug || _player._dead)
		{
			if (Disabled)
			{
				// Settle to a stop smoothly when disabled
				_targetVelocity = new Vector3(0f, -9.8f, 0f);
				Velocity = Velocity.Lerp(_targetVelocity, 4f * (float)delta);
				MoveAndSlide();
			}
			return;
		}

		// --- Cache frequently-used values once per frame ---
		_cachedPlayerPos    = _player.GlobalPosition;
		Vector3 myPos       = GlobalPosition;
		Vector3 diff        = _cachedPlayerPos - myPos;
		_distanceSqr        = diff.LengthSquared();                              // replaces .Length() * .Length()
		_spawnDistanceSqr   = (_startPos - myPos).LengthSquared();
		_playerSpawnDistanceSqr = (_startPos - _cachedPlayerPos).LengthSquared();
		_cachedInVillage    = _player._currentBiome.Contains("Village");

		// --- Biome/run state change checks (only trigger on actual change) ---
		if (_playerRunning != _player._running)
		{
			if (_player._running) PlayerRunCheck();
			_playerRunning = _player._running;
		}
		if (_playerBiome != _player._currentBiome)
		{
			if (_player._currentBiome != Biome) { _justWandered = false; ChooseNewWander(); }
			_playerBiome = _player._currentBiome;
		}

		_agroChangeCooldown -= 0.1f;
		_navUpdateTimer -= (float)delta;

		if (_count > 50 && _justSpawned) _justSpawned = false;

		_dashVelocity = Mathf.Lerp(_dashVelocity, 1f, 15f * (float)delta);

		// --- AI STATE MACHINE --- //

		bool outsideSpawnRange = _playerSpawnDistanceSqr > SpawnRangeSqr;
		bool inAttackRange     = _distanceSqr <= AttackRangeSqr;

		// CHASE MODE
		if (!_cachedInVillage && !outsideSpawnRange && _canSeePlayer
			&& (Chaser && !_attackAnim || MoveWhileAttack && Chaser)
			&& !Stationery && !Fleeing && !_retreating)
		{
			_justWandered = true;
			_player._inCombat = true;
			if (_navUpdateTimer <= 0f) { _navAgent.TargetPosition = _cachedPlayerPos; _navUpdateTimer = NavUpdateInterval; }

			Vector3 nextPoint = _navAgent.GetNextPathPosition();

			if (_knockbackVelocity.LengthSquared() > 0.25f)    // was > 0.5 on .Length()
			{
				_targetVelocity = _knockbackVelocity;
				Velocity = _targetVelocity;
			}
			else
			{
				_targetVelocity = (nextPoint - myPos).Normalized() * (RunSpeed * _dashVelocity + _speedOffset);
			}

			_lookDirection.LookAt(new Vector3(_cachedPlayerPos.X, myPos.Y, _cachedPlayerPos.Z), Vector3.Up);

			if (inAttackRange)
			{
				if (_knockbackVelocity.LengthSquared() <= 0.25f)
				{
					_targetVelocity = Vector3.Zero;
					Velocity = _targetVelocity;
				}
				if (_canAttack && !_attackAnim) { _attacking = true; AttackInitilize(); }
			}
			else _attacking = false;
		}

		// RANGED ENEMY
		else if (!_cachedInVillage && !outsideSpawnRange && _canSeePlayer
				 && !_attackAnim && !Chaser && !Stationery && !Fleeing && !_retreating)
		{
			_justWandered = true;
			_player._inCombat = true;
			if (_navUpdateTimer <= 0f) { _navAgent.TargetPosition = _rangedPosition; _navUpdateTimer = NavUpdateInterval; }

			Vector3 nextPoint = _navAgent.GetNextPathPosition();

			if (_knockbackVelocity.LengthSquared() > 0.25f)
			{
				_targetVelocity = _knockbackVelocity;
				Velocity = _targetVelocity;
			}
			else
			{
				_targetVelocity = (nextPoint - myPos).Normalized() * (RunSpeed * _dashVelocity + _speedOffset);
			}

			if (myPos.Snapped(0.1f) == _rangedPosition.Snapped(0.1f) || Velocity.Length() <= _veloThreshold)
			{
				_veloThreshold = -5f;
				_targetVelocity = Vector3.Zero;
				Velocity = _targetVelocity;
				AttackInitilize();
			}

			Vector3 moveDir = Velocity.Normalized();
			if (moveDir != Vector3.Zero)
				_lookDirection.LookAt(myPos + moveDir, Vector3.Up);
		}

		// STATIONARY
		else if (!outsideSpawnRange && Stationery && !Fleeing && !_retreating)
		{
			_justWandered = true;
			_lookDirection.LookAt(new Vector3(_cachedPlayerPos.X, myPos.Y, _cachedPlayerPos.Z), Vector3.Up);
			if (inAttackRange && _canAttack) { _attacking = true; AttackInitilize(); }
			else _attacking = false;
		}

		// FLEEING
		else if (Fleeing && _canSeePlayer && !_retreating)
		{
			const float fleeDistance = 8f;
			Vector3 awayDir = myPos - _cachedPlayerPos;
			if (awayDir.LengthSquared() < 0.0001f) awayDir = new Vector3(1, 0, 0);
			if (_navUpdateTimer <= 0f) { _navAgent.TargetPosition = myPos + awayDir.Normalized() * fleeDistance; _navUpdateTimer = NavUpdateInterval; }

			Vector3 nextPoint = _navAgent.GetNextPathPosition();
			_targetVelocity = (nextPoint - myPos).Normalized() * (RunSpeed * _dashVelocity + _speedOffset);

			Vector3 moveDir = _targetVelocity.Normalized();
			if (Velocity.LengthSquared() > 0.01f)
				_lookDirection.LookAt(myPos + moveDir, Vector3.Up);
		}

		// WANDERING (idle)
		else if (!_attackAnim && (outsideSpawnRange || !_canSeePlayer || _retreating || _cachedInVillage) && !Stationery)
		{
			if (!_looking)
			{
				if (_justWandered && !Fleeing)
				{
					_justWandered = false;
					_targetVelocity = Vector3.Zero;
					Velocity = _targetVelocity;
					ChooseNewWander();
				}
				else
				{
					if (_navUpdateTimer <= 0f) { _navAgent.TargetPosition = _wanderPos; _navUpdateTimer = NavUpdateInterval; }
					Vector3 nextPoint = _navAgent.GetNextPathPosition();
					_targetVelocity = (nextPoint - myPos).Normalized() * WalkSpeed;
				}

				Vector3 moveDir = Velocity.Normalized();
				if (Velocity.LengthSquared() > 0.01f)
					_lookDirection.LookAt(myPos + moveDir, Vector3.Up);

				if (_distanceSqr > SpawnDistance * SpawnDistance) { QueueFree(); return; }
			}
			else
			{
				const float turnSpeed = 0.3f;
				Vector3 currentForward = -_lookDirection.GlobalTransform.Basis.Z;
				Vector3 lookingForward = currentForward.Rotated(Vector3.Up, turnSpeed * (float)delta);
				_lookDirection.LookAt(_lookDirection.GlobalTransform.Origin + lookingForward, Vector3.Up);

				_lookingTimer += (float)delta;
				if (_lookingTimer >= MaxLookTime) { _looking = false; ChooseNewWander(); }
			}
		}

		// --- WANDER TIMER / LOOK TRIGGER --- //
		// Only tick and trigger when not already looking, so arriving at the wander
		// pos doesn't spam-reset _lookingTimer every frame causing infinite spinning.
		if (!_looking)
		{
			_count += 1;
			if (_count > 1000 || myPos.Snapped(0.1f) == _wanderPos.Snapped(0.1f))
			{
				_count = _rng.RandfRange(-100, 50);
				_targetVelocity = Vector3.Zero;
				Velocity = _targetVelocity;
				_looking = true;
				_lookingTimer = 0f;
			}
		}

		// --- RETREAT LOGIC --- //
		if (_spawnDistanceSqr > _currentSpawnRange * _currentSpawnRange && !_retreating && _canSeePlayer)
		{
			_currentSpawnRange /= 1.2f;
			_retreating = true;
			_wanderPos = _startPos;
		}
		else if (_spawnDistanceSqr <= _currentSpawnRange * _currentSpawnRange && _retreating)
		{
			_currentSpawnRange = SpawnRange;
			_retreating = false;
			ChooseNewWander();
		}
		if (inAttackRange) { _currentSpawnRange = SpawnRange; _retreating = false; }

		// --- PHYSICS --- //
		_knockbackVelocity = _knockbackVelocity.Lerp(Vector3.Zero, (float)delta * 5.0f);

		if (_stunned && !Stationery) { _targetVelocity = _knockbackVelocity; Velocity = _targetVelocity; }

		_targetVelocity = new Vector3(_targetVelocity.X, -9.8f, _targetVelocity.Z);
		Velocity = Velocity.Lerp(_targetVelocity, 4f * (float)delta);

		if (Fleeing) { MoveAndSlide(); }
		else if (_player._inv.Visible || (_stunned && !_attackException) || (_dashVelocity <= 1.01f && _dashAnim)) { _targetVelocity = Vector3.Zero; }
		else if (!Chaser && !_attackAnim) { MoveAndSlide(); }
		else if (_justSpawned || MoveWhileAttack) { MoveAndSlide(); }
		else if (!_attackAnim && _distanceSqr > AttackRangeSqr && _knockbackVelocity.LengthSquared() < 0.25f) { MoveAndSlide(); }
		else if ((_knockbackVelocity.LengthSquared() < 0.25f || _attackAnim) && !_attackException) { _targetVelocity = Vector3.Zero; }
		else { MoveAndSlide(); }
	}


	// --- SHARED ATTACK HIT HELPER --- //
	// Call this from every monster's _on_attackbox_area_entered.
	// The player hurtbox is in group "PlayerHurtbox", NOT "Player" — using the wrong group was the bug.
	protected bool TryHitPlayer(Node3D body)
	{
		if (body is Area3D area && area.IsInGroup("PlayerHurtbox") && !_hasHit)
		{
			_player.Damaged(BaseDamage + _damageOffset, this as Monster3d, "None");
			_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
			_hasHit = true;
			return true;
		}
		return false;
	}

	// --- ATTACK SYSTEM --- //
	public void AttackInitilize()
	{
		if (_stunned) return;

		if (Monster is hollowBrute hb) hb.Attack();
		else if (Monster is hollowNormal hn) hn.Attack();
		else if (Monster is hollowShadow hs) hs.Attack();
		else if (Monster is sCultist sc) sc.Attack();
		else if (Monster is flyingPesk fp) fp.Fly();
		else if (Monster is underBrush ub) ub.Attack();
		else if (Monster is vineTangler vt) vt.Attack();
		else if (Monster is weepingSpine ws) ws.Attack();
		else if (Monster is lumberJack lj) lj.Attack();
		else if (Monster is vCultist vc) vc.Attack();
		else if (Monster is revenanT rt) rt.Attack();
	}


	// --- STUN EFFECT --- //
	public async void Stunned()
	{
		if (Monster is flyingPesk)
		{
			_speedOffset = -3.5f;
			GetNode<GpuParticles3D>("Stunned").Emitting = true;
			await ToSignal(GetTree().CreateTimer(3f), "timeout");
			GetNode<GpuParticles3D>("Stunned").Emitting = false;
			_speedOffset = 0f;
		}
		else
		{
			if (Monster is underBrush ub) ub._currentAttackOffset = 0f;
			_stunned = true;
			_attackException = true;
			ApplyKnockback();
			GetNode<GpuParticles3D>("Stunned").Emitting = true;
			await ToSignal(GetTree().CreateTimer(1f), "timeout");
			GetNode<GpuParticles3D>("Stunned").Emitting = false;
			_stunned = false;
		}
	}


	// --- RANGED POSITION GENERATION --- //
	public async void RandomRangedPosition()
	{
		_rng.Randomize();
		float angle = GD.Randf() * 2 * MathF.PI;
		Vector3 center = _player.GlobalPosition;
		_rangedPosition = new Vector3(
			center.X + AttackRange * Mathf.Cos(angle),
			center.Y,
			center.Z + AttackRange * Mathf.Sin(angle)
		);
		await ToSignal(GetTree().CreateTimer(2f), "timeout");
		_veloThreshold = 0.5f;
	}


	// --- KNOCKBACK --- //
	private void ApplyKnockback()
	{
		Vector3 knockbackDir = (GlobalPosition - _player.Position).Normalized();
		_knockbackVelocity = knockbackDir * _player._knockbackStrength;
		_knockbackVelocity.Y = 0f;
	}

	private void ChooseNewWander()
	{
		if (Disabled){_wanderPos = new Vector3(GlobalPosition.X, 0f, GlobalPosition.Z); return;}
		float randZ = _startPos.Z + _rng.RandiRange(-WanderRange, WanderRange);
		float randX = _startPos.X + _rng.RandiRange(-WanderRange, WanderRange);
		_wanderPos = new Vector3(randX, 0f, randZ);
	}

	public void PackAgro()
	{
		foreach (Area3D agroArea in _agroArea.GetOverlappingAreas())
		{
			if (agroArea.GetParent() is Monster3d agroEnteredMonster)
			{
				if (agroEnteredMonster == this) return;
				agroEnteredMonster.PackAgro();
			}
		}
	}

	private void detectPlayer(Area3D area, bool seen, bool agro)
	{
		if (_agroChangeCooldown > 0) return;
		_agroChangeCooldown = 0.5f;

		if (seen)
		{
			if (area.IsInGroup("PlayerHurtbox"))
				_canSeePlayer = true;
			else if (area.IsInGroup("Monster") && _canSeePlayer && area.GetParent() is Monster3d vm && vm != this)
			{
				// PackAgro hook available here
			}
		}
		else
		{
			if (area.IsInGroup("PlayerHurtbox") && !_playerInWalkRange && !_quitePlayerInRange)
				_canSeePlayer = false;
		}
	}

	private void SetUpRanges()
    {
        if (Cutscene)
        {
			BaseDamage = 4.0f;         // Base damage of the monster
			WalkSpeed = 0.5f;             // Movement speed when they are wandering
			RunSpeed = 1.75f;              // Movement speed when they are chasing the player
        }
		if (Disabled)
		{
			WalkRange  = 0.5f;
			AgroLength = 1f;
		}
        else if (Cutscene)
        {
            WalkRange = 20f;
        }
		if (_walkArea.GetNode<CollisionShape3D>("CollisionShape3D").Shape.Duplicate() is SphereShape3D shape)
		{
			shape.Radius = WalkRange;
			_walkArea.GetNode<CollisionShape3D>("CollisionShape3D").Shape = shape;
			if (_walkArea.GetNode<MeshInstance3D>("Debug").Mesh.Duplicate() is SphereMesh debugShape)
			{
				debugShape.Radius = WalkRange ;
				_walkArea.GetNode<MeshInstance3D>("Debug").Mesh = debugShape;
			}
		}
		if (_runArea.GetNode<CollisionShape3D>("CollisionShape3D").Shape.Duplicate() is SphereShape3D shape2)
		{
			shape2.Radius = WalkRange * 3;
			_runArea.GetNode<CollisionShape3D>("CollisionShape3D").Shape = shape2;
			if (_runArea.GetNode<MeshInstance3D>("Debug").Mesh.Duplicate() is SphereMesh debugShape)
			{
				debugShape.Radius = WalkRange * 3;
				_runArea.GetNode<MeshInstance3D>("Debug").Mesh = debugShape;
			}
		}
		if (DebugShapes)
		{
			_walkArea.GetNode<MeshInstance3D>("Debug").Visible = true;
			_runArea.GetNode<MeshInstance3D>("Debug").Visible = true;
			_agroArea.GetNode<MeshInstance3D>("Debug").Visible = true;
		}

		Vector3 baseScale = _agroArea.Scale;
		baseScale.X = AgroLength;
		baseScale.Y = AgroFOV;
		baseScale.Z = AgroFOV;
		_agroArea.Scale = baseScale;


		// Cache squared ranges to avoid sqrt in EveryFrame
		AttackRangeSqr = AttackRange * AttackRange;
		SpawnRangeSqr = SpawnRange * SpawnRange;

		Vector3 diff = _player.GlobalPosition - GlobalPosition;
		GD.Print(diff);
		if (diff.Length() <= WalkRange)
        {
            ForceSeePlayer();
        }
    }

	public void ForceSeePlayer()
    {
        Area3D area = _player.GetNode<Area3D>("Hurtbox");
		if (area.IsInGroup("PlayerHurtbox"))
		{
			_playerInWalkRange = true;
			_agroChangeCooldown = 0;
			detectPlayer(area, true, false);
		}
    }

	private void _on_walk_range_area_entered(Area3D area)
	{
		if (area.IsInGroup("PlayerHurtbox"))
        {
            _playerInWalkRange = true;
			detectPlayer(area, true, false);
        }
	}

	private void _on_run_range_area_entered(Area3D area)
	{
		if (_player._running) detectPlayer(area, true, false);
		if (area.IsInGroup("PlayerHurtbox")) _quitePlayerInRange = true;
	}

	private void _on_run_range_area_exited(Area3D area)
	{
		detectPlayer(area, false, false);
		if (area.IsInGroup("PlayerHurtbox")) { _quitePlayerInRange = false; _playerInWalkRange = false; }
	}

	private void _on_agro_range_area_entered(Area3D area)
	{
		if (area.IsInGroup("PlayerHurtbox")) _playerInVisionRange = true;
		detectPlayer(area, true, true);
	}

	private void _on_agro_range_area_exited(Area3D area)
	{
		if (area.IsInGroup("PlayerHurtbox")) _playerInVisionRange = false;
		detectPlayer(area, false, true);
	}

	private void PlayerRunCheck()
	{
		if (_quitePlayerInRange)
			detectPlayer(_player.GetNode<Area3D>("Hurtbox"), true, false);
	}

	private bool CheckVision()
	{
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		var query = new PhysicsRayQueryParameters3D()
		{
			From = GlobalPosition,
			To = _player.GlobalPosition,
			CollideWithAreas = true,
			CollideWithBodies = true,
		};
		return spaceState.IntersectRay(query).Count == 0;
	}
}