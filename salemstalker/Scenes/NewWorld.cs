using Godot;
using System; 
using System.Collections.Generic;

public partial class NewWorld : Node3D
{
    public Godot.Collections.Dictionary<string,Variant> data = new Godot.Collections.Dictionary<string,Variant>();
    public string _savePath = "user://saveData.json";

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        if ( !FileAccess.FileExists(_savePath) || !SaveHandler.checkCompatibility(_savePath))
        {
            GD.Print("Save file missing/outdated");
            SaveHandler.createSaveFile(_savePath);
            GD.Print("Save file created");
        }
        else{ GD.Print("Save file exists/up-to-date"); }
        data = SaveHandler.LoadFromFile(_savePath);
        GD.Print("Save file loaded");
        if ((bool)data["tutorialComplete"] == true)
        {
            GD.Print("tutorial complete, putting player at village");
            GetNode<CharacterBody3D>("Player_3d").GlobalPosition = GetNode<Marker3D>("VillageMark").GlobalPosition ;
            GD.Print("player is at location: ",GetNode<CharacterBody3D>("Player_3d").GlobalPosition == GetNode<Marker3D>("VillageMark").GlobalPosition);
        }
    }

    public void ToggleIcon(Node3D parentNode, string goalGroup, bool toggle)
    {
        GD.Print(goalGroup);
        foreach (Node3D node in parentNode.GetChildren())
        {
            if (node.IsInGroup(goalGroup))
            {
                node.GetNode<Sprite3D>("Icon").Visible = toggle;
            }
        }
    }
    
}
