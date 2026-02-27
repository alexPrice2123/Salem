using Godot;
using System;
using System.Runtime.CompilerServices; // Usually for specific internal compiler needs, less common in game scripts.
using System.Collections.Generic;
using System.Linq; // For using Dictionary.

// Defines the player class, inheriting from Godot's 3D physics-based character node.
public partial class Player3d : CharacterBody3D
{
	// --- CONSTANTS ---
	public const float Speed = 3.5f;                 // Base player movement speed (units/second)
	public const float RunSpeed = 4.5f;              // Additional speed when running
	public const float JumpVelocity = 6.5f;          // Vertical velocity applied for jumping (not used in _PhysicsProcess shown)
	public const float BobFreq = 2f;                 // Frequency (speed) of the camera head-bob effect
	public const float BobAmp = 0.06f;               // Amplitude (intensity) of the camera head-bob effect

	// --- NODE REFERENCES ---
	private Node3D _head;                            // Player head node, controls vertical camera movement/head-bob offset
	private Camera3D _cam;                           // Player camera node (handles up/down rotation and FOV)
	private Control _interface;                      // Reference to the main Pause menu UI
	private Slider _senseBar;                        // Slider control within the pause menu for adjusting sensitivity
	public Control _inv;                             // Reference to the Inventory UI
	public Node3D _sword;                           // The currently equipped sword's mesh/root node
	private Node3D _eSecWeapon1;					 // The secondary weapon slot 1's root node
	private Node3D _eSecWeapon2;					 // The secondary weapon slot 2's root node
	private Node3D _eSecWeapon3;					 // The secondary weapon slot 3's root node
	private Node3D _eSecWeapon4;					 // The secondary weapon slot 4's root node
	private Control _combatNotif;                    // UI element for combat notifications/status
	private RayCast3D _ray;                          // Raycast used to detect interactable objects (NPCs, items)
	private Control _questBook;                      // The main container for the Quest Log UI
	public Control _questBox;                        // Container where active quest entries are listed
	private VBoxContainer _questTemplate;            // A hidden template used to instantiate new quest UI entries    
	private OmniLight3D _lantern; 					 // A light attached to the player (likely for dynamic lighting/mood based on health)
	private Control _dialogue;                       // Main container for all dialogue UI
	private Control _smithShop;                      // Main container for all blacksmith shop UI
	public itemList _itemInv;						//Reference to resource inv

	// --- WEAPON REFERENCES ---
	private PackedScene _shortSword = GD.Load<PackedScene>("res://Scenes/MainHandWeapons/shortsword.tscn"); // Pre-load shortsword scene resource
	private PackedScene _falchion = GD.Load<PackedScene>("res://Scenes/MainHandWeapons/falchion.tscn"); // Pre-load falchion scene resource
	private PackedScene _dagger = GD.Load<PackedScene>("res://Scenes/MainHandWeapons/dagger.tscn"); // Pre-load falchion scene resource
	private PackedScene _longsword = GD.Load<PackedScene>("res://Scenes/MainHandWeapons/longsword.tscn"); // Pre-load falchion scene resource
	private PackedScene _flintGun = GD.Load<PackedScene>("res://Scenes/OffHandWeapons/Flintlock.tscn"); // Pre-load Flintlock scene resource
	private PackedScene _stakeGun = GD.Load<PackedScene>("res://Scenes/OffHandWeapons/stake_gun.tscn"); // Pre-load Stake Gun scene resource
	public Dictionary<string, PackedScene> _weapon = new Dictionary<string, PackedScene>(); // Dictionary to store and manage available weapons
	private Dictionary<string, PackedScene> _secWeapon = new Dictionary<string, PackedScene>(); // Dictionary to store and manage available weapons

	// --- VARIABLES ---
	private PackedScene _pauseMenu = GD.Load<PackedScene>("res://Scenes/pause_menu.tscn");
	public float HorCamSense = 0.002f;                  // Horizontal camera mouse sensitivity multiplier
	public float VerCamSense = 0.002f;                  // Vertical camera mouse sensitivity multiplier
	protected RandomNumberGenerator _rng = new();       // Generator for random events like critical hits and effects
	public float _damage = 0.0f;                     	// Current attack damage value, adjusted by buffs/debuffs
	private ulong _lastHit = 0;                      	// Stores the game time (in milliseconds) of the player's last attack
	private int _comboNum = 0;						 	// Current step in the attack combo chain (0, 1, or 2)
	public float _knockbackStrength = 5.0f;          	// Force applied to enemies upon hit
	public bool _inCombat = false;                   	// Flag: true if the player has recently attacked or been attacked
	private float _combatCounter = 0;               	// Timer/frame counter for the combat cooldown
	private bool _inInv;                             	// Tracks if the player is currently in the inventory state (commented out)
	private float _dashVelocity = 0f;                	// Current extra speed from dashing
	private float _fullDashValue = 10.0f;            	// Max speed boost granted by a full dash
	private float _knockVelocity = 0f;               	// Current knockback applied to player
	private bool _cooldownSec;						 	// Tracks whether the player can use their secondary weapon
	public bool _running = false;                   	// True if the 'run' input is held down
	private bool _inStep = false;					 	// Prevents footstep audio from playing if player is already playing a step.
	private float _bobTime = 0.0f;                   	// Accumulator for the head-bob sine wave function
	public string _originalDialouge;                 	// Stores an NPC's default dialogue to restore it after interaction
	public CharacterBody3D _lastSeen;               	// Reference to the last interactable object the raycast hit
	public int _monstersKilled = 0;                  	// Counter for monsters killed (for quest tracking)
	public float _maxHealth = 100f;					 	// Maximum player health
	public float _health; 								// Current player health
	public Color _maxHealthColor = new Color(244f / 255f, 224f / 255f, 138f / 255f); // Goldish color for high health light
	public Color _minHealthColor = new Color(255f / 255f, 0f, 0f); // Red color for low health light
	private float _maxRange = 25f; 						// Max range for the lantern light
	private float _minRange = 5f; 						// Min range for the lantern light
	private bool _attackCooldown = false; 				// Flag: prevents attacking during a swing animation
	public Color _lightColor; 							// Current color of the lantern light
	private float _maxStamina = 100f; 					// Maximum player stamina
	private float _stamina; 							// Current player stamina
	private float _staminaGoal; 						// Lerp target for stamina (used for UI smooth transition)
	private Vector3 _baseHeadPosition; 					// The head node's default local position
	private Vector3 _headOffset = new Vector3(0f, 0f, 0f); // Dynamic offset for head (e.g., dash effect)
	private SwordHandler _swordInst;					// Script instance of the currently equipped sword
	public bool _blocking = false; 						// Flag: true if the player is holding the block input
	public bool _parry = false; 						// Flag: true during the small parry window at the start of a block
	public bool _parried = false; 						// Flag: true if a parry was successful (set by the enemy/damage function)
	private float _parryWindow = 0.15f; 				// Duration of the parry window (in seconds)
	private float _currentParryWindow; 					// Remaining time in the parry window
	public NpcVillager _villager; 						// Reference to the currently interacting villager NPC
	public bool _hasApple = false; 						// Quest item flag
	private float _staminaTimer = 2f;
	private float _currentStaminaTimer = 0f;
	private int equipSec = 2;
	public bool _twoHand = false;
	public float _hallucinationFactor = 0f;
	public float _speedOffset = 0f;
	public float _speedCount = 0f;
	public string _currentBiome = "Village";
	public string _lastBiome = "Village";
	private float _backSpeed = 0f;

	public int _swampMonstersKilled = 0;
	public int _plainsMonstersKilled = 0;
	public int _forestMonstersKilled = 0;
	public int _shrinesDestroyed = 0;
	public int _ratsKilled = 0;
 	public bool _inGoalArea = true;
	private List<string> _overlappingAreas = new List<string>();
	public float _lookingAtGoalPoint;
	public Vector3 _goalPointPos = new Vector3(999, 999, 999);
	public Node3D _goalPoint;
	public bool _inWater = false;
	public bool _dead = false;
	private SubViewportContainer _map;
	private Vector3 _cameraBaseRotation;
	private Vector3 _cameraBasePosition;
	private float _demoCount = 30f;
	private Godot.Collections.Array<string> _pickUpableItems { get; set; } = ["Taz", "Bridger", "Gnocchi"];

	// --- READY ---
	// Called when the node enters the scene tree for the first time. Used for setup.
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;      // Hide and lock the mouse cursor to the center of the screen
		
		// Get references to child nodes
		_head = GetNode<Node3D>("Head");
		_cam = GetNode<Camera3D>("Head/Camera3D");
		_sword = GetNode<Node3D>("Head/Camera3D/Sword").GetChild<Node3D>(0); // Get the first child of the 'Sword' node (the actual equipped weapon)
		_eSecWeapon1 = GetNode<Node3D>("Head/Camera3D/Offhand1").GetChild<Node3D>(0);
		_eSecWeapon2 = GetNode<Node3D>("Head/Camera3D/Offhand2").GetChild<Node3D>(0);
		_itemInv = GetNode<itemList>("UI/ResourceInv");
		//_eSecWeapon3 = GetNode<Node3D>("Head/Camera3D/Offhand3").GetChild<Node3D>(0);
		//_eSecWeapon4 = GetNode<Node3D>("Head/Camera3D/Offhand4").GetChild<Node3D>(0);
		_combatNotif = GetNode<Control>("UI/Combat");
		_inv = GetNode<Control>("UI/Inv");
		_ray = GetNode<RayCast3D>("Head/Camera3D/Ray");
		_questBook = GetNode<Control>("UI/Quest");
		_questBox = GetNode<Control>("UI/Quest/QuestBox");
		_questTemplate = GetNode<VBoxContainer>("UI/Container/QuestTemplate");
		_lantern = GetNode<OmniLight3D>("Head/Camera3D/Lantern");
		_dialogue = GetNode<Control>("UI/Dialogue");
		_smithShop = GetNode<Control>("UI/BlacksmithShop");
		_goalPoint = GetParent().GetNode<Node3D>("GoalArea/GoalPoint");
		_map = GetParent().GetNode<SubViewportContainer>("PaperMap");
		
		// Initialize starting values
		_health = _maxHealth;
		_stamina = _maxStamina;
		_baseHeadPosition = _head.Position;
		_swordInst = _sword as SwordHandler; // Cast the sword node to its script type
		_cameraBaseRotation = _cam.Rotation;
		_cameraBasePosition = _cam.Position;

		// Populate the weapon dictionary
		_weapon.Add("Shortsword", _shortSword);
		_weapon.Add("Falchion", _falchion);
		_weapon.Add("longsword", _longsword);
		_weapon.Add("dagger", _dagger);
		_secWeapon.Add("FlintGun", _flintGun);
		_secWeapon.Add("StakeGun", _stakeGun);
		StartCut();
	}

	// --- INPUT HANDLER ---
	// Called when an input event occurs.
	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("retry") && _dead == true)
		{
			GetNode<Ui>("UI")._loadingGoal = -1f;
		}
		if (Input.IsActionJustPressed("quit") && _dead == true)
		{
			GetTree().ChangeSceneToFile("res://Scenes/titlescreen.tscn");
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		if (_dead == true){return;}
		
		// --- Camera look ---
		if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			// Horizontal look
			_head.RotateY(-motion.Relative.X * HorCamSense);

			// Vertical look
			_cameraBaseRotation.X -= motion.Relative.Y * VerCamSense;
			_cameraBaseRotation.X = Mathf.Clamp(
				_cameraBaseRotation.X,
				Mathf.DegToRad(-80),
				Mathf.DegToRad(80)
			);
			_lookingAtGoalPoint = CalculateLookAtAlignment();
		}

		// --- Pause menu toggle (Escape) ---
		else if (Input.IsActionJustPressed("pause"))
		{
			if (Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				// Un-capture mouse, show pause menu, update sensitivity slider to current value
				Input.MouseMode = Input.MouseModeEnum.Visible;
				GetTree().Paused = true;
				Control pauseInst = _pauseMenu.Instantiate<Control>();
				GetTree().Root.AddChild(pauseInst);
				_interface = pauseInst;
				_senseBar = _interface.GetNode<HSlider>("Sense");
				_senseBar.Value = HorCamSense * 1000;
				if (pauseInst is PauseMenu menu)
				{
					menu._player = this;
				}
			}
			else
			{
				if (_inv.Visible == true) // pressing escape while the inventory is open will close it.
				{
					_inv.Visible = false;
					Input.MouseMode = Input.MouseModeEnum.Captured;
				}
				if (_questBook.Visible == true)
				{
					_questBook.Visible = false;
					Input.MouseMode = Input.MouseModeEnum.Captured;
				}                                                 // same for the quest and shop UI
				if (_smithShop.Visible == true)
				{
					_smithShop.Visible = false;
					Input.MouseMode = Input.MouseModeEnum.Captured;
				}
				if (_dialogue.Visible == true) { return; }
			}
		}

		// --- Sword attack (Attack Action) ---
		else if (Input.IsActionPressed("attack")
				 && _attackCooldown == false
				 && !IsInstanceValid(_lastSeen) // Not looking at an interactable object
				 && _inv.Visible == false)
		{
			// This block handles the first attack, potentially hiding a "Controls" overlay
			if (GetNode<Sprite2D>("UI/Controls").Visible == true)
			{
				GetNode<Sprite2D>("UI/Controls").Visible = false;
			}
			else
			{
				Swing(false); // Perform a normal sword swing
			}
		}

		// --- Block Start (Block Action) ---
		else if (Input.IsActionJustPressed("block")
				 && _attackCooldown == false
				 && !IsInstanceValid(_lastSeen)
				 && _inv.Visible == false)
		{
			Block(true); // Start blocking/parrying
		}
		// --- Block End (Block Action) ---
		else if (Input.IsActionJustReleased("block")
				 && _attackCooldown == false
				 && !IsInstanceValid(_lastSeen)
				 && _inv.Visible == false)
		{
			Block(false); // Stop blocking/parrying
		}

		// --- Inventory toggle (Inventory Action) ---
		else if (Input.IsActionJustPressed("inventory"))
		{
			GD.Print("INV");
			if (_inCombat == true || _questBook.Visible == true || _map.Visible == true) { return; } // Cannot open in combat or if quest book is open

			if (_inv.Visible == true)
			{
				// Hide inventory, capture mouse
				_inv.Visible = false;
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			else
			{
				// Show inventory, un-capture mouse
				_inv.Visible = true;
				Input.MouseMode = Input.MouseModeEnum.Visible;
				GetNode<Ui>("UI").Opened(); // Call a function on the main UI script
			}
		}

		// --- Questbook toggle (L Key) ---
		else if (Input.IsActionJustPressed("questOpen"))
		{
			if (_inv.Visible == true || _map.Visible == true) { return; }

			if (_questBook.Visible == true)
			{
				// Hide quest book, capture mouse
				_questBook.Visible = false;
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			else
			{
				// Show quest book, un-capture mouse
				_questBook.Visible = true;
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
		}

		// --- Map toggle (M Key) ---
		else if (Input.IsActionJustPressed("mapOpen"))
		{
			if (_inv.Visible == true || _questBook.Visible == true) { return; }

			if (_map.Visible == true)
			{
				// Hide map
				_map.Visible = false;
			}
			else 
			{
				// Show map
				_map.Visible = true;
			}
		}

		// --- Dash (Dash Action) ---
		else if (Input.IsActionJustPressed("dash"))
		{
			// Check if dash cooldown is over and player has enough stamina
			if (_dashVelocity <= 0.1f && _stamina >= 0.1f * _maxStamina)
			{
				_dashVelocity = _fullDashValue; // Apply max dash speed
				_stamina -= 20f; // Deduct stamina
			}
		}

		// --- Run (Shift Key) ---
		else if (@event is InputEventKey shiftKey && shiftKey.Keycode == Key.Shift)
		{
			_running = shiftKey.Pressed; // Set running state based on key press
										 // If stamina is zero or less, stop running immediately
			if (_running == false)
			{
				_currentStaminaTimer = _staminaTimer;
			}
			if (_stamina <= 0)
			{
				_running = false;
				_currentStaminaTimer = _staminaTimer;
			}
		}

		// --- Interact (E Key) ---
		else if (Input.IsActionJustPressed("interact"))
		{
			// Check if looking at a valid NpcVillager and call its Talk method
			if (IsInstanceValid(_lastSeen) && _lastSeen is NpcVillager villager)
			{
				villager.Talk();
			}
			if (IsInstanceValid(_lastSeen) && _inv.Visible == false)
			{
				string itemName = _lastSeen.Name;
				if (_lastSeen.Name == "Anvil")
				{
					_lastSeen = null;
					_smithShop.Visible = true;
					Input.MouseMode = Input.MouseModeEnum.Visible;
				}
				else if (((string)_lastSeen.Name).Contains("LogItem"))
				{
					_lastSeen.QueueFree();
					_lastSeen = null;
					_itemInv.AddResource("log", 1);
				}
				else if (_pickUpableItems.Contains(itemName))
				{
					_lastSeen.QueueFree();
					_lastSeen = null;
					_itemInv.AddResource(itemName.ToLower(), 1);
				}
			}
		}

		// --- Swap equiped secondary weapon ---
		else if (Input.IsActionJustPressed("specialSwap1")) { equipSec = 1; }
		else if (Input.IsActionJustPressed("specialSwap2")) { equipSec = 2; }
		else if (Input.IsActionJustPressed("specialSwap3")) { if (!_twoHand) { equipSec = 3; } 
															else { play_sfx(GD.Load<AudioStreamOggVorbis>("res://Assets/SFX/errorTemp.ogg")); }}
		else if (Input.IsActionJustPressed("specialSwap4")) { if (!_twoHand) { equipSec = 4; }
															else { play_sfx(GD.Load<AudioStreamOggVorbis>("res://Assets/SFX/errorTemp.ogg")); } }

		// --- Use secondary weapon ---
		else if (Input.IsActionJustPressed("special"))
		{
			if (!_cooldownSec)
			{
				secondaryCooldown();
				if (equipSec == 1)
				{
					if (_eSecWeapon1 is Flintlock flintchild)
					{
						GD.Print("wtf?");
						flintchild.specAction();
					}
					else if (_eSecWeapon1 is StakeGun stakeChild)
					{
						GD.Print("ShootPlease");
						stakeChild.specAction();
					}
				}
				else if (equipSec == 2)
				{
					if (_eSecWeapon2 is Flintlock flintchild)
					{
						flintchild.specAction();
					}
					else if (_eSecWeapon1 is StakeGun stakeChild)
					{
						stakeChild.specAction();
					}
				}
				else if (equipSec == 3 && !_twoHand)
				{
					if (_eSecWeapon3 is Flintlock flintchild)
					{
						flintchild.specAction();
					}
					else if (_eSecWeapon1 is StakeGun stakeChild)
					{
						stakeChild.specAction();
					}
				}
				else if (equipSec == 4 && !_twoHand)
				{
					if (_eSecWeapon4 is Flintlock flintchild)
					{
						flintchild.specAction();
					}
					else if (_eSecWeapon1 is StakeGun stakeChild)
					{
						stakeChild.specAction();
					}
				}
			}
		}

		if (Input.IsActionPressed("back"))
		{
			_backSpeed = Speed*(-0.30f);
			_running = false;
		}
		else
		{
			_backSpeed = 0;
		}
	}

	public void UnPause()
	{
		HorCamSense = Convert.ToSingle(_senseBar.Value / 1000);
		VerCamSense = Convert.ToSingle(_senseBar.Value / 1000);
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_interface = null;
	}

	// --- PHYSICS LOOP ---
	// Called every physics frame (usually 60 times per second). Used for movement and physics updates.
	public override void _PhysicsProcess(double delta)
	{
		if (_dead == true){return;}
		var camRef = (Camera)_cam;

		// FINAL camera transform = base + shake
		_cam.Position = _cameraBasePosition + camRef.ShakeOffsetPosition;
		_cam.Rotation = _cameraBaseRotation + camRef.ShakeOffsetRotation;

		_currentBiome = "Forest";
		_inGoalArea = false;

		if (_overlappingAreas.Contains("Plains")){_currentBiome = "Plains";}
		if (_overlappingAreas.Contains("Brittlebay Village")){_currentBiome = "Brittlebay Village";}
		if (_overlappingAreas.Contains("Swamp")){_currentBiome = "Swamp";}
		if (_overlappingAreas.Contains("GoalArea")) { _inGoalArea = true; }
		if (_overlappingAreas.Contains("Water")){ _knockVelocity = 15; }

		if (_currentBiome != _lastBiome)
		{
			_lastBiome = _currentBiome;
			GetNode<Ui>("UI")._areaNameTween = 3;
		}

		if (_currentBiome.Contains("Village"))
		{
			_health = _maxHealth;
			_stamina = _maxStamina;
		}

		if (!_inGoalArea)
		{
			GetNode<Label>("UI/DemoWarning").Visible = true;
			_demoCount -= (float)delta;
			if (_demoCount <= 0.75f)
			{
				_health = 0f;
			}
			GetNode<Label>("UI/DemoWarning").Text = "Turn Back Now \n" + Mathf.Round(_demoCount).ToString();
		}
		else
		{
			GetNode<Label>("UI/DemoWarning").Visible = false;
			_demoCount = 30f;
		}
		Vector3 velocity = Velocity; // Get the current velocity vector

		ShaderMaterial shaderMaterial = GetNode<ColorRect>("UI/Dither").Material as ShaderMaterial;
		shaderMaterial.SetShaderParameter("BlendAmount", Mathf.Lerp((float)shaderMaterial.GetShaderParameter("BlendAmount"), _hallucinationFactor, (float)delta));

		_hallucinationFactor = Mathf.Lerp(_hallucinationFactor, 0f, (float)delta / 10);
		if (_hallucinationFactor <= 0.4)
		{
			_hallucinationFactor = 0;
		}

		if (_currentStaminaTimer > 0f)
		{
			_currentStaminaTimer -= (float)delta;
		}
		// --- Lantern/Light Effects (Based on Health) ---
		// Smoothly interpolate the lantern's color between max and min health colors
		_lantern.LightColor = _lantern.LightColor.Lerp(_minHealthColor.Lerp(_maxHealthColor, _health / _maxHealth), (float)delta * 3f);
		_lightColor = _lantern.LightColor;
		// Scale the light's range based on current health
		_lantern.OmniRange = (_health / _maxHealth * _maxRange) + _minRange;

		// --- Stamina UI Update ---
		// Smoothly interpolate the stamina UI goal for a fluid bar movement
		_staminaGoal = Mathf.Lerp(_staminaGoal, _stamina / _maxStamina, (float)delta * 3f);
		// Update the ShaderMaterial parameter 'fill' to reflect the current stamina
		ShaderMaterial staminaShader = GetNode<Sprite2D>("UI/Stamina/Fill").Material as ShaderMaterial;
		staminaShader.SetShaderParameter("fill", _staminaGoal);

		// --- Death Condition ---
		if (_health <= 0)
		{
			_dead = true;
			GetNode<ColorRect>("UI/Dead").Visible = true;
		}

		// --- Parry Window Countdown ---
		if (_currentParryWindow > 0)
		{
			_currentParryWindow -= (float)delta;
		}
		else
		{
			// Parry window closed
			_parry = false;
			_currentParryWindow = 0;
		}

		// --- Successful Parry Execution ---
		if (_parried == true)
		{
			_parried = false;
			Parry(); // Trigger the visual/sound effect for a successful parry
		}

		// --- Combat Cooldown Handling ---
		_combatNotif.Visible = _inCombat; // Show/hide the combat UI notification
		_combatCounter += 1;
		if (_combatCounter >= 250) // If enough frames have passed (approx 4 seconds at 60fps)
		{
			_combatCounter = 0;
			_inCombat = false; // Exit combat state
			_swordInst.ResetMonsterList(); // Reset list of monsters hit (prevents debouncing issues)
		}

		// --- Stamina Regeneration ---
		if (_running == false && _currentStaminaTimer <= 0)
		{
			if (_inCombat == true)
			{
				_stamina += 0.04f * _maxStamina * (float)delta; // Regenerate stamina slowly in combat
			}
			else
			{
				_stamina += 0.08f * _maxStamina * (float)delta; // Regenerate stamina slowly out of combat
			}
			if (_stamina >= _maxStamina)
			{
				_stamina = _maxStamina; // Cap stamina at max
			}
		}

		if (_questBox.FindChild("Lukas") != null)
		{
			VBoxContainer currentQuest = _questBox.GetNode<VBoxContainer>("Lukas");
			// Update the quest objective text
			if (_shrinesDestroyed >= 3) { (currentQuest.GetNode("Number") as Label).Text = "Complete!"; }
			else { (currentQuest.GetNode("Number") as Label).Text = _shrinesDestroyed+"/3"; }
		}

		if (_questBox.FindChild("Elizabeth") != null)
		{
			VBoxContainer currentQuest = _questBox.GetNode<VBoxContainer>("Elizabeth");
			// Update the quest objective text
			int logCount = _itemInv.GetItemCount("log");
			if (logCount >= 15) { (currentQuest.GetNode("Number") as Label).Text = "Complete!"; }
			else { (currentQuest.GetNode("Number") as Label).Text = logCount+"/15"; }
		}

		if (_questBox.FindChild("Mary") != null)
		{
			VBoxContainer currentQuest = _questBox.GetNode<VBoxContainer>("Mary");
			// Update the quest objective text
			int catCount = _itemInv.GetItemCount("taz") + _itemInv.GetItemCount("bridger") + _itemInv.GetItemCount("gnocchi");
			if (catCount >= 3) { (currentQuest.GetNode("Number") as Label).Text = "Complete!"; }
			else { (currentQuest.GetNode("Number") as Label).Text = catCount+"/3"; }
		}

		// [Inventory Camera Transition - Commented Out]

		// --- Update sensitivity from pause menu ---
		if (_interface != null)
		{
			HorCamSense = Convert.ToSingle(_senseBar.Value / 1000);
			VerCamSense = Convert.ToSingle(_senseBar.Value / 1000);
		}

		// --- Movement input calculation ---

		Vector2 inputDir = Input.GetVector("left", "right", "forward", "back"); // Get normalized 2D input
		// Convert 2D input to 3D direction relative to the player's head/facing
		Vector3 direction = (_head.GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (direction != Vector3.Zero)
		{
			// Player is moving
			_fullDashValue = 10f; // Reset dash value to standard
			if (_stamina <= 0.02f * _maxStamina)
			{
				_running = false; // Force stop running if stamina is too low
				_stamina = 0f;
			}
			// Calculate new velocity: Direction * (BaseSpeed + RunSpeed if running + DashSpeed)
			velocity.X = direction.X * (Speed + _speedOffset + _backSpeed + (RunSpeed * Convert.ToInt32(_running)) + (Mathf.Abs(direction.X) * -_knockVelocity) + _dashVelocity);
			velocity.Z = direction.Z * (Speed + _speedOffset + _backSpeed + (RunSpeed * Convert.ToInt32(_running)) + (Mathf.Abs(direction.Z) * -_knockVelocity) + _dashVelocity);

			if (_running == true)
			{
				if (_inCombat == true)
				{
					_stamina -= 8f * (float)delta; // Deduct stamina while running  
				}
				else
				{
					_stamina -= 2f * (float)delta; // Deduct stamina while running  
				}
				play_footstep(0.35f);
			}
			else
			{
				play_footstep(0.7f);
			}
		}
		else
		{
			// Player is stationary (no directional input)
			_fullDashValue = 15f; // Increase max dash value for a full boost on next dash
								  // If dash is active, smoothly move the player forward based on the dash (maintains momentum)
			Vector3 tempvelo = velocity;
			tempvelo = tempvelo.Lerp(_cam.GlobalTransform.Basis.Z * -1 * (_dashVelocity - _knockVelocity), (float)delta * 10f);
			velocity = new Vector3(tempvelo.X, velocity.Y, tempvelo.Z);
			if (IsOnFloor())
			{
				velocity = new Vector3(velocity.X, 0f, velocity.Z); // Keep Y velocity (gravity/jump) separate
			}
		}

		// --- Dash & Knockback Decay and Head Offset ---
		_dashVelocity = Mathf.Lerp(_dashVelocity, 0f, (float)delta * 6f); // Dash speed smoothly decreases
		_knockVelocity = Mathf.Lerp(_knockVelocity, 0f, (float)delta * 6f); // Dash speed smoothly decreases
		// Apply a subtle head 'squash' effect during the dash decay
		_headOffset = _headOffset.Lerp(new Vector3(0f, -_dashVelocity / 5f, 0f), (float)delta * 6f);
		_head.Position = _baseHeadPosition + _headOffset;

		// --- Camera FOV Scaling (Speed Effect) ---
		// Smoothly scale FOV based on current movement speed (for a "speed effect")
		float fovGoal = Mathf.Lerp(_cam.Fov, Velocity.Length() + 80, (float)delta * 10f);
		if (GetNode<Sprite2D>("UI/Controls").Visible == false){_cam.Fov = fovGoal;}
		
		
		// --- Interaction detection (General) ---
		if (GetMouseCollision() != null)
		{
			//if (!IsInstanceValid(_lastSeen)){return;}
			CharacterBody3D targetNode = GetMouseCollision();
			if (!targetNode.IsInGroup("Monster")){_lastSeen = targetNode;}
			if (targetNode is Object obj)
			{
				obj._player = this;
			}
			if (targetNode.GetNodeOrNull<Label3D>("Title") != null)
			{
				targetNode.GetNode<Label3D>("Title").Visible = true;
			}
			if (targetNode is NpcVillager node)
			{
				if (node._object == "Boat")
				{
					node._questPrompt.Visible = true;
				}
			}
		}

		// --- NPC Interaction detection (Dialogue/Quest Prompt) ---
		if (GetMouseCollision() != null && _originalDialouge == null)
		{
			CharacterBody3D targetNode = GetMouseCollision();
			if (targetNode is NpcVillager villager)
			{
				if (villager._object == "None")
				{
					// Handle different quest states by modifying the NPC's displayed dialogue
					if (villager._questComplete == false && villager._questInProgress == false) // Initial quest state
					{
						_lastSeen = villager;
						_originalDialouge = villager._questPrompt.Text;
						villager._questPrompt.Text = villager._questPrompt.Text + "\n" + "E to Talk";
					}
					else if (villager._questComplete == false && villager._questInProgress == true) // Quest in progress
					{
						_lastSeen = villager;
						_originalDialouge = villager.WaitingDialogue;
						villager._questPrompt.Text = villager.WaitingDialogue;
					}
					else if (villager._questComplete == true & villager._questInProgress == true) // Quest complete, ready for turn-in
					{
						_lastSeen = villager;
						_originalDialouge = villager.WaitingDialogue;
						villager._questPrompt.Text = villager.WaitingDialogue + "\n" + "E to Talk";
					}
				}
			}
		}
		// --- Interaction End ---
		else if (GetMouseCollision() == null && _originalDialouge != null) // Player looks away
		{
			// Restore the NPC's original dialogue text
			if (_lastSeen is NpcVillager villager) { villager._questPrompt.Text = _originalDialouge; }
			_lastSeen = null;
			_originalDialouge = null;
		}

		_speedCount -= (float)delta;
		if (_speedCount <= 0)
		{
			_speedOffset = 0f;
			_speedCount = 0f;
		}

		// --- Head bob + sword bob ---
		if (_dashVelocity <= 1.0) // Only apply bob when not dashing
		{
			// Calculate head bob position and apply to the camera
			Transform3D camTransformGoal = _cam.Transform;
			_bobTime += (float)delta * velocity.Length() * (Convert.ToInt32(IsOnFloor()) + 0.2f); // Advance bob time based on speed
			camTransformGoal.Origin = HeadBob(_bobTime);
			_cam.Transform = camTransformGoal;

			// Calculate a separate, more subtle bob for the lantern light
			Transform3D lightTransformGoal = _lantern.Transform;
			lightTransformGoal.Origin = LightBob(_bobTime);
			_lantern.Transform = lightTransformGoal;
		}

		if (_stamina <= 0)
		{
			_stamina = 0; // Limit minimum stamina
		}
 
		if (GetMouseCollision()  == null)
		{
			_lastSeen = null;
		}

		// --- Gravity ---
		if (!IsOnFloor()) { velocity += new Vector3(0f,-3.8f,0f) * (float)delta; } // Apply gravity if not on the floor
		
		// --- Apply movement ---
		Velocity = velocity;
		// Only call MoveAndSlide() if not paused by UI elements (Inventory, Controls, Dialogue)
		if (_inv.Visible == false 
		&& GetNode<Sprite2D>("UI/Controls").Visible == false
		&& GetNode<Control>("UI/Dialogue").Visible == false) {MoveAndSlide(); }
	}


	// --- CUSTOM FUNCTIONS ---

	// Handles the sword attack sequence, damage calculation, and combo logic.
	private async void Swing(bool justEqquipped)
	{
		_swordInst.ResetMonsterDebounce(); // Allow the sword to hit new monsters
		_attackCooldown = true; // Start the attack cooldown
		float swingTime = (float)_sword.GetMeta("swingSpeed"); // Get swing time from weapon metadata
		float comboTime = swingTime * 1000 + 400; // Time window for the next combo hit (in ms)
		_rng.Randomize();
		_sword.GetNode<Area3D>("Hitbox").GetNode<CollisionShape3D>("CollisionShape3D").Disabled = false; // Enable the hitbox
		_damage = (float)_sword.GetMeta("damage");
		float tempHorSense = HorCamSense;
		float tempVerSense = VerCamSense;

		// Damage penalty if stamina is too low
		if (_stamina <= 0.02f * _maxStamina)
		{
			_damage *= 0.75f;
		}

		// Skip stamina deduction and set swing time to zero if just equipping the weapon (for animation only)
		if (justEqquipped == true)
		{
			swingTime = 0f;
		}
		else
		{
			_stamina -= 0.05f * _maxStamina; // Deduct stamina for the attack
		}

		// --- Combo and Crit Logic ---
		if (Time.GetTicksMsec() - _lastHit < comboTime && _comboNum == 0 || _comboNum == 1)
		{
			_comboNum++; // Advance combo counter
			// Check for critical hit chance (meta tag)
			if (_rng.Randf() <= (float)_sword.GetMeta("cChance"))
			{
				_damage *= (float)_sword.GetMeta("cPercent1");
				_swordInst._crit = true;
				GD.Print("CRIT");
			}
		}
		else // Reset combo if time window expired or after the final combo hit
		{
			_comboNum = 0;
			if (_rng.Randf() <= (float)_sword.GetMeta("cChance"))
			{
				_damage *= (float)_sword.GetMeta("cPercent2");
				_swordInst._crit = true;
				GD.Print("CRIT");
			}
		}

		// Third hit combo check (uses a separate combo percent)
		if (Time.GetTicksMsec() - _lastHit > comboTime && _comboNum == 2)
		{
			_comboNum = 0;
			if (_rng.Randf() <= (float)_sword.GetMeta("cChance"))
			{
				_damage *= (float)_sword.GetMeta("cPercent3");
				_swordInst._crit = true;
			}
		}

		// --- Play Animation based on Combo ---
		if (_comboNum == 0)
		{
			_sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Swing1");
			HorCamSense /= 2.5f;
			VerCamSense /= 3f;
		}
		else if (_comboNum == 1)
		{
			_sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Swing2");
			HorCamSense /= 2.5f;
			VerCamSense /= 3f;
		}
		else if (_comboNum == 2)
		{
			_damage = (float)_sword.GetMeta("hDamage"); // Use a special high-damage value for the final hit
			_sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Swing3");
			HorCamSense /= 2f;
			VerCamSense /= 5f;
		}

		if (justEqquipped == true)
		{
			_sword.GetNode<AnimationPlayer>("AnimationPlayer").Stop(); // Don't animate if just equipped
		}

		GD.Print(comboTime, "abc", swingTime, "abc", _comboNum);
		_lastHit = Time.GetTicksMsec(); // Record the time of this hit
		play_sfx(GD.Load<AudioStreamOggVorbis>("res://Assets/SFX/Swing1.ogg"));
		// Wait for the main part of the swing animation to finish
		await ToSignal(GetTree().CreateTimer(swingTime * 0.7), "timeout");
		_sword.GetNode<Area3D>("Hitbox").GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true; // Disable the hitbox

		// Wait for the remainder of the swing (0 seconds in this case, a slight delay might be intended)
		await ToSignal(GetTree().CreateTimer(swingTime * 0), "timeout");
		_attackCooldown = false; // End the attack cooldown

		// Reset the players sensitivity
		HorCamSense = tempHorSense;
		VerCamSense = tempVerSense;
	}

	// Handles the blocking and parrying mechanic.
	private async void Block(bool block)
	{
		_blocking = block;
		if (block == true)
		{
			_sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Parry"); // Start the parry animation
			await ToSignal(GetTree().CreateTimer(0.05), "timeout"); // Wait for a brief moment
			_parry = block; // Set parry flag to true (the active parry window)
			_currentParryWindow = _parryWindow; // Start the parry timer
		}
		else
		{
			_parry = block; // Set parry flag to false
			_sword.GetNode<AnimationPlayer>("AnimationPlayer").PlayBackwards("Parry"); // Reverse the animation
		}
	}
	
	// Executes effects for a successful parry (called from _PhysicsProcess if _parried is true).
	private void Parry()
	{
		_parry = false; // Ensure parry window is closed
		_sword.GetNode<GpuParticles3D>("Parry").Emitting = true; // Play parry visual effect
		_hallucinationFactor = 0f;

		int ranSound = _rng.RandiRange(0, 2); // Randomize which sound effect to play
		if (ranSound == 0)
		{
			play_sfx(GD.Load<AudioStreamOggVorbis>("res://Assets/SFX/Parry1.ogg"));
		}
		else if (ranSound == 1)
		{
			play_sfx(GD.Load<AudioStreamOggVorbis>("res://Assets/SFX/Parry2.ogg"));
		}
		else if (ranSound == 2)
		{
			play_sfx(GD.Load<AudioStreamOggVorbis>("res://Assets/SFX/Parry3.ogg"));
		}
		
	}

	// Calculates the position offset for the head-bob effect based on time.
	private Vector3 HeadBob(float bobTime)
	{
		Vector3 pos = Vector3.Zero;
		pos.Y = Mathf.Sin(bobTime * BobFreq) * BobAmp; // Vertical bob using sine wave
		pos.X = Mathf.Cos(bobTime * BobFreq / 2) * BobAmp; // Horizontal bob using cosine wave (half frequency)
		return pos;
	}

	// Calculates the position offset for the lantern-bob effect (slightly different parameters).
	private Vector3 LightBob(float bobTime)
	{
		Vector3 pos = Vector3.Zero;
		pos.Y = Mathf.Sin(bobTime * BobFreq / 1.5f) * BobAmp;
		pos.X = Mathf.Cos(bobTime * BobFreq / 2.5f) * BobAmp;
		return pos;
	}
	
	// Performs a raycast to detect interactable CharacterBody3D objects in front of the camera.
	public CharacterBody3D GetMouseCollision()
	{
		if (_ray.IsColliding())
		{
			var collider = _ray.GetCollider();
			// Check if the collider is a CharacterBody3D and not the player itself
			if (collider != null && collider.GetClass() == "CharacterBody3D" && collider != this)
			{
				return (CharacterBody3D)_ray.GetCollider();
			}
		}
		return null;
	}

	// Updates the monster kill count, and updates the quest UI.
	public void MonsterKilled(string MonsterType, string MonsterBiome)
	{
		_monstersKilled += 1;
		if (MonsterBiome == "Swamp"){_swampMonstersKilled += 1;}
		else if (MonsterBiome == "Plains"){_plainsMonstersKilled += 1;}
		else if (MonsterBiome == "Forest"){_forestMonstersKilled += 1;}
		if (MonsterType == "villageRat")
		{
			_ratsKilled += 1;
		}
		if (_questBox.FindChild("Martha") != null)
		{
			VBoxContainer currentQuest = _questBox.GetNode<VBoxContainer>("Martha");
			// Update the quest objective text
			if (/*_swampMonstersKilled >= 5 && _plainsMonstersKilled >=5 && */_forestMonstersKilled >= 5) { (currentQuest.GetNode("Number") as Label).Text = "Complete!"; }
			else { (currentQuest.GetNode("Number") as Label).Text = "Forest: "+_forestMonstersKilled + "/5 \n"
			//+"Swamp: "+_swampMonstersKilled+ "/5 \n"
			/*+"Plains: "+_plainsMonstersKilled+ "/5 \n"*/; }
		}
		else if (_questBox.FindChild("Dillon") != null)
		{
			VBoxContainer currentQuest = _questBox.GetNode<VBoxContainer>("Dillon");
			// Update the quest objective text
			if (_ratsKilled >= 5) { (currentQuest.GetNode("Number") as Label).Text = "Complete!"; }
			else { (currentQuest.GetNode("Number") as Label).Text = _ratsKilled+"/5"; }
		}
	}

	// Removes a completed quest entry from the quest box UI.
	public void RemoveQuest(string QuestName)
	{
		if (_questBox.FindChild(QuestName) != null)
		{
			if (QuestName == "Elizabeth"){_itemInv.SubtractResource("log", _itemInv.GetItemCount("log"));}
			if (QuestName == "Mary"){_itemInv.SubtractResource("taz",1); _itemInv.SubtractResource("bridger",1); _itemInv.SubtractResource("gnocchi",1);}
			_questBox.FindChild(QuestName).QueueFree(); // Delete the UI node
		}
	}

	// Placeholder function for UI button interaction (currently only prints the button name).
	private void _on_quest_button_button_up(string buttonName)
	{
		GD.Print(buttonName);
	}

	// Creates a new quest entry in the quest box UI using the template node.
	public void GetQuest(string QuestTitle, string QuestGoal, string VillagerName)
	{
		Control questText = (VBoxContainer)_questTemplate.Duplicate(); // Clone the template
		_questBox.AddChild(questText);
		questText.Owner = _questBox; // Set owner for proper scene cleanup/handling
		questText.Name = VillagerName;
		questText.GetNode<Label>("Quest").Text = QuestTitle;
		questText.GetNode<Label>("Number").Text = QuestGoal.Replace(", ", "\n");
	}

	// Calls the Accepted/Ignored/Continue methods on the currently referenced NpcVillager.
	public void QuestAccepted()
	{
		_villager.Accepted();
	}

	public void QuestIgnored()
	{
		_villager.Ignored();
	}
	public void ContinueDialouge()
	{
		_villager.Continue();
	}

	// Handles damage taken by the player from a Monster3d (melee damage).
	public void Damaged(float takenDamage, Monster3d monster, string effect)
	{
		if (effect == "Hallucinate")
		{
			_hallucinationFactor = 1f;
		}
		if (effect == "StaminaDrain" && _blocking == false)
		{
			_stamina -= _stamina*0.15f;
		}
		if (effect == "BarkSpikes")
		{
			_knockVelocity = 75f;
		}
		if (effect == "Push")
		{
			_knockVelocity = 30f;
		}
		float shakeFade = 1f;
		if (takenDamage > _maxHealth/3){shakeFade = 0.5f;}
		if (_blocking == true && _parry == false)
		{
			// Regular block: reduce damage, deduct stamina, play block animation
			takenDamage *= 0.5f;
			_stamina -= 0.15f * _maxStamina;
			_sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Block");
			play_sfx(GD.Load<AudioStreamOggVorbis>("res://Assets/SFX/Block1.ogg"));
			if (effect != "Push"){_knockVelocity = 1f;}
			if (_cam is Camera cam)
			{
				cam.StartShake(takenDamage/80, shakeFade);
			}
			
		}
		else if (_blocking == true && _parry == true)
		{
			// Successful parry: restore stamina, negate damage, stun the monster, set parried flag
			_stamina += 0.20f * _maxStamina;
			takenDamage = 0f;
			monster.Stunned();
			_sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Block");
			_parried = true;
			_knockVelocity = 0f;
			if (_cam is Camera cam)
			{
				cam.StartShake(takenDamage/90, shakeFade);
			}
		}
		else if (_cam is Camera cam)
		{
			cam.StartShake(takenDamage/70, shakeFade);
		}
		
		// Damage multiplier if player is out of stamina
		if (_stamina <= 0.02f * _maxStamina)
		{
			takenDamage *= 1.1f;
			_knockVelocity *= 1.1f;
		}
		_health -= takenDamage; // Apply final damage
	}
	
	// Handles damage taken by the player from a RigidBody3D (ranged projectile).
	public void RangedDamaged(float takenDamage, RigidBody3D projectile, string effect)
	{
		_knockVelocity = 0.5f;
		if (effect == "Slowed"  && _blocking == false)
		{
			_speedOffset = -2.5f;
			_speedCount = 1.5f;
		}
		float shakeFade = 1f;
		if (takenDamage > _maxHealth/3){shakeFade = 0.5f;}
		if (_blocking == true && _parry == false)
		{
			// Regular block: reduce damage, deduct stamina, play block animation, destroy projectile
			takenDamage *= 0.5f;
			_stamina -= 0.15f * _maxStamina;
			_sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Block");
			play_sfx(GD.Load<AudioStreamOggVorbis>("res://Assets/SFX/Block1.ogg"));
			if (effect != "Push"){_knockVelocity = 0.25f;}
			if (_cam is Camera cam)
			{
				cam.StartShake(takenDamage/80, shakeFade);
			}
		}
		else if (_blocking == true && _parry == true)
		{
			// Successful parry: restore stamina, negate damage, destroy projectile, set parried flag
			_stamina += 0.15f * _maxStamina;
			takenDamage = 0f;
			_sword.GetNode<AnimationPlayer>("AnimationPlayer").Play("Block");
			projectile.QueueFree();
			_parried = true;
			_knockVelocity = 0f;
			if (_cam is Camera cam)
			{
				cam.StartShake(takenDamage/90, shakeFade);
			}
		}
		else if (_cam is Camera cam)
		{
			cam.StartShake(takenDamage/70, shakeFade);
		}
		
		_health -= takenDamage; // Apply final damage
	}

	// Switches the player's equipped primary weapon.
	public void SwitchPrimaryWeapon(string wepaonName, bool twoHanded = false)
	{
		_twoHand = twoHanded;
		PackedScene weaponScene = _weapon[wepaonName]; // Get the scene resource from the dictionary
		Node3D holder = GetNode<Node3D>("Head/Camera3D/Sword");
		holder.GetChild<Node3D>(0).QueueFree(); // Delete the old weapon
		Node3D swordInstance = weaponScene.Instantiate<Node3D>(); // Create new weapon instance
		holder.AddChild(swordInstance);                                             // Add new weapon to holder node
		swordInstance.Position = holder.Position;
		_sword = swordInstance; // Update the main sword reference
		_swordInst = _sword as SwordHandler; // Update the sword script reference
		Swing(true); // Call swing with 'justEquipped' to reset animation/position
	}

	// Switches the player's secondary weapon slots.
	public void SwitchSecondaryWeapon(string wepaonName, int slot)
	{ 
		PackedScene weaponScene = _secWeapon[wepaonName]; // Get the scene resource from the dictionary
		Node3D holder;
		if (slot == 0)
		{
			holder = GetNode<Node3D>("Head/Camera3D/Offhand1");
		}
		else if (slot == 1)
		{
			holder = GetNode<Node3D>("Head/Camera3D/Offhand2");
		}
		else if (slot == 2)
		{
			holder = GetNode<Node3D>("Head/Camera3D/Offhand3");
		}
		else
		{
			holder = GetNode<Node3D>("Head/Camera3D/Offhand4");
		}
		if (holder.GetChild(0) != null)
		{
			holder.GetChild<Node3D>(0).QueueFree(); // Delete the old weapon
		}
		Node3D weaponInstance = weaponScene.Instantiate<Node3D>(); // Create new weapon instance
		holder.AddChild(weaponInstance);                                             // Add new weapon to holder node
		weaponInstance.Position = holder.Position;
		if (slot == 0)
		{
			_eSecWeapon1 = weaponInstance;
		}
		else if (slot == 1)
		{
			_eSecWeapon2 = weaponInstance;
		}
		else if (slot == 2)
		{
			_eSecWeapon3 = weaponInstance;
		}
		else
		{
			_eSecWeapon4 = weaponInstance;
		}
	}
	public async void play_sfx(AudioStreamOggVorbis soundeffect)
	{
		AudioStreamPlayer player = new();
		AddChild(player);
		player.Stream = soundeffect;
		player.Play();
		await ToSignal(player, "finished");
		player.QueueFree();
	}
	public async void play_footstep(float stepSpeed)
	{
		if (!_inStep)
		{
			_inStep = true;
			await ToSignal(GetTree().CreateTimer(stepSpeed), "timeout");
			int ranStep = _rng.RandiRange(0, 2);
			if (ranStep == 0)
			{
				play_sfx(GD.Load<AudioStreamOggVorbis>("res://Assets/SFX/Footstep1.ogg"));
			}
			else if (ranStep == 1)
			{
				play_sfx(GD.Load<AudioStreamOggVorbis>("res://Assets/SFX/Footstep2.ogg"));
			}
			else
			{
				play_sfx(GD.Load<AudioStreamOggVorbis>("res://Assets/SFX/Footstep3.ogg"));
			}
			_inStep = false;
		}
		else
		{
			await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
		}
	}

	private async void secondaryCooldown()
	{
		_cooldownSec = true;
		if (equipSec == 1)
		{
			await ToSignal(GetTree().CreateTimer((float)_eSecWeapon1.GetMeta("cooldown")), "timeout");
		}
		else if (equipSec == 2)
		{
			await ToSignal(GetTree().CreateTimer((float)_eSecWeapon2.GetMeta("cooldown")), "timeout");
		}
		else if (equipSec == 3)
		{
			await ToSignal(GetTree().CreateTimer((float)_eSecWeapon3.GetMeta("cooldown")), "timeout");
		}
		else
		{
			await ToSignal(GetTree().CreateTimer((float)_eSecWeapon4.GetMeta("cooldown")), "timeout");
		}
		_cooldownSec = false;	
	}

	private void _on_hurtbox_area_entered(Area3D zone)
	{
		// Add all groups of the entered area to our tracking list
		foreach (string group in zone.GetGroups())
		{
			if (!_overlappingAreas.Contains(group))
			{
				_overlappingAreas.Add(group);
			}
			FindClosestGoal(zone);
		}
	}
	private void _on_hurtbox_area_exited(Area3D zone)
	{
		// Remove groups of the exited area from our tracking list
		foreach (string group in zone.GetGroups())
		{
			// Only remove if no other overlapping area belongs to that group
			FindClosestGoal(zone);
			bool groupStillPresent = false;
			Area3D playerArea = GetNode<Area3D>("Hurtbox");
			foreach (Area3D currentOverlap in playerArea.GetOverlappingAreas())
			{
				if (currentOverlap.IsInGroup(group))
				{
					groupStillPresent = true;
					break;
				}
			}

			if (!groupStillPresent)
			{
				_overlappingAreas.Remove(group);
			}
		}
	}
	
	private void FindClosestGoal(Area3D zone)
	{
		if (zone.GetGroups().Contains("GoalArea"))
		{
			foreach (CollisionShape3D collision in zone.GetChildren())
			{
				if ((collision.GlobalPosition - GlobalPosition).Length() < (_goalPointPos - GlobalPosition).Length())
				{
					_goalPointPos = collision.GlobalPosition;
					_goalPoint.GlobalPosition = GlobalPosition;
				}
			}
		}
	}

	public float CalculateLookAtAlignment()
	{
		if (_goalPoint == null)
		{
			return 1.0f; 
		}
		Vector3 playerForward = _cam.GlobalTransform.Basis.Z.Normalized(); 
		Vector3 playerToTarget = (_goalPoint.GlobalTransform.Origin - GlobalTransform.Origin).Normalized();

		float dotProduct = playerForward.Dot(playerToTarget);
		float alignment = (1.0f - dotProduct) / 2.0f;

		return alignment;
	}

	private async void StartCut()
    {
        await ToSignal(GetTree().CreateTimer(2f), "timeout");
		if (IsInstanceValid(_lastSeen) && _lastSeen is NpcVillager villager)
		{
			villager.Talk();
		}
    }
}
