using AltLibrary.Common.AltBiomes;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
	[ReinitializeDuringResizeArrays]
	public sealed class Chambersite_Stone_Wall : ModWall {
		public static Dictionary<ushort, ushort> AddChambersite = [];
		public static List<int> chambersiteWalls = [];
		public static int[] wallCounts = WallID.Sets.Factory.CreateIntSet();
		public static ActionZipper<ushort> baseWallTypes = new(
			WallID.AmethystUnsafe,
			WallID.TopazUnsafe,
			WallID.SapphireUnsafe,
			WallID.EmeraldUnsafe,
			WallID.RubyUnsafe,
			WallID.DiamondUnsafe
		);
		public override void Unload() {
			AddChambersite = null;
		}
		public override void Load() {
			try {
				IL_SceneMetrics.ScanAndExportToMain += il => {
					ILCursor c = new(il);
					int loc = -1;
					c.GotoNext(MoveType.AfterLabel,
						i => i.MatchLdloca(out loc),
						i => i.MatchCall<Tile>("active"),
						i => i.MatchBrtrue(out _)
					);
					c.EmitLdloc(loc);
					c.EmitDelegate((Tile tile) => {
						wallCounts[tile.WallType]++;
					});
				};
			} catch (Exception ex) {
				if (Origins.LogLoadingILError($"{nameof(Chambersite_Stone_Wall)}.CountWalls", ex)) throw;
			}
			//IL_00c6: ldloca.s 7
			//IL_00c8: call instance bool Terraria.Tile::active()
			//IL_00cd: brtrue.s IL_00f9
			Mod.AddContent(new WallItem(this));
		}

		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallID.Stone;//what wall type this wall is considered to be when blending
			AddMapEntry(GetWallMapColor(WallID.Stone));
			DustType = DustID.GemEmerald;
			//AddChambersite.Add(WallID.Stone, Type);
			chambersiteWalls.Add(Type);
			OriginsModIntegrations.SetupContent();
		}
		public override bool Drop(int i, int j, ref int type) {
			return false;
		}
		public static void AddChild(int type, AltBiome biome) {
			biome.AddWallConversions(type, WallType<Chambersite_Stone_Wall>());
		}
	}
	[Autoload(false)]
	public class Auto_Chambersite_Wall(ModWall baseWall, Color mapColor, Func<AltBiome> biome) : ModWall {
		public ModWall BaseWall { get; } = baseWall;
		public override string Name => "Chambersite_" + BaseWall.Name;
		public override string Texture => BaseWall.Texture;
		public override void Load() => Mod.AddContent(new Auto_Chambersite_Wall_Item(this));
		static AutoLoadingAsset<Texture2D> overlay = "Origins/Walls/Overlays/Chambersite/Chambersite_Ore_Wall";
		private static VertexColors _glowPaintColors = new(Color.White);
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = BaseWall.Type;//what wall type this wall is considered to be when blending
			AddMapEntry(mapColor);
			Chambersite_Stone_Wall.AddChild(Type, biome());
			Chambersite_Stone_Wall.baseWallTypes.Add(GemWallAction);
			Chambersite_Stone_Wall.AddChambersite.Add(BaseWall.Type, Type);
			DustType = DustID.GemEmerald;
			Chambersite_Stone_Wall.chambersiteWalls.Add(Type);
		}
		void GemWallAction(ushort baseWall) {
			biome().AddWallConversions(Type, baseWall);
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];
			VertexColors vertices;
			if (tile.IsWallFullbright) {
				vertices = _glowPaintColors;
			} else {
				Lighting.GetCornerColors(i, j, out vertices);
			}
			Vector2 offset = new(Main.drawToScreen ? 0 : Main.offScreenRange);
			Rectangle frame = new(tile.WallFrameX, tile.WallFrameY + Main.wallFrame[tile.WallType] * 180, 32, 32);
			Main.tileBatch.Draw(overlay, new Vector2(i * 16 - (int)Main.screenPosition.X - 8, j * 16 - (int)Main.screenPosition.Y - 8) + offset, frame, vertices, Vector2.Zero, 1f, SpriteEffects.None);
		}
	}
	public class Auto_Chambersite_Wall_Item(ModWall wall) : WallItem(wall) {
		public override string Texture => Wall.Texture + "_Item";
		static AutoLoadingAsset<Texture2D> overlay = "Origins/Walls/Overlays/Chambersite/Chambersite_Ore_Wall_Item";
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
			Vector2 origin = overlay.Value.Size() * 0.5f;
			Rectangle originalFrame;
			{
				Texture2D texture = TextureAssets.Item[Type].Value;
				if (Main.itemAnimations[Type] != null) {
					originalFrame = Main.itemAnimations[Type].GetFrame(texture, Main.itemFrameCounter[whoAmI]);
				} else {
					originalFrame = texture.Frame();
				}
			}
			Vector2 vector2 = new((Item.width / 2) - originalFrame.Width * 0.5f, Item.height - originalFrame.Height);
			Vector2 position = Item.position - Main.screenPosition + originalFrame.Size() * 0.5f + vector2;
			spriteBatch.Draw(overlay, position, null, alphaColor, rotation, origin, scale, SpriteEffects.None, 0f);
			if (Item.shimmered) {
				spriteBatch.Draw(overlay, position, null, alphaColor with { A = 0 }, rotation, origin, scale, SpriteEffects.None, 0f);
			}
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			spriteBatch.Draw(overlay, position, frame, drawColor, 0f, overlay.Value.Size() * 0.5f, scale, SpriteEffects.None, 0f);
		}
	}
	public class Chambersite_Crimstone_Wall : ModWall {
		public override void Load() => Mod.AddContent(new Auto_Chambersite_Wall_Item(this));
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallID.CrimstoneEcho;//what wall type this wall is considered to be when blending
			AddMapEntry(GetWallMapColor(WallID.CrimstoneUnsafe));
			Chambersite_Stone_Wall.AddChild(Type, GetInstance<CrimsonAltBiome>());
			Chambersite_Stone_Wall.baseWallTypes.Add(baseWall => GetInstance<CrimsonAltBiome>().AddWallConversions(Type, baseWall));
			Chambersite_Stone_Wall.AddChambersite.Add(WallID.CrimstoneUnsafe, Type);
			DustType = DustID.GemEmerald;
			Chambersite_Stone_Wall.chambersiteWalls.Add(Type);
		}
	}
	public class Chambersite_Ebonstone_Wall : ModWall {
		public override void Load() => Mod.AddContent(new Auto_Chambersite_Wall_Item(this));
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallID.EbonstoneEcho;//what wall type this wall is considered to be when blending
			AddMapEntry(GetWallMapColor(WallID.EbonstoneUnsafe));
			Chambersite_Stone_Wall.AddChild(Type, GetInstance<CorruptionAltBiome>());
			Chambersite_Stone_Wall.baseWallTypes.Add(baseWall => GetInstance<CorruptionAltBiome>().AddWallConversions(Type, baseWall));
			Chambersite_Stone_Wall.AddChambersite.Add(WallID.EbonstoneUnsafe, Type);
			DustType = DustID.GemEmerald;
			Chambersite_Stone_Wall.chambersiteWalls.Add(Type);
		}
	}
}
