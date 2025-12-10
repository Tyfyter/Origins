using Microsoft.Xna.Framework.Graphics;
using Origins.UI;
using PegasusLib;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.Items.Accessories {
	public class Stress_Ball : ModItem {
		public static int TimePerFrame => 1;
		public static Range CooldownRange => 450..900;
		public static float BuffDuration => 60 * 7f;
		public static float DecayDuration => 60 * 0.2f;
		public static int SqueezeCount => 12;
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 20);
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
		}
		static SlotId LoopSound;
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.stressBall = true;
			if (player.whoAmI != Main.myPlayer) return;
			if (originPlayer.stressBallTimer >= 0) {
				player.GetDamage(DamageClass.Generic) += 0.4f * originPlayer.stressBallStrength;
				if (MathUtils.LinearSmoothing(ref originPlayer.stressBallStrength, 0, 1f / BuffDuration)) originPlayer.stressBallTimer++;
				int chance = originPlayer.stressBallTimer - CooldownRange.Start.Value;
				if (chance > 0 && Main.rand.NextBool(chance, CooldownRange.End.Value - CooldownRange.Start.Value)) originPlayer.stressBallTimer = -1;
			} else {
				if (!LoopSound.IsValid || !SoundEngine.TryGetActiveSound(LoopSound, out ActiveSound loop)) {
					LoopSound = SoundEngine.PlaySound(Origins.Sounds.ShimmershotCharging, updateCallback: static sound => {
						OriginPlayer originPlayer = Main.LocalPlayer.OriginPlayer();
						if (originPlayer.stressBallTimer < 0) {
							sound.Pitch = originPlayer.stressBallStrength / SqueezeCount;
						} else {
							MathUtils.LinearSmoothing(ref sound.Volume, 0, 1f / 30);
						}
						return sound.Volume > 0;
					});
				}
				if (originPlayer.stressBallTimer < -1) originPlayer.stressBallTimer++;
				else MathUtils.LinearSmoothing(ref originPlayer.stressBallStrength, 0, 1f / DecayDuration);
				if (Keybindings.StressBall.Current) {
					if (originPlayer.stressBallTimer >= -1) {
						SoundStyle sound = SoundID.Duck;
						if (!OriginsModIntegrations.CheckAprilFools()) {
							switch (Main.rand.Next(3)) {
								case 0:
								sound = SoundID.NPCHit15;
								break;
								case 1:
								sound = SoundID.NPCHit16;
								break;
								case 2:
								sound = SoundID.NPCHit17;
								break;
							}
						}
						SoundEngine.PlaySound(sound);
					}
					originPlayer.stressBallTimer = -TimePerFrame * 4;
				}
				if (originPlayer.stressBallTimer == -2) {
					originPlayer.stressBallStrength = float.Ceiling(originPlayer.stressBallStrength + 1);
					if (originPlayer.stressBallStrength >= SqueezeCount) {
						originPlayer.stressBallStrength = 1;
						originPlayer.stressBallTimer = 0;
					}
				}
			}
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.SubstituteKeybind(Keybindings.StressBall);
		}
	}
	public class Stress_Ball_UI : SwitchableUIState {
		public AutoLoadingAsset<Texture2D> baseTexture = typeof(Stress_Ball_UI).GetDefaultTMLName();
		public AutoLoadingAsset<Texture2D>[] chargeTextures = new AutoLoadingAsset<Texture2D>[5];
		public override void AddToList() => OriginSystem.Instance.AccessoryHUD.AddState(this);
		public override bool IsActive() => OriginPlayer.LocalOriginPlayer?.stressBall ?? false;
		public Stress_Ball_UI() : base() {
			OverrideSamplerState = SamplerState.PointClamp;
			for (int i = 0; i < chargeTextures.Length; i++) chargeTextures[i] = typeof(Stress_Ball_UI).GetDefaultTMLName("_Charge" + (i + 1));
		}
		public override InterfaceScaleType ScaleType => InterfaceScaleType.Game;
		public override void Draw(SpriteBatch spriteBatch) {
			OriginPlayer originPlayer = Main.LocalPlayer.OriginPlayer();
			if (originPlayer.stressBallTimer >= 0) return;
			Rectangle frame = baseTexture.Frame(verticalFrames: 5, frameY: 4 + (originPlayer.stressBallTimer / Stress_Ball.TimePerFrame));
			DrawData data = new(baseTexture,
				Main.LocalPlayer.MountedCenter.Floor() - new Vector2(Main.LocalPlayer.direction * 48, -Main.LocalPlayer.gfxOffY) - frame.Size() * 0.5f - Main.screenPosition,
				frame,
				Color.White
			);
			data.Draw(spriteBatch);
			float strength = float.Ceiling(originPlayer.stressBallStrength);
			int meterFrame = (int)((strength * (chargeTextures.Length + 1)) / Stress_Ball.SqueezeCount) - 1;
			if (meterFrame < 0) return;
			data.texture = chargeTextures[int.Min(meterFrame, chargeTextures.Length - 1)];
			data.sourceRect = null;
			data.color *= 0.5f + (originPlayer.stressBallStrength + 1 - strength) * 0.5f;
			data.Draw(spriteBatch);
		}
	}
}
