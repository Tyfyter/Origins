using Origins.Items.Weapons.Summoner;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
	public class Crimbrain : ModNPC {
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 4;
			CrimsonGlobalNPC.NPCTypes.Add(Type);
			CrimsonGlobalNPC.AssimilationAmounts.Add(Type, 0.04f);
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = new(0, -16),
				PortraitPositionYOverride = -32
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DemonEye);
			NPC.aiStyle = 86;
			NPC.lifeMax = 36;
			NPC.defense = 10;
			NPC.damage = 25;
			NPC.width = 34;
			NPC.height = 28;
			NPC.friendly = false;
			NPC.knockBackResist = 1.5f;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerFloorY > Main.worldSurface + 50 || spawnInfo.SpawnTileY >= Main.worldSurface - 50) return 0;
			if (!spawnInfo.Player.ZoneCrimson) return 0;
			return 0.1f * (spawnInfo.Player.ZoneSkyHeight ? 2 : 1);
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			target.AddBuff(BuffID.Confused, 20);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Vertebrae));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Brainy_Staff>(), 17));
		}
		public override void AI() {
			if (!NPC.HasValidTarget) NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.spriteDirection = NPC.direction;
			NPC.knockBackResist = 1.5f;
			NPC.velocity *= 0.97f;
		}
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter > 7) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 28) % 112, 34, 26);
				NPC.frameCounter = 0;
			}
		}
	}
}
