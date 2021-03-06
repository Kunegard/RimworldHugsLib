﻿using System.Reflection;
using HugsLib.GuiInject;
using HugsLib.Utils;
using UnityEngine;
using Verse;

namespace HugsLib.Logs {
	/**
	 * Injects two new buttons into the logger window by using WindowInjectionManager.
	 * "Copy" will copy the selected message to the clipboard and "Share logs" will initate the log publishing process.
	 */
	internal static class LogWindowInjection {
		private const float MessageDetailsScrollBarWidth = 16f;
		private static readonly Color shareButtonColor = new Color(.3f, 1f, .3f, 1f);
		private static FieldInfo selectedMessageField;

		public static void PrepareReflection() {
			selectedMessageField = typeof(EditWindow_Log).GetField("selectedMessage", BindingFlags.NonPublic | BindingFlags.Static);
			if (selectedMessageField != null && selectedMessageField.FieldType != typeof(LogMessage)) selectedMessageField = null;
			if (selectedMessageField == null) HugsLibController.Logger.Error("Failed to reflect EditWindow_Log.selectedMessage");
		}

		[WindowInjection(typeof(EditWindow_Log))]
		private static void DrawLogWindowButtons(Window window, Rect inRect) {
			var selectedMessage = selectedMessageField != null ? (LogMessage)selectedMessageField.GetValue(window) : null;
			var shareButtonPos = new Vector2(inRect.width, inRect.y);
			var prevColor = GUI.color;
			GUI.color = shareButtonColor;
			if (DoAutoWidthButton(shareButtonPos, "HugsLib_logs_shareBtn".Translate(), true)) {
				HugsLibController.Instance.LogUploader.ShowPublishPrompt();
			}
			GUI.color = prevColor;
			if (selectedMessage != null) {
				var copyButtonPos = new Vector2(inRect.width - MessageDetailsScrollBarWidth, inRect.height);
				if (DoAutoWidthButton(copyButtonPos, "HugsLib_logs_copy".Translate(), false)) {
					CopyMessage(selectedMessage);
				}
			}
		}

		private static bool DoAutoWidthButton(Vector2 position, string label, bool topAlign) {
			const float ButtonPaddingX = 16f;
			const float ButtonPaddingY = 2f;
			var buttonSize = Text.CalcSize(label);
			buttonSize.x += ButtonPaddingX;
			buttonSize.y += ButtonPaddingY;
			var buttonRect = new Rect(position.x - buttonSize.x, topAlign ? position.y : position.y - buttonSize.y, buttonSize.x, buttonSize.y);
			return Widgets.ButtonText(buttonRect, label);
		}

		private static void CopyMessage(LogMessage logMessage) {
			HugsLibUtility.CopyToClipboard(logMessage.text + "\n" + logMessage.StackTrace);
		}
	}
}