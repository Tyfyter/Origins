using ModLiquidLib.ModLoader;
using Origins.Liquids;
using Origins.Tiles.Other;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Origins.Tiles.Mechanical {
	#region Base Classes
	[Autoload(false)]
	public sealed class BaseLiquidSensor(BaseLiquidSensorEntity sensor) : ModTile {
		public override string Name => sensor.Name;
		public override string Texture => sensor.GetType().GetDefaultTMLName();
		public TileItem Item { get; private set; }
		public sealed override void Load() {
			Item = new TileItem(this, textureOverride: sensor.ItemTexture)
			.WithExtraStaticDefaults(item => {
				item.ResearchUnlockCount = 5;
				this.DropTileItem(item);
			}).WithExtraDefaults(item => {
				item.CloneDefaults(ItemID.LogicSensor_Liquid);
				item.createTile = Type;
				item.placeStyle = 0;
			}).WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.Cog, 5)
				.AddIngredient(sensor.DropperType)
				.AddIngredient(ItemID.Wire)
				.AddTile(TileID.MythrilAnvil)
				.Register();
			}).RegisterItem();
			sensor.OnLoad(this);
		}
		public override void SetStaticDefaults() {
			TileID.Sets.AllowsSaveCompressionBatching[Type] = true;
			TileID.Sets.DoesntGetReplacedWithTileReplacement[Type] = true;
			Main.tileFrameImportant[Type] = true;
			//Using TileObjectData we use HookPostPlaceMyPlayer to spawn a Tile Entity, using the ModTileEntity's Hook_AfterPlacement to spawn a TEModLogicSensors
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(sensor.Hook_AfterPlacement, -1, 0, processedCoordinates: true);
			TileObjectData.addTile(Type);

			AddMapEntry(sensor.MapColor);
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (!fail && !effectOnly) sensor.Kill(i, j);
		}
	}
	//Here is the Tile Entity for sensors, ported from TELogicSensor to have modded sensors that detects modded liquids
	public abstract class BaseLiquidSensorEntity<TLiquid, TDropper> : BaseLiquidSensorEntity where TLiquid : ModLiquid where TDropper : BaseMagicDropper {
		public override int LiquidType => LiquidLoader.LiquidType<TLiquid>();
		public override int DropperType => TileItem.Get<TDropper>().Type;
	}
	public abstract class BaseLiquidSensorEntity : ModTileEntity {
		public abstract Color MapColor { get; }
		public abstract int LiquidType { get; }
		public abstract int DropperType { get; }
		public virtual string ItemTexture => GetType().GetDefaultTMLName() + "_Item";
		static readonly Dictionary<Type, BaseLiquidSensor> tilesByType = [];
		public override void Load() {
			BaseLiquidSensor tile = new(this);
			Mod.AddContent(tile);
			tilesByType[GetType()] = tile;
		}
		public virtual void OnLoad(ModTile tile) { }

		private static readonly List<Point16> tripPoints = new();

		private static readonly List<int> markedIDsForRemoval = new();

		private static bool inUpdateLoop;

		public bool On = false;

		public int CountedData;

		public override void NetPlaceEntityAttempt(int x, int y) {
			int iD = Place(x, y);
			((BaseLiquidSensorEntity)ByID[iD]).FigureCheckState();
			NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, iD, x, y);
		}

		public override bool IsTileValidForEntity(int x, int y) {
			if (!Main.tile[x, y].HasTile || Main.tile[x, y].TileType != tilesByType[GetType()].Type || Main.tile[x, y].TileFrameY % 18 != 0 || Main.tile[x, y].TileFrameX % 18 != 0) return false;
			return true;
		}

		public override void PreGlobalUpdate() {
			inUpdateLoop = true;
			markedIDsForRemoval.Clear();
		}

		public override void Update() {
			Dust.QuickDust(Position.X, Position.Y, Color.White);
			bool state = GetState(Position.X, Position.Y, this);
			if (On != state) ChangeState(state, TripWire: true);
		}

		public override void PostGlobalUpdate() {
			inUpdateLoop = false;
			foreach (Point16 tripPoint in tripPoints) {
				SoundEngine.PlaySound(SoundID.Mech, tripPoint.ToVector2() * 16f);
				Wiring.TripWire(tripPoint.X, tripPoint.Y, 1, 1);
				if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.HitSwitch, -1, -1, null, tripPoint.X, tripPoint.Y);
			}
			tripPoints.Clear();
			foreach (int item in markedIDsForRemoval) {
				if (ByID.TryGetValue(item, out TileEntity value) && value.type == Type) {
					lock (EntityCreationLock) {
						ByID.Remove(item);
						ByPosition.Remove(value.Position);
					}
				}
			}
			markedIDsForRemoval.Clear();
		}

		public void ChangeState(bool onState, bool TripWire) {
			if (onState == On || SanityCheck(Position.X, Position.Y)) {
				Main.tile[Position.X, Position.Y].TileFrameX = (short)(onState ? 18 : 0);
				if (On.TrySet(onState)) tripPoints.Add(Position);
				if (Main.netMode == NetmodeID.Server) NetMessage.SendTileSquare(-1, Position.X, Position.Y);
			}
		}
		public virtual bool MatchesLiquidType(Tile tile) => tile.LiquidType == LiquidType;

		public bool GetState(int x, int y, BaseLiquidSensorEntity instance = null) {
			if (instance == null) return false;
			Tile tile = Main.tile[x, y];
			bool switched = tile.LiquidAmount != 0 && MatchesLiquidType(tile);
			if (!switched && instance.On) {
				if (instance.CountedData == 0) instance.CountedData = 15;
				else if (instance.CountedData > 0) instance.CountedData--;
				switched = instance.CountedData > 0;
			}
			return switched;
		}

		public void FigureCheckState() {
			GetFrame(Position.X, Position.Y, On);
		}

		public static void GetFrame(int x, int y, bool on) {
			Tile tile = Main.tile[x, y];
			tile.TileFrameX = (short)(on ? 18 : 0);
		}

		public bool SanityCheck(int x, int y) {
			if (!Main.tile[x, y].HasTile || Main.tile[x, y].TileType != tilesByType[GetType()].Type) {
				Kill(x, y);
				return false;
			}
			return true;
		}

		public override int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) {
			GetFrame(x, y, GetState(x, y));
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendTileSquare(Main.myPlayer, x, y);
				NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, x, y, Type);
				return -1;
			}
			int placeID = Place(x, y);
			((BaseLiquidSensorEntity)ByID[placeID]).FigureCheckState();
			return placeID;
		}

		public override void OnKill() {
			int x = Position.X;
			int y = Position.Y;
			if (!ByPosition.TryGetValue(new Point16(x, y), out TileEntity value) || value.type != Type) return;
			if (((BaseLiquidSensorEntity)value).On) {
				Wiring.HitSwitch(value.Position.X, value.Position.Y);
				NetMessage.SendData(MessageID.HitSwitch, -1, -1, null, value.Position.X, value.Position.Y);
			}
			if (inUpdateLoop) {
				markedIDsForRemoval.Add(value.ID);
				return;
			}
		}

		public override void NetSend(BinaryWriter writer) {
			writer.Write(On);
		}

		public override void NetReceive(BinaryReader reader) {
			On = reader.ReadBoolean();
		}

		public override void LoadData(TagCompound tag) {
			On = tag.GetBool(nameof(On));
		}

		public override void SaveData(TagCompound tag) {
			tag[nameof(On)] = On;
		}

		public override string ToString() {
			return Position.X + "x  " + Position.Y + "y";
		}
	}
	#endregion
	public class Oil_Liquid_Sensor : BaseLiquidSensorEntity<Oil, Magic_Dropper_Oil> {
		public override Color MapColor => FromHexRGB(0x212121);
	}
}
