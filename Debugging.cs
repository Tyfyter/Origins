using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

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
#if DEBUG
	internal class ShaderTestingEffect : ModSceneEffect {
		bool isActive = false;
		public override bool IsSceneEffectActive(Player player) {
			if (Keybindings.DebugScreenShader.JustPressed) isActive ^= true;
			return isActive;
		}
		public override void SpecialVisuals(Player player, bool isActive) {
			ScreenShaderData screenShaderData = Filters.Scene["Origins:TestingShader"].GetShader();
			screenShaderData.UseProgress(1);
			screenShaderData.Shader.Parameters["uScale"].SetValue(64);
			screenShaderData.Shader.Parameters["uSaturation"].SetValue(1);
			if (Main.projectile.FirstOrDefault(p => p.active && Main.projPet[p.type]) is Projectile { Center: Vector2 worldPosition }) {
				Filters.Scene["Origins:TestingShader"].Opacity = isActive.ToInt();
				player.ManageSpecialBiomeVisuals("Origins:TestingShader", isActive, worldPosition);
			} else {
				Filters.Scene["Origins:TestingShader"].Opacity = 0;
				player.ManageSpecialBiomeVisuals("Origins:TestingShader", false, Vector2.Zero);
			}
		}
	}
#endif
}
