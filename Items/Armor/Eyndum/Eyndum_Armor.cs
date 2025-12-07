using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Eyndum {
    [AutoloadEquip(EquipType.Head)]
	public class Eyndum_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            "PostMLArmorSet",
            "MeleeBoostGear",
            "RangedBoostGear",
            "MagicBoostGear",
            "SummmonBoostGear",
            "ExplosiveBoostGear"
        ];
        public override void SetStaticDefaults() {
			Origins.AddHelmetGlowmask(this);
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.defense = 16;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = CrimsonRarity.ID;
		}
		public override void UpdateEquip(Player player) {
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Eyndum_Breastplate>() && legs.type == ModContent.ItemType<Eyndum_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.GetModPlayer<OriginPlayer>().eyndumSet = true;
			if (player.whoAmI == Main.myPlayer && !Main.gameMenu) Origins.SetEyndumCoreUI();
		}
		public override void AddRecipes() {
			/*Recipe.Create(Type)
            .AddIngredient(ModContent.ItemType<Defiled_Bar>(), 15)
            //.AddIngredient(ModContent.ItemType<>(), 10)
            recipe.SetResult(this);
            .AddTile(TileID.Anvils)
            .Register();*/
		}
		public string ArmorSetName => "Eyndum_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Eyndum_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Eyndum_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Eyndum_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			Origins.AddBreastplateGlowmask(this);
		}
		public override void SetDefaults() {
			Item.defense = 16;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = CrimsonRarity.ID;
		}
		public override void UpdateEquip(Player player) {
			player.statLifeMax2 += 20;
        }
		public override void AddRecipes() {
			/*Recipe.Create(Type)
            .AddIngredient(ModContent.ItemType<Defiled_Bar>(), 25)
            //.AddIngredient(ModContent.ItemType<>(), 20)
            recipe.SetResult(this);
            .AddTile(TileID.Anvils)
            .Register();*/
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Eyndum_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			Origins.AddLeggingGlowMask(this);
		}
		public override void SetDefaults() {
			Item.defense = 12;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = CrimsonRarity.ID;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.2f;
		}
		public override void AddRecipes() {
			/*Recipe.Create(Type)
            .AddIngredient(ModContent.ItemType<Defiled_Bar>(), 20)
            //.AddIngredient(ModContent.ItemType<>(), 15)
            recipe.SetResult(this);
            .AddTile(TileID.Anvils)
            .Register();*/
		}
	}
}
