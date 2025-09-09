using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class NpcVillager : CharacterBody3D
{
	// - Constants -
	public const float Speed = 3.0f;     
	public const float Range = 5.0f;    

	// - Variables -
	private Player3d _player;                                                   // Reference to the player object
	private RandomNumberGenerator _rng = new() ;          // RNG for idle times
	private bool moveStatus = true ;
	private NavigationAgent3D _navigationAgent ;
	private Label3D _questPrompt ;
	private Vector3 WanderTarget ;
	public Vector3 MovementTarget
	{
		get { return _navigationAgent.TargetPosition; }
		set { _navigationAgent.TargetPosition = value; }
	}

   

	public override void _Ready()
	{
		// Get reference to the player
		_player = this.GetParent().GetNode<Player3d>("Player_3d");

		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		_questPrompt = GetNode<Label3D>("QuestPrompt");
		_questPrompt.Hide();

		// Make sure to not await during _Ready.
		Callable.From(ActorSetup).CallDeferred();
	}

	public override void _PhysicsProcess(double delta) //Event tick; happens every frame
	{
		float distance = (_player.GlobalPosition - GlobalPosition).Length();
		Vector3 velocity = new();
		if (_navigationAgent.IsNavigationFinished())
		{
			GD.Print("Getting new target and idling.");
			WanderTarget = NavigationServer3D.MapGetRandomPoint(_navigationAgent.GetNavigationMap(), 1, false);
			WanderIdle();
		}
		if (distance <= Range) // If player is close enough
		{
			// Face the player
			Vector3 playerPos = _player.GlobalPosition;
			LookAt(new Vector3(playerPos.X, GlobalPosition.Y, playerPos.Z), Vector3.Up);
			moveStatus = false ;
			velocity = Vector3.Zero;
			
			_questPrompt.Show();
		}
		
		if (moveStatus)
		{
			GD.Print("Moving.");
			MovementTarget = WanderTarget ;
			Vector3 nextPoint = _navigationAgent.GetNextPathPosition() ;
			velocity += (nextPoint - GlobalTransform.Origin).Normalized() * Speed ;

			// Face wander target
			LookAt(new Vector3(nextPoint.X, GlobalPosition.Y, nextPoint.Z), Vector3.Up);
		}
		_questPrompt.Hide();
		moveStatus = true;
		Velocity = velocity;
		MoveAndSlide();
	}

	private async void ActorSetup()
	{
		// Wait for the first physics frame so the NavigationServer can sync.
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

		// Now that the navigation map is no longer empty, set the movement target.
		MovementTarget = GlobalPosition;
	}

	private async void WanderIdle()
	{
		GD.Print("Idle start");
		await ToSignal(GetTree().CreateTimer(_rng.RandfRange(0.0f,10.0f)), "timeout");
		GD.Print("Idle end");
	}
}
