using Godot;
using System;

public partial class Monster3d : CharacterBody3D
{
    // --- CONSTANTS ---
    public const float Speed = 2.5f;             // Movement speed
    public const float JumpVelocity = 6.5f;      // Jump strength (unused)
    public const float MaxHealth = 100.0f;         // Maximum monster health
    public const float Range = 25.0f;            // Detection range for chasing
    private const double SpawnDistance = 100;    // Distance from player before despawning

    // --- VARIABLES ---
    private Player3d _player;                    // Reference to the player
    public float _health = MaxHealth;            // Current health of the monster
    private Vector3 _knockbackVelocity = Vector3.Zero; // Knockback force applied when hit
    private Vector3 _wanderPos;                  // Current wander target position
    private float _count;                        // Frame counter used to time wander updates
    private RandomNumberGenerator _rng = new();  // Random number generator for wander movement
    private NavigationAgent3D _navAgent;         // Pathfinding/navigation agent
    private Vector3 _startPos;                   // Starting position (wander center point)
    private Node3D _hitFX;                       // Hit effect visual node
    private Node3D _body;                        // Monster body mesh node
    private bool _canBeHit = true;               // Prevents rapid re-hits during invulnerability
    private Vector3 _currentRot;                 // Stores current rotation of monster
    public bool _attacking = false;
    public bool _canAttack = true;
    private AnimationPlayer _animPlayer;
    private float attackOneLength;
    private CollisionShape3D _attackBox;
    private bool _hasHit = false;
    private float _speedChange = 0f;
    private float _damage = 15f;

    // --- READY ---
    public override void _Ready()
    {
        _player = this.GetParent().GetParent().GetParent().GetParent().GetNode<Player3d>("Player_3d");
        _rng.Randomize();
        _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        _startPos = GlobalPosition;

        // Initialize wander position randomly around spawn
        float randZ = GlobalPosition.Z + _rng.RandiRange(-50, 50);
        float randX = GlobalPosition.X + _rng.RandiRange(-50, 50);
        _wanderPos = new Vector3(randX, 0f, randZ);

        _hitFX = GetNode<Node3D>("HitFX");
        _body = GetNode<Node3D>("Body");
        _currentRot = GlobalRotation;
        _attackBox = GetNode<CollisionShape3D>("Attackbox/CollisionShape3D");

        _animPlayer = GetNode<AnimationPlayer>("Body/AnimationPlayer");
        attackOneLength = _animPlayer.GetAnimation("attack").Length;
    }

    // --- DAMAGE HANDLER ---
    private async void _on_hurtbox_area_entered(Area3D body)
    {
        if (body.IsInGroup("Weapon") && _canBeHit)
        {
            _canBeHit = false;
            float damage = _player._damage; // Damage dealt by player

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
            if (_health <= 0)
            {
                _player.MonsterKilled("Monster");
                QueueFree(); // Destroy monster when health hits zero
            }

            // Short invulnerability window
            await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
            _canBeHit = true;
        }
    }

    // --- PHYSICS LOOP ---
    public override void _PhysicsProcess(double delta)
    {
        _count += 1;
        float distance = (_player.GlobalPosition - GlobalPosition).Length();

        // Update wander position every ~250 frames
        if (_count >= 250)
        {
            _count = _rng.RandiRange(-100, 50);
            float randZ = _startPos.Z + _rng.RandiRange(-50, 50);
            float randX = _startPos.X + _rng.RandiRange(-50, 50);
            _wanderPos = new Vector3(randX, _player.GlobalPosition.Y, randZ);
        }

        // --- Chase Player ---
        if (distance <= Range)
        {
            _navAgent.TargetPosition = _player.GlobalPosition;
            Vector3 nextPoint = _navAgent.GetNextPathPosition();
            Velocity = (nextPoint - GlobalTransform.Origin).Normalized() * (Speed + _speedChange) + _knockbackVelocity;

            if (distance < 5)
            {
                _speedChange = 2.5f;
            }
            else
            {
                _speedChange = 0f;
            }

            // Handle Y-axis alignment
            if (GlobalPosition.Snapped(0.1f).Y == _player.GlobalPosition.Snapped(0.1f).Y)
            {
                Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);
            }
            else if (!IsOnFloor())
            {
                Velocity = new Vector3(Velocity.X, -9.8f, Velocity.Z);
            }

            // Face player
            Vector3 playerPos = _player.GlobalPosition;
            LookAt(new Vector3(playerPos.X, GlobalPosition.Y, playerPos.Z), Vector3.Up);

            _player._inCombat = true;

            if (distance <= 2f && _canAttack == true)
            {
                _attacking = true;
                Attack(2.5f);
            }
            else if (distance > 2f)
            {
                _attacking = false;
            }
        }
        // --- Wander ---
        else
        {
            _navAgent.TargetPosition = _wanderPos;
            Vector3 nextPoint = _navAgent.GetNextPathPosition();
            Velocity = ((nextPoint - GlobalTransform.Origin).Normalized() * Speed) + _knockbackVelocity;

            // Handle Y-axis alignment
            if (GlobalPosition.Snapped(0.1f).Y == _player.GlobalPosition.Snapped(0.1f).Y)
            {
                Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);
            }
            else if (!IsOnFloor())
            {
                Velocity = new Vector3(Velocity.X, -9.8f, Velocity.Z);
            }

            // Face wander position
            LookAt(new Vector3(_wanderPos.X, GlobalPosition.Y, _wanderPos.Z), Vector3.Up);

            // Despawn if too far from player
            if (distance > SpawnDistance)
            {
                QueueFree();
            }
        }

        // --- Movement ---
        if (_player._inv.Visible == false)
        {
            MoveAndSlide();
        }

        // Smooth knockback decay
        _knockbackVelocity = _knockbackVelocity.Lerp(Vector3.Zero, (float)delta * 5.0f);
    }

    private async void Attack(float delayLength)
    {
        _attackBox.Disabled = false;
        _hasHit = false;
        await ToSignal(GetTree().CreateTimer(attackOneLength), "timeout");
        _attackBox.Disabled = true;
        _canAttack = false;
        await ToSignal(GetTree().CreateTimer(delayLength), "timeout");
        _canAttack = true;
    }

    private void _on_attackbox_area_entered(Node3D body)
    {
        if (body.IsInGroup("Player") && _hasHit == false && body.Name == "Hurtbox")
        {
            _player._health -= 10f;
            _attackBox.Disabled = true;
            _hasHit = true;
        }
    }
}
