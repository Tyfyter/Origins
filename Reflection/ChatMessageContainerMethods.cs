using Microsoft.Xna.Framework;
using PegasusLib;
using PegasusLib.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Chat;
using Terraria.UI.Chat;

namespace Origins.Reflection {
	public class ChatMessageContainerMethods : ReflectionLoader {
		public static FastFieldInfo<ChatMessageContainer, bool> _prepared;
		public static FastFieldInfo<ChatMessageContainer, List<TextSnippet[]>> _parsedText;
		public static FastFieldInfo<RemadeChatMonitor, List<ChatMessageContainer>> _messages;
		public static void CreateCustomMessage(params TextSnippet[] textSnippets) {
			if (Main.chatMonitor is RemadeChatMonitor chatMonitor) {
				chatMonitor.AddNewMessage("placeholder", Color.White);
				ChatMessageContainer message = _messages.GetValue(chatMonitor)[0];
				_prepared.SetValue(message, true);
				_parsedText.SetValue(message, [textSnippets]);
			}
		}
		public static void CreateCustomMessage(List<TextSnippet> textSnippets) => CreateCustomMessage(textSnippets.ToArray());
		public static void CreateCustomMessage(params List<TextSnippet>[] textSnippets) => CreateCustomMessage(textSnippets.SelectMany(s => s).ToArray());
	}
}
