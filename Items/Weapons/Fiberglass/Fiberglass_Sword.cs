using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Fiberglass {
    public class Fiberglass_Sword : ModItem, IElementalItem {
		public ushort Element => Elements.Fiberglass;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Sword");
			Tooltip.SetDefault("Be careful, it's sharp");
			SacrificeTotal = 1;
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
			Item.autoReuse = true;
            Item.useTurn = true;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
		}
	}
}
