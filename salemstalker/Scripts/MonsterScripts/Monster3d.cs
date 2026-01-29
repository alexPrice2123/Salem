using Godot;
using System;
using System.IO;
using System.Linq;

public partial class Monster3d : CharacterBody3D
{
	//---- THIS IS THE BASE MONSTER SCRIPT ALL VARIABLES WILL BE CHANGED IN THE INDIVIDUAL MONSTER SCRIPTS


	// --- CONSTANTS ---
	protected float RunSpeed = 3f;              // Movement speed when they are chasing the player
	protected float WalkSpeed = 2f;             // Movement speed when they are wandering
	protected float MaxHealth = 100.0f;         // Maximum monster health
	protected float WalkRange = 15.0f;              // Walk hearing detection (sprint hearing is 3x this)
	protected float AgroFOV = 5.0f;          	// The vision FOV of the monster
	protected float AgroLength = 5.0f;          // The detection length of the monsters vision
	protected double SpawnDistance = 100;       // Distance from player before despawning
	protected float BaseDamage = 10.0f;         // Base damage of the monster
	protected int WanderRange = 10;             // The range the monster can wander from its spawn point
	protected float AttackSpeed = 0.5f;         // The time between its attacks
	protected float AttackRange = 1f;           // The distance the monster gets from the player before stopping and attacking
	protected CharacterBody3D Monster;          // A reference to the monster
	protected bool Chaser = false;              // If this monster chasing the player or finds a point within a range of the player
	protected bool MoveWhileAttack = false;     // Can this monster move while attacking
	protected bool Flying = false;              // Should gravity be applied to this monster
	public bool Debug = false;                  // If true this monster wont move or attack
	public bool Shadow = false;                 // Decides if the monster can become a phantom
	public bool Stationery = false;             // If the monster shouldnt move at all
	public string Biome = "Plains";             // What Biome this monster spawned in
	public bool Fleeing = false;				// If the monster is fleeing
	public float SpawnRange = 50f;              // The disntance the monster can be from spawn before retreeting back to it
	public float MaxLookTime = 1f;				// How long the monster looks around when wandering (in seconds)
	public Node3D MultBodyRef = null;
	public Node3D MultHitRef = null;
	public bool DebugShapes = false;				// If debug hitboxes should be enabled

	// --- NODE REFERENCES ---
	protected Player3d _player;                 // Reference to the player
	protected NavigationAgent3D _navAgent;      // Pathfinding/navigation agent
	protected Node3D _hitFX;                    // Hit effect visual node
	protected Node3D _body;                     // Monster body mesh node
	protected CollisionShape3D _attackBox;      // The attack box of the monster
	protected Node3D _lookDirection;            // The goal look direction that the monster should lerp to
	protected Area3D _walkArea;
	protected Area3D _runArea;
	protected Area3D _agroArea;

	// --- VARIABLES ---
	public float _health;                       // Current health of the monster
	protected Vector3 _knockbackVelocity = Vector3.Zero; // Knockback force applied when hit
	protected Vector3 _wanderPos;               // Current wander target position
	protected float _count;                     // Frame counter used to time wander updates
	protected RandomNumberGenerator _rng = new(); // Random number generator for wander movement
	public Vector3 _startPos;                // Starting position (wander center point)
	public bool _canBeHit = true;               // Prevents rapid re-hits during invulnerability
	protected Vector3 _currentRot;              // Stores current rotation of monster
	public bool _attacking = false;             // Is the monster attacking
	public bool _canAttack = true;              // Is the monster able to attack
	protected bool _hasHit = false;             // Has the monster been hit by the player (so it doesnt get double hit)
	protected float _speedOffset = 0f;          // Number that adds to the speed
	protected float _damageOffset = 0f;         // Number that adds to the damage
	protected bool _justSpawned = true;         // Did this monster just spawn (giving 50 frames to move away from the hut before it does anything)
	public bool _attackAnim = false;         // Should the attack anim for the monster be playing
	public bool _attackException = false;       // An exception to anything causing the monster not to move
	protected bool _stunned = false;            // Is the monster stunned
	protected Vector3 _targetVelocity;          // The velocity that the monster will lerp towards
	protected float _dashVelocity = 1f;         // The velocity added onto target velocity from the monsters dash
	protected Vector3 _rangedPosition;          // The point chosen by non chaser monsters to go to
	protected float _veloThreshold = -5f;       // The velocity threshold that ranged monsters have to get to, to stop and attack
	protected bool _dashAnim = false;           // Should the dash anim be playing
	protected bool _canSeePlayer = false;
	public float _currentSpawnRange;         //The disntance the monster can be from spawn before retreeting back to it
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
	

	// --- READY --- //
	public void Initialization()
	{
		_player = GetParent().GetParent().GetParent().GetParent().GetNode<Player3d>("Player_3d");
		_rng.Randomize();
		_navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

		// Pick initial random wander target
		GD.Print(_startPos);    
		float randZ = _startPos.Z + _rng.RandiRange(-WanderRange, WanderRange);
		float randX = _startPos.X + _rng.RandiRange(-WanderRange, WanderRange);
		_wanderPos = new Vector3(randX, 0f, randZ);
		// Assign visual and functional nodes
		if (MultBodyRef != null){_hitFX = MultHitRef;
		_body = MultBodyRef;}
		else{_hitFX = GetNode<Node3D>("HitFX");
		_body = GetNode<Node3D>("Body");}
		_currentRot = GlobalRotation;
		_attackBox = GetNode<CollisionShape3D>("Attackbox/CollisionShape3D");
		_health = MaxHealth;
		_lookDirection = GetNode<Node3D>("Direction");
		_walkArea = GetNode<Area3D>("WalkRange");
		_runArea = GetNode<Area3D>("RunRange");
		_agroArea = GetNode<Area3D>("AgroRange");

		if (_walkArea.GetNode<CollisionShape3D>("CollisionShape3D").Shape is SphereShape3D shape)
		{
			shape.Radius = WalkRange;
			if (_walkArea.GetNode<MeshInstance3D>("Debug").Mesh is SphereMesh debugShape)
			{
				debugShape.Radius = WalkRange;
			}
		}
		if (_runArea.GetNode<CollisionShape3D>("CollisionShape3D").Shape is SphereShape3D shape2)
		{
			shape2.Radius = WalkRange*3;
			if (_runArea.GetNode<MeshInstance3D>("Debug").Mesh is SphereMesh debugShape)
			{
				debugShape.Radius = WalkRange*3;
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
	}


	// --- DAMAGE SYSTEM --- //
	// Checks what hit the monster and applies damage appropriately
	public void Damaged(Area3D body)
	{
		if (body.IsInGroup("Weapon") && _canBeHit)
		{
			DamageHandler(false, _player._damage);
		}
		else if (body.IsInGroup("PlayerProj") && _canBeHit)
		{
			float damage = MaxHealth * (float)body.GetParent().GetMeta("DamagePer");
			if (body.GetParent() is StakeBullet sb)
			{
				sb.CountPierce();
			}
			DamageHandler(false, damage);
		}
	}

	private async void DamageHandler(bool knockBack, float damage)
	{
		// Increased damage if stunned
		if (_stunned) { damage *= 1.3f; }

		// Optional knockback force
		if (knockBack == true) { ApplyKnockback(); }

		// Quick visual hit reaction'
		if (HasChildWithName(_hitFX, "AnimationPlayer"))
		{
		   _hitFX.GetNode<AnimationPlayer>("AnimationPlayer").Play("idle"); 
		}
		_hitFX.Visible = true;
		_body.Visible = false;
		//_canAttack = false;
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

		// Reduce health
		_health -= damage;

		_hitFX.Visible = false;
		_body.Visible = true;
	}

	public async void Bleed(float bleedDamage, float bleedLength)
	{
		
	}

	public bool HasChildWithName(Node node, string childName)
	{
		// Get all children of the current node.
		Godot.Collections.Array<Node> children = node.GetChildren();

		// Iterate through the children and check their names.
		foreach (Node child in children)
		{
			if (child.Name == childName)
			{
				return true; // Found a child with the specified name.
			}
		}
		return false; // No child with the specified name was found.
	}

	// --- CORE MONSTER AI LOOP --- //
	// Handles state: chase, attack, wander, despawn
	public void EveryFrame(double delta)
	{
		if (Debug == true || _player._dead == true)
		{
			return;
		}
		if (_playerRunning != _player._running){if (_player._running){PlayerRunCheck();}}
		_playerRunning = _player._running;
		if (_playerBiome != _player._currentBiome){if(_player._currentBiome != Biome){_justWandered = false; ChooseNewWander();}}
		_playerBiome = _player._currentBiome;
		float distance = (_player.GlobalPosition - GlobalPosition).Length();
		float spawnDistance = (_startPos - GlobalPosition).Length();
		float playerSpawnDistance = (_startPos - _player.GlobalPosition).Length();

		_agroChangeCooldown -= 0.1f;

		// Delay initial behavior when first spawned
		if (_count > 50 && _justSpawned)
			_justSpawned = false;

		_dashVelocity = Mathf.Lerp(_dashVelocity, 1f, 15f * (float)delta);

		if (_playerInVisionRange && CheckVision() && !_canSeePlayer)
		{
			detectPlayer(_player.GetNode<Area3D>("Hurtbox"), true, true);
		}

		// CHASE MODE: If player close enough and monster is a chaser
		if (!_player._currentBiome.Contains("Village") && playerSpawnDistance <= SpawnRange && _canSeePlayer && (Chaser && !_attackAnim || MoveWhileAttack && Chaser) && Stationery == false && Fleeing == false && !_retreating)
		{
			_justWandered = true;
			// Navigation pathing toward player
			_navAgent.TargetPosition = _player.GlobalPosition;
			_player._inCombat = true;
			Vector3 nextPoint = _navAgent.GetNextPathPosition();
			// Apply movement and knockback forces
			if (_knockbackVelocity.Length() > 0.5)
			{
				_targetVelocity = Vector3.Zero + _knockbackVelocity;
				Velocity = _targetVelocity;
			}
			else
			{
				_targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * (RunSpeed * _dashVelocity + _speedOffset);
			}

			// Rotate monster to face player
			Vector3 playerPos = _player.GlobalPosition;
			_lookDirection.LookAt(new Vector3(playerPos.X, GlobalPosition.Y, playerPos.Z), Vector3.Up);

			// Stop and attack if close enough
			if (distance <= AttackRange)
			{
				if (_knockbackVelocity.Length() <= 0.5)
				{
					_targetVelocity = Vector3.Zero;
					Velocity = _targetVelocity;
				}
				if (_canAttack && !_attackAnim)
				{
					_attacking = true;
					AttackInitilize();
				}
			}
			else _attacking = false;
		}

		// RANGED ENEMY BEHAVIOR: maintain distance then fire
		else if (!_player._currentBiome.Contains("Village") && playerSpawnDistance <= SpawnRange && _canSeePlayer && !_attackAnim && !Chaser && Stationery == false && Fleeing == false && !_retreating)
		{
			_justWandered = true;
			_player._inCombat = true; _navAgent.TargetPosition = _rangedPosition;
			Vector3 nextPoint = _navAgent.GetNextPathPosition();
			// Apply movement and knockback forces
			if (_knockbackVelocity.Length() > 0.5)
			{
				_targetVelocity = Vector3.Zero + _knockbackVelocity;
				Velocity = _targetVelocity;
			}
			else
			{
				_targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * (RunSpeed * _dashVelocity + _speedOffset);
			}
			// Attack when the monster gets near the finish position or if its been stading still
			if (GlobalPosition.Snapped(0.1f) == _rangedPosition.Snapped(0.1f) || Velocity.Length() <= _veloThreshold)
			{
				_veloThreshold = -5f;
				_targetVelocity = Vector3.Zero;
				Velocity = _targetVelocity;
				AttackInitilize();
			}
			// Make the monster look at where its moving
			Vector3 moveDirection = Velocity.Normalized(); 
			if (moveDirection != Vector3.Zero)
			{
				_lookDirection.LookAt(GlobalTransform.Origin + moveDirection, Vector3.Up); 
			}
		}
		else if (playerSpawnDistance <= SpawnRange && Stationery == true && Fleeing == false && !_retreating)
		{
			_justWandered = true;
			// Rotate monster to face player
			Vector3 playerPos = _player.GlobalPosition;
			_lookDirection.LookAt(new Vector3(playerPos.X, GlobalPosition.Y, playerPos.Z), Vector3.Up);

			// Stop and attack if close enough
			if (distance <= AttackRange && _canAttack == true)
			{
			   _attacking = true;
				AttackInitilize();
			}
			else _attacking = false;
		}
		else if (Fleeing == true && _canSeePlayer && !_retreating)
		{
			// how far the rat will try to get away from the player
			float _fleeDistance = 8f;

			// Navigation pathing away from player
			Vector3 myPos = GlobalTransform.Origin;
			Vector3 playerPos = _player.GlobalPosition;

			// direction from player to rat (i.e. away direction)
			Vector3 awayDir = myPos - playerPos;

			// if positions are equal (rare), pick a fallback direction
			if (awayDir.LengthSquared() < 0.0001f)
			{
				// fallback: use a small offset on X axis so we have a valid direction
				awayDir = new Vector3(1, 0, 0);
			}

			Vector3 fleeTarget = myPos + awayDir.Normalized() * _fleeDistance;
			_navAgent.TargetPosition = fleeTarget;

			Vector3 nextPoint = _navAgent.GetNextPathPosition();

			_targetVelocity = (nextPoint - myPos).Normalized() * (RunSpeed * _dashVelocity + _speedOffset);

			Vector3 moveDirection = _targetVelocity.Normalized(); 
			if (moveDirection != Vector3.Zero)
			{
				_lookDirection.LookAt(GlobalTransform.Origin + moveDirection, Vector3.Up); 
			}
		}
		// WANDERING (Idle state)
		else if (!_attackAnim && (playerSpawnDistance > SpawnRange || !_canSeePlayer || _retreating || _playerBiome.Contains("Village")) && Stationery == false)
		{
			if (!_looking)
			{
				// Move randomly around spawn point
				if (_justWandered && !Fleeing)
				{
					_justWandered = false;
					_targetVelocity = Vector3.Zero;
					Velocity = _targetVelocity;
					ChooseNewWander();
				}
				else
				{
					_navAgent.TargetPosition = _wanderPos;
					Vector3 nextPoint = _navAgent.GetNextPathPosition();
					_targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * (WalkSpeed);
				}

				Vector3 moveDirection = Velocity.Normalized(); 
				if (moveDirection != Vector3.Zero)
				{
					_lookDirection.LookAt(GlobalTransform.Origin + moveDirection, Vector3.Up); 
				}

				// If too far from player, despawn
				if (distance > SpawnDistance) { QueueFree(); }
			}
			else
			{
				float turnSpeed = 0.3f;
				Vector3 currentForward = -_lookDirection.GlobalTransform.Basis.Z;
				Vector3 lookingForward = currentForward.Rotated(Vector3.Up, turnSpeed * (float)delta);

				// Apply the smooth rotation
				_lookDirection.LookAt(
					_lookDirection.GlobalTransform.Origin + lookingForward,
					Vector3.Up
				);

				_lookingTimer += (float)delta;
				if (_lookingTimer >= MaxLookTime)
				{
					_looking = false;
					ChooseNewWander();
				}
			}
		}

		_count += 1;
		if (_count > 1000 || GlobalPosition.Snapped(0.1f) == _wanderPos.Snapped(0.1f))
		{
			_count = _rng.RandfRange(-100, 50);
			_targetVelocity = Vector3.Zero;
			Velocity = _targetVelocity;
			_looking = true;
			_lookingTimer = 0f;
		}

		if (spawnDistance > _currentSpawnRange && !_retreating && _canSeePlayer)
		{
			_currentSpawnRange/=1.2f;
			_retreating = true;
			_wanderPos = _startPos;
		}
		else if (spawnDistance <= _currentSpawnRange && _retreating)
		{
			_currentSpawnRange = SpawnRange;
			_retreating = false;
		}
		if (distance <= AttackRange)
		{
			_currentSpawnRange = SpawnRange;
			_retreating = false;
		}
		// Smooth knockback over time
		_knockbackVelocity = _knockbackVelocity.Lerp(Vector3.Zero, (float)delta * 5.0f);

		// If the monster is stunned then dont let it move
		if (_stunned == true && Stationery == false) { _targetVelocity = Vector3.Zero + _knockbackVelocity; Velocity = _targetVelocity; }

		_targetVelocity = new Vector3(_targetVelocity.X, -9.8f, _targetVelocity.Z);

		// Apply velocity smoothing + movement checks
		Velocity = Velocity.Lerp(_targetVelocity, 4f * (float)delta);

		if (Fleeing){MoveAndSlide();}
		// Things that make the monster stop moving no matter what
		else if (_player._inv.Visible == true || (_stunned == true && _attackException == false) || (_dashVelocity <= 1.01f && _dashAnim == true)) { _targetVelocity = Vector3.Zero; }
		// If the monster isnt a chaser and isnt attacking then it can move
		else if (Chaser == false && _attackAnim == false) { MoveAndSlide(); }
		// If the monster just spawned or is able to move while attacking then it can move
		else if (_justSpawned == true || MoveWhileAttack == true) { MoveAndSlide(); }
		// If the monster isnt attacking, is outside of the attack range and is being knocked back then it can move
		else if (_attackAnim == false && distance > AttackRange && _knockbackVelocity.Length() < 0.5f) { MoveAndSlide(); }
		// If the monster (is being knocked back, or attacking) and the attack exception is false then it cant move
		else if ((_knockbackVelocity.Length() < 0.5f || _attackAnim == true) && _attackException == false) { _targetVelocity = Vector3.Zero; }
		// Else move
		else { MoveAndSlide(); } 
	}


	// --- ATTACK SYSTEM --- //
	// Calls the attack animation/method for the specific monster type
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
			if (Monster is underBrush ub)
			{
				ub._currentAttackOffset = 0f;
			}
			_stunned = true;
			_attackException = true;
			ApplyKnockback();

			// Visual stun effect
			GetNode<GpuParticles3D>("Stunned").Emitting = true;
			await ToSignal(GetTree().CreateTimer(1f), "timeout");
			GetNode<GpuParticles3D>("Stunned").Emitting = false;
			_stunned = false;
		}
	}


	// --- RANGED POSITION GENERATION --- //
	// Picks a random point around the player to stand before shooting
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

	// --- KNOCKBACK FUNCTION --- //
	private void ApplyKnockback()
	{
		Vector3 knockbackDir = (GlobalPosition - _player.Position).Normalized();
		_knockbackVelocity = knockbackDir * _player._knockbackStrength;
		_knockbackVelocity.Y = 0f;
	}

	private async void ChooseNewWander()
	{
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
				if (agroEnteredMonster == this){return;}
				agroEnteredMonster.PackAgro();
			} 
		}
	}

	private void detectPlayer(Area3D area, bool seen, bool agro)
	{
		if (_agroChangeCooldown > 0){return;}
		_agroChangeCooldown = 0.5f;
		if (agro)
		{
			//if (!CheckVision()){return;}
		}
		if (seen)
		{
			if (area.IsInGroup("Player"))
			{
				_canSeePlayer = true;
			}
			else if (area.IsInGroup("Monster"))
			{
				if (_canSeePlayer == true && area.GetParent() is Monster3d visEnteredMonster) 
				{
					if (visEnteredMonster == this){return;}
					//visEnteredMonster.PackAgro();
				}
			}
		}
		else
		{
			if (area.IsInGroup("PlayerHurtbox") && !_playerInWalkRange && !_quitePlayerInRange)
			{
				_canSeePlayer = false;
			}
		}
	}
	
	private void _on_walk_range_area_entered(Area3D area) //When the player gets in range of walk noise detection
	{
		if (area.IsInGroup("PlayerHurtbox")){_playerInWalkRange = true;}
		detectPlayer(area, true, false);
	}

	private void _on_run_range_area_entered(Area3D area) //When the player gets in range of walk run detection
	{
		if (_player._running == true)
		{
			detectPlayer(area, true, false);
		}
		if (area.IsInGroup("PlayerHurtbox")){_quitePlayerInRange = true;}
	}

	private void _on_run_range_area_exited(Area3D area) //When the player leaves the run range
	{
		detectPlayer(area, false, false);
		if (area.IsInGroup("PlayerHurtbox")){_quitePlayerInRange = false; _playerInWalkRange = false;}
	}

	private void _on_agro_range_area_entered(Area3D area) //When the player gets in range of the monster to see them; not agro
	{
		if (area.IsInGroup("PlayerHurtbox")){_playerInVisionRange = true;}
		detectPlayer(area, true, true);
	}
	private void _on_agro_range_area_exited(Area3D area) //When the player gets into the agro range
	{
		if (area.IsInGroup("PlayerHurtbox")){_playerInVisionRange = false;}
		detectPlayer(area, false, true);
	}

	private void PlayerRunCheck()
	{
		if (_quitePlayerInRange)
		{
			detectPlayer(_player.GetNode<Area3D>("Hurtbox"), true, false);
		}
		
	}

	private bool CheckVision()
	{
		// 1. Get the physics space state
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

		// 2. Define the query parameters
		var query = new PhysicsRayQueryParameters3D()
		{
			From = GlobalPosition,
			To = _player.GlobalPosition,
			CollideWithAreas = true, // Set to false if you only want to collide with bodies
			CollideWithBodies = true,
		};

		// 3. Perform the raycast
		var result = spaceState.IntersectRay(query);

		// 4. Check the result
		if (result.Count > 0)
		{
			// An object was hit between point A and point B
			return false; // Path is NOT clear
		}
		else
		{
			// Nothing was hit along the path
			return true; // Path IS clear
		}
	}
}
