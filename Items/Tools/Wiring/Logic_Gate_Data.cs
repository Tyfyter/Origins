using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using Terraria.IO;
using Terraria.ModLoader;

namespace Origins.Items.Tools.Wiring {
	public struct Logic_Gate_Data : ITileData {
		internal byte data;
		const byte truth_mask = 0b00000111;
		const byte power_mask = 0b00001000;
		const byte wires_mask = 0b11110000;
		public LogicGateTruthTable TruthTable {
			readonly get => Unsafe.BitCast<byte, LogicGateTruthTable>(data);
			private set => data = (byte)((data & ~truth_mask) | (Unsafe.BitCast<LogicGateTruthTable, byte>(value) & truth_mask));
		}
		public bool Powered {
			readonly get => GetBits(data, power_mask);
			private set => SetBits(value, ref data, power_mask);
		}
		public LogicGateWires Wires {
			readonly get => Unsafe.BitCast<byte, LogicGateWires>(data);
			private set => data = (byte)((data & ~wires_mask) | (Unsafe.BitCast<LogicGateWires, byte>(value) & wires_mask));
		}
		static bool GetBits(int bits, int mask)
			=> (bits & mask) != 0;
		static void SetBits(bool value, ref byte bits, byte mask) {
			if (value) bits |= mask;
			else bits &= (byte)~mask;
		}
		internal static Asset<Texture2D> texture;
		static void IL_Main_DrawWires(ILContext il) {
			ILCursor c = new(il);
			int x = -1;
			int y = -1;
			c.GotoNext(MoveType.AfterLabel,
				i => i.MatchLdloc(out x),
				i => i.MatchLdloc(out y),
				i => i.MatchCall<Main>(nameof(Player.CanDoWireStuffHere)),
				i => i.MatchBrfalse(out _)
			);
			c.EmitLdloc(x);
			c.EmitLdloc(y);
			c.EmitDelegate(static (int i, int j) => Main.tile[i, j].Get<Logic_Gate_Data>().DrawGate(i, j));
		}
		readonly void DrawGate(int i, int j) {
			if ((data & truth_mask) == 0) return;
			Color color = default;
			switch (Main.LocalPlayer.OriginPlayer().InfoAccMechShowAshenWires ? Main.LocalPlayer.BuilderToggleState<Logic_Gate_Toggle>() : 1) {
				case 0:
				color = Color.White;
				break;
				case 2:
				color = Lighting.GetColor(i, j) * 0.5f;
				break;
				case 3:
				color = Color.Transparent;
				break;
			}
			Main.spriteBatch.Draw(
				texture.Value,
				new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y),
				new Rectangle(18 * (data & truth_mask), 0, 16, 16),
				color * (WiresUI.Settings.HideWires ? 0.5f : 1f),
				0f,
				Vector2.Zero,
				1f,
				SpriteEffects.None,
				0f
			);
		}
		public static void Load() {
			IL_Main.DrawWires += IL_Main_DrawWires;
			texture = ModContent.Request<Texture2D>("Origins/Items/Tools/Wiring/Ashen_Logic_Gates");
		}
		public readonly struct LogicGateTruthTable {
			readonly byte value;
			public readonly bool this[bool a, bool b] => (value & (1 << (a.ToInt() + b.Mul(2) - 1))) != 0;
		}
		public readonly struct LogicGateWires {
			readonly byte value;
			const int a_mask = 0b0011;
			const int b_mask = 0b0100;
			const int o_mask = 0b1000;
			public void GetWires(out int a, out int b, out int output) {
				int v = value >> 4;
				a = (v & a_mask) % a_mask;
				b = (a + 1 + (v & b_mask >> 2)) % a_mask;
				output = 3 - (a + b) * (v & o_mask >> 3);
			}
		}
	}
	public class Logic_Gate_Toggle : WireBuilderToggle {
		public override string Texture => $"{GetType().Namespace.Replace('.', '/')}/Ashen_Wire_Builder_Icons";
		public override Position OrderPosition => new After(ModContent.GetInstance<White_Wire_Toggle>());
		public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams) {
			drawParams.Frame.X = 40;
			drawParams.Frame.Width = 14;
			return base.Draw(spriteBatch, ref drawParams);
		}
	}
}
