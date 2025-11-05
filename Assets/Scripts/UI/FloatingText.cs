using UnityEngine;
using UnityEngine.UI;

namespace TheRPG.UI
{
	public sealed class FloatingText : MonoBehaviour
	{
		public static void Spawn(Canvas parent, string text, Color color)
		{
			var go = new GameObject("FloatingText");
			go.transform.SetParent(parent.transform, false);
			var t = go.AddComponent<Text>();
			t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			t.text = text;
			t.color = color;
			t.alignment = TextAnchor.MiddleCenter;
			var rt = t.rectTransform;
			rt.anchorMin = new Vector2(0.5f, 0.5f);
			rt.anchorMax = new Vector2(0.5f, 0.5f);
			rt.anchoredPosition = Vector2.zero;
			go.AddComponent<FloatingTextAnim>();
		}

		private sealed class FloatingTextAnim : MonoBehaviour
		{
			private float _time;
			private Text _text;
			void Awake() { _text = GetComponent<Text>(); }
			void Update()
			{
				_time += Time.deltaTime;
				transform.Translate(0, 30f * Time.deltaTime, 0);
				if (_text != null)
				{
					var c = _text.color; c.a = Mathf.Lerp(1f, 0f, _time / 1.0f); _text.color = c;
				}
				if (_time >= 1.0f) Destroy(gameObject);
			}
		}
	}
}


