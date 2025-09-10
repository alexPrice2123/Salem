using Godot;
using System;

public partial class Player3d : CharacterBody3D
{
    // --- Player Settings ---
    public const float Speed = 10.0f; // Player movement speed
    public const float JumpVelocity = 6.5f; // Player jump strength
    public float CamSense = 0.002f; // Camera mouse sensitivity

    // --- Node References ---
    private Node3D _head; // Player head node (used for rotation)
    private Camera3D _cam; // Player camera
    private Control _interface; // Pause menu UI
    private Slider _senseBar; // Sensitivity slider in pause menu
    public Control _inv; // Inventory UI
    private MeshInstance3D _sword; // Sword model in hand
    private Control _combatNotif; // Combat notification UI

    // --- Combat Variables ---
    public float _damage = 0.0f; // Player attack damage
    public float _knockbackStrength = 15.0f; // Knockback strength when attacking
    public bool _inCombat = false; // Tracks if player is currently in combat
    private float _combatCounter = 0; // Counter for combat cooldown
    private bool inInv; // Tracks if player is currently inside inventory view

    // --- Godot Lifecycle Functions ---

    // Called once when the node enters the scene tree
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured; // Capture mouse cursor
        _head = GetNode<Node3D>("Head"); // Get player head
        _cam = GetNode<Camera3D>("Head/Camera3D"); // Get camera under head
        _interface = GetNode<Control>("UI/PauseMenu"); // Get pause menu UI
        _senseBar = GetNode<Slider>("UI/PauseMenu/Sense"); // Get sensitivity slider
        _sword = GetNode<MeshInstance3D>("Head/Camera3D/Sword/Handle"); // Get sword mesh
        _combatNotif = GetNode<Control>("UI/Combat"); // Get combat notification
        _inv = GetNode<Control>("UI/Inv"); // Get inventory UI

        _damage = 1.0f; // Default starting damage
    }

    // Handles player input events (mouse, keyboard, etc.)
    public override void _Input(InputEvent @event)
    {
        // Handle mouse movement (camera look)
        if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            _head.RotateY(-motion.Relative.X * CamSense);
            _cam.RotateX(-motion.Relative.Y * CamSense);

            Vector3 camRot = _cam.Rotation;
            camRot.X = Mathf.Clamp(camRot.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f));
            _cam.Rotation = camRot;
        }

        // Handle Escape key (pause menu toggle)
        else if (@event is InputEventKey escapeKey && escapeKey.Keycode == Key.Escape && escapeKey.Pressed)
        {
            if (_inv.Visible == true)
            {
                return;
            }
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
                _interface.Visible = true;
                _senseBar.Value = CamSense * 1000;
            }
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
                _interface.Visible = false;
            }
        }

        // Handle mouse button click (sword attack)
        else if (@event is InputEventMouseButton click && Input.MouseMode == Input.MouseModeEnum.Captured && click.Pressed && _sword.GetNode<AnimationPlayer>("AnimationPlayer").IsPlaying() == false)
        {
            Swing();
        }

        // Handle I key (inventory toggle)
        else if (@event is InputEventKey iKey && iKey.Keycode == Key.I && iKey.Pressed)
        {
            if (_inCombat == true)
            {
                return;
            }
            if (_inv.Visible == true)
            {
                _inv.Visible = false;
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
            else
            {
                _inv.Visible = true;
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
        }
    }

    // Called every frame for physics updates (movement, gravity, combat)
    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Handle combat notification and cooldown
        if (_inCombat == true)
        {
            _combatNotif.Visible = true;
        }
        else
        {
            _combatNotif.Visible = false;
        }
        _combatCounter += 1;
        if (_combatCounter >= 250)
        {
            _combatCounter = 0;
            _inCombat = false;
        }

        // Handle inventory camera switching
        if (_inv.Visible == true)
        {
            inInv = true;
            _cam.GlobalPosition = _cam.GlobalPosition.Lerp(_head.GetNode<CsgBox3D>("InvCam").GlobalPosition, (float)delta * 2f);
            _cam.GlobalRotation = _cam.GlobalRotation.Lerp(_head.GetNode<CsgBox3D>("InvCam").GlobalRotation, (float)delta * 2f);
            _sword.Visible = false;
        }
        else if (_cam.GlobalPosition.Snapped(0.1f) != _head.GetNode<CsgBox3D>("Cam").GlobalPosition.Snapped(0.1f) && inInv == true)
        {
            _cam.GlobalPosition = _cam.GlobalPosition.Lerp(_head.GetNode<CsgBox3D>("Cam").GlobalPosition, (float)delta * 2f);
            _cam.GlobalRotation = _cam.GlobalRotation.Lerp(_head.GetNode<CsgBox3D>("Cam").GlobalRotation, (float)delta * 2f);
            _sword.Visible = true;
        }
        else if (_cam.GlobalPosition.Snapped(0.1f) == _head.GetNode<CsgBox3D>("Cam").GlobalPosition.Snapped(0.1f) && inInv == true)
        {
            inInv = false;
        }

        // Update sensitivity if pause menu is open
        if (_interface.Visible == true)
        {
            CamSense = Convert.ToSingle(_senseBar.Value / 1000);
        }

        // Apply gravity if not on ground
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        // Handle jumping
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        // Handle movement input
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
        if (_inv.Visible == false)
        {
          MoveAndSlide(); // Apply velocity to player
        }
    }

    // --- Custom Functions ---

    // Plays sword swing animation and temporarily enables hitbox
    async void Swing()
    {
        _sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Swing");
        _sword.GetNode<Area3D>("Hitbox").GetNode<CollisionShape3D>("CollisionShape3D").Disabled = false;
        await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
        _sword.GetNode<Area3D>("Hitbox").GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
    }

    // Returns the 3D world position under the mouse cursor
    public Vector3 getMousePos()
    {
        var viewport = GetViewport();
        var mousePosition = viewport.GetMousePosition();
        var camera = viewport.GetCamera3D();
        var rayOrigin = camera.ProjectRayOrigin(mousePosition);
        var rayDirection = camera.ProjectRayNormal(mousePosition);
        float rayLength = camera.Far;
        var rayEnd = rayOrigin + rayDirection * rayLength;
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
        var result = spaceState.IntersectRay(query);
        Vector3 mousePosition3D;
        if (result.ContainsKey("position"))
        {
            mousePosition3D = (Vector3)result["position"];
            return mousePosition3D;
        }
        else
        {
            return new Vector3(0,0,0);
        }
    }
}
