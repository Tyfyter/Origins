using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Other.Testing;
using Origins.Projectiles;
using Origins.Tiles.Other;
using PegasusLib;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Brine {
	public class Peat_Moss : OriginTile {
		public string[] Categories => [
			"Grass"
		];
		public override void SetStaticDefaults() {
			//Main.tileMergeDirt[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			RegisterItemDrop(ItemType<Peat_Moss_Item>());
			AddMapEntry(new Color(18, 160, 56));
			HitSound = SoundID.Dig;
			DustType = DustID.GrassBlades;
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (!fail) {
				Projectile.NewProjectile(
					WorldGen.GetItemSource_FromTileBreak(i, j),
					new Vector2(i * 16 + 8, j * 16 + 8),
					Vector2.Zero,
					ProjectileType<Peat_Moss_Tile_Explosion>(),
					20 + (int)(10 * ContentExtensions.DifficultyDamageMultiplier),
					4
				);
			}
		}
		public override void RandomUpdate(int i, int j) {
			if (!Framing.GetTileSafely(i, j + 1).HasTile) {
				if (TileObject.CanPlace(i, j + 1, TileType<Brineglow>(), 0, 0, out TileObject objectData, false, checkStay: true)) {
					objectData.style = 0;
					objectData.alternate = 0;
					objectData.random = 0;
					TileObject.Place(objectData);
				}
			}
		}
	}
	public class Peat_Moss_Item : MaterialItem {
		public override int ResearchUnlockCount => 100;
		public override int Value => Item.sellPrice(copper: 60);
		public override int Rare => ItemRarityID.Green;
		public override bool Hardmode => false;
		public override bool HasTooltip => true;
		public override void AddRecipes() {
			Recipe.Create(ItemID.ExplosivePowder)
			.AddIngredient(this, 2)
			.AddTile(TileID.GlassKiln)
			.DisableDecraft()
			.Register();
		}
	}
	public class Peat_Moss_Tile_Explosion : ExplosionProjectile {
		public override DamageClass DamageType => DamageClasses.Explosive;
		public override int Size => 48;
		public override SoundStyle? Sound => SoundID.Item14.WithVolume(0.66f);
		public override bool Hostile => true;
		public override int FireDustAmount => 0;
		public override int SmokeDustAmount => 15;
		public override int SmokeGoreAmount => 2;
		public override int SelfDamageCooldownCounter => ImmunityCooldownID.TileContactDamage;
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.friendly = true;
			Projectile.trap = true;
		}
	}
	public class Peat_Moss_Debug_Item : TestingItem {
		public override string Texture => typeof(Peat_Moss_Item).GetDefaultTMLName();
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Peat_Moss>());
		}
	}
}
