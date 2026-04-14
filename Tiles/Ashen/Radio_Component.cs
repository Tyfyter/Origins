using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Tools.Wiring;
using Origins.World.BiomeData;
using PegasusLib.Networking;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.UI;
using static Origins.Items.Tools.Wiring.Logic_Gate_System;

namespace Origins.Tiles.Ashen {
	public class Radio_Component : OriginTile, IComplexMineDamageTile, IAshenWireTile, IAshenPowerConduitTile {
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
		public void UpdatePowerState(int i, int j, bool powered) { }
		public override void HitWire(int i, int j) {
			if (!Ashen_Wire_Data.HittingAshenWires && TileEntity.TryGet(i, j, out Radio_Component_TE te)) te.TriggerVanilla();
		}
		public void Poke(Point position, int fromWireType) {
			if (!TileEntity.TryGet(position.X, position.Y, out Radio_Component_TE te) || te.Mode != Mode.Receive) return;
			te.GetReceiveWires(out int input, out _);
			if (fromWireType == input) Ashen_Wire_Data.SetTilePowered(position.X, position.Y, false);// && !IsPowered(position.X, position.Y)
		}
		public bool IsPowered(int i, int j) {
			if (!TileEntity.TryGet(i, j, out Radio_Component_TE te) || te.Mode != Mode.Receive) return AshenWireTile.DefaultIsPowered(i, j);
			te.GetReceiveWires(out int input, out _);
			using IAshenPowerConduitTile.WalkedConduitOutput __ = new(new(i, j));
			return Ashen_Wire_Data.GetPower(i, j, input) && IAshenPowerConduitTile.FindValidPowerSource(new(i, j), input);
		}
		public bool ShouldCountAsPowerSource(Point position, int forWireType) {
			if (!TileEntity.TryGet(position.X, position.Y, out Radio_Component_TE te) || te.Mode != Mode.Receive) return false;
			using IAshenPowerConduitTile.WalkedConduitOutput _ = new(position);
			te.GetReceiveWires(out int input, out int output);
			return forWireType == output && IAshenPowerConduitTile.FindValidPowerSource(position, input);
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
			UIElement wireRoot;
			Action deactivate;
			public override Action Initialize(UIElement root) {
				root.Append(new UIImageFramed(ComponentUI.Textures, new(486, 2, 240, 150)));
				root.Append(new Radio_Element(new(Position), this) {
					Left = new(2, 0),
					Top = new(0, 1)
				});
				wireRoot = new() {
					Width = new(root.Width.Pixels, 0),
					Height = new(root.Height.Pixels, 0)
				};
				InitializeHooks();
				root.Append(wireRoot);
				return () => deactivate?.Invoke();
			}
			void InitializeHooks() {
				deactivate = null;
				wireRoot?.RemoveAllChildren();
				if (!GetTE(out Radio_Component_TE tileEntity)) return;
				switch (tileEntity.Mode) {
					case Mode.Send:
					deactivate = SetupWires(new(wireRoot,
						wires => {
							for (int i = 0; i < 3; i++) {
								if ((tileEntity.Wires & (1 << i)) != 0) wires[i] = i;
							}
						},
						wires => {
							byte attached = 0;
							for (int i = 0; i < 3; i++) attached |= (byte)(1 << wires[i]);
							new Radio_Component_TE.Set_Wires_Action(Position.X, Position.Y, attached).Perform();
							return true;
						},
						new(32, 18),
						new(32, 58),
						new(32, 98)
					) {
						CanUpdate = _ => true,
						ModifyWires = wire => {
							if (wire.wireType == 3) {
								wire.connectedTo = -2;
							} else {
								wire.disconnectedPosition = new(8, wire.Parent.Height.Pixels * 0.25f * (wire.wireType + 1) - 2);
							}
						}
					});
					break;

					case Mode.Receive:
					const int a_mask = 0b0011;
					const int b_mask = 0b0100;
					deactivate = SetupWires(new(wireRoot,
						wires => tileEntity.GetReceiveWires(out wires[0], out wires[1]),
						wires => {
							if (wires.Contains(-1)) return false;
							int result = wires[0];
							if ((wires[1] - wires[0] + 3) % 3 != 1) result |= b_mask;
							return true;
						},
						new(24, 62),
						new(176, 62, true)
					) {
						ModifyWires = wire => {
							if (wire.wireType == 3) wire.connectedTo = -2;
						}
					});
					break;

					case Mode.Signal:
					static void HideNonVanilla(ComponentUI.DraggableWire wire) {
						if (wire.wireType != 3) wire.connectedTo = -2;
					}
					SetupWires(new(wireRoot,
						wires => wires[0] = 3,
						_ => true,
						new ComponentUI.Socket(24, 62, locked: true)
					) {
						ModifyWires = HideNonVanilla
					});
					SetupWires(new(wireRoot,
						wires => wires[0] = 3,
						_ => true,
						new ComponentUI.Socket(176, 62, true, true)
					) {
						ModifyWires = HideNonVanilla
					});
					break;
				}
				wireRoot.Activate();
			}
			public class Radio_Element(Point16 position, UI parentUI) : UIImageFramed(ComponentUI.Textures, new(244, 154, 130, 24)) {
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
							() => {
								new Radio_Component_TE.Set_Mode_Action(position.X, position.Y, mode).Perform();
								parentUI.InitializeHooks();
							},
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
			public override int GetHashCode() => Position.GetHashCode();
			public virtual bool Equals(UI other) => base.Equals(other);
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
		static readonly bool[] triggered = new bool[count];
		static readonly bool[] nextTriggered = new bool[count];
		public int Channel { get; private set; } = 0;
		public byte Wires { get; private set; } = 0;
		public Radio_Component.Mode Mode { get; private set; }
		bool sentThisTick;
		public void GetReceiveWires(out int input, out int output) {
			const int a_mask = 0b0011;
			const int b_mask = 0b0100;
			input = (Wires & a_mask) % a_mask;
			output = (input + 1 + ((Wires & b_mask) >> 2)) % a_mask;
		}
		public override void NetSend(BinaryWriter writer) {
			writer.Write(Channel);
			writer.Write(Wires);
			writer.Write((byte)Mode);
		}
		public override void NetReceive(BinaryReader reader) {
			Channel = reader.ReadInt32();
			Wires = reader.ReadByte();
			Mode = (Radio_Component.Mode)reader.ReadByte();
		}
		public override void PreGlobalUpdate() {
			Array.Clear(powered);
			foreach ((Point16 position, TileEntity te) in ByPosition) {
				if (te is not Radio_Component_TE component || component.Mode != Radio_Component.Mode.Send || powered[component.Channel]) continue;
				Ashen_Wire_Data data = Main.tile[position].Get<Ashen_Wire_Data>();
				if ((component.Wires & (1 << 0)) != 0) powered[component.Channel] = data.BrownWirePowered;
				if ((component.Wires & (1 << 1)) != 0) powered[component.Channel] |= data.BlackWirePowered;
				if ((component.Wires & (1 << 2)) != 0) powered[component.Channel] |= data.WhiteWirePowered;
			}
			nextTriggered.CopyTo(triggered.AsSpan());
			Array.Clear(nextTriggered);
		}
		public override void Update() {
			if (!IsTileValidForEntity(Position.X, Position.Y)) {
				Kill(Position.X, Position.Y);
				return;
			}
			switch (Mode) {
				case Radio_Component.Mode.Receive:
				Ashen_Wire_Data.SetTilePowered(Position.X, Position.Y, powered[Channel]);
				break;
				case Radio_Component.Mode.Signal:
				if (sentThisTick.TrySet(false)) break;
				if (triggered[Channel]) Wiring.TripWire(Position.X, Position.Y, 1, 1);
				break;
			}
		}
		public void TriggerVanilla() {
			if (Mode == Radio_Component.Mode.Signal) {
				nextTriggered[Channel] = true;
				sentThisTick = true;
			}
		}
		public override bool IsTileValidForEntity(int x, int y) {
			if (x < 0 || y < 0) return false;
			return Main.tile[x, y].TileIsType(ModContent.TileType<Radio_Component>());
		}
		public override void SaveData(TagCompound tag) {
			tag[nameof(Channel)] = Channel;
			tag[nameof(Wires)] = Wires;
			tag[nameof(Mode)] = Mode.ToString();
		}
		public override void LoadData(TagCompound tag) {
			Channel = tag.SafeGet(nameof(Channel), Channel);
			Wires = tag.SafeGet(nameof(Wires), Wires);
			Mode = Enum.TryParse(tag.SafeGet(nameof(Mode), Mode.ToString()), out Radio_Component.Mode mode) ? mode : Mode;
		}
		public record class Set_Channel_Action(int I, int J, int Channel) : AutoSyncedAction {
			protected override bool ShouldPerform => TryGet<Radio_Component_TE>(I, J, out _);
			protected override void Perform() {
				if (!TryGet(I, J, out Radio_Component_TE te)) return;
				te.Channel = Channel;
			}
		}
		public record class Set_Mode_Action(int I, int J, Radio_Component.Mode Mode) : AutoSyncedAction, IBroken {
			static string IBroken.BrokenReason => "Enum support added in incoming PegasusLib update";
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
		public record class Set_Wires_Action(int I, int J, byte Wires) : AutoSyncedAction {
			protected override bool ShouldPerform => TryGet<Radio_Component_TE>(I, J, out _);
			protected override void Perform() {
				if (!TryGet(I, J, out Radio_Component_TE te)) return;
				te.Wires = Wires;
			}
		}
	}
}
