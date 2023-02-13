using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	//[AutoloadEquip(EquipType.HandsOn)] needs HandsOn sprite for this to work
    public class Personal_Laser_Blade : ModItem, IElementalItem {
		public const int max_charge = 75;
		public ushort Element => Elements.Fire;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Personal Laser Blade");
			Tooltip.SetDefault("Time your swings for more powerful blows\n'Be careful, it's hot'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.damage = 67;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.DamageType = DamageClass.Melee;
			Item.width = 42;
			Item.height = 42;
			Item.crit = 10;
			Item.useTime = 1;
			Item.useAnimation = 30;
			Item.noMelee = true;
			Item.shoot = Personal_Laser_Blade_P.ID;
			Item.shootSpeed = 3f;
			Item.knockBack = 1;
			Item.autoReuse = false;
			Item.useTurn = false;
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item45;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Busted_Servo>(), 13);
			recipe.AddIngredient(ModContent.ItemType<Power_Core>());
			recipe.AddIngredient(ModContent.ItemType<Rotor>(), 2);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 8);
			recipe.AddTile(TileID.MythrilAnvil); //Fabricator
			recipe.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			ref int laserBladeCharge = ref player.GetModPlayer<OriginPlayer>().laserBladeCharge;
			velocity = OriginExtensions.Vec2FromPolar(
				player.direction == 1 ? player.itemRotation : player.itemRotation + MathHelper.Pi,
				velocity.Length() * (1 + (laserBladeCharge / (float)max_charge))
			);
			damage += (laserBladeCharge * damage) / max_charge;
			if (player.ItemAnimationEndingOrEnded) {
				laserBladeCharge = 0;
			}
		}
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			ref int laserBladeCharge = ref player.GetModPlayer<OriginPlayer>().laserBladeCharge;
			if (laserBladeCharge < max_charge) {
				laserBladeCharge = Math.Min(laserBladeCharge + 2, max_charge + 1);// increments by 2 since it's decrementing by 1 at the same rate
				if (laserBladeCharge >= max_charge) {
					for (int i = 0; i < 20; i++) {
						Dust.NewDust(
							player.position,
							player.width,
							player.height,
							DustID.GoldFlame
						);
					}
				}
			} else {
				laserBladeCharge = max_charge + 1;
			}
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
			Projectile.timeLeft = 35;
			Projectile.hide = true;
		}
		public override void AI() {
			Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Clentaminator_Blue, 0, 0, 65, new Color(240, 50, 0), 0.9f);
			dust.velocity *= 1.8f;
			const float sizeValue = 10;
			if (Projectile.ai[0] < sizeValue) {
				Projectile.ai[0] += 1.25f;
			} else {
				Projectile.ai[0] = sizeValue;
			}
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			if (Main.rand.NextBool()) target.AddBuff(BuffID.OnFire, 600);
		}
	}
}
