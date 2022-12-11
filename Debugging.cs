using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Origins {
	internal static class Debugging {
		internal static void ChatOverhead(string message, int duration = 5) {
#if DEBUG
			Main.LocalPlayer.chatOverhead.NewMessage(message, duration);
#endif
		}
	}
}
