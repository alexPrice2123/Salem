using Godot;
using System;

public partial class NpcVillager : CharacterBody3D
{
	private NavigationAgent3D _navigationAgent; 
	
    public const float speed = 5.0f ;
	
	public Vector3 MovementTarget
    {
        get { return _navigationAgent.TargetPosition; }
        set { _navigationAgent.TargetPosition = value; }
    }


    public override void _PhysicsProcess(double delta) //Event tick; happens every frame
	{
		
		Vector3 velocity = Velocity;

        
        // Add the gravity.
        if (! IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
		
		
		Velocity = velocity;
		MoveAndSlide();
	}

	

	
}
