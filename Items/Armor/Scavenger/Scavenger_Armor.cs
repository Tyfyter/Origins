using Origins.Dev;
using Origins.Items.Materials;
using Origins.Tiles.Ashen;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Scavenger {
	[AutoloadEquip(EquipType.Head)]
	public class Scavenger_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            "ArmorSet",
            "ExplosiveBoostGear",
			"SelfDamageProtek"
		];
        public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
			Origins.AddHelmetGlowmask(this);
		}
        public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClasses.Explosive) += 8;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Scavenger_Breastplate>() && legs.type == ModContent.ItemType<Scavenger_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Scavenger");
			player.GetModPlayer<OriginPlayer>().scavengerSet = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Biocomponent10>(), 16)
			.AddIngredient(ModContent.ItemType<Sanguinite_Ore_Item>(), 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public string ArmorSetName => "Scavenger_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Scavenger_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Scavenger_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Scavenger_Breastplate : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.25f;
        }
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Biocomponent10>(), 28)
			.AddIngredient(ModContent.ItemType<Sanguinite_Ore_Item>(), 20)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Scavenger_Greaves : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().explosiveProjectileSpeed += 0.15f;
        }
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Biocomponent10>(), 22)
			.AddIngredient(ModContent.ItemType<Sanguinite_Ore_Item>(), 14)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}