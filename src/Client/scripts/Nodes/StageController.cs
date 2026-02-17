using Godot;
using YojigenShift.Po5.Scripts.Bridges;
using YojigenShift.Po5.Scripts.Managers;
using YojigenShift.Po5.Scripts.Resources;
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

	[Export] public int InitialChaosCount { get; set; } = 120;
	[Export] public Rect2 ChaosArea { get; set; } = new Rect2(50, 200, 620, 600);

	[Export] public DialogueController IntroDialogue { get; set; }

	[Export] public HUDController HUD { get; set; }
	[Signal] public delegate void NextBallChangedEventHandler(int nextTypeInt);

	[Export] public NarrativeAsset StoryData;

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

		SpawnInitialChaos();

		CallDeferred(nameof(CheckAndStartGame));
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

	public void CheckTutorial(bool isSheng, YiBridge.GameElement subject, YiBridge.GameElement obj)
	{
		if (IntroDialogue == null || StoryData == null) return;

		if (IntroDialogue.Visible) return;

		if (isSheng && !GameManager.Instance.HasSeenSheng)
		{
			GameManager.Instance.HasSeenSheng = true;
			PauseGameAndShowDialogue(StoryData.TutorialSheng);
		}
		else if (!isSheng && !GameManager.Instance.HasSeenKe)
		{
			GameManager.Instance.HasSeenKe = true;
			PauseGameAndShowDialogue(StoryData.TutorialKe);
		}
	}

	private void SpawnInitialChaos()
	{
		for (int i = 0; i < InitialChaosCount; i++)
		{
			var ball = BallPrefab.Instantiate<BallNode>();

			float x = _rng.RandfRange(ChaosArea.Position.X, ChaosArea.End.X);
			float y = _rng.RandfRange(ChaosArea.Position.Y, ChaosArea.End.Y);

			ball.Position = new Vector2(x, y);

			if (PathContainer != null) PathContainer.AddChild(ball);
			else AddChild(ball);

			ball.Setup(GetRandomElement());
			ball.IsInert = true;
		}
	}

	private void PauseGameAndShowDialogue(string[] lines)
	{
		_canSpawn = false;

		IntroDialogue.DialogueFinished += OnTutorialFinished;
		IntroDialogue.StartDialogue(lines);
	}

	private void OnTutorialFinished()
	{
		IntroDialogue.DialogueFinished -= OnTutorialFinished;
		_canSpawn = true;
	}

	private void CheckAndStartGame()
	{
		if (!GameManager.Instance.HasSeenIntro && IntroDialogue != null && StoryData != null)
		{
			IntroDialogue.StartDialogue(StoryData.IntroLines);
		}
		else
		{
			StartGameplay();
		}
	}

	private void OnIntroFinished()
	{
		GameManager.Instance.HasSeenIntro = true;

		IntroDialogue.DialogueFinished -= OnIntroFinished;

		StartGameplay();
	}

	private void StartGameplay()
	{
		_canSpawn = true;
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

		float screenWidth = GetViewportRect().Size.X;

		float margin = 35f;

		// 2. Set position (Mouse X, Fixed Y)
		float clampedX = Mathf.Clamp(xPosition, margin, screenWidth - margin);
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
