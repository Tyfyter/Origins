using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Other.Fish;
using Origins.NPCs.Riven;
using Origins.World.BiomeData;
using rail;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Critters {
	public class Amoeba_Buggy : Glowing_Mod_NPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 18, 12);
		public int AnimationFrames => 4;
		public int FrameDuration => 8;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override void SetStaticDefaults() {
			Main.npcCatchable[Type] = true;
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = new(0, 22),
				PortraitPositionYOverride = 42
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.BlueDragonfly);
			NPC.catchItem = ModContent.ItemType<Amoeba_Buggy_Item>();
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type
			];
		}
		public override void FindFrame(int frameHeight) {
			NPC.frameCounter += 0.75f;
			NPC.frameCounter += NPC.velocity.LengthSquared() / 16;
			NPC.spriteDirection = Math.Sign(NPC.velocity.X);
			if (NPC.frameCounter >= 7) {
				NPC.frameCounter = 0;
				if ((NPC.frame.Y += NPC.frame.Height) / NPC.frame.Height >= Main.npcFrameCount[Type]) {
					NPC.frame.Y = 0;
				}
			}
		}
	}
	public class Bug : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 20, 12);
		public int AnimationFrames => 2;
		public int FrameDuration => 8;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override void SetStaticDefaults() {
			Main.npcCatchable[Type] = true;
			Main.npcFrameCount[Type] = 2;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.GoldWaterStrider);
			NPC.catchItem = ModContent.ItemType<Bug_Item>();
		}
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter >= 7) {
				NPC.frameCounter = 0;
				if ((NPC.frame.Y += NPC.frame.Height) / NPC.frame.Height >= Main.npcFrameCount[Type]) {
					NPC.frame.Y = 0;
				}
				if (NPC.velocity.X != 0) NPC.spriteDirection = Math.Sign(NPC.velocity.X);
			}
		}
		public override void AI() {
			if (NPC.frameCounter == 0) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					if (i == NPC.whoAmI) continue;
					NPC other = Main.npc[i];
					if (other.active && NPC.Hitbox.Intersects(other.Hitbox)) {
						other.spriteDirection *= -1;
					}
				}
				for (int i = 0; i < Main.maxPlayers; i++) {
					Player target = Main.player[i];
					if (target.active && NPC.Hitbox.Intersects(target.Hitbox)) {
						target.gravDir *= -1;
						target.AddBuff(BuffID.Gravitation, 30);
					}
				}
			}
			if (Main.rand.NextBool(100)) {
				int tries = 0;
				float range = 16 * 10;
				Rectangle checkbox = NPC.Hitbox;
				while (++tries < 1000) {
					Vector2 pos = NPC.position + new Vector2(Main.rand.NextFloat(-range, range), Main.rand.NextFloat(-range, range));
					checkbox.X = (int)pos.X;
					checkbox.Y = (int)pos.Y;
					if (!checkbox.OverlapsAnyTiles()) {
						NPC.Teleport(pos, -1);
						break;
					}
				}
			}
		}
	}
	public class Cicada_3301 : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 20, 13);
		public int AnimationFrames => 2;
		public int FrameDuration => 8;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override void SetStaticDefaults() {
			Main.npcCatchable[Type] = true;
			Main.npcFrameCount[Type] = 2;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Buggy);
			NPC.catchItem = ModContent.ItemType<Cicada_3301_Item>();
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
		}
		public override void FindFrame(int frameHeight) {
			if (Main.rand.NextBool(350)) {
				SoundEngine.PlaySound(Origins.Sounds.Amalgamation.WithPitch(1).WithVolumeScale(0.5f), NPC.Center);
			}
			NPC.spriteDirection = Math.Sign(NPC.velocity.X);
			if (++NPC.frameCounter >= 7) {
				NPC.frameCounter = 0;
				if ((NPC.frame.Y += NPC.frame.Height) / NPC.frame.Height >= Main.npcFrameCount[Type]) {
					NPC.frame.Y = 0;
				}
			}
		}
	}
}
