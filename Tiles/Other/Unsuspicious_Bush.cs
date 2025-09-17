using PegasusLib.Networking;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Other {
	public class Unsuspicious_Bush : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.HasOutlines[Type] = false;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 3, 0);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.CoordinateHeights = [16, 18];
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(100, 157, 6));
			DustType = DustID.GrassBlades;
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => false;
		public override bool RightClick(int i, int j) {
			new Unsuspicious_Bush_Action(!OriginSystem.Instance.ForceAF).Perform();
			return true;
		}
	}
	public class Unsuspicious_Bush_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Unsuspicious_Bush>());
			Item.value = Item.sellPrice(gold: 0);
			Item.rare = ItemRarityID.Green;
			Item.placeStyle = 0;
		}
	}
	public record class Unsuspicious_Bush_Action(bool On) : SyncedAction {
		public Unsuspicious_Bush_Action() : this(false) { }
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			On = reader.ReadBoolean()
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write(On);
		}
		protected override void Perform() {
			OriginSystem.Instance.ForceAF = On;
		}
	}
}
