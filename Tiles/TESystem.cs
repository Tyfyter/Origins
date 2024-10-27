using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Tiles {
	public abstract class TESystem : ModSystem {
		public int TESystemType { get; private set; }
		static List<TESystem> TESystems;
		public List<Point16> tileEntityLocations = new();
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
}
