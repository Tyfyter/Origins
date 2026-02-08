using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils;
using Origins.Dusts;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Demolitionist;
using Origins.Liquids;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	#region Base Class
	public abstract class LiquidBomb<TLiquid, TDust> : LiquidBomb where TLiquid : ModLiquid where TDust : ModDust {
		public override int LiquidType => LiquidLoader.LiquidType<TLiquid>();
		public override int DustType => ModContent.DustType<TDust>();
	}
	public abstract class LiquidBomb<TLiquid> : LiquidBomb where TLiquid : ModLiquid {
		public override int LiquidType => LiquidLoader.LiquidType<TLiquid>();
	}
	public abstract class LiquidBomb : ModItem {
		public abstract int LiquidType { get; }
		public abstract int DustType { get; }
		LiquidBombP projectile;
		protected override bool CloneNewInstances => true;
		public override void Load() {
			Mod.AddContent(projectile = new(this));
		}
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true;

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
			SafeSetStaticDefaults();
		}
		public virtual void SafeSetStaticDefaults() { }
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WetBomb);
			Item.shoot = projectile.Type;
			SafeSetDefaults();
		}
		public virtual void SafeSetDefaults() { }
		public virtual int GetLiquid(int x, int y) => LiquidType;
		public override void AddRecipes() {
			Recipe.Create(ItemID.DryBomb)
			.AddIngredient(Type)
			.SortAfterFirstRecipesOf(ItemID.DryBomb)
			.Register();

			CreateRecipe()
			.AddIngredient(ItemID.DryBomb)
			.AddLiquid(LiquidType)
			.SortAfterFirstRecipesOf(ItemID.HoneyBomb)
			.Register();
		}
	}
	#endregion
	public class Oil_Bomb : LiquidBomb<Oil, Black_Smoke_Dust> { }
}

namespace Origins.Projectiles.Weapons {
	[Autoload(false)]
	public class LiquidBombP(LiquidBomb item) : ModProjectile {
		[field: CloneByReference]
		LiquidBomb Item { get; } = item;
		public override string Name => Item.Name + "_P";
		public override LocalizedText DisplayName => Item.DisplayName;
		public override string Texture => Item.Texture;
		protected override bool CloneNewInstances => true;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.LavaBomb);
			Projectile.aiStyle = ProjAIStyleID.Explosive;
			AIType = ProjectileID.DryBomb;
			Projectile.timeLeft = 180;
			//Origins.instance.Logger.Info($"Projectile: Name: {Name}, Type: {Type}, Texture: {Texture}");
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = false;
			return true;
		}
		//This is to make the bomb emit our liquid's splash at its fuse like other liquid bombs
		public override bool PreAI() {
			if (Projectile.owner != Main.myPlayer || Projectile.timeLeft > 3) {
				if (Main.rand.NextBool(2)) {
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, Item.DustType, 0f, 0f, 100);
					dust.scale = 1f + Main.rand.Next(5) * 0.1f;
					dust.noGravity = true;
					Vector2 center = Projectile.Center;
					Vector2 spinPoint = new(0f, -Projectile.height / 2f - 6);
					double rot = Projectile.rotation;
					dust.position = center + spinPoint.RotatedBy(rot) * 1.1f;
				}
			}
			return true;
		}
		public override void OnKill(int timeLeft) {
			BaseLiquidRocketP.BaseLiquidExplosiveKill(Projectile, Item.LiquidType, Item.DustType);
		}
	}
}
