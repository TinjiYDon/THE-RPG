using UnityEngine;
using UnityEngine.UI;
using TheRPG.Narrative;

namespace TheRPG.UI
{
	public sealed class GuidanceBanner : MonoBehaviour
	{
		Canvas _canvas;
		GameObject _panel;
		Text _text;

		void Ensure()
		{
			if (_canvas == null)
			{
				var go = new GameObject("GuidanceCanvas");
				_canvas = go.AddComponent<Canvas>();
				_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				go.AddComponent<GraphicRaycaster>();
			}
			if (_panel == null)
			{
				_panel = new GameObject("GuidancePanel");
				_panel.transform.SetParent(_canvas.transform, false);
				var img = _panel.AddComponent<Image>();
				img.color = new Color(0f, 0f, 0f, 0.6f);
				var rt = _panel.GetComponent<RectTransform>();
				rt.anchorMin = new Vector2(0.5f, 0f);
				rt.anchorMax = new Vector2(0.5f, 0f);
				rt.sizeDelta = new Vector2(900, 80);
				rt.anchoredPosition = new Vector2(0, 60);

				var textGo = new GameObject("Text");
				textGo.transform.SetParent(_panel.transform, false);
				_text = textGo.AddComponent<Text>();
				_text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
				_text.alignment = TextAnchor.MiddleCenter;
				_text.color = Color.white;
				_text.fontSize = 20;
				var trt = _text.rectTransform; trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
			}
		}

		public void Show(string key, float seconds = 3f)
		{
			Ensure();
			_panel.SetActive(true);
			_text.text = StringTable.Get(key, key);
			CancelInvoke(nameof(Hide));
			Invoke(nameof(Hide), seconds);
		}

		public void Hide()
		{
			if (_panel != null) _panel.SetActive(false);
		}
	}
}


