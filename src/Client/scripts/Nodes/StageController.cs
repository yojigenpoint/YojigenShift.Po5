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

	[Export] public DialogueController IntroDialogue { get; set; }

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

		CheckAndStartGame();
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

	private void CheckAndStartGame()
	{
		// 如果还没看过剧情，且对话框引用存在
		if (!GameManager.Instance.HasSeenIntro && IntroDialogue != null)
		{
			GD.Print("[Stage] Starting Intro Narrative...");

			// 1. 绑定对话结束信号
			IntroDialogue.DialogueFinished += OnIntroFinished;

			// 2. 准备台词 (这里可以以后从 JSON 或 Resource 读取)
			string[] introLines = new string[]
			{
					"咳咳... 年轻人，你终于来了。",
					"这世间五行混乱，天地之气失衡。",
					"你需要通过「五行相生」之理，调和这些元气。",
					"切记：水生木，木生火... 只有顺势而为，方能功德圆满。",
                    "去吧，让我看看你的悟性！"
			};

			// 3. 开始对话 (游戏保持暂停状态 _canSpawn = false)
			IntroDialogue.StartDialogue(introLines);
		}
		else
		{
			// 如果已经看过，或者没有对话框，直接开始
			StartGameplay();
		}
	}

	private void OnIntroFinished()
	{
		GD.Print("[Stage] Intro Finished. Starting Game.");

		// 标记已看
		GameManager.Instance.HasSeenIntro = true;

		// 解绑信号防止内存泄漏
		IntroDialogue.DialogueFinished -= OnIntroFinished;

		StartGameplay();
	}

	private void StartGameplay()
	{
		// 允许生成
		_canSpawn = true;
		GD.Print("[Stage] Gameplay Started!");
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
