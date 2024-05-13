using Terraria;

namespace Origins {
	internal static class Debugging {
		internal static void ChatOverhead(string message, int duration = 5) {
#if DEBUG
			if (Main.dedServ) return;
			Main.LocalPlayer.chatOverhead.NewMessage(message, duration);
#endif
		}
		internal static void ChatOverhead(object message, int duration = 5) {
			ChatOverhead(message.ToString(), duration);
		}
	}
}
