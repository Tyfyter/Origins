using Microsoft.Xna.Framework;
using Origins.Items.Armor.Riven;
using Origins.Items.Materials;
using Origins.Items.Weapons.Demolitionist;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class Spider_Amoeba : Glowing_Mod_NPC, IRivenEnemy {
		public AssimilationAmount? Assimilation => 0.04f;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 5;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		public override void SetDefaults() {// could not add stats because 
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.width = 68;
			NPC.height = 30;
			SetSharedDefaults();
		}
		public void SetSharedDefaults() {
			NPC.lifeMax = 81;
			NPC.defense = 10;
			NPC.damage = 33;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath24.WithPitch(0.6f);
			NPC.value = 90;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type
			];
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return spawnInfo.SpawnTileY < Main.worldSurface ? 0 : Riven_Hive.SpawnRates.LandEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.Spighter;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				this.GetBestiaryFlavorText(),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bud_Barnacle>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ameballoon>(), 1, 3, 4));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Mask>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Coat>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Pants>(), 525));
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.FaceTarget();
				NPC.spriteDirection = NPC.direction;
			}
			//increment frameCounter every frame and run the following code when it exceeds 7 (i.e. run the following code every 8 frames)
			
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			if (NPC.velocity.Y == 0f && NPC.NPCCanStickToWalls()) {
				NPC.Transform(ModContent.NPCType<Spider_Amoeba_Wall>());
			}
		}
		public override void FindFrame(int frameHeight) {
			if (NPC.velocity.Y == 0) NPC.DoFrames(7);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			//spawn gore if npc is dead after being hit
			if (NPC.life < 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(1, 4));
			}
		}
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
            OriginPlayer.InflictTorn(target, 180, targetSeverity: 1f - 0.85f);
        }
    }
	public class Spider_Amoeba_Wall : Spider_Amoeba {
		public override void Load() { }
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 4;
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
		}
		public override void SetDefaults() {// could not add stats because 
			NPC.CloneDefaults(NPCID.WallCreeperWall);
			NPC.aiStyle = NPCAIStyleID.Spider;
			NPC.width = 68;
			NPC.height = 68;
			SetSharedDefaults();
			this.CopyBanner<Spider_Amoeba>();
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0;
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bud_Barnacle>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ameballoon>(), 1, 3, 4));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Mask>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Coat>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Pants>(), 525));
		}
		public override void AI() {
			NPC.DoFrames(7);
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			if (!NPC.NPCCanStickToWalls()) {
				NPC.Transform(ModContent.NPCType<Spider_Amoeba>());
			}
		}
	}
}
