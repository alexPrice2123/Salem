using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Player3d : CharacterBody3D
{
	// --- CONSTANTS ---
	public const float Speed = 4.5f;                 // Base player movement speed
	public const float RunSpeed = 6.5f;              // Running movement speed
	public const float JumpVelocity = 6.5f;          // Player jump strength
	public const float BobFreq = 2.0f;               // Frequency of camera head-bob
	public const float BobAmp = 0.06f;               // Amplitude of camera head-bob
	public float CamSense = 0.002f;                  // Camera mouse sensitivity

	// --- NODE REFERENCES ---
	private Node3D _head;                            // Player head node (handles rotation)
	private Camera3D _cam;                           // Player camera node
	private Control _interface;                      // Pause menu UI
	private Slider _senseBar;                        // Sensitivity slider in pause menu
	public Control _inv;                             // Inventory UI
	private Node3D _sword;                           // Sword hitbox mesh (damages enemies)
	private Control _combatNotif;                    // Combat notification UI
	private RayCast3D _ray;                          // Forward raycast (for NPC interaction)
	private Control _questBook;                      // Parent node for all quest UI
	public Control _questBox;                        // Quest display UI container
	private VBoxContainer _questTemplate;            // Template node for quests
	private OmniLight3D _lantern;

	// --- COMBAT VARIABLES ---
	public float _damage = 0.0f;                     // Attack damage value
	private ulong _lastHit = 0;                      // Time of the last time a player hit
	private int _comboNum = 0;
	public float _knockbackStrength = 15.0f;         // Knockback force applied to enemies
	public bool _inCombat = false;                   // Tracks if player is currently in combat
	private float _combatCounter = 0;                // Frame counter for combat cooldown
	private bool _inInv;                             // Tracks if inventory is open
	private float _dashVelocity = 0f;                // Current dash boost velocity
	private float _fullDashValue = 10.0f;            // Maximum dash velocity
	private bool _running = false;                   // True if player is holding run input
	private float _bobTime = 0.0f;                   // Time accumulator for head-bob effect
	private string _originalDialouge;                // Stores NPC dialogue before player interacts
	private CharacterBody3D _lastSeen;               // Last seen NPC in interaction range
	public int _monstersKilled = 0;                  // Monster kill counter (for quests)
	public float _maxHealth = 100f;
	public float _health;
	public Color _maxHealthColor = new Color(244f / 255f, 224f / 255f, 138f / 255f);
	public Color _minHealthColor = new Color(255f / 255f, 0f, 0f);
	private float _maxRange = 25f;
	private float _minRange = 5f;
	// --- READY ---
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;      // Capture mouse cursor at start
		_head = GetNode<Node3D>("Head");
		_cam = GetNode<Camera3D>("Head/Camera3D");
		_interface = GetNode<Control>("UI/PauseMenu");
		_senseBar = GetNode<Slider>("UI/PauseMenu/Sense");
		_sword = GetNode<Node3D>("Head/Camera3D/Sword/Falchion");
		_combatNotif = GetNode<Control>("UI/Combat");
		_inv = GetNode<Control>("UI/Inv");
		_ray = GetNode<RayCast3D>("Head/Camera3D/Ray");
		_questBook = GetNode<Control>("UI/Quest");
		_questBox = GetNode<Control>("UI/Quest/QuestBox");
		_questTemplate = GetNode<VBoxContainer>("UI/Container/QuestTemplate");
		_lantern = GetNode<OmniLight3D>("Head/Camera3D/Lantern");
		_health = _maxHealth;
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
		else if (Input.IsActionPressed("attack")
				 && _sword.GetNode<AnimationPlayer>("AnimationPlayer").IsPlaying() == false
				 && _lastSeen == null)
		{
			Swing();
		}

		// --- Inventory toggle ---
		else if (Input.IsActionJustPressed("inventory"))
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

		// --- Questbook toggle ---
		else if (@event is InputEventKey lKey && lKey.Keycode == Key.L && lKey.Pressed)
		{
			if (_inCombat == true) { return; }

			if (_questBook.Visible == true)
			{
				_questBook.Visible = false;
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			else if (_inv.Visible == false)
			{
				_questBook.Visible = true;
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
		}

		// --- Dash (Space key) ---
		else if (Input.IsActionJustPressed("dash"))
		{
			if (_dashVelocity <= 0.1f) { _dashVelocity = _fullDashValue; }
		}

		// --- Run (Shift key) ---
		else if (@event is InputEventKey shiftKey && shiftKey.Keycode == Key.Shift)
		{
			_running = shiftKey.Pressed;
		}

		// --- Interact (E key) ---
		else if (Input.IsActionJustPressed("interact"))
		{
			if (_lastSeen != null && _lastSeen is NpcVillager villager)
			{
				villager.Talk();
			}
		}
	}

	// --- PHYSICS LOOP ---
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		_lantern.LightColor = _minHealthColor.Lerp(_maxHealthColor, _health / _maxHealth);
		_lantern.OmniRange = (_health / _maxHealth * _maxRange) + _minRange;

		if (_health <= 0)
		{
			QueueFree();
		}

		// --- Combat handling ---
		_combatNotif.Visible = _inCombat;
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
		if (_interface.Visible == true) { CamSense = Convert.ToSingle(_senseBar.Value / 1000); }

		// --- Gravity ---
		if (!IsOnFloor()) { velocity += GetGravity() * (float)delta; }

		// --- Movement input ---
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
		Vector3 direction = (_head.GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			_fullDashValue = 10f;
			velocity.X = direction.X * (Speed + RunSpeed * Convert.ToInt32(_running) + _dashVelocity);
			velocity.Z = direction.Z * (Speed + RunSpeed * Convert.ToInt32(_running) + _dashVelocity);
		}
		else
		{
			_fullDashValue = 15f;
			velocity = velocity.Lerp(_cam.GlobalTransform.Basis.Z * -1 * _dashVelocity, (float)delta * 10f);
			velocity = new Vector3(velocity.X, 0f, velocity.Z);
		}

		// --- Dash decay ---
		_dashVelocity = Mathf.Lerp(_dashVelocity, 0f, (float)delta * 6f);

		// --- Camera FOV scaling ---
		float fovGoal = Mathf.Lerp(_cam.Fov, Velocity.Length() + 80, (float)delta * 10f);
		_cam.Fov = fovGoal;

		// --- NPC Interaction detection ---
		if (GetMouseCollision() != null && _originalDialouge == null)
		{
			CharacterBody3D targetNode = GetMouseCollision();
			if (targetNode is NpcVillager villager && villager._questComplete == false) // if the node that the player is looking at (argument 1), has a script attached to it named (argument 2) then set that to the variable (argument 3)
			{
				_lastSeen = villager;
				_originalDialouge = villager._questPrompt.Text;
				villager._questPrompt.Text = villager._questPrompt.Text + "\n" + "E to Talk";
			}
		}
		else if (GetMouseCollision() == null && _originalDialouge != null) //set the NPC's dialouge back to the original
		{
			if (_lastSeen is NpcVillager villager) { villager._questPrompt.Text = _originalDialouge; }
			_lastSeen = null;
			_originalDialouge = null;
		}

		// --- Head bob + sword bob ---
		if (_dashVelocity <= 1.0)
		{
			Transform3D camTransformGoal = _cam.Transform;
			_bobTime += (float)delta * velocity.Length() * (Convert.ToInt32(IsOnFloor()) + 0.2f);
			camTransformGoal.Origin = HeadBob(_bobTime);
			_cam.Transform = camTransformGoal;

			Transform3D lightTransformGoal = _lantern.Transform;
			lightTransformGoal.Origin = LightBob(_bobTime);
			_lantern.Transform = lightTransformGoal;
		}

		// --- Apply movement ---
		Velocity = velocity;
		if (_inv.Visible == false) { MoveAndSlide(); }
	}

	// --- CUSTOM FUNCTIONS ---
	private async void Swing()
	{
		float swingTime = (float)_sword.GetMeta("swingSpeed");
		float comboTime = swingTime * 1000 + 400;
		if (Time.GetTicksMsec() - _lastHit < comboTime && _comboNum == 0 || _comboNum == 1)
		{
			_comboNum++;
		}
		else
		{
			_comboNum = 0;
		}
		if (Time.GetTicksMsec() - _lastHit > comboTime && _comboNum == 2)
		{
			_comboNum = 0;
		}
		_sword.GetNode<Area3D>("Hitbox").GetNode<CollisionPolygon3D>("CollisionShape3D").Disabled = false;
		_damage = (float)_sword.GetMeta("damage");
		if (_comboNum == 0)
		{
			_sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Swing1");
		}
		else if (_comboNum == 1)
		{
			_sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Swing2");
		}
		else if (_comboNum == 2)
		{
			_damage = (float)_sword.GetMeta("hDamage");
			_sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Swing3");
		}
		GD.Print(comboTime, "abc", swingTime, "abc", _comboNum);
		_lastHit = Time.GetTicksMsec();
		await ToSignal(GetTree().CreateTimer(swingTime), "timeout");
		_sword.GetNode<Area3D>("Hitbox").GetNode<CollisionPolygon3D>("CollisionShape3D").Disabled = true;
	}

	private Vector3 HeadBob(float bobTime)
	{
		Vector3 pos = Vector3.Zero;
		pos.Y = Mathf.Sin(bobTime * BobFreq) * BobAmp;
		pos.X = Mathf.Cos(bobTime * BobFreq / 2) * BobAmp;
		return pos;
	}

	private Vector3 LightBob(float bobTime)
	{
		Vector3 pos = Vector3.Zero;
		pos.Y = Mathf.Sin(bobTime * BobFreq / 1.5f) * BobAmp;
		pos.X = Mathf.Cos(bobTime * BobFreq / 2.5f) * BobAmp;
		return pos;
	}

	public CharacterBody3D GetMouseCollision()
	{
		if (_ray.IsColliding())
		{
			if (_ray.GetCollider().GetClass() == "CharacterBody3D" && _ray.GetCollider() != this)
			{
				return (CharacterBody3D)_ray.GetCollider();
			}
		}
		return null;
	}

	public void MonsterKilled(string MonsterType)
	{
		_monstersKilled += 1;
		if (_questBox.FindChild("KillMonsters") != null)
		{
			if (_monstersKilled >= 5) { (_questBox.GetNode("KillMonsters/Number") as Label).Text = "Complete!"; }
			else { (_questBox.GetNode("KillMonsters/Number") as Label).Text = _monstersKilled + "/5"; }
		}
	}

	public void RemoveQuest(string QuestName)
	{
		if (_questBox.FindChild(QuestName) != null)
		{
			_questBox.FindChild(QuestName).QueueFree();
		}
	}

	public void GetQuest(string QuestTitle, string QuestGoal)
	{
		Control questText = (VBoxContainer)_questTemplate.Duplicate();
		_questBox.AddChild(questText);
		questText.Owner = _questBox;
		questText.Name = QuestTitle;
		questText.GetNode<Label>("Quest").Text = QuestTitle;
		questText.GetNode<Label>("Number").Text = QuestGoal;
	}

	public void _on_hurtbox_entered(Node3D body)
	{
		if (body.IsInGroup("Monster"))
		{

			GetTree().Quit();
		}

	}
}
