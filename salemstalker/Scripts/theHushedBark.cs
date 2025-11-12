using Godot;
using System;

public partial class theHushedBark : Node3D
{
	// Called when the node enters the scene tree for the first time.
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	private Node3D _targetTree;
	private float _enemySpawnRadius = 10f;
	private Player3d _player;
	private bool _canBeHit = true;
	private Node3D _hitFX;
	private Node3D _body;
	private float _health = 100f;
	public override void _Ready()
    {
		_rng.Randomize();
		_player = this.GetParent().GetNode<Player3d>("Player_3d");
		string rngNumber = _rng.RandiRange(1, 3).ToString();
		Node3D _targetTree = GetNode<Node3D>("Tree" + rngNumber);
		_targetTree.GetNode<Node3D>("FakeTree").Visible = false;
		_targetTree.GetNode<Node3D>("BossTree").Visible = true;

		_hitFX = _targetTree.GetNode<Node3D>("HitFX");
		_body = _targetTree.GetNode<Node3D>("BossTree");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public async void Damaged(Area3D body)
    {
        if (body.IsInGroup("Weapon") && _canBeHit)
        {
			// Quick visual hit reaction
			//_hitFX.GetNode<AnimationPlayer>("AnimationPlayer").Play("idle");
			_hitFX.Visible = true;
			_body.Visible = false;
			_canBeHit = false;
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			_canBeHit = true;

			// Reduce health
			_health -= _player._damage;

			_hitFX.Visible = false;
			_body.Visible = true;
        }
    }
}
