using Godot;
using System;

public partial class ItemDropper : Node3D
{
	PackedScene item = ResourceLoader.Load<PackedScene>("res://Scenes/monst_item.tscn");
	private RandomNumberGenerator _rng = new(); 
	private Node3D _world;
	public override void _Ready()
	{
		_rng.Randomize();
		_world = GetParent<Node3D>();
	}

	public void Drop(string name, float chance, int amount, Vector3 location)
	{
		location.Y += 1.5f;
		for (int i = 0; i < amount; i++)
		{
			if (_rng.Randf() < chance)
			{
				Item _itemInst = (Item)item.Instantiate();
				AddChild(_itemInst);
				if (_itemInst is Item itemInst) { itemInst.InstantiatedItem = _itemInst; }
				_itemInst.GetNode<Sprite3D>("ItemPic").Texture = (Texture2D)GD.Load("res://Assets/UI/resources/" + name + ".png");
				_itemInst.Name = name;
				_itemInst.GlobalPosition = location;
				_itemInst.PickUpPause(_itemInst.GetNode<Area3D>("Hitbox"));
				float _randHoriz = _rng.RandfRange(-1f, 1f);
				_itemInst._dropDirection = new Vector3(_randHoriz, 1.5f, _randHoriz);
				_itemInst.Velocity = _itemInst._dropDirection;
				GD.Print("dropped at " + _itemInst.GlobalPosition);
				//_itemInst.GetNode<Area3D>("Hitbox").Visible = false;
			}
		}
	}
}
