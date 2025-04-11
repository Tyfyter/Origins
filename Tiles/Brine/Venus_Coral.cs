using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Origins.Items.Materials;
using Origins.Items.Other.Testing;
using Origins.Projectiles;
using Origins.Tiles.Other;
using PegasusLib;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Brine {
	public class Venus_Coral : OriginTile {
		public override void Load() {
			try {
				IL_Collision.StickyTiles += IL_Collision_StickyTiles;
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(IL_Collision_StickyTiles), e)) throw;
			}
		}
		static void IL_Collision_StickyTiles(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.Before, i => i.MatchLdcI4(TileID.Cobweb));
			c.EmitDelegate((int type) => {
				if (type == TileType<Venus_Coral>()) return TileID.Cobweb;
				return type;
			});
		}
		public override void SetStaticDefaults() {
			//Main.tileMergeDirt[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileCut[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			//RegisterItemDrop(ItemType<Venus_Coral_Item>());
			AddMapEntry(new(164, 103, 32));
			HitSound = SoundID.Grass;
			DustType = DustID.BrownMoss;
		}

		public override void RandomUpdate(int i, int j) {
			switch (Main.rand.Next(4)) {
				default:
				j++;
				break;
				case 1:
				j--;
				break;
				case 2:
				i++;
				break;
				case 3:
				i--;
				break;
			}
			Tile growTile = Framing.GetTileSafely(i, j);
			if (!growTile.HasTile && growTile.LiquidAmount >= 255) {
				WorldGen.PlaceTile(i, j, Type);
			}
		}
	}
	/*public class Venus_Coral_Item : MaterialItem {
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
	}*/
	public class Venus_Coral_Debug_Item : TestingItem {
		public override string Texture => typeof(Venus_Coral).GetDefaultTMLName();
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Venus_Coral>());
		}
	}
}
