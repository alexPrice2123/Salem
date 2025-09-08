using Godot;
using System;

public partial class Player3d : CharacterBody3D
{
	public const float Speed = 10.0f; //Player speed
	public const float JumpVelocity = 6.5f; //Jump power
	public float CamSense = 0.002f; //Sensitivity

	private Node3D _head;
	private Camera3D _cam;
	private Control _interface;
	private Slider _senseBar;
	private MeshInstance3D _sword;
	public float _damage = 0.0f;
	public float _knockbackStrength = 15.0f;


	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_head = GetNode<Node3D>("Head"); //Declares the head componant
		_cam = GetNode<Camera3D>("Head/Camera3D"); //Declares the camera componant
		_interface = GetNode<Control>("UI/Main"); //Delcares the main UI
		_senseBar = GetNode<Slider>("UI/Main/Sense"); //Declares the camera slider
		_sword = GetNode<MeshInstance3D>("Head/Camera3D/Sword/Handle"); //Declares the sword
		_damage = 1.0f;
	}
	
	private async void _on_hitbox_body_entered(Node3D body)
	{
		if (body.IsInGroup("Monster"))
		{
			_sword.GetNode<MeshInstance3D>("Blade").GetNode<GpuParticles3D>("Blood").Emitting = true;
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			_sword.GetNode<MeshInstance3D>("Blade").GetNode<GpuParticles3D>("Blood").Emitting = false;
		}
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
				_interface.Visible = true;
				_senseBar.Value = CamSense * 1000;
			}
			else
			{
				Input.MouseMode = Input.MouseModeEnum.Captured; //Make the mouse invisible
				_interface.Visible = false;
			}

		}
		else if (@event is InputEventMouseButton click && Input.MouseMode == Input.MouseModeEnum.Captured && click.Pressed && _sword.GetNode<AnimationPlayer>("AnimationPlayer").IsPlaying() == false)
		{
			Swing();
		}
	}

	public override void _PhysicsProcess(double delta) //Event tick; happens every frame
	{
		Vector3 velocity = Velocity;

		if (_interface.Visible == true) {
			CamSense = Convert.ToSingle(_senseBar.Value / 1000);
		}

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

	//Custom Functions

	async void Swing()
	{
		_sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Swing");
		_cam.GetNode<Area3D>("Hitbox").GetNode<CollisionShape3D>("CollisionShape3D").Disabled = false;
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		_cam.GetNode<Area3D>("Hitbox").GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
	}
}

