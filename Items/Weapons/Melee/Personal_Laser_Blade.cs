using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	//[AutoloadEquip(EquipType.HandsOn)]
	public class Personal_Laser_Blade : ModItem, IElementalItem {
		public ushort Element => Elements.Fire;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Personal Laser Blade");
			Tooltip.SetDefault("'Be careful, it's hot'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.damage = 67;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.DamageType = DamageClass.Melee;
			Item.width = 42;
			Item.height = 42;
			Item.useTime = 1;
			Item.useAnimation = 19;
			Item.noMelee = true;
			Item.shoot = Personal_Laser_Blade_P.ID;
			Item.shootSpeed = 6;
			Item.knockBack = 2;
			Item.autoReuse = true;
			Item.useTurn = false;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = OriginExtensions.Vec2FromPolar(player.direction == 1 ? player.itemRotation : player.itemRotation + MathHelper.Pi, velocity.Length());
		}
		public override void UseItemFrame(Player player) {
			player.handon = Item.handOnSlot;
		}
	}
	public class Personal_Laser_Blade_P : ModProjectile, IElementalProjectile {
		public ushort Element => Elements.Fire;
		public override string Texture => "Origins/Items/Weapons/Melee/Personal_Laser_Blade";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Personal Laser Blade");
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Flames);
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 15;
			Projectile.hide = true;
		}
		public override void AI() {
			const float sizeValue = 10;
			if (Projectile.ai[0] < sizeValue) {
				Projectile.ai[0] += 1.25f;
			} else {
				Projectile.ai[0] = sizeValue;
			}
		}
	}
}
