using Godot;

namespace YojigenShift.Po5.Scripts.Resources;

/// <summary>
/// Stores game scripts and dialogue lines.
/// </summary>
[GlobalClass]
public partial class NarrativeAsset : Resource
{
	// Prologue
	[Export(PropertyHint.MultilineText)]
	public string[] IntroLines { get; set; }

	// Tutorial: Generates (Only trigger once)
	[Export(PropertyHint.MultilineText)]
	public string[] TutorialSheng { get; set; }

	// tutorial: Overcomes
	[Export(PropertyHint.MultilineText)]
	public string[] TutorialKe { get; set; }

	// Fail comfort (Random one)
	[Export(PropertyHint.MultilineText)]
	public string[] GameOverComforts { get; set; }
}
