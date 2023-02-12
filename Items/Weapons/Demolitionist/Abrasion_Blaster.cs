using Microsoft.Xna.Framework;
using Origins.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Abrasion_Blaster : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Abrasion Blaster");
			Tooltip.SetDefault("Shots can destroy tiles");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SniperRifle);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 50;
			Item.crit = 14;
			Item.useAnimation = 32;
			Item.useTime = 16;
			Item.shoot = ModContent.ProjectileType<Dreikan_Shot>();
			Item.reuseDelay = 6;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = CrimsonRarity.ID;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == ProjectileID.Bullet) type = Item.shoot;
			SoundEngine.PlaySound(SoundID.Item40, position);
			SoundEngine.PlaySound(SoundID.Item36.WithVolume(0.75f), position);
			OriginGlobalProj.extraUpdatesNext = 2;
		}
	}
}
