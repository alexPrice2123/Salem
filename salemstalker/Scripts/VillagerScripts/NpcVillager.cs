using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class NpcVillager : CharacterBody3D
{
	// - Constants -
	public const float Speed = 1f;                       // The AI's speed
	public const float Range = 3.0f;                       // The max range between player and AI

	// - Variables -
	protected int _questRequirement;                         
	protected Player3d _player;                              // Reference to the player object
	protected RandomNumberGenerator _rng = new();            // RNG for idle times
	protected bool moveStatus = true;                        // Whether the AI is in movement state or not
	protected bool idleStatus = false;                       // Whether the AI is idling or not 
	protected bool _questAccepted;
	protected string _name;
	protected NavigationAgent3D _navigationAgent;            // Reference to the agent object
	public Label3D _questPrompt;                           // Reference to the prompt object
	public Label _dialogueBox;
	public Control _dialogue; 
	public Button _acceptButton;
	public Button _ignoreButton;
	[Export]
	public string NPCName = "Bob"; 
	[Export]
	public string InitialDialogue = "Initial";             // This dialogue goes into the QuestPrompt 3d label, the rest of the dialogue is spoken through the UI
	[Export]
	public Godot.Collections.Array<string> QuestDialogue { get; set; } = new Godot.Collections.Array<string>{ "Quest" };
	[Export]
	public Godot.Collections.Array<string> AcceptedDialogue { get; set; } = new Godot.Collections.Array<string>{ "Accept" };
	[Export]
	public string IgnoredDialogue = "Ignored";
	[Export]
	public string WaitingDialogue = "Waiting";
	[Export]
	public Godot.Collections.Array<string> DoneDialogue { get; set; } = new Godot.Collections.Array<string>{ "Done" };
	[Export]
	public string PostDoneDialogue = "Done";
	[Export]
	public string QuestTitle = "Title";
	[Export]
	public string QuestGoal = "Goal";
	[Export]
	public bool _questComplete = false;
	public bool _questInProgress = false;
	public CharacterBody3D Villager;
	private int _dialougeIndex = 0;
	private string _currentDialouge = "Initial";
	public bool _hasTalked = false;
	public string _object = "None";
	protected bool ObjectActivated = false;
	protected Ui _interface;
	protected Node3D _lookDirection;
	protected Vector3 _startPos;
	protected Vector3 _wanderPos;
	protected float _wanderRange;

	public Vector3 MovementTarget                           // The target for the AI to pathfind to
	{
		get { return _navigationAgent.TargetPosition; }
		set { _navigationAgent.TargetPosition = value; }
	}

	public void InitializeVillager()
	{
		// Get reference to relevant nodes.
		_player = this.GetParent().GetNode<Player3d>("Player_3d");
		_questPrompt = GetNode<Label3D>("QuestPrompt");
		_dialogueBox = _player.GetNode<Label>("UI/Dialogue/DialogueBox");
		_dialogue = _player.GetNode<Control>("UI/Dialogue");
		_acceptButton = _player.GetNode<Button>("UI/Dialogue/AcceptButton");
		_acceptButton.Pressed += QuestAccepted;
		_ignoreButton = _player.GetNode<Button>("UI/Dialogue/IgnoreButton");
		_ignoreButton.Pressed += QuestIgnored;
		_interface = _player.GetNode<Ui>("UI");
		_lookDirection = GetNode<Node3D>("Direction");
		_startPos = GlobalPosition;
		_wanderRange = GetNode<CsgSphere3D>("Range").Radius;
		GetNode<CsgSphere3D>("Range").QueueFree();
		if (_object == "None")
        {
           _navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
			ChooseNewWander();
        }
		_questPrompt.Text = InitialDialogue;

		// Hide the quest prompt
		_questPrompt.Hide();
		// Make sure to not await during _Ready.
		Callable.From(ActorSetup).CallDeferred();
	}
	
	private async void ChooseNewWander()
	{
		float randZ = _startPos.Z + _rng.RandfRange(-_wanderRange, _wanderRange);
		float randX = _startPos.X + _rng.RandfRange(-_wanderRange, _wanderRange);
		_wanderPos = new Vector3(randX, 0f, randZ);
	}

	public void QuestAccepted() //changes dialouge from the quest dialouge to accepted dialouge
	{
		GD.Print("Accepted");
	}
	public void QuestIgnored() //changes dialouge from the quest dialouge to ignored dialouge
	{ 
		GD.Print("Ignored");
	}

	private void RotateFunc(double delta)
	{
		if (Mathf.RadToDeg(_lookDirection.GlobalRotation.Y) >= 175 || Mathf.RadToDeg(_lookDirection.GlobalRotation.Y) <= -175)
		{
			GlobalRotation = new Vector3(GlobalRotation.X, _lookDirection.GlobalRotation.Y, GlobalRotation.Z);
		}
		else
		{
			float newRotation = Mathf.Lerp(GlobalRotation.Y, _lookDirection.GlobalRotation.Y, (float)delta * 10f);
			GlobalRotation = new Vector3(GlobalRotation.X, newRotation, GlobalRotation.Z);
		}
	}
	
	public void EveryFrame(double delta) //Event tick; happens every frame
	{
		if (_object != "None")
		{
			if (_player._lastSeen != this)
            {
                _questPrompt.Visible = false;
            }
			return;
		}
		// Get the distance between the player and AI
		float distance = (_player.GlobalPosition - GlobalPosition).Length();
		// Add a temp variable for velocity
		Vector3 velocity = new();
		

		if (_questComplete == true && _questInProgress == false && _hasTalked == true)
		{
			_questPrompt.Text = PostDoneDialogue;
			if (NPCName == "Martha")
            {
                GetParent<DemoHandler>()._marthaDone = true;
            }
			else if (NPCName == "Lukas")
            {
                GetParent<DemoHandler>()._lukasDone = true;
            }
			else if (NPCName == "Dillon")
            {
                GetParent<DemoHandler>()._dillonDone = true;
            }
		}

		if (_navigationAgent.IsNavigationFinished()) // If the AI is close enough to pathfinding target
		{
			// Get a new target position
			ChooseNewWander();
			MovementTarget = _wanderPos;

			// Make the AI stop moving
			moveStatus = false;
			idleStatus = true;
			velocity = Vector3.Zero;

			// Make the AI idle
			WanderIdle();

		}

		if (distance <= Range) // If player is close enough
		{
			// Face the player
			Vector3 playerPos = _player.GlobalPosition;
			if (GlobalPosition.DistanceTo(playerPos) > 0.01f)
			{
				_lookDirection.LookAt(new Vector3(playerPos.X, GlobalPosition.Y + 0.001f, playerPos.Z), Vector3.Up);
			}

			// Make the AI stop moving
			moveStatus = false;
			idleStatus = false;
			velocity = Vector3.Zero;

			// Make the quest prompt appear 
			if (_dialogue.Visible == false){ _questPrompt.Visible = true; }
		}

		if (moveStatus)
		{
			// Calculate movement to next point
			MovementTarget = _wanderPos;
			Vector3 nextPoint = _navigationAgent.GetNextPathPosition();
			velocity += (nextPoint - GlobalTransform.Origin).Normalized() * Speed;

			// Make sure the quest prompt is hidden whenever player is not near
			_questPrompt.Visible = false;

			// Face wander target
			Vector3 moveDirection = Velocity.Normalized(); 
			if (moveDirection != Vector3.Zero)
			{
				_lookDirection.LookAt(GlobalTransform.Origin + moveDirection, Vector3.Up); 
			}
		}

		// Allow the AI to move when not idling
		if (!idleStatus)
		{
			moveStatus = true;
		}

		// Add safe velocity and move the AI
		Velocity = velocity;
		if (!IsOnFloor())
        {
            Velocity += new Vector3(0f,-50f,0f) * (float)delta;
        }
		RotateFunc(delta);
		MoveAndSlide();
	}

	protected async void ActorSetup()
	{
		// Wait for the first physics frame so the NavigationServer can sync.
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

		// Now that the navigation map is no longer empty, set the movement target.
		MovementTarget = GlobalPosition;
		ChooseNewWander();
	}

	protected async void WanderIdle()
	{
		// Make the villager wait for 4-10 seconds before moving again
		await ToSignal(GetTree().CreateTimer(_rng.RandfRange(4.0f, 10.0f)), "timeout");
		moveStatus = true;
		idleStatus = false;
	}

	public void EndDialouge()
    {
        _player._villager = null;
		_dialogue.Visible = false;
		_dialougeIndex = 0;
		_currentDialouge = "Initial";
		Input.MouseMode = Input.MouseModeEnum.Captured;
    }

	public void Accepted()
	{
		if (_object != "None")
        {
            ObjectActivated = true;
        }
        else
        {
			_player.GetQuest(QuestTitle, QuestGoal, NPCName);
			_dialougeIndex = 0;
			_currentDialouge = "Accepted";
			_hasTalked = true;
			_dialogueBox.Text = AcceptedDialogue[_dialougeIndex];
			_questInProgress = true;
			_questPrompt.Text = WaitingDialogue;
			_player._originalDialouge = WaitingDialogue;
			_dialogue.GetNode<Button>("Continue").Visible = true;
			_dialogue.GetNode<Button>("AcceptButton").Visible = false;
			_dialogue.GetNode<Button>("IgnoreButton").Visible = false;   
        }
	}

	public void Ignored()
	{
		if (_object != "None")
        {
            EndDialouge();
        }
        else
        {
           	_currentDialouge = "Ignored";
			_dialogueBox.Text = IgnoredDialogue;
			_dialogue.GetNode<Button>("Continue").Visible = true;
			_dialogue.GetNode<Button>("AcceptButton").Visible = false;
			_dialogue.GetNode<Button>("IgnoreButton").Visible = false; 
        }
	}

	public void Continue()
	{
		if ((_currentDialouge == "Quest" && _dialougeIndex < QuestDialogue.Count-1)
		|| (_currentDialouge == "Accepted" && _dialougeIndex < AcceptedDialogue.Count-1)
		|| (_currentDialouge == "Done" && _dialougeIndex < DoneDialogue.Count-1))
		{
			if (_currentDialouge == "Quest")
			{
				_dialougeIndex += 1;
				_dialogueBox.Text = QuestDialogue[_dialougeIndex];
			}
			else if (_currentDialouge == "Accepted")
			{
				_dialougeIndex += 1;
				_dialogueBox.Text = AcceptedDialogue[_dialougeIndex];
			}
			else if (_currentDialouge == "Done")
            {
				_dialougeIndex += 1;
				_dialogueBox.Text = DoneDialogue[_dialougeIndex];
            }
		}
		else
		{
			if (_currentDialouge == "Done")
            {
				_questInProgress = false;
				_hasTalked = true;
				_player.RemoveQuest(NPCName);
            }
			EndDialouge();
		}
		CheckDialougeIndex();
	}

	public void Talk()
	{
		if (_questComplete == true && _questInProgress == false && _hasTalked == true) { return; } //if the quest is done the player can't interact

		if (_questComplete == true ) //what happens when the player talks to him after completing the quest
		{
			_dialougeIndex = 0;
			_currentDialouge = "Done";
			_questPrompt.Visible = false;
			_player._villager = this;
			_dialogue.Visible = true;
			_dialogueBox.Text = DoneDialogue[_dialougeIndex];
			Input.MouseMode = Input.MouseModeEnum.Visible;
			_dialogueBox.GetNode<Label>("NameText").Text = NPCName;
		}
		else  //what happens when the player talks to him before completing the quest
		{
			_dialogueBox.Text = WaitingDialogue;
		}

		if (_questPrompt.Visible == true && _questInProgress == false && _questComplete == false && _currentDialouge != "Quest")
		{
			_dialougeIndex = 0;
			_currentDialouge = "Quest";
			_questPrompt.Visible = false;
			_player._villager = this;
			_dialogue.Visible = true;
			_dialogueBox.Text = QuestDialogue[_dialougeIndex];
			Input.MouseMode = Input.MouseModeEnum.Visible;
			GD.Print(Input.MouseMode);
			_dialogueBox.GetNode<Label>("NameText").Text = NPCName;
		}

		CheckDialougeIndex();
	}

    private void CheckDialougeIndex()
    {
		if (_currentDialouge == "Quest")
		{
			GD.Print(_dialougeIndex);
			if (_dialougeIndex < QuestDialogue.Count - 1)
			{
				_dialogue.GetNode<Button>("Continue").Visible = true;
				_dialogue.GetNode<Button>("AcceptButton").Visible = false;
				_dialogue.GetNode<Button>("IgnoreButton").Visible = false;
			}
			else
			{
				_dialogue.GetNode<Button>("Continue").Visible = false;
				_dialogue.GetNode<Button>("AcceptButton").Visible = true;
				_dialogue.GetNode<Button>("IgnoreButton").Visible = true;
			}
		}
		else if (_currentDialouge == "Accepted" || _currentDialouge == "Done")
		{
			GD.Print(_dialougeIndex);
			_dialogue.GetNode<Button>("Continue").Visible = true;
			_dialogue.GetNode<Button>("AcceptButton").Visible = false;
			_dialogue.GetNode<Button>("IgnoreButton").Visible = false;
        }
    }
}
