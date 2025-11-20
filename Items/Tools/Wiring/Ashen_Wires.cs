using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Items.Other.Consumables;
using Origins.Reflection;
using Origins.Tiles.Ashen;
using Origins.World;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Origins.Items.Tools.Wiring {
	public class Brown_Wire_Mode : WireMode {
		AutoLoadingAsset<Texture2D> back = "Origins/Items/Tools/Wiring/Ashen_Wires_BG";
		public override Color? WireKiteColor => Color.Chocolate;
		public override void SetupSets() {
			Sets.AshenWires[Type] = true;
		}
		public override bool GetWire(int x, int y) => Main.tile[x, y].Get<Ashen_Wire_Data>().HasBrownWire;
		public override bool SetWire(int x, int y, bool value) {
			if (Main.tile[x, y].Get<Ashen_Wire_Data>().HasBrownWire != value) {
				Ashen_Wire_Data.SetWire(x, y, 0, value);
				return true;
			}
			return false;
		}
		public override void Draw(Vector2 position, bool hovered, WirePetalData data) {
			GetTints(hovered, data.HasFlag(WirePetalData.Enabled), out Color backTint, out Color iconTint);
			DrawIcon(TextureAssets.WireUi[hovered.ToInt() + data.HasFlag(WirePetalData.Cutter).ToInt() * 8].Value, position, backTint);
			if (!data.HasFlag(WirePetalData.Cutter)) DrawIcon(back, position, backTint);
			DrawIcon(Texture2D.Value, position, iconTint);
		}
	}
	public class Black_Wire_Mode : WireMode {
		AutoLoadingAsset<Texture2D> back = "Origins/Items/Tools/Wiring/Ashen_Wires_BG";
		public override Color? WireKiteColor => Color.Black;
		public override void SetupSets() {
			Sets.AshenWires[Type] = true;
		}
		public override bool GetWire(int x, int y) => Main.tile[x, y].Get<Ashen_Wire_Data>().HasBlackWire;
		public override bool SetWire(int x, int y, bool value) {
			if (Main.tile[x, y].Get<Ashen_Wire_Data>().HasBlackWire != value) {
				Ashen_Wire_Data.SetWire(x, y, 1, value);
				return true;
			}
			return false;
		}
		public override void Draw(Vector2 position, bool hovered, WirePetalData data) {
			GetTints(hovered, data.HasFlag(WirePetalData.Enabled), out Color backTint, out Color iconTint);
			DrawIcon(TextureAssets.WireUi[hovered.ToInt() + data.HasFlag(WirePetalData.Cutter).ToInt() * 8].Value, position, backTint);
			if (!data.HasFlag(WirePetalData.Cutter)) DrawIcon(back, position, backTint);
			DrawIcon(Texture2D.Value, position, iconTint);
		}
		public override IEnumerable<WireMode> SortAfter() => [ModContent.GetInstance<Brown_Wire_Mode>()];
	}
	public class White_Wire_Mode : WireMode {
		AutoLoadingAsset<Texture2D> back = "Origins/Items/Tools/Wiring/Ashen_Wires_BG";
		public override Color? WireKiteColor => Color.White;
		public override void SetupSets() {
			Sets.AshenWires[Type] = true;
		}
		public override bool GetWire(int x, int y) => Main.tile[x, y].Get<Ashen_Wire_Data>().HasWhiteWire;
		public override bool SetWire(int x, int y, bool value) {
			if (Main.tile[x, y].Get<Ashen_Wire_Data>().HasWhiteWire != value) {
				Ashen_Wire_Data.SetWire(x, y, 2, value);
				return true;
			}
			return false;
		}
		public override void Draw(Vector2 position, bool hovered, WirePetalData data) {
			GetTints(hovered, data.HasFlag(WirePetalData.Enabled), out Color backTint, out Color iconTint);
			DrawIcon(TextureAssets.WireUi[hovered.ToInt() + data.HasFlag(WirePetalData.Cutter).ToInt() * 8].Value, position, backTint);
			if (!data.HasFlag(WirePetalData.Cutter)) DrawIcon(back, position, backTint);
			DrawIcon(Texture2D.Value, position, iconTint);
		}
		public override IEnumerable<WireMode> SortAfter() => [ModContent.GetInstance<Black_Wire_Mode>()];
	}
	public class Brown_Wire_Toggle : WireBuilderToggle {
		public override string Texture => $"{GetType().Namespace.Replace('.','/')}/Ashen_Wire_Builder_Icons";
		public override Position OrderPosition => new After(ModContent.GetInstance<YellowWireVisibilityBuilderToggle>());
		public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams) {
			drawParams.Frame.X = 0;
			drawParams.Frame.Width = 14;
			return base.Draw(spriteBatch, ref drawParams);
		}
	}
	public class Black_Wire_Toggle : WireBuilderToggle {
		public override string Texture => $"{GetType().Namespace.Replace('.','/')}/Ashen_Wire_Builder_Icons";
		public override Position OrderPosition => new After(ModContent.GetInstance<Brown_Wire_Toggle>());
		public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams) {
			drawParams.Frame.X = 16;
			drawParams.Frame.Width = 14;
			return base.Draw(spriteBatch, ref drawParams);
		}
	}
	public class White_Wire_Toggle : WireBuilderToggle {
		public override string Texture => $"{GetType().Namespace.Replace('.','/')}/Ashen_Wire_Builder_Icons";
		public override Position OrderPosition => new After(ModContent.GetInstance<Black_Wire_Toggle>());
		public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams) {
			drawParams.Frame.X = 32;
			drawParams.Frame.Width = 14;
			return base.Draw(spriteBatch, ref drawParams);
		}
	}
	public abstract class WireBuilderToggle : BuilderToggle {
		public override int NumberOfStates => 3;
		public override bool Active() => Main.LocalPlayer.OriginPlayer().InfoAccMechShowAshenWires;
		LocalizedText name;
		public override void SetStaticDefaults() {
			name = Language.GetOrRegister($"Mods.{Mod.Name}.BuilderToggle.{Name}");
		}
		public override string DisplayValue() {
			string modeText = "";
			switch (CurrentState) {
				case 0:
				modeText = Language.GetTextValue("GameUI.Bright");
				break;
				case 1:
				modeText = Language.GetTextValue("GameUI.Normal");
				break;
				case 2:
				modeText = Language.GetTextValue("GameUI.Faded");
				break;
				case 3: //Should never reach here but vanilla has it
				Language.GetTextValue("GameUI.Hidden");
				break;
			}

			return $"{name.Value}: {modeText}";
		}

		public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams) {
			base.Draw(spriteBatch, ref drawParams);
			drawParams.Color = default;
			switch (CurrentState) {
				case 0:
				drawParams.Color = Color.White;
				break;
				case 1:
				drawParams.Color = new Color(127, 127, 127);
				break;
				case 2:
				drawParams.Color = new Color(127, 127, 127).MultiplyRGBA(new Color(0.66f, 0.66f, 0.66f, 0.66f));
				break;
				case 3: //Should never reach here but vanilla has it
				drawParams.Color = new Color(127, 127, 127).MultiplyRGBA(new Color(0.33f, 0.33f, 0.33f, 0.33f));
				break;
			}
			return true;
		}
	}
	public struct Ashen_Wire_Data : ITileData {
		public static bool HittingAshenWires { get; private set; }
		public static readonly FrameCachedValue<float> pulse = new(() => MathF.Sin((float)Main.timeForVisualEffects / 20) * 0.5f + 0.5f);
		internal byte data;
		public bool HasBrownWire {
			readonly get => GetBit(data, 0);
			private set => SetBit(value, ref data, 0);
		}
		public bool BrownWirePowered {
			readonly get => GetBit(data, 1);
			private set => SetBit(value, ref data, 1);
		}
		public bool HasBlackWire {
			readonly get => GetBit(data, 2);
			private set => SetBit(value, ref data, 2);
		}
		public bool BlackWirePowered {
			readonly get => GetBit(data, 3);
			private set => SetBit(value, ref data, 3);
		}
		public bool HasWhiteWire {
			readonly get => GetBit(data, 4);
			private set => SetBit(value, ref data, 4);
		}
		public bool WhiteWirePowered {
			readonly get => GetBit(data, 5);
			private set => SetBit(value, ref data, 5);
		}
		public readonly bool AnyWire => HasBrownWire || HasBlackWire || HasWhiteWire;
		public readonly bool AnyPower => BrownWirePowered || BlackWirePowered || WhiteWirePowered;
		public bool IsTilePowered {
			readonly get => GetBit(data, 7);
			private set => SetBit(value, ref data, 7);
		}
		public readonly bool GetWire(int wireType) => GetBit(data, wireType << 1);
		public readonly bool GetPower(int wireType) => GetBit(data, (wireType << 1) + 1);
		public static bool GetPower(int i, int j, int wireType) => Main.tile[i, j].Get<Ashen_Wire_Data>().GetPower(wireType);
		static bool GetBit(int bits, int offset)
			=> (bits & 1 << offset) != 0;
		static void SetBit(bool value, ref byte bits, int offset) {
			byte didDone = (byte)(1 << offset);
			if (value) bits |= didDone;
			else bits &= (byte)~didDone;
		}
		public static void SetWire(int i, int j, int wireType, bool value) {
			ref Ashen_Wire_Data data = ref Main.tile[i, j].Get<Ashen_Wire_Data>();
			if (value != data.GetWire(wireType)) {
				SetBit(value, ref data.data, wireType << 1);
				if (value) {
					if (GetPower(i + 1, j, wireType) || GetPower(i - 1, j, wireType) || GetPower(i, j + 1, wireType) || GetPower(i, j - 1, wireType)) {
						SetPowered(i, j, wireType, true);
						if (!GetPower(i + 1, j, wireType) || !GetPower(i - 1, j, wireType) || !GetPower(i, j + 1, wireType) || !GetPower(i, j - 1, wireType)) PropegatePowerState(i, j, wireType, true);
					} else if (data.IsTilePowered) PropegatePowerState(i, j, wireType, true);
				} else {
					SetPowered(i, j, wireType, false);
					TryPropegateDepowered(i + 1, j, wireType);
					TryPropegateDepowered(i - 1, j, wireType);
					TryPropegateDepowered(i, j + 1, wireType);
					TryPropegateDepowered(i, j - 1, wireType);
				}
				Ashen_Wire_System.SendWireData(i, j, Main.myPlayer);
			}
		}
		static void SetPowered(int i, int j, int wireType, bool value) {
			ref Ashen_Wire_Data data = ref Main.tile[i, j].Get<Ashen_Wire_Data>();
			bool wasPowered = data.AnyPower;
			if (value != data.GetPower(wireType)) SetBit(value, ref data.data, (wireType << 1) + 1);
			if (data.AnyPower != wasPowered && !WiringMethods._wireSkip.Value.ContainsKey(new(i, j))) {
				bool powerMultiTile = true;
				if (wasPowered && TileObjectData.GetTileData(Main.tile[i, j]) is TileObjectData tileData) {
					TileUtils.GetMultiTileTopLeft(i, j, tileData, out int left, out int top);
					for (int x = 0; x < tileData.Width && powerMultiTile; x++) {
						for (int y = 0; y < tileData.Height && powerMultiTile; y++) {
							if (Main.tile[left + x, top + y].Get<Ashen_Wire_Data>().AnyPower) powerMultiTile = false;
						}
					}
				}
				if (powerMultiTile) {
					try {
						HittingAshenWires = true;
						WiringMethods.HitWireSingle(i, j);
					} finally {
						HittingAshenWires = false;
					}
				}
			}
			Ashen_Wire_System.SendWireData(i, j, Main.myPlayer);
		}
		public static void SetMultiTilePowered(int i, int j, bool value) {
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			for (int x = 0; x < data.Width; x++) {
				for (int y = 0; y < data.Height; y++) {
					SetTilePowered(left + x, top + y, value, false);
				}
			}
			WiringMethods._wireSkip.Value.Clear();
		}
		public static void SetTilePowered(int i, int j, bool value, bool clearWireSkip = true) {
			ref Ashen_Wire_Data data = ref Main.tile[i, j].Get<Ashen_Wire_Data>();
			if (value == data.IsTilePowered) return;
			data.IsTilePowered = value;
			if (data.AnyWire) Main.NewText($"power: {value}");
			if (value) {
				PropegatePowerState(i, j, 0, value);
				PropegatePowerState(i, j, 1, value);
				PropegatePowerState(i, j, 2, value);
			} else {
				TryPropegateDepowered(i, j, 0);
				TryPropegateDepowered(i, j, 1);
				TryPropegateDepowered(i, j, 2);
			}
			if (clearWireSkip) WiringMethods._wireSkip.Value.Clear();
		}
		static void TryPropegateDepowered(int i, int j, int wireType) {
			bool Counter(Point position) {
				return Framing.GetTileSafely(position).Get<Ashen_Wire_Data>().GetWire(wireType);
			}
			bool Breaker(AreaAnalysis analysis) {
				return Framing.GetTileSafely(analysis.Counted[^1]).Get<Ashen_Wire_Data>().IsTilePowered;
			}
			if (!AreaAnalysis.March(i, j, AreaAnalysis.Orthogonals, Counter, Breaker).Broke) PropegatePowerState(i, j, wireType, false);
		}
		static void PropegatePowerState(int i, int j, int wireType, bool value) {
			bool Counter(Point position) {
				return Framing.GetTileSafely(position).Get<Ashen_Wire_Data>().GetWire(wireType);
			}
			bool Breaker(AreaAnalysis analysis) => false;
			IReadOnlyList<Point> tiles = AreaAnalysis.March(i, j, AreaAnalysis.Orthogonals, Counter, Breaker).Counted;
			for (int k = 0; k < tiles.Count; k++) SetPowered(tiles[k].X, tiles[k].Y, wireType, value);
		}
		public readonly void DrawWires(int i, int j) {
			DrawWire<Brown_Wire_Toggle>(i, j, 0);
			DrawWire<Black_Wire_Toggle>(i, j, 1);
			DrawWire<White_Wire_Toggle>(i, j, 2);
		}
		readonly void DrawWire<TBuilderToggle>(int i, int j, int wireType) where TBuilderToggle : WireBuilderToggle {
			if (GetWire(wireType)) {
				Color color = Lighting.GetColor(i, j);
				int num15 = 0;
				if (Main.tile[i, j - 1].Get<Ashen_Wire_Data>().GetWire(wireType)) num15 += 18;
				if (Main.tile[i + 1, j].Get<Ashen_Wire_Data>().GetWire(wireType)) num15 += 36;
				if (Main.tile[i, j + 1].Get<Ashen_Wire_Data>().GetWire(wireType)) num15 += 72;
				if (Main.tile[i - 1, j].Get<Ashen_Wire_Data>().GetWire(wireType)) num15 += 144;
				float colorMult = 1;
				switch (Main.LocalPlayer.OriginPlayer().InfoAccMechShowAshenWires ? Main.LocalPlayer.BuilderToggleState<TBuilderToggle>() : 1) {
					case 0:
					color = Color.White;
					break;
					case 2:
					color *= 0.5f;
					colorMult = 0.5f;
					break;
					case 3:
					color = Color.Transparent;
					colorMult = 0;
					break;
				}
				Main.spriteBatch.Draw(
					underlayTexture.Value,
					new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y),
					new Rectangle(num15, wireType * 18, 16, 16),
					color * (WiresUI.Settings.HideWires ? 0.5f : 1f),
					0f,
					Vector2.Zero,
					1f,
					SpriteEffects.None,
					0f
				);
				if (GetPower(wireType)) Main.spriteBatch.Draw(
						glowTexture.Value,
						new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y),
						new Rectangle(num15, 0, 16, 16),
						new Color(255, 255, 255, 0) * (colorMult * pulse.Value),
						0f,
						Vector2.Zero,
						1f,
						SpriteEffects.None,
						0f
					);
			}
		}
		internal static Asset<Texture2D> underlayTexture;
		internal static Asset<Texture2D> glowTexture;
		static void IL_Main_DrawWires(ILContext il) {
			ILCursor c = new(il);
			int x = -1;
			int y = -1;
			int _tile = -1;
			c.GotoNext(MoveType.AfterLabel,
				i => i.MatchLdsflda<Main>(nameof(Main.tile)),
				i => i.MatchLdloc(out x),
				i => i.MatchLdloc(out y),
				i => i.MatchCall<Tilemap>("get_Item"),
				i => i.MatchStloc(out _tile),
				i => i.MatchLdloca(_tile),
				i => i.MatchCall<Tile>("actuator"),
				i => i.MatchBrfalse(out _)
			);
			c.EmitLdloc(x);
			c.EmitLdloc(y);
			c.EmitDelegate((int i, int j) => Main.tile[i, j].Get<Ashen_Wire_Data>().DrawWires(i, j));
		}
		public static void Load() {
			IL_Main.DrawWires += IL_Main_DrawWires;
			const string texture = "Origins/Items/Tools/Wiring/Ashen_Wires";
			underlayTexture = ModContent.Request<Texture2D>(texture);
			glowTexture = ModContent.Request<Texture2D>(texture + "_Active");
		}
	}
	public class Ashen_Wire_Global_Tile : GlobalTile {
		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (!fail) Ashen_Wire_Data.SetTilePowered(i, j, false);
		}
	}
	public class Ashen_Wire_System : ModSystem {
		public override void Load() {
			Ashen_Wire_Data.Load();
		}
		public static void SendWireData(int i, int j, int ignoreClient = -1) {
			if (NetmodeActive.SinglePlayer) return;
			ModPacket packet = Origins.instance.GetPacket();
			packet.Write(Origins.NetMessageType.sync_ashen_wires);
			packet.Write((ushort)i);
			packet.Write((ushort)j);
			packet.Write(Main.tile[i, j].Get<Ashen_Wire_Data>().data);
			packet.Send(ignoreClient: ignoreClient);
		}
		public override void SaveWorldData(TagCompound tag) {
			using MemoryStream data = new(Main.maxTilesX);
			using (BinaryWriter writer = new(data, Encoding.UTF8)) {
				writer.Write((byte)0); // version just in case 
									   // if MyTileData is updated, update this 'version' number 
									   // and add handling logic in LoadWorldData for backwards compat
				writer.Write(checked((ushort)Main.maxTilesX));
				writer.Write(checked((ushort)Main.maxTilesY));
				ReadOnlySpan<byte> worldData = MemoryMarshal.Cast<Ashen_Wire_Data, byte>(Main.tile.GetData<Ashen_Wire_Data>());
				writer.Write(worldData);
				int count = 0;
				for (int i = 0; i < worldData.Length; i++) {
					if (worldData[i] != 0) count++;
				}
			}
			tag["AshenWires"] = data.ToArray();
		}
		public override void LoadWorldData(TagCompound tag) {
			if (tag.TryGet("AshenWires", out byte[] data)) {
				using BinaryReader reader = new(new MemoryStream(data), Encoding.UTF8);
				byte version = reader.ReadByte();
				if (version == 0) {
					int width = reader.ReadUInt16();
					int height = reader.ReadUInt16();
					if (width != Main.maxTilesX || height != Main.maxTilesY) {
						// the world was somehow resized
						// up to you what to do here 
						throw new NotImplementedException("World size was changed");
					} else {
						Span<byte> worldData = MemoryMarshal.Cast<Ashen_Wire_Data, byte>(Main.tile.GetData<Ashen_Wire_Data>().AsSpan());
						int length = reader.Read(worldData);
						int count = 0;
						for (int i = 0; i < worldData.Length; i++) {
							if (worldData[i] != 0) count++;
						}
					}
				}
				// add more else-ifs for newer versions of the data
				else {
					throw new Exception("Unknown world data saved version");
				}
			}
		}
	}
}
