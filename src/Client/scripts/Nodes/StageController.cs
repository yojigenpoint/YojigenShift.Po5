using Godot;
using YojigenShift.Po5.Scripts.Bridges;

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

	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	public override void _Ready()
	{
		_rng.Randomize();

		// Safety check
		if (BallPrefab == null)
		{
			GD.PushError("[StageController] BallPrefab is not assigned! Please set it in Inspector.");
			SetProcessUnhandledInput(false); // Disable input if setup is wrong
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventScreenTouch touchEvent)
		{
			if (touchEvent.Pressed)
			{
				SpawnBallAt(touchEvent.Position.X);
			}
		}
	}

	private void SpawnBallAt(float xPosition)
	{
		// 1. Instantiate the ball
		var ballInstance = BallPrefab.Instantiate<BallNode>();

		// 2. Set position (Mouse X, Fixed Y)
		ballInstance.Position = new Vector2(xPosition, SpawnHeightY);

		// 3. Add to scene tree
		if (PathContainer != null)
			PathContainer.AddChild(ballInstance);
		else
			AddChild(ballInstance);

		// 4. Randomize Element
		// Get a random enum value from GameElement (0 to 4)
		int randomTypeInt = _rng.RandiRange(0, 4);
		var randomType = (YiBridge.GameElement)randomTypeInt;

		// 5. Initialize the ball
		ballInstance.Setup(randomType);
	}
}
