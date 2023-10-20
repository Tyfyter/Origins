using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Armor.Vanity.Dev.PlagueTexan;
using Origins.Questing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins {
	public partial class OriginPlayer : ModPlayer {
		internal static List<Player> cachedTornPlayers;
		internal static bool anyActiveTorn;
		internal static bool drawingTorn;
		public static ScreenTarget TornScreenTarget { get; private set; }
		public override void Load() {
			if (Main.dedServ) return;
			cachedTornPlayers = new();
			TornScreenTarget = new(
				MaskAura,
				() => {
					bool isActive = anyActiveTorn;
					anyActiveTorn = false;
					return isActive && Lighting.NotRetro;
				},
				0
			);
			On_Main.DrawInfernoRings += Main_DrawInfernoRings;
		}
		private void Main_DrawInfernoRings(On_Main.orig_DrawInfernoRings orig, Main self) {
			orig(self);
			if (Main.dedServ) return;
			if (Lighting.NotRetro) DrawAura(Main.spriteBatch);
		}
		static void MaskAura(SpriteBatch spriteBatch) {
			if (Main.dedServ) return;
			//SpriteBatch mainSpriteBatch = Main.spriteBatch;
			SpriteBatchState state = spriteBatch.GetState();
			try {
				//Main.spriteBatch = spriteBatch;
				drawingTorn = true;
				//spriteBatch.End();
				spriteBatch.Restart(state);
				Origins.drawPlayersWithShader = Origins.coordinateMaskFilterID;
				Origins.coordinateMaskFilter.Shader.Parameters["uCoordinateSize"].SetValue(new Vector2(20, 24));//put the size of the texture in here

				PlayerShaderSet dontCoverArmor = new PlayerShaderSet(0);
				Origins.keepPlayerShader = Origins.transparencyFilterID;
				dontCoverArmor.cHead = Origins.keepPlayerShader;
				for (int i = 0; i < cachedTornPlayers.Count; i++) {
					Player player = cachedTornPlayers[i];
					PlayerShaderSet shaderSet = new(player);
					dontCoverArmor.Apply(player);
					Vector2 itemLocation = player.itemLocation;
					try {
						player.itemLocation = Vector2.Zero;
						Main.PlayerRenderer.DrawPlayer(
							Main.Camera,
							player,
							player.position + new Vector2(0, player.gfxOffY),
							player.fullRotation,
							player.fullRotationOrigin,
							scale:Main.GameViewMatrix.Zoom.X
						);
					} finally {
						player.itemLocation = itemLocation;
						shaderSet.Apply(player);
					}
				}
				//Main.PlayerRenderer.DrawPlayers(Main.Camera, cachedTornPlayers);
			} finally {
				Origins.drawPlayersWithShader = -1;
				Origins.keepPlayerShader = -1;
				cachedTornPlayers.Clear();
				drawingTorn = false;
				spriteBatch.Restart(state);
				//Main.spriteBatch = mainSpriteBatch;
			}
		}
		static void DrawAura(SpriteBatch spriteBatch) {
			if (Main.dedServ) return;
			//anyActive = false;
			Main.LocalPlayer.ManageSpecialBiomeVisuals("Origins:MaskedTornFilter", anyActiveTorn, Main.LocalPlayer.Center);
			if (anyActiveTorn) {
				Filters.Scene["Origins:MaskedTornFilter"].GetShader().UseImage(TornScreenTarget.RenderTarget, 1);
			}
			//spriteBatch.Draw(TornScreenTarget.RenderTarget, Vector2.Zero, Color.Blue);
		}
		public override void HideDrawLayers(PlayerDrawSet drawInfo) {
			Item item = drawInfo.heldItem;
			if (item.ModItem is ICustomDrawItem) PlayerDrawLayers.HeldItem.Hide();

			if (mountOnly && !drawInfo.headOnlyRender) {
				for (int i = 0; i < PlayerDrawLayerLoader.DrawOrder.Count; i++) {
					PlayerDrawLayer layer = PlayerDrawLayerLoader.DrawOrder[i];
					if (layer != PlayerDrawLayers.MountFront && layer != PlayerDrawLayers.MountBack) {
						layer.Hide();
					}
				}
			}
			if (hideAllLayers) {
				foreach (var layer in PlayerDrawLayerLoader.Layers) {
					layer.Hide();
				}
			}
		}
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {

			if (plagueSight) drawInfo.colorEyes = IsDevName(Player.name, 1) ? new Color(43, 185, 255) : Color.Gold;
			if (mysteriousSprayMult != 1f) {
				float lightSaturationMult = (float)Math.Pow(mysteriousSprayMult, 2f);
				float saturationMult = 1f - (float)Math.Pow(1f - mysteriousSprayMult, 1.5f);
				drawInfo.colorArmorHead = OriginExtensions.Desaturate(drawInfo.colorArmorHead, lightSaturationMult);
				drawInfo.colorArmorBody = OriginExtensions.Desaturate(drawInfo.colorArmorBody, lightSaturationMult);
				drawInfo.colorArmorLegs = OriginExtensions.Desaturate(drawInfo.colorArmorLegs, lightSaturationMult);
				drawInfo.floatingTubeColor = OriginExtensions.Desaturate(drawInfo.floatingTubeColor, lightSaturationMult);
				drawInfo.itemColor = OriginExtensions.Desaturate(drawInfo.itemColor, lightSaturationMult);

				drawInfo.headGlowColor = OriginExtensions.Desaturate(drawInfo.headGlowColor, saturationMult);
				drawInfo.armGlowColor = OriginExtensions.Desaturate(drawInfo.armGlowColor, saturationMult);
				drawInfo.bodyGlowColor = OriginExtensions.Desaturate(drawInfo.bodyGlowColor, saturationMult);
				drawInfo.legsGlowColor = OriginExtensions.Desaturate(drawInfo.legsGlowColor, saturationMult);

				drawInfo.colorElectricity = OriginExtensions.Desaturate(drawInfo.colorElectricity, saturationMult);
				drawInfo.ArkhalisColor = OriginExtensions.Desaturate(drawInfo.ArkhalisColor, saturationMult);

				drawInfo.colorHair = OriginExtensions.Desaturate(drawInfo.colorHair, saturationMult);
				drawInfo.colorHead = OriginExtensions.Desaturate(drawInfo.colorHead, saturationMult);
				drawInfo.colorEyes = Color.Lerp(drawInfo.colorEyes, Color.White, 1f - saturationMult);
				drawInfo.colorEyeWhites = Color.Lerp(drawInfo.colorEyeWhites, Color.Black, 1f - saturationMult);
				drawInfo.colorBodySkin = OriginExtensions.Desaturate(drawInfo.colorBodySkin, saturationMult);

			}
			if (drawInfo.drawPlayer.shield == Resin_Shield.ShieldID && resinShieldCooldown > 0) {
				drawInfo.drawPlayer.shield = (sbyte)Resin_Shield.InactiveShieldID;
			}
			if (tornCurrentSeverity > 0 && !drawingTorn) {
				cachedTornPlayers.Add(Player);
				anyActiveTorn = true;
			}
		}
		public override void FrameEffects() {
			for (int i = 13; i < 18 + Player.extraAccessorySlots; i++) {
				if (Player.armor[i].type == Plague_Texan_Sight.ID) Plague_Texan_Sight.ApplyVisuals(Player);
			}
		}
	}
}
