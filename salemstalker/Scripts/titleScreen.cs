using Godot;
using System;

public partial class titleScreen : Node3D
{
	private bool _play = false;
	private ShaderMaterial _loadingMaterial;
	private ColorRect _loadingUI;
	private TextureButton _playButton;
	private TextureButton _creditButton;
	private TextureButton _quitButton;
	private TextureButton _backButton;
	private string _nextScene;
	public Godot.Collections.Dictionary<string,Variant> data = new Godot.Collections.Dictionary<string,Variant>();
	public string _savePath = "user://saveData.json";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_loadingUI = GetNode<ColorRect>("Loading");
		_loadingMaterial = _loadingUI.Material as ShaderMaterial;
		_playButton = GetNode<TextureButton>("Buttons/Play");
		_creditButton = GetNode<TextureButton>("Buttons/Credits");
		_quitButton = GetNode<TextureButton>("Buttons/Quit");
		_backButton = GetNode<TextureButton>("Credits/Back");
		if ( !FileAccess.FileExists(_savePath) )
		{
			GD.Print("Save file missing");
			SaveHandler.createSaveFile(_savePath);
			GD.Print("Save file created");
		}
		else{ GD.Print("Save file exists"); }
        if ( !SaveHandler.checkCompatibility(_savePath) ){GetNode<AnimatedSprite2D>("Buttons/FunnyReset").Visible = true ;}
		data = SaveHandler.LoadFromFile(_savePath);
		GD.Print("Save file loaded");
		GetNode<Label>("Buttons/gameVer").Text = "Beta" + (string)data["version"];
	}


	private async void _on_play_button_up()
	{
		if ( !FileAccess.FileExists(_savePath))
		{
			GD.Print("Save file missing");
			SaveHandler.createSaveFile(_savePath);
			GD.Print("Save file created");
		}
		else{ GD.Print("Save file exists"); }
		GD.Print("Entering save file: ", _savePath);
		while ((float)_loadingMaterial.GetShaderParameter("progress") >= -1f)
		{
			_loadingMaterial.SetShaderParameter("progress", (float)_loadingMaterial.GetShaderParameter("progress")-0.05f);
			await ToSignal(GetTree().CreateTimer(0.01), "timeout"); 
		}
		if ((bool)data["tutorialComplete"] == true)
		{
			_nextScene = "res://Scenes/super_new_world.tscn";
		}
		else
		{
			_nextScene = "res://Scenes/cutscene_1.tscn";
		}
		GetTree().ChangeSceneToFile(_nextScene);
		
	}
	private void _on_credits_button_up()
	{
		if (_play){return;}
		GetNode<ColorRect>("Credits").Visible = true;
	}
	private void _on_quit_button_up()
	{
		if (_play){return;}
		GetTree().Quit(); // Quit the scene 
	}
	private void _on_back_button_up()
	{
		if (_play){return;}
		GetNode<ColorRect>("Credits").Visible = false;
	}
	private void _on_reset_save_button_pressed()
	{
		SaveHandler.createSaveFile(_savePath);
        GetNode<AnimatedSprite2D>("Buttons/FunnyReset").Visible = false ;
		data = SaveHandler.LoadFromFile(_savePath);
	}
	private void _on_dev_menu_button_button_up()
	{
		GetNode<ColorRect>("DevMenu").Visible = true ;
		GetNode<CheckBox>("DevMenu/tutorialCheck").ButtonPressed = (bool)data["tutorialComplete"];
		GetNode<CheckBox>("DevMenu/ShrinesButtons/shrine1").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["shrineComplete"])["shrine1"];
		GetNode<CheckBox>("DevMenu/ShrinesButtons/shrine2").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["shrineComplete"])["shrine2"];
		GetNode<CheckBox>("DevMenu/ShrinesButtons/shrine3").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["shrineComplete"])["shrine3"];
		GetNode<CheckBox>("DevMenu/ShrinesButtons/shrine4").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["shrineComplete"])["shrine4"];
		GetNode<CheckBox>("DevMenu/ShrinesButtons/shrine5").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["shrineComplete"])["shrine5"];
		GetNode<CheckBox>("DevMenu/MainWeapons/shortCheck").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["mainWeaponUnlocked"])["shortsword"];
		GetNode<CheckBox>("DevMenu/MainWeapons/longCheck").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["mainWeaponUnlocked"])["longsword"];
		GetNode<CheckBox>("DevMenu/MainWeapons/daggerCheck").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["mainWeaponUnlocked"])["dagger"];
		GetNode<CheckBox>("DevMenu/MainWeapons/flailCheck").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["mainWeaponUnlocked"])["flail"];
		GetNode<CheckBox>("DevMenu/MainWeapons/rapierCheck").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["mainWeaponUnlocked"])["rapier"];
		GetNode<CheckBox>("DevMenu/SecondaryWeapons/flintlockCheck").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["secondaryWeaponUnlocked"])["flintlock"];
		GetNode<CheckBox>("DevMenu/SecondaryWeapons/stakegunCheck").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["secondaryWeaponUnlocked"])["stakegun"];
		GetNode<CheckBox>("DevMenu/SecondaryWeapons/shieldCheck").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["secondaryWeaponUnlocked"])["shield"];
		GetNode<CheckBox>("DevMenu/SecondaryWeapons/tomahawkCheck").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["secondaryWeaponUnlocked"])["tomahawk"];
		GetNode<CheckBox>("DevMenu/SecondaryWeapons/throwingkCheck").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["secondaryWeaponUnlocked"])["throwingknives"];
		GetNode<CheckBox>("DevMenu/SecondaryWeapons/caltropsCheck").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["secondaryWeaponUnlocked"])["caltrops"];
		GetNode<CheckBox>("DevMenu/SecondaryWeapons/relicCheck").ButtonPressed = ((Godot.Collections.Dictionary<string, bool>)data["secondaryWeaponUnlocked"])["holyrelic"];
		if(((string)data["lastLocation"]).Equals("intro"))
		{
			GetNode<OptionButton>("DevMenu/placeSelect").Selected = 0 ;
		}
		else if(((string)data["lastLocation"]).Equals("village1"))
		{
			GetNode<OptionButton>("DevMenu/placeSelect").Selected = 1 ;
		}
		else if(((string)data["lastLocation"]).Equals("bossTest"))
		{
			GetNode<OptionButton>("DevMenu/placeSelect").Selected = 2 ;
		}
	}
	private void _on_exit_dev_button_up()
	{
		GetNode<ColorRect>("DevMenu").Visible = false ;
	}
	private void _on_save_dev_button_up()
	{
		data["tutorialComplete"] = GetNode<CheckBox>("DevMenu/tutorialCheck").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["shrineComplete"])["shrine1"] = GetNode<CheckBox>("DevMenu/ShrinesButtons/shrine1").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["shrineComplete"])["shrine2"] = GetNode<CheckBox>("DevMenu/ShrinesButtons/shrine2").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["shrineComplete"])["shrine3"] = GetNode<CheckBox>("DevMenu/ShrinesButtons/shrine3").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["shrineComplete"])["shrine4"] = GetNode<CheckBox>("DevMenu/ShrinesButtons/shrine4").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["shrineComplete"])["shrine5"] = GetNode<CheckBox>("DevMenu/ShrinesButtons/shrine5").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["mainWeaponUnlocked"])["shortsword"] = GetNode<CheckBox>("DevMenu/MainWeapons/shortCheck").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["mainWeaponUnlocked"])["longsword"] = GetNode<CheckBox>("DevMenu/MainWeapons/longCheck").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["mainWeaponUnlocked"])["dagger"] = GetNode<CheckBox>("DevMenu/MainWeapons/daggerCheck").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["mainWeaponUnlocked"])["flail"] = GetNode<CheckBox>("DevMenu/MainWeapons/flailCheck").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["mainWeaponUnlocked"])["rapier"] = GetNode<CheckBox>("DevMenu/MainWeapons/rapierCheck").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["secondaryWeaponUnlocked"])["flintlock"] = GetNode<CheckBox>("DevMenu/SecondaryWeapons/flintlockCheck").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["secondaryWeaponUnlocked"])["stakegun"] = GetNode<CheckBox>("DevMenu/SecondaryWeapons/stakegunCheck").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["secondaryWeaponUnlocked"])["shield"] = GetNode<CheckBox>("DevMenu/SecondaryWeapons/shieldCheck").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["secondaryWeaponUnlocked"])["tomahawk"] = GetNode<CheckBox>("DevMenu/SecondaryWeapons/tomahawkCheck").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["secondaryWeaponUnlocked"])["throwingknives"] = GetNode<CheckBox>("DevMenu/SecondaryWeapons/throwingkCheck").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["secondaryWeaponUnlocked"])["caltrops"] = GetNode<CheckBox>("DevMenu/SecondaryWeapons/caltropsCheck").ButtonPressed;
		((Godot.Collections.Dictionary<string, bool>)data["secondaryWeaponUnlocked"])["holyrelic"] = GetNode<CheckBox>("DevMenu/SecondaryWeapons/relicCheck").ButtonPressed;
		if(GetNode<OptionButton>("DevMenu/placeSelect").Selected == 0)
		{
			data["lastLocation"] = "intro" ;
		}
		else if(GetNode<OptionButton>("DevMenu/placeSelect").Selected == 1)
		{
			data["lastLocation"] = "village1" ;
		}
		else if(GetNode<OptionButton>("DevMenu/placeSelect").Selected == 2)
		{
			data["lastLocation"] = "bossTest" ;
		}
		if (GetNode<CheckBox>("DevMenu/deathBag/bagCheck").ButtonPressed)
		{
			data["deathBagPos"] = new Vector3((float)GetNode<SpinBox>("DevMenu/deathBag/X").Value,(float)GetNode<SpinBox>("DevMenu/deathBag/Y").Value,(float)GetNode<SpinBox>("DevMenu/deathBag/Z").Value);
		}
		SaveHandler.SaveToFile(data,_savePath);
	}
	private void _on_bag_check_toggled(bool toggled)
	{
		GetNode<GridContainer>("DevMenu/deathBag/bagPos").Visible = toggled ;
	}
}

// code code coding code code 1+1=2 code woahhh code woahhhh coding code im adding yaoi to the code yaoi code + yuri code
