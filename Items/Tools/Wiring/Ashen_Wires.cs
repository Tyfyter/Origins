using CalamityMod.Buffs.Potions;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Newtonsoft.Json.Linq;
using Origins.Items.Weapons.Demolitionist;
using Origins.Reflection;
using Origins.UI;
using Origins.World;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.Items.Tools.Wiring {
	public class Brown_Wire_Mode : WireMode {
		AutoLoadingAsset<Texture2D> back = "Origins/Items/Tools/Wiring/Ashen_Wires_BG";
		public override Color? WireKiteColor => Color.Chocolate;
		public override void SetupSets() {
			Sets.AshenWires[Type] = true;
		}
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
	}
	public struct Ashen_Wire_Data : ITileData {
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
		public readonly bool AnyPower => BrownWirePowered || BlackWirePowered;
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
				if (value) if (GetPower(i + 1, j, wireType) || GetPower(i - 1, j, wireType) || GetPower(i, j + 1, wireType) || GetPower(i, j - 1, wireType)) {
						SetPowered(i, j, wireType, true);
						if (!GetPower(i + 1, j, wireType) || !GetPower(i - 1, j, wireType) || !GetPower(i, j + 1, wireType) || !GetPower(i, j - 1, wireType)) PropegatePowerState(i, j, wireType, true);
					} else if (data.IsTilePowered) PropegatePowerState(i, j, wireType, true);
					else {
						SetPowered(i, j, wireType, false);
						TryPropegateDepowered(i + 1, j, wireType);
						TryPropegateDepowered(i - 1, j, wireType);
						TryPropegateDepowered(i, j + 1, wireType);
						TryPropegateDepowered(i, j - 1, wireType);
					}
			}
		}
		static void SetPowered(int i, int j, int wireType, bool value) {
			ref Ashen_Wire_Data data = ref Main.tile[i, j].Get<Ashen_Wire_Data>();
			bool wasPowered = data.AnyPower;
			if (value != data.GetPower(wireType)) SetBit(value, ref data.data, (wireType << 1) + 1);
			if (data.AnyPower != wasPowered) WiringMethods.HitWireSingle(i, j);
		}
		public static void SetTilePowered(int i, int j, bool value) {
			ref Ashen_Wire_Data data = ref Main.tile[i, j].Get<Ashen_Wire_Data>();
			if (value == data.IsTilePowered) return;
			data.IsTilePowered = value;
			if (value) {
				PropegatePowerState(i, j, 0, value);
				PropegatePowerState(i, j, 1, value);
			} else {
				TryPropegateDepowered(i, j, 0);
				TryPropegateDepowered(i, j, 1);
			}
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
			DrawWire(i, j, 0);
			DrawWire(i, j, 1);
		}
		readonly void DrawWire(int i, int j, int wireType) {
			if (GetWire(wireType)) {
				Color color = Lighting.GetColor(i, j);
				int num15 = 0;
				if (Main.tile[i, j - 1].Get<Ashen_Wire_Data>().GetWire(wireType)) num15 += 18;
				if (Main.tile[i + 1, j].Get<Ashen_Wire_Data>().GetWire(wireType)) num15 += 36;
				if (Main.tile[i, j + 1].Get<Ashen_Wire_Data>().GetWire(wireType)) num15 += 72;
				if (Main.tile[i - 1, j].Get<Ashen_Wire_Data>().GetWire(wireType)) num15 += 144;
				switch (Main.LocalPlayer.InfoAccMechShowWires ? Main.LocalPlayer.builderAccStatus[9] : 1) {
					case 0:
					color = Color.White;
					break;
					case 2:
					color *= 0.5f;
					break;
					case 3:
					color = Color.Transparent;
					break;
				}
				Main.spriteBatch.Draw(
					underlayTexture.Value,
					new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y),
					new Rectangle(num15, 0, 16, 16),
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
						new Color(255, 255, 255, 0),
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
}
