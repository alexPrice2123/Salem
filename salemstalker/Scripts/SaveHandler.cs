using Godot;
using System;
using System.Collections.Generic;

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
            { "version", "0.5.4"},
            { "lastLocation", "intro" },
            { "tutorialComplete", false},
            { "mainWeaponUnlocked", new Godot.Collections.Dictionary<string, bool>{
                {"shortsword",true},
                {"longsword",false},
                {"dagger",false},
                {"flail",false},
                {"rapier",false}
                }
            },
            { "secondaryWeaponUnlocked", new Godot.Collections.Dictionary<string, bool>{
                {"flintlock",true},
                {"stakegun",false},
                {"shield",false},
                {"tomahawk",false},
                {"throwingknives",false},
                {"holyrelic",false},
                {"caltrops",false}
                }
            },
            {"resourceInventory", new Godot.Collections.Dictionary<string, int>{
                {"heartT1",0},
                {"heartT2",0},
                {"fleshT1",0},
                {"fleshT2",0},
                {"fangT1",0},
                {"fangT2",0},
                {"oozeT1",0},
                {"oozeT2",0},
                {"vesselT1",0},
                {"vesselT2",0},
                {"seedT1",0},
                {"seedT2",0}
                }
            },
            { "questList",new Godot.Collections.Dictionary<string, int>{
                /* -- 0 : unaccepted, 
                      1 : accepted with no progress,
                      2 : in-progress, 
                      3 : complete -- */
                {"mary",0},
                {"elizabeth",0},
                {"dillon",0},
                {"lukas",0},
                {"samuel",0},
                {"martha",0},
                {"finn",0},
                {"matthew1",0},
                {"matthew2",0},
                {"sophia",0},
                {"mason",0},
                {"donna",0},
                {"balthasar",0},
                {"clement",0},
                {"gabriel",0},
                {"priest",0},
                {"frederick",0},
                {"stranger",0},
                }
            },
            {"shrineComplete",new Godot.Collections.Dictionary<string, bool>{
                {"shrine1",false},
                {"shrine2",false},
                {"shrine3",false},
                {"shrine4",false}, 
                {"shrine5",false}
                }
            },
            {"deathBagPos",new Vector3(0,0,0)},
            {"deathBagCon", new Godot.Collections.Dictionary<string, int>{
                {"heartT1",0},
                {"heartT2",0},
                {"fleshT1",0},
                {"fleshT2",0},
                {"fangT1",0},
                {"fangT2",0},
                {"oozeT1",0},
                {"oozeT2",0},
                {"vesselT1",0},
                {"vesselT2",0},
                {"seedT1",0},
                {"seedT2",0}
                }
            }

        });
        file.StoreLine(jsonData);
        GD.Print("File set to defaults");
    }

    public static bool checkCompatibility(string savePath)
    {
        Godot.Collections.Dictionary<string,Variant> data = LoadFromFile(savePath);
        if(!data.ContainsKey("version")) {GD.Print("Save is not up to date!"); return(false);}
        if (((string)data["version"]).Equals("0.5.4")){GD.Print("Save is up to date!"); return(true);}
        else{GD.Print("Save is not up to date!"); return(false);}
    }
}
