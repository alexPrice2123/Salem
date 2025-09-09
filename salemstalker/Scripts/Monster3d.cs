using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class Monster3d : CharacterBody3D
{
	// --- CONSTANTS ---
	public const float Speed = 5.0f;           // Movement speed of the monster
	public const float JumpVelocity = 6.5f;    // Jump power (not used in this script)
	public const float MaxHealth = 5.0f;       // Maximum health of the monster
	public const float Range = 25.0f;          // Detection range for the player

	// --- VARIABLES ---
	private Player3d _player;                  // Reference to the player object
	private StandardMaterial3D _material;      // Material of the monster for visual feedback
	public float _health = MaxHealth;          // Current health (starts at MaxHealth)
	private Vector3 _knockbackVelocity = Vector3.Zero; // Stores knockback effect when hit
	private Vector3 _wanderPos;                // Target position when wandering
	private float _count;                      // Counter used for timing wander changes
	private RandomNumberGenerator _rng = new RandomNumberGenerator(); // RNG for wander positions
	private NavigationAgent3D _navAgent;       // Navigation agent for pathfinding
	private Vector3 startPos;                  // Starting position (used for wandering area)

	// Called when the node enters the scene tree
	public override void _Ready()
	{
		// Get reference to the player
		GD.Print(this.GetParent().GetParent().GetParent().Name);
		_player = this.GetParent().GetParent().GetParent().GetNode<Player3d>("Player_3d");

		// Get and configure material overlay for damage effects
		_material = GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverlay as StandardMaterial3D;
		GetNode<MeshInstance3D>("MeshInstance3D").SetSurfaceOverrideMaterial(0, _material);
		_material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
		_material.EmissionEnabled = true;

		// Start material fully transparent
		Color basecolor = _material.AlbedoColor;
		basecolor.A = 0.0f;
		_material.AlbedoColor = basecolor;

		// Initialize RNG
		_rng.Randomize();

		// Get navigation agent for movement/pathfinding
		_navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

		// Store starting position
		startPos = GlobalPosition;
		
		//Sets inital wander location
		float randZ = GlobalPosition.Z + _rng.RandiRange(-50, 50);
		float randX = GlobalPosition.X + _rng.RandiRange(-50, 50);
		_wanderPos = new Vector3(randX, 0f, randZ);
	}

	// Triggered when the monster's hitbox collides with an Area3D
	private async void _on_hitbox_area_entered(Area3D body)
	{
		GD.Print(body);

		// Check if collider belongs to the player
		if (body.IsInGroup("Player"))
		{
			// Take damage from the player's damage value
			float _damage = _player._damage;

			// Make monster flash visible (semi-transparent) when hit
			Color basecolor = _material.AlbedoColor;
			basecolor.A = 0.5f;

			// Apply knockback in opposite direction of the player
			Vector3 knockbackDirection = (GlobalPosition - _player.Position).Normalized();
			_knockbackVelocity = knockbackDirection * _player._knockbackStrength;
			_material.AlbedoColor = basecolor;

			// Wait 0.1s then reset transparency
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			basecolor.A = 0.0f;

			// Reset material transparency
			_material.AlbedoColor = basecolor;

			// Reduce health
			_health -= _damage;

			// Destroy monster if health reaches 0
			if (_health <= 0){
				QueueFree();
			}
		}
	}

	// Physics loop: runs every frame
	public override void _PhysicsProcess(double delta)
	{
		// Wander counter increments each frame
		_count += 1;

		// Every ~250 frames, pick a new wander position
		if (_count >= 250)
		{
			_count = _rng.RandiRange(-100, 50); // Adds randomness to timing
			float randZ = startPos.Z + _rng.RandiRange(-50, 50);
			float randX = startPos.X + _rng.RandiRange(-50, 50);
			_wanderPos = new Vector3(randX, 0f, randZ);
		}

		// Calculate distance to player
		float distance = (_player.GlobalPosition - GlobalPosition).Length();

		if (distance <= Range) // If player is close enough
		{
			// Move toward player
			_navAgent.TargetPosition = _player.GlobalPosition;
			Vector3 nextPoint = _navAgent.GetNextPathPosition();
			Velocity = ((nextPoint - GlobalTransform.Origin).Normalized() * Speed) + _knockbackVelocity;

			// Face the player while chasing
			Vector3 playerPos = _player.GlobalPosition;
			LookAt(new Vector3(playerPos.X, GlobalPosition.Y, playerPos.Z), Vector3.Up);
		}
		else // If player is far, wander randomly
		{
			_navAgent.TargetPosition = _wanderPos;
			Vector3 nextPoint = _navAgent.GetNextPathPosition();
			Velocity = ((nextPoint - GlobalTransform.Origin).Normalized() * Speed) + _knockbackVelocity;

			// Face wander target
			LookAt(new Vector3(_wanderPos.X, GlobalPosition.Y, _wanderPos.Z), Vector3.Up);
		}

		// Apply movement
		MoveAndSlide();

		// Smoothly reduce knockback velocity over time
		_knockbackVelocity = _knockbackVelocity.Lerp(Vector3.Zero, (float)delta * 5.0f);
	}
}
