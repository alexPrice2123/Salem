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
	private OptionButton _shopTypeSelection;
	private int _typeSelection;
	private TextureButton _shopOption1;
	private TextureButton _shopOption2;
	private TextureButton _shopOption3;
	private TextureButton _shopOption4;
	private string _shopSelection = "Shortsword";
	private Label _areaName;
	public float _areaNameTween = 0f;
	private Dictionary<string, float> _upgrades = new Dictionary<string, float>();
	private Dictionary<string, string> _specialAttacks = new Dictionary<string, string>();
	private Dictionary<string, string> _upgradeNames = new Dictionary<string, string>();
	private Dictionary<string, string[]> _requirementRef = new Dictionary<string, string[]>();
	private Dictionary<string, int[]> _amountRef = new Dictionary<string, int[]>();
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
	private bool _upgPossible = false;

	// ---------------- MMM SPAGHETTI
	// shortsword
	public static string[] _shortswordUpg1Req = { "bleedheart", "deadooze" }; public static int[] _shortswordUpg1Amount = { 3, 3 };
    public static string[] _shortswordUpg2Req = { "deadooze", "fang", "scorchedflesh" }; public static int[] _shortswordUpg2Amount = { 6, 3, 3 };

	// dagger
	public static string[] _daggerUpg1Req = { "emptyves", "seed" }; public static int[] _daggerUpg1Amount = { 3, 3 };
    public static string[] _daggerUpg2Req = { "seed", "scorchedflesh", "heart" }; public static int[] _daggerUpg2Amount = { 6, 3, 3 };

	// longsword
	public static string[] _longswordUpg1Req = { "deadooze", "chippedfang" }; public static int[] _longswordUpg1Amount = { 5, 5 };
    public static string[] _longswordUpg2Req = { "chippedfang", "sprout", "heart" }; public static int[] _longsword2Amount = { 8, 5, 5 };

	public override void _Ready()
	{
		Instance = this;
		if (GetParent() is Player3d player)
		{
			_player = player;
		}
		_slotSelect = GetNode<Control>("Inv/SubPort/Sub/SlotSelector");
		_loadingUI = GetNode<Control>("Loading");
		_shopTypeSelection = GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions");
		_shopOption1 = GetNode<TextureButton>("BlacksmithShop/ShopOption1");
		_shopOption2 = GetNode<TextureButton>("BlacksmithShop/ShopOption2");
		_shopOption3 = GetNode<TextureButton>("BlacksmithShop/ShopOption3");
		_shopOption4 = GetNode<TextureButton>("BlacksmithShop/ShopOption4");
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
		_upgrades.Add("damage1", 2);
		_upgrades.Add("damage2", 4.5f);
		_upgrades.Add("damage3", 7.5f);
		_upgrades.Add("cChance", 0.02f);
		_upgrades.Add("bChance", 0.015f);
		_upgrades.Add("cPercent", 0.04f);
		_upgradeNames.Add("damage", "Damage");
		_upgradeNames.Add("hDamage", "Strong Damage");
		_upgradeNames.Add("cChance", "Critical Chance");
		_upgradeNames.Add("bChance", "Bleed Chance");
		_upgradeNames.Add("cPercent1", "Critical Damage");
		_upgradeNames.Add("cPercent2", "Special Critical Damage");
		_upgradeNames.Add("cPercent3", "Heavy Critical Damage");
		_specialAttacks.Add("Shortsword", "Pommel Strike");
		//_specialAttacks.Add("Flail", "");

		_requirementRef.Add("shortsword1", _shortswordUpg1Req);
		_requirementRef.Add("shortsword2", _shortswordUpg1Req);
		_requirementRef.Add("dagger1", _daggerUpg1Req);
		_requirementRef.Add("dagger2", _daggerUpg1Req);
		_requirementRef.Add("longsword1", _longswordUpg1Req);
		_requirementRef.Add("longsword2", _longswordUpg1Req);

		_amountRef.Add("shortsword1", _shortswordUpg1Amount);
		_amountRef.Add("shortsword2", _shortswordUpg1Amount);
		_amountRef.Add("dagger1", _daggerUpg1Amount);
		_amountRef.Add("dagger2", _daggerUpg1Amount);
		_amountRef.Add("longsword1", _longswordUpg1Amount);
		_amountRef.Add("longsword2", _longswordUpg1Amount);

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
			//_shopOption4.Visible = true;
			_shopOption1.Visible = true;
		}
		else if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 1)
		{
			GetNode<Label>("BlacksmithShop/ShopOption1/Label").Text = "Longsword";
			GetNode<Label>("BlacksmithShop/ShopOption2/Label").Text = "Greatsword";
			GetNode<Label>("BlacksmithShop/ShopOption3/Label").Text = "Battle Axe";
			GetNode<Label>("BlacksmithShop/ShopOption4/Label").Text = "Halberd";
			//_shopOption4.Visible = true;
			_shopOption1.Visible = false;
		}
		else if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 2)
		{
			GetNode<Label>("BlacksmithShop/ShopOption1/Label").Text = "Flintlock";
			GetNode<Label>("BlacksmithShop/ShopOption2/Label").Text = "Stake Launcher";
			GetNode<Label>("BlacksmithShop/ShopOption3/Label").Text = "Throwables";
			_shopOption4.Visible = false;
		}
		else
		{
			GetNode<Label>("BlacksmithShop/ShopOption1").Text = "Shield";
			GetNode<Label>("BlacksmithShop/ShopOption2").Text = "Chain Hook";
			GetNode<Label>("BlacksmithShop/ShopOption3").Text = "Holy Relic";
			_shopOption4.Visible = false;
		}
		if (_loaded == true) //Waits 1 second for the game to load before the ui tweens
		{
			_loadingValue = Mathf.Lerp(_loadingValue, _loadingGoal, (float)delta);
			_loadingMaterial.SetShaderParameter("progress", _loadingValue);
			if (_loadingValue <= -0.8f && _player._dead == true && _loadingUI.GetNode<Label>("Text").Text == "Loading...")
			{
				GetTree().ReloadCurrentScene(); // Reload the scene 
			}
			else if (_loadingValue <= -0.8f && _loadingUI.GetNode<Label>("Text").Text == "Loading...")
			{
				_loadingGoal = 3f;
				_loadingDone = true;
			}
		}
		GetNode<ColorRect>("BlacksmithShop/View/Warning").GlobalPosition = GetGlobalMousePosition();
		GetNode<Sprite2D>("Loading/Load").Rotate(-0.1f);
	}

	//--- Dialouge ---
	private void _on_accept_button_button_up() { _player.QuestAccepted(); }
	private void _on_ignore_button_button_up() { _player.QuestIgnored(); }
	private void _on_continue_button_up() { _player.ContinueDialouge(); }

	// --- Blacksmith Shop ---
	private void _on_shop_option1_button_up()
	{
		if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 0) 
		{ PlayShopAnim("shortsword"); _shopSelection = "Shortsword"; _player.SwitchPrimaryWeapon(_shopSelection); }
		/*if (_shopTypeSelection.Selected == 1) { PlayShopAnim("Longsword"); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim("Flintlock"); }
		if (_shopTypeSelection.Selected == 3) { PlayShopAnim("Shield"); }*/
	}
	private void _on_shop_option2_button_up()
	{
		//if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 0) 
		//{ PlayShopAnim("falchion"); _shopSelection = "Falchion"; _player.SwitchPrimaryWeapon(_shopSelection); }
		/*if (_shopTypeSelection.Selected == 1) { PlayShopAnim("Greatsword"); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim("Stake Launcher"); }
		if (_shopTypeSelection.Selected == 3) { PlayShopAnim("Chain Hook"); }*/ // We only have the first 2 one-handed weapons so they're the only ones that aren't commented out
		
	}
	/*private void _on_shop_option3_button_up() // We might not have the ability to upgrade secondary weapons so we might not have any _shopOption2 or 3
	{
		if (_shopTypeSelection.Selected == 0) { PlayShopAnim(""); }
		if (_shopTypeSelection.Selected == 1) { PlayShopAnim(""); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim(""); }
	}*/
	private void _on_shop_option4_button_up()
	{
		//if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 0) 
		//{ PlayShopAnim("dagger"); _shopSelection = "Dagger"; _player.SwitchPrimaryWeapon(_shopSelection); }
		//if (_shopTypeSelection.Selected == 1) { PlayShopAnim(""); }
		//if (_shopTypeSelection.Selected == 2) { PlayShopAnim(""); }
	}
	private void _on_upgrade_button_up()
	{
		//ColorRect _desc = GetNode<ColorRect>("BlacksmithShop/View/WeaponDesc"); <--- For later
		// UI references and sets
		Control _upg = GetNode<Control>("BlacksmithShop/View/UpgradeMenu");
		Label _details = _upg.GetNode<Label>("Requirements/Details");
		_upg.GetNode<Label>("UpgradePrompt").Text = "Upgrade\n" + _shopSelection + "?";
		_upg.GetNode<Control>("Requirements").Visible = true;

		// sword and spaghetti refrences for resources
		PackedScene _swordScn = _player._weapon[_shopSelection];
		itemList _resourceScript = (itemList)_resourceInv;
		int count = 0;
		GD.Print(_shopSelection);
		string[] _requirements = _requirementRef[_shopSelection.ToLower() + ((int)_swordScn.GetMeta("level") + 1)];
		int[] _amount = _amountRef[_shopSelection.ToLower() + ((int)_swordScn.GetMeta("level") + 1)];
		GD.Print(_requirements);
		GD.Print("length = " + _requirements.Length);

		// ----- Checks if you have the resources you need to upgrade it
		for(int i = 0; i < _requirements.Length; i++)
		{
			if(_resourceScript.GetItemCount(_requirements[i]) >= _amount[i])
			{
				GD.Print(_requirements[i]);
				GD.Print("amount of " + _requirements[i] + " = " + _resourceScript.GetItemCount(_requirements[i]));
				count++;
			}
		}
		GD.Print("count = " + count);
		if(count >= _requirements.Length) { _upgPossible = true; } else { _upgPossible = false; }

		// ----- Sets the requirements UI
		_details.Text += "Requirements:\n";
		for (int i = 0; i < _requirements.Length; i++)
		{
			_details.Text += _requirements[i] + " (" + _resourceScript.GetItemCount(_requirements[i]) + "/" + _amount[i] + ")\n";
		}
		_details.Text += "\nUpgrades:\n";
		foreach (string stat in GetUpgrades((Node3D)_player._weapon[_shopSelection].Instantiate()))
		{
			if ((int)_player._weapon[_shopSelection].GetMeta("level") < 3)
			{
				if (!(stat.IndexOf("Percent") >= 0))
				{
					_details.Text += _upgradeNames[stat] + "\n";
				}
			}
			else
			{
				if (stat.IndexOf("Percent") >= 0)
				{
					_details.Text += _upgradeNames[stat] + "\n";
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
	private void _on_upgrade_mouse_entered()
	{
		if ((int)_player._weapon[_shopSelection].GetMeta("level") >= 4)
        {
			GetNode<Label>("BlacksmithShop/View/Warning/Warning").Text = "This weapon is already max level!";
			GetNode<ColorRect>("BlacksmithShop/View/Warning").Visible = true;
			GetNode<Button>("BlacksmithShop/View/Upgrade").Disabled = true;
        }
	}
	private void _on_upgrade_mouse_exited()
    {
		GetNode<ColorRect>("BlacksmithShop/View/Warning").Visible = false;
		GetNode<Button>("BlacksmithShop/View/Upgrade").Disabled = false;
    }
	private void _on_upgrade_conf_mouse_entered()
	{
		if (!_upgPossible)
        {
			GetNode<Label>("BlacksmithShop/View/Warning/Warning").Text = "You don't have enough resources!";
			GetNode<ColorRect>("BlacksmithShop/View/Warning").Visible = true;
			GetNode<Button>("BlacksmithShop/View/UpgradeMenu/Requirements/UpgradeConf").Disabled = true;
        }
	}
	private void _on_upgrade_conf_mouse_exited()
    {
		GetNode<ColorRect>("BlacksmithShop/View/Warning").Visible = false;
		GetNode<Button>("BlacksmithShop/View/UpgradeMenu/Requirements/UpgradeConf").Disabled = false;
    }
	private void _on_upgrade_conf_button_up()
	{
		GetNode<Label>("BlacksmithShop/View/UpgradeMenu/UpgradePrompt").Text = _shopSelection + "\nUpgraded!";
		Control _resultsPage = GetNode<Control>("BlacksmithShop/View/UpgradeMenu/Results");
		PackedScene _swordScn = _player._weapon[_shopSelection];
		Node3D _swordInst = (Node3D)_swordScn.Instantiate();
		itemList _resourceScript = (itemList)_resourceInv;

		GD.Print(_shopSelection);
		_swordScn.SetMeta("level", (int)_swordScn.GetMeta("level") + 1);
		string[] _requirements = _requirementRef[_shopSelection.ToLower() + (int)_swordScn.GetMeta("level")];
		int[] _amount = _amountRef[_shopSelection.ToLower() + (int)_swordScn.GetMeta("level")];

		if(_upgPossible)
		{
			for(int i = 0; i < _requirements.Length; i++)
			{
				_resourceScript.SubtractResource(_requirements[i], _amount[i]);
			}
			foreach (string stat in GetUpgrades(_swordInst))
			{
				if ((stat.Equals("cChance") || stat.Equals("bChance")) && (int)_swordScn.GetMeta("level") < 4)
				{
					SetResults(stat, stat, _upgradeNames[stat]);
				}
				if ((stat.Equals("damage") || stat.Equals("hDamage")) && (int)_swordScn.GetMeta("level") < 4)
				{
					SetResults(stat, GetUpgrades(_swordInst)[0] + (int)_swordScn.GetMeta("level"), _upgradeNames[stat]);
				}
				if ((stat.Equals("cPercent1") || stat.Equals("cPercent2") || stat.Equals("cPercent3")) && (int)_swordScn.GetMeta("level") >= 4)
				{
					SetResults(stat, "cPercent", _upgradeNames[stat]);
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
	}
	private void _on_upgrade_deny_button_up()
	{
		GetNode<Control>("BlacksmithShop/View/UpgradeMenu").Visible = false;
	}
	private void _on_done_button_up()
	{
		Control _upg = GetNode<Control>("BlacksmithShop/View/UpgradeMenu");
		_upg.Visible = false;
		_upg.GetNode<Control>("Results").Visible = false;
		_upg.GetNode<Label>("Results/StatName").Text = "";
		_upg.GetNode<Label>("Results/Amount").Text = "";
		_upg.GetNode<Label>("Results/Addition").Text = "";
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
	private void _on_stake_gun_mouse_exited() { PlayInvAnim("StakeGun", false); }
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
	
	private void PlayShopAnim(string item)
	{
		if (_prevSelection != item) // switches from the shown weapon on the preview to the selected weapon
		{
			if (_prevSelection != null) 
			{ 
				GetNode<Node3D>("BlacksmithShop/View/PortContainer/Port/ShopPreviewWorld").GetNode<Node3D>(_prevSelection).GetNode<AnimationPlayer>("PreviewAnim").PlayBackwards(item.Substr(0, 3).ToLower() + "PreviewAnim");
			}
			_prevSelection = item;
			GetNode<Node3D>("BlacksmithShop/View/PortContainer/Port/ShopPreviewWorld").GetNode<Node3D>(item).GetNode<AnimationPlayer>("PreviewAnim").Play(item.Substr(0, 3).ToLower() + "PreviewAnim");
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

	private void SetResults(string statName, string specificStatName, string upgradeName)
	{
		Control _results = GetNode<Control>("BlacksmithShop/View/UpgradeMenu/Results");
		PackedScene _swordScn = _player._weapon[_shopSelection];
		if (statName.IndexOf("Chance") >= 0 || statName.IndexOf("Percent") >= 0)
		{
			_results.GetNode<Label>("Amount").Text += Math.Round((float)_swordScn.GetMeta(statName) * 100, 3) + "%\n";
			_results.GetNode<Label>("Addition").Text += "+" + Math.Round(_upgrades[specificStatName] * 100, 3) + "%\n";
		}
		else
		{
			_results.GetNode<Label>("Amount").Text += Math.Round((float)_swordScn.GetMeta(statName), 3) + "\n";
			_results.GetNode<Label>("Addition").Text += "+" + Math.Round(_upgrades[specificStatName], 3) + "\n";
		}
		_results.GetNode<Label>("StatName").Text += upgradeName + ".......................................\n";
		_swordScn.SetMeta(statName, Math.Round((float)_swordScn.GetMeta(statName) + _upgrades[specificStatName], 3));
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
}
