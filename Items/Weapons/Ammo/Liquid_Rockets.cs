using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils;
using Origins.Dusts;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Demolitionist;
using Origins.Liquids;
using Origins.Projectiles.Weapons;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod.Projectiles;

namespace Origins.Items.Weapons.Ammo {
	#region Base Class
	public abstract class BaseLiquidRocket<TLiquid, TDust> : BaseLiquidRocket where TLiquid : ModLiquid where TDust : ModDust {
		public override int LiquidType => LiquidLoader.LiquidType<TLiquid>();
		public override int DustType => ModContent.DustType<TDust>();
	}
	public abstract class BaseLiquidRocket<TLiquid> : BaseLiquidRocket where TLiquid : ModLiquid {
		public override int LiquidType => LiquidLoader.LiquidType<TLiquid>();
	}
	public abstract class BaseLiquidRocket : ModItem {
		public abstract int LiquidType { get; }
		public abstract int DustType { get; }
		public virtual Color DustColor { get; }
		protected override bool CloneNewInstances => true;
		public override void Load() {
			Mod.AddContent(new BaseLiquidRocketP(this));
			Mod.AddContent(new BaseLiquidGrenade(this));
			Mod.AddContent(new BaseLiquidMine(this));
			Mod.AddContent(new BaseLiquidSnowmanRocket(this));
		}
		public override void SetStaticDefaults() {
			AmmoID.Sets.IsSpecialist[Type] = true;
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.Celeb2].Add(Type, ProjectileID.Celeb2Rocket);

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
			SafeSetStaticDefaults();
		}
		public virtual void SafeSetStaticDefaults() { }
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LavaRocket);
			SafeSetDefaults();
		}
		public virtual void SafeSetDefaults() { }

		public override void AddRecipes() {
			Recipe.Create(ItemID.DryRocket)
			.AddIngredient(Type)
			.SortAfterFirstRecipesOf(ItemID.DryRocket)
			.Register();

			CreateRecipe()
			.AddIngredient(ItemID.DryRocket)
			.AddLiquid(LiquidType)
			.Register();
		}
	}
	[Autoload(false)]
	public class BaseLiquidRocketP(BaseLiquidRocket item) : ModProjectile {
		[field: CloneByReference]
		BaseLiquidRocket Item { get; } = item;
		public override string Name => Item.Name + "_P";
		public override string Texture => Item.Texture + "_P";
		public override LocalizedText DisplayName => Item.DisplayName;
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher].Add(Item.Type, Type);
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.DryRocket);
			AIType = ProjectileID.DryRocket;
			//Origins.instance.Logger.Info($"Projectile: Name: {Name}, Type: {Type}, Texture: {Texture}");
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.Kill();
			return true;
		}
		public override void OnKill(int timeLeft) {
			BaseLiquidExplosiveKill(Projectile, Item.LiquidType, Item.DustType, Item.DustColor);
		}

		public static void BaseLiquidExplosiveKill(Projectile proj, int liquid, int dust, Color color, bool isGrenade = false) {
			proj.Resize(22, 22);
			if (isGrenade) SoundEngine.PlaySound(in SoundID.Item62, proj.position);
			else SoundEngine.PlaySound(in SoundID.Item14, proj.position);

			for (int i = 0; i < 30; i++) {
				Dust.NewDustDirect(proj.position, proj.width, proj.height, DustID.Smoke, 0f, 0f, 100, color, 1.5f)
				.velocity *= 1.4f;
			}
			for (int i = 0; i < 80; i++) {
				Dust.NewDustDirect(proj.position, proj.width, proj.height, dust, 0f, 0f, 100, color, 2.2f)
				.velocity *= 7f;
				Dust.NewDustDirect(proj.position, proj.width, proj.height, dust, 0f, 0f, 100, color, 1.3f)
				.velocity *= 4f;
			}
			for (int i = 1; i <= 2; i++) {
				for (int x = -1; x <= 1; x += 2) {
					for (int y = -1; y <= 1; y += 2) {
						Gore gore = Gore.NewGoreDirect(new EntitySource_Death(proj), proj.position, Vector2.Zero, Main.rand.Next(61, 64));
						gore.velocity *= i == 1 ? 0.4f : 0.8f;
						gore.velocity += new Vector2(x, y);
					}
				}
			}
			if (!NetmodeActive.MultiplayerClient) {
				Point projPos = proj.Center.ToTileCoordinates();
				proj.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks(projPos, 3f, SpreadLiquid(liquid, dust));
			}
		}
		public static Utils.TileActionAttempt SpreadLiquid<TLiquid, TDust>(Func<Color> color = null) where TLiquid : ModLiquid where TDust : ModDust {
			return SpreadLiquid(LiquidLoader.LiquidType<TLiquid>, ModContent.DustType<TDust>, color);
		}
		public static Utils.TileActionAttempt SpreadLiquid<TLiquid>(Func<int> dust, Func<Color> color = null) where TLiquid : ModLiquid {
			return SpreadLiquid(LiquidLoader.LiquidType<TLiquid>, dust, color);
		}
		public static Utils.TileActionAttempt SpreadLiquid(int liquid, int dust, Color color = default) {
			return SpreadLiquid(() => liquid, () => dust, () => color);
		}
		public static Utils.TileActionAttempt SpreadLiquid(Func<int> liquid, Func<int> dust, Func<Color> color = null) {
			bool SpreadLiquid(int x, int y) {
				if (Vector2.Distance(DelegateMethods.v2_1, new Vector2(x, y)) > DelegateMethods.f_1)
					return false;

				Color dustColor = color?.Invoke() ?? Color.Transparent;
				if (WorldGen.PlaceLiquid(x, y, (byte)liquid(), byte.MaxValue)) {
					Vector2 position = new(x * 16, y * 16);
					int dustType = dust();
					for (int i = 0; i < 3; i++) {
						Dust dust = Dust.NewDustDirect(position, 16, 16, dustType, 0f, 0f, 100, dustColor, 2.2f);
						dust.velocity.Y -= 1.2f;
						dust.velocity *= 7f;

						dust = Dust.NewDustDirect(position, 16, 16, dustType, 0f, 0f, 100, dustColor, 1.3f);
						dust.velocity.Y -= 1.2f;
						dust.velocity *= 4f;
					}
					return true;
				}
				return false;
			}
			return SpreadLiquid;
		}
	}
	[Autoload(false)]
	public class BaseLiquidGrenade(BaseLiquidRocket item) : ModProjectile {
		[field: CloneByReference]
		BaseLiquidRocket Item { get; } = item;
		public override string Name => Item.Name + "_Grenade_P";
		public override string Texture => Item.Texture + "_Grenade_P";
		public override LocalizedText DisplayName => Item.DisplayName;
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.GrenadeLauncher].Add(Item.Type, Type);
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.DryGrenade);
			AIType = ProjectileID.DryGrenade;
			Projectile.timeLeft = 180;
			//Origins.instance.Logger.Info($"Projectile: Name: {Name}, Type: {Type}, Texture: {Texture}");
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = false;
			return true;
		}
		public override void OnKill(int timeLeft) {
			BaseLiquidRocketP.BaseLiquidExplosiveKill(Projectile, Item.LiquidType, Item.DustType, Item.DustColor, true);
		}
	}
	[Autoload(false)]
	public class BaseLiquidMine(BaseLiquidRocket item) : ModProjectile {
		[field: CloneByReference]
		BaseLiquidRocket Item { get; } = item;
		public override string Name => Item.Name + "_Mine_P";
		public override string Texture => Item.Texture + "_Mine_P";
		public override LocalizedText DisplayName => Item.DisplayName;
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.ProximityMineLauncher].Add(Item.Type, Type);
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.DryMine);
			AIType = ProjectileID.DryMine;
			//Origins.instance.Logger.Info($"Projectile: Name: {Name}, Type: {Type}, Texture: {Texture}");
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = false;
			return true;
		}
		public override void OnKill(int timeLeft) {
			BaseLiquidRocketP.BaseLiquidExplosiveKill(Projectile, Item.LiquidType, Item.DustType, Item.DustColor);
		}
	}
	[Autoload(false)]
	public class BaseLiquidSnowmanRocket(BaseLiquidRocket item) : ModProjectile {
		[field: CloneByReference]
		BaseLiquidRocket Item { get; } = item;
		public override string Name => Item.Name + "_Snowman_P";
		public override string Texture => Item.Texture + "_Snowman_P";
		public override LocalizedText DisplayName => Item.DisplayName;
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.SnowmanCannon].Add(Item.Type, Type);
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.DrySnowmanRocket);
			AIType = ProjectileID.DrySnowmanRocket;
			//Origins.instance.Logger.Info($"Projectile: Name: {Name}, Type: {Type}, Texture: {Texture}");
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.Kill();
			return true;
		}
		public override void OnKill(int timeLeft) {
			BaseLiquidRocketP.BaseLiquidExplosiveKill(Projectile, Item.LiquidType, Item.DustType, Item.DustColor);
		}
	}
	#endregion
	public class Oil_Rocket : BaseLiquidRocket<Oil> {
		public override int DustType => GetDustType();
		public override Color DustColor => GetDustColor();
		public static int GetDustType() => OriginsModIntegrations.CheckAprilFools() ? ModContent.DustType<Black_Smoke_Dust>() : White_Water_Dust.ID;
		public static Color GetDustColor() => OriginsModIntegrations.CheckAprilFools() ? default : FromHexRGB(0x1B1B1B);
	}
	public class Brine_Rocket : BaseLiquidRocket<Brine> {
		public override string Texture => typeof(Oil_Rocket).GetDefaultTMLName();
		public override int DustType => DustID.Water_Jungle;
	}
}
