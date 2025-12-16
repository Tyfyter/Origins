using Origins.NPCs;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.UI.Chat;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Localization;
using System.Collections.Generic;
using Origins.Graphics;
using Terraria.Graphics.Effects;
using Origins.Items.Accessories;
using PegasusLib;
using PegasusLib.Graphics;
using PegasusLib.UI;

namespace Origins.Buffs {
	public class Torn_Debuff : ModBuff {
		public static int ID { get; private set; }
		public LocalizedText EffectDescription;
		public LocalizedText EffectDescriptionSpecific;
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			EffectDescription = this.GetLocalization(nameof(EffectDescription));
			EffectDescriptionSpecific = this.GetLocalization(nameof(EffectDescriptionSpecific));
			Buff_Hint_Handler.CombineBuffHintModifiers(Type, modifyBuffTip: (lines, item, player) => {
				lines.Add(!player && (item?.ModItem is ITornSource tornSource) ? EffectDescriptionSpecific.Format(tornSource.Severity) : EffectDescription.Value);
			});
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().tornDebuff = true;
			if (player.buffTime[buffIndex] <= 1) {
				player.buffTime[buffIndex] = 10;
				player.buffType[buffIndex] = Torn_Decay_Debuff.ID;
			}
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().tornDebuff = true;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams) {
			Main.buffNoTimeDisplay[Type] = Main.LocalPlayer.GetModPlayer<OriginPlayer>().hideTornTime;
			return true;
		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			string text = $"{Main.LocalPlayer.GetModPlayer<OriginPlayer>().tornCurrentSeverity * 100:#0}%";
			ReLogic.Graphics.DynamicSpriteFont font = FontAssets.CombatText[0].Value;
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				font,
				text,
				drawParams.TextPosition + new Vector2(0, Main.buffNoTimeDisplay[Type] ? 0 : 16),
				new Color(50, 180, 230),
				0,
				font.MeasureString(text) * new Vector2(0f, 0f),
				new Vector2(0.55f)
			);
		}
		internal static List<Player> cachedTornPlayers;
		internal static List<NPC> cachedTornNPCs;
		internal static bool anyActiveTorn;
		public static ScreenTarget TornScreenTarget { get; private set; }
		public override void Load() {
			if (Main.dedServ) return;
			cachedTornPlayers = new();
			cachedTornNPCs = new();
			TornScreenTarget = new(
				MaskAura,
				() => {
					bool isActive = anyActiveTorn;
					anyActiveTorn = false;
					return isActive && Lighting.NotRetro;
				},
				1
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
				GraphicsUtils.drawingEffect = true;
				//spriteBatch.End();
				//spriteBatch.Restart(state);
				Origins.drawPlayersWithShader = Origins.coordinateMaskFilterID;
				Origins.coordinateMaskFilter.Shader.Parameters["uCoordinateSize"].SetValue(new Vector2(256, 256));//put the size of the texture in here

				PlayerShaderSet dontCoverArmor = new(0);
				Origins.keepPlayerShader = Origins.transparencyFilterID;
				dontCoverArmor.cHead = Origins.keepPlayerShader;
				for (int i = 0; i < cachedTornPlayers.Count; i++) {
					Player player = cachedTornPlayers[i];
					Origins.coordinateMaskFilter.UseColor(new Vector3(player.GetModPlayer<OriginPlayer>().tornOffset, 0));
					//Origins.coordinateMaskFilter.UseColor(new Vector3());
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
							scale: 1
						);
					} finally {
						player.itemLocation = itemLocation;
						shaderSet.Apply(player);
					}
				}
				if (cachedTornNPCs.Count > 0) {
					spriteBatch.Restart(state, effect: Origins.coordinateMaskFilter.Shader);
					EffectParameter worldPosition = Origins.coordinateMaskFilter.Shader.Parameters["uWorldPosition"];
					EffectParameter imageSize0 = Origins.coordinateMaskFilter.Shader.Parameters["uImageSize0"];
					EffectParameter sourceRect = Origins.coordinateMaskFilter.Shader.Parameters["uSourceRect"];
					EffectParameter scale = Origins.coordinateMaskFilter.Shader.Parameters["uScale"];
					EffectParameter offset = Origins.coordinateMaskFilter.Shader.Parameters["uOffset"];
					EffectParameter color = Origins.coordinateMaskFilter.Shader.Parameters["uColor"];
					for (int i = 0; i < cachedTornNPCs.Count; i++) {
						NPC npc = cachedTornNPCs[i];
						if (npc.active) {
							OriginGlobalNPC gNPC = npc.GetGlobalNPC<OriginGlobalNPC>();
							color.SetValue(new Vector3(gNPC.tornOffset, gNPC.tornCurrentSeverity));
							worldPosition.SetValue(npc.position);
							offset.SetValue(npc.position);
							sourceRect.SetValue(new Vector4(npc.frame.X, npc.frame.Y, npc.frame.Width, npc.frame.Height));
							imageSize0.SetValue(TextureAssets.Npc[npc.type].Size());
							scale.SetValue(npc.scale);
							Main.instance.DrawNPC(npc.whoAmI, npc.behindTiles);
						}
					}
				}
				//Main.PlayerRenderer.DrawPlayers(Main.Camera, cachedTornPlayers);
			} finally {
				Origins.drawPlayersWithShader = -1;
				Origins.keepPlayerShader = -1;
				cachedTornPlayers.Clear();
				cachedTornNPCs.Clear();
				GraphicsUtils.drawingEffect = false;
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
	}
	public interface ITornSource {
		float Severity { get; }
	}
	public class Torn_Decay_Debuff : Torn_Debuff {
		public static new int ID { get; private set; }
		public override string Texture => "Origins/Buffs/Torn_Debuff";
		public override LocalizedText DisplayName => Language.GetOrRegister($"Mods.Origins.{LocalizationCategory}.Torn_Debuff.DisplayName");
		public override LocalizedText Description => Language.GetOrRegister($"Mods.Origins.{LocalizationCategory}.Torn_Debuff.Description");
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			if (player.GetModPlayer<OriginPlayer>().tornCurrentSeverity <= 0) {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams) => true;
	}
}
