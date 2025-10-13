using Godot;
using System;

public partial class VCultist : Monster3d
{
    // Called when the node enters the scene tree for the first time.

    private PackedScene _darkOrb = GD.Load<PackedScene>("res://Scenes/Monsters/MonsterAssets/orb.tscn"); // Scene reference to the dark orb
    private Node3D _spawn;
    private float _projectileSpeed = 15f;
    public override void _Ready()
    {
        Speed = 4.5f;             // Movement speed
        MaxHealth = 100.0f;         // Maximum monster health
        Range = 50.0f;            // Detection range for chasing
        SpawnDistance = 100;    // Distance from player before despawning
        BaseDamage = 15.0f;
        WanderRange = 50;
        AttackSpeed = 6f;
        AttackRange = 15f;
        Monster = this;

        _spawn = GetNode<Node3D>("Body/metarig/Skeleton3D/Cylinder/Spawn");
        Initialization();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_health <= 0)
        {
            _player.MonsterKilled("VCultist");
            QueueFree(); // Destroy monster when health hits zero
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
            _player._health -= BaseDamage + _damageOffset;
            _attackBox.Disabled = true;
            _hasHit = true;
        }
    }

    public async void Attack()
    {
        _canAttack = false;
        _attackAnim = true;
        await ToSignal(GetTree().CreateTimer(AttackSpeed), "timeout");
        RigidBody3D projectileInstance = _darkOrb.Instantiate<RigidBody3D>(); // Create monster instance
        _player.GetParent().AddChild(projectileInstance);                                             // Add monster to holder node
        projectileInstance.GlobalPosition = _spawn.GlobalPosition;
        if (projectileInstance is Orb orb)
        {
            orb._playerOrb = _player;
            orb._damageOrb = BaseDamage + _damageOffset;
            orb.Shoot(_projectileSpeed);
        }
        _canAttack = true;
        await ToSignal(GetTree().CreateTimer(0.5), "timeout");
        _attackAnim = false;
	}
}
