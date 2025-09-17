using Origins.Dev;
using Origins.Items.Materials;
using PegasusLib;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Air_Tank : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Misc"
		];
		public static int BackSlot { get; private set; }
		public override void SetStaticDefaults() {
			BackSlot = Item.backSlot;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) => UpdateInventory(player);
		public override void UpdateInventory(Player player) {
			player.buffImmune[BuffID.Suffocation] = true;
			if (player.OriginPlayer().airTank.TrySet(true)) player.AddMaxBreath(257);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddRecipeGroup("AdamantiteBars", 20)
            .AddIngredient(ModContent.ItemType<Rubber>(), 12)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
