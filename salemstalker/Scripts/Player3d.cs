using Godot;
using System;

public partial class Player3d : CharacterBody3D
{
	public const float Speed = 5.0f; //Player speed
	public const float JumpVelocity = 4.5f; //Jump power
	public const float CamSense = 0.006f; //Sensitivity

	private Node3D _head;
	private Camera3D _cam;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_head = GetNode<Node3D>("Head"); //Declares the head componant
		_cam = GetNode<Camera3D>("Head/Camera3D"); //Declares the camera componant
    }

	public override void _Input(InputEvent @event) //When the player does any input
	{
		if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured) //When the input is mouse movment
		{
			_head.RotateY(-motion.Relative.X * CamSense); //Rotate the head along the X axis
			_cam.RotateX(-motion.Relative.Y * CamSense); //Rotate the camera along the Y axis

			Vector3 camRot = _cam.Rotation;
			camRot.X = Mathf.Clamp(camRot.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f)); //Clamp the rotation of the camera
			_cam.Rotation = camRot;
		}
		else if (@event is InputEventKey key && key.Keycode == Key.Escape && key.Pressed) //When the input is a keycode and the keycode is escape
		{
			if (Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				Input.MouseMode = Input.MouseModeEnum.Visible; //Make the mouse visible
			}
			else
				Input.MouseMode = Input.MouseModeEnum.Captured; //Make the mouse invisible
		}
    }

	public override void _PhysicsProcess(double delta) //Event tick; happens every frame
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 direction = (_head.GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed / 10);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed / 10);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
