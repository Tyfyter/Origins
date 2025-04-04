using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
	public class Cannihound : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 82, 50);
		public int AnimationFrames => 120;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 15;
			CrimsonGlobalNPC.NPCTypes.Add(Type);
			AssimilationLoader.AddNPCAssimilation<Crimson_Assimilation>(Type, 0.04f);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Unicorn;
			NPC.lifeMax = 198;
			NPC.defense = 20;
			NPC.damage = 25;
			NPC.width = 54;
			NPC.height = 50;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.7f;
			NPC.value = 75;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerFloorY > Main.worldSurface + 50 || spawnInfo.SpawnTileY >= Main.worldSurface - 50) return 0;
			if (!spawnInfo.Player.ZoneCrimson) return 0;
			return 0.1f * (spawnInfo.Player.ZoneSkyHeight ? 2 : 1) * (spawnInfo.Player.ZoneSnow ? 1.5f : 1);
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			if (Main.rand.NextBool()) target.AddBuff(BuffID.Bleeding, 20);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Vertebrae));
		}
		public override void AI() {
			if (!NPC.HasValidTarget) NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.spriteDirection = NPC.direction;
			NPC.knockBackResist = 1.5f;
		}
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter > 2) {
				int toFrame = NPC.frame.Y + 50 % 750;
				if (toFrame >= 250) toFrame = 0;
				NPC.frame = new Rectangle(0, toFrame, 82, 50);
				NPC.frameCounter = 0;
			}
		}
	}
}
