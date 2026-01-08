using Godot;
using System;
[Tool]
public partial class BorderTool : StaticBody3D
{
	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			foreach (CollisionShape3D shape in GetChildren())
            {
				if (shape.Shape is BoxShape3D boxShape)
                {
                   shape.GetNode<CsgBox3D>("Mesh").Size = boxShape.Size; 
                }
                 shape.GetNode<CsgBox3D>("Mesh").GlobalRotation = shape.GlobalRotation;
            }
		}
		else
		{
			foreach (CollisionShape3D shape in GetChildren())
            {
				if (shape.GetChildCount() > 0)
                {
                    shape.GetNode<CsgBox3D>("Mesh").QueueFree();
                }
            }
		}
	}
}
