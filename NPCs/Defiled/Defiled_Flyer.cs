using Microsoft.Xna.Framework;
using Origins.Items.Armor.Defiled;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Consumables.Food;
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
	public class Defiled_Flyer : ModNPC, IDefiledEnemy {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$Defiled} Phantom");
			//Origins.AddGlowMask(NPC. "NPCs/Defiled/Defiled_Flyer_Glow");
			Main.npcFrameCount[NPC.type] = 4;
			SpawnModBiomes = new int[] {
				ModContent.GetInstance<Defiled_Wastelands>().Type
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Bunny);
			NPC.aiStyle = 14;
			NPC.lifeMax = 42;
			NPC.defense = 10;
			NPC.damage = 20;
			NPC.width = 104;
			NPC.height = 38;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			NPC.knockBackResist = 0.75f;
			NPC.value = 76;
		}
		public int MaxMana => 35;
		public int MaxManaDrain => 15;
		public float Mana { get; set; }
		public override void OnHitPlayer(Player target, int damage, bool crit) {
			this.DrainMana(target);
		}
		public void Regenerate(out int lifeRegen) {
			int factor = 57 / ((NPC.life / 10) + 1);
			lifeRegen = factor;
			Mana -= factor / 180f;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				new FlavorTextBestiaryInfoElement("A great antibody for reaching airborne or hard-to-reach threats. This may have once been a flying creature like a bird."),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Krunch_Mix>(), 17));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Helmet>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Breastplate>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Greaves>(), 525));
		}
		public override void AI() {
			if (Main.rand.NextBool(900)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(1f, 1.1f), NPC.Center);
			NPC.FaceTarget();
			if (!NPC.HasValidTarget) NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.spriteDirection = NPC.direction;
			if (++NPC.frameCounter > 5) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 38) % 152, 104, 36);
				NPC.frameCounter = 0;
			}
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
				for (int i = 0; i < 2; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
				for (int i = 0; i < 6; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4)));
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
