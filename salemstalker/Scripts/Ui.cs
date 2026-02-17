using Godot;
using Microsoft.VisualBasic;
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

public partial class Ui : Control
{
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
	private float _loadingValue = -1f;
	public float _loadingGoal = 3f;
	public bool _loadingDone = false;
	public string _loadingObjective = "None";
	public override void _Ready()
	{
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
		PlayShopAnim("falchion");
		PlayShopAnim("shortsword");
		Load();
	}

	public override void _Process(double delta)
	{
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
			_shopOption4.Visible = true;
		}
		else if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 1)
		{
			GetNode<Label>("BlacksmithShop/ShopOption1/Label").Text = "Longsword";
			GetNode<Label>("BlacksmithShop/ShopOption2/Label").Text = "Greatsword";
			GetNode<Label>("BlacksmithShop/ShopOption3/Label").Text = "Battle Axe";
			GetNode<Label>("BlacksmithShop/ShopOption4/Label").Text = "Halberd";
			_shopOption4.Visible = true;
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
		//GD.Print((float)_player._weapon[_shopSelection].GetMeta("damage"));
		GetNode<ColorRect>("BlacksmithShop/View/Warning").GlobalPosition = GetGlobalMousePosition();
	}

	//--- Dialouge ---
	private void _on_accept_button_button_up() { _player.QuestAccepted(); }
	private void _on_ignore_button_button_up() { _player.QuestIgnored(); }
	private void _on_continue_button_up() { _player.ContinueDialouge(); }

	// --- Blacksmith Shop ---
	private void _on_shop_option1_button_up()
	{
		if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 0) { PlayShopAnim("shortsword"); _shopSelection = "Shortsword"; _player.SwitchPrimaryWeapon(_shopSelection); }
		/*if (_shopTypeSelection.Selected == 1) { PlayShopAnim("Longsword"); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim("Flintlock"); }
		if (_shopTypeSelection.Selected == 3) { PlayShopAnim("Shield"); }*/
	}
	private void _on_shop_option2_button_up()
	{
		if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 0) { PlayShopAnim("falchion"); _shopSelection = "Falchion"; _player.SwitchPrimaryWeapon(_shopSelection); }
		/*if (_shopTypeSelection.Selected == 1) { PlayShopAnim("Greatsword"); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim("Stake Launcher"); }
		if (_shopTypeSelection.Selected == 3) { PlayShopAnim("Chain Hook"); }*/ // We only have the first 2 one-handed weapons so they're the only ones that aren't commented out
		
	}
	/*private void _on_shop_option3_button_up() // We might not have the ability to upgrade secondary weapons so we might not have any _shopOption2 or 3
	{
		if (_shopTypeSelection.Selected == 0) { PlayShopAnim(""); }
		if (_shopTypeSelection.Selected == 1) { PlayShopAnim(""); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim(""); }
	}
	private void _on_shop_option4_button_up()
	{
		if (_shopTypeSelection.Selected == 0) { PlayShopAnim(""); }
		if (_shopTypeSelection.Selected == 1) { PlayShopAnim(""); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim(""); }
	}*/
	private void _on_upgrade_button_up()
	{
		//ColorRect _desc = GetNode<ColorRect>("BlacksmithShop/View/WeaponDesc"); <--- For later
		Control _upg = GetNode<Control>("BlacksmithShop/View/UpgradeMenu");
		_upg.GetNode<Label>("Requirements/Details").Text = "Upgrades:\n";
		_upg.GetNode<Label>("UpgradePrompt").Text = "Upgrade\n" + _shopSelection + "?";
		_upg.GetNode<Control>("Requirements").Visible = true;
		foreach (string stat in GetUpgrades((Node3D)_player._weapon[_shopSelection].Instantiate()))
		{
			if ((int)_player._weapon[_shopSelection].GetMeta("level") < 3)
			{
				if (!(stat.IndexOf("Percent") >= 0))
				{
					_upg.GetNode<Label>("Requirements/Details").Text += _upgradeNames[stat] + "\n";
				}
			}
			else
			{
				if (stat.IndexOf("Percent") >= 0)
				{
					_upg.GetNode<Label>("Requirements/Details").Text += _upgradeNames[stat] + "\n";
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
			GetNode<ColorRect>("BlacksmithShop/View/Warning").Visible = true;
			GetNode<Button>("BlacksmithShop/View/Upgrade").Disabled = true;
        }
	}
	private void _on_upgrade_mouse_exited()
    {
		GetNode<ColorRect>("BlacksmithShop/View/Warning").Visible = false;
		GetNode<Button>("BlacksmithShop/View/Upgrade").Disabled = false;
    }
	private void _on_upgrade_conf_button_up()
	{
		GetNode<Label>("BlacksmithShop/View/UpgradeMenu/UpgradePrompt").Text = _shopSelection + "\nUpgraded!";
		Node3D swordInst = (Node3D)_player._weapon[_shopSelection].Instantiate();

		_player._weapon[_shopSelection].SetMeta("level", (int)_player._weapon[_shopSelection].GetMeta("level") + 1);
		foreach (string stat in GetUpgrades(swordInst))
		{
			if ((stat.Equals("cChance") || stat.Equals("bChance")) && (int)_player._weapon[_shopSelection].GetMeta("level") < 4)
			{
				SetResults(stat, stat, _upgradeNames[stat]);
			}
			if ((stat.Equals("damage") || stat.Equals("hDamage")) && (int)_player._weapon[_shopSelection].GetMeta("level") < 4)
			{
				SetResults(stat, GetUpgrades(swordInst)[0] + (int)_player._weapon[_shopSelection].GetMeta("level"), _upgradeNames[stat]);
			}
			if ((stat.Equals("cPercent1") || stat.Equals("cPercent2") || stat.Equals("cPercent3")) && (int)_player._weapon[_shopSelection].GetMeta("level") >= 4)
            {
				SetResults(stat, "cPercent", _upgradeNames[stat]);
            }
		}
		GD.Print((float)_player._weapon[_shopSelection].GetMeta("damage"));
		GetNode<Control>("BlacksmithShop/View/UpgradeMenu/Requirements").Visible = false;
		GetNode<Control>("BlacksmithShop/View/UpgradeMenu/Results").Visible = true;
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
	private void _on_dagger_button_up(){ _player.SwitchPrimaryWeapon("dagger"); }
	
	// --- ShortSword ---
	private void _on_shortsword_mouse_entered(){ PlayInvAnim("Shortsword", true); }
	private void _on_shortsword_mouse_exited(){ PlayInvAnim("Shortsword", false); }
	private void _on_shortsword_button_up() { _player.SwitchPrimaryWeapon("Shortsword"); }

	// --- Longsword ---
	private void _on_longsword_mouse_entered() { PlayInvAnim("longsword", true); }
	private void _on_longsword_mouse_exited() { PlayInvAnim("longsword", false); }
	private void _on_longsword_button_up(){ _player.SwitchPrimaryWeapon("longsword"); }

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
		await ToSignal(GetTree().CreateTimer(1), "timeout");
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
				GetNode<Node3D>("BlacksmithShop/View/PortContainer/Port/ShopPreviewWorld").GetNode<Node3D>(_prevSelection).GetNode<AnimationPlayer>("PreviewAnim").PlayBackwards("PreviewAnim");
			}
			_prevSelection = item;
			GetNode<Node3D>("BlacksmithShop/View/PortContainer/Port/ShopPreviewWorld").GetNode<Node3D>(item).GetNode<AnimationPlayer>("PreviewAnim").Play("PreviewAnim");
		}
	}

	private async void _on_slot_4_button_up()
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
	}

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
		if (statName.IndexOf("Chance") >= 0 || statName.IndexOf("Percent") >= 0)
		{
			_results.GetNode<Label>("Amount").Text += Math.Round((float)_player._weapon[_shopSelection].GetMeta(statName) * 100, 3) + "%\n";
			_results.GetNode<Label>("Addition").Text += "+" + Math.Round(_upgrades[specificStatName] * 100, 3) + "%\n";
		}
		else
		{
			_results.GetNode<Label>("Amount").Text += Math.Round((float)_player._weapon[_shopSelection].GetMeta(statName), 3) + "\n";
			_results.GetNode<Label>("Addition").Text += "+" + Math.Round(_upgrades[specificStatName], 3) + "\n";
		}
		_results.GetNode<Label>("StatName").Text += upgradeName + ".......................................\n";
		_player._weapon[_shopSelection].SetMeta(statName, Math.Round((float)_player._weapon[_shopSelection].GetMeta(statName) + _upgrades[specificStatName], 3));
	}
}
