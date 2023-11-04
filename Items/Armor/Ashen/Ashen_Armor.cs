using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Ashen
{
    [AutoloadEquip(EquipType.Head)]
	public class Ashen_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Ashen/Ashen_Helmet_Head_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) += 0.08f;
		}
        public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Ashen_Breastplate>() && legs.type == ModContent.ItemType<Ashen_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "+15% knockback and knockback resistance\n-15% self-damage";
            player.GetModPlayer<OriginPlayer>().ashenKBReduction = true;
            player.GetKnockback(DamageClass.Generic) += 0.15f;
            player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.15f;
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 15);
			recipe.AddIngredient(ModContent.ItemType<NE8>(), 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
		public string ArmorSetName => "Ashen_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Ashen_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Ashen_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Ashen_Breastplate : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
            player.GetAttackSpeed(DamageClass.Generic) += 0.08f;
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 25);
			recipe.AddIngredient(ModContent.ItemType<NE8>(), 20);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Ashen_Greaves : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
            player.GetCritChance(DamageClass.Generic) += 8;
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 20);
			recipe.AddIngredient(ModContent.ItemType<NE8>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
