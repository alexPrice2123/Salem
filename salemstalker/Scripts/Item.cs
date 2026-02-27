using Godot;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

public partial class Item : CharacterBody3D
{
	public static Item Instance { get; private set; }
	private RandomNumberGenerator _rng = new(); 
	private Vector3 _dropDirection = new Vector3();
	//private Node3D _holder;

	public override void _Ready()
	{
		Instance = this;
		//PackedScene world = ResourceLoader.Load<PackedScene>("res://Scenes/newWorld.tscn");
		//Node3D _world = (Node3D)world.Instantiate();
		_rng.Randomize();
		//_holder = GetParent().GetNode<Node3D>("World").GetNode<Node3D>("MonstItemHolder");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		if (!IsOnFloor()) { velocity.Y -= 6f * (float)delta; }
		else { Velocity = new Vector3(0, 0, 0); }
		Velocity = velocity;
		MoveAndSlide();
	}
	private void _on_hitbox_body_entered(Node3D body) // This method essentially works as a PickUpItem method
	{
		if (body is Player3d player)
		{
			GD.Print("picked up at " + player.GlobalPosition);

			//_holder.GetNode<CharacterBody3D>().QueueFree();
		}
	}

	private async void PickUpPause(Area3D detbox)
	{
		detbox.Visible = false;
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		detbox.Visible = true;
	}

	public void Drop(string name, float chance, int amount, Vector3 location)
	{
		location.Y += 1;
		for (int i = 0; i < amount; i++)
		{
			if (_rng.Randf() < chance)
			{
				PackedScene item = ResourceLoader.Load<PackedScene>("res://Scenes/monst_item.tscn");
				CharacterBody3D _item = item.Instantiate<CharacterBody3D>();
				_item.GetNode<Sprite3D>("ItemPic").Texture = (Texture2D)GD.Load("res://Assets/UI/resources/" + name + ".png");
				_item.Name = name;
				GetParent().GetNode<Node3D>("World").AddChild(_item);
				_item.GlobalPosition = location;
				PickUpPause(_item.GetNode<Area3D>("Hitbox"));
				int _randHoriz = _rng.RandiRange(-10, 10);
				_dropDirection = new Vector3(_randHoriz, _rng.RandiRange(3, 6), _randHoriz);
				Velocity = _dropDirection;
				GD.Print("dropped at " + _item.GlobalPosition);
				_item.GetNode<Area3D>("Hitbox").Visible = false;
			}
		}
	}
}
