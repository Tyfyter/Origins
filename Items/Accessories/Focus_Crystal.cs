using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Dev;
using Origins.NPCs.Brine.Boss;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Balloon)]
	public class Focus_Crystal : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override void SetStaticDefaults() {
			AprilFoolsTextures.AddItem(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 30);
			Item.rare = ItemRarityID.Yellow;
			Item.expert = true;
			Item.value = Item.sellPrice(gold: 7);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddIngredient(ItemID.ShinyStone)
            .AddIngredient(ModContent.ItemType<Ruby_Reticle>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.rubyReticle = true;
			originPlayer.focusCrystal = true;
		}
	}
}
