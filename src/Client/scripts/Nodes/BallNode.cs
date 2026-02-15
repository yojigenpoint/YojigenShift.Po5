using Godot;
using YojigenShift.Po5.Scripts.Bridges;
using YojigenShift.Po5.Scripts.Managers;

namespace YojigenShift.Po5.Scripts.Nodes;

/// <summary>
/// Controls the physical and visual behavior of an Element Ball.
/// Handles collision detection and requests relationship checks via YiBridge.
/// </summary>
public partial class BallNode : RigidBody2D
{
	// The element type of this specific ball (Wood, Fire, etc.)
	public YiBridge.GameElement ElementType { get; private set; } = YiBridge.GameElement.Wood;

	private Label _label;
	private Sprite2D _sprite;

	// Flag to prevent double-processing collisions (e.g., if already dying)
	private bool _isProcessed = false;

	public override void _Ready()
	{
		_label = GetNodeOrNull<Label>("Label");
		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");

		ContactMonitor = true;
		MaxContactsReported = 3;

		BodyEntered += OnBodyEntered;

		// Spawn Animation: Pop in
		Scale = Vector2.Zero;
		var tween = CreateTween();
		tween.TweenProperty(this, "scale", Vector2.One, 0.3f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
	}

	public void Setup(YiBridge.GameElement type)
	{
		ElementType = type;
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		Color color = new Color(1, 1, 1);

		// Temporary color mapping
		switch (ElementType)
		{
			case YiBridge.GameElement.Wood: color = new Color(0.2f, 0.8f, 0.2f); break; // Green
			case YiBridge.GameElement.Fire: color = new Color(0.9f, 0.2f, 0.2f); break; // Red
			case YiBridge.GameElement.Earth: color = new Color(0.8f, 0.7f, 0.2f); break; // Yellow
			case YiBridge.GameElement.Metal: color = new Color(0.9f, 0.9f, 0.9f); break; // White
			case YiBridge.GameElement.Water: color = new Color(0.2f, 0.4f, 0.9f); break; // Blue
		}

		if (_sprite != null) _sprite.Modulate = color;
		if (_label != null) _label.Text = ElementType.ToString();
	}

	private void OnBodyEntered(Node body)
	{
		// If I'm already dying or processed, ignore collisions
		if (_isProcessed) return;

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

		switch (relation)
		{
			case YiBridge.RelationType.Generating:
				// Logic: Mother sacrifices herself to feed the child.
				GD.Print($"[Sheng/生] {this.ElementType} -> {other.ElementType}");

				GameManager.Instance.AddScore(10);

				this.Die();
				other.Upgrade(); 
				break;

			case YiBridge.RelationType.Overcoming:
				// Logic: Victim is destroyed.
				GD.Print($"[Ke/克] {this.ElementType} -> {other.ElementType}");

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

		// Visual feedback: Pulse bigger
		var tween = CreateTween();
		tween.TweenProperty(this, "scale", Scale * 1.2f, 0.15f);
		tween.TweenProperty(this, "scale", Scale * 1.0f, 0.15f);
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
}
