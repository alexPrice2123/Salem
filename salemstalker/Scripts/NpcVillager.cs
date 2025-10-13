using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class NpcVillager : CharacterBody3D
{
	// - Constants -
	public const float Speed = 3.0f;                       // The AI's speed
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
	protected Vector3 WanderTarget;                          // The target for the AI to wander to whenever it is moving
	[Export]
	public string NPCName = "Bob"; 
	[Export]
	public string InitialDialogue = "Initial";             // This dialogue goes into the QuestPrompt 3d label, the rest of the dialogue is spoken through the UI
	[Export]
	public string QuestDialogue = "Quest";
	[Export]
	public string AcceptedDialogue = "Accepted";
	[Export]
	public string IgnoredDialogue = "Ignored";
	[Export]
	public string WaitingDialogue = "Waiting";
	[Export]
	public string DoneDialogue = "Done";
	[Export]
	public string QuestTitle = "Title";
	[Export]
	public string QuestGoal = "Goal";
	[Export]
	public bool _questComplete = false;
	public bool _questInProgress = false;
	public CharacterBody3D Villager;

	public Vector3 MovementTarget                           // The target for the AI to pathfind to
	{
		get { return _navigationAgent.TargetPosition; }
		set { _navigationAgent.TargetPosition = value; }
	}

	public void InitializeVillager()
	{
		// Get reference to relevant nodes.
		_player = this.GetParent().GetNode<Player3d>("Player_3d");
		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		_questPrompt = GetNode<Label3D>("QuestPrompt");
		_dialogueBox = _player.GetNode<Label>("UI/Dialogue/DialogueBox");
		_dialogue = _player.GetNode<Control>("UI/Dialogue");
		_acceptButton = _player.GetNode<Button>("UI/Dialogue/AcceptButton");
		_acceptButton.Pressed += QuestAccepted;
		_ignoreButton = _player.GetNode<Button>("UI/Dialogue/IgnoreButton");
		_ignoreButton.Pressed += QuestIgnored;
		WanderTarget = NavigationServer3D.MapGetRandomPoint(_navigationAgent.GetNavigationMap(), 2, false);

		_questPrompt.Text = InitialDialogue;

		// Hide the quest prompt
		_questPrompt.Hide();
		// Make sure to not await during _Ready.
		Callable.From(ActorSetup).CallDeferred();
	}
	
	public void QuestAccepted() //changes dialouge from the quest dialouge to accepted dialouge
	{
		GD.Print("Accepted");
	}
	public void QuestIgnored() //changes dialouge from the quest dialouge to ignored dialouge
	{ 
		GD.Print("Ignored");
	}
	
	public void EveryFrame(double delta) //Event tick; happens every frame
	{
		// Get the distance between the player and AI
		float distance = (_player.GlobalPosition - GlobalPosition).Length();
		// Add a temp variable for velocity
		Vector3 velocity = new();

		if (_questComplete == true && _questInProgress == false)
		{
			_questPrompt.Text = DoneDialogue;
		}

		if (_navigationAgent.IsNavigationFinished()) // If the AI is close enough to pathfinding target
		{
			// Get a new target position
			WanderTarget = NavigationServer3D.MapGetRandomPoint(_navigationAgent.GetNavigationMap(), 2, false);
			MovementTarget = WanderTarget;

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
				LookAt(new Vector3(playerPos.X, GlobalPosition.Y + 0.001f, playerPos.Z), Vector3.Up);
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
			MovementTarget = WanderTarget;
			Vector3 nextPoint = _navigationAgent.GetNextPathPosition();
			velocity += (nextPoint - GlobalTransform.Origin).Normalized() * Speed;

			// Make sure the quest prompt is hidden whenever player is not near
			_questPrompt.Visible = false;

			// Face wander target
			if (GlobalPosition.DistanceTo(nextPoint) > 0.01f)
			{
				LookAt(new Vector3(nextPoint.X, GlobalPosition.Y, nextPoint.Z), Vector3.Up);
			}
		}

		// Allow the AI to move when not idling
		if (!idleStatus)
		{
			moveStatus = true;
		}

		// Add safe velocity and move the AI
		Velocity = velocity;
		MoveAndSlide();
	}

	protected async void ActorSetup()
	{
		// Wait for the first physics frame so the NavigationServer can sync.
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

		// Now that the navigation map is no longer empty, set the movement target.
		MovementTarget = GlobalPosition;
		WanderTarget = NavigationServer3D.MapGetRandomPoint(_navigationAgent.GetNavigationMap(), 2, false);
	}

	protected async void WanderIdle()
	{
		// Make the villager wait for 4-10 seconds before moving again
		await ToSignal(GetTree().CreateTimer(_rng.RandfRange(4.0f, 10.0f)), "timeout");
		moveStatus = true;
		idleStatus = false;
	}

	public void Accepted()
	{
		_player.GetQuest(QuestTitle, QuestGoal);
		_dialogueBox.Text = AcceptedDialogue;
		_questInProgress = true;
		_questPrompt.Text = WaitingDialogue;
		_player._originalDialouge = WaitingDialogue;
		_dialogue.GetNode<Button>("Continue").Visible = true;
		_dialogue.GetNode<Button>("AcceptButton").Visible = false;
		_dialogue.GetNode<Button>("IgnoreButton").Visible = false;
	}

	public void Ignored()
	{
		_dialogueBox.Text = IgnoredDialogue;
		_dialogue.GetNode<Button>("Continue").Visible = true;
		_dialogue.GetNode<Button>("AcceptButton").Visible = false;
		_dialogue.GetNode<Button>("IgnoreButton").Visible = false;
	}

	public void Continue()
    {
		_player._villager = null;
		_dialogue.Visible = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
    }

	public void Talk()
	{
		_dialogue.GetNode<Button>("Continue").Visible = false;
		_dialogue.GetNode<Button>("AcceptButton").Visible = true;
		_dialogue.GetNode<Button>("IgnoreButton").Visible = true;
		if (Villager is KillMonstersQuest killVillager)
		{
			killVillager.NPCTalk();
		}
		else if (Villager is GetAppleQuest appleVillager)
        {
			appleVillager.NPCTalk();
        }
	}
}
