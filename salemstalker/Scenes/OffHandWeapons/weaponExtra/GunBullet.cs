using Godot;
using System;
using System.Threading.Tasks;

public partial class GunBullet : RigidBody3D
{
    private float _speed = 10.0f;
    private int _count = 0;
    public override void _PhysicsProcess(double delta)
    {
        ApplyCentralImpulse(GlobalTransform.Basis.Z.Normalized() * _speed);
        _count += 1;
        if (_count > 500)
        {
            GD.Print("bulletDespawn");
            QueueFree();
        }
    }
    
    public async void _on_hit_area_3d_body_entered(Node3D body)
    {
        if (body.IsInGroup("Monster"))
        {
            GD.Print("munchHit");
            QueueFree();
        }
    }
}
