using Godot;
using System;

public partial class Player3d : CharacterBody3D
{
    public const float Speed = 10.0f; // Player movement speed
    public const float JumpVelocity = 6.5f; // Jump power
    public float CamSense = 0.002f; // Camera sensitivity

    private Node3D _head; // Reference to the player's head node
    private Camera3D _cam; // Reference to the player's camera
    private Control _interface; // Reference to the UI interface node
    private Slider _senseBar; // Reference to the slider for adjusting camera sensitivity
    private MeshInstance3D _sword; // Reference to the sword mesh in the player's hand
    public float _damage = 0.0f; // Player's attack damage
    public float _knockbackStrength = 15.0f; // Strength of knockback when hitting an enemy

    // Called when the node enters the scene tree
    public override void _Ready()
    {
        // Initialize the player controls and UI components when the game starts
        Input.MouseMode = Input.MouseModeEnum.Captured; // Captures the mouse cursor
        _head = GetNode<Node3D>("Head"); // Declares the head component
        _cam = GetNode<Camera3D>("Head/Camera3D"); // Declares the camera component under the head
        _interface = GetNode<Control>("UI/Main"); // Declares the main UI control
        _senseBar = GetNode<Slider>("UI/Main/Sense"); // Declares the slider for adjusting camera sensitivity
        _sword = GetNode<MeshInstance3D>("Head/Camera3D/Sword/Handle"); // Declares the sword object under the camera
        _damage = 1.0f; // Initializes the damage value
    }

    // Called when any input event occurs (e.g., key presses, mouse movements)
    public override void _Input(InputEvent @event)
    {
        // Handle mouse movement input to rotate the player's head and camera
        if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            _head.RotateY(-motion.Relative.X * CamSense); // Rotate the player's head horizontally
            _cam.RotateX(-motion.Relative.Y * CamSense); // Rotate the camera vertically

            // Clamp vertical camera rotation to prevent excessive tilting
            Vector3 camRot = _cam.Rotation;
            camRot.X = Mathf.Clamp(camRot.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f));
            _cam.Rotation = camRot;
        }
        
        // Handle Escape key press to toggle mouse visibility and UI
        else if (@event is InputEventKey key && key.Keycode == Key.Escape && key.Pressed)
        {
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible; // Make the mouse visible
                _interface.Visible = true; // Show the UI interface
                _senseBar.Value = CamSense * 1000; // Update the slider value to match the current camera sensitivity
            }
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Captured; // Hide the mouse again
                _interface.Visible = false; // Hide the UI interface
            }
        }
        
        // Handle mouse button click (for sword swinging)
        else if (@event is InputEventMouseButton click && Input.MouseMode == Input.MouseModeEnum.Captured && click.Pressed && _sword.GetNode<AnimationPlayer>("AnimationPlayer").IsPlaying() == false)
        {
            Swing(); // Perform the sword swing if it's not already animating
        }
    }

    // Called every frame for physics calculations
    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity; // Get the current velocity

        // If the UI is visible, update camera sensitivity from the slider
        if (_interface.Visible == true)
        {
            CamSense = Convert.ToSingle(_senseBar.Value / 1000); // Update the camera sensitivity based on slider value
        }

        // Apply gravity if the player is not on the floor
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta; // Add gravity effect to the velocity
        }

        // Handle jump input
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity; // Set the vertical velocity to the jump strength
        }

        // Get the player's movement direction from the input and apply it
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        Vector3 direction = (_head.GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized(); // Convert input to direction
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed; // Move player horizontally along the X axis
            velocity.Z = direction.Z * Speed; // Move player horizontally along the Z axis
        }
        else
        {
            // Gradually decelerate the player if no movement input is provided
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed / 10);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed / 10);
        }

        Velocity = velocity; // Set the new velocity for the player
        MoveAndSlide(); // Move the player based on the calculated velocity
    }

    // Custom functions
    async void Swing()
    {
        _sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Swing");
        _sword.GetNode<Area3D>("Hitbox").GetNode<CollisionShape3D>("CollisionShape3D").Disabled = false;
        await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
        _sword.GetNode<Area3D>("Hitbox").GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
    }
}
