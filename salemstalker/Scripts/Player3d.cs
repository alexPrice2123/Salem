using Godot;
using System;

public partial class Player3d : CharacterBody3D
{
    // --- CONSTANTS ---
    public const float Speed = 10.0f;                 // Player base movement speed
    public const float JumpVelocity = 6.5f;           // Player jump strength
    public float CamSense = 0.002f;                   // Mouse sensitivity for camera

    // --- NODE REFERENCES ---
    private Node3D _head;                             // Player head node (handles rotation)
    private Camera3D _cam;                            // Player camera
    private Control _interface;                       // Pause menu UI
    private Slider _senseBar;                         // Sensitivity slider in pause menu
    public Control _inv;                              // Inventory UI
    private MeshInstance3D _sword;                    // Sword mesh in hand
    private Control _combatNotif;                     // Combat notification UI

    // --- COMBAT VARIABLES ---
    public float _damage = 0.0f;                      // Player attack damage
    public float _knockbackStrength = 15.0f;          // Knockback strength applied to enemies
    public bool _inCombat = false;                    // Tracks if player is in combat
    private float _combatCounter = 0;                 // Frame counter for combat timeout
    private bool _inInv;                              // True if player is viewing inventory
    private float _dashVelocity = 0f;                 // Current dash velocity boost
    private float _fullDashValue = 10.0f;             // Max dash velocity

    // --- READY ---
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;      // Capture mouse on start
        _head = GetNode<Node3D>("Head");
        _cam = GetNode<Camera3D>("Head/Camera3D");
        _interface = GetNode<Control>("UI/PauseMenu");
        _senseBar = GetNode<Slider>("UI/PauseMenu/Sense");
        _sword = GetNode<MeshInstance3D>("Head/Camera3D/Sword/Handle");
        _combatNotif = GetNode<Control>("UI/Combat");
        _inv = GetNode<Control>("UI/Inv");

        _damage = 1.0f;                                     // Default starting damage
    }

    // --- INPUT HANDLER ---
    public override void _Input(InputEvent @event)
    {
        // --- Camera look ---
        if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            _head.RotateY(-motion.Relative.X * CamSense);
            _cam.RotateX(-motion.Relative.Y * CamSense);

            Vector3 camRot = _cam.Rotation;
            camRot.X = Mathf.Clamp(camRot.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f));
            _cam.Rotation = camRot;
        }

        // --- Pause menu toggle ---
        else if (@event is InputEventKey escapeKey && escapeKey.Keycode == Key.Escape && escapeKey.Pressed)
        {
            if (_inv.Visible == true) { return; }

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

        // --- Sword attack ---
        else if (@event is InputEventMouseButton click && Input.MouseMode == Input.MouseModeEnum.Captured && click.Pressed
                 && _sword.GetNode<AnimationPlayer>("AnimationPlayer").IsPlaying() == false)
        {
            Swing();
        }

        // --- Inventory toggle ---
        else if (@event is InputEventKey iKey && iKey.Keycode == Key.I && iKey.Pressed)
        {
            if (_inCombat == true) { return; }

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

        // --- Dash (Space key) ---
        else if (@event is InputEventKey spaceKey && spaceKey.Keycode == Key.Space && spaceKey.Pressed)
        {
            if (_dashVelocity <= 0.1f)
            {
                _dashVelocity = _fullDashValue;
            }
        }
    }

    // --- PHYSICS LOOP ---
    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // --- Combat handling ---
        if (_inCombat == true) { _combatNotif.Visible = true; }
        else { _combatNotif.Visible = false; }

        _combatCounter += 1;
        if (_combatCounter >= 250)
        {
            _combatCounter = 0;
            _inCombat = false;
        }

        // --- Inventory camera transition ---
        if (_inv.Visible == true)
        {
            _inInv = true;
            _cam.GlobalPosition = _cam.GlobalPosition.Lerp(_head.GetNode<CsgBox3D>("InvCam").GlobalPosition, (float)delta * 3f);
            _cam.GlobalRotation = _cam.GlobalRotation.Lerp(_head.GetNode<CsgBox3D>("InvCam").GlobalRotation, (float)delta * 3f);
            _sword.Visible = false;
        }
        else if (_cam.GlobalPosition.Snapped(0.1f) != _head.GetNode<CsgBox3D>("Cam").GlobalPosition.Snapped(0.1f) && _inInv == true)
        {
            _cam.GlobalPosition = _cam.GlobalPosition.Lerp(_head.GetNode<CsgBox3D>("Cam").GlobalPosition, (float)delta * 3f);
            _cam.GlobalRotation = _cam.GlobalRotation.Lerp(_head.GetNode<CsgBox3D>("Cam").GlobalRotation, (float)delta * 3f);
            _sword.Visible = true;
        }
        else if (_cam.GlobalPosition.Snapped(0.1f) == _head.GetNode<CsgBox3D>("Cam").GlobalPosition.Snapped(0.1f) && _inInv == true)
        {
            _inInv = false;
        }

        // --- Update sensitivity from pause menu ---
        if (_interface.Visible == true)
        {
            CamSense = Convert.ToSingle(_senseBar.Value / 1000);
        }

        // --- Gravity ---
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        // --- Movement input ---
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        Vector3 direction = (_head.GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            _fullDashValue = 10f;
            velocity.X = direction.X * (Speed + _dashVelocity);
            velocity.Z = direction.Z * (Speed + _dashVelocity);
        }
        else
        {
            _fullDashValue = 25f;
            velocity = velocity.Lerp(_cam.GlobalTransform.Basis.Z * -1 * _dashVelocity, (float)delta * 10f);
            velocity = new Vector3(velocity.X, 0f, velocity.Z);
        }

        // --- Dash decay ---
        float lerpDash = _dashVelocity;
        lerpDash = Mathf.Lerp(lerpDash, 0f, (float)delta * 3f);
        _dashVelocity = lerpDash;

        // --- Camera FOV scaling ---
        float fovGoal = Mathf.Lerp(_cam.Fov, Velocity.Length() + 80, (float)delta * 10f);
        _cam.Fov = fovGoal;

        // --- Apply movement ---
        Velocity = velocity;
        if (_inv.Visible == false)
        {
            MoveAndSlide();
        }
    }

    // --- CUSTOM FUNCTIONS ---
    private async void Swing()
    {
        _sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Swing");
        _sword.GetNode<Area3D>("Hitbox").GetNode<CollisionShape3D>("CollisionShape3D").Disabled = false;
        await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
        _sword.GetNode<Area3D>("Hitbox").GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
    }

    public Vector3 GetMousePos()
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

        if (result.ContainsKey("position"))
        {
            return (Vector3)result["position"];
        }
        return new Vector3(0, 0, 0);
    }
}
