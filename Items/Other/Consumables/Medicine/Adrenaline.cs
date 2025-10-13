using Origins.Buffs;
using Origins.CrossMod.Avalon;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Medicine {
	public class Adrenaline : ModItem {
		public static int HealStamina => 30;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 30;
		}
		public override void SetDefaults() {
			Item.DefaultToHealingPotion(16, 16, 100);
			Item.buffTime = 15 * 60;
			Item.value = Item.sellPrice(silver: 2);
		}
		// TODO: https://github.com/tModLoader/tModLoader/pull/4786 arrives in stable in November
#if !TML_2025_08
		public override void ModifyPotionDelay(Player player, ref int baseDelay) {
			baseDelay += 60 * 15;
		}
#endif
		public override bool? UseItem(Player player) {
			for (int i = 0; i < Player.MaxBuffs; i++) {
				int buffType = player.buffType[i];
				switch (buffType) {
					case BuffID.Weak:
					case BuffID.BrokenArmor:
					case BuffID.WitheredArmor:
					case BuffID.WitheredWeapon:
					player.DelBuff(i--);
					break;
				}
			}
			player.AddBuff(ModContent.BuffType<Adrenaline_Buff>(), Item.buffTime);
			Stamina.Restore(player, HealStamina);
			return base.UseItem(player);
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (OriginsModIntegrations.Avalon is null) return;
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Name == "HealLife") {
					tooltips.Insert(i + 1, new(Mod, "HealLife", Language.GetTextValue("Mods.Origins.Items.GenericTooltip.HealStamina", HealStamina)));
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
	public class Adrenaline_Buff : ModBuff {
		public override string Texture => typeof(Adrenaline).GetDefaultTMLName();
		public override void Update(Player player, ref int buffIndex) {
			player.buffImmune[BuffID.Weak] = true;
			player.buffImmune[BuffID.BrokenArmor] = true;
			player.buffImmune[BuffID.WitheredArmor] = true;
			player.buffImmune[BuffID.WitheredWeapon] = true;
		}
	}
}
