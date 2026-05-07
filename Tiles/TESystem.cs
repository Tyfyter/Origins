using Origins.Items.Tools.Wiring;
using Origins.Tiles.Defiled;
using PegasusLib.Networking;
using Stubble.Core.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Origins.Tiles {
	public abstract class TESystem : ModSystem {
		public int TESystemType { get; private set; }
		static List<TESystem> TESystems;
		public static int TESystemCount => TESystems.Count;
		public List<Point16> tileEntityLocations = [];
		public static ReadOnlyCollection<Point16> GetLocations<TSystem>() where TSystem : TESystem => ModContent.GetInstance<TSystem>().tileEntityLocations.AsReadOnly();
		public override void Load() {
			TESystems ??= [];
			TESystemType = TESystems.Count;
			TESystems.Add(this);
		}
		public override void OnModUnload() {
			TESystems = null;
		}
		public static TESystem Get(int type) => TESystems[type];
		public void AddTileEntity(Point16 pos) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				ModPacket packet = Mod.GetPacket();
				packet.Write(Origins.NetMessageType.place_tile_entity);
				packet.Write((ushort)TESystemType);
				packet.Write((short)pos.X);
				packet.Write((short)pos.Y);
				packet.Send(-1, Main.myPlayer);
			} else {
				tileEntityLocations.Add(pos);
			}
		}
		public override void SaveWorldData(TagCompound tag) {
			tag[nameof(tileEntityLocations)] = tileEntityLocations;
		}
		public override void LoadWorldData(TagCompound tag) {
			tileEntityLocations = tag.Get<List<Point16>>(nameof(tileEntityLocations));
		}
		public override void ClearWorld() => tileEntityLocations = [];
		public static void SyncAllToPlayer(int player) {
			foreach (TESystem system in TESystems) system.SyncToPlayer(player);
		}
		public void SyncToPlayer(int player) {
			for (int i = 0; i < tileEntityLocations.Count; i++) {
				ModPacket packet = Mod.GetPacket();
				packet.Write(Origins.NetMessageType.place_tile_entity);
				packet.Write((ushort)TESystemType);
				packet.Write((short)tileEntityLocations[i].X);
				packet.Write((short)tileEntityLocations[i].Y);
				packet.Send(player);
			}
		}
	}
	public abstract class TESystem<T> : ModSystem, IBroken where T : TESystem<T>.ITileEntityData {
		static string IBroken.BrokenReason => "Temp code to work with two very different PegasusLib versions";
		public static int TESystemCount => ComplexTESystems.Count;
		private static List<TESystem<T>> ComplexTESystems = [];
		public ushort CTESystemType { get; private set; } = ushort.MaxValue;
		public Dictionary<Point16, T> tileEntities = [];
		public override void Load() {
			CTESystemType = (ushort)ComplexTESystems.Count;
			ComplexTESystems.Add(this);
			OriginPlayer.SyncToNewPlayer += DoFullSync;
			Type isyncedAction = typeof(SyncedAction).GetInterface("ISyncedAction");
			if (isyncedAction is null) {
				RegisterAction_Old();
			} else {
				RegisterAction_Future(isyncedAction);
			}
		}
		[NoJIT]
		void RegisterAction_Old() {
			if (ModContent.GetInstance<PlaceComplexTEAction>() is null) Mod.AddContent(new PlaceComplexTEAction());
		}
		void RegisterAction_Future(Type isyncedAction) {
			isyncedAction.GetNestedType("Loading").GetMethod("EnsureLoaded", [typeof(Mod), typeof(Type)]).Invoke(null, [Mod, typeof(PlaceComplexTEAction)]);
		}
		public void AddTileEntity(Point16 pos, T value) => new PlaceComplexTEAction(CTESystemType, pos, value).Perform();
		public sealed override void SaveWorldData(TagCompound tag) {
			TagCompound locations = new();
			foreach ((Point16 pos, T value) in tileEntities) {
				TagCompound tileEntity = new();
				value.SaveTE(tileEntity);
				locations[$"{pos.X},{pos.Y}"] = tileEntity;
			}
			tag[nameof(tileEntities)] = locations;
		}
		protected virtual void PreLoad() { }
		public sealed override void LoadWorldData(TagCompound tag) {
			tileEntities.Clear();
			PreLoad();
			foreach ((string position, object value) in tag.Get<TagCompound>(nameof(tileEntities))) {
				string[] pos = position.Split(',');
				tileEntities[new(int.Parse(pos[0]), int.Parse(pos[1]))] = T.LoadTE((TagCompound)value);
			}
			PostLoad();
		}
		protected virtual void PostLoad() { }
		public override void ClearWorld() => tileEntities = [];
		protected abstract bool IsValidTile(Tile tile);
		static readonly Stack<Point16> toRemove = [];
		public sealed override void PreUpdateEntities() {
			toRemove.Clear();
			foreach ((Point16 position, T data) in tileEntities) {
				if (!IsValidTile(Main.tile[position])) toRemove.Push(position);
				else {
					data.Update(position);
					data.TryClean(CTESystemType, position);
				}
			}
			while (toRemove.TryPop(out Point16 position)) tileEntities.Remove(position);
		}
		void DoFullSync(int toPlayer) {
			foreach ((Point16 pos, T value) in tileEntities) new PlaceComplexTEAction(CTESystemType, pos, value).Send(toPlayer);
		}
		public interface ITileEntityData {
			public bool IsDirty { get; set; }
			public void Update(Point16 position);
			public sealed void TryClean(ushort cteSystemType, Point16 position) {
				if (IsDirty) {
					IsDirty = false;
					new PlaceComplexTEAction(cteSystemType, position, (T)this).Send();
				}
			}
			void NetSend(BinaryWriter writer);
			static abstract T NetReceive(BinaryReader reader);
			void SaveTE(TagCompound tag);
			static abstract T LoadTE(TagCompound tag);
		}
		record PlaceComplexTEAction(ushort CTESystemType, Point16 Position, T Value) : SyncedAction() {
			public PlaceComplexTEAction() : this(default, default, default) { }
			public override SyncedAction NetReceive(BinaryReader reader) => this with {
				CTESystemType = reader.ReadUInt16(),
				Position = ReadPoint16(reader),
				Value = T.NetReceive(reader)
			};
			public override void NetSend(BinaryWriter writer) {
				writer.Write((ushort)CTESystemType);
				WritePoint16(writer, Position);
				Value.NetSend(writer);
			}
			protected override void Perform() {
				ComplexTESystems[CTESystemType].tileEntities[Position] = Value;
			}
		}
	}
}
