using Godot;
using System;

public partial class Flintlock : Node3D
{
    private PackedScene _gunBullet = GD.Load<PackedScene>("res://Scenes/OffHandWeapons/weaponExtra/gun_bullet.tscn");
    private SpotLight3D _flash;
    private GpuParticles3D _smoke;
    private Marker3D _barrel;
    private Marker3D _direct;

    public override void _Ready()
    {
        _flash = GetNode<SpotLight3D>("Flash");
        _smoke = GetNode<GpuParticles3D>("Smoke");
        _barrel = GetNode<Marker3D>("BarrelPos");
        _direct = GetNode<Marker3D>("DirectionPos");
    }
    public async void specAction()
    {
        RigidBody3D temPrj = _gunBullet.Instantiate<RigidBody3D>();
        AddSibling(temPrj);
        temPrj.Position = _barrel.Position;
        temPrj.Rotation = _barrel.Rotation;
        muzzleFlash();
    }
    
    private async void muzzleFlash()
    {
        _smoke.Emitting = true;
        _flash.Visible = true;
        await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
        _flash.Visible = false;
        _smoke.Emitting = false;
    }
}
