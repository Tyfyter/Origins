using Origins.Dev;
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
	public class Defiled_Wisp : Glowing_Mod_NPC, IWikiNPC {
		public Rectangle DrawRect => new(0, -2, 24, 26);
		public int AnimationFrames => 24;
		public int FrameDuration => 1;
		public AssimilationAmount? Assimilation => 0.01f;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Demon_Eye;
			NPC.lifeMax = 12;
			NPC.damage = 10;
			NPC.defense = 1;
			NPC.width = 22;
			NPC.height = 22;
			NPC.knockBackResist = 0.2f;
			NPC.HitSound = SoundID.NPCHit36;
			NPC.DeathSound = SoundID.NPCDeath39;
			NPC.value = 500f;
			NPC.noTileCollide = true;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
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
			if (!NPC.HasValidTarget) NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.spriteDirection = NPC.direction;
		}
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter > 5) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 24) % 96, 22, 24);
				NPC.frameCounter = 0;
			}
		}
	}
}
