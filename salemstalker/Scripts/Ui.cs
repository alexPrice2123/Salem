using Godot;
using Microsoft.VisualBasic;
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

public partial class Ui : Control
{
	private Player3d _player;
	private string _hovering = "falchionHover";
	private string _prevSelection;
	private string _secItemSend = null;
	private bool _loaded = false;
	private Control _loadingUI;
	private ShaderMaterial _loadingMaterial;
	private Control _slotSelect;
	private OptionButton _shopTypeSelection;
	private int _typeSelection;
	private Button _shopOption1;
	private Button _shopOption2;
	private Button _shopOption3;
	private Button _shopOption4;
	private string _shopSelection = "Shortsword";
	private Label _areaName;
	public float _areaNameTween = 0f;
	private Dictionary<string, float> _upgrades = new Dictionary<string, float>();
	private Dictionary<string, string> _specialAttacks = new Dictionary<string, string>();
	private Dictionary<string, int> _levels = new Dictionary<string, int>();
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
		_shopOption1 = GetNode<Button>("BlacksmithShop/ShopOption1");
		_shopOption2 = GetNode<Button>("BlacksmithShop/ShopOption2");
		_shopOption3 = GetNode<Button>("BlacksmithShop/ShopOption3");
		_shopOption4 = GetNode<Button>("BlacksmithShop/ShopOption4");
		_loadingUI.Visible = true;
		_loadingMaterial = _loadingUI.Material as ShaderMaterial;
		_areaName = GetNode<Label>("Area");
		_upgrades.Add("damage1", 2);
		_upgrades.Add("damage2", 4.5f);
		_upgrades.Add("damage3", 7.5f);
		_upgrades.Add("cChance", 0.02f);
		_upgrades.Add("bChance", 0.015f);
		_specialAttacks.Add("Shortsword", "Pommel Strike");
		//_specialAttacks.Add("Flail", "");
		_levels.Add("Shortsword", 1);
		_levels.Add("Falchion", 1);
		PlayShopAnim("falchion");
		PlayShopAnim("shortsword");
		Load();

		_player._sword.SetMeta("damage", 20.0);
		GD.Print(_player._sword.GetMeta("damage"));
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
			GetNode<Button>("BlacksmithShop/ShopOption1").Text = "Shortsword";
			GetNode<Button>("BlacksmithShop/ShopOption2").Text = "Falchion";
			GetNode<Button>("BlacksmithShop/ShopOption3").Text = "Rapier";
			GetNode<Button>("BlacksmithShop/ShopOption4").Text = "Dagger";
			GetNode<Button>("BlacksmithShop/ShopOption4").Visible = true;
		}
		else if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 1)
		{
			GetNode<Button>("BlacksmithShop/ShopOption1").Text = "Longsword";
			GetNode<Button>("BlacksmithShop/ShopOption2").Text = "Greatsword";
			GetNode<Button>("BlacksmithShop/ShopOption3").Text = "Battle Axe";
			GetNode<Button>("BlacksmithShop/ShopOption4").Text = "Halberd";
			GetNode<Button>("BlacksmithShop/ShopOption4").Visible = true;
		}
		else if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 2)
		{
			GetNode<Button>("BlacksmithShop/ShopOption1").Text = "Flintlock";
			GetNode<Button>("BlacksmithShop/ShopOption2").Text = "Stake Launcher";
			GetNode<Button>("BlacksmithShop/ShopOption3").Text = "Throwables";
			GetNode<Button>("BlacksmithShop/ShopOption4").Visible = false;
		}
		else
		{
			GetNode<Button>("BlacksmithShop/ShopOption1").Text = "Shield";
			GetNode<Button>("BlacksmithShop/ShopOption2").Text = "Chain Hook";
			GetNode<Button>("BlacksmithShop/ShopOption3").Text = "Holy Relic";
			GetNode<Button>("BlacksmithShop/ShopOption4").Visible = false;
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
	}

	//--- Dialouge ---
	private void _on_accept_button_button_up() { _player.QuestAccepted(); }
	private void _on_ignore_button_button_up() { _player.QuestIgnored(); }
	private void _on_continue_button_up() { _player.ContinueDialouge(); }

	// --- Blacksmith Shop ---
	private void _on_shop_option1_button_up()
	{
		if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 0) { PlayShopAnim("shortsword"); _shopSelection = "Shortsword";}
		/*if (_shopTypeSelection.Selected == 1) { PlayShopAnim("Longsword"); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim("Flintlock"); }
		if (_shopTypeSelection.Selected == 3) { PlayShopAnim("Shield"); }*/
	}
	private void _on_shop_option2_button_up()
	{
		if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 0) { PlayShopAnim("falchion"); _shopSelection = "Falchion";}
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
		ColorRect _desc = GetNode<ColorRect>("BlacksmithShop/View/WeaponDesc");
		Control _upg = GetNode<Control>("BlacksmithShop/View/UpgradeMenu");
		_upg.GetNode<Label>("UpgradePrompt").Text = "Upgrade\n" + _shopSelection + "?";
		_upg.GetNode<Control>("Requirements").Visible = true;
		if(!_upg.Visible)
		{
			_upg.Visible = true;
		}
		else
		{
			_upg.Visible = false;
		}
	}
	private void _on_upgrade_conf_button_up()
	{
        Control _results = GetNode<Control>("BlacksmithShop/View/UpgradeMenu/Results");
        GetNode<Label>("BlacksmithShop/View/UpgradeMenu/UpgradePrompt").Text = _shopSelection + "\nUpgraded!";
        PackedScene weaponScene = _player._weapon[_shopSelection];
		Node3D _swordInstance = weaponScene.Instantiate<Node3D>();
		_swordInstance.SetMeta("level", _levels[_shopSelection]);
		foreach (string stat in GetUpgrades(_swordInstance))
		{
			if (stat.Equals("cChance") || stat.Equals("bChance"))
			{
				_results.GetNode<Label>("StatName").Text += stat + "\n";
				_results.GetNode<Label>("Amount").Text += (float)_swordInstance.GetMeta(stat) + "\n";
				_swordInstance.SetMeta(stat, (float)_swordInstance.GetMeta(stat) + _upgrades[stat]);
				_results.GetNode<Label>("Addition").Text += "+" + _upgrades[stat] + "\n";
			}
			if (stat.Equals("damage") || stat.Equals("hDamage"))
			{
				_results.GetNode<Label>("StatName").Text += stat + "\n";
				_results.GetNode<Label>("Amount").Text += (float)_swordInstance.GetMeta(stat) + "\n";
				_swordInstance.SetMeta(stat, (float)_swordInstance.GetMeta(stat) + _upgrades[GetUpgrades(_swordInstance)[0] + (int)_swordInstance.GetMeta("level")]);
				_results.GetNode<Label>("Addition").Text += "+" + _upgrades[GetUpgrades(_swordInstance)[0] + (int)_swordInstance.GetMeta("level")] + "\n";
			}
		}
		GD.Print(_player._sword.GetMeta("damage"));
		GD.Print(_swordInstance.GetMeta("hDamage"));
		GD.Print(_swordInstance.GetMeta("cChance"));
		GD.Print(_swordInstance.GetMeta("bChance"));
		GD.Print((int)_swordInstance.GetMeta("level"));
		_player.SwitchPrimaryWeapon(_shopSelection);
		GetNode<Control>("BlacksmithShop/View/UpgradeMenu/Requirements").Visible = false;
		_results.Visible = true;
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
       foreach(string stat in _allAttributes)
       {
           if(weapon.HasMeta(stat))
           {
               _weaponAttributes.Add(stat);
           }
       }
       return _weaponAttributes;
   }
}
