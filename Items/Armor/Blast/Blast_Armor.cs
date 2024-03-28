using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Blast {
    [AutoloadEquip(EquipType.Head)]
	public class Blast_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => new string[] {
            "HardmodeArmorSet",
            "ExplosiveBoostGear"
        };
		public override void SetDefaults() {
			Item.defense = 8;
			Item.value = Item.sellPrice(gold: 7, silver: 50);
			Item.rare = ItemRarityID.Yellow;
		}
        public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClass.Generic) += 0.1f;
            player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.05f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Blast_Breastplate>() && legs.type == ModContent.ItemType<Blast_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Blast");
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.explosiveSelfDamage -= 0.25f;
			//originPlayer.blastSet = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ChlorophyteBar, 12);
            recipe.AddIngredient(ModContent.ItemType<Blast_Resistant_Plate>());
            recipe.AddIngredient(ModContent.ItemType<Busted_Servo>(), 2);
            recipe.AddTile(TileID.MythrilAnvil); //fabricator
			recipe.Register();
		}
		public string ArmorSetName => "Blast_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Blast_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Blast_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Blast_Breastplate : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 18;
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
            player.noKnockback = true;
            player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.05f;
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.ChlorophyteBar, 24);
            recipe.AddIngredient(ModContent.ItemType<Blast_Resistant_Plate>());
            recipe.AddIngredient(ModContent.ItemType<Busted_Servo>(), 2);
            recipe.AddIngredient(ModContent.ItemType<Power_Core>(), 2);
            recipe.AddIngredient(ModContent.ItemType<Rotor>(), 2);
            recipe.AddTile(TileID.MythrilAnvil); //fabricator
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Blast_Greaves : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 13;
			Item.value = Item.sellPrice(gold: 4, silver: 50);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.25f;
			player.runAcceleration += 0.02f;
			player.jumpSpeedBoost += 2;
            player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.05f;
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.ChlorophyteBar, 18);
            recipe.AddIngredient(ModContent.ItemType<Blast_Resistant_Plate>());
            recipe.AddIngredient(ModContent.ItemType<Busted_Servo>(), 2);
            recipe.AddIngredient(ModContent.ItemType<Rotor>(), 2);
            recipe.AddTile(TileID.MythrilAnvil); //fabricator
			recipe.Register();
		}
	}
}
