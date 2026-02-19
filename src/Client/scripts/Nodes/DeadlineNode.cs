using Godot;
using YojigenShift.Po5.Scripts.Managers;

namespace YojigenShift.Po5.Scripts.Nodes;

/// <summary>
/// Detects if balls stay in the danger zone for too long.
/// </summary>
public partial class DeadlineNode : Area2D
{
	[Export] public float TimeLimit { get; set; } = 3.0f; // Seconds before Game Over
	[Export] public TextureRect VisualWarning { get; set; }

	private float _timer = 0f;
	private const float VelocityThreshold = 10.0f; // Minimum speed to be considered "moving"

	public override void _Ready()
	{
		if (VisualWarning != null)
			VisualWarning.Modulate = new Color(1, 1, 1, 0); // Start invisible
	}

	public override void _Process(double delta)
	{
		if (GameManager.Instance.IsGameOver) return;

		bool dangerDetected = false;

		var bodies = GetOverlappingBodies();

		foreach (var body in bodies)
		{
			if (body is BallNode ball)
			{
				if (ball.LinearVelocity.Length() < VelocityThreshold)
				{
					dangerDetected = true;
					break;
				}
			}
		}

		if (dangerDetected)
		{
			_timer += (float)delta;

			float dangerLevel = Mathf.Clamp(_timer / TimeLimit, 0f, 1f);

			// Visual warning (Flash Red)
			if (VisualWarning != null)
			{
				float baseAlpha = dangerLevel * 0.8f;
				float flash = (Mathf.Sin(_timer * 10) * 0.5f + 0.5f) * 0.2f;
				
				VisualWarning.Modulate = new Color(1, 1, 1, baseAlpha + flash);
			}

			if (_timer >= TimeLimit)
			{
				GameManager.Instance.TriggerGameOver();
			}
		}
		else
		{
			_timer = 0f;
			if (VisualWarning != null) 
				VisualWarning.Modulate = VisualWarning.Modulate.Lerp(new Color(1, 1, 1, 0), (float)delta * 5);
		}
	}
}
