using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Tiles.Defiled;
using PegasusLib;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	//TODO: flower effect, stop automatic turnaround from movement
	public class Amnestic_Rose : ModItem, ICustomDrawItem {
		static AutoLoadingAsset<Texture2D> stemTexture = typeof(Amnestic_Rose).GetDefaultTMLName() + "_Stem";
		static AutoLoadingAsset<Texture2D> flowerTexture = typeof(Amnestic_Rose).GetDefaultTMLName() + "_Flower";
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.useStyle = ItemUseStyleID.HiddenAnimation;
			Item.holdStyle = 420;
			Item.damage = 45;
			Item.DamageType = DamageClass.Magic;
			Item.shoot = ModContent.ProjectileType<Amnestic_Rose_Flower_P>();
			Item.noMelee = true;
			Item.width = 42;
			Item.height = 42;
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.mana = 16;
			Item.UseSound = SoundID.Item17;
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.Pink;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Wilting_Rose_Item>()
			.AddIngredient(ItemID.SoulofNight, 100)
			.AddIngredient<Black_Bile>(50)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		static Vector2 GetStartPosition(Player player) {
			return player.MountedCenter - new Vector2(player.width * player.direction * 0.5f, 0);
		}
		static void UpdateJoints(OriginPlayer originPlayer, PolarVec2[] joints) {
			int playerDirection = originPlayer.Player.direction;
			float diff;
			int dir;
			for (int j = 0; j < 2; j++) {
				int atLimit = 0;
				for (int i = 0; i < joints.Length; i++) {
					float prevRot;
					float highMaxCurve = 1;
					if (i > 0) {
						prevRot = joints[i - 1].Theta;
					} else {
						prevRot = -MathHelper.PiOver2 - playerDirection * 1.5f;
						highMaxCurve = 0.75f;
					}
					diff = GeometryUtils.AngleDif(joints[i].Theta, prevRot, out dir);
					float maxCurve = dir == playerDirection ? 0.2f : highMaxCurve;
					if (diff > maxCurve) {
						joints[i].Theta += (diff - maxCurve) * dir;
						atLimit++;
					} else if (dir == playerDirection || diff < 0.2f) {
						joints[i].Theta += 0.002f * originPlayer.Player.direction;
					}else if (dir != playerDirection && diff > 0.8f) {
						joints[i].Theta -= 0.002f * originPlayer.Player.direction;
					}
				}
				Vector2 pos = GetEndPos(originPlayer.Player, joints);
				diff = GeometryUtils.AngleDif(joints[^1].Theta, (originPlayer.relativeTarget + originPlayer.Player.Bottom - pos).ToRotation(), out dir) * dir;
				for (int i = 0; i < joints.Length; i++) {
					float bonus = 1f;
					if (i > 0) {
						bonus = 1 + float.Max(0, GeometryUtils.AngleDif(joints[i].Theta, joints[i - 1].Theta, out dir) - 0.5f) * dir;
					}
					joints[i].Theta += diff * 0.01f * (bonus + (atLimit / (float)joints.Length) * 2 + i * 0.25f);
				}
			}
		}
		static Vector2 GetEndPos(Player player, PolarVec2[] joints) {
			Vector2 startPos = GetStartPosition(player);
			for (int i = 0; i < joints.Length; i++) {
				startPos += (Vector2)joints[i];
			}
			return startPos;
		}
		public override void HoldItem(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			if (player.whoAmI == Main.myPlayer) originPlayer.relativeTarget = Main.MouseWorld - player.Bottom;
			if ((originPlayer.relativeTarget.X > 0) != (player.direction > 0)) {
				player.ChangeDir(originPlayer.relativeTarget.X > 0 ? 1 : -1);
				originPlayer.changedDir = true;
			}
			if (originPlayer.changedDir || originPlayer.amnesticRoseHoldTime <= 0) {
				float startRot = 3;
				if (player.direction == -1) startRot = MathHelper.Pi - startRot;
				float unit = player.direction * 0.9f;
				originPlayer.amnesticRoseJoints = [
					new(40, startRot + unit),
					new(40, startRot + unit * 2),
					new(40, startRot + unit * 3),
					new(40, startRot + unit * 4),
					new(28, startRot + unit * 5) // flower, 
				];
			}
			originPlayer.amnesticRoseHoldTime = 3;
			UpdateJoints(originPlayer, originPlayer.amnesticRoseJoints);
			if (originPlayer.amnesticRoseBloomTime.Cooldown()) {
				Vector2 pos = GetEndPos(originPlayer.Player, originPlayer.amnesticRoseJoints) - 8 * Vector2.One;
				for (int i = 0; i < 16; i++) {
					Dust.NewDust(pos, 16, 16, DustID.Paint, newColor: new Color(70, 60, 80));
				}
			}
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			OriginPlayer originPlayer = player.OriginPlayer();
			position = GetEndPos(player, originPlayer.amnesticRoseJoints);
			velocity = GeometryUtils.Vec2FromPolar(velocity.Length(), originPlayer.amnesticRoseJoints[^1].Theta);
			if (originPlayer.amnesticRoseBloomTime > 0) type = ModContent.ProjectileType<Amnestic_Rose_Thorn>();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			OriginPlayer originPlayer = player.OriginPlayer();
			if (originPlayer.amnesticRoseBloomTime > 0) {
				for (int i = 0; i < 8; i++) {
					Projectile.NewProjectile(
						source,
						position,
						velocity.RotatedByRandom(0.5f) * (1 + (i % 2 == 0).ToDirectionInt() * 0.1f),
						type,
						damage,
						knockback,
						player.whoAmI
					);
				}
				return false;
			}
			originPlayer.amnesticRoseBloomTime = 60 * 8;
			return true;
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			OriginPlayer originPlayer = drawInfo.drawPlayer.OriginPlayer();
			Rectangle frame = stemTexture.Frame(3);
			DrawData data = new(
				stemTexture,
				Vector2.Zero,
				frame,
				Color.White
			) {
				origin = new(frame.Width * 0.5f, frame.Height)
			};
			Vector2 pos = GetStartPosition(drawInfo.drawPlayer) - Main.screenPosition;
			for (int i = 0; i < originPlayer.amnesticRoseJoints.Length - 1; i++) {
				frame.X = frame.Width * (i % 3);
				data.sourceRect = frame;
				data.rotation = originPlayer.amnesticRoseJoints[i].Theta + MathHelper.PiOver2;
				data.position = pos;
				drawInfo.DrawDataCache.Add(data);
				pos += (Vector2)originPlayer.amnesticRoseJoints[i];
			}
			frame = flowerTexture.Frame(verticalFrames: 2, frameY: (originPlayer.amnesticRoseBloomTime <= 0).ToInt());
			data.texture = flowerTexture;
			data.sourceRect = frame;
			data.origin = new(frame.Width * 0.5f, frame.Height);
			data.rotation = originPlayer.amnesticRoseJoints[^1].Theta + MathHelper.PiOver2;
			data.position = pos;
			drawInfo.DrawDataCache.Add(data);
		}
	}
	public class Amnestic_Rose_Thorn : ModProjectile {
		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.friendly = true;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}
	}
	public class Amnestic_Rose_Flower_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
		}
		public override void AI() {
			if (Projectile.ai[0] == 1) {
				Projectile.ai[1] += 1 / 60f;
				if (Projectile.ai[1] > 1) Projectile.Kill();
			}
		}
		void StartBloom() {
			if (!Projectile.ai[0].TrySet(1)) return;
			Projectile.timeLeft = 120;
			Projectile.velocity = Vector2.Zero;
			Projectile.tileCollide = false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			StartBloom();
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			StartBloom();
			return false;
		}
	}
}
