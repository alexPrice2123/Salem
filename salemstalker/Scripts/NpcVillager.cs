using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class NpcVillager : CharacterBody3D
{
	// - Constants -
	public const float Speed = 5.0f;     					// The AI's speed
	public const float Range = 5.0f;    					// The max range between player and AI

	// - Variables -
	private Player3d _player;                               // Reference to the player object
	private RandomNumberGenerator _rng = new() ;            // RNG for idle times
	private bool moveStatus = true ;						// Whether the AI is in movement state or not
    private bool idleStatus = false;						// Whether the AI is idling or not 
    private NavigationAgent3D _navigationAgent ;			// Reference to the agent object
	public Label3D _questPrompt ;							// Reference to the prompt object
	private Vector3 WanderTarget ;							// The target for the AI to wander to whenever it is moving
	[Export]
	public string InitialDialouge;
	[Export]
	public string QuestDialouge;
	[Export]
	public string AcceptedDialouge;
	[Export]
	public string WaitingDialouge;
	[Export]
	public string DoneDialouge;
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

		_questPrompt.Text = InitialDialouge;

		// Hide the quest prompt
		_questPrompt.Hide();

		// Make sure to not await during _Ready.
		Callable.From(ActorSetup).CallDeferred();
	}

	public override void _PhysicsProcess(double delta) //Event tick; happens every frame
	{
		// Get the distance between the player and AI
		float distance = (_player.GlobalPosition - GlobalPosition).Length();
		// Add a temp variable for velocity
		Vector3 velocity = new();

		if (_questComplete == true)
		{
			_questPrompt.Text = DoneDialouge;
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
            moveStatus = false ;
			idleStatus = false ;
			velocity = Vector3.Zero;
			
			// Make the quest prompt appear 
			_questPrompt.Show();
		}
		
		if (moveStatus)
		{	
			// Calculate movement to next point
			MovementTarget = WanderTarget ;
			Vector3 nextPoint = _navigationAgent.GetNextPathPosition() ;
			velocity += (nextPoint - GlobalTransform.Origin).Normalized() * Speed ;

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
		await ToSignal(GetTree().CreateTimer(_rng.RandfRange(4.0f,10.0f)), "timeout"); 
		moveStatus = true;
        idleStatus = false;
    }
}
