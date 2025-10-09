using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class NpcVillager : CharacterBody3D
{
	// - Constants -
	public const float Speed = 5.0f;                       // The AI's speed
	public const float Range = 5.0f;                       // The max range between player and AI

	// - Variables -
	private int _questRequirement;                         
	private Player3d _player;                              // Reference to the player object
	private RandomNumberGenerator _rng = new();            // RNG for idle times
	private bool moveStatus = true;                        // Whether the AI is in movement state or not
	private bool idleStatus = false;                       // Whether the AI is idling or not 
	private bool _questAccepted;
	private string _name;
	private NavigationAgent3D _navigationAgent;            // Reference to the agent object
	public Label3D _questPrompt;                           // Reference to the prompt object
	public Label _dialogueBox;
	public Control _dialogue; 
	public Button _acceptButton;
	public Button _ignoreButton;
	private Vector3 WanderTarget;                          // The target for the AI to wander to whenever it is moving
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

	public Vector3 MovementTarget                           // The target for the AI to pathfind to
	{
		get { return _navigationAgent.TargetPosition; }
		set { _navigationAgent.TargetPosition = value; }
	}

	public override void _Ready()
	{
		// Get reference to relevant nodes.
		_player = this.GetParent().GetNode<Player3d>("Player_3d");
		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		_questPrompt = GetNode<Label3D>("QuestPrompt");
		_dialogueBox = GetNode<Label>("UI/Dialogue/DialogueBox");
		_dialogue = GetNode<Control>("UI/Dialogue");
		_acceptButton = GetNode<Button>("UI/Dialogue/AcceptButton");
		_acceptButton.Pressed += QuestAccepted;
		_ignoreButton = GetNode<Button>("UI/Dialogue/IgnoreButton");
		_ignoreButton.Pressed += QuestIgnored;

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
	
	public override void _PhysicsProcess(double delta) //Event tick; happens every frame
	{
		// Get the distance between the player and AI
		float distance = (_player.GlobalPosition - GlobalPosition).Length();
		// Add a temp variable for velocity
		Vector3 velocity = new();

		if (_questComplete == true)
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
			_questPrompt.Show();
		}

		if (moveStatus)
		{
			// Calculate movement to next point
			MovementTarget = WanderTarget;
			Vector3 nextPoint = _navigationAgent.GetNextPathPosition();
			velocity += (nextPoint - GlobalTransform.Origin).Normalized() * Speed;

			// Make sure the quest prompt is hidden whenever player is not near
			_questPrompt.Hide();

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

	private async void ActorSetup()
	{
		// Wait for the first physics frame so the NavigationServer can sync.
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

		// Now that the navigation map is no longer empty, set the movement target.
		MovementTarget = GlobalPosition;
		WanderTarget = NavigationServer3D.MapGetRandomPoint(_navigationAgent.GetNavigationMap(), 2, false);
	}

	private async void WanderIdle()
	{
		// Make the villager wait for 4-10 seconds before moving again
		await ToSignal(GetTree().CreateTimer(_rng.RandfRange(4.0f, 10.0f)), "timeout");
		moveStatus = true;
		idleStatus = false;
	}

	public void Talk()
	{
		bool done = false;
		if (_questComplete == true) { return; } //if the quest is done the player can't interact
		
		if (_player._monstersKilled < 5 && _player._questBox.FindChild("KillMonsters") == null) //changes dialouge from the initial dialouge to quest dialouge 
		{
			_dialogueBox.Text = QuestDialogue;
		}
		if (_player._monstersKilled >= 5) //what happens when the player talks to him after completing the quest
		{
			_dialogueBox.Text = DoneDialogue;
			_questComplete = true;
			_player.RemoveQuest("KillMonsters");
		}
		else if (_player._monstersKilled < 5) //what happens when the player talks to him before completing the quest
		{
			_dialogueBox.Text = WaitingDialogue;
		}
		//if(Input.IsActionJustPressed("continue_dialogue") && !done)
		//{
			
		//}
	}
}
