    using Godot;
    using System;

    [Tool] // This makes the script run in the editor
    public partial class WanderTool : CsgSphere3D
	{
		[Export]
		public float _wanderRange = 1.5f;
		public override void _Process(double delta)
        {
            if (Engine.IsEditorHint())
            {
                Radius = GetParent().GetNode<CsgSphere3D>("Range").Radius * _wanderRange;
            }
            else
            {
                // Code that runs only in the game
            }
        }
	}
