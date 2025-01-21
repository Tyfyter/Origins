using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using PegasusLib;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Armor.Rubber {
	[AutoloadEquip(EquipType.Head)]
	public class Rubber_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string[] Categories => [
            "ArmorSet"
        ];
		public override void SetDefaults() {
			Item.defense = 2;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClass.Generic) += 5f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Rubber_Breastplate>() && legs.type == ModContent.ItemType<Rubber_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Rubber");
			player.buffImmune[ModContent.BuffType<Static_Shock_Debuff>()] = true;
			player.buffImmune[BuffID.Electrified] = true;
			player.endurance += (1 - player.endurance) * 0.05f;
			if (OriginsModIntegrations.CheckAprilFools() && player.OriginPlayer().nearTrafficCone > 0) player.moveSpeed += 2f;
		}
		public override void ArmorSetShadows(Player player) {
			if (Main.rand.NextBool(2)) {
				Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.WaterCandle, 0, 1, 255, new Color(255, 150, 30), 1f);
				dust.noGravity = true;
				dust.velocity *= 1.2f;
			}
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddIngredient<Materials.Rubber>(10)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public string ArmorSetName => "Rubber_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Rubber_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Rubber_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Rubber_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().rubberBody = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Materials.Rubber>(25)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Rubber_Greaves : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 2;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) += 0.05f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Materials.Rubber>(15)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
