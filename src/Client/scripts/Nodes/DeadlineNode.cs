using Godot;
using YojigenShift.Po5.Scripts.Managers;

namespace YojigenShift.Po5.Scripts.Nodes;

/// <summary>
/// Detects if balls stay in the danger zone for too long.
/// </summary>
public partial class DeadlineNode : Area2D
{
	[Export] public float TimeLimit { get; set; } = 2.0f; // Seconds before Game Over

	private float _timer = 0f;
	private int _ballsInZone = 0;
	private ColorRect _visualLine; // Optional: To flash red

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		// Optional: Find a visual child to animate
		_visualLine = GetNodeOrNull<ColorRect>("ColorRect");
	}

	public override void _Process(double delta)
	{
		if (GameManager.Instance.IsGameOver) return;

		// Only count down if balls are in the zone AND they are mostly stopped (sleeping)
		// But checking sleeping is tricky. For prototype, just checking presence is enough.
		// We should ensure we don't count the ball currently being spawned/falling through.
		// (Typically balls spawn *above* the line. If they pile up *to* the line, that's game over)

		if (_ballsInZone > 0)
		{
			_timer += (float)delta;

			// Visual warning (Flash Red)
			if (_visualLine != null)
			{
				float flash = Mathf.Sin(_timer * 10) * 0.5f + 0.5f;
				_visualLine.Color = new Color(1, 0, 0, flash * 0.5f);
			}

			if (_timer >= TimeLimit)
			{
				GameManager.Instance.TriggerGameOver();
			}
		}
		else
		{
			_timer = 0f;
			if (_visualLine != null) _visualLine.Color = new Color(1, 0, 0, 0); // Hide
		}
	}

	private void OnBodyEntered(Node body)
	{
		if (body is BallNode)
		{
			// Verify if the ball is actually settled? 
			// For simplified logic: Any ball touching the line counts.
			_ballsInZone++;
		}
	}

	private void OnBodyExited(Node body)
	{
		if (body is BallNode)
		{
			_ballsInZone--;
			if (_ballsInZone < 0) _ballsInZone = 0; // Safety clamp
		}
	}
}
