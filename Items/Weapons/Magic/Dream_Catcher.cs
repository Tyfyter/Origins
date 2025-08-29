using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Weapons.Magic {
	public class Dream_Catcher : ModItem, ICustomDrawItem {
		private Asset<Texture2D> _smolTexture;
		public Texture2D SmolTexture {
			get {
				ModContent.RequestIfExists(Texture + "_P", out _smolTexture);
				return _smolTexture.Value;
			}
		}
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 28;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Magic;
			Item.damage = 7;
			Item.holdStyle = ItemHoldStyleID.HoldLamp;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 18;
			Item.useTime = 6;
			Item.mana = 16;
			Item.shoot = ModContent.ProjectileType<Dream_Catcher_Shard>();
			Item.shootSpeed = 10f;
			Item.knockBack = 0.5f;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
			if (player.altFunctionUse != 2) mult *= 0.5f;
			else mult *= (player.ownedProjectileCounts[ModContent.ProjectileType<Dream_Catcher_P>()] < 1).ToInt();
		}
		public override bool? UseItem(Player player) {
			int dreamcatcherProj = ModContent.ProjectileType<Dream_Catcher_P>();
			if (player.altFunctionUse == 2 && player.ItemAnimationJustStarted && player.ownedProjectileCounts[dreamcatcherProj] > 0) {
				foreach (Projectile proj in Main.ActiveProjectiles) {
					if (proj.owner == player.whoAmI && proj.type == dreamcatcherProj) {
						proj.ai[2] = 1;
						proj.netUpdate = true;
						break;
					}
				}
			}
			return base.UseItem(player);
		}
		public override bool CanShoot(Player player) => player.altFunctionUse != 2 || player.ownedProjectileCounts[ModContent.ProjectileType<Dream_Catcher_P>()] < 1;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				type = ModContent.ProjectileType<Dream_Catcher_P>();
			} else {
				ModifyShootStats(player, ref position, ref velocity, ref type, ref knockback);
			}
		}
		public static void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref float knockback) {
			if (player.ownedProjectileCounts[ModContent.ProjectileType<Dream_Catcher_P>()] > 0) {
				Projectile projectile = null;
				int dreamcatcherProj = ModContent.ProjectileType<Dream_Catcher_P>();
				foreach (Projectile proj in Main.ActiveProjectiles) {
					if (proj.owner == player.whoAmI && proj.type == dreamcatcherProj) {
						projectile = proj;
						break;
					}
				}
				if (projectile is not null) {
					position = projectile.Center;
					velocity = projectile.Center.DirectionTo(Main.MouseWorld) * velocity.Length();
				}
			}
			velocity += Main.rand.NextVector2Circular(1, 1);
			if (Main.rand.NextBool(7)) {
				type = ModContent.ProjectileType<Dream_Catcher_Vine>();
				knockback *= 5;
			}
		}
		public override void UseItemFrame(Player player) => HoldItemFrame(player);
		public override void HoldItemFrame(Player player) {
			if (Main.menuMode is MenuID.FancyUI or MenuID.CharacterSelect) return;
			OriginPlayer originPlayer = player.OriginPlayer();
			player.SetCompositeArmBack(
				true,
				player.ItemAnimationActive ? Player.CompositeArmStretchAmount.Quarter : Player.CompositeArmStretchAmount.Full,
				(-1f - Math.Clamp((player.velocity.Y - player.GetModPlayer<OriginPlayer>().AverageOldVelocity().Y) / 64, -0.1f, 0.1f)) * player.direction
			);
			originPlayer.dreamcatcherHoldTime = 3;
			Vector2 pos = player.GetCompositeArmPosition(true);
			pos.Y -= 4 * player.gravDir;
			if (originPlayer.dreamcatcherWorldPosition is Vector2 dreamcatcherWorldPosition) {
				Vector2 diff = dreamcatcherWorldPosition - pos;
				originPlayer.dreamcatcherRotSpeed += Math.Sign(diff.X) * Math.Abs(diff.X) * -0.005f;
			}
			originPlayer.dreamcatcherWorldPosition = pos;
		}
		public static void UpdateVisual(Player player, ref float dreamcatcherAngle, ref float dreamcatcherRotSpeed) {
			if (player.gravDir == -1) dreamcatcherAngle += MathHelper.Pi;
			dreamcatcherAngle = MathHelper.WrapAngle(dreamcatcherAngle + dreamcatcherRotSpeed);
			//Debugging.ChatOverhead(float.Pow(Math.Abs(dreamcatcherRotSpeed), 0.1f));
			if (dreamcatcherRotSpeed != 0) dreamcatcherRotSpeed *= Math.Clamp(0.93f + Math.Abs(dreamcatcherAngle) * 0.04f, 0, 1);
			dreamcatcherRotSpeed = Math.Clamp(dreamcatcherRotSpeed + float.Sin(dreamcatcherAngle) * -0.1f, -0.4f, 0.4f);
			if (player.gravDir == -1) dreamcatcherAngle -= MathHelper.Pi;
		}
		public bool BackHand => false;
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Vector2 pos = drawInfo.drawPlayer.GetCompositeArmPosition(true);
			pos.Y -= 4 * drawInfo.drawPlayer.gravDir;
			if (drawInfo.drawPlayer.mount?.Active == true && drawInfo.drawPlayer.mount.Type == MountID.Wolf) {
				pos = drawInfo.Position;
				pos.X += drawInfo.drawPlayer.width / 2 + 32 * drawInfo.drawPlayer.direction;
				pos.Y += 17;
				pos.Floor();
			}
			DrawData data = new(
				SmolTexture,
				pos - Main.screenPosition,
				SmolTexture.Frame(verticalFrames: 10),
				lightColor,
				drawInfo.drawPlayer.OriginPlayer().dreamcatcherAngle + (drawInfo.drawPlayer.gravDir - 1) * MathHelper.PiOver2,
				new Vector2(9, 0),
				1,
				drawInfo.itemEffect & SpriteEffects.FlipHorizontally
			);
			drawInfo.DrawDataCache.Add(data);
		}
	}
	public class Dream_Catcher_Shard : ModProjectile {
		public override string Texture => typeof(Fiberglass_Shard_P).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.friendly = true;
			Projectile.width = 5;
			Projectile.height = 5;
			Projectile.extraUpdates = 1;
			Projectile.penetrate = 3;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60;
			CooldownSlot = -2;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}
		readonly bool[] hasHit = new bool[Main.maxPlayers];
		public override bool CanHitPvp(Player target) => !hasHit[target.whoAmI];
		public override void OnHitPlayer(Player target, Player.HurtInfo info) => hasHit[target.whoAmI] = true;
	}
	public class Dream_Catcher_Vine : ModProjectile {
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.friendly = true;
			Projectile.width = 5;
			Projectile.height = 5;
			Projectile.extraUpdates = 1;
			Projectile.penetrate = 7;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.CritChance *= 2;
			Projectile.OriginalCritChance *= 2;
			int dreamcatcherProj = ModContent.ProjectileType<Dream_Catcher_P>();
			Projectile.ai[2] = -1;
			foreach (Projectile proj in Main.ActiveProjectiles) {
				if (proj.owner == Projectile.owner && proj.type == dreamcatcherProj) {
					Projectile.ai[2] = proj.identity;
					break;
				}
			}
		}
		public override void AI() {
			Entity target = null;
			float knockbackMult = 0;
			Entity owner = Projectile.ai[2] == -1 ? Main.player[Projectile.owner] : Projectile.GetRelatedProjectile(2);
			if (owner is null) {
				if (!Projectile.ai[2].TrySet(-1)) Projectile.Kill();
				return;
			}
			Vector2 toOwner = (owner.Center - Projectile.Center).Normalized(out float dist);
			Projectile.rotation = toOwner.ToRotation();
			Projectile.tileCollide = Projectile.ai[1] == 0;
			switch (Projectile.ai[1]) {
				case 0: {
					if (dist > 16 * 40) {
						Projectile.ai[1] = 3;
						goto case 3;
					}
					break;
				}
				case 1: {
					NPC _target = Main.npc[(int)Projectile.ai[0]];
					if (!_target.active) {
						Projectile.ai[1] = 3;
						goto case 3;
					}
					target = _target;
					knockbackMult = _target.knockBackResist;
					break;
				}
				case 2: {
					Player _target = Main.player[(int)Projectile.ai[0]];
					if (_target.dead || !_target.active) {
						Projectile.ai[1] = 3;
						goto case 3;
					}
					target = _target;
					knockbackMult = (!_target.noKnockback).ToInt();
					break;
				}
				case 3: {
					Projectile.velocity = toOwner * 6;
					if (dist < 32) Projectile.Kill();
					break;
				}
			}
			if (target is not null) {
				Projectile.Center = target.Center;
				target.velocity = Vector2.Lerp(target.velocity, toOwner * Projectile.knockBack, knockbackMult);
			}
		}
		public int chainFrameSeed = -1;
		public override bool PreDraw(ref Color lightColor) {
			if (chainFrameSeed == -1) {
				chainFrameSeed = Main.rand.Next(0, ushort.MaxValue);
			}
			FastRandom chainRandom = new(chainFrameSeed);
			Vector2 chainDrawPosition = Projectile.Center;
			Entity owner = Projectile.ai[2] == -1 ? Main.player[Projectile.owner] : Projectile.GetRelatedProjectile(2);
			if (owner is null) {
				if (!Projectile.ai[2].TrySet(-1)) Projectile.Kill();
				return false;
			}
			Vector2 start = owner.Center.MoveTowards(chainDrawPosition, 20f);
			chainDrawPosition = chainDrawPosition.MoveTowards(start, 20f);
			Vector2 vectorFromProjectileToPlayerArms = start - chainDrawPosition;
			float rotation = vectorFromProjectileToPlayerArms.ToRotation() + MathHelper.Pi;
			foreach (Vector2 chainPosition in GetChainPositions(chainDrawPosition, vectorFromProjectileToPlayerArms)) {
				Main.EntitySpriteDraw(
					TextureAssets.Projectile[Type].Value,
					chainPosition - Main.screenPosition,
					TextureAssets.Projectile[Type].Value.Frame(4, frameX: chainRandom.Next(1, 3)),
					Lighting.GetColor(chainPosition.ToTileCoordinates()),
					rotation,
					new Vector2(10, 14),
					Projectile.scale,
					0,
				0);
			}
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				start - Main.screenPosition,
				TextureAssets.Projectile[Type].Value.Frame(4),
				Lighting.GetColor(start.ToTileCoordinates()),
				rotation,
				new Vector2(10, 14),
				Projectile.scale,
				0,
			0);
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				chainDrawPosition - Main.screenPosition,
				TextureAssets.Projectile[Type].Value.Frame(4, frameX: 3),
				Lighting.GetColor(chainDrawPosition.ToTileCoordinates()),
				rotation,
				new Vector2(10, 14),
				Projectile.scale,
				0,
			0);
			return false;
		}
		IEnumerable<Vector2> GetChainPositions(Vector2 chainDrawPosition, Vector2 vectorFromProjectileToPlayerArms) {
			const int overlapPixels = 0;
			float chainLength = (16 - (overlapPixels * 2)) * Projectile.scale;
			Vector2 unitVectorFromProjectileToPlayerArms = vectorFromProjectileToPlayerArms.SafeNormalize(Vector2.Zero) * chainLength;
			float chainLengthRemainingToDraw = vectorFromProjectileToPlayerArms.Length() / chainLength + 1;
			while (chainLengthRemainingToDraw > 0f) {
				yield return chainDrawPosition;
				chainDrawPosition += unitVectorFromProjectileToPlayerArms;
				chainLengthRemainingToDraw--;
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.ai[1] == 0) {
				Projectile.ai[0] = target.whoAmI;
				Projectile.ai[1] = 1;
				Projectile.netUpdate = true;
			}
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (Projectile.ai[1] == 0) {
				Projectile.ai[0] = target.whoAmI;
				Projectile.ai[1] = 2;
				Projectile.netUpdate = true;
			}
			Projectile.penetrate--;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.ai[1] = 3;
			Projectile.netUpdate = true;
			return false;
		}
	}
	public class Dream_Catcher_P : ModProjectile {
		public override string Texture => typeof(Dream_Catcher).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.friendly = true;
			Projectile.width = 36;
			Projectile.height = 36;
			Projectile.extraUpdates = 0;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 6;
			CooldownSlot = -2;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse && itemUse.Player.whoAmI == Main.myPlayer) {
				(Projectile.ai[0], Projectile.ai[1]) = Main.MouseWorld;
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.manaRegenDelay < player.maxRegenDelay) player.manaRegenDelay = (int)player.maxRegenDelay;
			Projectile.tileCollide = Projectile.ai[2] == 0;
			switch (Projectile.ai[2]) {
				case 0: {
					if (Projectile.velocity != default) {
						Vector2 toTarget = (new Vector2(Projectile.ai[0], Projectile.ai[1]) - Projectile.Center).Normalized(out float dist);
						Projectile.velocity = toTarget * Math.Min(Projectile.velocity.Length(), dist);
					}
					break;
				}
				case 1: {
					Vector2 toTarget = (Main.player[Projectile.owner].MountedCenter - Projectile.Center).Normalized(out float dist);
					Projectile.velocity = toTarget * Math.Min(Math.Max(Projectile.velocity.Length(), 10), dist);
					if (dist < 8) Projectile.Kill();
					break;
				}
			}
			for (int i = 0; i < playerImmune.Length; i++) playerImmune[i].Cooldown();
		}
		readonly int[] playerImmune = new int[Main.maxPlayers];
		public override bool CanHitPvp(Player target) => playerImmune[target.whoAmI] <= 0;
		public override void OnHitPlayer(Player target, Player.HurtInfo info) => playerImmune[target.whoAmI] = Projectile.localNPCHitCooldown;
	}
}
