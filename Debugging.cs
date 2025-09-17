using System;
using System.Linq;
using Terraria;
using Terraria.Chat;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
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
		internal static void ChatMessage(string message, bool all = false) {
#if DEBUG
			if (Main.dedServ) return;
			if (all) {
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), Color.White);
			} else {
				Main.NewText(message);
			}
#endif
		}
		internal static void ChatMessage(object message, bool all = false) {
			ChatMessage(message.ToString(), all);
		}
		internal static void Assert(bool value, Exception exception) {
			if (!value) throw exception;
		}
		static bool firstFirstUpdate = true;
		internal static bool firstUpdate = false;
		//Running OriginSystem.PreUpdatePlayers for the first time
		//Running OriginPlayer.PreUpdate for the first time
		//Running OriginPlayer.SetControls for the first time
		//Running OriginPlayer.ResetEffects for the first time
		//Running OriginPlayer.UpdateDyes for the first time
		//Running OriginPlayer.PreUpdateBuffs for the first time
		//Running OriginPlayer.UpdateEquips for the first time
		//Running OriginPlayer.PreModifyLuck for the first time
		//Running OriginPlayer.PostUpdateEquips for the first time
		//Running OriginPlayer.PostUpdateMiscEffects for the first time
		//Running OriginPlayer.UpdateLifeRegen for the first time
		//Running OriginPlayer.PostUpdateRunSpeeds for the first time
		//Running OriginPlayer.PreUpdateMovement for the first time
		//Running Player.UpdateTouchingTiles for the first time
		//Running Player.SlopingCollision for the first time
		//Running PressurePlateHelper.UpdatePlayerPosition for the first time
		//Running OriginPlayer.PreItemCheck for the first time
		//Running OriginPlayer.ModifyWeaponDamage for the first time
		//Running OriginPlayer.ModifyWeaponDamage (after) for the first time
		//Running OriginPlayer.PostItemCheck for the first time
		//Running OriginPlayer.FrameEffects for the first time
		//Running OriginPlayer.PostUpdate for the first time
		//Running OriginSystem.PreUpdateNPCs for the first time
		//Running OriginSystem.PreUpdateGores for the first time
		//Running OriginSystem.PreUpdateProjectiles for the first time
		//Running OriginSystem.PreUpdateItems for the first time
		//Running OriginSystem.PreUpdateDusts for the first time
		//Running OriginSystem.PreUpdateTime for the first time
		//Running OriginSystem.PreUpdateWorld for the first time
		//Running OriginSystem.PreUpdateInvasions for the first time
		//Running OriginSystem.PostUpdateInvasions for the first time
		//Running OriginSystem.PostUpdateEverything for the first time
		//Running PostUpdateEverything (passed tickers) for the first time
		public static void LogFirstRun(string name, bool isGameLoop = false) {
			if (firstFirstUpdate && isGameLoop) {
				firstUpdate = true;
				firstFirstUpdate = false;
			}
			if (firstUpdate) {
				Origins.instance.Logger.Info($"Running {name} for the first time");
			}
		}
		public static void LogFirstRun(Delegate del, bool isGameLoop = false) => LogFirstRun($"{del.Method.DeclaringType.Name}.{del.Method.Name}", isGameLoop);
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
