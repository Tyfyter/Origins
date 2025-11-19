using Origins.Buffs;
using Origins.CrossMod.Avalon;
using Origins.Items.Materials;
using Origins.Items.Other.Fish;
using Origins.Tiles.Ashen;
using Origins.Tiles.Brine;
using PegasusLib;
using PegasusLib.Reflection;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Terraria.Recipe;

namespace Origins.Items.Other.Consumables.Medicine {
	public class Adrenaline : MedicineBase {
		public static int HealStamina => 30;
		public override int HealAmount => 100;
		public override int ImmunityDuration => 15 * 60;
		public override IEnumerable<int> GetDefaultImmunity() {
			yield return BuffID.Weak;
			yield return BuffID.BrokenArmor;
			yield return BuffID.WitheredArmor;
			yield return BuffID.WitheredWeapon;
		}
		public override void OnUseItem(Player player) {
			Stamina.Restore(player, HealStamina);
		}
		public override void PostModifyTooltips(List<TooltipLine> tooltips) {
			if (OriginsModIntegrations.Avalon is null) return;
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Name == "HealLife") {
					tooltips.Insert(i + 1, new(Mod, "HealStamina", Language.GetTextValue("Mods.Origins.Items.GenericTooltip.HealStamina", HealStamina)));
					break;
				}
			}
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.FallenStar)
			.AddIngredient(ItemID.RockLobster)
			.AddRecipeGroup(ALRecipeGroups.RottenChunks)
			.AddTile<Medicine_Fabricator>()
			.AddConsumeIngredientCallback(IngredientQuantityRules.Alchemy)
			.Register();
		}
	}
	public class Brightsee : MedicineBase {
		public override int HealAmount => 50;
		public override int ImmunityDuration => 4 * 60 * 60;
		public override int CooldownIncrease => -15 * 60;
		public override IEnumerable<int> GetDefaultImmunity() {
			yield return BuffID.Darkness;
			yield return BuffID.Blackout;
			yield return BuffID.Obstructed;
		}
		public override void UpdateBuff(Player player, ref int buffIndex) {
			player.nightVision = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient<Tree_Sap>()
			.AddIngredient<Bleeding_Obsidian_Shard>()
			.AddTile<Medicine_Fabricator>()
			.AddConsumeIngredientCallback(IngredientQuantityRules.Alchemy)
			.Register();
		}
	}
	public class Blood_Pack : MedicineBase {
		public override int HealAmount => 50;
		public override int ImmunityDuration => 2 * 60 * 60;
		public override int CooldownIncrease => -15 * 60;
		public override IEnumerable<int> GetDefaultImmunity() {
			yield return BuffID.Bleeding;
		}
		public override void UpdateBuff(Player player, ref int buffIndex) {
			player.lifeRegen += 2 * 2; // 2 HP/sec
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ItemID.Hemopiranha)
			.AddIngredient(ItemID.Gel)
			.AddTile<Medicine_Fabricator>()
			.AddConsumeIngredientCallback(IngredientQuantityRules.Alchemy)
			.Register();
		}
	}
	public class Fire_Band : MedicineBase {
		public override int HealAmount => 100;
		public override int ImmunityDuration => 4 * 60 * 60;
		public override IEnumerable<int> GetDefaultImmunity() {
			yield return BuffID.OnFire;
			yield return BuffID.CursedInferno;
			yield return BuffID.OnFire3;
			yield return BuffID.ShadowFlame;
			yield return BuffID.Frostburn;
			yield return BuffID.Frostburn2;
			yield return BuffID.Burning;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Adhesive_Wrap>()
			.AddIngredient<Brineglow_Item>()
			.AddTile<Medicine_Fabricator>()
			.AddConsumeIngredientCallback(IngredientQuantityRules.Alchemy)
			.Register();
		}
	}
	public class Medicinal_Acid : MedicineBase {
		public static float MaxTornSeverity => 0.4f;
		public override int HealAmount => 100;
		public override int ImmunityDuration => 2 * 60 * 60;
		public override IEnumerable<int> GetDefaultImmunity() {
			yield return ModContent.BuffType<Toxic_Shock_Debuff>();
		}
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxTornSeverity);
		public override void OnUseItem(Player player) {
			if (player.OriginPlayer().tornCurrentSeverity > 0.4f) player.OriginPlayer().preMedicinalAcidLife = player.statLifeMax2;
		}
		public override void UpdateBuff(Player player, ref int buffIndex) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.medicinalAcid = true;
			if (originPlayer.preMedicinalAcidLife > 0 && player.statLifeMax2 != originPlayer.preMedicinalAcidLife) {
				originPlayer.medicinalAcidLife = player.statLifeMax2 - originPlayer.preMedicinalAcidLife;
				originPlayer.preMedicinalAcidLife = 0;
			}
			int maxHealPerTick = 1;
			while (originPlayer.medicinalAcidLife > 0) {
				if (--maxHealPerTick < 0) break;
				originPlayer.medicinalAcidLife--;
				player.lifeRegenCount += 120;
			}
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddRecipeGroup(ALRecipeGroups.RottenChunks)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ItemID.Flounder)
			.AddTile<Medicine_Fabricator>()
			.AddConsumeIngredientCallback(IngredientQuantityRules.Alchemy)
			.Register();
		}
	}
	public class Morphine : MedicineBase {
		public override int HealAmount => 120;
		public override int ImmunityDuration => 1;
		public override IEnumerable<int> GetDefaultImmunity() => [];
		public override string BuffTexture => Texture;
		public override void PostSetStaticDefaults() {
			Main.buffNoTimeDisplay[ImmunityBuff.Type] = true;
			BuffID.Sets.TimeLeftDoesNotDecrease[ImmunityBuff.Type] = true;
		}
		public override void UpdateBuff(Player player, ref int buffIndex) {
			player.lifeRegen += 3 * 2; // 3 HP/sec
			if (player.potionDelay <= 0) player.DelBuff(buffIndex--);
		}
		public override void PostModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Name == "BuffTime") {
					tooltips.RemoveAt(i);
					break;
				}
			}
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.BottledHoney)
			.AddRecipeGroup(ALRecipeGroups.Deathweed)
			.AddTile<Medicine_Fabricator>()
			.AddConsumeIngredientCallback(IngredientQuantityRules.Alchemy)
			.Register();
		}
	}
	public class Rasterwrap : MedicineBase {
		public override int HealAmount => 80;
		public override int ImmunityDuration => 4 * 60 * 60;
		public override int CooldownIncrease => 0;
		public override IEnumerable<int> GetDefaultImmunity() {
			yield return ModContent.BuffType<Rasterized_Debuff>();
			yield return BuffID.Stoned;
			yield return BuffID.Slow;
			yield return BuffID.Webbed;
		}
		public override void OnUseItem(Player player) {
			player.AddBuff(ModContent.BuffType<Mana_Buffer_Debuff>(), 20 * 60);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Adhesive_Wrap>()
			.AddIngredient<Strange_String>()
			.AddIngredient<Bilemouth>()
			.AddTile<Medicine_Fabricator>()
			.AddConsumeIngredientCallback(IngredientQuantityRules.Alchemy)
			.Register();
		}
	}
	public class Sanguis_Pack : MedicineBase {
		public override int HealAmount => 100;
		public override int ImmunityDuration => 3 * 60 * 60;
		public override string BuffTexture => "Terraria/Images/Buff_" + BuffID.Warmth;
		public override IEnumerable<int> GetDefaultImmunity() {
			yield return BuffID.Frostburn;
			yield return BuffID.Frostburn2;
			yield return BuffID.Chilled;
			yield return BuffID.Frozen;
		}
		public override void UpdateBuff(Player player, ref int buffIndex) {
			player.resistCold = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient<Polyeel>()
			.AddIngredient<Sanguinite_Ore_Item>()
			.AddTile<Medicine_Fabricator>()
			.AddConsumeIngredientCallback(IngredientQuantityRules.Alchemy)
			.Register();
		}
	}
	public class Unmarked_Antidote : MedicineBase {
		public override int HealAmount => 100;
		public override int ImmunityDuration => 4 * 60 * 60;
		public override int CooldownIncrease => 0;
		public override IEnumerable<int> GetDefaultImmunity() {
			yield return BuffID.Poisoned;
			yield return BuffID.Venom;
			yield return BuffID.Ichor;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ItemID.Stinger)
			.AddIngredient<Brineglow_Item>()
			.AddTile<Medicine_Fabricator>()
			.AddConsumeIngredientCallback(IngredientQuantityRules.Alchemy)
			.Register();
		}
	}
	public class Multimed : MedicineBase {
		public override int HealAmount => 150;
		public override int ImmunityDuration => 2 * 60 * 60;
		public override int CooldownIncrease => 60 * 60;
		public override IEnumerable<int> GetDefaultImmunity() => Main.debuff.GetTrueIndexes();
		public override void OnLoad() {
			On_Item.GetShimmered += On_Item_GetShimmered;
		}

		void On_Item_GetShimmered(On_Item.orig_GetShimmered orig, Item self) {
			if (self.type == Type) {
				WeightedRandom<int> items = new();
				int[] medicines = AnyDifferentMedicine.RecipeGroup.ValidItems.ToArray();
				int[] stacks = new int[medicines.Length];
				RangeRandom range = new(Main.rand, 0, medicines.Length);
				for (; self.stack > 0; self.stack--) {
					range.Reset();
					for (int i = 0; i < 3; i++) {
						int index = range.Get();
						range.Multiply(index, index + 1, 0);
						stacks[index]++;
					}
				}
				for (int i = 0; i < medicines.Length; i++) {
					if (stacks[i] <= 0) continue;
					Item newItem = Main.item[Item.NewItem(self.GetSource_Misc("Shimmer"), self.position, self.width, self.height, medicines[i], stacks[i])];
					newItem.shimmerTime = 1f;
					newItem.shimmered = true;
					newItem.shimmerWet = true;
					newItem.wet = true;
					newItem.velocity *= 0.1f;
					newItem.playerIndexTheItemIsReservedFor = Main.myPlayer;
					newItem.velocity.X = 1f * i;
					newItem.velocity.X *= 1f + i * 0.05f;
					if (i % 2 == 0) {
						newItem.velocity.X *= -1f;
					}
					NetMessage.SendData(MessageID.SyncItemsWithShimmer, -1, -1, null, newItem.whoAmI, 1f);
				}
				self.TurnToAir();
				if (Main.netMode == NetmodeID.SinglePlayer) {
					Item.ShimmerEffect(self.Center);
				} else {
					NetMessage.SendData(MessageID.ShimmerActions, -1, -1, null, 0, (int)self.Center.X, (int)self.Center.Y);
					NetMessage.SendData(MessageID.SyncItemsWithShimmer, -1, -1, null, self.whoAmI, 1f);
				}
				AchievementsHelper.NotifyProgressionEvent(27);
				return;
			}
			orig(self);
		}

		public override void PostSetStaticDefaults() {
			ImmunityListOverride = this.GetLocalization(nameof(ImmunityListOverride));
		}
		public override void UpdateBuff(Player player, ref int buffIndex) {
			player.lifeRegen += 3 * 2; // 3 HP/sec
		}
		public override void AddRecipes() => CreateRecipe()
			.AddRecipeGroup(AnyDifferentMedicine.RecipeGroup, 3)
			.AddTile<Medicine_Fabricator>()
			.Register();
		class Comparer : IEqualityComparer<HashSet<int>> {
			public bool Equals(HashSet<int> x, HashSet<int> y) => x.SetEquals(y);
			public int GetHashCode([DisallowNull] HashSet<int> obj) => obj.Aggregate((value, hash) => hash + value.GetHashCode());
		}
	}
	public class Bottomless_Multimed : Multimed {
		public override bool HasSpecialBuff => true;
		public override void PostSetStaticDefaults() {
			base.PostSetStaticDefaults();
			ImmunityBuff = ModContent.GetInstance<Multimed>().ImmunityBuff;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.consumable = false;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Multimed>()
			.AddIngredient(ItemID.ChlorophyteBar)
			.AddIngredient(ItemID.Ectoplasm)
			.AddTile<Medicine_Fabricator>()
			.Register();
		}
	}
}
