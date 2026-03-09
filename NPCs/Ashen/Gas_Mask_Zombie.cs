using AltLibrary.Common.Systems;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.LootConditions;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.NPCs.Ashen {
	[Autoload(false)]
	public class Gas_Mask_Zombie(Gas_Mask_Variants variant, float scale = 1f) : ModNPC, IWikiNPC, IAshenEnemy {
		public override string Name => $"{base.Name}{variant.VariantName}{scale}";
		public override string Texture => typeof(Gas_Mask_Zombie).GetDefaultTMLName() + variant.VariantName;
		public override LocalizedText DisplayName => Mod.GetLocalization($"{LocalizationCategory}.{base.Name}.{nameof(DisplayName)}");
		protected override bool CloneNewInstances => true;
		public Rectangle DrawRect => new(0, 0, 34, 46);
		public int AnimationFrames => 3;
		public int FrameDuration => 3;
		bool IAshenEnemy.IsRobotic => false;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = Main.npcFrameCount[variant.VanillaVariant];
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft; // uncomment this line and comment the line underneath this line to check the stats of all variants
			//NPCID.Sets.NPCBestiaryDrawOffset[Type] = scale == 1f ? NPCID.Sets.NPCBestiaryDrawOffset[variant.VanillaVariant] : NPCExtensions.HideInBestiary;
			/*NPCID.Sets.DontDoHardmodeScaling[Type] = true;
			OriginsSets.NPCs.CustomExpertScaling[Type] = npc => {
				if (Main.hardMode) {
					int strength = npc.damage + 6 + npc.lifeMax / 4;
					if (strength == 0) strength = 1;
					int targetStrength = 80;
					if (NPC.downedPlantBoss) targetStrength += 40;
					if (strength < targetStrength) {
						float num3 = targetStrength / strength;
						npc.damage = (int)(npc.damage * num3 * 0.9);
						npc.defense = (int)(npc.defense * (num3 + 4) / 5);
						npc.lifeMax = (int)(npc.lifeMax * num3 * 1.1);
						npc.value = (int)(npc.value * num3 * 0.8);
					}
				}
			};*/
			GetInstance<Ashen_Biome.SpawnRates>().AddSpawn(Type, BiomeSpawnChance);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(variant.VanillaVariant);
			NPC.defense = (int)(NPC.defense * scale);
			NPC.damage = (int)(NPC.damage * scale);
			NPC.lifeMax = (int)(NPC.lifeMax * scale);
			NPC.value = (int)(NPC.value * scale);
			NPC.npcSlots *= scale;
			NPC.knockBackResist *= 2f - scale;
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.scale = scale;
			NPC.DeathSound = Origins.Sounds.MaskedZombieDeath;
			AIType = variant.VanillaVariant;
			Banner = Item.NPCtoBanner(NPCID.Zombie);
			SpawnModBiomes = [
				GetInstance<Ashen_Biome>().Type,
			];
		}
		public override bool PreAI() {
			NPC.spriteDirection = NPC.direction;
			return true;
		}
		bool SharedSpawnConditions(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown) return false;
			if (spawnInfo.Player.ZoneGraveyard || !Main.dayTime) return variant.CanSpawn(spawnInfo);
			return false;
		}
		static float PortionedSpawnRate(NPCSpawnInfo spawnInfo) => Ashen_Biome.SpawnRates.PowerZombie / Gas_Mask_Variants.TotalSpawnWeight(spawnInfo);
		public float BiomeSpawnChance(NPCSpawnInfo spawnInfo) {
			if (!SharedSpawnConditions(spawnInfo)) return 0;
			return PortionedSpawnRate(spawnInfo);
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!SharedSpawnConditions(spawnInfo)) return 0;
			if (WorldBiomeManager.GetWorldEvil(true) is not Ashen_Alt_Biome) return 0;
			return PortionedSpawnRate(spawnInfo) * 0.08f;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			IBestiaryInfoElement text = Gas_Mask_Variants.variants[0].zomb.GetBestiaryFlavorText();
			if (!variant.VariantName.Contains("Armed")) text = variant.zomb.GetBestiaryFlavorText();

			bestiaryEntry.AddTags(
				text,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime
			);
		}
		public override void FindFrame(int frameHeight) {
			NPC.VanillaFindFrame(frameHeight, false, variant.VanillaVariant);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Amber, 20));
			npcLoot.Add(new CommonDrop(ItemType<Gas_Mask>(), 50));
			npcLoot.Add(new CopyNPCDropRule(variant.VanillaVariant));
		}
		public override void AddShops() {
			ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[Type] = ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[variant.zomb.Type];
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0 || OriginsModIntegrations.CheckAprilFools()) {
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 4);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 4);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 5);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 5);
			}
		}
	}
	public abstract class Gas_Mask_Variants : ILoadable {
		public virtual string VariantName => GetType().Name["Variant".Length..];
		public virtual int VanillaVariant => NPCID.Zombie;
		public virtual bool HasScaledVariants => true;
		public static readonly List<Gas_Mask_Variants> variants = [];
		public Gas_Mask_Zombie zomb;
		public virtual bool CanSpawn(NPCSpawnInfo spawnInfo) => true;
		public static float TotalSpawnWeight(NPCSpawnInfo spawnInfo) => variants.Count(v => v.CanSpawn(spawnInfo)) * 3;
		public void Load(Mod mod) {
			zomb = new(this);
			mod.AddContent(zomb);
			if (HasScaledVariants) {
				mod.AddContent(new Gas_Mask_Zombie(this, 0.9f));
				mod.AddContent(new Gas_Mask_Zombie(this, 1.1f));
			}
			variants.Add(this);
		}

		public void Unload() { }
		public class Variant : Gas_Mask_Variants { }
		public class VariantBald : Gas_Mask_Variants {
			public override int VanillaVariant => NPCID.BaldZombie;
		}
		public class VariantPincushion : Gas_Mask_Variants {
			public override int VanillaVariant => NPCID.PincushionZombie;
		}
		public class VariantSwamp : Gas_Mask_Variants {
			public override int VanillaVariant => NPCID.SwampZombie;
		}
		public class VariantTwiggy : Gas_Mask_Variants {
			public override int VanillaVariant => NPCID.TwiggyZombie;
		}
		public class VariantFemale : Gas_Mask_Variants {
			public override int VanillaVariant => NPCID.FemaleZombie;
		}
		public class VariantTorch : Gas_Mask_Variants {
			public override int VanillaVariant => NPCID.TorchZombie;
			public override bool HasScaledVariants => false;
		}
		public class VariantArmedTorch : Gas_Mask_Variants {
			public override int VanillaVariant => NPCID.ArmedTorchZombie;
			public override bool HasScaledVariants => false;
			public override bool CanSpawn(NPCSpawnInfo spawnInfo) => Main.expertMode;
		}
		public class VariantDoctor : Gas_Mask_Variants {
			public override int VanillaVariant => NPCID.ZombieDoctor;
			public override bool HasScaledVariants => false;
			public override bool CanSpawn(NPCSpawnInfo spawnInfo) => Main.halloween;
		}
	}
}
