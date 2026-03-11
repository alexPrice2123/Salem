using Godot;
using System;
using System.Security.Cryptography;

public partial class Flintlock : Node3D
{
    private PackedScene _gunBullet = GD.Load<PackedScene>("res://Scenes/OffHandWeapons/weaponExtra/gun_bullet.tscn");
    private SpotLight3D _flash;
    private GpuParticles3D _smoke;
    private Marker3D _barrel;
    private Marker3D _direct;
    private bool shooting = false;

    public override void _Ready()
    {
        _flash = GetNode<SpotLight3D>("Flash");
        _smoke = GetNode<GpuParticles3D>("Smoke");
        _barrel = GetNode<Marker3D>("BarrelPos");

    }
    public async void specAction()
    {
        shooting = true;
        await ToSignal(GetTree().CreateTimer(0.7f), "timeout");
        RigidBody3D temPrj = _gunBullet.Instantiate<RigidBody3D>();
        AddSibling(temPrj);
        temPrj.Position = _barrel.Position;
        temPrj.Rotation = _barrel.Rotation;
        temPrj.Reparent(temPrj.GetParent().GetParent().GetParent().GetParent().GetParent());
        muzzleFlash();
        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
        shooting = false;
    }
    
    private async void muzzleFlash()
    {
        _smoke.Emitting = true;
        await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
        _flash.Visible = true;
        await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
        _flash.Visible = false;
        _smoke.Emitting = false;
    }
}
