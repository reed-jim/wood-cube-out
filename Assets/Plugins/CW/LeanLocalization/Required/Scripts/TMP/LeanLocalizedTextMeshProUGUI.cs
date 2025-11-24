using UnityEngine;
using TMPro;
using System.Text;
using System;

namespace Lean.Localization
{
	/// <summary>This component will update a TMPro.TextMeshProUGUI component with localized text, or use a fallback if none is found.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(TextMeshProUGUI))]
	[AddComponentMenu(LeanLocalization.ComponentPathPrefix + "Localized TextMeshProUGUI")]
	public class LeanLocalizedTextMeshProUGUI : LeanLocalizedBehaviour
	{
		#region PRIVATE FIELD
		private LeanTranslation _cachedTranslation;
		private string _cachedParameter;
		private string _cachedParameterValue;
		#endregion

		// public event Action textTranslatedEvent;

		[Tooltip("If PhraseName couldn't be found, this text will be used")]
		public string FallbackText;

		// This gets called every time the translation needs updating
		public override void UpdateTranslation(LeanTranslation translation)
		{
			// Get the TextMeshProUGUI component attached to this GameObject
			var text = GetComponent<TextMeshProUGUI>();

			// avoid TMP_SubMeshUI duplicate
			TMP_SubMeshUI subMeshUI = text.GetComponentInChildren<TMP_SubMeshUI>();

			if (subMeshUI != null)
			{
				Destroy(subMeshUI.gameObject);
			}

			// Use translation?
			if (translation != null && translation.Data is string)
			{
				text.text = LeanTranslation.FormatText((string)translation.Data, text.text, this, gameObject);
			}
			// Use fallback?
			else
			{
				text.text = LeanTranslation.FormatText(FallbackText, text.text, this, gameObject);
			}

			if (!String.IsNullOrEmpty(_cachedParameter))
			{
				UpdateTranslationWithParameter(_cachedParameter, _cachedParameterValue);
			}

			// textTranslatedEvent?.Invoke();

			_cachedTranslation = translation;
		}

		// SAFERIO - CUSTOMIZATION
		/// <summary>
		/// You typically use UpdateTranslationWithParameter when your localized string contains a placeholder like {PlayerName} or {Score} and you want to replace it at runtime.
		/// In your LeanLocalization data (or FallbackText) you have something like:
		/// "Hello, {PlayerName}! You have {Score} coins."
		/// localizedText.UpdateTranslationWithParameter("PlayerName", "Alice");
		/// </summary>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		public void UpdateTranslationWithParameter(string parameter, string value)
		{
			var textMeshPro = GetComponent<TextMeshProUGUI>();

			if (_cachedTranslation != null && _cachedTranslation.Data is string)
			{
				textMeshPro.text = LeanTranslation.FormatText((string)_cachedTranslation.Data, textMeshPro.text, this, gameObject);
			}
			else
			{
				textMeshPro.text = LeanTranslation.FormatText(FallbackText, textMeshPro.text, this, gameObject);
			}

			StringBuilder pattern = new StringBuilder();

			pattern.Append("{");
			pattern.Append(parameter);
			pattern.Append("}");

			// Debug.Log($"LevelText: {textMeshPro.text}");
			
			textMeshPro.text = textMeshPro.text.Replace(pattern.ToString(), value);
			// Debug.Log($"LevelText 2: {textMeshPro.text}");

			_cachedParameter = parameter;
			_cachedParameterValue = value;
		}

		protected virtual void Awake()
		{
			// Should we set FallbackText?
			if (string.IsNullOrEmpty(FallbackText) == true)
			{
				// Get the TextMeshProUGUI component attached to this GameObject
				var text = GetComponent<TextMeshProUGUI>();

				// Copy current text to fallback
				FallbackText = text.text;
			}
		}
	}
}