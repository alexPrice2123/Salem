using Godot;
using System;

public partial class Monster3d : CharacterBody3D
{
    //---- THIS IS THE BASE MONSTER SCRIPT ALL VARIABLES WILL BE CHANGED IN THE INDIVIDUAL MONSTER SCRIPTS
    // --- CONSTANTS ---
    
    protected float Speed = 4.5f;             // Movement speed
    protected float MaxHealth = 100.0f;         // Maximum monster health
    protected float Range = 25.0f;            // Detection range for chasing
    protected double SpawnDistance = 100;    // Distance from player before despawning
    protected float BaseDamage = 10.0f;
    protected int WanderRange = 50;
    protected float AttackSpeed = 0.5f;
    protected float AttackRange = 1f;
    protected CharacterBody3D Monster;
    protected bool Chaser = false;

    // --- VARIABLES ---
    protected Player3d _player;                    // Reference to the player
    public float _health;            // Current health of the monster
    protected Vector3 _knockbackVelocity = Vector3.Zero; // Knockback force applied when hit
    protected Vector3 _wanderPos;                  // Current wander target position
    protected float _count;                        // Frame counter used to time wander updates
    protected RandomNumberGenerator _rng = new();  // Random number generator for wander movement
    protected NavigationAgent3D _navAgent;         // Pathfinding/navigation agent
    protected Vector3 _startPos;                   // Starting position (wander center point)
    protected Node3D _hitFX;                       // Hit effect visual node
    protected Node3D _body;                        // Monster body mesh node
    public bool _canBeHit = true;               // Prevents rapid re-hits during invulnerability
    protected Vector3 _currentRot;                 // Stores current rotation of monster
    public bool _attacking = false;
    public bool _canAttack = true;
    protected AnimationPlayer _animPlayer;
    protected float attackOneLength;
    protected CollisionShape3D _attackBox;
    protected bool _hasHit = false;
    protected float _speedOffset = 0f;
    protected float _damageOffset = 0f;
    protected bool _justSpawned = true;
    protected bool _attackAnim = false;
    public bool _attackException = false;
    protected bool _stunned = false;
    protected Vector3 _targetVelocity;
    protected float _dashVelocity = 1f;
    protected Vector3 _rangedPosition;
    protected float _veloThreshold = -5f;
    protected bool _dashAnim = false;
    protected Node3D _lookDirection;

    // --- READY ---
    public void Initialization()
    {
        _player = this.GetParent().GetParent().GetParent().GetParent().GetNode<Player3d>("Player_3d");
        _rng.Randomize();
        _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        _startPos = GlobalPosition;

        // Initialize wander position randomly around spawn
        float randZ = GlobalPosition.Z + _rng.RandiRange(-WanderRange, WanderRange);
        float randX = GlobalPosition.X + _rng.RandiRange(-WanderRange, WanderRange);
        _wanderPos = new Vector3(randX, 0f, randZ);

        _hitFX = GetNode<Node3D>("HitFX");
        _body = GetNode<Node3D>("Body");
        _currentRot = GlobalRotation;
        _attackBox = GetNode<CollisionShape3D>("Attackbox/CollisionShape3D");
        _health = MaxHealth;
        _lookDirection = GetNode<Node3D>("Direction");

        _animPlayer = GetNode<AnimationPlayer>("Body/AnimationPlayer");
        attackOneLength = _animPlayer.GetAnimation("attack").Length;
    }

    // --- DAMAGE HANDLER ---
    public async void Damaged(Area3D body)
    {
        if (body.IsInGroup("Weapon") && _canBeHit)
        {
            _canBeHit = false;
            float damage = _player._damage; // Damage dealt by player
            if (_stunned == true)
            {
                damage *= 1.3f;
            }

            // Swap body FX on hit
            _hitFX.Visible = true;
            _body.Visible = false;
            _canAttack = false;

            // Apply knockback
            Vector3 knockbackDir = (GlobalPosition - _player.Position).Normalized();
            _knockbackVelocity = knockbackDir * _player._knockbackStrength;

            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

            // Reset visuals
            _hitFX.Visible = false;
            _body.Visible = true;

            // Apply damage
            _health -= damage;
        }
    }

    // --- PHYSICS LOOP ---
    public void EveryFrame(double delta)
    {
        float distance = (_player.GlobalPosition - GlobalPosition).Length();
        if (_count > 50 && _justSpawned == true)
        {
            _justSpawned = false;
        }


        // Update wander position every ~250 frames
        if (_count >= 250)
        {
            _count = _rng.RandiRange(-100, 50);
            float randZ = _startPos.Z + _rng.RandiRange(-WanderRange, WanderRange);
            float randX = _startPos.X + _rng.RandiRange(-WanderRange, WanderRange);
            _wanderPos = new Vector3(randX, _player.GlobalPosition.Y, randZ);
        }
        _dashVelocity = Mathf.Lerp(_dashVelocity, 1f, 10f * (float)delta);
        // --- Chase Player ---
        if (distance <= Range && _attackAnim == false && Chaser == true)
        {
            _navAgent.TargetPosition = _player.GlobalPosition;
            Vector3 nextPoint = _navAgent.GetNextPathPosition();
            if (_knockbackVelocity.Length() > 1)
            {
                Velocity = (nextPoint - GlobalTransform.Origin).Normalized() * (Speed * _dashVelocity + _speedOffset) + _knockbackVelocity;
            }
            else
            {
                _targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * (Speed * _dashVelocity + _speedOffset) + _knockbackVelocity;
            }
            // Handle Y-axis alignment
            if (GlobalPosition.Snapped(0.1f).Y == _player.GlobalPosition.Snapped(0.1f).Y)
            {
                _targetVelocity = new Vector3(_targetVelocity.X, 0f, _targetVelocity.Z);
            }
            else if (!IsOnFloor())
            {
                _targetVelocity = new Vector3(_targetVelocity.X, -9.8f, _targetVelocity.Z);
            }

            // Face player
            Vector3 playerPos = _player.GlobalPosition;
            LookAt(new Vector3(playerPos.X, GlobalPosition.Y, playerPos.Z), Vector3.Up);

            _player._inCombat = true;

            if (distance <= AttackRange && _canAttack == true && _attackAnim == false)
            {
                _attacking = true;

                AttackInitilize();
            }
            else if (distance > AttackRange)
            {
                _attacking = false;
            }

        }
        // --- Ranged Enemy Chase ---
        else if (_attackAnim == false && Chaser == false)
        {
            _navAgent.TargetPosition = _rangedPosition;
            Vector3 nextPoint = _navAgent.GetNextPathPosition();
            _targetVelocity = ((nextPoint - GlobalTransform.Origin).Normalized() * Speed) + _knockbackVelocity;

            if (GlobalPosition.Snapped(0.1f) == _rangedPosition.Snapped(0.1f) || Velocity.Length() <= _veloThreshold)
            {
                _veloThreshold = -5f;
                _targetVelocity = Vector3.Zero;
                Velocity = _targetVelocity; 
                AttackInitilize();
            }

            // Handle Y-axis alignment
            if (GlobalPosition.Snapped(0.1f).Y == _player.GlobalPosition.Snapped(0.1f).Y)
            {
                _targetVelocity = new Vector3(_targetVelocity.X, 0f, _targetVelocity.Z);
            }
            else if (!IsOnFloor())
            {
                _targetVelocity = new Vector3(_targetVelocity.X, -9.8f, _targetVelocity.Z);
            }

            // Face wander position
            Vector3 moveDirection = Velocity.Normalized();
            if (moveDirection != Vector3.Zero)
            {
                _lookDirection.LookAt(GlobalTransform.Origin + moveDirection, Vector3.Up);
            }
        }
        // --- Wander ---
        else if (_attackAnim == false)
        {
            _navAgent.TargetPosition = _wanderPos;
            Vector3 nextPoint = _navAgent.GetNextPathPosition();
            _targetVelocity = ((nextPoint - GlobalTransform.Origin).Normalized() * Speed) + _knockbackVelocity;

            // Handle Y-axis alignment
            if (GlobalPosition.Snapped(0.1f).Y == _player.GlobalPosition.Snapped(0.1f).Y)
            {
                _targetVelocity = new Vector3(_targetVelocity.X, 0f, _targetVelocity.Z);
            }
            else if (!IsOnFloor())
            {
                _targetVelocity = new Vector3(_targetVelocity.X, -9.8f, _targetVelocity.Z);
            }

            // Face wander position
            LookAt(new Vector3(_wanderPos.X, GlobalPosition.Y, _wanderPos.Z), Vector3.Up);

            // Despawn if too far from player
            if (distance > SpawnDistance)
            {
                QueueFree();
            }
        }
        Velocity = Velocity.Lerp(_targetVelocity, 2f * (float)delta);

        // --- Movement ---
        if (_player._inv.Visible == true || (_stunned == true && _attackException == false) || (_dashVelocity >= 0.99f && _dashAnim == true)) { _targetVelocity = Vector3.Zero; }
        else if (Chaser == false && _attackAnim == false){ MoveAndSlide(); }
        else if (_justSpawned == true) { MoveAndSlide(); }
        else if (_attackAnim == false && distance > AttackRange && _knockbackVelocity.Length() < 0.5f) { MoveAndSlide(); }
        else if ((_knockbackVelocity.Length() < 0.5f || _attackAnim == true) && _attackException == false) { _targetVelocity = Vector3.Zero; }
        else { MoveAndSlide(); }

        // Smooth knockback decay
        _knockbackVelocity = _knockbackVelocity.Lerp(Vector3.Zero, (float)delta * 5.0f);
    }

    public void AttackInitilize()
    {
        if (_stunned == true) { return; }
        
        if (Monster is TheHollow hollow)
        {
            hollow.Attack();
        }
        else if (Monster is VCultist vCult)
        {
            if (_dashVelocity < 0.99f){ return; }
            vCult.Attack();
        }
    }

    public async void Stunned()
    {
        _stunned = true;
        _attackException = true;
        Vector3 knockbackDir = (GlobalPosition - _player.Position).Normalized();
        _knockbackVelocity = knockbackDir * _player._knockbackStrength;
        GetNode<GpuParticles3D>("Stunned").Emitting = true;
        await ToSignal(GetTree().CreateTimer(1f), "timeout");
        GetNode<GpuParticles3D>("Stunned").Emitting = false;
        _stunned = false;

    }

    public async void RandomRangedPosition()
    {
        _rng.Randomize();
        float randZ = _player.GlobalPosition.Z + _rng.RandiRange(-1, 1) * AttackRange;
        float rangeZ = randZ - _player.GlobalPosition.Z;
        if (rangeZ == 0) { randZ += AttackRange; }
        GD.Print(randZ - _player.GlobalPosition.Z+" Z");
        float randX = _player.GlobalPosition.X + _rng.RandiRange(-1, 1) * AttackRange;
        float rangeX = randX - _player.GlobalPosition.Z;
        if (rangeX == 0) { randX += AttackRange; }
        GD.Print(randX - _player.GlobalPosition.X+" X");
        _rangedPosition = new Vector3(randX, _player.GlobalPosition.Y, randZ);
        await ToSignal(GetTree().CreateTimer(2f), "timeout");    
        _veloThreshold = 0.5f; 
    }
}
