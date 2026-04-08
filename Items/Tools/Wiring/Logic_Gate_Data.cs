using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.UI.Snippets;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.Items.Tools.Wiring {
	public class Logic_Gate(Logic_Gate_Data.LogicGateTruthTable truthTable, string name) : ModItem {
		static readonly Dictionary<Logic_Gate_Data.LogicGateTruthTable, Logic_Gate> gates = new() {
			[new(0)] = null
		};
		Logic_Gate() : this(default, default) { }
		public readonly string name = name;
		public static string GetName(Logic_Gate_Data.LogicGateTruthTable table) => gates[table].name;
		public override string Name => $"{base.Name}_{name}";
		public Logic_Gate_Data.LogicGateTruthTable TruthTable { get; } = truthTable;
		protected override bool CloneNewInstances => true;
		public override string Texture => Logic_Gate_Data.texture_path;
		public static LocalizedText GetLocalization(string suffix) => Language.GetText($"Mods.Origins.Items.Logic_Gate.{suffix}");
		public static LocalizedText TryGetLocalization(string suffix) => TextUtils.LanguageTree.Find($"Mods.Origins.Items.Logic_Gate.{suffix}")?.value;
		public override LocalizedText DisplayName => GetLocalization("DisplayName").WithFormatArgs(TryGetLocalization($"Name.{name}")?.Value ?? name);
		public override LocalizedText Tooltip => TruthTable.GetStatement("A", "B", GetLocalization("Output"));
		public override void SetStaticDefaults() => Main.RegisterItemAnimation(Item.type, new DrawAnimationManual(0b111 + 1) { Frame = TruthTable.value });
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Actuator);
			Item.consumable = true;
		}
		public override bool IsLoadingEnabled(Mod mod) {
			if (TruthTable.IsEmpty) {
				Logic_Gate_Data.Load();
				mod.AddContent(new Logic_Gate(0b100, "AND"));
				mod.AddContent(new Logic_Gate(0b111, "OR"));
				mod.AddContent(new Logic_Gate(0b011, "XOR"));
				mod.AddContent(new Logic_Gate(0b001, "NIMPLY"));
				mod.AddContent(new Logic_Gate(0b101, "Diode"));
				//mod.AddContent(new Logic_Gate(0b010, "NIMPLIED"));
				FixNIMPLY();
				return false;
			}
			return gates.TryAdd(TruthTable, this);
		}
		static void FixNIMPLY() {
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			DynamicSpriteFont.SpriteCharacterData badArrow = font.SpriteCharacters['⇏'];
			if (badArrow.Glyph != new Rectangle(92, 45, 13, 12)) return;
			if (badArrow.Kerning != new Vector3(1, 13, 1)) return;
			if (badArrow.Padding != new Rectangle(0, 3, 13, 29)) return;
			if (!ModContent.RequestIfExists("Origins/Textures/Chars/Mouse_Text_NIMPLY", out Asset<Texture2D> asset)) return;
			Task.Run(asset.Wait).ContinueWith(_ => {
				font.SpriteCharacters['⇏'] = new(
					asset.Value,
					asset.Value.Bounds,
					new(0, 3, 18, 29),
					new(1, 18, 1)
				);
			});
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			LocalizedText text = ItemSlot.ShiftInUse ? 
				GetLocalization("Table").WithFormatArgs(TruthTable[1, 1], TruthTable[1, 0], TruthTable[0, 1])
				: GetLocalization("HoldShiftForTruthTable");
			tooltips.Add("TruthTable", text);
		}
		public override bool? UseItem(Player player) {
			if (!player.controlUseItem) return false;
			if (Main.tile[Player.tileTargetX, Player.tileTargetY].Get<Logic_Gate_Data>().TruthTable.IsEmpty) {
				SoundEngine.PlaySound(SoundID.Dig, new(Player.tileTargetX * 16, Player.tileTargetY * 16));
				Logic_Gate_Data.SetTruthTable(new(Player.tileTargetX, Player.tileTargetY), TruthTable);
				//AutopounderSystem.SendAutopounderData(Player.tileTargetX, Player.tileTargetY, Main.myPlayer);
				return true;
			}

			return false;
		}
	}
	public struct Logic_Gate_Data : ITileData {
		public const string texture_path = "Origins/Items/Tools/Wiring/Ashen_Logic_Gates";
		internal byte data;
		const byte truth_mask = 0b00000111;
		const byte power_mask = 0b00001000;
		const byte wires_mask = 0b11110000;
		public LogicGateTruthTable TruthTable {
			readonly get => Unsafe.BitCast<byte, LogicGateTruthTable>(data);
			private set => data = (byte)((data & ~truth_mask) | (value.value & truth_mask));
		}
		public bool Powered {
			readonly get => GetBits(data, power_mask);
			private set => SetBits(value, ref data, power_mask);
		}
		public LogicGateWires Wires {
			readonly get => Unsafe.BitCast<byte, LogicGateWires>(data);
			private set => data = (byte)((data & ~wires_mask) | (Unsafe.BitCast<LogicGateWires, byte>(value) & wires_mask));
		}
		public readonly int OutputWire {
			get {
				Wires.GetWires(out _, out _, out int output);
				return output;
			}
		}
		static bool GetBits(int bits, int mask)
			=> (bits & mask) != 0;
		static void SetBits(bool value, ref byte bits, byte mask) {
			if (value) bits |= mask;
			else bits &= (byte)~mask;
		}
		public readonly bool ShouldCountAsPowerSource(Point position, int forWireType, bool skipWalked = false) {
			if (skipWalked && walkedLogicOutputs[position] > 0) return false;
			return OutputWire != forWireType && PowersWire(position, out _);
		}
		public readonly bool PowersWire(Point position, out int output) {
			Wires.GetWires(out int a, out int b, out output);
			if (TruthTable.IsEmpty) return false;
			using WalkedLogicOutput _ = new(position);
			return TruthTable[
				IAshenPowerConduitTile.FindValidPowerSource(position, a),
				IAshenPowerConduitTile.FindValidPowerSource(position, b)
			];
		}
		public void PostUpdate(Point position, int oldOutput) {
			bool isPowered = PowersWire(position, out int newOutput);
			if (newOutput != oldOutput) {
				if (Powered) ChangeWires(position, oldOutput, false);
				ChangeWires(position, newOutput, isPowered);
			} else if (isPowered != Powered) {
				ChangeWires(position, newOutput, isPowered);
			}
			Powered = isPowered;
			static void ChangeWires(Point position, int kind, bool powered) {
				if (kind == 3) {
					Terraria.Wiring.TripWire(position.X, position.Y, 1, 1);
					return;
				}
				if (powered) {
					Ashen_Wire_Data.PropegatePowerState(position.X, position.Y, kind, true);
				} else {
					Ashen_Wire_Data.TryPropegateDepowered(position.X, position.Y, kind);
				}
			}
		}
		public delegate void LogicGateUpdate(Tile tile);
		public static void Update(Point position, LogicGateUpdate update) {
			Tile tile = Main.tile[position];
			ref Logic_Gate_Data data = ref tile.Get<Logic_Gate_Data>();
			data.Wires.GetWires(out _, out _, out int oldOutput);
			update(tile);
			data.PostUpdate(position, oldOutput);
		}
		public static void SetTruthTable(Point position, LogicGateTruthTable table) => Update(position, tile => {
			tile.Get<Logic_Gate_Data>().TruthTable = table;
			tile.Get<Logic_Gate_Data>().Wires = new(0b10000000);
			Main.NewText(tile.Get<Logic_Gate_Data>().GetStatement());
		});
		public static void SetWires(Point position, LogicGateWires wires) => Update(position, tile => tile.Get<Logic_Gate_Data>().Wires = wires);

		internal static Asset<Texture2D> texture;
		static void IL_Main_DrawWires(ILContext il) {
			ILCursor c = new(il);
			int x = -1;
			int y = -1;
			c.GotoNext(MoveType.After,
				i => i.MatchLdloc(out x),
				i => i.MatchLdloc(out y),
				i => i.MatchCallOrCallvirt<Player>(nameof(Player.CanDoWireStuffHere)),
				i => i.MatchBrfalse(out _)
			);
			c.EmitLdloc(x);
			c.EmitLdloc(y);
			c.EmitDelegate(static (int i, int j) => Main.tile[i, j].Get<Logic_Gate_Data>().DrawGate(i, j));
		}
		readonly void DrawGate(int i, int j) {
			if (TruthTable.IsEmpty) return;
			Color color = Lighting.GetColor(i, j);
			switch (Main.LocalPlayer.OriginPlayer().InfoAccMechShowAshenWires ? Main.LocalPlayer.BuilderToggleState<Logic_Gate_Toggle>() : 1) {
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
			texture = ModContent.Request<Texture2D>(texture_path);
		}
		public readonly LocalizedText GetStatement() {
			Wires.GetWires(out int a, out int b, out int output);
			static string GetName(int value) => value switch {
				0 => "Origins/Brown_Wire_Mode",
				1 => "Origins/Black_Wire_Mode",
				2 => "Origins/White_Wire_Mode",
				3 => "Vanilla",
				_ => throw new NotImplementedException()
			};
			static string Get(int value) => true ? $"[wireblock:{GetName(value)}]" : GetName(value);
			return TruthTable.GetStatement(
				Get(a),
				Get(b),
				Get(output)
			);
		}
		[DebuggerDisplay("{value:B}")]
		public readonly struct LogicGateTruthTable(byte value) : IEquatable<LogicGateTruthTable> {
			public readonly byte value = (byte)(value & truth_mask);
			public readonly bool IsEmpty => (value & truth_mask) == 0;
			public readonly bool this[bool a, bool b] {
				get {
					int index = a.ToInt() + b.Mul(2);
					return (value & (1 << index - 1)) != 0;
				}
			}
			public readonly int this[int a, int b] => this[a != 0, b != 0].ToInt();
			public override bool Equals(object obj) => obj is LogicGateTruthTable other && Equals(other);
			public bool Equals(LogicGateTruthTable other) => (other.value & truth_mask) == (value & truth_mask);
			public override int GetHashCode() => value & truth_mask;
			public static bool operator ==(LogicGateTruthTable left, LogicGateTruthTable right) => left.Equals(right);
			public static bool operator !=(LogicGateTruthTable left, LogicGateTruthTable right) => !(left == right);
			public static implicit operator LogicGateTruthTable(byte value) => new(value);
			public static implicit operator LogicGateTruthTable(int value) => (byte)value;
			public readonly LocalizedText GetStatement(object a, object b, object output) {
				string name = Logic_Gate.GetName(this);
				return (Logic_Gate.TryGetLocalization("Statement." + name) ?? Logic_Gate.GetLocalization("Statement")).WithFormatArgs(
					a,
					Logic_Gate.GetLocalization("LogicSymbols." + name),
					b,
					output
				);
			}
			public override string ToString() => 
				$"   B ¬B\n" +
				$" A {this[1, 1]}  {this[1, 0]}\n" +
				$"¬A {this[0, 1]}  0";
		}
		public readonly struct LogicGateWires(byte value) {
			readonly byte value = (byte)(value & wires_mask);
			const int a_mask = 0b0011;
			const int b_mask = 0b0100;
			const int o_mask = 0b1000;
			public void GetWires(out int a, out int b, out int output) {
				int v = value >> 4;
				a = (v & a_mask) % a_mask;
				b = (a + 1 + ((v & b_mask) >> 2)) % a_mask;
				output = 3 - (a + b) * ((v & o_mask) >> 3);
			}
			public override string ToString() {
				GetWires(out int a, out int b, out int output);
				static string Get(int value) => value switch {
					0 => "Brown",
					1 => "Black",
					2 => "White",
					3 => "Vanilla",
					_ => throw new NotImplementedException()
				};
				return $"{Get(a)}, {Get(b)} => {Get(output)}";
			}
			public override bool Equals(object obj) => obj is LogicGateWires other && Equals(other);
			public bool Equals(LogicGateWires other) => (other.value & wires_mask) == (value & wires_mask);
			public override int GetHashCode() => value & wires_mask;
			public static bool operator ==(LogicGateWires left, LogicGateWires right) => left.Equals(right);
			public static bool operator !=(LogicGateWires left, LogicGateWires right) => !(left == right);
			public static implicit operator LogicGateWires(byte value) => new(value);
			public static implicit operator LogicGateWires(int value) => (byte)value;
		}
		public readonly ref struct WalkedLogicOutput : IDisposable {
			public readonly Point position;
			public WalkedLogicOutput(Point position) {
				this.position = position;
				walkedLogicOutputs[position]++;
			}
			public void Dispose() => walkedLogicOutputs[position]--;
		}
		static readonly FungibleSet<Point> walkedLogicOutputs = [];
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
