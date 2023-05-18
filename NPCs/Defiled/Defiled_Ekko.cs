using Microsoft.Xna.Framework;
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

namespace Origins.NPCs.Defiled {
    public class Defiled_Ekko : ModNPC, IDefiledEnemy {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$Defiled} Ekko");
			Main.npcFrameCount[NPC.type] = 14;
			SpawnModBiomes = new int[] {
				ModContent.GetInstance<Defiled_Wastelands>().Type
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.PirateCrossbower);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 200;
			NPC.defense = 26;
			NPC.damage = 20;
			NPC.width = 26;
			NPC.height = 42;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			AnimationType = NPCID.Merchant;
		}
		public int MaxMana => 50;
		public int MaxManaDrain => 10;
		public float Mana { get; set; }
		public override void OnHitPlayer(Player target, int damage, bool crit) {
			this.DrainMana(target);
		}
		public void Regenerate(out int lifeRegen) {
			int factor = 48 / ((NPC.life / 10) + 1);
			lifeRegen = factor;
			Mana -= factor / 120f;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				/*if (heartBroken <= 1) {
				new FlavorTextBestiaryInfoElement("Please just be my friend... I even tried to look like you. Am I beautiful?");
				} else {
				new FlavorTextBestiaryInfoElement("An attempt for the Defiled to copy the abilities of the Terrarian, this being serves as a huge threat to anything that is cleasning the Defiled.");
				}*/
				new FlavorTextBestiaryInfoElement("An attempt for the Defiled to copy the abilities of the Terrarian, this being serves as a huge threat to anything that is cleasning the Defiled."),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
		}
		public override void AI() {
			if (Main.rand.NextBool(800)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle, NPC.Center);
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.FaceTarget();
				NPC.spriteDirection = NPC.direction;
			}
			if (++NPC.frameCounter > 7) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 56) % 784, 36, 56);
				NPC.frameCounter = 0;
			}
		}
		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;)
				Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4)));
		}
		public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit) {
			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? 0 : halfWidth;
			for (int i = Main.rand.Next(3); i-- > 0;) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), new Vector2(knockback * player.direction, -0.1f * knockback), Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4)));
		}
		public override void HitEffect(int hitDirection, double damage) {
			if (NPC.life < 0) {
				for (int i = 0; i < 3; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
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
