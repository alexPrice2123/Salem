using Godot;
using System;

public partial class Camera : Camera3D
{
    public float ShakeStrength = 0f;
    public float ShakeFade = 1f;

    public float MaxOffset = 0.5f;
    public float MaxRotation = 0.1f;

    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    // This is the *additive* shake offset
    public Vector3 ShakeOffsetPosition = Vector3.Zero;
    public Vector3 ShakeOffsetRotation = Vector3.Zero;

    public override void _Ready()
    {
        _rng.Randomize();
    }

    public override void _Process(double delta)
    {
        if (ShakeStrength > 0)
        {
            ShakeStrength = Mathf.Max(0, ShakeStrength - ShakeFade * (float)delta);

            ShakeOffsetPosition = new Vector3(
                _rng.RandfRange(-MaxOffset, MaxOffset),
                _rng.RandfRange(-MaxOffset, MaxOffset),
                _rng.RandfRange(-MaxOffset, MaxOffset)
            ) * ShakeStrength;

            ShakeOffsetRotation = new Vector3(
                _rng.RandfRange(-MaxRotation, MaxRotation),
                _rng.RandfRange(-MaxRotation, MaxRotation),
                _rng.RandfRange(-MaxRotation, MaxRotation)
            ) * ShakeStrength;
        }
        else
        {
            ShakeOffsetPosition = Vector3.Zero;
            ShakeOffsetRotation = Vector3.Zero;
        }
    }

    public void StartShake(float strength, float fade)
    {
        ShakeStrength = strength;
        ShakeFade = fade;
    }
}
