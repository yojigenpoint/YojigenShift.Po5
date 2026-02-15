using Godot;
using System;
using YojigenShift.YiFramework.Core;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;

namespace YojigenShift.Po5.Scripts.Bridges;

/// <summary>
/// Acts as the middleware between Godot game logic and the YiFramework library.
/// This ensures that if the underlying library changes, we only need to update this bridge.
/// </summary>
public partial class YiBridge : Node
{
	// Singleton instance
	public static YiBridge Instance { get; private set; }

	// Enum mapping: Maps game internal enums to YiFramework enums.
	// This decouples the game from the library.
	public enum GameElement
	{
		Wood = 0,
		Fire = 1,
		Earth = 2,
		Metal = 3,
		Water = 4
	}

	public enum RelationType
	{
		None,
		Generating, // Sheng (Wood -> Fire)
		Overcoming, // Ke (Water -> Fire)
		Same        // (Fire == Fire)
	}

	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
			InitializeFramework();
		}
		else
		{
			QueueFree();
		}
	}

	/// <summary>
	/// Initializes the YiFramework logic engine.
	/// </summary>
	private void InitializeFramework()
	{
		GD.Print("[YiBridge] Initializing YiFramework...");

		try
		{
			// TODO: Call your framework's initialization method here if it exists.
			// Example: YiSystem.Initialize();

			GD.Print("[YiBridge] YiFramework initialized successfully.");
		}
		catch (Exception e)
		{
			GD.PushError($"[YiBridge] Failed to initialize YiFramework: {e.Message}");
		}
	}

	/// <summary>
	/// Helper to map GameElement to WuXingType.
	/// This is the "Anti-Corruption Layer".
	/// </summary>
	private WuXingType ToWuXing(GameElement element)
	{
		return element switch
		{
			GameElement.Wood => WuXingType.Wood,
			GameElement.Fire => WuXingType.Fire,
			GameElement.Earth => WuXingType.Earth,
			GameElement.Metal => WuXingType.Metal,
			GameElement.Water => WuXingType.Water,
			// Handle unexpected values gracefully or throw
			_ => throw new ArgumentException($"Cannot map GameElement {element} to WuXingType")
		};
	}

	/// <summary>
	/// Determines the relationship between two elements.
	/// Used for collision logic (e.g., Ball A hits Ball B).
	/// </summary>
	public RelationType GetRelationship(GameElement subject, GameElement target)
	{
		// 1. Convert Game types to Framework types
		WuXingType wuXingSubject = ToWuXing(subject);
		WuXingType wuXingTarget = ToWuXing(target);

		// 2. Call YiFramework Logic
		InteractionType result = WuXingMath.Compare(wuXingSubject, wuXingTarget);

		// 3. Map Framework result back to Game result
		return result switch
		{
			InteractionType.Generates => RelationType.Generating,
			InteractionType.Overcomes => RelationType.Overcoming,
			InteractionType.Same => RelationType.Same,
			_ => RelationType.None
		};
	}

	/// <summary>
	/// Gets the current Lunar context (Year, Month, Day, Hour).
	/// Used for the "Real-time" gameplay feature.
	/// </summary>
	public string GetCurrentLunarInfo()
	{
		// TODO: Replace with actual YiFramework Lunar calculation
		// return LunarCalendar.Now().ToString();
		return "Mock: JiaChen Year, BingYin Month";
	}

	/// <summary>
	/// A helper to get the localized name of an element.
	/// </summary>
	public string GetElementName(GameElement element)
	{
		WuXingType elem = ToWuXing(element);
		return elem.GetLocalizedName();
	}
}
