using AltLibrary.Common.AltBiomes;
using Origins.Tiles.Defiled;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Tiles {
	public abstract class OriginTile : ModTile {
		public static List<OriginTile> IDs { get; internal set; }
		public static List<int> DefiledTiles { get; internal set; }
		public ushort mergeID;
		public override void Load() {
			if (IDs != null) {
				IDs.Add(this);
			} else {
				IDs = new List<OriginTile>() { this };
			}
			mergeID = Type;
			if (this is IDefiledTile) {
				if (DefiledTiles != null) {
					DefiledTiles.Add(Type);
				} else {
					DefiledTiles = [Type];
				}
			}
		}
		protected void AddDefiledTile() {
		}
	}
	//temp solution
	public interface IDefiledTile { }
	public interface IRivenTile { }
	public interface IAshenTile { }
}
