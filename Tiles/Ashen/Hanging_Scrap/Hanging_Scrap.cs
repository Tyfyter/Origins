using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Items.Weapons.Ammo;
using PegasusLib.Networking;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Tiles.Ashen.Hanging_Scrap {
	public class No_Hanging_Scrap : HangingScrap {
		public override string Texture => "Terraria/Images/NPC_0";
		public override int ScrapValue => 0;
		public override Vector2 Origin => default;
	}
	public class Large_Scrap1 : HangingScrap {
		public override int ScrapValue => Main.rand.Next(3, 10);
		public override Vector2 Origin => new(35, 57);
	}
	public class Large_Scrap2 : HangingScrap {
		public override int ScrapValue => Main.rand.Next(3, 10);
		public override Vector2 Origin => new(35, 57);
	}
	public class Large_Scrap3 : HangingScrap {
		public override int ScrapValue => Main.rand.Next(3, 10);
		public override Vector2 Origin => new(35, 57);
	}
	public class Large_Scrap4 : HangingScrap {
		public override int ScrapValue => Main.rand.Next(3, 10);
		public override Vector2 Origin => new(35, 57);
	}
	public class Large_Scrap5 : HangingScrap {
		public override int ScrapValue => Main.rand.Next(3, 10);
		public override Vector2 Origin => new(35, 57);
	}
	public class Large_Scrap6 : HangingScrap {
		public override int ScrapValue => Main.rand.Next(3, 10);
		public override Vector2 Origin => new(35, 57);
	}
	public class Small_Scrap1 : HangingScrap {
		public override int ScrapValue => Main.rand.Next(1, 5);
		public override Vector2 Origin => new(16, 16);
	}
	public class Small_Scrap2 : HangingScrap {
		public override int ScrapValue => Main.rand.Next(1, 5);
		public override Vector2 Origin => new(16, 16);
	}
	public class Small_Scrap3 : HangingScrap {
		public override int ScrapValue => Main.rand.Next(1, 5);
		public override Vector2 Origin => new(16, 16);
	}
	[Autoload(false)]
	public class Hanging_Scrap_Item(HangingScrap scrap) : ModItem, ICustomPlaceTileItem {
		protected override bool CloneNewInstances => true;
		public override string Texture => scrap.Texture;
		public override string Name => scrap.Name + "_Item";
		public override void SetStaticDefaults() {
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Hanging_Scrap_Tile>());
		}
		public void PlaceTile(On_Player.orig_PlaceThing_Tiles orig, bool inRange) {
			if (!inRange) return;
			Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
			if (tile.HasTile) {
				if (tile.TileType == Item.createTile || tile.Get<Hanging_Scrap_Data>().HasScrap) return;
				Main.LocalPlayer.ApplyItemTime(Item, Main.LocalPlayer.tileSpeed);
			} else {
				orig(Main.LocalPlayer);
				if (tile.TileType != Item.createTile) return;
			}
			new Hanging_Scrap_Action(new(Player.tileTargetX, Player.tileTargetY), new(scrap.Type, (Half)Main.rand.NextFloat(float.Tau))).Perform();
		}
	}
	public class Hanging_Scrap_Tile : ModTile {
		public override string Texture => "Terraria/Images/NPC_0";
		public override void SetStaticDefaults() {
			TileID.Sets.DoesntPlaceWithTileReplacement[Type] = true;
			Main.tileNoFail[Type] = true;
			MineResist = 2;
			HitSound = SoundID.NPCHit42.WithPitch(1.5f).WithVolume(0.5f);
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) => false;
	}
	public abstract class HangingScrap : ModTexturedType {
		static readonly List<HangingScrap> scrap = [];
		public byte Type { get; private set; }
		public Asset<Texture2D> Texture2D { get; private set; }
		protected sealed override void Register() {
			if (this is No_Hanging_Scrap) {
				Type = 0;
				scrap.Insert(0, this);
				return;
			}
			int type = scrap.Count;
			if (!scrap.Any(s => s is No_Hanging_Scrap)) type++;
			if (type > byte.MaxValue) return;
			Type = (byte)type;
			scrap.Add(this);
			Mod.AddContent(new Hanging_Scrap_Item(this));
		}
		public sealed override void SetupContent() {
			Texture2D = ModContent.Request<Texture2D>(Texture);
			SetStaticDefaults();
		}
		public static HangingScrap Get(int type) => scrap[type];
		public static HangingScrap Get(byte type) => scrap[type];
		public abstract int ScrapValue { get; }
		public abstract Vector2 Origin { get; }
	}
	[StructLayout(LayoutKind.Explicit, Size = 4)]
	public record struct Hanging_Scrap_Data([field: FieldOffset(0)] byte ScrapType, [field: FieldOffset(2)] Half Rotation) : ITileData {
		public readonly bool HasScrap => ScrapType != 0;
		public readonly HangingScrap Scrap => HangingScrap.Get(ScrapType);
		public readonly void Write(BinaryWriter writer) {
			writer.Write(ScrapType);
			writer.Write(Rotation);
		}
		public static Hanging_Scrap_Data Read(BinaryReader reader) => new(reader.ReadByte(), reader.ReadHalf());
	}
	public class Hanging_Scrap_Global_Tile : GlobalTile {
		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (!effectOnly) {
				new Hanging_Scrap_Action(new(i, j), new()).Perform();
			}
		}
		public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			if (!Main.tile[i, j].Get<Hanging_Scrap_Data>().HasScrap) return;
			((TileID.Sets.DrawTileInSolidLayer[type] ?? Main.tileSolid[type]) ? Hanging_Scrap_Overlay.fromSolids : Hanging_Scrap_Overlay.fromNonSolids).Add((i, j));
		}
	}
	public class Hanging_Scrap_System : ModSystem {
		public override void SaveWorldData(TagCompound tag) {
			using MemoryStream data = new(Main.maxTilesX);
			using (BinaryWriter writer = new(data, Encoding.UTF8)) {
				writer.Write((byte)1); // version just in case 
									   // if Hanging_Scrap_Data is updated, update this 'version' number 
									   // and add handling logic in LoadWorldData for backwards compat
				SerializeScrap(writer);
			}
			tag["HangingScrap"] = data.ToArray();
		}
		public override void LoadWorldData(TagCompound tag) {
			if (tag.TryGet("HangingScrap", out byte[] data)) {
				using BinaryReader reader = new(new MemoryStream(data), Encoding.UTF8);
				byte version = reader.ReadByte();
				switch (version) {
					case 1:
					DeserializeScrap(reader);
					break;
					case 0:
					Span<byte> worldData = MemoryMarshal.Cast<Hanging_Scrap_Data, byte>(Main.tile.GetData<Hanging_Scrap_Data>().AsSpan());
					reader.Read(worldData);
					break;
					default:// add more cases for newer versions of the data
					throw new Exception("Unknown world data saved version");
				}
			}
		}
		public static void SerializeScrap(BinaryWriter writer) {
			writer.Write(checked((ushort)Main.maxTilesX));
			writer.Write(checked((ushort)Main.maxTilesY));
			int scrapCount = 0;
			int scrapThreshold = (Main.maxTilesX * Main.maxTilesY) / 2;
			foreach (Hanging_Scrap_Data data in Main.tile.GetData<Hanging_Scrap_Data>().AsSpan()) {
				if (data.HasScrap && ++scrapCount >= scrapThreshold) break;
			}
			writer.Write(scrapCount >= scrapThreshold);
			if (scrapCount >= scrapThreshold) {
				for (int i = 0; i < Main.maxTilesX; i++) {
					for (int j = 0; j < Main.maxTilesY; j++) {
						Main.tile[i, j].Get<Hanging_Scrap_Data>().Write(writer);
					}
				}
			} else {
				writer.Write((int)scrapCount);
				for (int i = 0; i < Main.maxTilesX; i++) {
					for (int j = 0; j < Main.maxTilesY; j++) {
						Hanging_Scrap_Data data = Main.tile[i, j].Get<Hanging_Scrap_Data>();
						if (data.HasScrap) {
							writer.Write((ushort)i);
							writer.Write((ushort)j);
							data.Write(writer);
						}
					}
				}
			}
		}
		public static void DeserializeScrap(BinaryReader reader) {
			int width = reader.ReadUInt16();
			int height = reader.ReadUInt16();
			if (width != Main.maxTilesX || height != Main.maxTilesY) {
				// the world was somehow resized
				// up to you what to do here 
				throw new NotImplementedException("World size was changed");
			} else {
				if (reader.ReadBoolean()) {
					Span<byte> worldData = MemoryMarshal.Cast<Hanging_Scrap_Data, byte>(Main.tile.GetData<Hanging_Scrap_Data>().AsSpan());
					reader.Read(worldData);
				} else {
					for (int i = reader.ReadInt32(); i > 0; i--) {
						Main.tile[reader.ReadUInt16(), reader.ReadUInt16()].Get<Hanging_Scrap_Data>() = Hanging_Scrap_Data.Read(reader);
					}
				}
			}
		}
		public override void NetSend(BinaryWriter writer) {
			SerializeScrap(writer);
		}
		public override void NetReceive(BinaryReader reader) {
			DeserializeScrap(reader);
		}
	}
	public class Hanging_Scrap_Overlay() : Overlay(EffectPriority.VeryLow, RenderLayers.Walls), ILoadable {
		public static List<(int i, int j)> fromSolids = [];
		public static List<(int i, int j)> fromNonSolids = [];
		public override void Draw(SpriteBatch spriteBatch) {
			for (int k = 0; k < fromSolids.Count; k++) {
				(int i, int j) = fromSolids[k];
				DrawScrap(spriteBatch, i, j);
			}
			for (int k = 0; k < fromNonSolids.Count; k++) {
				(int i, int j) = fromNonSolids[k];
				DrawScrap(spriteBatch, i, j);
			}
		}
		public static void DrawScrap(SpriteBatch spriteBatch, int i, int j) {
			Hanging_Scrap_Data scrap = Main.tile[i, j].Get<Hanging_Scrap_Data>();
			if (!scrap.HasScrap) return;
			Vector2 pos = new Vector2(i * 16 + 8, j * 16 + 8) - Main.screenPosition;
			HangingScrap hangingScrap = HangingScrap.Get(scrap.ScrapType);
			spriteBatch.Draw(
				hangingScrap.Texture2D.Value,
				pos,
				null,
				Lighting.GetColor(i, j),
				(float)scrap.Rotation,
				hangingScrap.Origin,
				1,
				0,
			0);
		}
		public override void Update(GameTime gameTime) { }
		public override void Activate(Vector2 position, params object[] args) {
			Opacity = 1;
			Mode = OverlayMode.Active;
		}
		public override void Deactivate(params object[] args) { }
		public override bool IsVisible() => true;
		public static void ForceActive() {
			if (Overlays.Scene[typeof(Hanging_Scrap_Overlay).FullName].Mode != OverlayMode.Active) {
				Overlays.Scene.Activate(typeof(Hanging_Scrap_Overlay).FullName, default);
			}
		}
		public void Load(Mod mod) {
			Overlays.Scene[GetType().FullName] = this;
			On_TileDrawing.PreDrawTiles += On_TileDrawing_PreDrawTiles;
		}

		static void On_TileDrawing_PreDrawTiles(On_TileDrawing.orig_PreDrawTiles orig, TileDrawing self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets) {
			orig(self, solidLayer, forRenderTargets, intoRenderTargets);
			if (!intoRenderTargets && !Lighting.UpdateEveryFrame) return;
			if (solidLayer) {
				fromSolids.Clear();
			} else {
				fromNonSolids.Clear();
			}
		}

		public void Unload() { }
	}
	public record class Hanging_Scrap_Action(Point16 Pos, Hanging_Scrap_Data ScrapData) : SyncedAction {
		protected override bool ShouldPerform => Main.tile[Pos].Get<Hanging_Scrap_Data>() != ScrapData;
		public Hanging_Scrap_Action() : this(default, default) { }
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			Pos = new(reader.ReadInt16(), reader.ReadInt16()),
			ScrapData = Hanging_Scrap_Data.Read(reader)
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write((short)Pos.X);
			writer.Write((short)Pos.Y);
			ScrapData.Write(writer);
		}
		protected override void Perform() {
			ref Hanging_Scrap_Data data = ref Main.tile[Pos].Get<Hanging_Scrap_Data>();
			int valueChange = data.Scrap.ScrapValue - ScrapData.Scrap.ScrapValue;
			data = ScrapData;
			if (valueChange > 0 && !NetmodeActive.MultiplayerClient) {
				int item = Item.NewItem(
					WorldGen.GetItemSource_FromTileBreak(Pos.X, Pos.Y),
					Pos.ToWorldCoordinates(),
					ModContent.ItemType<Scrap>(),
					valueChange
				);
				if (!NetmodeActive.SinglePlayer) {
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
				}
			}
		}
	}
}
