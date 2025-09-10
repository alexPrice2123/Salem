using Godot;
using System;

public partial class Monster3d : CharacterBody3D
{
    // --- CONSTANTS ---
    public const float Speed = 5.0f;            // Movement speed
    public const float JumpVelocity = 6.5f;     // Jump strength (unused)
    public const float MaxHealth = 5.0f;        // Maximum health
    public const float Range = 25.0f;           // Detection range for chasing

    // --- VARIABLES ---
    private Player3d _player;                   // Reference to the player
    public float _health = MaxHealth;           // Current monster health
    private Vector3 _knockbackVelocity = Vector3.Zero; // Knockback velocity applied when hit
    private Vector3 _wanderPos;                 // Current target position for wandering
    private float _count;                       // Frame counter for wander timing
    private RandomNumberGenerator _rng = new(); // RNG for randomizing wander positions
    private NavigationAgent3D _navAgent;        // Navigation/pathfinding component
    private Vector3 startPos;                   // Starting position, used as wander center
    private Node3D _hitFX;                      // Visual effect node shown when hit
    private Node3D _body;                       // Monster body mesh
    private bool _canBeHit = true;              // Prevents being hit multiple times in quick succession
    private Vector3 _currentRot;                // Tracks current rotation

    // --- READY ---
    public override void _Ready()
    {
        _player = this.GetParent().GetParent().GetParent().GetParent().GetNode<Player3d>("Player_3d");
        _rng.Randomize();
        _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        startPos = GlobalPosition;

        float randZ = GlobalPosition.Z + _rng.RandiRange(-50, 50);
        float randX = GlobalPosition.X + _rng.RandiRange(-50, 50);
        _wanderPos = new Vector3(randX, 0f, randZ);

        _hitFX = GetNode<Node3D>("HitFX");
        _body = GetNode<Node3D>("Body");
        _currentRot = GlobalRotation;
    }

    // --- DAMAGE HANDLER ---
    private async void _on_hitbox_area_entered(Area3D body)
    {
        if (body.IsInGroup("Player") && _canBeHit)
        {
            _canBeHit = false;
            float _damage = _player._damage; // Damage dealt by the player

            _hitFX.Visible = true;
            _body.Visible = false;

            Vector3 knockbackDirection = (GlobalPosition - _player.Position).Normalized();
            _knockbackVelocity = knockbackDirection * _player._knockbackStrength;

            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

            _hitFX.Visible = false;
            _body.Visible = true;

            _health -= _damage;
            if (_health <= 0)
            {
                QueueFree(); // Destroy monster if health reaches zero
            }

            await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
            _canBeHit = true;
        }
    }

    // --- PHYSICS LOOP ---
    public override void _PhysicsProcess(double delta)
    {
        _count += 1;
        float distance = (_player.GlobalPosition - GlobalPosition).Length();

        // Update wander target every ~250 frames
        if (_count >= 250)
        {
            _count = _rng.RandiRange(-100, 50);
            float randZ = startPos.Z + _rng.RandiRange(-50, 50);
            float randX = startPos.X + _rng.RandiRange(-50, 50);
            _wanderPos = new Vector3(randX, 0f, randZ);
        }

        // --- Chase Player ---
        if (distance <= Range)
        {
            _navAgent.TargetPosition = _player.GlobalPosition;
            Vector3 nextPoint = _navAgent.GetNextPathPosition();
            Velocity = ((nextPoint - GlobalTransform.Origin).Normalized() * Speed) + _knockbackVelocity;

            Vector3 playerPos = _player.GlobalPosition;
            LookAt(new Vector3(playerPos.X, GlobalPosition.Y, playerPos.Z), Vector3.Up);
            _player._inCombat = true;
        }
        // --- Wander ---
        else
        {
            _navAgent.TargetPosition = _wanderPos;
            Vector3 nextPoint = _navAgent.GetNextPathPosition();
            Velocity = ((nextPoint - GlobalTransform.Origin).Normalized() * Speed) + _knockbackVelocity;

            LookAt(new Vector3(_wanderPos.X, GlobalPosition.Y, _wanderPos.Z), Vector3.Up);
        }

        // --- Movement ---
        if (_player._inv.Visible == false)
        {
            MoveAndSlide(); 
        }
        _knockbackVelocity = _knockbackVelocity.Lerp(Vector3.Zero, (float)delta * 5.0f);
    }
}
