using Godot;
using System.Collections.Generic;

namespace YojigenShift.Po5.Scripts.UI;

public partial class DialogueController : Control
{
	[Export] public TextureRect Portrait { get; set; }
	[Export] public RichTextLabel ContentLabel { get; set; }
	[Export] public Control ClickIndicator { get; set; }

	// 事件：当对话结束时触发
	[Signal] public delegate void DialogueFinishedEventHandler();

	private Queue<string> _sentences = new Queue<string>();
	private bool _isTyping = false;

	public override void _Ready()
	{
		// 初始隐藏，需要外部调用 StartDialogue 开启
		Visible = false;
	}

	public void StartDialogue(string[] lines)
	{
		_sentences.Clear();
		foreach (var line in lines)
		{
			_sentences.Enqueue(line);
		}

		Visible = true;
		ShowNextSentence();
	}

	public override void _GuiInput(InputEvent @event)
	{
		// 点击屏幕任意位置继续
		if (@event is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
		{
			if (_isTyping)
			{
				// 如果正在打字，点击则瞬间显示全字
				ContentLabel.VisibleCharacters = -1;
				_isTyping = false;
			}
			else
			{
				// 如果已显示完，点击则下一句
				ShowNextSentence();
			}
			AcceptEvent(); // 阻止点击穿透到后面
		}
	}

	private void ShowNextSentence()
	{
		if (_sentences.Count == 0)
		{
			EndDialogue();
			return;
		}

		string line = _sentences.Dequeue();
		ContentLabel.Text = line;
		ContentLabel.VisibleCharacters = 0;
		_isTyping = true;

		// 打字机效果
		var tween = CreateTween();
		float duration = line.Length * 0.05f; // 每个字 0.05秒
		tween.TweenProperty(ContentLabel, "visible_characters", line.Length, duration);
		tween.TweenCallback(Callable.From(() => _isTyping = false));
	}

	private void EndDialogue()
	{
		Visible = false;
		EmitSignal(SignalName.DialogueFinished);
	}
}
