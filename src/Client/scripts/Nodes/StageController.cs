using Godot;
using YojigenShift.Po5.Scripts.Bridges;
using YojigenShift.Po5.Scripts.Managers;
using YojigenShift.Po5.Scripts.UI;

namespace YojigenShift.Po5.Scripts.Nodes;

/// <summary>
/// Manages the game stage logic: Spawning balls, handling input, and tracking game state.
/// </summary>
public partial class StageController : Node2D
{
	[Export]
	public PackedScene BallPrefab { get; set; } // Drag Ball.tscn here in Inspector

	[Export]
	public Node PathContainer { get; set; } // Optional: A node to organize spawned balls

	[Export]
	public float SpawnHeightY { get; set; } = 100f; // Fixed height for spawning

	[Export] public float SpawnCooldown { get; set; } = 0.6f;

	[Export] public HUDController HUD { get; set; }
	[Signal] public delegate void NextBallChangedEventHandler(int nextTypeInt);

	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	private YiBridge.GameElement _nextElementType;
	private bool _canSpawn = true;

	public override void _Ready()
	{
		_rng.Randomize();

		// Safety check
		if (BallPrefab == null)
		{
			SetProcessUnhandledInput(false);
			return;
		}

		if (HUD != null)
			NextBallChanged += HUD.OnNextBallChanged;

		_nextElementType = GetRandomElement();

		CallDeferred(nameof(EmitInitialSignal));
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (GameManager.Instance.IsGameOver) return;
		
		if (!_canSpawn) return;

		if (@event is InputEventScreenTouch touchEvent && touchEvent.Pressed)
		{
			SpawnBallAt(touchEvent.Position.X);
		}
	}

	private void EmitInitialSignal()
	{
		EmitSignal(SignalName.NextBallChanged, (int)_nextElementType);
	}

	private void SpawnBallAt(float xPosition)
	{
		_canSpawn = false;

		GetTree().CreateTimer(SpawnCooldown).Timeout += () => _canSpawn = true;

		// 1. Instantiate the ball
		var ballInstance = BallPrefab.Instantiate<BallNode>();

		// 2. Set position (Mouse X, Fixed Y)
		float clampedX = Mathf.Clamp(xPosition, 50, 670);
		ballInstance.Position = new Vector2(clampedX, SpawnHeightY);

		// 3. Add to scene tree
		if (PathContainer != null)
			PathContainer.AddChild(ballInstance);
		else
			AddChild(ballInstance);

		ballInstance.Setup(_nextElementType);

		_nextElementType = GetRandomElement();
		EmitSignal(SignalName.NextBallChanged, (int)_nextElementType);
	}

	private YiBridge.GameElement GetRandomElement()
	{
		return (YiBridge.GameElement)_rng.RandiRange(0, 4);
	}
}
