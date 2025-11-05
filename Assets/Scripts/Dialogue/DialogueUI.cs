using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TheRPG.Content;
using TheRPG.Narrative;
using TheRPG.World;

namespace TheRPG.Dialogue
{
	public interface IDialogueUI
	{
		Task<Choice> Show(NarrativeEvent ev, string renderedText, IReadOnlyList<Choice> choices, WorldState world);
		void Log(DialogueEntry entry);
		void ShowLoading(string message);
		void HideLoading();
		void ShowProgress(float progress, string message);
	}

	public sealed class DialogueEntry
	{
		public string EventId;
		public string ChoiceId;
		public string Text;
	}

	public sealed class DialogueUI : MonoBehaviour, IDialogueUI
	{
		[SerializeField] Canvas _canvas;
		[SerializeField] Text _text;
		[SerializeField] RectTransform _buttonsRoot;
		[SerializeField] Button _buttonPrefab;

		TaskCompletionSource<Choice> _pending;
		readonly List<Button> _spawned = new List<Button>();
		GameObject _loadingPanel;
		Text _loadingText;
		Image _progressBar;

		bool _typing;
		float _typeSpeed = 0.02f; // seconds per char
		bool _skip;

		void Awake()
		{
			EnsureUi();
		}

		void Update()
		{
			if (_typing)
			{
				if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) _skip = true;
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) _typeSpeed = 0.005f; else _typeSpeed = 0.02f;
			}
		}

		public async Task<Choice> Show(NarrativeEvent ev, string renderedText, IReadOnlyList<Choice> choices, WorldState world)
		{
			EnsureUi();
			ClearButtons();
			await TypeText(Localize(renderedText ?? ev?.Text?.Base ?? string.Empty));
			_pending = new TaskCompletionSource<Choice>();

			var evaluator = new ConditionEvaluator();
			for (int i = 0; i < (choices?.Count ?? 0); i++)
			{
				var ch = choices[i];
				var btn = Instantiate(_buttonPrefab, _buttonsRoot);
				btn.GetComponentInChildren<Text>().text = Localize(ch.Label);
				var ok = ch.Requirements == null || evaluator.Evaluate(ch.Requirements, world);
				btn.interactable = ok;
				btn.onClick.AddListener(() => OnPick(ch));
				_spawned.Add(btn);

				if (!ok)
				{
					var reasonGo = new GameObject("Reason");
					reasonGo.transform.SetParent(btn.transform, false);
					var reason = reasonGo.AddComponent<Text>();
					reason.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
					reason.alignment = TextAnchor.UpperCenter;
					reason.color = new Color(1f, 0.6f, 0.6f, 0.9f);
					reason.fontSize = 14;
					var rt = reason.rectTransform;
					rt.anchorMin = new Vector2(0, 0);
					rt.anchorMax = new Vector2(1, 0);
					rt.anchoredPosition = new Vector2(0, -18);
					rt.offsetMin = new Vector2(0, -18);
					rt.offsetMax = new Vector2(0, 0);
					reason.text = BuildReason(ch.Requirements);
				}
			}
			return await _pending.Task;
		}

		string BuildReason(IReadOnlyList<Condition> conditions)
		{
			if (conditions == null || conditions.Count == 0) return StringTable.Get("ui.button.cancel", "取消");
			var parts = new System.Text.StringBuilder();
			for (int i = 0; i < conditions.Count; i++)
			{
				var c = conditions[i];
				if (i > 0) parts.Append(" · ");
				parts.Append($"{c.Key} {c.Op} {c.Value}");
			}
			return parts.ToString();
		}

		async Task TypeText(string full)
		{
			_typing = true; _skip = false;
			_text.text = string.Empty;
			for (int i = 0; i < full.Length; i++)
			{
				_text.text = full.Substring(0, i + 1);
				if (_skip) { _text.text = full; break; }
				await Task.Delay(TimeSpan.FromSeconds(_typeSpeed));
			}
			_typing = false; _skip = false;
		}

		string Localize(string input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;
			if (input.StartsWith("@"))
			{
				var key = input.Substring(1);
				return StringTable.Get(key, input);
			}
			return input;
		}

		public void Log(DialogueEntry entry)
		{
			// Hook for history log (optional)
		}

		public void ShowLoading(string message)
		{
			EnsureLoadingPanel();
			if (_loadingPanel != null) _loadingPanel.SetActive(true);
			if (_loadingText != null) _loadingText.text = message ?? "加载中...";
			if (_progressBar != null) _progressBar.fillAmount = 0f;
		}

		public void HideLoading()
		{
			if (_loadingPanel != null) _loadingPanel.SetActive(false);
		}

		public void ShowProgress(float progress, string message)
		{
			EnsureLoadingPanel();
			if (_loadingPanel != null) _loadingPanel.SetActive(true);
			if (_loadingText != null) _loadingText.text = message ?? $"加载中... {progress:P0}";
			if (_progressBar != null) _progressBar.fillAmount = Mathf.Clamp01(progress);
		}

		void EnsureLoadingPanel()
		{
			if (_loadingPanel != null) return;
			EnsureUi();
			var panelGo = new GameObject("LoadingPanel");
			panelGo.transform.SetParent(_canvas.transform, false);
			var panelImg = panelGo.AddComponent<Image>();
			panelImg.color = new Color(0, 0, 0, 0.7f);
			var rt = panelGo.GetComponent<RectTransform>();
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.offsetMin = Vector2.zero;
			rt.offsetMax = Vector2.zero;

			var textGo = new GameObject("LoadingText");
			textGo.transform.SetParent(panelGo.transform, false);
			_loadingText = textGo.AddComponent<Text>();
			_loadingText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			_loadingText.alignment = TextAnchor.MiddleCenter;
			_loadingText.color = Color.white;
			_loadingText.fontSize = 24;
			var textRt = _loadingText.rectTransform;
			textRt.anchorMin = new Vector2(0.1f, 0.5f);
			textRt.anchorMax = new Vector2(0.9f, 0.6f);
			textRt.offsetMin = Vector2.zero;
			textRt.offsetMax = Vector2.zero;

			var barGo = new GameObject("ProgressBar");
			barGo.transform.SetParent(panelGo.transform, false);
			var barBg = barGo.AddComponent<Image>();
			barBg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
			var barRt = barBg.rectTransform;
			barRt.anchorMin = new Vector2(0.1f, 0.4f);
			barRt.anchorMax = new Vector2(0.9f, 0.45f);
			barRt.offsetMin = Vector2.zero;
			barRt.offsetMax = Vector2.zero;

			var barFillGo = new GameObject("Fill");
			barFillGo.transform.SetParent(barGo.transform, false);
			_progressBar = barFillGo.AddComponent<Image>();
			_progressBar.color = new Color(0.2f, 0.8f, 0.2f, 1f);
			_progressBar.type = Image.Type.Filled;
			_progressBar.fillMethod = Image.FillMethod.Horizontal;
			var fillRt = _progressBar.rectTransform;
			fillRt.anchorMin = Vector2.zero;
			fillRt.anchorMax = Vector2.one;
			fillRt.offsetMin = Vector2.zero;
			fillRt.offsetMax = Vector2.zero;

			_loadingPanel = panelGo;
			_loadingPanel.SetActive(false);
		}

		void OnPick(Choice ch)
		{
			if (_pending != null && !_pending.Task.IsCompleted)
			{
				_pending.TrySetResult(ch);
			}
		}

		void EnsureUi()
		{
			if (_canvas != null && _text != null && _buttonsRoot != null && _buttonPrefab != null) return;
			if (_canvas == null)
			{
				var go = new GameObject("DialogueCanvas");
				_canvas = go.AddComponent<Canvas>();
				_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				go.AddComponent<GraphicRaycaster>();
			}
			if (_text == null)
			{
				var textGo = new GameObject("Text");
				textGo.transform.SetParent(_canvas.transform, false);
				_text = textGo.AddComponent<Text>();
				_text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
				_text.alignment = TextAnchor.UpperLeft;
				_text.rectTransform.anchorMin = new Vector2(0.05f, 0.55f);
				_text.rectTransform.anchorMax = new Vector2(0.95f, 0.95f);
				_text.rectTransform.offsetMin = Vector2.zero;
				_text.rectTransform.offsetMax = Vector2.zero;
			}
			if (_buttonsRoot == null)
			{
				var btnRoot = new GameObject("Buttons");
				btnRoot.transform.SetParent(_canvas.transform, false);
				_buttonsRoot = btnRoot.AddComponent<RectTransform>();
				_buttonsRoot.anchorMin = new Vector2(0.05f, 0.05f);
				_buttonsRoot.anchorMax = new Vector2(0.95f, 0.45f);
				_buttonsRoot.offsetMin = Vector2.zero;
				_buttonsRoot.offsetMax = Vector2.zero;
			}
			if (_buttonPrefab == null)
			{
				_buttonPrefab = CreateDefaultButtonPrefab();
			}
		}

		void ClearButtons()
		{
			for (int i = 0; i < _spawned.Count; i++)
			{
				if (_spawned[i] != null) Destroy(_spawned[i].gameObject);
			}
			_spawned.Clear();
		}

		Button CreateDefaultButtonPrefab()
		{
			var btnGo = new GameObject("ButtonPrefab");
			btnGo.SetActive(false);
			var img = btnGo.AddComponent<Image>();
			img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
			var btn = btnGo.AddComponent<Button>();

			var labelGo = new GameObject("Label");
			labelGo.transform.SetParent(btnGo.transform, false);
			var label = labelGo.AddComponent<Text>();
			label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			label.alignment = TextAnchor.MiddleCenter;
			label.color = Color.white;
			var rt = label.rectTransform;
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.offsetMin = Vector2.zero;
			rt.offsetMax = Vector2.zero;

			// layout when instantiating
			btnGo.AddComponent<LayoutElement>().minHeight = 40f;

			return btn;
		}
	}
}


