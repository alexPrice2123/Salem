using Godot;
using System;

public partial class Monster3d : CharacterBody3D
{
    //---- THIS IS THE BASE MONSTER SCRIPT ALL VARIABLES WILL BE CHANGED IN THE INDIVIDUAL MONSTER SCRIPTS


    // --- CONSTANTS ---
    protected float Speed = 4.5f;               // Movement speed
    protected float MaxHealth = 100.0f;         // Maximum monster health
    protected float Range = 25.0f;              // Detection range for chasing
    protected double SpawnDistance = 100;       // Distance from player before despawning
    protected float BaseDamage = 10.0f;         // Base damage of the monster
    protected int WanderRange = 50;             // The range the monster can wander from its spawn point
    protected float AttackSpeed = 0.5f;         // The time between its attacks
    protected float AttackRange = 1f;           // The distance the monster gets from the player before stopping and attacking
    protected CharacterBody3D Monster;          // A reference to the monster
    protected bool Chaser = false;              // If this monster chasing the player or finds a point within a range of the player
    protected bool MoveWhileAttack = false;     // Can this monster move while attacking
    protected bool Flying = false;              // Should gravity be applied to this monster
    public bool Debug = false;                  // If true this monster wont move or attack

    // --- NODE REFERENCES ---
    protected Player3d _player;                 // Reference to the player
    protected NavigationAgent3D _navAgent;      // Pathfinding/navigation agent
    protected Node3D _hitFX;                    // Hit effect visual node
    protected Node3D _body;                     // Monster body mesh node
    protected CollisionShape3D _attackBox;      // The attack box of the monster
    protected Node3D _lookDirection;            // The goal look direction that the monster should lerp to

    // --- VARIABLES ---
    public float _health;                       // Current health of the monster
    protected Vector3 _knockbackVelocity = Vector3.Zero; // Knockback force applied when hit
    protected Vector3 _wanderPos;               // Current wander target position
    protected float _count;                     // Frame counter used to time wander updates
    protected RandomNumberGenerator _rng = new(); // Random number generator for wander movement
    protected Vector3 _startPos;                // Starting position (wander center point)
    public bool _canBeHit = true;               // Prevents rapid re-hits during invulnerability
    protected Vector3 _currentRot;              // Stores current rotation of monster
    public bool _attacking = false;             // Is the monster attacking
    public bool _canAttack = true;              // Is the monster able to attack
    protected bool _hasHit = false;             // Has the monster been hit by the player (so it doesnt get double hit)
    protected float _speedOffset = 0f;          // Number that adds to the speed
    protected float _damageOffset = 0f;         // Number that adds to the damage
    protected bool _justSpawned = true;         // Did this monster just spawn (giving 50 frames to move away from the hut before it does anything)
    protected bool _attackAnim = false;         // Should the attack anim for the monster be playing
    public bool _attackException = false;       // An exception to anything causing the monster not to move
    protected bool _stunned = false;            // Is the monster stunned
    protected Vector3 _targetVelocity;          // The velocity that the monster will lerp towards
    protected float _dashVelocity = 1f;         // The velocity added onto target velocity from the monsters dash
    protected Vector3 _rangedPosition;          // The point chosen by non chaser monsters to go to
    protected float _veloThreshold = -5f;       // The velocity threshold that ranged monsters have to get to, to stop and attack
    protected bool _dashAnim = false;           // Should the dash anim be playing

    // --- READY --- //
    public void Initialization()
    {
        _player = this.GetParent().GetParent().GetParent().GetParent().GetNode<Player3d>("Player_3d");
        _rng.Randomize();
        _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        _startPos = GlobalPosition;

        // Pick initial random wander target
        float randZ = GlobalPosition.Z + _rng.RandiRange(-WanderRange, WanderRange);
        float randX = GlobalPosition.X + _rng.RandiRange(-WanderRange, WanderRange);
        _wanderPos = new Vector3(randX, 0f, randZ);

        // Assign visual and functional nodes
        _hitFX = GetNode<Node3D>("HitFX");
        _body = GetNode<Node3D>("Body");
        _currentRot = GlobalRotation;
        _attackBox = GetNode<CollisionShape3D>("Attackbox/CollisionShape3D");
        _health = MaxHealth;
        _lookDirection = GetNode<Node3D>("Direction");
    }


    // --- DAMAGE SYSTEM --- //
    // Checks what hit the monster and applies damage appropriately
    public void Damaged(Area3D body)
    {
        if (body.IsInGroup("Weapon") && _canBeHit)
        {
            DamageHandler(true, _player._damage);
        }
        else if (body.IsInGroup("PlayerProj") && _canBeHit)
        {
            float damage = MaxHealth * (float)body.GetParent().GetMeta("DamagePer");
            DamageHandler(false, damage);
        }
    }

    private async void DamageHandler(bool knockBack, float damage)
    {
        // Increased damage if stunned
        if (_stunned) { damage *= 1.3f; }

        // Optional knockback force
        if (knockBack == true) { ApplyKnockback(); }

        // Quick visual hit reaction
        _hitFX.Visible = true;
        _body.Visible = false;
        _canAttack = false;
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

        // Reduce health
        _health -= damage;

        _hitFX.Visible = false;
        _body.Visible = true;
    }


    // --- CORE MONSTER AI LOOP --- //
    // Handles state: chase, attack, wander, despawn
    public void EveryFrame(double delta)
    {
        if (Debug == true)
        {
            return;
        }
        float distance = (_player.GlobalPosition - GlobalPosition).Length();

        // Delay initial behavior when first spawned
        if (_count > 50 && _justSpawned)
            _justSpawned = false;

        // Occasionally pick new wander location
        if (_count >= 250)
        {
            _count = _rng.RandiRange(-100, 50);
            float randZ = _startPos.Z + _rng.RandiRange(-WanderRange, WanderRange);
            float randX = _startPos.X + _rng.RandiRange(-WanderRange, WanderRange);
            _wanderPos = new Vector3(randX, _player.GlobalPosition.Y, randZ);
        }

        _dashVelocity = Mathf.Lerp(_dashVelocity, 1f, 15f * (float)delta);

        // CHASE MODE: If player close enough and monster is a chaser
        if (distance <= Range && (Chaser && !_attackAnim || MoveWhileAttack && Chaser))
        {
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
                _targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * (Speed * _dashVelocity + _speedOffset);
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
        else if (!_attackAnim && !Chaser)
        {
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
                _targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * (Speed * _dashVelocity + _speedOffset);
            }
            // Attack when the monster gets near the finish position or if its been stading still
            if (GlobalPosition.Snapped(0.1f) == _rangedPosition.Snapped(0.1f) || Velocity.Length() <= _veloThreshold)
            {
                _veloThreshold = -5f;
                _targetVelocity = Vector3.Zero;
                Velocity = _targetVelocity;
                AttackInitilize();
            }
            //Only apply gravity if not on the same Y as the player
            if (GlobalPosition.Snapped(0.1f).Y == _player.GlobalPosition.Snapped(0.1f).Y || Flying == true)
            {
                _targetVelocity = new Vector3(_targetVelocity.X, 0f, _targetVelocity.Z);
            }
            else if (!IsOnFloor())
            {
                _targetVelocity = new Vector3(_targetVelocity.X, -9.8f, _targetVelocity.Z);
            }
            // Make the monster look at where its moving
            Vector3 moveDirection = Velocity.Normalized(); 
            if (moveDirection != Vector3.Zero)
            {
                _lookDirection.LookAt(GlobalTransform.Origin + moveDirection, Vector3.Up); 
            }
        }

        // WANDERING (Idle state)
        else if (!_attackAnim)
        {
            // Move randomly around spawn point
            _navAgent.TargetPosition = _wanderPos;
            Vector3 nextPoint = _navAgent.GetNextPathPosition();
            _targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * Speed;

            // Make the monster look at where its moving
            Vector3 moveDirection = Velocity.Normalized(); 
            if (moveDirection != Vector3.Zero)
            {
                _lookDirection.LookAt(GlobalTransform.Origin + moveDirection, Vector3.Up); 
            }

            // If too far from player, despawn
            if (distance > SpawnDistance) { QueueFree(); }
        }

        // Smooth knockback over time
        _knockbackVelocity = _knockbackVelocity.Lerp(Vector3.Zero, (float)delta * 5.0f);

        // If the monster is stunned then dont let it move
        if (_stunned == true) { _targetVelocity = Vector3.Zero + _knockbackVelocity; Velocity = _targetVelocity; }

        // Apply velocity smoothing + movement checks
        Velocity = Velocity.Lerp(_targetVelocity, 4f * (float)delta);

        // Things that make the monster stop moving no matter what
        if (_player._inv.Visible == true || (_stunned == true && _attackException == false) || (_dashVelocity <= 1.01f && _dashAnim == true)) { _targetVelocity = Vector3.Zero; }
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
        else if (Monster is vCultist vc) vc.Attack();
        else if (Monster is flyingPesk fp ) fp.Fly();
    }


    // --- STUN EFFECT --- //
    public async void Stunned()
    {
        _stunned = true;
        _attackException = true;
        ApplyKnockback();

        // Visual stun effect
        GetNode<GpuParticles3D>("Stunned").Emitting = true;
        await ToSignal(GetTree().CreateTimer(1f), "timeout");
        GetNode<GpuParticles3D>("Stunned").Emitting = false;

        _stunned = false;
    }


    // --- RANGED POSITION GENERATION --- //
    // Picks a random point around the player to stand before shooting
    public async void RandomRangedPosition()
    {
        _rng.Randomize();
        float randomRadius = Mathf.Sqrt(GD.Randf()) * AttackRange;
        float angle = GD.Randf() * 2 * MathF.PI;

        Vector3 center = _player.GlobalPosition;
        _rangedPosition = new Vector3(
            center.X + randomRadius * Mathf.Cos(angle),
            center.Y,
            center.Z + randomRadius * Mathf.Sin(angle)
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
}