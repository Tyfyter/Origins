using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Fiberglass {
	public class Fiberglass_Sword : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Sword");
			Tooltip.SetDefault("Be careful, it's sharp");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
			Item.damage = 18;
			Item.DamageType = DamageClass.Melee;
			Item.width = 42;
			Item.height = 42;
			Item.useTime = 16;
			Item.useAnimation = 16;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 2;
			Item.value = 5000;
			Item.autoReuse = true;
            Item.useTurn = true;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
		}
	}
}
