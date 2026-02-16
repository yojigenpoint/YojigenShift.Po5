using Godot;
using YojigenShift.Po5.Scripts.Bridges;
using YojigenShift.Po5.Scripts.Managers;
using YojigenShift.Po5.Scripts.Resources;

namespace YojigenShift.Po5.Scripts.UI;

public partial class LibraryController : Control
{
	[ExportGroup("Data Source")]
	[Export] public ElementAsset ElemTheme { get; set; } // 拖入 DefaultTheme.tres

	[ExportGroup("UI References")]
	[Export] public Button BackButton { get; set; }
	[Export] public Control DetailPopup { get; set; } // 详情弹窗面板
	[Export] public Button CloseDetailButton { get; set; }

	// 详情页内的控件
	[ExportSubgroup("Detail Content")]
	[Export] public TextureRect DetailIcon { get; set; }
	[Export] public Label DetailTitle { get; set; }
	[Export] public RichTextLabel DetailDesc { get; set; } // 使用 RichText 支持排版
	[Export] public Label DetailSheng { get; set; }
	[Export] public Label DetailKe { get; set; }

	// 按钮容器 (假设里面有5个按钮，命名为 BtnWood, BtnFire...)
	[Export] public Control ButtonGrid { get; set; }

	public override void _Ready()
	{
		// 初始隐藏弹窗
		if (DetailPopup != null) DetailPopup.Visible = false;

		// 绑定通用按钮
		if (BackButton != null) BackButton.Pressed += OnBackPressed;
		if (CloseDetailButton != null) CloseDetailButton.Pressed += ClosePopup;

		// 自动绑定五行按钮
		// 假设 ButtonGrid 下面有 5 个按钮，顺序对应 Wood(0) -> Water(4)
		if (ButtonGrid != null)
		{
			int index = 0;
			foreach (var child in ButtonGrid.GetChildren())
			{
				if (child is BaseButton btn)
				{
					// 捕获当前的 index 变量
					int capturedIndex = index;
					btn.Pressed += () => OnElementPressed((YiBridge.GameElement)capturedIndex);

					// 初始化按钮图标 (可选)
					if (ElemTheme != null && btn is TextureButton texBtn)
					{
						// 如果是 TextureButton，可以自动设置图标
						// texBtn.TextureNormal = Theme.GetIcon((YiBridge.GameElement)capturedIndex);
					}
					index++;
				}
			}
		}
	}

	private void OnElementPressed(YiBridge.GameElement type)
	{
		if (ElemTheme == null) return;

		// 1. 填充数据
		if (DetailIcon != null)
		{
			DetailIcon.Texture = ElemTheme.GetIcon(type);
			DetailIcon.Modulate = ElemTheme.GetColor(type);
		}

		if (DetailTitle != null)
		{
			//DetailTitle.Text = ElemTheme.GetName(type);
			DetailTitle.Text = YiBridge.Instance.GetElementName(type);
			DetailTitle.Modulate = ElemTheme.GetColor(type); // 标题使用元素色
		}

		//if (DetailDesc != null) DetailDesc.Text = ElemTheme.GetDescription(type);
		//if (DetailSheng != null) DetailSheng.Text = $"[生] {ElemTheme.GetShengText(type)}";
		//if (DetailKe != null) DetailKe.Text = $"[克] {ElemTheme.GetKeText(type)}";

		// 2. 显示弹窗 (带简单的弹出动画)
		if (DetailPopup != null)
		{
			DetailPopup.Visible = true;
			DetailPopup.Modulate = new Color(1, 1, 1, 0);
			DetailPopup.Scale = new Vector2(0.9f, 0.9f);

			var tween = CreateTween();
			tween.SetParallel(true);
			tween.TweenProperty(DetailPopup, "modulate:a", 1.0f, 0.2f);
			tween.TweenProperty(DetailPopup, "scale", Vector2.One, 0.2f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		}
	}

	private void ClosePopup()
	{
		if (DetailPopup != null) DetailPopup.Visible = false;
	}

	private void OnBackPressed()
	{
		// 返回主菜单
		if (SceneManager.Instance != null)
		{
			SceneManager.Instance.ChangeScene("res://src/Client/scenes/UI/MainMenu.tscn");
		}
		else
		{
			GetTree().ChangeSceneToFile("res://src/Client/scenes/UI/MainMenu.tscn");
		}
	}
}
