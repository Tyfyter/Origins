using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Other.Dyes;
using Origins.NPCs.Defiled.Boss;
using Origins.Tiles;
using PegasusLib.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Latchkey : ModItem {
		public string[] Categories => [
			WikiCategories.ExpendableTool
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.HiddenAnimation;
			Item.rare = ItemRarityID.Green;
			Item.width = 18;
			Item.height = 20;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.UseSound = Origins.Sounds.PowerUp.WithVolume(0.75f);
			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.mana = 8;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.shootSpeed = 4;
			Item.shoot = ModContent.ProjectileType<Latchkey_P>();
			Item.value = Item.sellPrice(copper: 20);
		}
		public override bool? UseItem(Player player) {
			player.AddBuff(ModContent.BuffType<Mana_Buffer_Debuff>(), 90);
			return base.UseItem(player);
		}
	}
	public class Latchkey_Jump_Refresh : ExtraJump {
		public override Position GetDefaultPosition() => BeforeBottleJumps;
		public override bool CanStart(Player player) => false;
		public override float GetDurationMultiplier(Player player) => 0;
	}
	public class Latchkey_P : ModProjectile {
		public const int max_updates = 4;
		public override string Texture => typeof(Refactoring_Pieces).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.DD2SquireSonicBoom);
			Projectile.width = 20;
			Projectile.height = 42;
			Projectile.alpha = 0;
			Projectile.MaxUpdates = max_updates;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 10 * max_updates;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			Player owner = Main.player[Projectile.owner];

			owner.velocity = Vector2.UnitY * -0.01f;
			ref ExtraJumpState jumpState = ref owner.GetJumpState<Latchkey_Jump_Refresh>();
			if (jumpState.Available) {
				owner.RefreshMovementAbilities();
				jumpState.Available = false;
			}

			if (Projectile.ai[0] > 0) {
				Projectile.timeLeft = 2;
				if (--Projectile.ai[0] <= 0) Projectile.Kill();
			} else {
				Vector2 pos = Projectile.position;
				Rectangle hitbox = Projectile.Hitbox;
				const int length = 16 * 4;
				int i = 0;
				bool didBreak = false;
				for (; i < length; i++) {
					pos += Projectile.velocity;
					hitbox.X = (int)pos.X;
					hitbox.Y = (int)pos.Y;
					bool shouldBreak = false;
					switch (OverlapsAnyTiles(hitbox)) {
						case 0:
						if (i == 0 || Projectile.ai[0] != 0) {
							shouldBreak = true;
						} else {
							Projectile.ai[0] = i;
						}
						break;
						case 2:
						Projectile.Kill();
						return;
					}
					if (shouldBreak) {
						didBreak = true;
						break;
					}
				}
				if (!didBreak) {
					Projectile.Kill();
				}
			}

			owner.Center = Projectile.Center;
			owner.velocity += Projectile.velocity;
			if (++Projectile.frameCounter >= 14) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
			}
		}
		public override void OnKill(int timeLeft) {
			// dusts and/or gores here
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			//hitbox.Inflate(4, 12);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Player owner = Main.player[Projectile.owner];
			owner.Center = Projectile.Center;
			owner.velocity = Projectile.velocity;

			return false;
		}
		public override bool PreDraw(ref Color lightColor) {

			return false;
		}
		static int OverlapsAnyTiles(Rectangle area) {
			Rectangle checkArea = area;
			Point topLeft = area.TopLeft().ToTileCoordinates();
			Point bottomRight = area.BottomRight().ToTileCoordinates();
			int minX = Utils.Clamp(topLeft.X, 0, Main.maxTilesX - 1);
			int minY = Utils.Clamp(topLeft.Y, 0, Main.maxTilesY - 1);
			int maxX = Utils.Clamp(bottomRight.X, 0, Main.maxTilesX - 1) - minX;
			int maxY = Utils.Clamp(bottomRight.Y, 0, Main.maxTilesY - 1) - minY;
			int cornerX = area.X - topLeft.X * 16;
			int cornerY = area.Y - topLeft.Y * 16;
			int highestValue = 0;
			for (int i = 0; i <= maxX; i++) {
				for (int j = 0; j <= maxY; j++) {
					Tile tile = Main.tile[i + minX, j + minY];
					if (tile != null && tile.HasFullSolidTile()) {
						checkArea.X = i * -16 + cornerX;
						checkArea.Y = j * -16 + cornerY;
						int value = TileLoader.GetTile(tile.TileType) is IDefiledTile ? 1 : 2;
						if (highestValue >= value) continue;
						if (tile.Slope != SlopeType.Solid) {
							if (CollisionExtensions.TileTriangles[(int)tile.Slope - 1].Intersects(checkArea)) highestValue = value;
						} else {
							if (CollisionExtensions.TileRectangles[(int)tile.BlockType].Intersects(checkArea)) highestValue = value;
						}
					}
				}
			}
			return highestValue;
		}
	}
	public class Latchkey_Visuals : ModSystem, ITangelaHaver {
		internal RenderTarget2D renderTarget;
		internal RenderTarget2D oldRenderTarget;
		bool oldHadLatchkeys = false;
		public int? TangelaSeed { get; set; }
		public override void PostDrawTiles() {
			int projType = ModContent.ProjectileType<Latchkey_P>();
			List<Projectile> latchkeys = [];
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				if (projectile.type != projType) continue;
				latchkeys.Add(projectile);
			}

			int amalgamationType = ModContent.NPCType<Defiled_Amalgamation>();
			List<NPC> amalgamations = [];
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.type != amalgamationType) continue;
				int difficultyMult = Defiled_Amalgamation.DifficultyMult;
				switch ((int)npc.ai[0]) {
					case Defiled_Amalgamation.state_sidestep_dash:
					amalgamations.Add(npc);
					break;
					case Defiled_Amalgamation.state_single_dash:
					if (npc.ai[1] >= 30 - difficultyMult * 5) amalgamations.Add(npc);
					break;
					case Defiled_Amalgamation.state_triple_dash:
					int cycleLength = 100 - (difficultyMult * 4);
					int dashLength = 60 - (difficultyMult * 2);
					int activeLength = cycleLength * 2 + dashLength;
					if (npc.ai[1] < activeLength && npc.ai[1] % cycleLength >= 10 - (difficultyMult * 3) && npc.ai[1] % cycleLength <= dashLength) amalgamations.Add(npc);
					break;
				}
			}
			if (latchkeys.Count > 0 || amalgamations.Count > 0) {
				RenderTargetBinding[] oldRenderTargets = Main.graphics.GraphicsDevice.GetRenderTargets();
				Utils.Swap(ref renderTarget, ref oldRenderTarget);
				Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
				Main.graphics.GraphicsDevice.Clear(Color.Transparent);

				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);
				if (oldHadLatchkeys) Main.spriteBatch.Draw(oldRenderTarget, Main.screenLastPosition - Main.screenPosition, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);

				for (int i = 0; i < latchkeys.Count; i++) {
					Player player = Main.player[latchkeys[i].owner];
					Main.PlayerRenderer.DrawPlayer(
						Main.Camera,
						player,
						player.position + new Vector2(0, player.gfxOffY),
						player.fullRotation,
						player.fullRotationOrigin,
						scale: 1
					);
				}
				for (int i = 0; i < amalgamations.Count; i++) {
					Main.instance.DrawNPCDirect(Main.spriteBatch, amalgamations[i], false, Main.screenPosition);
				}

				Main.spriteBatch.End();
				Main.graphics.GraphicsDevice.UseOldRenderTargets(oldRenderTargets);

				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

				TangelaVisual.DrawTangela(this, renderTarget, Vector2.Zero, null, 0, Vector2.Zero, Vector2.One, SpriteEffects.None);
				Main.spriteBatch.End();

				oldHadLatchkeys = true;
			} else {
				oldHadLatchkeys = false;
			}
		}
		public override void Load() {
			if (Main.dedServ) return;
			Main.QueueMainThreadAction(SetupRenderTargets);
			Main.OnResolutionChanged += Resize;
		}
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			renderTarget.Dispose();
			oldRenderTarget.Dispose();
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (renderTarget is not null && !renderTarget.IsDisposed) return;
			renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			oldRenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
		public override void Unload() {
			Main.QueueMainThreadAction(() => {
				renderTarget.Dispose();
				oldRenderTarget.Dispose();
			});
			Main.OnResolutionChanged -= Resize;
		}
	}
}
