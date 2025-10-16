using Origins.Dev;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Firespit : ModItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => [
            WikiCategories.Gun
        ];
        public override void SetStaticDefaults() {
			ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.OnFire3];
		}
		public override void SetDefaults() {
			Item.damage = 36;
			Item.DamageType = DamageClass.Ranged;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.crit = 1;
			Item.useAnimation = 35;
			Item.useTime = 2;
			Item.width = 58;
			Item.height = 22;
			Item.useAmmo = ItemID.Fireblossom;
			Item.shoot = ModContent.ProjectileType<Lava_Shot>();
			Item.shootSpeed = 8.75f;
			Item.UseSound = SoundID.Item20;
			Item.reuseDelay = 9;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.glowMask = glowmask;
			Item.consumeAmmoOnFirstShotOnly = true;
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Item20, player.itemLocation);
			return null;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.itemAnimationMax - player.itemAnimation > 9) return;
			Vector2 offset = Vector2.Normalize(velocity);
			offset = offset * 24 + offset.RotatedBy(-MathHelper.PiOver2 * player.direction) * 8;
			position += offset;
			velocity = velocity.RotatedByRandom(0.65);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.itemAnimationMax - player.itemAnimation > 9) return false;
			Lava_Shot.damageType = DamageClass.Ranged;
			return true;
		}
	}
}
