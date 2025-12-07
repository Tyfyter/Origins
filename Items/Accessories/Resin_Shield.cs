using Origins.Dev;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Shield)]
	public class Resin_Shield : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Vitality",
			"ExplosiveBoostAcc",
			"SelfDamageProtek"
		];
		public static int ShieldID { get; private set; }
		public static int InactiveShieldID { get; private set; }
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			ShieldID = Item.shieldSlot;
		}
		public override void Load() {
			if (Main.netMode == NetmodeID.Server) return;
			InactiveShieldID = EquipLoader.AddEquipTexture(
				Mod,
				"Origins/Items/Accessories/Resin_Shield_Shield_Inactive",
				EquipType.Shield,
				name: "Resin_Shield_Shield_Inactive"
			);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(36, 38);
			Item.defense = 3;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.noKnockback = true;
			player.fireWalk = true;
			player.GetModPlayer<OriginPlayer>().ResinShield = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.Amber, 6)
			.AddIngredient(ItemID.ObsidianShield)
			.AddIngredient(ModContent.ItemType<Carburite_Item>(), 12)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
