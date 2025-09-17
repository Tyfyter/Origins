using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Liars_Tongue : ModItem {
		public override void SetDefaults() {
			Item.DefaultToAccessory(38, 32);
			Item.damage = 76;
			Item.DamageType = DamageClass.Melee;
			Item.knockBack = 7;
			Item.useTime = 15 * 60;
			Item.mana = 140;
			Item.shootSpeed = 4;
			Item.shoot = ModContent.ProjectileType<Forbidden_Voice_P>();
			Item.buffType = BuffID.Silenced;
			Item.rare = CursedRarity.ID;
			Item.master = true;
			Item.value = Item.sellPrice(gold: 5);
		}
		public override bool CanUseItem(Player player) => false;
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.cursedVoice = true;
			originPlayer.cursedVoiceItem = Item;
			if (originPlayer.cursedVoiceCooldownMax == 0) originPlayer.cursedVoiceCooldownMax = 1;
			player.endurance += (1 - player.endurance) * (Math.Clamp(1 - originPlayer.cursedVoiceCooldown / (float)originPlayer.cursedVoiceCooldownMax, 0, 1) * 0.17f + 0.1f);
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.SubstituteKeybind(Keybindings.ForbiddenVoice);
		}
		public override bool MeleePrefix() => true;
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.WormScarf)
			.AddIngredient<Forbidden_Voice>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
