using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories;
public class Glitter_Glue : ModItem {
	public override void SetStaticDefaults() {
		OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, _) => {
			if (!Main.dayTime) global.SetUpdateCountBoost(proj, global.UpdateCountBoost + 1);
		});
	}
	public override void SetDefaults() {
		Item.DefaultToAccessory();
		Item.rare = ItemRarityID.Yellow;
		Item.master = true;
		Item.damage = 40;
		Item.DamageType = DamageClass.Magic;
		Item.shoot = ProjectileID.FairyQueenMagicItemShot;
		Item.knockBack = 1;
		Item.useTime = (int)(60 * 2.6);// controls cooldown
		Item.useAnimation = Item.useTime;
		Item.useLimitPerAnimation = 12; // controls burst count
		Item.value = Item.sellPrice(gold: 5);
		Item.ArmorPenetration += 3;
	}
	public override void UpdateAccessory(Player player, bool hideVisual) => player.OriginPlayer().glitterGlue = Item;
	public override bool MagicPrefix() => true;
	public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
		if (Main.dayTime) damage *= 1.25f;
	}
	public override int ChoosePrefix(UnifiedRandom rand) {
		return OriginExtensions.GetAllPrefixes(Item, rand, (PrefixCategory.AnyWeapon, 1), (PrefixCategory.Magic, 1), (PrefixCategory.Accessory, 2));
	}
}
