using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Armor.Defiled;
using Origins.Items.Materials;
using Origins.Items.Weapons.Melee;
using Origins.Journal;
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
	public class Defiled_Cyclops : Glowing_Mod_NPC, IDefiledEnemy, IWikiNPC, IJournalEntrySource {
		public Rectangle DrawRect => new(0, 7, 46, 58);
		public int AnimationFrames => 32;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public AssimilationAmount? Assimilation => 0.08f;
		public const float speedMult = 1.3f;
		public string EntryName => "Origins/" + typeof(Defiled_Cyclops_Entry).Name;
		public class Defiled_Cyclops_Entry : JournalEntry {
			public override string TextKey => "Defiled_Cyclops";
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 7;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 110;
			NPC.defense = 8;
			NPC.damage = 42;
			NPC.width = 28;
			NPC.height = 40;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			NPC.value = 90;
			NPC.knockBackResist = 0.5f;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type,
				ModContent.GetInstance<Underground_Defiled_Wastelands_Biome>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public int MaxMana => 50;
		public int MaxManaDrain => 10;
		public float Mana { get; set; }
		public void Regenerate(out int lifeRegen) {
			int factor = 48 / ((NPC.life / 10) + 1);
			lifeRegen = factor;
			Mana -= factor / 120f;// 1 mana for every 1 health regenerated
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.DesertCave) return 0;
			return Defiled_Wastelands.SpawnRates.LandEnemyRate(spawnInfo, false) * Defiled_Wastelands.SpawnRates.Cyclops;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bone_Latcher>(), 38));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Lousy_Liver>(), 87));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Helmet>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Breastplate>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Greaves>(), 525));
		}
		public override bool? CanFallThroughPlatforms() => NPC.directionY == 1 && NPC.target >= 0 && NPC.targetRect.Bottom > NPC.position.Y + NPC.height + NPC.velocity.Y;
		public override void AI() {
			if (Main.rand.NextBool(800)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle, NPC.Center);
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.spriteDirection = NPC.direction;
			}
			if (NPC.collideY && Math.Sign(NPC.velocity.X) == NPC.direction) NPC.velocity.X /= speedMult;
		}
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter > 5) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 58) % 406, 46, 56);
				NPC.frameCounter = 0;
			}
		}
		public void SpawnWisp(NPC npc)
		{
			if (Main.masterMode || (Main.expertMode && Main.rand.NextBool()))
			{
				NPC.NewNPC(npc.GetSource_Death(), (int)npc.position.X, (int)npc.position.Y, ModContent.NPCType<Defiled_Wisp>());
			}
		}
		public override void PostAI() {
			if (NPC.collideY && Math.Sign(NPC.velocity.X) == NPC.direction) NPC.velocity.X *= speedMult;
			/*if(!attacking) {
				npc.Hitbox = new Rectangle((int)npc.position.X+(npc.oldDirection == 1 ? 70 : 52), (int)npc.position.Y, 56, npc.height);
			}*/
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;)
				Origins.instance.SpawnGoreByName(NPC.GetSource_OnHit(NPC), Main.rand.NextVectorIn(spawnbox), projectile.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? 0 : halfWidth;
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_OnHit(NPC), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(), "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
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
