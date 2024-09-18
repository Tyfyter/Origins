using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Tendon {
    [AutoloadEquip(EquipType.Head)]
	public class Tendon_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            "ArmorSet",
            "RangedBoostGear",
			"ExplosiveBoostGear",
			"GenericBoostGear"
        ];
        public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Ranged) += 0.1f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Tendon_Shirt>() && legs.type == ModContent.ItemType<Tendon_Pants>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Tendon");
			player.GetModPlayer<OriginPlayer>().tendonSet = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.CrimtaneOre, 8)
			.AddIngredient(ItemID.Vertebrae, 16)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public string ArmorSetName => "Tendon_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Tendon_Shirt>();
		public int LegsItemID => ModContent.ItemType<Tendon_Pants>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Tendon_Shirt : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClass.Generic) += 0.06f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.CrimtaneOre, 20)
			.AddIngredient(ItemID.Vertebrae, 28)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Tendon_Pants : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.ammoBox = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.CrimtaneOre, 14)
			.AddIngredient(ItemID.Vertebrae, 22)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
