using Microsoft.Xna.Framework;
using Origins.Tiles.Other;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
    public class Graveshield_Zombie : ModNPC {
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
			AIType = NPCID.Zombie;
			AnimationType = NPCID.Zombie;
			Banner = NPCID.Zombie;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
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
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime
			);
		}
		public override void OnKill() {
			NPC.DropTombstoneTownNPC(NetworkText.FromKey(Lang.misc[19].Key, NPC.GetFullNetName()));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life < 0 || OriginsModIntegrations.CheckAprilFools()) {
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 4);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 4);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 5);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 5);
			}
		}
	}
}
