using Godot;
using System;

public partial class NpcVillager : CharacterBody3D
{
	private NavigationAgent3D _navigationAgent; 
	
    public const float speed = 5.0f ;
	private Vector3 _movementTargetPosition = new Vector3(-3.0f, 0.0f, 2.0f);
	
	public Vector3 MovementTarget
    {
        get { return _navigationAgent.TargetPosition; }
        set { _navigationAgent.TargetPosition = value; }
    }

	public override void _Ready()
    {
        base._Ready();

        _navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

        // Make sure to not await during _Ready.
        Callable.From(ActorSetup).CallDeferred();
    }

    public override void _PhysicsProcess(double delta) //Event tick; happens every frame
	{
		if (NavigationServer3D.MapGetIterationId(_navigationAgent.GetNavigationMap()) == 0)
        {
            return;
        }
		if (_navigationAgent.IsNavigationFinished())
        {
			GD.Print(MovementTarget);
            MovementTarget = NavigationServer3D.MapGetRandomPoint(_navigationAgent.GetNavigationMap(), 1, false);
        }
		Vector3 velocity = Velocity;
        Vector3 currentAgentPosition = GlobalTransform.Origin;
        Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();
        // Add the gravity.
        if (! IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
		
		
		Velocity = velocity + currentAgentPosition.DirectionTo(nextPathPosition) * speed;
		MoveAndSlide();
	}

	private async void ActorSetup()
    {
        // Wait for the first physics frame so the NavigationServer can sync.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        // Now that the navigation map is no longer empty, set the movement target.
        MovementTarget = _movementTargetPosition;
    }

	
}
