using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories;
[AutoloadEquip(EquipType.Shoes, EquipType.Wings)]
public class Cluster_Bomb_Boots : ModItem {
	public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
			Language.GetOrRegister($"Mods.Origins.Items.{nameof(Bomb_Rack)}.Tooltip"),
			Language.GetOrRegister($"Mods.Origins.Items.{nameof(Scrap_Compactor)}.Tooltip")
		);
	public override void SetStaticDefaults() {
		ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new(160);
	}
	public override void SetDefaults() {
		Item.DefaultToAccessory(30, 24);
		Item.DamageType = DamageClasses.Explosive;
		Item.damage = 48;
		Item.useTime = 12;
		Item.shoot = ModContent.ProjectileType<Bomb_Rack_Carpet_Bomb>();
		Item.knockBack = 3;
		Item.useLimitPerAnimation = 9;
		Item.useAnimation = Item.useTime;
		Item.rare = ItemRarityID.Green;
		Item.value = Item.sellPrice(gold: 4);
		Item.expert = true;
	}
	public override void UpdateAccessory(Player player, bool hideVisual) {
		Max(ref player.accRunSpeed, 6f);
		player.rocketBoots = player.vanityRocketBoots = 1;
		OriginPlayer originPlayer = player.OriginPlayer();
		originPlayer.bombRack = Item;
		originPlayer.bombRackVisual = !hideVisual;
		originPlayer.scrapCompactor = true;
	}
	public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
		Min(ref player.wingTime, player.wingTimeMax);
		if (player.wingTimeMax == 0) return;
		float multiplier = player.wingTime / player.wingTimeMax;
		multiplier *= multiplier;
		ascentWhenFalling *= multiplier;
		ascentWhenRising *= multiplier;
		maxCanAscendMultiplier *= multiplier;
		maxAscentMultiplier *= multiplier;
		constantAscend *= multiplier;
	}
	public override bool WingUpdate(Player player, bool inUse) => true;
	public override bool WeaponPrefix() => true;
	public override int ChoosePrefix(UnifiedRandom rand) {
		return OriginExtensions.GetAllPrefixes(Item, rand, (PrefixCategory.AnyWeapon, 1), (PrefixCategory.Accessory, 2));
	}
	public override void AddRecipes() {
		CreateRecipe()
		.AddIngredient<Bomb_Rack>()
		.AddIngredient<Scrap_Compactor>()
		.AddTile(TileID.TinkerersWorkbench)
		.Register();
	}
}
