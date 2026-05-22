using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.Projectiles;
using Origins.Tiles.Other;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Items;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class SMART_Wrench : ModItem, ICustomWikiStat, IExpectToBeUnobtainable {
		public override void SetDefaults() {
			Item.damage = 11;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 18;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.buffType = ModContent.BuffType<Smart_Turret_Buff>();
			Item.shoot = ModContent.ProjectileType<Smart_Turret>();
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item44;
			Item.noMelee = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Busted_Servo>(), 15)
			.AddIngredient(ModContent.ItemType<Power_Core>())
			.AddIngredient(ModContent.ItemType<Rotor>(), 5)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.ownedProjectileCounts[Item.shoot] >= 2) type = Smart_Turret_Counter.ID;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);
			return false;
		}
	}
	public class Smart_Turret_Buff : MinionBuff {
		public override string Texture => typeof(SMART_Wrench).GetDefaultTMLName();
		public override IEnumerable<int> ProjectileTypes() => [
			ModContent.ProjectileType<Smart_Turret>(),
			ModContent.ProjectileType<Smart_Turret_Counter>()
		];
		protected override void SetBuffFlag(Player player) => player.OriginPlayer().smartTurret = true;
	}
}
namespace Origins.Items.Weapons.Summoner.Minions {
	public class Smart_Turret : MinionBase {
		public int UpgradeCount => Owner.ownedProjectileCounts[Smart_Turret_Counter.ID];
		public int ProjectileTime {
			get {
				switch (UpgradeCount) {
					case 0:
					return 9;
					case 1:
					return 30;
					default:
					return 20;
				}
			}
		}
		public int ProjectileType {
			get {
				switch (UpgradeCount) {
					case 0:
					return ProjectileID.Bullet;
					case 1:
					return ProjectileID.Grenade;
					default:
					return ProjectileID.RocketFireworkBlue;
				}
			}
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.ignoreWater = false;
			Projectile.manualDirectionChange = true;
			Projectile.netImportant = true;
		}
		public override void MoveTowardsTarget() {
			Projectile.minionSlots = UpgradeCount > 0 ? float.BitDecrement(1) : 1;
			Rectangle target = targetingData.targetHitbox;
			if (Projectile.velocity.X != 0) Projectile.direction = Math.Sign(Projectile.velocity.X);
			float walkSpeed = 0.5f * SpeedModifier;
			const float walkDrag = 0.95f;
			if (targetingData.HasTarget) {
				Vector2 diff = target.Center() - Projectile.Center;
				Projectile.direction = Math.Sign(diff.X);
				if (Projectile.ai[1].CycleUp(ProjectileTime, SpeedModifier)) {
					SoundEngine.PlaySound(Origins.Sounds.EnergyRipple.WithPitch(1f).WithVolume(0.25f), Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item26.WithPitchRange(1.2f, 1.28f).WithVolume(0.1f), Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item35.WithPitchRange(0.2f, 0.3f).WithVolume(0.2f), Projectile.Center);
					Projectile.SpawnProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						diff.SafeNormalize(default) * 16,
						ProjectileType,
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner
					);
				}
				Projectile.velocity.X *= walkDrag;
			} else {
				target.Inflate(0, 16 * 100);
				float dir = Math.Sign(target.Center().X - Projectile.Center.X);
				if (!target.Intersects(Projectile.Hitbox)) {
					Projectile.velocity.X += walkSpeed * dir;
				}
				Projectile.velocity.X *= walkDrag;
				OriginExtensions.AngularSmoothing(ref Projectile.rotation, MathHelper.PiOver2 - 1.6f * dir, 0.05f);
				Projectile.ai[0] = -2;
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X != oldVelocity.X) {
				Vector2 pos = Projectile.position;
				Collision.StepDown(ref Projectile.position, ref oldVelocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
				Collision.StepUp(ref Projectile.position, ref oldVelocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);

				if (Projectile.position == pos) {
					int dir = Math.Sign(oldVelocity.X);
					Vector2 collisionPos = (Projectile.Bottom + new Vector2(18 * dir, 0));
					if (Framing.GetTileSafely(collisionPos.ToTileCoordinates()).HasFullSolidTile() && !Framing.GetTileSafely((collisionPos - new Vector2(0, 12)).ToTileCoordinates()).HasFullSolidTile()) {
						Projectile.velocity.Y = -5;
					}
				}
			}
			return false;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = targetingData.targetHitbox.Y > Projectile.BottomLeft.Y && (!targetingData.HasTarget || !CollisionExt.CanHitRay(Projectile.Center, targetingData.targetHitbox.Center()));
			return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
		}
		public override ref bool HasBuff(Player player) => ref player.OriginPlayer().smartTurret;
		public override void OnKill(int timeLeft) {
			base.OnKill(timeLeft);
		}
	}
	public class Smart_Turret_Counter : ModProjectile {
		public override string Texture => base.Texture.Replace("_Counter", null); // this one actually doesn't need a texture
		public override void SetStaticDefaults() {
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			OriginsSets.Projectiles.SupportsRealSpeedBuffs[Type] = static (_, _) => { };
			ID = Type;
		}
		public static int ID { get; private set; }
		public override void SetDefaults() {
			Projectile.netImportant = true;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.timeLeft = 60;
			Projectile.aiStyle = ProjAIStyleID.DesertTigerBall;
			Projectile.hide = true;
		}
		public override bool PreAI() {
			Player player = Main.player[Projectile.owner];
			if (player.ownedProjectileCounts[Projectile.type] > 1 && Projectile.localAI[0] == 0f) {
				Projectile.localAI[0] = 1f;
				SoundEngine.PlaySound(in SoundID.AbigailUpgrade, Projectile.Center);
			}
			ref bool smartTurret = ref player.OriginPlayer().smartTurret;
			if (player.dead) smartTurret = false;
			else if (smartTurret) Projectile.timeLeft = 2;
			return true;
		}
	}
}
