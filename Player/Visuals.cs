using CalamityMod.Graphics.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Origins.Buffs;
using Origins.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Other.Dyes;
using Origins.Items.Tools.Wiring;
using Origins.Items.Vanity.Dev.PlagueTexan;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Ranged;
using Origins.Layers;
using Origins.Tiles.Ashen;
using Origins.Tiles.Defiled;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins {
	public partial class OriginPlayer : ModPlayer {
		public override void HideDrawLayers(PlayerDrawSet drawInfo) {
			Item item = drawInfo.heldItem;
			if (item.ModItem is ICustomDrawItem custom && custom.HideNormalDraw) PlayerDrawLayers.HeldItem.Hide();
			PlayerDrawLayers.CaptureTheGem.Hide();

			if (mountOnly && !drawInfo.headOnlyRender) {
				for (int i = 0; i < PlayerDrawLayerLoader.DrawOrder.Count; i++) {
					PlayerDrawLayer layer = PlayerDrawLayerLoader.DrawOrder[i];
					if (layer != PlayerDrawLayers.MountFront && layer != PlayerDrawLayers.MountBack) {
						layer.Hide();
					}
				}
			}
			if (hideAllLayers) {
				foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers) {
					layer.Hide();
				}
			} else if (dashVaseVisual) {
				foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers) {
					if (layer is not Fallacious_Vase_Dash_Layer) layer.Hide();
				}
			}
		}
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
			if (weakShimmer) {
				drawInfo.colorHair = drawInfo.drawPlayer.GetImmuneAlpha(drawInfo.drawPlayer.GetHairColor(), drawInfo.shadow);
				drawInfo.colorEyeWhites = drawInfo.drawPlayer.GetImmuneAlpha(Color.White, drawInfo.shadow);
				drawInfo.colorEyes = drawInfo.drawPlayer.GetImmuneAlpha(drawInfo.drawPlayer.eyeColor, drawInfo.shadow);
				drawInfo.colorHead = drawInfo.drawPlayer.GetImmuneAlpha(drawInfo.drawPlayer.skinColor, drawInfo.shadow);
				drawInfo.colorBodySkin = drawInfo.drawPlayer.GetImmuneAlpha(drawInfo.drawPlayer.skinColor, drawInfo.shadow);
				drawInfo.colorLegs = drawInfo.drawPlayer.GetImmuneAlpha(drawInfo.drawPlayer.skinColor, drawInfo.shadow);
				drawInfo.colorShirt = drawInfo.drawPlayer.GetImmuneAlphaPure(drawInfo.drawPlayer.shirtColor, drawInfo.shadow);
				drawInfo.colorUnderShirt = drawInfo.drawPlayer.GetImmuneAlphaPure(drawInfo.drawPlayer.underShirtColor, drawInfo.shadow);
				drawInfo.colorPants = drawInfo.drawPlayer.GetImmuneAlphaPure(drawInfo.drawPlayer.pantsColor, drawInfo.shadow);
				drawInfo.colorShoes = drawInfo.drawPlayer.GetImmuneAlphaPure(drawInfo.drawPlayer.shoeColor, drawInfo.shadow);
				drawInfo.colorArmorHead = drawInfo.drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow);
				drawInfo.colorArmorBody = drawInfo.drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow);
				drawInfo.colorMount = drawInfo.colorArmorBody;
				drawInfo.colorArmorLegs = drawInfo.drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow);
				drawInfo.floatingTubeColor = drawInfo.drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow);
			}

			if (cursedCrownVisual) {
				drawInfo.skinDyePacked = GameShaders.Armor.GetShaderIdFromItemId(ItemID.BlueAcidDye);
				const float alphaMult = 0.9f;
				drawInfo.colorHead *= alphaMult;
				drawInfo.colorBodySkin *= alphaMult;
				drawInfo.colorLegs *= alphaMult;
			}
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
			if (tornCurrentSeverity > 0 && !GraphicsUtils.drawingEffect) {
				Torn_Debuff.cachedTornPlayers.Add(Player);
				Torn_Debuff.anyActiveTorn = true;
			}
			if (blizzardwalkerJacketVisual && blizzardwalkerActiveTime > 0) {
				float progress = blizzardwalkerActiveTime / (float)Blizzardwalkers_Jacket.max_active_time;
				drawInfo.colorEyes = Color.Lerp(drawInfo.colorEyes, Color.Red, progress * progress);
			}
			if (gasMask && OriginsModIntegrations.CheckAprilFools()) {
				drawInfo.drawPlayer.face = ModContent.GetInstance<Gas_Mask>().Item.faceSlot;
			}
			if (drawInfo.drawPlayer.front > 0 && OriginsSets.Armor.Front.DrawsInNeckLayer[drawInfo.drawPlayer.front]) drawInfo.drawFrontAccInNeckAccLayer = true;

			if (lunaticsRuneCharge > 0) {
				float charge = lunaticsRuneCharge / (float)Lunatics_Rune.ChargeThreshold;
				drawInfo.colorEyeWhites = Color.Lerp(drawInfo.colorEyeWhites, Color.White, charge);
				Vector3 hsl = Main.rgbToHsl(drawInfo.colorEyes);
				hsl.Y = float.Lerp(hsl.Y, float.Pow(hsl.Y, 0.5f), charge);
				hsl.Z = float.Lerp(hsl.Z, 1 - 0.5f * hsl.Y, charge);
				drawInfo.colorEyes = Main.hslToRgb(hsl);
			}
			for (int i = 0; i < (ShadowType.currentlyDrawing?.Length ?? 0); i++) {
				ShadowType.currentlyDrawing[i].ModifyDrawInfo(ref drawInfo);
			}
		}
		public override void FrameEffects() {
			Debugging.LogFirstRun(FrameEffects);
			for (int i = 13; i < 18 + Player.extraAccessorySlots * 2; i++) {
				if (!Player.armor.IndexInRange(i)) break;
				if (Player.armor[i].type == Plague_Texan_Sight.ID) Plague_Texan_Sight.ApplyVisuals(Player);
			}
			if (shineSparkCharge > 0 || shineSparkDashTime > 0) {
				Player.armorEffectDrawShadow = true;
			}
		}
		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
			if (blizzardwalkerJacketVisual && blizzardwalkerActiveTime > 0) {
				float progress = blizzardwalkerActiveTime / (float)Blizzardwalkers_Jacket.max_active_time;
				progress *= progress;
				float progressInvColor = 1 - (progress * 0.8f);
				r = r * (1 - progress) * progressInvColor + (50 / 255f) * progress * 0.8f;
				g = g * (1 - progress) * progressInvColor;
				b = b * (1 - progress) * progressInvColor + (160 / 255f) * progress * 0.8f;
				a = a * (1 - progress) * progressInvColor + progress * 0.9f;
			}
			if (shineSparkCharge > 0 || shineSparkDashTime > 0) {
				fullBright = true;
				for (int i = 0; i < 3; i++) {
					const float speed_mult = -1.5f;
					int dust = Dust.NewDust(Player.position, Player.width, Player.height, DustID.VilePowder, Player.velocity.X * speed_mult, Player.velocity.Y * speed_mult);
					Main.dust[dust].noGravity = true;
					drawInfo.DustCache.Add(dust);
				}
			}
		}
		public override void PostUpdateRunSpeeds() {
			Debugging.LogFirstRun(PostUpdateRunSpeeds);
			oldGravDir = Player.gravDir;
			if (forceFallthrough) Player.GoingDownWithGrapple = true;
			forceFallthrough = false;
			if (murkySludge) Player.accRunSpeed = Player.maxRunSpeed;
			Player.runAcceleration *= moveSpeedMult;
			Player.maxRunSpeed *= moveSpeedMult;
			Player.accRunSpeed *= moveSpeedMult;
			if (cursedCrown && Player.velocity.Y == 0) {
				Player.maxRunSpeed *= 0.9f;
				Player.accRunSpeed *= 0.9f;
			}
			if (dashVase && Player.dashDelay < 0) {
				const float factor = 1.5f;
				Player.accRunSpeed *= factor;
				Player.maxRunSpeed *= factor;
			}
			int brambleType = ModContent.TileType<Tangela_Bramble>();
			foreach (Point point in Collision.GetTilesIn(Player.position, Player.BottomRight)) {
				if (Framing.GetTileSafely(point).TileIsType(brambleType)) {
					Tangela_Bramble.StandInside(Player);
					break;
				}
			}
		}
		public override void ModifyZoom(ref float zoom) {
			if (Main.mouseRight && Player.HeldItem?.ModItem is Shimmershot or Laser_Target_Locator) {
				if (zoom == -1) zoom = 0;
				zoom += 0.5f;
			}
		}
		public override void TransformDrawData(ref PlayerDrawSet drawInfo) {
			int oldForcedShader = Origins.forcePlayerShader;
			bool resetKeepPlayerShader = Origins.keepPlayerShader == -1;
			if (Origins.drawPlayersWithShader >= 0) {
				Origins.forcePlayerShader = Origins.drawPlayersWithShader;
				if (Origins.drawPlayersWithShader == Origins.coordinateMaskFilterID) {
					Origins.coordinateMaskFilter.Shader.Parameters["uOffset"].SetValue(Player.position);
					Origins.coordinateMaskFilter.Shader.Parameters["uScale"].SetValue(1f);
					Origins.coordinateMaskFilter.UseColor(new Vector3(tornOffset, tornCurrentSeverity));
				}
			} else if (oldForcedShader == -1) {
				if (VisualRasterizedTime > 0) {
					if (resetKeepPlayerShader) Origins.keepPlayerShader = Anti_Gray_Dye.ShaderID;
					Origins.forcePlayerShader = Rasterized_Dye.ShaderID;
				} else if (shineSparkCharge > 0 || shineSparkDashTime > 0) {
					Origins.forcePlayerShader = Shimmer_Dye.ShaderID;
				} else {
					List<VisualEffectPlayer.VisualEffect> effects = Player.GetModPlayer<VisualEffectPlayer>().effects;
					for (int i = 0; i < effects.Count; i++) {
						if (effects[i].SetForcedShader()) break;
					}
				}
			}
			if (Origins.forcePlayerShader >= 0) {
				for (int i = 0; i < drawInfo.DrawDataCache.Count; i++) {
					if (drawInfo.DrawDataCache[i].shader != Origins.keepPlayerShader) drawInfo.DrawDataCache[i] = drawInfo.DrawDataCache[i] with { shader = Origins.forcePlayerShader };
				}
			}
			for (int i = 0; i < (ShadowType.currentlyDrawing?.Length ?? 0); i++) {
				ShadowType.currentlyDrawing[i].TransformDrawData(ref drawInfo);
			}
			Origins.forcePlayerShader = oldForcedShader;
			if (resetKeepPlayerShader) Origins.keepPlayerShader = -1;
		}
		public override void DrawPlayer(Camera camera) {
			if (amebicVialVisible) {

				const float offset = 2;
				Origins.forcePlayerShader = Origins.amebicProtectionShaderID;
				int itemAnimation = Player.itemAnimation;

				Origins.amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(offset, 0));
				Main.PlayerRenderer.DrawPlayer(camera, Player, Player.position + new Vector2(offset, 0), 0, default, 0.01f);

				Origins.amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(-offset, 0));
				Main.PlayerRenderer.DrawPlayer(camera, Player, Player.position + new Vector2(-offset, 0), 0, default, 0.01f);

				Origins.amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(0, offset));
				Main.PlayerRenderer.DrawPlayer(camera, Player, Player.position + new Vector2(0, offset), 0, default, 0.01f);

				Origins.amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(0, -offset));
				Main.PlayerRenderer.DrawPlayer(camera, Player, Player.position + new Vector2(0, -offset), 0, default, 0.01f);

				Origins.forcePlayerShader = -1;
				Player.itemAnimation = itemAnimation;
			}
			ShadowType.Draw(camera, Player);
		}

		internal void UpdateMurkySludgeSounds() {
			int sludge = ModContent.TileType<Murky_Sludge>();
			int grass = ModContent.TileType<Ashen_Murky_Sludge_Grass>();
			Rectangle hitbox = Player.Hitbox;
			hitbox.Inflate(1, 1);
			if (hitbox.OverlapsAnyTiles(out List<Point> intersectingTiles)) {
				for (int i = 0; i < intersectingTiles.Count; i++) {
					Tile tile = Main.tile[intersectingTiles[i]];
					if (tile.HasTile && (tile.TileType == sludge || tile.TileType == grass)) {
						touchingMurkySludges.Add(intersectingTiles[i]);
					}
				}
			}
			if ((murkySludgeTouchTimer == 0 || touchingMurkySludges.Count == 0) && !touchingMurkySludges.SetEquals(touchedMurkySludges)) {
				SoundEngine.PlaySound(Origins.Sounds.Glorp, Player.Bottom);
				murkySludgeTouchTimer = 15 - Math.Clamp(Math.Abs(Player.velocity.X) - 2, 0, 5);
			}
			murkySludgeTouchTimer.Cooldown();
			Utils.Swap(ref touchingMurkySludges, ref touchedMurkySludges);
			touchingMurkySludges.Clear();
		}
	}
	public class VisualEffectPlayer : ModPlayer {
		public List<VisualEffect> effects = [];
		public override void ResetEffects() {
			for (int i = effects.Count - 1; i >= 0; i--) {
				effects[i].ResetEffects();
				if (!effects[i].active) effects.RemoveAt(i);
			}
		}
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
			for (int i = 0; i < effects.Count; i++) {
				effects[i].ModifyDrawInfo(ref drawInfo);
			}
		}
		public override void FrameEffects() {
			for (int i = 0; i < effects.Count; i++) {
				effects[i].FrameEffects(Player);
			}
		}
		public abstract class VisualEffect {
			public bool active = true;
			public virtual void ResetEffects() { }
			public virtual bool SetForcedShader() => false;
			public virtual void ModifyDrawInfo(ref PlayerDrawSet drawInfo) { }
			public virtual void FrameEffects(Player player) { }
		}
	}
	[ReinitializeDuringResizeArrays]
	static class ShadowTypeLoader {
		static ShadowTypeLoader() {
			ShadowType.Sort();
		}
	}
	public abstract class ShadowType : ModType {
		internal static ShadowType[] currentlyDrawing;
		public static int ShadowTypeCount => shadowTypes.Count;
		public static ShadowType PartialEffects {  get; private set; }
		static readonly List<ShadowType> shadowTypes = [];
		public int Type { get; private set; }
		public virtual IEnumerable<ShadowType> SortAbove() => [];
		public virtual IEnumerable<ShadowType> SortBelow() => [];
		public abstract IEnumerable<ShadowData> GetShadowData(Player player, ShadowData from);
		public virtual void ModifyDrawInfo(ref PlayerDrawSet drawInfo) { }
		public virtual void TransformDrawData(ref PlayerDrawSet drawInfo) { }
		public static void Draw(Camera camera, Player player) {
			bool[] activeShadows = player.OriginPlayer().activeShadows;
			Queue<ShadowData> sourceQueue = [];
			Queue<ShadowData> drawQueue = [];
			sourceQueue.Enqueue(new(player.position + Vector2.UnitY * player.gfxOffY, player.direction, Rotation: player.fullRotation, RotationOrigin: player.fullRotationOrigin, ShadowTypes: []));
			for (int i = 0; i < shadowTypes.Count; i++) {
				if (activeShadows[i]) {
					while (sourceQueue.TryDequeue(out ShadowData source)) {
						ShadowType[] shadowStack = [..source.ShadowTypes, shadowTypes[i]];
						foreach (ShadowData item in shadowTypes[i].GetShadowData(player, source)) {
							drawQueue.Enqueue(item with { ShadowTypes = shadowStack });
						}
						drawQueue.Enqueue(source);
					}
					Utils.Swap(ref sourceQueue, ref drawQueue);
				}
			}
			int oldForcedShader = Origins.forcePlayerShader;
			int direction = player.direction;
			while (sourceQueue.TryDequeue(out ShadowData source)) {
				if (source.ShadowTypes is null || source.ShadowTypes.Length == 0) continue;
				Origins.forcePlayerShader = source.Shader;
				currentlyDrawing = source.ShadowTypes;
				player.direction = source.Direction;
				source.PreDraw?.Invoke();
				Main.PlayerRenderer.DrawPlayer(camera, player, source.Position, source.Rotation, source.RotationOrigin, source.Shadow, source.Scale);
			}
			player.direction = direction;
			currentlyDrawing = null;
			Origins.forcePlayerShader = oldForcedShader;
		}
		internal static void Sort() {
			List<ShadowType> loadedTypes = new TopoSort<ShadowType>(shadowTypes,
				mode => mode.SortBelow(),
				mode => mode.SortAbove()
			).Sort();
			shadowTypes.Clear();
			shadowTypes.AddRange(loadedTypes);
			for (int i = 0; i < shadowTypes.Count; i++) {
				shadowTypes[i].Type = i;
				if (shadowTypes[i] is Separator) shadowTypes.RemoveAt(i--);
			}
		}
		protected sealed override void Register() {
			shadowTypes.Add(this);
			ModTypeLookup<ShadowType>.Register(this);
		}
		public record struct ShadowData(Vector2 Position, int Direction, int Shader = -1, float Rotation = 0, Vector2 RotationOrigin = default, float Shadow = 0f, float Scale = 1f, Action PreDraw = null, ShadowType[] ShadowTypes = default);
		class Separator : ShadowType {
			public override IEnumerable<ShadowData> GetShadowData(Player player, ShadowData from) => throw new NotImplementedException();
		}
		class EffectSeparator : Separator {
			public override void Load() => PartialEffects = this;
		}
	}
}
