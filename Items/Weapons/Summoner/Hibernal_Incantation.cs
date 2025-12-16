using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;
using System.IO;
using Terraria.Audio;
using System.Collections.Generic;

namespace Origins.Items.Weapons.Summoner {
	public class Hibernal_Incantation : ModItem, ICustomDrawItem {
		private Asset<Texture2D> _smolTexture;
		public Texture2D SmolTexture => (_smolTexture ??= this.GetSmallTexture())?.Value;
		public override void SetStaticDefaults() {
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.Frostburn];
		}
		public override void SetDefaults() {
			Item.damage = 10;
			Item.DamageType = DamageClasses.Incantation;
			Item.noMelee = true;
			Item.width = 22;
			Item.height = 34;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 26;
			Item.useAnimation = 26;
			Item.shoot = ModContent.ProjectileType<Hibernal_Incantation_P>();
			Item.shootSpeed = 9.75f;
			Item.mana = 14;
			Item.knockBack = 1f;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item8;
			Item.channel = true;
			Item.holdStyle = ItemHoldStyleID.HoldLamp;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Book, 5)
			.AddIngredient(ItemID.Shiverthorn, 7)
			.AddIngredient(ItemID.IceBlock, 15)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override void UseItemFrame(Player player) => Incantations.HoldItemFrame(player);
		public override void HoldItemFrame(Player player) => Incantations.HoldItemFrame(player);
		public bool BackHand => true;
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Incantations.DrawInHand(
				SmolTexture,
				ref drawInfo,
				lightColor
			);
		}
	}
	public class Hibernal_Incantation_P : ModProjectile {
		public const int frameSpeed = 5;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 6;
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.ignoreWater = true;
			Projectile.friendly = true;
			Projectile.timeLeft = 150;
			Projectile.alpha = 50;
			Projectile.manualDirectionChange = true;
		}
		(bool manual, Vector2 target) targetData;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (Projectile.ai[0] == 0 && Projectile.velocity.X != 0) Projectile.direction = Math.Sign(Projectile.velocity.X);
			#region Find target
			Vector2 targetCenter = Projectile.Center;
			float targetAngle = 0;
			bool foundTarget = false;
			if (player.channel && Projectile.ai[0] == 0) {
				foundTarget = true;
				if (Main.myPlayer == Projectile.owner) targetCenter = Main.MouseWorld;
				else if (targetData.manual) targetCenter = targetData.target;
				Projectile.timeLeft = 90;
				Projectile.localNPCHitCooldown = 30;
				Vector2 oldTarg = targetData.target;
				targetData = (true, targetCenter);
				if (targetData.manual && oldTarg != targetCenter) Projectile.netUpdate = true;
			} else {
				Projectile.ai[0] = 1;
				Projectile.penetrate = 1;
			}
			#endregion

			#region Movement
			// Default movement parameters (here for attacking)
			float currentSpeed = Projectile.velocity.Length();
			float speed = 18f;
			float turnSpeed = 24f;
			if (foundTarget) {
				if ((int)Math.Ceiling(targetAngle) == -1) {
					targetCenter.Y -= 16;
				}
			}
			LinearSmoothing(ref currentSpeed, speed, 1f + (currentSpeed / 25f));
			if (foundTarget) {
				Vector2 direction = targetCenter - Projectile.Center;
				float distance = direction.Length();
				direction /= distance;
				Projectile.velocity = (Vector2.Normalize(Projectile.velocity + direction * turnSpeed) * currentSpeed).WithMaxLength(distance);
			}
			#endregion

			#region Animation and visuals

			Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1;

			// This is a simple "loop through all frames from top to bottom" animation
			Projectile.rotation += 0.4f * Projectile.direction;
			if (Main.rand.NextBool(2)) {
				Dust dust = Dust.NewDustDirect(Projectile.position - Vector2.One * 2, Projectile.width + 4, Projectile.height + 4, DustID.IceTorch, Projectile.velocity.X * 0.4f, Projectile.velocity.Y * 0.4f, 100, Scale: 1.2f);
				dust.noGravity = true;
				dust.velocity *= 1.8f;
				dust.velocity.Y -= 0.5f;
			}
			if (Projectile.owner == Main.myPlayer && ++Projectile.localAI[1] > 23) {
				SpawnProj();
				Projectile.localAI[1] = 0;
			}
			#endregion
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(targetData.manual);
			if (targetData.manual) writer.WriteVector2(targetData.target);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			targetData = reader.ReadBoolean() ? (true, reader.ReadVector2()) : (false, default);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Hibernal_Incantation_Buff.ID, 240);
			if (Main.rand.NextBool(4)) target.AddBuff(BuffID.Frostburn, damageDone * 6);
			if (target.life > 0 && target.CanBeChasedBy()) Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
			if (Projectile.ai[0] != 0) Projectile.damage = (int)(Projectile.damage * 0.96f);
			Projectile.localAI[2]++;
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
			for (int i = 0; i < 7; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position - Vector2.One * 2, Projectile.width + 4, Projectile.height + 4, DustID.IceTorch, Projectile.oldVelocity.X * 0.4f, Projectile.oldVelocity.Y * 0.4f, 100, Scale: 1.2f);
				dust.noGravity = true;
				dust.velocity *= 3.6f;
				dust.velocity.Y -= 0.5f;
			}
			if (Projectile.owner == Main.myPlayer) {
				for (int i = (int)(Projectile.localAI[2] / 3); i < 3; i++) {
					SpawnProj();
				}
			}
		}
		void SpawnProj() {
			Projectile.NewProjectileDirect(
				Projectile.GetSource_Death(),
				Projectile.Center,
				(Main.rand.NextVector2Unit() * 3) + (Projectile.velocity * 0.25f),
				ModContent.ProjectileType<Hibernal_Icicle_P>(),
				Projectile.damage / 3,
				Projectile.knockBack
			);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 24;
			height = 24;
			fallThrough = true;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) => true;
	}
	public class Hibernal_Icicle_P : ModProjectile {
		public static float margin = 0.5f;
		float stabVel = 1;
		float drawOffsetY;
		public override string Texture => "Origins/Projectiles/Weapons/Icicle_P";
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.penetrate = 2;//when projectile.penetrate reaches 0 the projectile is destroyed
			Projectile.extraUpdates = 1;
			Projectile.aiStyle = ProjAIStyleID.Arrow;
			Projectile.localNPCHitCooldown = 10;
			Projectile.usesLocalNPCImmunity = true;
			drawOffsetY = 0;//-34;
			DrawOriginOffsetX = -0.5f;
			Projectile.hide = true;
			Projectile.scale = 0.85f;
		}
		public override void AI() {
			if (Projectile.aiStyle == 0) {
				if (drawOffsetY > -20) {
					drawOffsetY -= stabVel;
				} else {
					drawOffsetY = -20;
				}
				Projectile.velocity = Vector2.Zero;
			}
			DrawOriginOffsetY = (int)drawOffsetY;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Hibernal_Incantation_Buff.ID, 240);
			if (Main.rand.NextBool(4)) target.AddBuff(BuffID.Frostburn, damageDone * 4);
			if (target.life > 0 && target.CanBeChasedBy()) Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
			if (Projectile.ai[0] != 0) Projectile.damage = (int)(Projectile.damage * 0.96f);
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.aiStyle != 0) {
				Projectile.aiStyle = 0;
				Projectile.knockBack = 0.1f;
				Projectile.timeLeft = 90;
				stabVel = Projectile.velocity.Length() * 0.85f;//(oldVelocity-projectile.velocity).Length();
			}
			return false;
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			if (Projectile.aiStyle == 0) {
				hitbox = hitbox.Add(new Vector2(0, -1.5f).RotatedBy(Projectile.rotation) * (-34 - drawOffsetY));
			}
		}
		public override bool? CanHitNPC(NPC target) {
			return (Projectile.aiStyle != 0 || PokeAngle(target.velocity)) ? null : false;
		}
		bool PokeAngle(Vector2 velocity) {
			return NormDot(velocity, Vec2FromPolar(Projectile.rotation - MathHelper.PiOver2)) > 1f - margin;
		}
	}
}
