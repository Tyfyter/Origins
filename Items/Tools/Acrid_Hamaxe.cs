using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Acrid_Hamaxe : ModItem, ICustomWikiStat {
		static short glowmask;
		public string[] Categories => [
			"Tool"
		];
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TitaniumWaraxe);
			Item.damage = 31;
			Item.DamageType = DamageClass.Melee;
			Item.pick = 0;
			Item.hammer = 65;
			Item.axe = 22;
			Item.useTime = 7;
			Item.useAnimation = 22;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
		}
		public override float UseTimeMultiplier(Player player) {
			return player.wet ? 1.5f : 1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 20)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
