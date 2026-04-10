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
		GD.Print((bool)data["tutorialComplete"] == true, " ohstuffsave");
		if (((string)data["lastLocation"]).Equals("village1"))
		{
			GetNode<CharacterBody3D>("Player_3d").GlobalPosition = GetNode<Marker3D>("VillageMark").GlobalPosition ;
		}
		else if (((string)data["lastLocation"]).Equals("bossMark"))
		{
			GetNode<CharacterBody3D>("Player_3d").GlobalPosition = GetNode<Marker3D>("BossMark").GlobalPosition ;
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
	
	private void _on_brittlebay_area_entered(Area3D area)
	{
		if (area.IsInGroup("Player"))
		{
			data["lastLocation"] = "village1";
		}
		SaveHandler.SaveToFile(data,_savePath);
	}
	private void _on_brittlebay_area_exited(Area3D area)
	{
		if (area.IsInGroup("Player"))
		{
			data["lastLocation"] = "village1";
		}
		SaveHandler.SaveToFile(data,_savePath);
	}

	/// <summary>
	/// Updates the worlds known data of a quest
	/// </summary>
	/// <param name="villager">
	/// The name of the villager
	/// </param>
	/// <param name="progress">
	/// The level of progress for the quest to be changed to 
	/// -- 0 : unaccepted, 1 : accepted with no progress,2 : in-progress, 3 : complete -- 
	/// </param>
	public void updateQuest(string villager,int progress)
	{
		data[villager] = progress ;
	}
}
