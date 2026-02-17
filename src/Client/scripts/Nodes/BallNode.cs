using Godot;
using YojigenShift.Po5.Scripts.Bridges;
using YojigenShift.Po5.Scripts.Managers;
using YojigenShift.Po5.Scripts.Resources;

namespace YojigenShift.Po5.Scripts.Nodes;

/// <summary>
/// Controls the physical and visual behavior of an Element Ball.
/// Handles collision detection and requests relationship checks via YiBridge.
/// </summary>
public partial class BallNode : RigidBody2D
{
	[Export] public ElementAsset Theme { get; set; }

	// The element type of this specific ball (Wood, Fire, etc.)
	public YiBridge.GameElement ElementType { get; private set; } = YiBridge.GameElement.Wood;
	public bool IsInert { get; set; } = false;

	private Label _label;
	private Sprite2D _iconSprite;
	private Sprite2D _baseSprite;
	private Sprite2D _glassSprite;

	private Node2D _visuals;

	// Flag to prevent double-processing collisions (e.g., if already dying)
	private bool _isProcessed = false;

	private int _currentLevel = 0;
	private const int MaxLevel = 2;

	public override void _Ready()
	{
		_label = GetNodeOrNull<Label>("Label");

		_baseSprite = GetNode<Sprite2D>("Visuals/BaseColor");
		_iconSprite = GetNode<Sprite2D>("Visuals/InkIcon");
		_glassSprite = GetNode<Sprite2D>("Visuals/GlassShell");

		_visuals = GetNode<Node2D>("Visuals");

		ContactMonitor = true;
		MaxContactsReported = 3;

		BodyEntered += OnBodyEntered;

		// Spawn Animation: Pop in
		Scale = Vector2.Zero;
		float animTime = IsInert ? 0.1f : 0.3f;
		var tween = CreateTween();
		tween.TweenProperty(this, "scale", Vector2.One * 0.65f, animTime)
			 .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
	}

	public void Setup(YiBridge.GameElement type)
	{
		ElementType = type;
		UpdateVisuals();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_glassSprite != null)
		{
			_glassSprite.GlobalRotation = 0;
		}
	}

	private void UpdateVisuals()
	{
		if (Theme == null) return;

		Color color = Theme.GetColor(ElementType);
		Texture2D icon = Theme.GetIcon(ElementType);

		if (_baseSprite != null)
		{
			_baseSprite.Texture = Theme.GlassOverlay;
			_baseSprite.Modulate = Theme.GetColor(ElementType);
		}

		if (_iconSprite != null)
		{
			_iconSprite.Texture = icon;
		}

		if (_glassSprite != null)
		{
			_glassSprite.Texture = Theme.GlassOverlay;
			_glassSprite.Modulate = new Color(1, 1, 1, 0.6f);
		}

		if (_label != null) _label.Text = YiBridge.Instance.GetElementName(ElementType);
	}

	private void OnBodyEntered(Node body)
	{
		// If I'm already dying or processed, ignore collisions
		if (_isProcessed) return;

		if (IsInert) return;

		if (body is BallNode otherBall)
		{
			// If the other ball is already dying, ignore it
			if (otherBall._isProcessed || otherBall.IsQueuedForDeletion()) return;

			ProcessInteraction(otherBall);
		}
	}

	private void ProcessInteraction(BallNode other)
	{
		// Query YiBridge for the relationship: "My Element" vs "Other Element"
		var relation = YiBridge.Instance.GetRelationship(this.ElementType, other.ElementType);

		var stageController = GetTree().CurrentScene as StageController;

		switch (relation)
		{
			case YiBridge.RelationType.Generating:
				// Logic: Mother sacrifices herself to feed the child.
				stageController.CheckTutorial(true, this.ElementType, other.ElementType);

				AudioManager.Instance.PlaySFX("merge");
				GameManager.Instance.AddScore(10);

				this.Die();
				other.Upgrade(); 
				break;

			case YiBridge.RelationType.Overcoming:
				// Logic: Victim is destroyed.
				stageController.CheckTutorial(false, this.ElementType, other.ElementType);

				AudioManager.Instance.PlaySFX("destroy", 0.2f);
				GameManager.Instance.AddScore(50);

				other.Die();
				break;

				// Case: Same or None -> Do nothing, just physics collision.
		}
	}

	/// <summary>
	/// Called when this ball receives "Sheng" energy.
	/// </summary>
	public void Upgrade()
	{
		if (_isProcessed) return;

		_currentLevel++;

		if (_currentLevel > MaxLevel)
		{
			Ascend();
		}
		else
		{
			float baseScale = 0.65f;
			float targetScale = baseScale * (1.0f +(_currentLevel * 0.3f));

			var tween = CreateTween();
			tween.TweenProperty(this, "scale", Vector2.One * targetScale, 0.3f)
				 .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		}
	}

	/// <summary>
	/// Called when this ball is consumed or destroyed.
	/// </summary>
	public void Die()
	{
		if (_isProcessed) return;
		_isProcessed = true;

		// Visual feedback: Shrink to zero then delete
		// Disable collision immediately so it doesn't trigger others while shrinking
		SetDeferred("freeze", true); // Or disable collision shape
		var collider = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (collider != null) collider.SetDeferred("disabled", true);

		var tween = CreateTween();
		tween.TweenProperty(this, "scale", Vector2.Zero, 0.15f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
		tween.TweenCallback(Callable.From(QueueFree));
	}

	/// <summary>
	/// Gets called when this ball reaches max level, deisappears, and triggers high score bonus.
	/// </summary>
	private void Ascend()
	{
		if (_isProcessed) return;
		_isProcessed = true;

		GD.Print($"[Ascend] {ElementType} has reached nirvana!");
		GameManager.Instance.AddScore(100);
		AudioManager.Instance.PlaySFX("merge", 0.5f);

		SetDeferred("freeze", true);
		var collider = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (collider != null) collider.SetDeferred("disabled", true);

		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(this, "scale", Scale * 1.5f, 0.5f);
		tween.TweenProperty(this, "modulate:a", 0f, 0.5f);
		tween.Chain().TweenCallback(Callable.From(QueueFree));
	}
}
