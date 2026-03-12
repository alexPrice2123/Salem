using Godot;
using System;

public partial class SaveHandler : GodotObject
{
    public static void SaveToFile(Godot.Collections.Dictionary<string,Variant> toSave, string savePath)
    {
        GD.Print("Attempting to save to: ", savePath, " with data: ", toSave.ToString());
        using FileAccess file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
        string jsonData = Json.Stringify(toSave);
        file.StoreLine(jsonData);
        GD.Print("Save complete");
    }

    public static Godot.Collections.Dictionary<string,Variant> LoadFromFile(string savePath)
    {
        using FileAccess file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
        string content = file.GetAsText(); 
        return (Godot.Collections.Dictionary<string,Variant>)Json.ParseString(content); 
    }

    public static void createSaveFile(string savePath)
    {
        if(!FileAccess.FileExists(savePath)){GD.Print("Creating save file");}
        else{GD.Print("File exists, reseting");}
        using FileAccess file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
        string jsonData = Json.Stringify(new Godot.Collections.Dictionary<string, Variant>{
            { "lastLocation", "intro" },
            { "tutorialComplete", false}
        });
        file.StoreLine(jsonData);
        GD.Print("File set to defaults");
    }
}
