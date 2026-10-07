using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Other.Consumables;
using Origins.Items.Tools.Wiring;
using Origins.Journal;
using Origins.World.BiomeData;
using PegasusLib.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Origins.Items.Tools.Wiring.Ashen_Wire_Data;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	[ReinitializeDuringResizeArrays]
	public abstract class Mechanical_Key_Node : ModTile, IAshenPowerConduitTile, IGlowingModTile, IAshenWireTile {
		public static byte[] OverrideClearance = TileID.Sets.Factory.CreateCustomSet<byte>(0);
		public static int NPCOverrideRange => 8;
		public Mechanical_Key_Node_Item Item { get; private set; }
		public abstract int KeyType { get; }
		public abstract byte NPCAccessLevel { get; }
		public override string HighlightTexture => typeof(Mechanical_Key_Node).GetDefaultTMLName("_Highlight");
		public virtual Color SwitchColor => FromHexRGB(0x7a391a);
		public virtual Color MapColor => SwitchColor;
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		public sealed override void Load() {
			Mod.AddContent(Item = new(this));
			this.SetupGlowKeys();
		}
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public void FancyLightingGlowColor(Tile tile, int x, int y, ref Vector3 color) {
			if (tile.TileFrameX >= 18) {
				color.DoFancyGlow(SwitchColor.ToVector3(), tile.TileColor);
				color.DoFancyGlow(tile.TileFrameY >= 18 ? Vector3.Up : Vector3.Right, tile.TileColor);
			}
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			}
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileBlockLight[Type] = false;
			Main.tileMergeDirt[Type] = false;
			TileID.Sets.DrawTileInSolidLayer[Type] = true;
			TileID.Sets.CanPlaceNextToNonSolidTile[Type] = true;
			AddMapEntry(MapColor, CreateMapEntryName());

			MinPick = 65;
			MineResist = 2;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
			TileID.Sets.HasOutlines[Type] = true;
			OverrideClearance[Type] = NPCAccessLevel;
			RegisterItemDrop(Item.Type);
		}
		public override void HitWire(int i, int j) {
			if (Ashen_Wire_Data.HittingAshenWires) UpdatePowerState(i, j, IsPowered(i, j));
			else if (propagatingToggleStates.Count > 0 && !propagatingToggleStates.Contains(new(i, j))) {
				Tile progenitor = Main.tile[propagatingToggleStates.First()];
				if (Type == progenitor.TileType) new Mechanical_Switch_Action(new(i, j), progenitor.TileFrameY != 0).Perform();
			}
		}
		public bool IsPowered(int i, int j) {
			Tile tile = Main.tile[i, j];
			Point pos = new(i, j);
			bool inputPower = false;
			if (tile.Get<Ashen_Wire_Data>().AnyPower) {
				using IAshenPowerConduitTile.WalkedConduitOutput _ = new(pos);
				inputPower = IAshenPowerConduitTile.FindValidPowerSource(pos, 0)
						|| IAshenPowerConduitTile.FindValidPowerSource(pos, 1)
						|| IAshenPowerConduitTile.FindValidPowerSource(pos, 2);
			}
			return inputPower;
		}
		public void UpdatePowerState(int i, int j, bool powered) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX.TrySet(powered.Mul<short>(18))) {
				if (tile.TileFrameY != 0) Ashen_Wire_Data.SetTilePowered(i, j, powered);
				NetMessage.SendData(MessageID.TileSquare, Main.myPlayer, -1, null, i, j, 1, 1);
			}
		}
		public override bool CanExplode(int i, int j) => false;
		/*public override bool CanKillTile(int i, int j, ref bool blockDamaged) {
			Tile tile = Main.tile[i, j];
			return tile.TileFrameX == 0 || tile.TileFrameY == 0;
		}*/
		public bool HasKey(Player player) => player.HasItemInInventoryOrOpenVoidBag(KeyType);
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => HasKey(settings.player);
		public override bool RightClick(int i, int j) {
			if (!HasKey(Main.LocalPlayer)) return false;
			SoundEngine.PlaySound(SoundID.Unlock.WithPitch(-0.5f));
			new Mechanical_Switch_Action(new(i, j), Main.tile[i, j].TileFrameY == 0).Perform();
			return true;
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowColor = GlowColor;
			drawData.glowSourceRect = new(drawData.tileFrameX, drawData.tileFrameY, 16, 16);
			drawData.glowTexture = this.GetGlowTexture(drawData.tileCache.TileColor);
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			new Sync_Mechanical_Switch_Action(i, j).Perform();
		}
		static HashSet<Point16> propagatingToggleStates = [];
		public record class Mechanical_Switch_Action(Point16 Pos, bool On) : AutoSyncedAction {
			public override bool ServerOnly => true;
			protected override bool ShouldPerform {
				get {
					Tile tile = Main.tile[Pos];
					return TileLoader.GetTile(tile.TileType) is Mechanical_Key_Node && tile.TileFrameY != On.Mul<short>(18);
				}
			}
			public Mechanical_Switch_Action() : this(default, default) { }
			protected override void Perform() {
				Tile tile = Main.tile[Pos];
				if (TileLoader.GetTile(tile.TileType) is Mechanical_Key_Node) {
					tile.TileFrameY = On.Mul<short>(18);
					using (HittingWiresOverride _0 = new(false)) {
						using WireOverride _1 = WireOverride.New;
						try {
							propagatingToggleStates.Add(Pos);
							Wiring.TripWire(Pos.X, Pos.Y, 1, 1);
						} finally {
							propagatingToggleStates.Remove(Pos);
						}
					}
					bool inputPower = tile.TileFrameX != 0 && tile.TileFrameY != 0;
					using (IAshenPowerConduitTile.WalkedConduitOutput _ = new(Pos.ToPoint())) {
						if (inputPower) inputPower = IAshenPowerConduitTile.FindValidPowerSource(Pos.ToPoint(), 0)
								|| IAshenPowerConduitTile.FindValidPowerSource(Pos.ToPoint(), 1)
								|| IAshenPowerConduitTile.FindValidPowerSource(Pos.ToPoint(), 2);
					}
					Ashen_Wire_Data.SetTilePowered(Pos.X, Pos.Y, inputPower);
					NetMessage.SendData(MessageID.TileSquare, Main.myPlayer, -1, null, Pos.X, Pos.Y, 1, 1);
				}
			}
			ref struct WireOverride {
				ScopedOverride<bool> running;
				ScopedOverride<DoubleStack<Point16>> _wireList;
				ScopedOverride<DoubleStack<byte>> _wireDirectionList;
				ScopedOverride<Dictionary<Point16, byte>> _toProcess;
				ScopedOverride<Queue<Point16>> _LampsToCheck;
				ScopedOverride<Queue<Point16>> _GatesNext;
				ScopedOverride<Vector2[]> _teleport;
				ScopedOverride<int[]> _inPumpX;
				ScopedOverride<int[]> _inPumpY;
				ScopedOverride<int> _numInPump;
				ScopedOverride<int[]> _outPumpX;
				ScopedOverride<int[]> _outPumpY;
				ScopedOverride<int> _numOutPump;
				ScopedOverride<int> _currentWireColor;
				public static WireOverride New => new() {
					running = new(ref Wiring.running, false),
					_wireList = new(ref Wiring._wireList, new()),
					_wireDirectionList = new(ref Wiring._wireDirectionList, new()),
					_toProcess = new(ref Wiring._toProcess, new()),
					_LampsToCheck = new(ref Wiring._LampsToCheck, new()),
					_GatesNext = new(ref Wiring._GatesNext, new()),
					_teleport = new(ref Wiring._teleport, new Vector2[2]),
					_inPumpX = new(ref Wiring._inPumpX, new int[20]),
					_inPumpY = new(ref Wiring._inPumpY, new int[20]),
					_numInPump = new(ref Wiring._numInPump, 0),
					_outPumpX = new(ref Wiring._outPumpX, new int[20]),
					_outPumpY = new(ref Wiring._outPumpY, new int[20]),
					_numOutPump = new(ref Wiring._numOutPump, 0),
					_currentWireColor = new(ref Wiring._currentWireColor, 0)
				};
				public void Dispose() {
					running.Dispose();
					_wireList.Dispose();
					_wireDirectionList.Dispose();
					_toProcess.Dispose();
					_LampsToCheck.Dispose();
					_GatesNext.Dispose();
					_teleport.Dispose();
					_inPumpX.Dispose();
					_inPumpY.Dispose();
					_numInPump.Dispose();
					_outPumpX.Dispose();
					_outPumpY.Dispose();
					_numOutPump.Dispose();
					_currentWireColor.Dispose();
				}
			}
		}
		public record class Sync_Mechanical_Switch_Action(int I, int J) : SyncedAction {
			public override bool ServerOnly => true;
			public Sync_Mechanical_Switch_Action() : this(0, 0) { }
			public override SyncedAction NetReceive(BinaryReader reader) => this with {
				I = reader.ReadInt16(),
				J = reader.ReadInt16(),
			};
			public override void NetSend(BinaryWriter writer) {
				writer.Write((short)I);
				writer.Write((short)J);
			}
			protected override void Perform() {
				if (TileLoader.GetTile(Main.tile[I, J].TileType) is Mechanical_Key_Node @switch) {
					@switch.HitWire(I, J);
				}
			}
		}
		public bool ShouldCountAsPowerSource(Point position, int forWireType) {
			using IAshenPowerConduitTile.WalkedConduitOutput _ = new(position);
			bool powered = false;
			for (int i = 0; !powered && i < 3; i++) {
				powered = i != forWireType && IAshenPowerConduitTile.FindValidPowerSource(position, i);
			}
			return powered;
		}

		public class Mechanical_Key_Node_Item(Mechanical_Key_Node tile) : ModItem {
			static AutoLoadingAsset<Texture2D> overlay = typeof(Mechanical_Key_Node_Item).GetDefaultTMLName("_Color");
			public override string Name => tile.Name + "_Item";
			public override string Texture => GetType().GetDefaultTMLName();
			protected override bool CloneNewInstances => true;
			public override void SetStaticDefaults() {
				Item.ResearchUnlockCount = 100;
				SprockeyEntry.AddEntryOnUse<Mechanical_Key_Node_Entry>(this);
			}
			public override void SetDefaults() {
				Item.DefaultToPlaceableTile(tile.Type);
				Item.mech = true;
			}
			public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
				spriteBatch.Draw(overlay, position, frame, tile.SwitchColor.MultiplyRGBA(drawColor), 0, origin, scale, SpriteEffects.None, 0);
			}
			public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
				Vector2 origin = overlay.Value.Size() * 0.5f;
				Rectangle originalFrame;
				{
					Texture2D texture = TextureAssets.Item[Type].Value;
					if (Main.itemAnimations[Type] != null) {
						originalFrame = Main.itemAnimations[Type].GetFrame(texture, Main.itemFrameCounter[whoAmI]);
					} else {
						originalFrame = texture.Frame();
					}
				}
				alphaColor = alphaColor.MultiplyRGBA(tile.SwitchColor);
				Vector2 vector2 = new((Item.width / 2) - originalFrame.Width * 0.5f, Item.height - originalFrame.Height);
				Vector2 position = Item.position - Main.screenPosition + originalFrame.Size() * 0.5f + vector2;
				spriteBatch.Draw(overlay, position, null, alphaColor, rotation, origin, scale, SpriteEffects.None, 0f);
				if (Item.shimmered) {
					spriteBatch.Draw(overlay, position, null, alphaColor with { A = 0 }, rotation, origin, scale, SpriteEffects.None, 0f);
				}
			}
		}

		public static MultiDictionary<Point16, Entity> usedByNPCs = [];
		const float inv_sqrt_2 = 1f / 1.4142135f;
		public static void AddNPCUse(Point16 pos, Entity npc) {
			bool alreadyOverridden = false;
			if (usedByNPCs.TryGetValue(pos, out IEnumerable<Entity> alreadyOverriding)) {
				if (alreadyOverriding.Contains(npc)) return;
				alreadyOverridden = alreadyOverriding.Any();
			}
			if (!alreadyOverridden && Main.tile[pos].TileFrameY == 0) return;
			usedByNPCs.Add(pos, npc);
			new Mechanical_Switch_Action(pos, false).Perform();
		}
		public static void DoNPCUse(Entity npc, int maxClearance = 2) {
			if (NetmodeActive.MultiplayerClient) return;
			int x = (int)(npc.Center.X / 16);
			int y = (int)(npc.Center.Y / 16);
			int overrideRange = NPCOverrideRange - 1;
			Tile tile;
			for (int j = 0; j < overrideRange * inv_sqrt_2; j++) {
				for (int i = j; i < overrideRange; i++) {
					int xPos = x + i * npc.direction;
					if (i * i + j * j <= overrideRange * overrideRange && WorldGen.InWorld(xPos, y + j)) {
						tile = Main.tile[xPos, y + j];
						if (tile.HasTile && OverrideClearance[tile.TileType] > 0 && OverrideClearance[tile.TileType] <= maxClearance) AddNPCUse(new(xPos, y + j), npc);
					}
					if (j != 0 && i * i + j * j <= overrideRange * overrideRange && WorldGen.InWorld(xPos, y - j)) {
						tile = Main.tile[xPos, y - j];
						if (tile.HasTile && OverrideClearance[tile.TileType] > 0 && OverrideClearance[tile.TileType] <= maxClearance) AddNPCUse(new(xPos, y - j), npc);
					}
				}
			}
		}
		static readonly Stack<Point16> removePositions = new();
		internal static void UpdateNPCUse() {
			foreach ((Point16 pos, IEnumerable<Entity> _npcs) in usedByNPCs.Keys.Select(k => (k, usedByNPCs[k]))) {
				Vector2 worldPosition = pos.ToWorldCoordinates();
				List<Entity> npcs = (List<Entity>)_npcs;
				for (int i = npcs.Count - 1; i >= 0; i--) {
					if (!npcs[i].active || !npcs[i].WithinRange(worldPosition, NPCOverrideRange * 16)) npcs.RemoveAt(i);
				}
				if (npcs.Count <= 0) removePositions.Push(pos);
			}
			while (removePositions.TryPop(out Point16 pos)) {
				usedByNPCs.Remove(pos);
				new Mechanical_Switch_Action(pos, true).Perform();
			}
		}
		internal static void SaveOverrides(TagCompound tag) => tag["MKNOverrides"] = usedByNPCs.Keys.ToList();
		internal static void LoadOverrides(TagCompound tag) {
			if (tag.TryGet("MKNOverrides", out List<Point16> positions)) {
				foreach (Point16 pos in positions) usedByNPCs.Add(pos, []);
			}
		}
		internal static void ClearOverrides() => usedByNPCs.Clear();
	}
	[LegacyName("Purple_Mechanical_Switch", "Purple_Mechanical_Key_Node")]
	public class Mechanical_Key_Node_Purple : Mechanical_Key_Node {
		public override Color SwitchColor => new Color(109, 10, 145);
		public override int KeyType => ItemType<Mechanical_Key_Purple>();
		public override byte NPCAccessLevel => 1;
	}
	[LegacyName("Blue_Mechanical_Key_Node")]
	public class Mechanical_Key_Node_Blue : Mechanical_Key_Node {
		public override Color SwitchColor => new Color(0, 80, 240);
		public override int KeyType => ItemType<Mechanical_Key_Blue>();
		public override byte NPCAccessLevel => 2;
	}
	[LegacyName("Green_Mechanical_Key_Node")]
	public class Mechanical_Key_Node_Green : Mechanical_Key_Node {
		public override Color SwitchColor => new Color(16, 240, 0);
		public override int KeyType => ItemType<Mechanical_Key_Green>();
		public override byte NPCAccessLevel => 3;
	}
	[LegacyName("Yellow_Mechanical_Key_Node")]
	public class Mechanical_Key_Node_Yellow : Mechanical_Key_Node {
		public override Color SwitchColor => new Color(255, 179, 0);
		public override int KeyType => ItemType<Mechanical_Key_Yellow>();
		public override byte NPCAccessLevel => 4;
	}
	[LegacyName("Orange_Mechanical_Key_Node")]
	public class Mechanical_Key_Node_Orange : Mechanical_Key_Node {
		public override Color SwitchColor => new Color(255, 81, 0);
		public override int KeyType => ItemType<Mechanical_Key_Orange>();
		public override byte NPCAccessLevel => 5;
	}
}
