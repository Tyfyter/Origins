using Origins.Items.Weapons.Ammo;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Rattlesnake : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Musket);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 80;
			Item.width = 56;
			Item.height = 26;
			Item.useTime = 57;
			Item.useAnimation = 57;
			Item.shoot = ModContent.ProjectileType<Metal_Slug_P>();
			Item.useAmmo = ModContent.ItemType<Metal_Slug>();
			Item.knockBack = 8f;
			Item.shootSpeed = 12f;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = Origins.Sounds.HeavyCannon.WithPitchRange(0.9f, 1f);
			Item.autoReuse = true;
            Item.ArmorPenetration += 6;
        }
		public override Vector2? HoldoutOffset() {
			return Vector2.Zero;
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			if (player.mount.Active) damage *= 1.15f;
			if (player.OriginPlayer().LuckyHatGunActive) {
				float mult = (90f / Math.Max(Item.useAnimation, 8)) * 1.3f;
				if (mult > 1) damage *= mult;
			}
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == Metal_Slug_P.ID) type = Item.shoot;
            Vector2 offset = (velocity.RotatedBy(MathHelper.PiOver2 * -player.direction) * 5) / velocity.Length();
            position += offset;
        }
	}
}
