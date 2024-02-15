using Origins.Buffs;
using Origins.Items.Weapons.Demolitionist;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class Amebic_Slime : Glowing_Mod_NPC, IRivenEnemy {
		public override string GlowTexturePath => Texture;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 2;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Crimslime);
			NPC.lifeMax = 60;
			NPC.defense = 5;
			NPC.damage = 30;
			NPC.width = 32;
			NPC.height = 24;
			NPC.friendly = false;
			NPC.value = 40;
        }
		public override void FindFrame(int frameHeight) {
			NPC.CloneFrame(NPCID.Crimslime, frameHeight);
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Riven_Hive.SpawnRates.LandEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.AmebSlime;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				this.GetBestiaryFlavorText("With its viscosity unchanged, it behaves like most slimes... the only difference is that it tends to easily digest whatever it absorbs."),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Gel, 1, 2, 4));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ameballoon>(), 1, 3, 6));
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			OriginPlayer.InflictTorn(target, 180, targetSeverity: 1f - 0.9f);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life < 0) {
				for (int i = 0; i < 5; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Origins.instance.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4)));
			}
		}
	}
}
