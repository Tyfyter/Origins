using Origins.Items.Other.Consumables;
using Origins.Items.Tools.Wiring;
using PegasusLib.Networking;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public abstract class Mechanical_Switch : Transistor {
		public abstract int ItemType { get; }
		public abstract int KeyType { get; }
		public override string HighlightTexture => typeof(Mechanical_Switch).GetDefaultTMLName("_Highlight");
		static bool loadedHook = false;
		delegate void orig_ModifySmartInteractCoords(int type, ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY);
		static void Hook_ModifySmartInteractCoords(orig_ModifySmartInteractCoords orig, int type, ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY) {
			orig(type, ref width, ref height, ref frameWidth, ref frameHeight, ref extraY);
			if (TileLoader.GetTile(type) is Mechanical_Switch) {
				width = 1;
				height = 1;
				extraY = 0;
			}
		}
		public override void Load() {
			if (loadedHook.TrySet(true)) MonoModHooks.Add(((Delegate)TileLoader.ModifySmartInteractCoords).Method, Hook_ModifySmartInteractCoords);
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			TileID.Sets.HasOutlines[Type] = true;
			RegisterItemDrop(ItemType);
		}
		public override void HitWire(int i, int j) {
			Tile tile = Main.tile[i, j];
			if ((tile.TileFrameY / 18) % 3 == 1) return;
			base.HitWire(i, j);
		}
		public bool HasKey(Player player) => player.HasItemInInventoryOrOpenVoidBag(KeyType);
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => (Main.tile[i, j].TileFrameY / 18) % 3 == 1 && HasKey(settings.player);
		public override bool RightClick(int i, int j) {
			if (!HasKey(Main.LocalPlayer)) return false;
			Point pos = new(i, j);
			Tile tile = Main.tile[pos];
			Point dir = GetDirection(tile);
			switch ((tile.TileFrameY / 18) % 3) {
				case 0:
				pos += dir;
				break;

				case 2:
				pos -= dir;
				break;
			}
			new Mechanical_Switch_Action(new Point16(pos)).Perform();
			return true;
		}
		void BaseHitWire(int i, int j) => base.HitWire(i, j);
		public override void UpdateTransistor(Point pos) {
			Point originalPos = pos;
			Point dir = GetDirection(Main.tile[pos]);
			pos += dir + dir;
			ref Ashen_Wire_Data output = ref Main.tile[pos].Get<Ashen_Wire_Data>();
			bool wasPowered = output.IsTilePowered;
			base.UpdateTransistor(originalPos);
			if (output.IsTilePowered != wasPowered) {
				Wiring.TripWire(pos.X, pos.Y, 1, 1);
			}
		}
		public record class Mechanical_Switch_Action(Point16 Pos) : SyncedAction {
			public override bool ServerOnly => true;
			public Mechanical_Switch_Action() : this(Point16.Zero) { }
			public override SyncedAction NetReceive(BinaryReader reader) => this with {
				Pos = new(reader.ReadInt16(), reader.ReadInt16())
			};
			public override void NetSend(BinaryWriter writer) {
				writer.Write((short)Pos.X);
				writer.Write((short)Pos.Y);
			}
			protected override void Perform() {
				if (TileLoader.GetTile(Main.tile[Pos].TileType) is Mechanical_Switch @switch) @switch.BaseHitWire(Pos.X, Pos.Y);
			}
		}
	}
	public class Purple_Mechanical_Switch : Mechanical_Switch {
		public override Color MapColor => FromHexRGB(0x6d0a91);
		public override int ItemType => ItemType<Purple_Mechanical_Switch_Item>();
		public override int KeyType => ItemType<Mechanical_Key_Purple>();
	}
	public class Purple_Mechanical_Switch_Item : Transistor_Item {
		public override string Texture => typeof(Transistor_Item).GetDefaultTMLName();
		public override void SetDefaults() {
			base.SetDefaults();
			Item.createTile = TileType<Purple_Mechanical_Switch>();
		}
	}
}
