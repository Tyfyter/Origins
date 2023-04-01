using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Summoner;
using Origins.World.BiomeData;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
    public class Chunky_Slime : ModNPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Chunky Slime");
			Main.npcFrameCount[NPC.type] = 2;
			SpawnModBiomes = new int[] {
				ModContent.GetInstance<Defiled_Wastelands>().Type
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Crimslime);
			NPC.aiStyle = NPCAIStyleID.Slime;
			NPC.lifeMax = 60;
			NPC.defense = 6;
			NPC.damage = 30;
			NPC.width = 32;
			NPC.height = 24;
			NPC.friendly = false;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			NPC.value = 40;
		}
		public override void FindFrame(int frameHeight) {
			NPC.CloneFrame(NPCID.Crimslime, frameHeight);
		}
		static int MaxMana => 50;
		static int MaxManaDrain => 10;
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
				int factor = 48 / ((NPC.life / 10) + 1);
				if (!NPC.HasBuff(BuffID.Bleeding)) NPC.lifeRegen += factor;
				Mana -= factor / 120f;// 1 mana for every 1 health regenerated
			}
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				new FlavorTextBestiaryInfoElement("A slime desaturated of its color due to contact with black bile. It now spends the rest of its days to the {$Defiled} will.")
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Gel, 1, 2, 4));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled_Spirit>(), 10));
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
