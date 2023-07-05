using Origins.Items.Other.Fish;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Critters {
	public class Amoeba_Buggy : ModNPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Amoeba Buggy");
			Main.npcCatchable[Type] = true;
			Main.npcFrameCount[Type] = 4;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.BlueDragonfly);
			NPC.catchItem = ModContent.ItemType<Amoeba_Buggy_Item>();
		}
		public override void AI() {
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
	public class Bug : ModNPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Bug");
			Main.npcCatchable[Type] = true;
			Main.npcFrameCount[Type] = 2;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.GoldWaterStrider);
			NPC.catchItem = ModContent.ItemType<Bug_Item>();
		}
		public override void AI() {
			if (++NPC.frameCounter >= 7) {
				NPC.frameCounter = 0;
				if ((NPC.frame.Y += NPC.frame.Height) / NPC.frame.Height >= Main.npcFrameCount[Type]) {
					NPC.frame.Y = 0;
				}
				if (NPC.velocity.X != 0) NPC.spriteDirection = Math.Sign(NPC.velocity.X);
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
		}
	}
	public class Cicada_3301 : ModNPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cicada 3301");
			Main.npcCatchable[Type] = true;
			Main.npcFrameCount[Type] = 2;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Buggy);
			NPC.catchItem = ModContent.ItemType<Cicada_3301_Item>();
		}
		public override void AI() {
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
