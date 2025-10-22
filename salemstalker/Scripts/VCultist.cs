using Godot;
using System;

public partial class VCultist : Monster3d
{
    // Called when the node enters the scene tree for the first time.

    private PackedScene _darkOrb = GD.Load<PackedScene>("res://Scenes/Monsters/MonsterAssets/orb.tscn"); // Scene reference to the dark orb
    private Node3D _spawn;
    private float _projectileSpeed = 25f;
    private float _dashRange = 5f;
    private bool _dashing = false;
    private bool _dashAnim = false;
    private GpuParticles3D _leftArmMagic;
    private GpuParticles3D _rightArmMagic;
    private GpuParticles3D _magicOrbParticle;
    private MeshInstance3D _orb;
    private Vector3 _orbGoal = new Vector3(0f, 0f, 0f);
    private float _orbTweenTime = 1f;
    public override void _Ready()
    {
        Speed = 4.5f;             // Movement speed
        MaxHealth = 100.0f;         // Maximum monster health
        Range = 50.0f;            // Detection range for chasing
        SpawnDistance = 100;    // Distance from player before despawning
        BaseDamage = 25.0f;
        WanderRange = 50;
        AttackSpeed = 4f;
        AttackRange = 15f;
        Monster = this;

        _spawn = GetNode<Node3D>("Spawn");
        Initialization();

        _leftArmMagic = GetNode<GpuParticles3D>("Body/metarig/Skeleton3D/arur_l/arur_l/Magic");
        _rightArmMagic = GetNode<GpuParticles3D>("Body/metarig/Skeleton3D/arua_r/arua_r/Magic");
        _magicOrbParticle = GetNode<GpuParticles3D>("Magic");
        _orb = GetNode<MeshInstance3D>("Orb");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        EveryFrame(delta);
        float distance = (_player.GlobalPosition - GlobalPosition).Length();
        if (distance <= _dashRange && _canAttack == false && _attackAnim == false)
        {
            if (_dashing == false)
            {
                Dash();
            }
        }
        else
        {
            _dashing = false;
            _dashAnim = false;
        }
        if (_dashVelocity < 0.99f)
        {
            MoveAndSlide();
        }
        if (_health <= 0)
        {
            _player.MonsterKilled("VCultist");
            QueueFree(); // Destroy monster when health hits zero
        }
        _orb.Scale = _orb.Scale.Lerp(_orbGoal, _orbTweenTime * (float)delta);
    }
    
    private async void Dash()
    {
        _dashAnim = true;
        await ToSignal(GetTree().CreateTimer(1), "timeout");
        _dashVelocity = -3f;
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
        _rightArmMagic.Emitting = false;
        _leftArmMagic.Emitting = false;
        _magicOrbParticle.Emitting = true;
        _orbTweenTime = 1f;
        _orbGoal = new Vector3(-0.2f, -0.2f, -0.2f);
        _canAttack = false;
        _attackAnim = true;
        await ToSignal(GetTree().CreateTimer(AttackSpeed - 1), "timeout");
        _canAttack = true;
        await ToSignal(GetTree().CreateTimer(0.92), "timeout");
        RigidBody3D projectileInstance = _darkOrb.Instantiate<RigidBody3D>(); // Create monster instance
        _player.GetParent().AddChild(projectileInstance);                                             // Add monster to holder node
        projectileInstance.GlobalPosition = _spawn.GlobalPosition;
        _rightArmMagic.Emitting = true;
        _leftArmMagic.Emitting = true;
        _orbTweenTime = 100f;
        _orbGoal = new Vector3(0f, 0f, 0f);
        _magicOrbParticle.Emitting = false;
        if (projectileInstance is Orb orb)
        {
            orb._playerOrb = _player;
            orb._damageOrb = BaseDamage + _damageOffset;
            orb.Shoot(_projectileSpeed);
        }
        await ToSignal(GetTree().CreateTimer(0.5), "timeout");
        _attackAnim = false;
        _canAttack = false;
        await ToSignal(GetTree().CreateTimer(1), "timeout");
        _canAttack = true;
	}
}
