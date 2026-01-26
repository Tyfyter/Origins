using Origins.Dusts;
using Origins.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories;
[AutoloadEquip(EquipType.Shoes, EquipType.Wings)]
public class Bomb_Rack : ModItem {
	public static int DebuffTime => Main.rand.Next(3, 7) * 60;
	public override void SetStaticDefaults() {
		ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new(160);
		Origins.AddGlowMask(this);
	}
	public override void SetDefaults() {
		Item.DefaultToAccessory();
		Item.DamageType = DamageClasses.Explosive;
		Item.damage = 30;
		Item.useTime = 12;
		Item.shoot = ModContent.ProjectileType<Bomb_Rack_Carpet_Bomb>();
		Item.knockBack = 3;
		Item.useLimitPerAnimation = 9;
		Item.useAnimation = Item.useTime;
		Item.rare = ItemRarityID.Blue;
		Item.value = Item.sellPrice(gold: 2);
		Item.expert = true;
	}
	public override void UpdateAccessory(Player player, bool hideVisual) {
		Max(ref player.accRunSpeed, 6f);
		player.rocketBoots = player.vanityRocketBoots = 1;
		OriginPlayer originPlayer = player.OriginPlayer();
		originPlayer.bombRack = Item;
		originPlayer.bombRackVisual = !hideVisual;
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
	public override bool WeaponPrefix() => true;
	public override int ChoosePrefix(UnifiedRandom rand) {
		return OriginExtensions.GetAllPrefixes(Item, rand, (PrefixCategory.AnyWeapon, 1), (PrefixCategory.Accessory, 2));
	}
}
public class Bomb_Rack_Carpet_Bomb : ModProjectile {
	public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ClusterGrenadeI;
	public override void SetDefaults() {
		Projectile.DamageType = DamageClasses.Explosive;
		Projectile.width = 16;
		Projectile.height = 16;
		Projectile.aiStyle = -1;
		Projectile.friendly = true;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 360;
	}
	public override void AI() {
		Projectile.rotation += 0.1f;
		Projectile.velocity.Y += 0.2f;
	}
	public override void OnKill(int timeLeft) {
		ExplosiveGlobalProjectile.DoExplosion(Projectile, 64, sound: SoundID.Item14);
	}
}
