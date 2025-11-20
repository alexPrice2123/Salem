using Godot;
using System;

public partial class Camera : Camera3D // Or Camera2D for 2D projects
{
    public float ShakeStrength = 0.0f; // Initial shake strength
    public float ShakeFade = 1f; // How quickly the shake fades out
    public float MaxOffset = 0.5f; // Maximum random offset for position
  	public float MaxRotation = 0.1f; // Maximum random rotation in radians

    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private Vector3 _initialPosition; // Store the camera's initial position
    public Vector3 _initialRotation; // Store the camera's initial rotation

    public override void _Ready()
    {
        _rng.Randomize();
    }

    public override void _Process(double delta)
    {
		_initialPosition = GetParent<Node3D>().GlobalPosition;
        if (ShakeStrength > 0)
        {
            // Reduce shake strength over time
            ShakeStrength = Mathf.Max(0, ShakeStrength - ShakeFade * (float)delta);

            // Generate random offsets for position and rotation
            Vector3 offset = new Vector3(
                _rng.RandfRange(-MaxOffset, MaxOffset),
                _rng.RandfRange(-MaxOffset, MaxOffset),
                _rng.RandfRange(-MaxOffset, MaxOffset)
            );

            Vector3 rotationOffset = new Vector3(
                _rng.RandfRange(-MaxRotation, MaxRotation),
                _rng.RandfRange(-MaxRotation, MaxRotation),
                _rng.RandfRange(-MaxRotation, MaxRotation)
            );

            // Apply the shake
            GlobalPosition = _initialPosition + offset * ShakeStrength;
            Rotation = _initialRotation + rotationOffset * ShakeStrength;
        }
        else
        {
            // Reset to initial position/rotation when shake ends
            GlobalPosition = _initialPosition;
            Rotation = _initialRotation;
        }
    }

    // Call this method to trigger a shake
    public void StartShake(float strength, float fade)
    {
        ShakeStrength = strength;
		ShakeFade = fade;
    }
}