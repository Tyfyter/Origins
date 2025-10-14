using Origins.Buffs;
using Origins.CrossMod.Avalon;
using Origins.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

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
			Recipe.Create(Type)
			.AddIngredient(ItemID.FallenStar)
			.AddIngredient(ItemID.RockLobster)
			.AddRecipeGroup(ALRecipeGroups.RottenChunks)
			.AddTile(TileID.Bottles)
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
			Recipe.Create(Type)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient<Tree_Sap>()
			.AddIngredient<Bleeding_Obsidian_Shard>()
			.AddTile(TileID.Bottles)
			.Register();
		}
	}
	public class Multimed : MedicineBase {
		public override int HealAmount => 150;
		public override int ImmunityDuration => 2 * 60 * 60;
		public override int CooldownIncrease => 60 * 60;
		public override IEnumerable<int> GetDefaultImmunity() => Main.debuff.GetTrueIndexes();
		public override void PostSetStaticDefaults() {
			ImmunityListOverride = this.GetLocalization(nameof(ImmunityListOverride));
		}
		public override void UpdateBuff(Player player, ref int buffIndex) {
			player.lifeRegen += 3 * 2; // 3 HP/sec
		}
		public override void AddRecipes() {
			/*Recipe.Create(Type)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient<Tree_Sap>()
			.AddIngredient<Bleeding_Obsidian_Shard>()
			.AddTile(TileID.Bottles)
			.Register();*/
		}
	}
	public class Bottomless_Multimed : Multimed {
		public override bool HasSpecialBuff => true;
		public override void PostSetStaticDefaults() {
			base.PostSetStaticDefaults();
			ImmunityBuff = ModContent.GetInstance<Multimed>().ImmunityBuff;
		}
	}
}
