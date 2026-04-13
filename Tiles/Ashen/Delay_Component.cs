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
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.UI;
using Terraria.UI.Chat;
using static Origins.Items.Tools.Wiring.Logic_Gate_System;

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
			bool GetTE(out Delay_Component_TE tileEntity) => TileEntity.TryGet(new(Position), out tileEntity);
			public override Action Initialize(UIElement root) {
				root.Append(new UIImageFramed(ComponentUI.Textures, new(728, 2, 240, 150)));
				root.Append(new Delay_Element(new(Position)) {
					Left = new(2, 0),
					Top = new(0, 1)
				});
				return () => {};
			}
			public class Delay_Element(Point16 position) : UIImageFramed(ComponentUI.Textures, new(316, 202, 112, 24)) {
				readonly Time_Radix[] timeRadices = Time_Radix.ParseRadices(Language.GetOrRegister($"Mods.Origins.Items.{nameof(Delay_Component)}_Item.Time").Value);
				bool GetTE(out Delay_Component_TE tileEntity) => TileEntity.TryGet(position, out tileEntity);
				public override void OnInitialize() {
					if (!GetTE(out Delay_Component_TE tileEntity)) return;
					Append(ComponentUI.ReframingButton.Disableable(
						() => tileEntity.Delay <= Delay_Component_TE.increment,
						() => new Delay_Component_TE.Set_Delay_Action(position.X, position.Y, tileEntity.Delay - Delay_Component_TE.increment).Perform(),
						new(248, 180, 18, 18),
						new(248, 200, 18, 18)
					).MoveTo(new(4, 2)));
					Append(ComponentUI.ReframingButton.Disableable(
						() => tileEntity.Delay >= Delay_Component_TE.max,
						() => new Delay_Component_TE.Set_Delay_Action(position.X, position.Y, tileEntity.Delay + Delay_Component_TE.increment).Perform(),
						new(292, 180, 18, 18),
						new(292, 200, 18, 18)
					).MoveTo(new(22, 2)));
				}
				protected override void DrawSelf(SpriteBatch spriteBatch) {
					if (!GetTE(out Delay_Component_TE tileEntity)) return;
					base.DrawSelf(spriteBatch);
					DynamicSpriteFont font = FontAssets.ItemStack.Value;
					string time = timeRadices.FormatTime(tileEntity.Delay);
					Vector2 scale = Vector2.One * 0.75f;
					ChatManager.DrawColorCodedString(
						spriteBatch,
						font,
						time,
						GetDimensions().Position() + new Vector2(98, 12),
						new Color(255, 81, 0),
						0,
						(font.MeasureString(time) - Vector2.UnitY * 6) * scale,
						scale
					);
				}
			}
		}
	}
	public class Delay_Component_TE : ModTileEntity {
		private readonly int[] timer = new int[2];
		private bool targetPowered;
		public const int increment = 6;
		public const int max = increment * 10 * 10;
		public int Delay { get; private set; } = 60;
		public override void NetSend(BinaryWriter writer) {
			for (int i = 0; i < timer.Length; i++) writer.Write(timer[i]);
			writer.Write(Delay);
			writer.Write(targetPowered);
		}
		public override void NetReceive(BinaryReader reader) {
			for (int i = 0; i < timer.Length; i++) timer[i] = reader.ReadInt32();
			Delay = reader.ReadInt32();
			targetPowered = reader.ReadBoolean();
		}
		public override void Update() {
			if (!IsTileValidForEntity(Position.X, Position.Y)) {
				Kill(Position.X, Position.Y);
				return;
			}
			if (timer[0] > 0 && ++timer[0] > Delay) {
				timer[0] = 0;
				Ashen_Wire_Data.SetTilePowered(Position.X, Position.Y, targetPowered);
			}
			if (timer[1] > 0 && ++timer[1] > Delay) {
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
		public override void SaveData(TagCompound tag) => tag[nameof(Delay)] = Delay;
		public override void LoadData(TagCompound tag) => Delay = tag.SafeGet(nameof(Delay), Delay);
		public record class Set_Delay_Action(int I, int J, int Delay) : AutoSyncedAction {
			protected override bool ShouldPerform => TryGet<Delay_Component_TE>(I, J, out _);
			protected override void Perform() {
				if (!TryGet(I, J, out Delay_Component_TE te)) return;
				te.Delay = Delay;
			}
		}
	}
}
