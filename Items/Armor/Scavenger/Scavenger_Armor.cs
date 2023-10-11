using Origins.Items.Materials;
using Origins.Tiles.Ashen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Scavenger {
    [AutoloadEquip(EquipType.Head)]
	public class Scavenger_Helmet : ModItem {
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClasses.Explosive) += 10;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Scavenger_Breastplate>() && legs.type == ModContent.ItemType<Scavenger_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "All explosives inflict 'Impeding Shrapnel'";
			//Originplayer.scavvyarmor = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Biocomponent10>(), 16);
			recipe.AddIngredient(ModContent.ItemType<Sanguinite_Ore_Item>(), 8);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Scavenger_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.25f;
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Biocomponent10>(), 28);
			recipe.AddIngredient(ModContent.ItemType<Sanguinite_Ore_Item>(), 20);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Scavenger_Greaves : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().explosiveProjectileSpeed += 0.15f;
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Biocomponent10>(), 22);
			recipe.AddIngredient(ModContent.ItemType<Sanguinite_Ore_Item>(), 14);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}