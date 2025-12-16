using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Projectiles;
using Origins.Tiles.Cubekon;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
    public class Flakjack : ModItem, ICustomDrawItem {
        public static AutoCastingAsset<Texture2D> UseTexture { get; private set; }
		public static AutoCastingAsset<Texture2D> UseGlowTexture { get; private set; }
		public override void Unload() {
			UseTexture = null;
			UseGlowTexture = null;
		}
		static short glowmask;
		public override void SetStaticDefaults() {
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				global.SetUpdateCountBoost(proj, global.UpdateCountBoost + 2);
			});
			if (!Main.dedServ) {
				UseTexture = Mod.Assets.Request<Texture2D>("Items/Weapons/Demolitionist/Flakjack_Use");
				UseGlowTexture = Mod.Assets.Request<Texture2D>("Items/Weapons/Demolitionist/Flakjack_Use_Glow");
			}
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SniperRifle);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.glowMask = glowmask;
			Item.damage = 192;
			Item.crit = 14;
			Item.useAnimation = 32;
			Item.useTime = 17;
			Item.useAmmo = ModContent.ItemType<Metal_Slug>();
			Item.shoot = ModContent.ProjectileType<Flakjack_P>();
			Item.shootSpeed = 12;
			Item.reuseDelay = 6;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(gold: 15);
			Item.rare = ButterscotchRarity.ID;
		}
        public override void AddRecipes() {
            Recipe.Create(Type)
            .AddIngredient(ModContent.ItemType<Fibron_Plating>(), 24)
            .AddIngredient(ModContent.ItemType<Qube_Item>(), 48)
            .AddTile(TileID.LunarCraftingStation) // Interstellar Sampler
            .Register();
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Item40, player.itemLocation);
			SoundEngine.PlaySound(SoundID.Item36.WithVolume(0.75f), player.itemLocation);
			return null;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 perp = velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(default);
			if (player.ItemUsesThisAnimation == 1) {
				position += perp * player.direction * 2;
			} else {
				position -= perp * player.direction * 6;
			}
			type = Item.shoot;
		}

		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			float itemRotation = drawPlayer.itemRotation;
			float scale = drawPlayer.GetAdjustedItemScale(Item);

			Vector2 pos = new((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

			int frame = 0;
			int useFrame = drawPlayer.itemTimeMax - drawPlayer.itemTime;
			switch (drawPlayer.ItemUsesThisAnimation) {
				case 1:
				if (useFrame < 3) {
					frame += 1;
				} else if (useFrame < 6) {
					frame += 2;
				} else {
					frame += 3;
				}
				break;

				case 2:
				frame = 3;
				goto case 1;
			}
			frame %= 6;

			drawInfo.DrawDataCache.Add(new DrawData(
				UseTexture,
				pos,
				new Rectangle(0, 30 * frame, 72, 28),
				Item.GetAlpha(lightColor),
				itemRotation,
				drawOrigin,
				scale,
				drawInfo.itemEffect,
			0));

			drawInfo.DrawDataCache.Add(new DrawData(
				UseGlowTexture,
				pos,
				new Rectangle(0, 30 * frame, 72, 28),
				Item.GetAlpha(Color.White),
				itemRotation,
				drawOrigin,
				scale,
				drawInfo.itemEffect,
			0));
		}
	}
	public class Flakjack_Explosion_P : ModProjectile, IIsExplodingProjectile {
		public override string Texture => "Terraria/Images/Projectile_16";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.timeLeft = 5;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 0;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.hide = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(
					Projectile,
					true,
					sound: SoundID.Item62
				);
				Projectile.ai[0] = 1;
			}
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding => true;
	}
	public class Flakjack_P : ModProjectile, IIsExplodingProjectile {
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 40;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.MaxUpdates = 10;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.timeLeft = 420;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
			Projectile.alpha = 255;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.velocity *= 0.2f;
			Projectile.GetGlobalProjectile<ExplosiveGlobalProjectile>().magicTripwire = true;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
		}
		public override Color? GetAlpha(Color lightColor) {
			if (Projectile.alpha < 200) {
				return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, (255 - Projectile.alpha) / 2);
			}
			return Color.Transparent;
		}
		public override void OnKill(int timeLeft) {
			if (Projectile.owner == Main.myPlayer) {
				Projectile.NewProjectileDirect(
					Projectile.GetSource_Death(),
					Projectile.Center,
					default,
					ModContent.ProjectileType<Flakjack_Explosion_P>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner
				);
			}
		}
		public bool IsExploding => false;
	}
}
