using Microsoft.Xna.Framework;
using Origins.Items.Armor.Defiled;
using Origins.World.BiomeData;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Defiled_Mite : ModNPC, IDefiledEnemy {
		internal const int spawnCheckDistance = 15;
		public const int aggroRange = 128;
		byte frame = 0;
		byte anger = 0;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 4;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Bunny);
			NPC.aiStyle = NPCAIStyleID.None;
			NPC.lifeMax = 44;
			NPC.defense = 12;
			NPC.damage = 38;
			NPC.width = 20;
			NPC.height = 18;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			NPC.value = 20;
			SpawnModBiomes = [
				ModContent.GetInstance<Underground_Defiled_Wastelands_Biome>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public int MaxMana => 16;
		public int MaxManaDrain => 8;
		public float Mana { get; set; }
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			this.DrainMana(target);
		}
		public void Regenerate(out int lifeRegen) {
			int factor = 64;
			lifeRegen = factor;
			Mana -= factor / 120f;// 1 mana for every 1 health regenerated
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Helmet>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Breastplate>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Greaves>(), 525));
		}
		public override bool PreAI() {
			if (NPC.HasPlayerTarget && Main.rand.NextBool(600)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(1.1f, 1.5f), NPC.Center);
			NPC.TargetClosest();
			NPC.aiStyle = NPC.HasPlayerTarget ? NPCAIStyleID.Fighter : NPCAIStyleID.None;
			if (((NPC.Center - NPC.targetRect.Center.ToVector2()) * new Vector2(1, 2)).Length() > aggroRange) {
				if (NPC.life < NPC.lifeMax) {
					NPC.aiStyle = NPCAIStyleID.Tortoise;
				} else {
					NPC.target = -1;
					NPC.aiStyle = NPCAIStyleID.None;
				}
			}
			if (NPC.HasPlayerTarget) {
				NPC.FaceTarget();
				NPC.spriteDirection = NPC.direction;
			}
			if (NPC.collideY) {
				NPC.rotation = 0;
				if (anger != 0) {
					if (anger > 1) anger--;
					NPC.aiStyle = NPCAIStyleID.Tortoise;
				} else if (NPC.aiStyle == NPCAIStyleID.None) {
					NPC.velocity.X *= 0.95f;
				}
				if (Math.Sign(NPC.velocity.X) == NPC.direction) {
					frame = (byte)((frame + 1) & 15);
				} else if (Math.Sign(NPC.velocity.X) == -NPC.direction) {
					frame = (byte)((frame - 1) & 15);
				}
			} else {
				if (anger == 1) anger = 0;
			}
			return NPC.aiStyle != NPCAIStyleID.None;
		}
		public override void FindFrame(int frameHeight) {
			NPC.frame = new Rectangle(0, 28 * (frame & 12) / 4, 32, 26);
		}
		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) {
			anger = 6;
		}
		public override int SpawnNPC(int tileX, int tileY) {
			Tile tile;
			for (int i = 0; i < spawnCheckDistance; i++) {
				tile = Main.tile[tileX, ++tileY];
				if (tile.HasTile) {
					tileY--;
					break;
				}
			}
			return base.SpawnNPC(tileX, tileY);
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
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/DF1_Gore");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Mana);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Mana = reader.ReadSingle();
		}
	}
}
