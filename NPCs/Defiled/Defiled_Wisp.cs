using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Demolitionist;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Defiled_Wisp : ModNPC {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 3;
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DungeonSpirit);
			NPC.aiStyle = NPCAIStyleID.Demon_Eye;
			NPC.lifeMax = 12;
			NPC.defense = 2;
			NPC.damage = 10;
			NPC.width = 22;
			NPC.height = 22;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit36;
			NPC.DeathSound = SoundID.NPCDeath39;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				this.GetBestiaryFlavorText("A rare entity that is released only once its former vessel has been dislodged from its bile interior."),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled_Spirit>(), 1, 1, 2));
			npcLoot.Add(ItemDropRule.ByCondition(new Conditions.PlayerNeedsHealing(), ItemID.Heart, 4));
		}
		public override void AI() {
			if (Main.rand.NextBool(3)) Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.Asphalt);
			NPCAimedTarget target = NPC.GetTargetData();
			NPC.rotation = NPC.AngleTo(target.Center) + MathHelper.PiOver2;
			if (Main.rand.NextBool(900)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(1f, 1.2f), NPC.Center);
			NPC.FaceTarget();
			if (!NPC.HasValidTarget) NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.spriteDirection = NPC.direction;
			if (++NPC.frameCounter > 5) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 24) % 96, 22, 24);
				NPC.frameCounter = 0;
			}
		}
	}
}
