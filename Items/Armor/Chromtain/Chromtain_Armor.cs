using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Chromtain {
    [AutoloadEquip(EquipType.Head)]
	public class Chromtain_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage, ICustomWikiStat {
        public string[] Categories => [
            "PostMLArmorSet"/*,
            "MeleeBoostGear" should probably give it melee boosts, when the time comes */
        ];
        public override void SetStaticDefaults() {
			Origins.AddHelmetGlowmask(this);
		}
		public override void SetDefaults() {
			Item.defense = 16;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = CrimsonRarity.ID;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Chromtain_Bar>(), 15)
			.AddTile(TileID.DefendersForge) //Interstellar Sampler
			.Register();
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Chromtain_Breastplate>() && legs.type == ModContent.ItemType<Chromtain_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Chromtain");
			player.aggro += 400;
			player.buffImmune[BuffID.BrokenArmor] = true;
			player.buffImmune[BuffID.WitheredArmor] = true;
		}
		public string ArmorSetName => "Chromtain_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Chromtain_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Chromtain_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Chromtain_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			Origins.AddBreastplateGlowmask(this);
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.defense = 40;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = CrimsonRarity.ID;
		}
		public override void UpdateEquip(Player player) {
			player.statLifeMax2 += 20;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Chromtain_Bar>(), 25)
			.AddTile(TileID.DefendersForge) //Interstellar Sampler
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Chromtain_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			Origins.AddLeggingGlowMask(this);
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.defense = 25;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = CrimsonRarity.ID;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Chromtain_Bar>(), 20)
			.AddTile(TileID.DefendersForge) //Interstellar Sampler
			.Register();
		}
	}
}
