using Godot;
using Microsoft.VisualBasic;
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;

public partial class Ui : Control
{
	public static Ui Instance { get; private set; }
	public Player3d _player;
	private string _hovering = "falchionHover";
	private string _prevSelection;
	private string _secItemSend = null;
	private bool _loaded = false;
	private Control _loadingUI;
	private ShaderMaterial _loadingMaterial;
	private Control _slotSelect;
	private OptionButton _sShopTypeSelection;
	private OptionButton _wShopTypeSelection;
	private int _typeSelection;
	private string _sShopSelection = "Shortsword";
	private string _wShopSelection = "lHealth";
	private Label _areaName;
	public float _areaNameTween = 0f;
	private Godot.Collections.Dictionary<string,Variant> _shopDialogue;
	private Godot.Collections.Dictionary<string,Variant> _upgradeNames;
	private Godot.Collections.Dictionary<string,Variant> _upgradeAmounts;
	private Godot.Collections.Dictionary<string,Variant> _resourceNames;
	private Godot.Collections.Dictionary<string,Variant> _sResourceRefrences;
	private Godot.Collections.Dictionary<string,Variant> _sResourceAmounts;
	private float _loadingValue = -1f;
	public float _loadingGoal = 3f;
	public bool _loadingDone = false;
	public string _loadingObjective = "None";
	private TextureProgressBar _secProgressBar1;
	private TextureProgressBar _secProgressBar2;
	//private TextureProgressBar _secProgressBar3;
	//private TextureProgressBar _secProgressBar4;
	public float _progress1 = 100;
	public float _progress2 = 100;
	//public float _progress3 = 100;
	//public float _progress4 = 100;
	public Control _resourceInv;
	public GridContainer _gridContainer;
	private Texture2D _invIcon;
	private Control _inv;
	public float _fadeProg = 0; //fade progress for fading to black
	private bool _itemCheck = false;

	public override void _Ready()
	{
		_shopDialogue = SaveHandler.LoadFromFile("Scripts/ShopDialogue.json");
		_upgradeNames = _shopDialogue["upgradeNames"].AsGodotDictionary<string,Variant>();
		_upgradeAmounts = _shopDialogue["upgradeAmounts"].AsGodotDictionary<string,Variant>();
		_resourceNames = _shopDialogue["resourceNames"].AsGodotDictionary<string,Variant>();
		_sResourceRefrences = _shopDialogue["smithResourceRefrences"].AsGodotDictionary<string,Variant>();
		_sResourceAmounts = _shopDialogue["smithResourceAmounts"].AsGodotDictionary<string,Variant>();
		Instance = this;
		if (GetParent() is Player3d player)
		{
			_player = player;
		}
		_slotSelect = GetNode<Control>("Inv/SubPort/Sub/SlotSelector");
		_loadingUI = GetNode<Control>("Loading");
		_sShopTypeSelection = GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions");
		_wShopTypeSelection = GetNode<OptionButton>("WizardShop/ShopTypeOptions");
		_secProgressBar1 = GetNode<TextureProgressBar>("SecCooldown1");
		_secProgressBar2 = GetNode<TextureProgressBar>("SecCooldown2");
		//_secProgressBar3 = GetNode<TextureProgressBar>("SecCooldown3");
		//_secProgressBar4 = GetNode<TextureProgressBar>("SecCooldown4");
		_resourceInv = GetNode<Control>("ResourceInv");
		_gridContainer = GetNode<GridContainer>("ResourceInv/GridContainer");
		_inv = GetNode<Control>("Inv");
		_invIcon = (Texture2D)GD.Load("res://icon.svg");
		_loadingUI.Visible = true;
		_loadingMaterial = _loadingUI.Material as ShaderMaterial;
		_areaName = GetNode<Label>("Area");

		for(int i = 1; i < 30; i++)
        {
			_gridContainer.AddChild(_gridContainer.GetNode("InvSlot1").Duplicate());
			_gridContainer.GetChild(i).Name = "InvSlot" + i;
			//GD.Print(_gridContainer.GetChild(i).Name);
        }

		PlayShopAnim("dagger");
		PlayShopAnim("falchion");
		PlayShopAnim("shortsword");
		Load();
	}

	public override void _Process(double delta)
	{ 
		// I  don't really like programming but the dread that learning how to do another discipline's skills brings outweighs that of being stuck with this
		// maybe I lowk just hate being in UI genuinely like it was a joke before yet for more I yearn either way
		GetParent().GetNode<ColorRect>("Fade").Color = GetParent().GetNode<ColorRect>("Fade").Color.Lerp(new Color(0,0,0,_fadeProg), (float)delta*2);

		Color newTransparency = _areaName.Modulate;
		newTransparency.A = Mathf.Lerp(_areaName.Modulate.A, _areaNameTween, (float)delta);
		_areaName.Modulate = newTransparency;
		_areaName.Text = _player._currentBiome;
		GetNode<Label>("FPS").Text = Engine.GetFramesPerSecond().ToString();

		_areaNameTween = Mathf.Lerp(_areaNameTween, 0f, (float)delta);

		// Switch the titles of the buttons based on what shopOption you have selected
		if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 0) 
		{
			GetNode<Label>("BlacksmithShop/ShopOption1/Label").Text = "Shortsword";
			GetNode<Label>("BlacksmithShop/ShopOption2/Label").Text = "Falchion";
			GetNode<Label>("BlacksmithShop/ShopOption3/Label").Text = "Rapier";
			GetNode<Label>("BlacksmithShop/ShopOption4/Label").Text = "Dagger";
		}
		else
		{
			GetNode<Label>("BlacksmithShop/ShopOption1/Label").Text = "Longsword";
			GetNode<Label>("BlacksmithShop/ShopOption2/Label").Text = "Greatsword";
			GetNode<Label>("BlacksmithShop/ShopOption3/Label").Text = "Battle Axe";
			GetNode<Label>("BlacksmithShop/ShopOption4/Label").Text = "Halberd";
		}
		//same for wizard shop
		if (GetNode<OptionButton>("WizardShop/ShopTypeOptions").Selected == 0) 
		{
			GetNode<Label>("WizardShop/ShopOption2/Label").Text = "Normal";
			GetNode<Button>("WizardShop/ShopOption3").Visible = true;
		}
		else 
		{
			GetNode<Label>("WizardShop/ShopOption2/Label").Text = "Greater";
			GetNode<Button>("WizardShop/ShopOption3").Visible = false;
		}
		if (_loaded == true) //Waits 1 second for the game to load before the ui tweens
		{
			_loadingValue = Mathf.Lerp(_loadingValue, _loadingGoal, (float)delta);
			_loadingMaterial.SetShaderParameter("progress", _loadingValue);
			if (_loadingValue <= -0.8f && _player._dead == true)
			{
				GetTree().ReloadCurrentScene(); // Reload the scene 
			}
			else if (_loadingValue <= -0.8f )
			{
				_loadingGoal = 3f;
				_loadingDone = true;
			}
		}
		GetNode<ColorRect>("BlacksmithShop/Warning").GlobalPosition = GetGlobalMousePosition();
		GetNode<Sprite2D>("Loading/Load").Rotate(-0.1f);
	}

	//--- Dialouge ---
	private void _on_accept_button_button_up() { _player.QuestAccepted(); }
	private void _on_ignore_button_button_up() { _player.QuestIgnored(); }
	private void _on_continue_button_up() { _player.ContinueDialouge(); }

	// --- Blacksmith Shop ---
	private void _on_sshop_option1_button_up()
	{
		if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 0) { PlayShopAnim("shortsword"); _sShopSelection = "Shortsword"; _player.SwitchPrimaryWeapon(_sShopSelection); }
		//else { PlayShopAnim("Longsword"); }
	}
	private void _on_sshop_option2_button_up()
	{
		if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 0) { PlayShopAnim("falchion"); _sShopSelection = "Falchion"; _player.SwitchPrimaryWeapon(_sShopSelection); }
		//else { PlayShopAnim("Greatsword"); }
		
	}
	private void _on_sshop_option3_button_up()
	{
		if (_sShopTypeSelection.Selected == 0) { PlayShopAnim(""); }
		else { PlayShopAnim(""); }
	}
	private void _on_sshop_option4_button_up()
	{
		if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 0) { PlayShopAnim("dagger"); _sShopSelection = "Dagger"; _player.SwitchPrimaryWeapon(_sShopSelection); }
		else { PlayShopAnim(""); }
	}
	private void _on_wshop_option1_button_up()
	{
		if (_wShopTypeSelection.Selected == 0) { _wShopSelection = "lHealth"; }
		if (_wShopTypeSelection.Selected == 1) { _wShopSelection = "lSpeed"; }
		if (_wShopTypeSelection.Selected == 2) { _wShopSelection = "lStrength"; }
	}
	private void _on_wshop_option2_button_up()
	{
		if (_wShopTypeSelection.Selected == 0) { _wShopSelection = "health"; }
		if (_wShopTypeSelection.Selected == 1) { _wShopSelection = "gSpeed"; }
		if (_wShopTypeSelection.Selected == 2) { _wShopSelection = "gStrength"; }
	}
	private void _on_wshop_option3_button_up()
	{
		_wShopSelection = "GHealth";
	}
	private void _on_upgrade_button_up()
	{
		//ColorRect _desc = GetNode<ColorRect>("BlacksmithShop/View/WeaponDesc"); <--- For later
		// UI references and sets
		GetNode<Control>("BlacksmithShop/View").Visible = true;
		Control _upg = GetNode<Control>("BlacksmithShop/View/UpgradeMenu");
		Label _details = _upg.GetNode<Label>("Requirements/Details");
		_upg.GetNode<Label>("UpgradePrompt").Text = "Upgrade\n" + _sShopSelection + "?";
		_upg.GetNode<Control>("Requirements").Visible = true;
		_upg.GetNode<Label>("Requirements/Details").Text = "";

		// sword and spaghetti refrences for resources
		itemList _resourceScript = (itemList)_resourceInv;
		PackedScene _swordScn = _player._weapon[_sShopSelection];
		Node3D _swordNode = _swordScn.Instantiate<Node3D>();
		int lvl = (int)_swordNode.GetMeta("level");
		string upgName;
		Godot.Collections.Array<string> resRef = _sResourceRefrences[_sShopSelection.ToLower() + (lvl + 1)].AsGodotArray<string>();
		Godot.Collections.Array<int> resAmounts = _sResourceAmounts[_sShopSelection.ToLower() + (lvl + 1)].AsGodotArray<int>();
		_itemCheck = ItemCheck(resRef, resAmounts);
		// ----- Sets the requirements UI
		_details.Text += "Requirements:\n";
		for (int i = 0; i < resRef.Count; i++)
		{
			_details.Text += _resourceNames[resRef[i]] + " (" + _resourceScript.GetItemCount(resRef[i]) + "/" + resAmounts[i] + ")\n";
		}
		_details.Text += "\nUpgrades:\n";
		foreach (string stat in GetUpgrades(_swordNode))
		{
			upgName = Json.Stringify(_upgradeNames[stat]);
			string statName = upgName.Substring(1, upgName.Length - 2);
			if ((int)_swordNode.GetMeta("level") < 3)
			{
				if (!(stat.IndexOf("Percent") >= 0))
				{
					_details.Text += statName + "\n";
				}
			}
			else
			{
				if (stat.IndexOf("Percent") >= 0)
				{
					_details.Text += statName + "\n";
				}
			}
		}
		if (!_upg.Visible)
		{
			_upg.Visible = true;
		}
		else
		{
			_upg.Visible = false;
		}
	}

	private void _on_craft_button_up()
	{
		
	}
	private void _on_upgrade_mouse_entered()
	{
		if ((int)_player._weapon[_sShopSelection].Instantiate<Node3D>().GetMeta("level") >= 3)
        {
			GetNode<Label>("BlacksmithShop/Warning/Warning").Text = "This weapon is already max level!";
			GetNode<ColorRect>("BlacksmithShop/Warning").Visible = true;
			GetNode<Button>("BlacksmithShop/Upgrade").Disabled = true;
        }
	}
	private void _on_upgrade_mouse_exited()
    {
		GetNode<ColorRect>("BlacksmithShop/Warning").Visible = false;
		GetNode<Button>("BlacksmithShop/Upgrade").Disabled = false;
    }
	private void _on_upgrade_conf_mouse_entered()
	{
		if (!_itemCheck)
        {
			GetNode<Label>("BlacksmithShop/Warning/Warning").Text = "You don't have enough resources!";
			GetNode<ColorRect>("BlacksmithShop/Warning").Visible = true;
			GetNode<Button>("BlacksmithShop/View/UpgradeMenu/Requirements/UpgradeConf").Disabled = true;
        }
	}
	private void _on_upgrade_conf_mouse_exited()
    {
		GetNode<ColorRect>("BlacksmithShop/Warning").Visible = false;
		GetNode<Button>("BlacksmithShop/View/UpgradeMenu/Requirements/UpgradeConf").Disabled = false;
    }
	private void _on_upgrade_conf_button_up()
	{
		GetNode<Label>("BlacksmithShop/View/UpgradeMenu/UpgradePrompt").Text = _sShopSelection + "\nUpgraded!";
		Control _resultsPage = GetNode<Control>("BlacksmithShop/View/UpgradeMenu/Results");
		PackedScene _swordScn = _player._weapon[_sShopSelection];
		Node3D _swordNode = _swordScn.Instantiate<Node3D>();
		itemList _resourceScript = (itemList)_resourceInv;
		_swordNode.SetMeta("level", (int)_swordNode.GetMeta("level") + 1);
		int nLvl = (int)_swordNode.GetMeta("level");
		GD.Print(nLvl);
		string upgName;
		Godot.Collections.Array<string> resRef = _sResourceRefrences[_sShopSelection.ToLower() + nLvl].AsGodotArray<string>();
		Godot.Collections.Array<int> resAmounts = _sResourceAmounts[_sShopSelection.ToLower() + nLvl].AsGodotArray<int>();

		if(ItemCheck(resRef, resAmounts))
		{
			_itemCheck = ItemCheck(resRef, resAmounts);
			for(int i = 0; i < resRef.Count; i++)
			{
				_resourceScript.SubtractResource(resRef[i], resAmounts[i]);
			}
			foreach (string stat in GetUpgrades(_swordNode))
			{
				upgName = Json.Stringify(_upgradeNames[stat]);
				string statName = upgName.Substring(1, upgName.Length - 2);
				if ((stat.Equals("cChance") || stat.Equals("bChance")) && nLvl < 4)
				{
					(string, float) metaSet = SetResults(stat, stat, statName, _swordNode);
					
				}
				if ((stat.Equals("damage") || stat.Equals("hDamage")) && nLvl < 4)
				{
					(string, float) metaSet = SetResults(stat, GetUpgrades(_swordNode)[0] + nLvl, statName, _swordNode);
					_swordNode.SetMeta(metaSet.Item1, metaSet.Item2);
				}
				if ((stat.Equals("cPercent1") || stat.Equals("cPercent2") || stat.Equals("cPercent3")) && nLvl >= 4)
				{
					(string, float) metaSet = SetResults(stat, "cPercent" + nLvl, statName, _swordNode);
					_swordNode.SetMeta(metaSet.Item1, metaSet.Item2);
				}
			}
			GetNode<Control>("BlacksmithShop/View/UpgradeMenu/Requirements").Visible = false;
			_resultsPage.Visible = true;
		}
		else
		{
			GetNode<Control>("BlacksmithShop/View/UpgradeMenu/Requirements").Visible = false;
			_resultsPage.Visible = true;
		}
		_swordScn.Pack(_swordNode);
	}
	private void _on_upgrade_deny_button_up()
	{
		GetNode<Control>("BlacksmithShop/View").Visible = false;
		GetNode<Control>("BlacksmithShop/View/UpgradeMenu").Visible = false;
		GetNode<Label>("BlacksmithShop/View/UpgradeMenu/Requirements/Details").Text = "";
	}
	private void _on_done_button_up()
	{
		GetNode<Control>("BlacksmithShop/View").Visible = false;
		Control _upg = GetNode<Control>("BlacksmithShop/View/UpgradeMenu");
		_upg.Visible = false;
		_upg.GetNode<Control>("Results").Visible = false;
		_upg.GetNode<Label>("Results/StatName").Text = "";
		_upg.GetNode<Label>("Results/Amount").Text = "";
		_upg.GetNode<Label>("Results/Addition").Text = "";
		_upg.GetNode<Label>("Requirements/Details").Text = "";
	}


	private void _on_view_button_up()
	{
		ColorRect _desc = GetNode<ColorRect>("BlacksmithShop/View/WeaponDesc");
		if (!_desc.Visible)
		{
			_desc.Visible = true;
		}
		else
		{
			_desc.Visible = false;
		}
	}
   
	// --- Falchion ---
	private void _on_falchion_mouse_entered(){ PlayInvAnim("Falchion", true); }
	private void _on_falchion_mouse_exited(){ PlayInvAnim("Falchion", false); }
	private void _on_falchion_button_up() { _player.SwitchPrimaryWeapon("Falchion"); }

	// --- Dagger ---
	private void _on_dagger_mouse_entered() { PlayInvAnim("dagger", true); }
	private void _on_dagger_mouse_exited() { PlayInvAnim("dagger", false); }
	private void _on_dagger_button_up(){ _player.SwitchPrimaryWeapon("Dagger"); }
	
	// --- ShortSword ---
	private void _on_shortsword_mouse_entered(){ PlayInvAnim("Shortsword", true); }
	private void _on_shortsword_mouse_exited(){ PlayInvAnim("Shortsword", false); }
	private void _on_shortsword_button_up() { _player.SwitchPrimaryWeapon("Shortsword"); }

	// --- Longsword ---
	private void _on_longsword_mouse_entered() { PlayInvAnim("longsword", true); }
	private void _on_longsword_mouse_exited() { PlayInvAnim("longsword", false); }
	private void _on_longsword_button_up(){ _player.SwitchPrimaryWeapon("Longsword"); }

	// --- StakeGun ---
	private void _on_stake_gun_mouse_entered() { PlayInvAnim("StakeGun", true); }
	private void _on_stake_gun_mouse_exited() { PlayInvAnim("StakeGun", false); } // son im crane
	private void _on_stake_gun_button_up()
	{
		_secItemSend = "StakeGun";
		_slotSelect.Visible = true;
	}

	// --- Flintlock ---
	private void _on_gun_mouse_entered() { PlayInvAnim("flintlock", true); }
	private void _on_gun_mouse_exited() { PlayInvAnim("flintlock", false); }
	private void _on_gun_button_up()
	{
		_secItemSend = "FlintGun";
		_slotSelect.Visible = true;
	}

	private async void Load()
	{
		await ToSignal(GetTree().CreateTimer(4), "timeout");
		_loaded = true;
	}
	public void Opened()
	{
		PlayInvAnim("Shortsword", false);
		PlayInvAnim("Falchion", false);
		PlayInvAnim("StakeGun", false);
		PlayInvAnim("flintlock", false);
		PlayInvAnim("dagger", false);
		PlayInvAnim("longsword", false);
	}

	private void PlayInvAnim(string sword, bool forwards)
	{

		if (forwards == true)
		{
			GetNode<Node3D>("Inv/SubPort/Sub/InvWorld").GetNode<Node3D>(sword).GetNode<AnimationPlayer>("HoverAnim").Play("Hover");
		}
		else
		{
			GetNode<Node3D>("Inv/SubPort/Sub/InvWorld").GetNode<Node3D>(sword).GetNode<AnimationPlayer>("HoverAnim").PlayBackwards("Hover");
		}

	}
	
	private void PlayShopAnim(string item) // Why do I exist dude ts crazy
	{
		if (_prevSelection != item) // switches from the shown weapon on the preview to the selected weapon
		{
			if (_prevSelection != null) 
			{ 
				GetNode<AnimationPlayer>("BlacksmithShop/PortContainer/Port/SmithShopPreviewWorld/" + _prevSelection + "/PreviewAnim").PlayBackwards(item.Substr(0, 3).ToLower() + "PreviewAnim");
			}
			_prevSelection = item;
			GetNode<AnimationPlayer>("BlacksmithShop/PortContainer/Port/SmithShopPreviewWorld/" + item + "/PreviewAnim").Play(item.Substr(0, 3).ToLower() + "PreviewAnim");
		}
	} 

	/*private async void _on_slot_4_button_up()
	{
		_slotSelect.Visible = false;
		GD.Print("sendslot4", _secItemSend);
		_player.SwitchSecondaryWeapon(_secItemSend,3);
		await ToSignal(GetTree().CreateTimer(0.01), "timeout");
		_secItemSend = null;
	}

	private async void _on_slot_3_button_up()
	{
		_slotSelect.Visible = false;
		GD.Print("sendslot3", _secItemSend);
		_player.SwitchSecondaryWeapon(_secItemSend,2);
		await ToSignal(GetTree().CreateTimer(0.01), "timeout");
		_secItemSend = null;
	}*/

	private async void _on_slot_2_button_up()
	{
		_slotSelect.Visible = false;
		GD.Print("sendslot2", _secItemSend);
		_player.SwitchSecondaryWeapon(_secItemSend,1);
		await ToSignal(GetTree().CreateTimer(0.01), "timeout");
		_secItemSend = null;
	}
	
	private async void _on_slot_1_button_up()
	{
		_slotSelect.Visible = false;
		GD.Print("sendslot1", _secItemSend);
		_player.SwitchSecondaryWeapon(_secItemSend,0);
		await ToSignal(GetTree().CreateTimer(0.01), "timeout");
		_secItemSend = null;
	}
	private async void _on_button_button_down() { GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest); GetTree().Quit(); }

	private List<string> GetUpgrades(Node3D weapon) // Just gets the available stats of a weapon for upgrading
	{
		string[] _allAttributes = { "damage", "hDamage", "cChance", "cPercent1", "cPercent2", "cPercent3", "bChance" };
		List<string> _weaponAttributes = new List<string>();
		foreach (string stat in _allAttributes)
		{
			if (weapon.HasMeta(stat))
			{
				_weaponAttributes.Add(stat);
			}
		}
		return _weaponAttributes;
	}

	private (string, float) SetResults(string statName, string specificStatName, string upgradeName, Node3D _swordNode)
	{
		Control _results = GetNode<Control>("BlacksmithShop/View/UpgradeMenu/Results");
		if (statName.IndexOf("Chance") >= 0 || statName.IndexOf("Percent") >= 0)
		{
			//_results.GetNode<Label>("Amount").Text += Math.Round((float)_swordScn.GetMeta(statName) * 100, 3) + "%\n";
			GD.Print(specificStatName);
			_results.GetNode<Label>("Addition").Text += "+" + Math.Round((float)_upgradeAmounts[specificStatName] * 100, 3) + "%\n";
		}
		else
		{
			//_results.GetNode<Label>("Amount").Text += Math.Round((float)_swordScn.GetMeta(statName), 3) + "\n";
			GD.Print(specificStatName);
			_results.GetNode<Label>("Addition").Text += "+" + Math.Round((float)_upgradeAmounts[specificStatName], 3) + "\n";
		}
		_results.GetNode<Label>("StatName").Text += upgradeName + ".......................................\n";
		GD.Print(upgradeName);
		//_swordNode.SetMeta(statName, Math.Round((float)_swordNode.GetMeta(statName) + (float)_upgradeAmounts[specificStatName], 3));
		return (statName, (float)Math.Round((float)_swordNode.GetMeta(statName) + (float)_upgradeAmounts[specificStatName], 3));
		//_swordScn.Pack(_swordNode);
	}

	private void _on_resource_inv_button_up()
    {
		_resourceInv.Visible = true;
		_inv.Visible = false;
    }
	private void _on_back_button_up()
	{
		_resourceInv.Visible = false;
		_inv.Visible = true;
	}
	private void _on_smithshop_back_button_up()
	{
		GetNode<Control>("BlacksmithShop").Visible = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
	private void _on_wizshop_back_button_up()
	{
		GetNode<Control>("WizardShop").Visible = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	private bool ItemCheck(Godot.Collections.Array<string> req, Godot.Collections.Array<int> amount)
	{
		itemList _resourceScript = (itemList)_resourceInv;
		int count = 0;
		// ----- Checks if you have the resources you need to upgrade it
		for(int i = 0; i < req.Count; i++)
		{
			if(_resourceScript.GetItemCount(req[i]) >= amount[i])
			{
				//GD.Print(req[i]);
				//GD.Print("amount of " + req[i] + " = " + _resourceScript.GetItemCount(req[i]));
				count++;
			}
		}
		//GD.Print("req = " + req.Count);
		//GD.Print("count = " + count);
		if(count >= req.Count) { return true; } else { return false; }
	}
}// some day i will rule the world
