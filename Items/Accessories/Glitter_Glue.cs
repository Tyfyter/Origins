using Origins.Layers;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories;
public class Glitter_Glue : ModItem {
	public override void SetDefaults() {
		Item.DefaultToAccessory();
		Item.rare = ItemRarityID.Yellow;
		Item.master = true;
		Item.damage = 60;
		Item.DamageType = DamageClass.Magic;
		Item.shoot = ProjectileID.FairyQueenMagicItemShot;
		Item.knockBack = 1;
		Item.useTime = 60 * 4;
		Item.useAnimation = Item.useTime;
		Item.useLimitPerAnimation = 3;
		Item.value = Item.sellPrice(gold: 5);
	}
	public override void UpdateAccessory(Player player, bool hideVisual) => player.OriginPlayer().glitterGlue = Item;
	public override bool MagicPrefix() => true;
	public override int ChoosePrefix(UnifiedRandom rand) {
		return OriginExtensions.GetAllPrefixes(Item, rand, (PrefixCategory.AnyWeapon, 1), (PrefixCategory.Magic, 1), (PrefixCategory.Accessory, 2));
	}
}
