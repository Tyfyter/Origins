using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Tools.Wiring;
using Origins.NPCs.Ashen;
using Origins.UI;
using Origins.World.BiomeData;
using PegasusLib.Networking;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;
using Terraria.UI.Chat;
using static Origins.Items.Tools.Wiring.Logic_Gate_System;

namespace Origins.Tiles.Ashen {
	public class Radio_Component : OriginTile, IComplexMineDamageTile, IAshenWireTile {
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
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;
			AddMapEntry(new Color(255, 81, 0));

			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.CoordinateHeights = [16];

			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<Radio_Component_TE>().Generic_HookPostPlaceMyPlayer;
			TileObjectData.addTile(Type);
			ID = Type;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public void UpdatePowerState(int i, int j, bool powered) {
			if (TileEntity.TryGet(i, j, out Radio_Component_TE te)) te.TriggerAshen(powered);
		}
		public void Poke(Point position, int fromWireType) => UpdatePowerState(position.X, position.Y, AshenWireTile.DefaultIsPowered(position.X, position.Y));
		public override void HitWire(int i, int j) {
			if (!Ashen_Wire_Data.HittingAshenWires && TileEntity.TryGet(i, j, out Radio_Component_TE te)) te.TriggerVanilla();
		}
		public override bool RightClick(int i, int j) {
			SetComponentUI(new UI(i, j));
			return true;
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			tileFrameX = 0;
			tileFrameY = 0;
		}
		public record class UI(Point Position) : AComponentUI(Position) {
			public UI(int i, int j) : this(new Point(i, j)) { }
			public override bool ShouldClose => base.ShouldClose || !GetTE(out _);
			bool GetTE(out Radio_Component_TE tileEntity) => TileEntity.TryGet(new(Position), out tileEntity);
			public override Action Initialize(UIElement root) {
				root.Append(new UIImageFramed(ComponentUI.Textures, new(486, 2, 240, 150)));
				root.Append(new Delay_Element(new(Position)) {
					Left = new(2, 0),
					Top = new(0, 1)
				});
				return () => {};
			}
			public class Delay_Element(Point16 position) : UIImageFramed(ComponentUI.Textures, new(244, 154, 130, 24)) {
				bool GetTE(out Radio_Component_TE tileEntity) => TileEntity.TryGet(position, out tileEntity);
				public override void OnInitialize() {
					if (!GetTE(out Radio_Component_TE tileEntity)) return;
					Append(ComponentUI.ReframingButton.Disableable(
						() => tileEntity.Channel <= 0,
						() => new Radio_Component_TE.Set_Channel_Action(position.X, position.Y, tileEntity.Channel - 1).Perform(),
						new(248, 180, 18, 18),
						new(248, 200, 18, 18)
					).MoveTo(new(4, 2)));
					Append(ComponentUI.ReframingButton.Disableable(
						() => tileEntity.Channel >= Radio_Component_TE.max,
						() => new Radio_Component_TE.Set_Channel_Action(position.X, position.Y, tileEntity.Channel + 1).Perform(),
						new(292, 180, 18, 18),
						new(292, 200, 18, 18)
					).MoveTo(new(48, 2)));

					Vector2 pos = new(76, 2);
					for (int i = 0; i < 3; i++) {
						Mode mode = (Mode)i;
						Append(ComponentUI.ReframingButton.Disableable(
							() => tileEntity.Mode == mode,
							() => new Radio_Component_TE.Set_Mode_Action(position.X, position.Y, mode).Perform(),
							new(374 + i * 18, 180, 18, 18),
							new(318 + i * 18, 180, 18, 18)
						).MoveTo(pos));
						pos.X += 16;
					}
				}
				protected override void DrawSelf(SpriteBatch spriteBatch) {
					if (!GetTE(out Radio_Component_TE tileEntity)) return;
					base.DrawSelf(spriteBatch);
					spriteBatch.Draw(
						ComponentUI.Textures.Value,
						new Rectangle(22, 4, 26, 14).Add(GetDimensions().Position()),
						new Rectangle(266, 180 + 16 * tileEntity.Channel, 26, 14),
						Color.White
					);
				}
			}
		}
		public enum Mode : byte {
			Send,
			Receive,
			Signal
		}
	}
	public class Radio_Component_TE : ModTileEntity {
		public const int count = 8;
		public const int max = count - 1;
		static readonly bool[] powered = new bool[count];
		public int Channel { get; private set; } = 0;
		public Radio_Component.Mode Mode { get; private set; }
		public override void NetSend(BinaryWriter writer) {
			writer.Write(Channel);
			writer.Write(Channel);
		}
		public override void NetReceive(BinaryReader reader) {
			Channel = reader.ReadInt32();
		}
		public override void PreGlobalUpdate() {
			Array.Clear(powered);
			foreach ((Point16 position, TileEntity te) in ByPosition) {
				if (te is not Radio_Component_TE component || component.Mode != Radio_Component.Mode.Send || powered[component.Channel]) continue;
				powered[component.Channel] = AshenWireTile.DefaultIsPowered(position.X, position.Y);
			}
		}
		public override void Update() {
			if (!IsTileValidForEntity(Position.X, Position.Y)) {
				Kill(Position.X, Position.Y);
				return;
			}
			if (Mode == Radio_Component.Mode.Receive) {

			}
		}
		public void TriggerVanilla() {
		}
		public void TriggerAshen(bool powered) {
			if (powered == Radio_Component_TE.powered[Channel]) return;
		}
		public override bool IsTileValidForEntity(int x, int y) {
			if (x < 0 || y < 0) return false;
			return Main.tile[x, y].TileIsType(ModContent.TileType<Radio_Component>());
		}
		public record class Set_Channel_Action(int I, int J, int Channel) : AutoSyncedAction {
			protected override bool ShouldPerform => TryGet<Radio_Component_TE>(I, J, out _);
			protected override void Perform() {
				if (!TryGet(I, J, out Radio_Component_TE te)) return;
				te.Channel = Channel;
			}
		}
		public record class Set_Mode_Action(int I, int J, Radio_Component.Mode Mode) : AutoSyncedAction {
			protected override bool ShouldPerform => TryGet<Radio_Component_TE>(I, J, out _);
			protected override void Perform() {
				if (!TryGet(I, J, out Radio_Component_TE te)) return;
				te.Mode = Mode;
			}
			[AutoSyncSend<Radio_Component.Mode>]
			static void WriteMode(BinaryWriter writer, Radio_Component.Mode mode) => writer.Write((byte)mode);
			[AutoSyncReceive<Radio_Component.Mode>]
			static Radio_Component.Mode ReadMode(BinaryReader reader) => (Radio_Component.Mode)reader.ReadByte();
		}
	}
}
