using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Projectiles;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using PegasusLib;
using System;
using Terraria.Audio;
using System.Collections.Generic;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Melee;
using Origins.CrossMod;

namespace Origins.Items.Weapons.Ranged {
	public class Astoxo : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Bow
		];
		public override void SetStaticDefaults() {
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				global.astoxoEffect = true;
			});
			Origins.DamageBonusScale[Type] = 1.5f;
			CritType.SetCritType<Felnum_Crit_Type>(Type);
			OriginsSets.Items.FelnumItem[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Tsunami);
			Item.damage = 78;
			Item.width = 18;
			Item.height = 58;
			Item.useTime = Item.useAnimation = 29;
			Item.shootSpeed *= 1.5f;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.DaedalusStormbow)
			.AddIngredient(ModContent.ItemType<Valkyrum_Bar>(), 14)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, 0);
		}
		public static void DoEffect(Projectile projectile) {
			const float max_dist = 64;
			float floorDist = CollisionExt.Raymarch(projectile.Top, Vector2.UnitY, tile => !tile.HasSolidTile(), max_dist);
			if (floorDist == max_dist) floorDist = projectile.height;
			Projectile.NewProjectile(
				projectile.GetSource_Death(),
				projectile.Top + Vector2.UnitY * floorDist,
				projectile.velocity,
				ModContent.ProjectileType<Astoxo_Lightning>(),
				projectile.damage,
				projectile.knockBack * 4
			);
		}
	}
	public class Astoxo_Lightning : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_466";
		List<Vector2> positions;
		public override void SetDefaults() {
			Projectile.width = 16 * 5;
			Projectile.height = 0;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 10;
			Projectile.penetrate = -1;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (Projectile.localAI[0] == 0) {
				SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithVolume(2), Projectile.Center);
				SoundEngine.PlaySound(Main.rand.Next(Origins.Sounds.LightningSounds).WithVolume(2), Projectile.Center);
				Main.instance.CameraModifiers.Add(new CameraShakeModifier(
					Projectile.Center, 5f, 3f, 12, 750f, -1f, nameof(Astoxo)
				));
				Collision.HitTiles(Projectile.position, Vector2.UnitY * 4, Projectile.width, 4);
				Projectile.localAI[0] = Projectile.position.Y;
				Projectile.localAI[1] = 0;
				Rectangle checkBox = Projectile.Hitbox;
				checkBox.Y += 1;
				checkBox.Height = 1;
				checkBox.Inflate(-8, 0);
				foreach (Point tilePos in Collision.GetTilesIn(checkBox.TopLeft(), checkBox.BottomRight())) {
					Tile tile = Framing.GetTileSafely(tilePos);
					if (!tile.HasSolidTile() || tile.BlockType is BlockType.HalfBlock or BlockType.SlopeUpLeft or BlockType.SlopeUpRight) {
						Projectile.localAI[1] = 1;
						break;
					}
				}
			}
			if (positions is null) {
				positions = [];
				Vector2 offset = Vector2.Zero;
				while (offset.Y + Projectile.position.Y > 0) {
					positions.Add(offset + Projectile.Top);
					offset.X += Main.rand.NextFloat(64, 96 - offset.Y * 0.05f) * Main.rand.NextBool().ToDirectionInt();
					offset.X *= Main.rand.NextFloat(0.2f, 0.8f);
					offset.Y -= Main.rand.NextFloat(96, 128);
				}
			}
			if (Projectile.localAI[0] == 0) Projectile.Kill();
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.Y = 0;
			hitbox.Height = (int)Projectile.localAI[0];
		}
		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			MiscShaderData miscShaderData = GameShaders.Misc["MagicMissile"];
			miscShaderData.UseSaturation(-1f);
			miscShaderData.UseOpacity(4);
			miscShaderData.Apply();
			int maxLength = (int)Projectile.localAI[0] / 64;
			float[] oldRot = new float[maxLength];
			Vector2[] oldPos = new Vector2[maxLength];
			Vector2 start = Projectile.Center, end = Projectile.Center - Vector2.UnitY * Projectile.localAI[0];
			for (int i = 0; i < maxLength; i++) {
				oldPos[i] = Vector2.Lerp(start, end, i / (float)maxLength);
				oldRot[i] = MathHelper.PiOver2;
			}
			_vertexStrip.PrepareStrip(oldPos, oldRot, progress => {
				if (progress == 0 && Projectile.localAI[1] == 1) return Color.Transparent;
				return new Color(80, 204, 219, 0);
			}, _ => 32 * 5, -Main.screenPosition);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			if (positions is null) return false;
			OriginExtensions.DrawLightningArc(
				Main.spriteBatch,
				positions.ToArray(),
				TextureAssets.Extra[33].Value,
				1,
				-Main.screenPosition,
				(1f, new Color(80, 204, 219, 0) * 0.5f),
				(0.6666f, new Color(80, 251, 255, 0) * 0.5f),
				(0.3333f, new Color(200, 255, 255, 0) * 0.5f)
			);
			return false;
		}
	}
}
