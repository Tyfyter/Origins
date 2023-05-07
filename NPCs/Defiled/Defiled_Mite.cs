using Microsoft.Xna.Framework;
using Origins.Items.Armor.Defiled;
using Origins.Items.Materials;
using Origins.Items.Weapons.Demolitionist;
using Origins.World.BiomeData;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Items.Armor.Defiled.Defiled2_Helmet;

namespace Origins.NPCs.Defiled {
	public class Defiled_Mite : ModNPC {
		internal const int spawnCheckDistance = 15;
		public const int aggroRange = 128;
		byte frame = 0;
		byte anger = 0;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$Defiled} Mite");
			Main.npcFrameCount[NPC.type] = 4;
			SpawnModBiomes = new int[] {
				ModContent.GetInstance<Defiled_Wastelands>().Type
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Bunny);
			NPC.aiStyle = NPCAIStyleID.None;
			NPC.lifeMax = 22;
			NPC.defense = 9;
			NPC.damage = 34;
			NPC.width = 34;
			NPC.height = 26;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			NPC.value = 20;
		}
		static int MaxMana => 16;
		static int MaxManaDrain => 8;
		float Mana {
			get;
			set;
		}
		public override void OnHitPlayer(Player target, int damage, bool crit) {
			int maxDrain = (int)Math.Min(MaxMana - Mana, MaxManaDrain);
			int manaDrain = Math.Min(maxDrain, target.statMana);
			target.statMana -= manaDrain;
			Mana += manaDrain;
			if (target.manaRegenDelay < 10) target.manaRegenDelay = 10;
		}
		public override void UpdateLifeRegen(ref int damage) {
			if (NPC.life < NPC.lifeMax && Mana > 0) {
				int factor = 64;
				if (!NPC.HasBuff(BuffID.Bleeding)) NPC.lifeRegen += factor;
				Mana -= factor / 120f;// 1 mana for every 1 health regenerated
			}
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				new FlavorTextBestiaryInfoElement("Dweller of the {$Defiled} Caverns. Hard to spot as it does not move until prey draws near."),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled_Spirit>(), 10));
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
					NPC.velocity.X *= 1.2f;
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
		public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit) {
			anger = 6;
			return true;
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
		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;) Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4)));
		}
		public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit) {
			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? 0 : halfWidth;
			for (int i = Main.rand.Next(3); i-- > 0;) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), new Vector2(knockback * player.direction, -0.1f * knockback), Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4)));
		}
		public override void HitEffect(int hitDirection, double damage) {
			if (NPC.life < 0) {
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF1_Gore"));
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4)));
				for (int i = 0; i < 3; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4)));
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
