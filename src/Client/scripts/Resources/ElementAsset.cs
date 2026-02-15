using Godot;
using System;
using YojigenShift.Po5.Scripts.Bridges;

namespace YojigenShift.Po5.Scripts.Resources;

/// <summary>
/// A configuration asset that holds visual data for the Five Elements.
/// Create a .tres file from this resource in the Godot Editor.
/// </summary>
[GlobalClass]
public partial class ElementAsset : Resource
{
	[Export] public Texture2D[] Icons { get; set; } // Order: Wood, Fire, Earth, Metal, Water
	[Export] public Color[] Colors { get; set; } // Same order as above
	[Export] public Texture2D GlassOverlay { get; set; } // Shared Glas Shell
	[Export] public Texture2D BaseCircle { get; set; } // Shared Base Circle

	public Texture2D GetIcon(YiBridge.GameElement type)
	{
		int i = (int)type;
		if (Icons != null && i >= 0 && i < Icons.Length)
			return Icons[i];
		return null;
	}

	public Color GetColor(YiBridge.GameElement type)
	{
		int i = (int)type;
		if (Colors != null && i >= 0 && i < Colors.Length)
			return Colors[i];
		return new Color(1, 1, 1); // Default to white
	}
}
