using Godot;
using System;

public partial class StakeBullet : RigidBody3D
{
    private float _speed = 100.0f;
    private int _pierceCount = 0;
    private int _count = 0;
    public override void _Ready()
    {
        ApplyCentralImpulse(-GlobalTransform.Basis.Z.Normalized() * _speed);
    }
    public override void _PhysicsProcess(double delta)
    {
        LookAt(GlobalTransform.Origin + -LinearVelocity.Normalized());
        if (_pierceCount >= 3) { GD.Print("PierceMax");  QueueFree(); }         // Delete the projectile after piercing 3 enemies
        _count += 1;
        if (_count > 500)
        {
            QueueFree();
        }
    }

    // Change damage of bullet repsectivly    
    public async void CountPierce()
    {
        _pierceCount += 1;                                              // Tell the code how many enemies have been hit
        SetMeta("DamagePer", (float)GetMeta("DamagePer") - 0.05f);      // Change damage depending how many enemies have been hit
    }
}
