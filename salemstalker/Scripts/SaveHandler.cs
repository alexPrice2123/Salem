using Godot;
using System;

public partial class SaveHandler : GodotObject
{
    public static void SaveToFile(Godot.Collections.Dictionary<string,Variant> toSave, string savePath)
    {
        using FileAccess file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
        string jsonData = Json.Stringify(toSave);
        file.StoreLine(jsonData);
    }

    public static Godot.Collections.Dictionary<string,Variant> LoadFromFile(string savePath)
    {
        using FileAccess file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
        string content = file.GetAsText(); 
        return (Godot.Collections.Dictionary<string,Variant>)Json.ParseString(content); 
    }

    public static void createSaveFile(string savePath)
    {
        using FileAccess file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
        string jsonData = Json.Stringify(new Godot.Collections.Dictionary<string, Variant>{
            { "lastLocation", "intro" },
            { "tutorialComplete", true}
        });
        file.StoreLine(jsonData);
        GD.Print("File set to defaults");
    }
}
