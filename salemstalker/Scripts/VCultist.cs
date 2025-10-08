using Godot;
using System;

public partial class VCultist : Monster3d
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Speed = 4.5f;             // Movement speed
        MaxHealth = 100.0f;         // Maximum monster health
        Range = 50.0f;            // Detection range for chasing
        SpawnDistance = 100;    // Distance from player before despawning
        BaseDamage = 15.0f;
        WanderRange = 50;
        AttackSpeed = 2.5f;
        AttackRange = 15f;
        Monster = this;
        Initialization();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        float distance = (_player.GlobalPosition - GlobalPosition).Length();
        if (distance <= AttackRange)
        {

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
		_attackBox.Disabled = false;
        _hasHit = false;
        await ToSignal(GetTree().CreateTimer(attackOneLength), "timeout");
        _attackBox.Disabled = true;
        _canAttack = false;
        await ToSignal(GetTree().CreateTimer(AttackSpeed), "timeout");
        _canAttack = true;
	}
}
