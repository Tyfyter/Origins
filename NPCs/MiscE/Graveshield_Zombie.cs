using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Tiles.Other;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
    public class Graveshield_Zombie : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(2, 6, 36, 48);
		public int AnimationFrames => 24;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void SetStaticDefaults() {
			NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCID.BoneThrowingSkeleton;
			Main.npcFrameCount[NPC.type] = 3;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.lifeMax = 55;
			NPC.defense = 14;
			NPC.damage = 12;
			NPC.width = 28;
			NPC.height = 46;
			NPC.friendly = false;
			NPC.value = 100f;
			AIType = NPCID.Zombie;
			AnimationType = NPCID.Zombie;
			Banner = Item.NPCtoBanner(NPCID.Zombie);
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown) return 0;
			if ((spawnInfo.Player.ZoneGraveyard || !Main.dayTime) && spawnInfo.Player.ZoneForest) {
				return 0.05f;
			}
			return 0;
		}
		public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) {
			if (modifiers.HitDirection == -NPC.direction) {
				modifiers.Defense.Base += 6;
				modifiers.Knockback *= 0.25f;
			}
		}
		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (modifiers.HitDirection == -NPC.direction && Math.Sign(projectile.Center.X - NPC.Center.X) == NPC.direction) {
				modifiers.Defense.Base += 6;
				modifiers.Knockback *= 0.25f;
			}
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Graveyard,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(new LeadingConditionRule(new ShieldDropCondition()).WithOnSuccess(ItemDropRule.OneFromOptions(1, 
				ItemID.Gravestone,
				ItemID.Tombstone,
				ItemID.Headstone,
				ItemID.CrossGraveMarker,
				ItemID.Obelisk
			)));
		}
		public override void OnKill() {
			if (!OriginConfig.GraveshieldZombiesShouldDropAsItem) NPC.DropTombstoneTownNPC(NetworkText.FromKey(Lang.misc[19].Key, NPC.GetFullNetName()));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0 || OriginsModIntegrations.CheckAprilFools()) {
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 4);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 4);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 5);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 5);
			}
		}
		public class ShieldDropCondition : IItemDropRuleCondition {
			public bool CanDrop(DropAttemptInfo info) => OriginConfig.GraveshieldZombiesShouldDropAsItem;
			public bool CanShowItemDropInUI() => true;
			public string GetConditionDescription() => null;
		}
	}
}
