using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Core;
using Origins.Dusts;
using Origins.Graphics;
using Origins.Graphics.Primitives;
using Origins.Items.Tools.Wiring;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Origins.Core.MultiTypeMultiTile;
using static Origins.Graphics.Primitives.VertexMultiTile;

namespace Origins.Tiles.Ashen; 
[ReinitializeDuringResizeArrays]
public class Incinerator_Pit : OriginTile, IComplexMineDamageTile, IMultiTypeMultiTile, IAshenTile {
	readonly Sound activeSound = EnvironmentSounds.Register<Sound>();
	static Color poweredColor = new Color(255, 113, 0);
	public static int MaxFuel => 60 * 60 * 1;
	public static int PlayerPowerTime => 60 * 30;
	public static int ItemPowerTime(Item item) => (30 + (int)MathF.Pow(8, MathF.Log(item.value, 10)) + item.rare * 180 + ItemBonusPowerTime[item.type]) * item.stack;
	public static int NPCPowerTime(NPC npc) => 60 * 10 + (int)MathF.Pow(8, MathF.Log(npc.value, 10)) * 2 + npc.lifeMax / 60;

	public static int[] ItemBonusPowerTime = ItemID.Sets.Factory.CreateNamedSet($"{nameof(Incinerator_Pit)}_{nameof(ItemBonusPowerTime)}").RegisterIntSet();
	public static bool?[] ItemCanBeDestroyedOveride = ItemID.Sets.Factory.CreateNamedSet($"{nameof(Incinerator_Pit)}_{nameof(ItemCanBeDestroyedOveride)}").RegisterCustomSet<bool?>(null);
	AutoLoadingTexture grinderTexture = typeof(Incinerator_Pit).GetDefaultTMLName("_Grinder");
	AutoLoadingTexture glowTexture = typeof(Incinerator_Pit).GetDefaultTMLName("_Glow");
	AutoLoadingTexture pitGlowTexture = typeof(Incinerator_Pit_Pit).GetDefaultTMLName("_Glow");
	public static int ID { get; private set; }
	public static ShapeMap Shape => field = field || new ShapeMap(
		new() {
			['X'] = (ushort)ModContent.TileType<Incinerator_Pit>(),
			['O'] = (ushort)ModContent.TileType<Incinerator_Pit_Pit>()
		},
		"X         XXX",
		"XOOO   OOOXXX",
		"XOOOOOOOOOXXX",
		"XOOOOOOOOOXXX",
		"XOOOOOOOOOXXX",
		"XOOOOOOOOOXXX",
		"XOOOOOOOOOXXX",
		"XXXXXXXXXXXXX"
	);
	public override void Load() {
		new TileItem(this, true).RegisterItem();
		On_TileDrawing.PostDrawTiles += On_TileDrawing_PostDrawTiles;
	}
	static void On_TileDrawing_PostDrawTiles(On_TileDrawing.orig_PostDrawTiles orig, TileDrawing self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets) {
		drawnPoints.Clear();
		orig(self, solidLayer, forRenderTargets, intoRenderTargets);
	}
	protected virtual Color MapColor => new Color(81, 44, 23);
	public override void SetStaticDefaults() {
		this.SetIDProp();
		// Properties
		TileID.Sets.CanBeSloped[Type] = false;
		Main.tileSolid[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = false;
		TileID.Sets.HasOutlines[Type] = false;
		TileID.Sets.DisableSmartCursor[Type] = true;
		TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;

		// Names
		AddMapEntry(MapColor, CreateMapEntryName());

		// Placement
		TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
		TileObjectData.newTile.Width = 13;
		TileObjectData.newTile.SetHeight(8);
		TileObjectData.newTile.SetOriginBottomCenter();
		TileObjectData.newTile.Direction = TileObjectDirection.None;
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.HookPlaceOverride = Shape.Place;
		TileObjectData.addTile(Type);
		HitSound = SoundID.Tink;
		DustType = Ashen_Biome.DefaultTileDust;
	}

	public void MinePower(int i, int j, int minePower, ref int damage) {
		if (minePower < 55) damage = 0;
	}
	static readonly HashSet<Point> drawnPoints = [];
	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
		Tile tile = Main.tile[i, j];
		i -= tile.TileFrameX / 18;
		j -= tile.TileFrameY / 18;
		if (drawnPoints.Add(new(i, j))) Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomSolid);
		return false;
	}
	public override void NearbyEffects(int i, int j, bool closer) {
		if (closer) return;
		activeSound.TrySetNearest(new(i * 16 + 8, j * 16 + 8));
	}
	class Sound : AEnvironmentSound {
		public override void UpdateSound(Vector2 position) {
			float distFactor = ((position - Main.Camera.Center) / new Vector2(84.5f * 16, 62 * 16)).Abs(out _).Max();
			distFactor = Utils.Remap(distFactor, 1, 0.75f, 0, 1);

			//SoundEngine.PlaySound(SoundID.Zombie70.WithPitch(2f).WithVolume(0.08f * distFactor), position);
			SoundEngine.PlaySound(SoundID.Item140.WithPitch(-1.25f).WithVolume(0.15f * distFactor), position);
			SoundEngine.PlaySound(SoundID.Item143.WithPitch(-1.25f).WithVolume(0.1f * distFactor), position);
			SoundEngine.PlaySound(SoundID.Item144.WithPitch(2f).WithVolume(0.06f * distFactor), position);
			//SoundEngine.PlaySound(SoundID.Item29.WithPitch(1f).WithVolume(0.2f * distFactor), position);
		}
	}
	public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
		verts.Draw(
			i,
			j,
			spriteBatch,
			new WirePulseLayer(),
			Lit(TextureAssets.Tile[Incinerator_Pit_Pit.ID].Value),
			Glow(pitGlowTexture),
			new GrinderLayer(grinderTexture),
			Lit(TextureAssets.Tile[ID].Value),
			Glow(glowTexture)
		);
	}
	readonly struct GrinderLayer(Texture2D grinderTexture) : ILayer {
		private static readonly VertexRectangle grinderRect = new();
		bool ILayer.UsesVertices => false;
		readonly void ILayer.Draw(VertexPositionColorTexture[] vertices, short[] dices, int i, int j, SpriteBatch spriteBatch) {
			const float speed = 0.3f;

			Vector2 offset = new Vector2(i, j) * 16 - Main.screenPosition;
			Main.graphics.GraphicsDevice.Textures[0] = grinderTexture;
			DrawGrinder(offset + new Vector2(46, 64), (float)Main.timeForVisualEffects * speed);
			DrawGrinder(offset + new Vector2(134, 64), (float)Main.timeForVisualEffects * -speed + MathHelper.PiOver2);
			static void DrawGrinder(Vector2 position, float rotMult) {
				grinderRect.Draw(
					position,
					Color.White,
					new(88, 84),
					rotMult,
					position
				);
			}
		}
	}
	readonly struct WirePulseLayer : ILayer {
		readonly void ILayer.Draw(VertexPositionColorTexture[] vertices, short[] dices, int i, int j, SpriteBatch spriteBatch) {
			bool isPowered = WiresUI.Settings.DrawWires && Main.tile[i, j].Get<Ashen_Wire_Data>().IsTilePowered;
			float pulse = isPowered ? Ashen_Wire_Data.pulse.Value * 0.8f : 0;
			for (int n = 0; n < vertices.Length; n++) vertices[n].Color = Color.Lerp(vertices[n].Color, poweredColor, pulse);
		}
	}
	public static PlayerDeathReason DeathReason(Player player) => PlayerDeathReason.ByCustomReason(TextUtils.LanguageTree.Find("Mods.Origins.DeathMessage.Incinerator_Pit").SelectFrom(player.name).ToNetworkText());
	public static void HurtEntity(Entity entity, Action<int> bounce, Func<bool, int> hurt) {
		if (entity is null || bounce is null || hurt is null) return;
		Rectangle hitbox = entity.Hitbox;
		foreach (Point pos in hitbox.IterateTilesIn(CollisionExtensions.TileOrder.DescY)) {
			Tile tile = Main.tile[pos];
			if (tile.TileType != Incinerator_Pit_Pit.ID) continue;
			Point16 topLeft = new(pos.X - tile.TileFrameX / 18, pos.Y - tile.TileFrameY / 18);
			int sparks = ModContent.DustType<Spark_Dust>();
			if (tile.TileFrameY <= 18) {
				int dir = (tile.TileFrameX < 18 * 4).ToDirectionInt();
				bounce(dir);
				Rectangle dustRect = Rectangle.Intersect(hitbox, new(pos.X * 16, pos.Y * 16, 16, 16));
				for (int i = 0; i < 4; i++) {
					SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(2.2f).WithVolume(0.05f)/*, center*/);
					SoundEngine.PlaySound(SoundID.Item146.WithPitch(1.5f).WithVolume(0.05f)/*, center*/);
					Dust dust = EfficientDust.NewDustDirect(
						dustRect.TopLeft(),
						dustRect.Width,
						dustRect.Height,
						sparks
					);
					dust.velocity.X += dir * 4;
					dust.velocity += Main.rand.NextVector2Circular(2, 2);
					dust.noGravity = Main.rand.NextBool(2, 3);
				}
			} else {
				if (entity.velocity.Y < 0 || tile.TileFrameY <= 18 * 3) entity.velocity.Y += 4;
				if (tile.TileFrameY >= 18 * 3) {
					Vector2 effectPos = pos.ToWorldCoordinates();
					for (int i = (Main.tile[pos].TileFrameX / 18 - 5) * 4; i != 0; i -= Math.Sign(i)) {
						Vector2 mov = new(Math.Sign(i) * 4, 0);
						if (!hitbox.Contains(effectPos + mov)) break;
						effectPos += mov;
					}
					if (entity.velocity.Y >= 0 && Main.tile[entity.Center.ToTileCoordinates()].TileFrameY >= 18 * 4) {
						entity.velocity.Y *= -0.11f;
					}
					entity.velocity.X *= 0.8f;
					entity.velocity.X -= Math.Sign(Main.tile[(int)entity.Center.X / 16, pos.Y].TileFrameX - 5 * 18);
					if (Main.tile[topLeft].LoopSoundDelay(1)) {
						SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(2.2f).WithVolume(0.05f), effectPos);
						if (Main.rand.NextBool(8)) SoundEngine.PlaySound(Origins.Sounds.SmallSawStart.WithVolume(0.05f), effectPos);
						if (Main.rand.NextBool(10)) SoundEngine.PlaySound(SoundID.Item113.WithVolume(0.05f), effectPos);
					}
					if (Main.tile[effectPos.ToTileCoordinates()].LoopSoundDelay(1)) {
						Rectangle dustRect = new Rectangle(0, 0, 16, 16).Recentered(effectPos);
						for (int i = 0; i < 4; i++) {
							Dust dust = EfficientDust.NewDustDirect(
								dustRect.TopLeft(),
								dustRect.Width,
								dustRect.Height,
								sparks
							);
							if (Main.rand.NextBool(10)) {
								dust.velocity.Y -= 2 + Main.rand.NextFloat(1);
								dust.velocity.X *= 0.25f;
							} else {
								dust.velocity.Y -= 4 + Main.rand.NextFloat(2);
								dust.velocity.X *= 0.5f;
								dust.velocity *= 1.5f;
								dust.fadeIn = 1;
								dust.noGravity = true;
							}
						}
					}
				}
				int power = hurt(tile.TileFrameY >= 18 * 3);
				if (power > 0) Incinerator_Pit_TE.GetData(topLeft).Fuel += power;
			}
			break;
		}
	}
	bool IMultiTypeMultiTile.IsValidTile(Tile tile, int left, int top, int style) => Shape.Matches(tile, left, top, style);
	readonly VertexMultiTile verts = new(14, 9);
	public override void PlaceInWorld(int i, int j, Item item) {
		TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out int left, out int top);
		ModContent.GetInstance<Incinerator_Pit_TE>().AddTileEntity(new(left, top), new());
	}
	class Incinerator_Pit_TE : TESystem<Incinerator_Pit_TE.Data> {
		public static Data GetData(Point16 position) {
			ModContent.GetInstance<Incinerator_Pit_TE>().tileEntities.TryGetValue(position, out Data data);
			return data;
		}
		protected override bool IsValidTile(Tile tile) => tile.HasTile && (tile.TileType == ID || tile.TileType == Incinerator_Pit_Pit.ID);
		public class Data() : ITileEntityData {
			int fuel;
			public int Fuel {
				get => fuel;
				set => IsDirty |= fuel.TrySet(Math.Min(value, MaxFuel));
			}
			public void Update(Point16 position) {
				bool shouldGenerate = fuel > 0;
				if (Main.tile[position].Get<Ashen_Wire_Data>().IsTilePowered == shouldGenerate) goto consume;
				TileObjectData tileData = TileObjectData.GetTileData(Main.tile[position]);
				TileUtils.GetMultiTileTopLeft(position.X, position.Y, tileData, out int left, out int top);
				for (int j = 0; j < tileData.Height; j++) {
					for (int i = 0; i < tileData.Width; i++) {
						if (!Shape.Matches(Main.tile[left + i, top + j], left, top, 0)) continue;
						Ashen_Wire_Data.SetTilePowered(left + i, top + j, shouldGenerate);
					}
				}
				consume:
				if (fuel > 0) fuel--;
			}

			void ITileEntityData.SaveTE(TagCompound tag) {
				tag[nameof(fuel)] = fuel;
			}
			static Data ITileEntityData.LoadTE(TagCompound tag) {
				Data data = new();
				tag.TryGet(nameof(fuel), out data.fuel);
				return data;
			}
			void ITileEntityData.NetSend(BinaryWriter writer) {
				writer.Write(fuel);
			}
			static Data ITileEntityData.NetReceive(BinaryReader reader, Data existing) {
				existing ??= new Data();
				existing.fuel = reader.ReadInt32();
				return existing;
			}
			public bool IsDirty { get; set; }
		}
	}
	class GrindNPCs : GlobalNPC {
		int immuneTime;
		public override bool InstancePerEntity => true;
		public override void FindFrame(NPC npc, int frameHeight) {
			immuneTime.Cooldown();
			if (npc.noTileCollide || npc.dontTakeDamage) return;
			HurtEntity(npc,
				dir => {
					if (immuneTime > 0) return;
					npc.SimpleStrikeNPC(50, dir, knockBack: 4.5f);
					immuneTime = 6;
				},
				downInThere => {
					if (immuneTime > 0) return npc.active ? 0 : NPCPowerTime(npc);
					npc.SimpleStrikeNPC(100, 0, knockBack: 0);
					immuneTime = 6;
					return npc.active ? 0 : NPCPowerTime(npc);
				}
			);
		}
	}
	public class GrindItems : GlobalItem, IDrawItemInWorldEffect {
		public int noPickupTime;
		int grindTime;
		int grindDamage;
		int bounceCount;
		public override bool InstancePerEntity => true;
		public override void Load() {
			try {
				IL_Item.MoveInWorld += ReduceFriction;
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(ReduceFriction), e)) throw;
			}
		}
		static void ReduceFriction(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.After,
				i => i.MatchLdarg(2),
				i => i.MatchStfld(out _)
			);
			c.GotoNext(MoveType.After, i => i.MatchLdcR4(0.95f));
			c.EmitLdarg0();
			c.EmitDelegate(static (float friction, Item item) => {
				if (!item.IsAir && item.GetGlobalItem<GrindItems>().noPickupTime > 0) Max(ref friction, 0.99f);
				return friction;
			});
		}
		public override void PostUpdate(Item item) {
			noPickupTime.Cooldown();
			grindTime.Cooldown();
			if (item.beingGrabbed) return;
			HurtEntity(item,
				dir => {
					item.velocity = OriginExtensions.GetKnockback(4.5f, hitDirection: dir, yMult: -0.35f + bounceCount * 0.025f);
					noPickupTime = 35;
					bounceCount++;
				},
				downInThere => {
					if (downInThere) {
						item.velocity.Y = 0;
						grindTime = 3;
					}
					noPickupTime = 5;
					if (item.expert || item.master) return 0;
					if (ItemCanBeDestroyedOveride[item.type] ?? (item.rare is ItemRarityID.Gray or ItemRarityID.White or ItemRarityID.Blue)) {
						if (++grindDamage >= 150 + float.Pow(item.value, 0.35f) * 10f + Math.Max(item.rare * 150, 0)) {
							item.active = false;
							return ItemPowerTime(item);
						}
					}
					return 0;
				}
			);
		}
		public override void Update(Item item, ref float gravity, ref float maxFallSpeed) {
			if (noPickupTime > 0) {
				gravity *= 1 + bounceCount * 0.05f;
				maxFallSpeed *= 2;
			} else {
				bounceCount = 0;
				grindDamage = 0;
			}
		}
		public override bool GrabStyle(Item item, Player player) {
			if (noPickupTime > 0 && !item.Hitbox.Intersects(player.Hitbox)) {
				item.beingGrabbed = false;
				return true;
			}
			return base.GrabStyle(item, player);
		}
		Vector2 drawOffset;
		public void PrepareToDrawItemInWorld(Item item) {
			if (grindTime > 0) item.position += drawOffset = Main.rand.NextVector2Circular(4, 4);
			else drawOffset = default;
		}
		public void FinishDrawingItemInWorld(Item item) {
			item.position -= drawOffset;
		}
	}
}
public class Incinerator_Pit_Pit : Incinerator_Pit, IGlowingModTile {
	public new static int ID { get; private set; }
	protected override Color MapColor => new Color(255, 81, 0);
	public override void Load() { }
	public void FancyLightingGlowColor(Tile tile, int x, int y, ref Vector3 color) {
		if (ShouldGlow(tile)) color.DoFancyGlow(new(0.912f, 0.579f, 0f), tile.TileColor);
	}
	public override void SetStaticDefaults() {
		base.SetStaticDefaults();
		Main.tileSolid[Type] = false;
		Main.tileLighted[Type] = true;
	}
	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
		if (ShouldGlow(Main.tile[i, j])) {
			r = 0.912f;
			g = 0.579f;
			b = 0f;
		}
	}
	public static bool ShouldGlow(Tile tile) => Shape[tile.TileFrameX / 18, tile.TileFrameY / 18 + 1, 0] == 'X';
	CustomTilePaintLoader.CustomTileVariationKey IGlowingModTile.GlowPaintKey { get; set; }
	AutoCastingAsset<Texture2D> IGlowingModTile.GlowTexture { get; }
	Color IGlowingModTile.GlowColor => new(0.912f, 0.579f, 0f);
}
