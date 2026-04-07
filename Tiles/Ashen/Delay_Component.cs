using Origins.Items.Tools.Wiring;
using Origins.NPCs.Ashen;
using Origins.World.BiomeData;
using PegasusLib.Networking;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Delay_Component : OriginTile, IComplexMineDamageTile, IAshenWireTile, IAshenPowerConduitTile {
		public static int ID { get; private set; }
		public override void Load() {
			new TileItem(this, textureOverride: Texture)
			.WithExtraStaticDefaults(this.DropTileItem)
			.RegisterItem();
		}
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = false;
			Main.tileNoFail[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = false;
			TileID.Sets.FramesOnKillWall[Type] = true;
			AddMapEntry(new Color(255, 81, 0));

			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.CoordinateHeights = [16];

			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<Delay_Component_TE>().Generic_HookPostPlaceMyPlayer;
			TileObjectData.addTile(Type);
			ID = Type;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public void UpdatePowerState(int i, int j, bool powered) {
			if (TileEntity.TryGet(i, j, out Delay_Component_TE te)) te.TriggerAshen(powered);
		}
		bool isProcessingDelayComponent = false;
		public bool IsPowered(int i, int j) {
			Tile tile = Main.tile[i, j];
			Point pos = new(i, j);
			bool inputPower = false;
			if (tile.Get<Ashen_Wire_Data>().AnyPower) {
				using ScopedOverride<bool> process = isProcessingDelayComponent.ScopedOverride(true);
				using IAshenPowerConduitTile.WalkedConduitOutput _ = new(pos);
				inputPower = IAshenPowerConduitTile.FindValidPowerSource(pos, 0)
						|| IAshenPowerConduitTile.FindValidPowerSource(pos, 1)
						|| IAshenPowerConduitTile.FindValidPowerSource(pos, 2);
			}
			return inputPower;
		}
		public bool ShouldCountAsPowerSource(Point position, int forWireType) => !isProcessingDelayComponent;
		public void Poke(Point position, int fromWireType) => UpdatePowerState(position.X, position.Y, IsPowered(position.X, position.Y));
		public override void HitWire(int i, int j) {
			if (!Ashen_Wire_Data.HittingAshenWires && TileEntity.TryGet(i, j, out Delay_Component_TE te)) te.TriggerVanilla();
		}
	}
	public class Delay_Component_TE : ModTileEntity {
		private readonly int[] timer = new int[2];
		private bool targetPowered;
		private int delay = 60;
		public override void NetSend(BinaryWriter writer) {
			for (int i = 0; i < timer.Length; i++) writer.Write(timer[i]);
			writer.Write(delay);
			writer.Write(targetPowered);
		}
		public override void NetReceive(BinaryReader reader) {
			for (int i = 0; i < timer.Length; i++) timer[i] = reader.ReadInt32();
			delay = reader.ReadInt32();
			targetPowered = reader.ReadBoolean();
		}
		public override void Update() {
			if (!IsTileValidForEntity(Position.X, Position.Y)) {
				Kill(Position.X, Position.Y);
				return;
			}
			if (timer[0] > 0 && ++timer[0] > delay) {
				timer[0] = 0;
				Ashen_Wire_Data.SetTilePowered(Position.X, Position.Y, targetPowered);
			}
			if (timer[1] > 0 && ++timer[1] > delay) {
				timer[1] = 0;
				Wiring.TripWire(Position.X, Position.Y, 1, 1);
			}
		}
		public void TriggerVanilla() {
			timer[1] = (timer[1] == 0).ToInt();
		}
		public void TriggerAshen(bool powered) {
			if (powered == targetPowered) return;
			timer[0] = (Main.tile[Position].Get<Ashen_Wire_Data>().IsTilePowered != powered).ToInt();
			targetPowered = powered;
		}
		public override bool IsTileValidForEntity(int x, int y) {
			if (x < 0 || y < 0) return false;
			return Main.tile[x, y].TileIsType(ModContent.TileType<Delay_Component>());
		}
		public record class Set_Delay_Action(int I, int J, int Delay) : AutoSyncedAction {
			protected override bool ShouldPerform => TryGet<Delay_Component_TE>(I, J, out _);
			protected override void Perform() {
				if (!TryGet(I, J, out Delay_Component_TE te)) return;
				te.delay = Delay;
			}
		}
	}
}
