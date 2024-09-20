using Microsoft.Xna.Framework;
using Origins.Items.Armor.Defiled;
using Origins.Items.Materials;
using Origins.Items.Weapons.Demolitionist;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Defiled_Watcher : ModNPC, IDefiledEnemy {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 3;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.aiStyle = NPCAIStyleID.None;
			NPC.lifeMax = 160;
			NPC.defense = 9;
			NPC.damage = 49;
			NPC.width = 74;
			NPC.height = 74;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(0.25f, 0.5f);
			NPC.DeathSound = Origins.Sounds.DefiledKill.WithPitchRange(0.25f, 0.5f);
			NPC.value = 103;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public int MaxMana => 100;
		public int MaxManaDrain => 100;
		public float Mana { get; set; }
		public void Regenerate(out int lifeRegen) {
			int factor = 37 / ((NPC.life / 40) + 2);
			lifeRegen = factor;
			Mana -= factor / 90f;// 3 mana for every 2 health regenerated
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.SpawnTileY < Main.worldSurface || spawnInfo.DesertCave) return 0;
			return Defiled_Wastelands.SpawnRates.FlyingEnemyRate(spawnInfo, true) * Defiled_Wastelands.SpawnRates.Asphyxiator;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Black_Bile>(), 1, 1, 3));
		}
		public int Frame {
			get => NPC.frame.Y / 68;
			set => NPC.frame.Y = value * 68;
		}
		public override void AI() {
			NPC.TargetClosestUpgraded(false);
			float speed = 16f;
			float inertia = 64f;
			Vector2 vectorToTargetPosition = default;
			if (NPC.HasValidTarget && NPC.GetTargetData().Center.DistanceSQ(NPC.Center) < 10 * 10 * 16 * 16) {
				Player target = Main.player[NPC.target];
				vectorToTargetPosition = target.Center - NPC.Center;
			} else {
				if (NPC.velocity.LengthSquared() < 0.001f && Main.rand.NextBool(10)) {
					// If there is a case where it's not moving at all, give it a little "poke"
					NPC.velocity += Main.rand.NextVector2CircularEdge(1, 1) * 0.5f;
				} else {

					NPC.velocity.X = NPC.oldVelocity.X;
					NPC.velocity.Y = NPC.oldVelocity.Y;
				}
			}
			if (vectorToTargetPosition != default) {
				vectorToTargetPosition = vectorToTargetPosition.SafeNormalize(default);
				NPC.aiAction = 0;
				vectorToTargetPosition *= speed;
				NPC.velocity = (NPC.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			}
			Vector2 nextVel = Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height, true, true);
			if (nextVel.X != NPC.velocity.X) NPC.velocity.X *= -0.2f;
			if (nextVel.Y != NPC.velocity.Y) NPC.velocity.Y *= -0.2f;
			//if (Math.Abs(NPC.velocity.X) < 0.001f) NPC.velocity.X = 0;
			//if (Math.Abs(NPC.velocity.Y) < 0.001f) NPC.velocity.Y = 0;
			NPC.rotation = NPC.velocity.X * 0.1f;
			if (NPC.velocity.X == 0) NPC.direction = NPC.oldDirection;
			else NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.oldDirection = NPC.direction;
			NPC.spriteDirection = NPC.direction;
		}
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter > 9) {
				NPC.frameCounter = 0;
				if (++Frame >= 3) {
					Frame = 0;
					NPC.frameCounter = 0;
				}
			}
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? 0 : halfWidth;
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(), "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life < 0) {
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 10; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
	}
}
