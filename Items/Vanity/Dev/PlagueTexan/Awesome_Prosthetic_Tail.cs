using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Items.Accessories;
using Origins.Items.Vanity.Other;
using Origins.NPCs;
using Origins.NPCs.Defiled;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Vanity.Dev.PlagueTexan {
	public class Awesome_Prosthetic_Tail : ATail, IStackableAccessory {
		AutoLoadingAsset<Texture2D> tailTexture = typeof(Awesome_Prosthetic_Tail).GetDefaultTMLName("_Tail");
		AutoLoadingAsset<Texture2D> tailGlowTexture = typeof(Awesome_Prosthetic_Tail).GetDefaultTMLName("_Tail_Glow");
		public static DyedLight dyeableGlow = new(new(0.0845f, 0.3625f, 0.5f));
		public override int Length => 3 * Item.stack;
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 22);
			Item.rare = AltCyanRarity.ID;
			Item.value = Item.sellPrice(gold: 1);
			Item.maxStack = 999;
		}
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
		}
		internal Vector3 glowColor;
		internal ulong rateLimitTimer;
		public override void UpdateTail(Player player, List<TailSegment> tailSegments) {
			for (int i = 0; i < 2; i++) base.UpdateTail(player, tailSegments);
		}
		public override void UpdateTailSegment(Player player, IReadOnlyList<TailSegment> segments, int index) {
			DoBasicUpdate(player, segments, index, index > 1 ? 12 : 1);
			Awesome_Prosthetic_Tail_Player aptPlayer = player.GetModPlayer<Awesome_Prosthetic_Tail_Player>();
			if (index == 0) {
				switch (aptPlayer.mood) {
					case Mood.Happy:
					segments[index].rotation += (Utils.PingPongFrom01To010(player.miscCounter / 17f % 1) * 2 - 1) * 2f;
					break;
					default:
					segments[index].rotation += player.direction * (player.controlUp.ToInt() - player.controlDown.ToInt()) * 0.75f;
					break;
				}
			}
			dyeableGlow.GetColorWithRateLimit(ref glowColor, ref rateLimitTimer, aptPlayer.GetGlowDye, player, player);
			Lighting.AddLight(segments[index].position, glowColor);
		}
		public override void DrawTailSegment(ref PlayerDrawSet drawInfo, IReadOnlyList<TailSegment> segments, int index) {
			if (index == 0) return;
			TailSegment segment = segments[index];
			Rectangle frame = ((index - 1) * 3 / (segments.Count - 1)) switch {
				0 => new(0, 0, 10, 14),
				1 => new(12, 0, 8, 14),
				2 => new(22, 0, 10, 14),
				_ => new(0, 0, 0, 0)
			};
			Awesome_Prosthetic_Tail_Player colors = drawInfo.drawPlayer.GetModPlayer<Awesome_Prosthetic_Tail_Player>();
			DrawData data = new(
				tailTexture,
				segment.position - Main.screenPosition,
				frame,
				Lighting.GetColor(segment.position.ToTileCoordinates()),
				segment.rotation + MathHelper.PiOver2,
				frame.Size() * 0.5f,
				1,
				segment.effects
			) {
				shader = colors.cTail
			};
			drawInfo.DrawDataCache.Add(data);
			if (colors.cTailGlow is int cTailGlow) data.shader = cTailGlow;
			data.texture = tailGlowTexture;
			data.color = Color.White;
			drawInfo.DrawDataCache.Add(data);
		}
		public override void UpdateItemDye(Player player, int dye, bool hideVisual) {
			player.GetModPlayer<Awesome_Prosthetic_Tail_Player>().cTail = dye;
		}
		public class Awesome_Tail_Glow_Dye_Slot : ExtraDyeSlot {
			public override bool UseForSlot(Item equipped, Item vanity, bool equipHidden) => equipped?.ModItem is Awesome_Prosthetic_Tail || vanity?.ModItem is Awesome_Prosthetic_Tail;
			public override void ApplyDye(Player player, [NotNull] Item dye) {
				player.GetModPlayer<Awesome_Prosthetic_Tail_Player>().cTailGlow = dye.dye;
			}
		}
		public enum Mood {
			None,
			Happy
		}
		public class Awesome_Prosthetic_Tail_Player : ModPlayer {
			[AutoReset] public int cTail;
			[AutoReset] public int? cTailGlow;
			[AutoReset] public Mood mood;
			public override void Load() => autoReset = AutoResetAttribute.GenerateReset<Awesome_Prosthetic_Tail_Player>();
			static Action<Awesome_Prosthetic_Tail_Player> autoReset;
			public override void ResetEffects() {
				autoReset(this);
				Mood maxMood = Enum.GetValues<Mood>()[^1];
				void NewMood(Mood mood) {
					if (this.mood < mood) this.mood = mood;
				}
				foreach (NPC npc in Main.ActiveNPCs) {
					if (mood >= maxMood) break;
					if (!npc.IsWithin(Player, 16 * 35)) continue;
					switch (npc.type) {
						case NPCID.TownCat:
						case NPCID.BestiaryGirl:
						case NPCID.PartyGirl:
						NewMood(Mood.Happy);
						continue;
					}
					if (npc.ModNPC is Defiled_Chrysalis) {
						NewMood(Mood.Happy);
						continue;
					}
					switch (npc.GivenName) {
						case "Chrysalis":
						case "Luna":
						case "Celestia":
						NewMood(Mood.Happy);
						continue;
					}
				}
				foreach (Projectile projectile in Main.ActiveProjectiles) {
					if (mood >= maxMood) break;
					if (!projectile.IsWithin(Player, 16 * 35)) continue;
					switch (projectile.type) {
						case ProjectileID.BlackCat:
						case ProjectileID.Turtle:
						NewMood(Mood.Happy);
						continue;
					}
				}
			}
			public int? GetGlowDye() => cTailGlow ?? cTail;
		}
	}
}
