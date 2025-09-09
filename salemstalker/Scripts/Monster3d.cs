using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class Monster3d : CharacterBody3D
{
    // --- CONSTANTS ---
    public const float Speed = 5.0f;           // Movement speed of the monster
    public const float JumpVelocity = 6.5f;    // Jump power (currently not used in this script)
    public const float MaxHealth = 5.0f;       // Maximum health of the monster
    public const float Range = 25.0f;          // Detection range for the player (when to start chasing)

    // --- VARIABLES ---
    private Player3d _player;                  // Reference to the player object
    public float _health = MaxHealth;          // Current health, starts at MaxHealth
    private Vector3 _knockbackVelocity = Vector3.Zero; // Stores knockback velocity when hit
    private Vector3 _wanderPos;                // Target position for wandering behavior
    private float _count;                      // Counter for random wander behavior
    private RandomNumberGenerator _rng = new RandomNumberGenerator(); // RNG instance for randomizing positions
    private NavigationAgent3D _navAgent;       // Navigation agent to control monster movement
    private Vector3 startPos;                  // Starting position of the monster, used for wandering area
    private Node3D _hitFX;                     // FX node for visual effects when the monster gets hit
    private Node3D _body;                      // Main body node of the monster
	private bool _canBeHit = true;                     //Checks to make sure the monster isnt double hit
    private Vector3 _currentRot;

    // Called when the node enters the scene tree
    public override void _Ready()
    {
        // Setup references and initialize variables when the monster is ready
        _player = this.GetParent().GetParent().GetParent().GetParent().GetNode<Player3d>("Player_3d");
        _rng.Randomize();
        _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        startPos = GlobalPosition;

        // Set initial wandering target
        float randZ = GlobalPosition.Z + _rng.RandiRange(-50, 50);
        float randX = GlobalPosition.X + _rng.RandiRange(-50, 50);
        _wanderPos = new Vector3(randX, 0f, randZ);

        // Get references to visual effect and body node
        _hitFX = GetNode<Node3D>("HitFX");
        _body = GetNode<Node3D>("Body");
        _currentRot = GlobalRotation;
    }

    // Triggered when the monster's hitbox collides with an Area3D
    private async void _on_hitbox_area_entered(Area3D body)
    {
		// If the monster is hit by the player, process damage and knockback
		if (body.IsInGroup("Player") && _canBeHit == true)
		{
			_canBeHit = false;
			float _damage = _player._damage;  // Get damage from the player

			// Show hit effect and hide body for a short time
			_hitFX.Visible = true;
			_body.Visible = false;

			// Calculate knockback direction and apply it
			Vector3 knockbackDirection = (GlobalPosition - _player.Position).Normalized();
			_knockbackVelocity = knockbackDirection * _player._knockbackStrength;

			// Wait for a short time to show hit effect
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

			// Reset hit effect visibility and update health
			_hitFX.Visible = false;
			_body.Visible = true;

			// Reduce health and destroy monster if health is zero or less
			_health -= _damage;
			if (_health <= 0)
			{
				QueueFree(); // Remove the monster from the scene
			}
			await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
			_canBeHit = true;
        }
    }

    // Physics loop: runs every frame to update movement and behavior
    public override void _PhysicsProcess(double delta)
    {
        // Update wander behavior every 250 frames
        _count += 1;

        // Check distance between monster and player
        float distance = (_player.GlobalPosition - GlobalPosition).Length();
        if (_count >= 250)
        {
            _count = _rng.RandiRange(-100, 50); // Randomize timing for wander
            float randZ = startPos.Z + _rng.RandiRange(-50, 50); // Random wander position on Z axis
            float randX = startPos.X + _rng.RandiRange(-50, 50); // Random wander position on X axis
            _wanderPos = new Vector3(randX, 0f, randZ); // Set new wander target
        }
        // If the player is within range, chase the player
        if (distance <= Range)
        {
            _navAgent.TargetPosition = _player.GlobalPosition;  // Set target position to player's location
            Vector3 nextPoint = _navAgent.GetNextPathPosition();  // Get next path position for movement

            // Move towards the player, applying knockback if necessary
            Velocity = ((nextPoint - GlobalTransform.Origin).Normalized() * Speed) + _knockbackVelocity;

            // Face the player while chasing
            Vector3 playerPos = _player.GlobalPosition;
            LookAt(new Vector3(playerPos.X, GlobalPosition.Y, playerPos.Z), Vector3.Up);
        }
        else // If the player is out of range, wander randomly
        {
            _navAgent.TargetPosition = _wanderPos;  // Set target position to random wander point
            Vector3 nextPoint = _navAgent.GetNextPathPosition();  // Get next path position for wandering

            // Move towards the wander point, applying knockback if necessary
            Velocity = ((nextPoint - GlobalTransform.Origin).Normalized() * Speed) + _knockbackVelocity;

            // Face the wander target while moving
            LookAt(new Vector3(_wanderPos.X, GlobalPosition.Y, _wanderPos.Z), Vector3.Up);
        }

        // Apply movement and update knockback velocity over time
        MoveAndSlide();
        _knockbackVelocity = _knockbackVelocity.Lerp(Vector3.Zero, (float)delta * 5.0f);
    }
}
