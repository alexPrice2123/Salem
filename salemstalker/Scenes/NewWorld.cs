using Godot;
using System;
using System.Collections.Generic;

public partial class NewWorld : Node3D
{
    private Godot.Collections.Dictionary<string,Variant> data = new Godot.Collections.Dictionary<string,Variant>();
    string savePath = "user://saveData.json";
    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        if (!FileAccess.FileExists(savePath)){data.Add("place","testing");SaveToFile(data);GD.Print("Save Data created!");}
        data = LoadFromFile();
        GD.Print(data.ToString());
    }

    public void SaveToFile(Godot.Collections.Dictionary<string,Variant> toSave)
    {
        GD.Print( " this is a testing yeah5");
        using FileAccess file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
        GD.Print( " this is a testing yeah5");
        string jsonData = Json.Stringify(toSave);
        GD.Print( " this is a testing yeah5");
        file.StoreLine(jsonData);
        GD.Print( " this is a testing yeah5");
    }

    public Godot.Collections.Dictionary<string,Variant> LoadFromFile()
    {
        GD.Print( " this is a testing yeah2");
        using FileAccess file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
        GD.Print( " this is a testing yeah3");
        string content = file.GetAsText(); 
        GD.Print( " this is a testing yeah4");
        return (Godot.Collections.Dictionary<string,Variant>)Json.ParseString(content);
        
    }
}
