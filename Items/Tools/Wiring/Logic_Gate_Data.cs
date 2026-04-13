using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Buffs;
using Origins.UI;
using PegasusLib.Content;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using static Origins.Items.Tools.Wiring.Logic_Gate_System;

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
		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Type, new DrawAnimationManual(0b111 + 1) { Frame = TruthTable.Value });
			OriginsSets.Items.AshenWireable[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Actuator);
			Item.consumable = true;
		}
		public override bool IsLoadingEnabled(Mod mod) {
			if (TruthTable.IsEmpty) {
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
		public record class UI(Point Position) : AComponentUI(Position) {
			public UI(int i, int j) : this(new Point(i, j)) { }
			public override bool ShouldClose => base.ShouldClose || Main.tile[Position].Get<Logic_Gate_Data>().TruthTable.IsEmpty;
			public override Action Initialize(UIElement root) {
				switch (Data.TruthTable.Value) {
					case 0b100:
					case 0b111:
					case 0b011:
					case 0b001:
					return TwoInputs(root);

					case 0b101:
					root.Append(new UIImageFramed(ComponentUI.Textures, new(486, 154, 240, 150)));
					return SetupWires(root,
						wires => Data.Wires.GetWires(out wires[0], out _, out wires[1]),
						wires => {
							if (wires.Contains(-1)) return false;
							Logic_Gate_Data.SetWires(Position, Logic_Gate_Data.LogicGateWires.Create(wires[0], -1, wires[1]));
							return true;
						},
						new(24, 62),
						new(176, 62, true)
					);
				}
				return null;

				Action TwoInputs(UIElement root) {
					return SetupWires(root,
						wires => Data.Wires.GetWires(out wires[0], out wires[1], out wires[2]),
						wires => {
							if (wires.Contains(-1)) return false;
							Logic_Gate_Data.SetWires(Position, Logic_Gate_Data.LogicGateWires.Create(wires[0], wires[1], wires[2]));
							return true;
						},
						new(24, 36),
						new(24, 94),
						new(176, 62, true)
					);
				}
			}
		}
		class Logic_Gate_Interaction : GlobalTile {
			public override void Load() {
				On_Player.TileInteractionsCheck += On_Player_TileInteractionsCheck;
			}
			static void On_Player_TileInteractionsCheck(On_Player.orig_TileInteractionsCheck orig, Player self, int myX, int myY) {
				if (WiresUI.Settings.DrawWires && !WiresUI.Settings.HideWires && !Main.tile[myX, myY].Get<Logic_Gate_Data>().TruthTable.IsEmpty) {
					if (self.controlUseTile && self.releaseUseTile) SetComponentUI(new UI(myX, myY));
					return;
				}
				orig(self, myX, myY);
			}
		}
	}
	public struct Logic_Gate_Data : ITileData, IBroken {
		static string IBroken.BrokenReason => "Sync changes";
		public const string texture_path = "Origins/Items/Tools/Wiring/Ashen_Logic_Gates";
		private byte data;
		const byte truth_mask = 0b00000111;
		const byte power_mask = 0b00001000;
		const byte wires_mask = 0b11110000;
		public LogicGateTruthTable TruthTable {
			readonly get => Unsafe.BitCast<byte, LogicGateTruthTable>(data);
			private set => data = (byte)((data & ~truth_mask) | value.Value);
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
		public static void SetTruthTable(Point position, LogicGateTruthTable table) => Update(position, tile => tile.Get<Logic_Gate_Data>().TruthTable = table);
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
			readonly byte value = (byte)(value & truth_mask);
			public readonly byte Value => (byte)(value & truth_mask);
			public readonly bool IsEmpty => Value == 0;
			public readonly bool IsDiode => Value is 0b101 or 0b110;
			public readonly bool this[bool a, bool b] {
				get {
					int index = a.ToInt() + b.Mul(2);
					return (value & (1 << index - 1)) != 0;
				}
			}
			public readonly int this[int a, int b] => this[a != 0, b != 0].ToInt();
			public override bool Equals(object obj) => obj is LogicGateTruthTable other && Equals(other);
			public bool Equals(LogicGateTruthTable other) => other.Value == Value;
			public override int GetHashCode() => Value;
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
			public static LogicGateWires Create(int a, int b, int output) {
				if (a < 0 || a >= 3) throw new ArgumentException($"{a} was not in range [0, 3[", nameof(a));
				if (b == -1) {
					if (output == 3) b = (a + 1) % 3;
					else b = (3 - (a + output)) % 3;
				}
				if (b < 0 || b >= 3) throw new ArgumentException($"{b} was not in range [0, 3[", nameof(b));
				if (output < 0 || output > 3) throw new ArgumentException($"{output} was not in range [0, 3]", nameof(output));
				if (a == b) throw new ArgumentException($"{nameof(a)} and {nameof(b)} cannot be equal");
				if (a == output) throw new ArgumentException($"{nameof(a)} and {nameof(output)} cannot be equal");
				if (b == output) throw new ArgumentException($"{nameof(b)} and {nameof(output)} cannot be equal");
				int result = a;
				if ((b - a + 3) % 3 != 1) result |= b_mask;
				if (output != 3) result |= o_mask;
				return result;
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
			public static implicit operator LogicGateWires(int value) => new((byte)(value << 4));
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
	public class Logic_Component_Mode : WireMode, IBroken {
		static string IBroken.BrokenReason => "Needs PegaLib update for GetItemType and Visible";
		AutoLoadingAsset<Texture2D> back = "Origins/Items/Tools/Wiring/Ashen_Wires_BG";
		public override Color? WireKiteColor => new Color(179, 58, 0);
		public override bool IsExtra => true;
		public override void SetupSets() {
			OriginsSets.WireModes.LogicUpgrade[Type] = true;
		}
		public override bool GetWire(int x, int y) => !Main.tile[x, y].Get<Logic_Gate_Data>().TruthTable.IsEmpty;
		public override bool SetWire(int x, int y, bool value) {
			if (value) return false;
			if (!Main.tile[x, y].Get<Logic_Gate_Data>().TruthTable.IsEmpty) {
				Logic_Gate_Data.SetTruthTable(new(x, y), 0);
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
		public override IEnumerable<WireMode> SortAfter() => [ModContent.GetInstance<White_Wire_Mode>()];
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
	public class Logic_Gate_System : ModSystem {
		public override void Load() {
			Logic_Gate_Data.Load();
			if (Main.dedServ) return;
			ComponentUI.Textures = ModContent.Request<Texture2D>("Origins/UI/Logic_Components_Resource");
		}
		static readonly UserInterface componentUI = new();
		public override void UpdateUI(GameTime gameTime) {
			if (componentUI.CurrentState is not ComponentUI ui || ui.ShouldClose) {
				componentUI.SetState(null);
			}
			componentUI.Update(gameTime);
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryIndex != -1) {//error prevention & null check
				layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
					"Origins: Special Chest UI",
					delegate {
						componentUI?.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
		public interface IComponentUI {
			public bool ShouldClose { get; }
			public bool Locked => Main.LocalPlayer.OriginPlayer().InfoAccMechModifyComponents;
			public Vector2 StartPosition { get; }
			Action Initialize(UIElement root);
			public bool Equals(IComponentUI other);
		}
		public abstract record class AComponentUI(Point Position) : IComponentUI {
			public AComponentUI(int i, int j) : this(new Point(i, j)) { }
			public virtual bool ShouldClose => !LocalPlayerIsInInteractionRange(Position);
			public virtual Vector2 StartPosition => Position.ToWorldCoordinates(autoAddY: 16).ToScreenPosition();
			public ref Logic_Gate_Data Data => ref Main.tile[Position].Get<Logic_Gate_Data>();
			public abstract Action Initialize(UIElement root);
			protected Action SetupWires(UIElement root, Action<int[]> setup, ComponentUI.ApplySockets apply, params Span<ComponentUI.Socket> sockets) => ComponentUI.SetupWires(
				root,
				setup,
				apply,
				Position.ToWorldCoordinates(),
				sockets
			);
			bool IComponentUI.Equals(IComponentUI other) => other is AComponentUI otherUI && this == otherUI;
		}
		public class ComponentUI : UIState {
			public static Asset<Texture2D> Textures { get; set; }
			IComponentUI uiSource;
			Action onDeactivate;
			public bool ShouldClose => uiSource?.ShouldClose ?? true;
			public override void OnInitialize() {
				RemoveAllChildren();
				if (uiSource is null) return;
				UIBorder border = new();
				Append(
					new UIImageFramed(Textures, new(2, 2, 240, 150)) {
						HAlign = 0.5f
					}
					.MoveTo(uiSource.StartPosition - new Vector2(Main.screenWidth * 0.5f, 0))
					.Execute(UIDragController.Attach(new(_ => border.IsMouseHovering)))
					.StopClickThrough()
					.Execute(uiSource.Initialize, ref onDeactivate)
					.Execute(root => root.Append(border))
					.Execute(Lockout)
				);
			}
			class UIBorder() : UIImageFramed(Textures, new(244, 2, 240, 150)) {
				public override bool ContainsPoint(Vector2 point) {
					Rectangle dimensions = GetDimensions().ToRectangle();
					if (!dimensions.Contains(point)) return false;
					dimensions.Inflate(-12, -12);
					return !dimensions.Contains(point);
				}
			}
			void Lockout(UIElement root) {
				if (uiSource.Locked) return;
				root.Append(new UIImageFramed(Textures, new(2, 154, 240, 150)) {
					IgnoresMouseInteraction = false
				});
			}
			public override void OnDeactivate() => onDeactivate?.Invoke();
			public void SetUISource(IComponentUI newSource) {
				onDeactivate?.Invoke();
				if (uiSource?.Equals(newSource) ?? false) {
					uiSource = null;
					return;
				}
				uiSource = newSource;
				OnInitialize();
			}
			public ComponentUI(IComponentUI uiSource) => SetUISource(uiSource);
			public delegate bool ApplySockets(int[] wires);
			public static Action SetupWires(UIElement root, Action<int[]> setup, ApplySockets apply, Vector2? worldPosition = default, params Span<Socket> sockets) {
				Socket[] _sockets = new Socket[sockets.Length];
				for (int i = 0; i < sockets.Length; i++) {
					root.Append(_sockets[i] = sockets[i]);
				}
				int[] wires = new int[sockets.Length];
				setup(wires);
				for (int i = 0; i <= 3; i++) root.Append(new DraggableWire(i, _sockets, wires, apply, ShockPlayer));
				return () => {
					if (!apply(wires)) ShockPlayer();
				};
				void ShockPlayer() {
					Player player = Main.LocalPlayer;
					int dir = worldPosition.HasValue ? Math.Sign(player.Center.X - worldPosition.Value.X) : 0;
					player.Hurt(
						PlayerDeathReason.ByCustomReason(NetworkText.FromKey("Mods.Origins.DeathMessage.BadElectrician")),
						5,
						dir,
						cooldownCounter: ImmunityCooldownID.TileContactDamage,
						knockback: 3
					);
					player.AddBuff(Static_Shock_Debuff.ID, 60);
				}
			}
			public class Socket : UIImageFramed {
				public readonly bool output;
				public Socket(Vector2 position, bool output = false) : this(position.X, position.Y, output) { }
				public Socket(float x, float y, bool output = false) : base(Textures, new(2, 306, 40, 38)) {
					Left.Set(x, 0);
					Top.Set(y, 0);
					this.output = output;
				}
			}
			public class DraggableWire : UIElement {
				readonly int wireType;
				readonly Socket[] sockets;
				readonly Func<bool> isDragging;
				int connectedTo = -1;
				Vector2? disconnectedPosition;
				Vector2 edgePosition;
				public DraggableWire(int wireType, Socket[] sockets, int[] wires, ApplySockets apply, Action shockPlayer) {
					this.wireType = wireType;
					this.sockets = sockets;
					Width.Set(10, 0);
					Height.Set(10, 0);
					for (int i = 0; i < sockets.Length; i++) {
						if (wires[i] == wireType) {
							connectedTo = i;
							break;
						}
					}
					isDragging = UIDragController.Attach(this, new(
						PickUp: () => {
							if (connectedTo >= 0) {
								wires[connectedTo] = -1;
								//play detach sound here
							} else {
								//play pick-up sound here
							}
							connectedTo = -1;
						},
						ModifyOffset: (ref Vector2 position) => position += Main.MouseScreen - GetDimensions().Center(),
						Drop: () => {
							Vector2 center = GetDimensions().Center();
							for (int i = 0; i < sockets.Length; i++) {
								if (sockets[i].ContainsPoint(center)) {
									if (wireType == 3 && !sockets[i].output) {
										shockPlayer();
										break;
									}
									if (wires[i] != -1) {
										shockPlayer();
										break;
									}
									connectedTo = i;
									wires[connectedTo] = wireType;
									if (!wires.Contains(-1) && !apply(wires)) {
										shockPlayer();
										wires[connectedTo] = -1;
										connectedTo = -1;
										break;
									}
									//play attach sound here
									break;
								}
							}
							SetToRestPos();
						}
					));
				}
				public override void OnActivate() {
					CalculatedStyle parent = Parent.GetDimensions();
					switch (wireType) {
						case 0:
						disconnectedPosition = new(parent.Width * 0.25f - 2, parent.Height - 8);
						break;
						case 1:
						disconnectedPosition = new(parent.Width * 0.5f - 2, parent.Height - 8);
						break;
						case 2:
						disconnectedPosition = new(parent.Width * 0.75f - 2, parent.Height - 8);
						break;
						case 3:
						disconnectedPosition = new(parent.Width - 8, parent.Height * 0.5f - 16);
						break;
					}
					SetToRestPos();
				}
				void SetToRestPos() {
					if (connectedTo != -1) {
						CalculatedStyle parent = Parent.GetDimensions();
						CalculatedStyle socket = sockets[connectedTo].GetDimensions();
						edgePosition = new(sockets[connectedTo].output ? (parent.Width - 8) : 8, socket.Center().Y - parent.Y);

						Left.Set(Math.Clamp(edgePosition.X + parent.X, socket.X, socket.X + socket.Width - 8) - parent.X - 2.5f, 0);
						Top.Set(edgePosition.Y - 5, 0);
						return;
					}
					edgePosition = disconnectedPosition.Value;
					Left.Set(edgePosition.X - 5, 0);
					Top.Set(edgePosition.Y - 5, 0);
					switch (wireType) {
						case 0:
						case 1:
						case 2:
						Top.Pixels -= 12;
						break;
						case 3:
						Left.Pixels -= 12;
						break;
					}
				}
				public override bool ContainsPoint(Vector2 point) {
					if (base.ContainsPoint(point)) return true;
					return Collision.CheckAABBvLineCollision(
						point - new Vector2(5),
						new Vector2(10),
						GetDimensions().Center(),
						edgePosition + Parent.GetDimensions().Position()
					);
				}
				protected override void DrawSelf(SpriteBatch spriteBatch) {
					Vector2 edge = edgePosition + Parent.GetDimensions().Position();
					Vector2 center = GetDimensions().Center();
					Vector2 diff = (center - edge).Normalized(out float dist);
					Vector2 current = center - diff * 5;
					bool highlight = IsMouseHovering || isDragging();
					for (; dist > 0; dist -= 14) {
						int amount = Math.Min((int)dist, 14);
						if (highlight) spriteBatch.Draw(
							Textures.Value,
							current,
							new Rectangle(132 + (14 - amount), 306, amount, 12),
							Main.OurFavoriteColor,
							diff.ToRotation(),
							new Vector2(amount, 6),
							1,
							SpriteEffects.None,
						0);
						spriteBatch.Draw(
							Textures.Value,
							current,
							new Rectangle(80 + (14 - amount), 306 + 10 * wireType, amount, 8),
							Color.White,
							diff.ToRotation(),
							new Vector2(amount, 4),
							1,
							SpriteEffects.None,
						0);
						current -= diff * 14;
					}
					if (highlight) spriteBatch.Draw(
						Textures.Value,
						center,
						new Rectangle(156, 306, 12, 12),
						Main.OurFavoriteColor,
						diff.ToRotation(),
						new Vector2(5, 6),
						1,
						SpriteEffects.None,
					0);
					spriteBatch.Draw(
						Textures.Value,
						center,
						new Rectangle(120, 306 + 10 * wireType, 10, 8),
						Color.White,
						diff.ToRotation(),
						new Vector2(5, 4),
						1,
						SpriteEffects.None,
					0);
				}
			}
		}
		public static void SetComponentUI(IComponentUI newSource) {
			if (componentUI.CurrentState is ComponentUI ui) ui.SetUISource(newSource);
			else componentUI.SetState(new ComponentUI(newSource));
		}
		public static bool LocalPlayerIsInInteractionRange(Point position, int width = 1, int height = 1) {
			Player player = Main.LocalPlayer;
			int playerCenterX = (int)((player.position.X + player.width * 0.5f) / 16);
			int playerCenterY = (int)((player.position.Y + player.height * 0.5f) / 16);
			Rectangle r = new(position.X * 16, position.Y * 16, width * 16, height * 16);
			r.Inflate(-1, -1);
			Point point = r.ClosestPointInRect(player.Center).ToTileCoordinates();
			return playerCenterX >= point.X - Player.tileRangeX && playerCenterX <= point.X + Player.tileRangeX + 1
				&& playerCenterY >= point.Y - Player.tileRangeY && playerCenterY <= point.Y + Player.tileRangeY + 1;
		}

		public override void SaveWorldData(TagCompound tag) {
			using MemoryStream data = new(Main.maxTilesX);
			using (BinaryWriter writer = new(data, Encoding.UTF8)) {
				writer.Write((byte)0); // version just in case 
									   // if MyTileData is updated, update this 'version' number 
									   // and add handling logic in LoadWorldData for backwards compat
				writer.Write(checked((ushort)Main.maxTilesX));
				writer.Write(checked((ushort)Main.maxTilesY));
				writer.Write(MemoryMarshal.Cast<Logic_Gate_Data, byte>(Main.tile.GetData<Logic_Gate_Data>()));
			}
			tag["LogicComponents"] = data.ToArray();
		}
		public override void LoadWorldData(TagCompound tag) {
			if (tag.TryGet("LogicComponents", out byte[] data)) {
				using BinaryReader reader = new(new MemoryStream(data), Encoding.UTF8);
				switch (reader.ReadByte()) {
					case 0:
					int width = reader.ReadUInt16();
					int height = reader.ReadUInt16();
					if (width != Main.maxTilesX || height != Main.maxTilesY) {
						// the world was somehow resized
						// up to you what to do here 
						throw new NotImplementedException("World size was changed");
					} else {
						reader.Read(MemoryMarshal.Cast<Logic_Gate_Data, byte>(Main.tile.GetData<Logic_Gate_Data>().AsSpan()));
					}
					break;
					default:
					throw new Exception("Unknown world data saved version");
				}
			}
		}
	}
}
