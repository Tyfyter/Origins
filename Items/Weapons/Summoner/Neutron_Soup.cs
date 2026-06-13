using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Dev;
using Origins.Events;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Magic;
using Origins.NPCs;
using PegasusLib.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Neutron_Soup : ModItem, ICustomDrawItem {
		private Asset<Texture2D> _smolTexture;
		public Texture2D SmolTexture => (_smolTexture ??= this.GetSmallTexture())?.Value;
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			OriginsSets.Items.ItemsThatCanChannelWithRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 80;
			Item.DefaultToIncantation(25);
			Item.shoot = ModContent.ProjectileType<Neutron_Soup_P>();
			Item.shootSpeed = 10f;
			Item.mana = 24;
			Item.knockBack = 1f;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item8;
			Item.autoReuse = true;
		}
		public override void AddRecipes() => Recipe.Create(Type)
			.AddIngredient(ItemID.FragmentStardust, 18)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		public override void UseItemFrame(Player player) {
			Incantations.HoldItemFrame(player);
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.neutronSoupSpeed += originPlayer.neutronSoupAcceleration;
			if (float.Sign(originPlayer.neutronSoupSpeed) == float.Sign(originPlayer.neutronSoupOffset)) originPlayer.neutronSoupSpeed -= originPlayer.neutronSoupOffset * 0.03f;
			originPlayer.neutronSoupOffset += originPlayer.neutronSoupSpeed;
			originPlayer.neutronSoupOffset -= originPlayer.neutronSoupOffset * 0.1f;
		}
		public override void HoldItemFrame(Player player) => Incantations.HoldItemFrame(player);
		public bool BackHand => true;
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) => Incantations.DrawInHand(
			SmolTexture,
			ref drawInfo,
			lightColor
		);
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				type = ModContent.ProjectileType<Neutron_Soup_Beam>();
				player.StartChanneling(type);
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				Vector2 shootDir = velocity.Normalized(out _);
				Vector2 moveDir = new(player.controlRight.ToInt() - player.controlLeft.ToInt(), player.controlDown.ToInt() - player.controlUp.ToInt());
				if (moveDir.LengthSquared() > 1) moveDir.Normalize();
				player.velocity -= velocity * 0.1f
					* Utils.GetLerpValue(-16, 0, Vector2.Dot(player.velocity, shootDir), true)
					* Utils.Remap(Vector2.Dot(moveDir, shootDir), 0, 1, 1, 0.25f);
			}
			return true;
		}
	}
	public class Neutron_Soup_P : ModProjectile {
		public override string Texture => base.Texture[..^2];
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.ignoreWater = true;
			Projectile.friendly = true;
			Projectile.timeLeft = 3600;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			Projectile.velocity.Y += 0.1f;
			Projectile.velocity *= 0.99f;
			Projectile.rotation += Projectile.velocity.X * 0.02f;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Neutron_Soup_Buff.ID, 240);
			if (target.life > 0) Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
		}
	}
	public class Neutron_Soup_Beam : ModProjectile, IShadedProjectile {
		readonly static Sound sound = EnvironmentSounds.Register<Sound>();
		public override string Texture => $"Terraria/Images/NPC_0";
		public float ManaMultiplier => 1;
		public float ChargeTime => Projectile.ai[0];
		public float ChargeFactor => float.Min(Projectile.ai[2], ChargeTime);
		public bool Charged => Projectile.ai[2] >= ChargeTime;
		public int Shader => Quasar.ShaderID;
		Neutron_Soup_Beam() : base() => _ = sound;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600 + 64;
			Origins.HomingEffectivenessMultiplier[Type] = 25;
			Smog_Storm.CutThroughSmogStorm[Type] = proj => ((Neutron_Soup_Beam)proj.ModProjectile).Draw(true);
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.localNPCHitCooldown = 10;
			Projectile.usesLocalNPCImmunity = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse { Player: Player player }) Projectile.ai[0] = player.itemTimeMax;
			Projectile.velocity = Projectile.velocity.Normalized(out _);
		}
		public override bool ShouldUpdatePosition() => false;
		public Vector2 TargetPos {
			get => new(Projectile.localAI[0], Projectile.localAI[1]);
			set => (Projectile.localAI[0], Projectile.localAI[1]) = value;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			OriginPlayer originPlayer = player.OriginPlayer();
			if (!Lunatics_Rune.CheckMana(player, player.HeldItem, ManaMultiplier / 60f, pay: true)) {
				Projectile.Kill();
				return;
			}
			if (Projectile.velocity.X != 0) player.ChangeDir(Math.Sign(Projectile.velocity.X));

			SoundEngine.SoundPlayer.Play(SoundID.Item158.WithPitch(Projectile.ai[2] / 30).WithVolume(0.24f), player.position);
			SoundEngine.SoundPlayer.Play(SoundID.Item132.WithPitch(Projectile.ai[2] / 30).WithVolume(0.24f), player.position);
			Projectile.position = Main.GetPlayerArmPosition(Projectile);
			if (player.mount?.Active ?? false) Projectile.position.Y -= player.mount.PlayerOffset;
			Projectile.position = player.RotatedRelativePoint(Projectile.position);
			if (Projectile.IsLocallyOwned()) {
				if (Projectile.localAI[2].CycleUp(ChargeTime * 0.5f)) originPlayer.neutronSoupAcceleration += Main.rand.NextFloat(0.002f, 0.0025f) * Main.rand.NextBool().ToDirectionInt();
				if (!player.channel) {
					Projectile.Kill();
					return;
				}
				Vector2 position = Projectile.position;
				Vector2 direction = Main.MouseWorld - position;

				Vector2 velocity = Vector2.Normalize(direction);
				if (velocity.HasNaNs()) velocity = -Vector2.UnitY;
				velocity = velocity.RotatedBy(originPlayer.neutronSoupOffset);
				if (Projectile.velocity != velocity) {
					float diff = GeometryUtils.AngleDif(Projectile.velocity.ToRotation(), velocity.ToRotation(), out int dir);
					if (diff > 0) {
						Min(ref diff, 0.03f);
						Projectile.velocity = Projectile.velocity.RotatedBy(diff * dir);
						Projectile.netUpdate = true;
					}
				}
			}
			player.ChangeDir(Projectile.direction); // Change the player's direction based on the projectile's own
			player.heldProj = Projectile.whoAmI; // We tell the player that the drill is the held projectile, so it will draw in their hand
			player.SetDummyItemTime(2); // Make sure the player's item time does not change while the projectile is out
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

			float dist = CollisionExt.Raymarch(Projectile.position, Projectile.velocity, ProjectileID.Sets.DrawScreenCheckFluff[Type] - 64) + 24;
			Vector2 newTarget = Projectile.position + Projectile.velocity * dist;
			TargetPos = newTarget;
			Projectile.ai[2]++;
			Vector2 shootDir = Projectile.velocity.Normalized(out _);
			Vector2 moveDir = new(player.controlRight.ToInt() - player.controlLeft.ToInt(), player.controlDown.ToInt() - player.controlUp.ToInt());
			if (moveDir.LengthSquared() > 1) moveDir.Normalize();
			player.velocity -= Projectile.velocity * 0.3f * (25 / Projectile.ai[0])
				* Utils.GetLerpValue(-16, 0, Vector2.Dot(player.velocity, shootDir), true)
				* Utils.Remap(Vector2.Dot(moveDir, shootDir), 0, 1, 1, 0.25f);
			if (Projectile.localAI[1].CycleUp(ChargeTime)) Projectile.ai[0] = CombinedHooks.TotalUseTime(player.HeldItem.useTime, player, player.HeldItem);
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= float.Lerp(0.1f, 1f, ChargeFactor);
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.SourceDamage *= float.Lerp(0.1f, 1f, ChargeFactor);
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			float collisionPoint = 1;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.position, TargetPos, 16, ref collisionPoint);
		}
		public override bool PreDraw(ref Color lightColor) {
			Draw();
			return false;
		}
		void Draw(bool forSmog = false) {
			float progress = Projectile.ai[2] / ChargeTime;
			Min(ref progress, 1);
			if (!forSmog) {
				sound.TrySetNearest(Main.Camera.Center.SnapToLine(
					Projectile.position,
					Projectile.position + Projectile.velocity * Projectile.ai[1],
					radius: 16
				));
			}
			if (!Collision.CheckAABBvLineCollision(Main.screenPosition, Main.ScreenSize.ToVector2(), Projectile.position, TargetPos)) return;
			SpriteBatchState state = Main.spriteBatch.GetState();
			Main.spriteBatch.Restart(state, samplerState: SamplerState.LinearWrap);
			Vector2 diff = TargetPos - Projectile.position;
			Vector2 position = Projectile.position;
			position -= Main.screenPosition;
			float rotation = diff.ToRotation();
			float dist = diff.Length();
			float scale = (1f + forSmog.ToInt()) / 256f;
			Color color = new(100, 180, 255, 0);
			color *= progress;
			Rectangle frame = new(256 - (int)((Projectile.ai[2] * 32) % 256), 0, (int)dist, 256);
			DrawData data = new(
				TextureAssets.Extra[ExtrasID.RainbowRodTrailErosion].Value,
				position,
				frame,
				color * progress,
				rotation,
				Vector2.UnitY * 128,
				new Vector2(1, 64 * scale),
				0
			);
			Main.EntitySpriteDraw(data);
		}
		static float soundVolume;
		class Sound : AEnvironmentSound {
			SlotId droning;
			SoundStyle sound = new("Terraria/Sounds/Item_104", SoundType.Sound) {
				IsLooped = true,
				PitchRange = (-1.2f, -0.8f)
			};
			public override void UpdateSound(Vector2 position) {
				float volume = 0;
				int reset = 0;
				Maximize(ref soundVolume, 2f / float.Max(position.DistanceSQ(Main.Camera.Center) / (16 * 20 * 16 * 20), 1));
				droning.PlaySoundIfInactive(sound, null, playingSound => {
					if (GetPosition() is Vector2 pos) {
						MathUtils.LinearSmoothing(ref volume, soundVolume, 1f / 60);
						reset = 0;
					} else if (MathUtils.LinearSmoothing(ref volume, 0, 1f / 15)) {
						soundVolume = 0;
						return false;
					}
					playingSound.Volume = volume;
					if (++reset > 5) soundVolume = 0;
					return true;
				});
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Neutron_Soup_Buff.ID, 240);
			if (target.life > 0) Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
		}
	}
	public class Neutron_Soup_Buff : ModBuff, ICustomWikiStat {
		public bool CanExportStats => false;
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().neutronSoupDebuff = true;
		}
	}
}
