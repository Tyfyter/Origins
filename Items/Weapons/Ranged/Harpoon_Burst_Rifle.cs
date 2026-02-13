using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ammo;
using Origins.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
	public class Harpoon_Burst_Rifle : Harpoon_Gun {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				if (proj.aiStyle != ProjAIStyleID.HeldProjectile) {
					global.SetUpdateCountBoost(proj, global.UpdateCountBoost + 1);
					if (proj.TryGetGlobalProjectile(out HarpoonGlobalProjectile harpoonGlobal)) harpoonGlobal.extraGravity.Y -= 0.24f;
				}
			});
			ItemID.Sets.gunProj[Type] = true;
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			DefaultToHarpoonGun();
			Item.damage = 47;
			Item.knockBack = 5;
			Item.useAnimation = 5;
			Item.useTime = 5;
			Item.reuseDelay = 2;
			Item.width = 56;
			Item.height = 26;
			Item.shoot = ModContent.ProjectileType<Harpoon_Burst_Rifle_P>();
			Item.shootSpeed = 13.75f;
			Item.value = Item.sellPrice(gold: 2, silver: 80);
			Item.rare = ItemRarityID.LightRed;
			Item.noUseGraphic = true;
			Item.channel = true;
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return Main.rand.NextBool(6);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.ItemAnimationJustStarted) type = Item.shoot;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => true;
		public override void OnConsumeAmmo(Item ammo, Player player) {
			Harpoon_Burst_Rifle_P.consumed = true;
		}
	}
	public class Harpoon_Burst_Rifle_P : ModProjectile {
		internal static bool consumed = false;
		public override string Texture => typeof(Harpoon_Burst_Rifle).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.aiStyle = ProjAIStyleID.HeldProjectile;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.ignoreWater = true;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			OriginPlayer originPlayer = player.OriginPlayer();
			if (Projectile.ai[2] != 0) {
				SoundEngine.PlaySound(player.HeldItem.UseSound, Projectile.position);
				Projectile.ai[2] = 0;
			}
			if (Projectile.ai[0] == 0 && originPlayer.currentActiveHarpoons == 0) {
				if (player.reuseDelay > 0) player.reuseDelay--;
				if (player.reuseDelay <= 0) Projectile.ai[1] = 3;
			}
			if (Main.myPlayer == Projectile.owner) {
				if ((player.channel || originPlayer.currentActiveHarpoons != 0 || Projectile.ai[0] > 0) && !player.noItems && !player.CCed) {
					Vector2 position = player.MountedCenter + ((Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 12).Floor();
					Projectile.position = position;


					consumed = false;
					if (Projectile.ai[1] > 0 && --Projectile.ai[0] <= 0 && player.PickAmmo(player.HeldItem, out int projToShoot, out float speed, out int damage, out float knockBack, out int usedAmmoItemId)) {
						EntitySource_ItemUse_WithAmmo projectileSource = new(player, player.HeldItem, usedAmmoItemId);

						Vector2 direction = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY) - position;
						if (player.gravDir == -1f) direction.Y = (Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - position.Y;

						Vector2 velocity = Vector2.Normalize(direction);
						if (velocity.HasNaNs()) velocity = -Vector2.UnitY;
						velocity *= speed;

						CombinedHooks.ModifyShootStats(player, player.HeldItem, ref position, ref velocity, ref projToShoot, ref damage, ref knockBack);
						if (CombinedHooks.Shoot(player, player.HeldItem, projectileSource, position, velocity, projToShoot, damage, knockBack)) {
							Projectile.NewProjectile(projectileSource, position, velocity, projToShoot, damage, knockBack, Projectile.owner, ai1: consumed ? 1 : 0);
						}
						Projectile.ai[2] = 1;
						Projectile.ai[1]--;
						if (Projectile.ai[1] > 0) Projectile.ai[0] = CombinedHooks.TotalUseTime(player.HeldItem.useTime, player, player.HeldItem);
					}
				} else {
					if (player.reuseDelay > 0) player.reuseDelay--;
					if (player.reuseDelay <= 0) Projectile.Kill();
					Vector2 position = player.MountedCenter + ((Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 12).Floor();
					Projectile.position = position;
				}
			}
			if (originPlayer.currentActiveHarpoons > 0) {
				Vector2 velocity = Projectile.position.DirectionTo(originPlayer.currentActiveHarpoonAveragePosition);
				if (!velocity.HasNaNs()) Projectile.velocity = velocity;
			}
			Projectile.position.Y += player.gravDir * 2f;
		}
		public override bool ShouldUpdatePosition() => false;
		public override bool PreDraw(ref Color lightColor) {
			SpriteEffects dir = Main.player[Projectile.owner].direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			if (Main.player[Projectile.owner].gravDir == -1f) {
				dir ^= SpriteEffects.FlipVertically;
			}
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = new(27, 25);
			Main.EntitySpriteDraw(
				texture,
				Projectile.position - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation - MathHelper.PiOver2,
				origin.Apply(dir, texture.Size()),
				Projectile.scale,
				dir
			);
			return false;
		}
	}
}
