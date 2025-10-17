using Microsoft.Xna.Framework.Graphics;
using Origins.UI;
using ReLogic.Content;
using ReLogic.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools.Wiring {
	public static class WireModeLoader {
		public static int WireModeCount => wireModes.Count;
		static readonly List<WireMode> wireModes = [];
		static readonly List<WireMode> sortedWireModes = [];
		public static WireMode Get(int type) => wireModes.GetIfInRange(type);
		public static WireMode GetSorted(int index) => sortedWireModes.GetIfInRange(index);
		public static IEnumerable<WireMode> GetSorted(bool[] set) {
			for (int i = 0; i < sortedWireModes.Count; i++) {
				if (set[sortedWireModes[i].Type]) yield return sortedWireModes[i];
			}
		}
		public static IEnumerable<WireMode> GetSorted(BitArray set) {
			for (int i = 0; i < sortedWireModes.Count; i++) {
				if (set[sortedWireModes[i].Type]) yield return sortedWireModes[i];
			}
		}
		internal static void Register(WireMode mode) {
			mode.Type = WireModeCount;
			wireModes.Add(mode);
		}
		internal static void Sort() {
			sortedWireModes.Clear();
			sortedWireModes.AddRange(new TopoSort<WireMode>(wireModes,
				mode => mode.SortAfter(),
				mode => mode.SortBefore()
			).Sort());
		}
	}
	[Flags]
	public enum WirePetalData {
		Enabled = 1 << 0,
		Cutter = 1 << 1
	}
	public abstract class WireMode : ModTexturedType, IFlowerMenuItem<WirePetalData> {
		public int Type { get; internal set; }
		public virtual int ItemType { get; } = ItemID.Wire;
		public virtual bool IsExtra => false;
		public virtual int BreakDust => -1;
		public virtual Color? WireKiteColor => null;
		public Asset<Texture2D> Texture2D { get; private set; }
		protected sealed override void Register() {
			WireModeLoader.Register(this);
		}
		public sealed override void SetupContent() {
			if (!Main.dedServ) Texture2D = ModContent.Request<Texture2D>(Texture);
			SetStaticDefaults();
		}
		public virtual void SetupSets() { }
		public abstract bool SetWire(int x, int y, bool value);
		public static void DrawIcon(Texture2D texture, Vector2 position, Color tint) {
			Main.spriteBatch.Draw(
				texture,
				position,
				null,
				tint,
				0f,
				texture.Size() * 0.5f,
				1,
				SpriteEffects.None,
			0f);
		}
		public static void GetTints(bool hovered, bool enabled, out Color backTint, out Color iconTint) {
			if (enabled) {
				backTint = Color.White;
				iconTint = Color.White;
			} else if (hovered) {
				backTint = new Color(200, 200, 200);
				iconTint = new Color(120, 120, 120);
			} else {
				backTint = new Color(100, 100, 100);
				iconTint = new Color(80, 80, 80);
			}
		}
		public virtual void Draw(Vector2 position, bool hovered, WirePetalData data) {
			GetTints(hovered, data.HasFlag(WirePetalData.Enabled), out Color backTint, out Color iconTint);
			DrawIcon(TextureAssets.WireUi[hovered.ToInt() + data.HasFlag(WirePetalData.Cutter).ToInt() * 8].Value, position, backTint);
			DrawIcon(Texture2D.Value, position, iconTint);
		}
		public virtual IEnumerable<WireMode> SortAfter() => [];
		public virtual IEnumerable<WireMode> SortBefore() => [];
		public bool IsHovered(Vector2 position) => Main.MouseScreen.WithinRange(position, 20);

		[ReinitializeDuringResizeArrays]
		public static class Sets {
			public static SetFactory Factory = new(WireModeLoader.WireModeCount, nameof(WireModeID), WireModeID.Search);
			public static BitArray NormalWires = new(Factory.CreateBoolSet());
			public static BitArray AshenWires = new(Factory.CreateBoolSet());
			static Sets() {
				WireModeLoader.Sort();
				foreach (WireMode mode in ModContent.GetContent<WireMode>()) mode.SetupSets();
			}
		}
		private class WireModeID {
			/// <inheritdoc cref="IdDictionary"/>
			public static readonly IdDictionary Search = IdDictionary.Create<WireModeID, byte>();
		}
	}
	public class Actuator_Wire_Mode : WireMode {
		public override string Texture => "Terraria/Images/UI/Wires_10";
		public override int ItemType => ItemID.Actuator;
		public override bool IsExtra => true;
		public override void SetupSets() {
			Sets.NormalWires[Type] = true;
		}
		public override bool SetWire(int x, int y, bool value) {
			Tile tile = Main.tile[x, y];
			if (tile.HasActuator != value) {
				tile.HasActuator = value;
				NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 8 + (!value).ToInt(), x, y);
				return true;
			}
			return false;
		}
	}
	public class Red_Wire_Mode : WireMode {
		public override string Texture => "Terraria/Images/UI/Wires_2";
		public override int BreakDust => DustID.Adamantite;
		public override Color? WireKiteColor => Color.Red;
		public override void SetupSets() {
			Sets.NormalWires[Type] = true;
		}
		public override bool SetWire(int x, int y, bool value) {
			Tile tile = Main.tile[x, y];
			if (tile.RedWire != value) {
				tile.RedWire = value;
				NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 5 + (!value).ToInt(), x, y);
				return true;
			}
			return false;
		}
	}
	public class Blue_Wire_Mode : WireMode {
		public override string Texture => "Terraria/Images/UI/Wires_4";
		public override Color? WireKiteColor => Color.Blue;
		public override void SetupSets() {
			Sets.NormalWires[Type] = true;
		}
		public override bool SetWire(int x, int y, bool value) {
			Tile tile = Main.tile[x, y];
			if (tile.BlueWire != value) {
				tile.BlueWire = value;
				NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 10 + (!value).ToInt(), x, y);
				return true;
			}
			return false;
		}
		public override IEnumerable<WireMode> SortAfter() => [ModContent.GetInstance<Red_Wire_Mode>()];
	}
	public class Green_Wire_Mode : WireMode {
		public override string Texture => "Terraria/Images/UI/Wires_3";
		public override Color? WireKiteColor => Color.Lime;
		public override void SetupSets() {
			Sets.NormalWires[Type] = true;
		}
		public override bool SetWire(int x, int y, bool value) {
			Tile tile = Main.tile[x, y];
			if (tile.GreenWire != value) {
				tile.GreenWire = value;
				NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 12 + (!value).ToInt(), x, y);
				return true;
			}
			return false;
		}
		public override IEnumerable<WireMode> SortAfter() => [ModContent.GetInstance<Blue_Wire_Mode>()];
	}
	public class Yellow_Wire_Mode : WireMode {
		public override string Texture => "Terraria/Images/UI/Wires_5";
		public override Color? WireKiteColor => Color.Yellow;
		public override void SetupSets() {
			Sets.NormalWires[Type] = true;
		}
		public override bool SetWire(int x, int y, bool value) {
			Tile tile = Main.tile[x, y];
			if (tile.YellowWire != value) {
				tile.YellowWire = value;
				NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 16 + (!value).ToInt(), x, y);
				return true;
			}
			return false;
		}
		public override IEnumerable<WireMode> SortAfter() => [ModContent.GetInstance<Green_Wire_Mode>()];
	}
	public interface IWireTool {
		public IEnumerable<WireMode> Modes { get; }
	}
	[ReinitializeDuringResizeArrays]
	public class Wire_Mode_Kite : ItemModeFlowerMenu<WireMode, WirePetalData> {
		public static bool Cutter { get; set; }
		public static bool[] EnabledWires { get; } = WireMode.Sets.Factory.CreateBoolSet();
		public static IWireTool WireTool => Main.LocalPlayer.HeldItem.ModItem as IWireTool;
		public override bool IsActive() => WireTool is not null;
		public override float DrawCenter() {
			bool hovered = Main.MouseScreen.WithinRange(activationPosition, 20);
			int cutter = Cutter.ToInt();
			WireMode.GetTints(hovered, true, out Color backTint, out Color iconTint);
			WireMode.DrawIcon(TextureAssets.WireUi[hovered.ToInt() + cutter * 8].Value, activationPosition, backTint);
			WireMode.DrawIcon(TextureAssets.WireUi[6 + cutter].Value, activationPosition, iconTint);
			if (hovered) {
				Main.LocalPlayer.mouseInterface = true;
				if (Main.mouseLeft && Main.mouseLeftRelease) Cutter = !Cutter;
			}
			return 44;
		}
		public override WirePetalData GetData(WireMode mode) {
			WirePetalData data = 0;
			if (EnabledWires[mode.Type]) data |= WirePetalData.Enabled;
			if (Cutter) data |= WirePetalData.Cutter;
			return data;
		}
		public override void Click(WireMode mode) {
			EnabledWires[mode.Type] ^= true;
		}
		public override IEnumerable<WireMode> GetModes() => WireTool.Modes;
	}
}
