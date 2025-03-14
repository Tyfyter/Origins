using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.Items.Other.Dyes;
using Origins.Tiles;
using PegasusLib;
using PegasusLib.Graphics;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Light;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Latchkey : ModItem {
		public string[] Categories => [
			"ExpendableTool"
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
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.shootSpeed = 4;
			Item.shoot = ModContent.ProjectileType<Latchkey_P>();
			Item.value = Item.sellPrice(copper: 20);
		}
		public override bool? UseItem(Player player) {
			// aren't these supposed to have this as a drawback?
			//player.GetAssimilation<Defiled_Assimilation>().Percent += 0.03f;
			return base.UseItem(player);
		}
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

			if (Projectile.ai[0] > 0) {
				if (--Projectile.ai[0] <= 0) Projectile.Kill();
			} else {
				Vector2 pos = Projectile.position;
				Rectangle hitbox = Projectile.Hitbox;
				for (int i = 0; i < 11 * 4; i++) {
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
						owner.velocity = Vector2.Zero;
						return;
					}
					if (shouldBreak) break;
				}
			}

			owner.Center = Projectile.Center;
			owner.velocity = Projectile.velocity;
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
	public class Latchkey_Visuals : ModSystem {
		internal RenderTarget2D renderTarget;
		internal RenderTarget2D oldRenderTarget;
		bool oldHadLatchkeys = false;
		public override void PostDrawTiles() {
			int type = ModContent.ProjectileType<Latchkey_P>();
			List<Projectile> latchkeys = [];
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				if (projectile.type != type) continue;
				latchkeys.Add(projectile);
			}
			if (latchkeys.Count > 0) {
				RenderTargetBinding[] oldRenderTargets = Main.graphics.GraphicsDevice.GetRenderTargets();
				Utils.Swap(ref renderTarget, ref oldRenderTarget);
				Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
				Main.graphics.GraphicsDevice.Clear(Color.Transparent);
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
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

				Main.spriteBatch.End();
				Main.graphics.GraphicsDevice.UseOldRenderTargets(oldRenderTargets);
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
				Main.spriteBatch.Draw(renderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
				DrawData data = new(renderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None);
				ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(Shimmer_Dye.ShaderID, null); // temp shader choice
				shader.Apply(Main.LocalPlayer, data);
				data.Draw(Main.spriteBatch);
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
		/*FastFieldInfo<LightingEngine, LightMap> _workingLightMap = new("_workingLightMap", BindingFlags.NonPublic);
		FastFieldInfo<LightingEngine, Rectangle> _workingProcessedArea = new("_workingProcessedArea", BindingFlags.NonPublic);
		FastStaticFieldInfo<Lighting, ILightingEngine> _activeEngine = new("_activeEngine", BindingFlags.NonPublic);
		public override void ModifyLightingBrightness(ref float scale) {
			if (_activeEngine.Value is LightingEngine lightingEngine) {
				Rectangle workingProcessedArea = _workingProcessedArea.GetValue(lightingEngine);
				LightMap lightMap = _workingLightMap.GetValue(lightingEngine);
				Point startPos = new(Player.tileTargetX - workingProcessedArea.X, Player.tileTargetY - workingProcessedArea.Y);
				for (int i = -10; i < 11; i++) {
					int x = i + startPos.X;
					if (x < 0 || x >= lightMap.Width) continue;
					for (int j = -10; j < 11; j++) {
						int y = j + startPos.Y;
						if (y < 0 || y >= lightMap.Height) continue;
						lightMap.SetMaskAt(x, y, LightMaskMode.Solid);
						lightMap[x, y] = Vector3.Zero;
					}
				}
			}
		}*/
	}
}
