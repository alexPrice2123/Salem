using Godot;
using System;

public partial class Item : CharacterBody3D
{
	private string _name = "";
	private int _maxDrop = 0;
	private float _dropChance = 0;
	private Texture2D _image = (Texture2D)GD.Load("res://Assets/2D/eyeicon.png"); // the file directory for the item's image
	public bool _stored = false; // track weather its in an inventory or on the ground
	private Node3D _holder;
	public Item (string name, int max, float chance)
	{
		_name = name;
		_maxDrop = max;
		_dropChance = chance;
		//_image = (Texture2D)GD.Load("res://Assets/2D/" + name + ".png");
	}
	public Item (string name, float chance)
	{
		_name = name;
		_maxDrop = 1;
		_dropChance = chance;
		//_image = (Texture2D)GD.Load("res://Assets/2D/" + name + ".png");
	}

	public override void _Ready()
	{
		_holder = GetNode<Node3D>("ItemHolder");
	}
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		if (!IsOnFloor()) { velocity.Y -= 6f * (float)delta; }
		Velocity = velocity;
		MoveAndSlide();
	}
	private void _on_hitbox_body_entered(Node3D body) // This method essentially works as a PickUpItem method
	{
		if (body is Player3d player)
		{
			GD.Print("collided with " + player.Name);
			GD.Print(Name);
			for (int i = 1; i < 25; i++)
        	{
				if ((bool)Ui.Instance._resourceInv.GetNode("InvSlot" + i + "/InvTexture" + i).GetMeta("occupied"))
				{
					
				}
        	}
			QueueFree();
		}
	}

	public void Drop()
	{
		GetNode<Sprite3D>("ItemPic").Texture = _image;
		PackedScene item = ResourceLoader.Load<PackedScene>("res://Scenes/item.tscn");
		CharacterBody3D _item = item.Instantiate<CharacterBody3D>();
		
	}
}
