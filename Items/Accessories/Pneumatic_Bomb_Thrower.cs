using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Pneumatic_Bomb_Thrower : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.ExplosiveBoostAcc
		];
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 20);
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
			Item.glowMask = glowmask;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().bombHandlingDevice = true;
			player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed += 0.5f;
		}

        public override void AddRecipes() {
            Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Air_Tank>())
            .AddIngredient(ModContent.ItemType<Bomb_Handling_Device>())
			.AddTile(TileID.TinkerersWorkbench)
            .Register();
        }
    }
}
