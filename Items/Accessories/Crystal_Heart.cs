using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using PegasusLib;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories; 
public class Crystal_Heart : ModItem {
	public static int SlimeHPThreshold => 32;
	public static int WingSlot { get; private set; }
	static AutoLoadingAsset<Texture2D> normalTexture;
	static AutoLoadingAsset<Texture2D> afTexture;
	public override void Load() {
		WingSlot = EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Wings}", EquipType.Wings, this);
		normalTexture = Texture;
		afTexture = Texture + "_AF";
	}
	public override void SetStaticDefaults() {
		ArmorIDs.Wing.Sets.Stats[WingSlot] = new(165, 7);
	}
	public override void SetDefaults() {
		Item.DefaultToAccessory(38, 20);
		Item.DamageType = DamageClass.Summon;
		Item.damage = 30;
		Item.knockBack = 1;
		Item.shoot = ModContent.ProjectileType<Crystal_Heart_Slime_Flying>();
		Item.value = Item.sellPrice(gold: 2);
		Item.rare = ItemRarityID.Pink;
		Item.master = true;
	}
	public override void UpdateAccessory(Player player, bool isHidden) {
		if (IsActive(player)) {
			player.wingTimeMax = player.GetWingStats(WingSlot).FlyTime;
			if (!isHidden || (player.velocity.Y != 0f && !player.mount.Active)) {
				player.wings = WingSlot;
			}
			player.wingsLogic = WingSlot;
			player.equippedWings = Item;
		}
		OriginPlayer originPlayer = player.OriginPlayer();
		originPlayer.crystalHeart = true;
		if (originPlayer.crystalHeartCounter >= SlimeHPThreshold) {
			originPlayer.crystalHeartCounter -= SlimeHPThreshold;
			originPlayer.crystalHeartCounter /= 2;
			player.SpawnProjectile(
				player.GetSource_Accessory(Item),
				player.MountedCenter,
				default,
				ModContent.ProjectileType<Crystal_Heart_Slime_Flying>(),
				player.GetWeaponDamage(Item),
				player.GetWeaponKnockback(Item)
			).originalDamage = Item.damage;
		}
	}
	public override void UpdateItemDye(Player player, int dye, bool hideVisual) {
		if (IsActive(player)) player.cWings = dye;
	}
	public static void SpawnSlime(Player player, Item item) {
	}
	public static bool IsActive(Player player) => player.statLife <= player.statLifeMax2 * 0.5f;
	public override bool MagicPrefix() => true;
	public override int ChoosePrefix(UnifiedRandom rand) => Item.AccessoryOrSpecialPrefix(rand, PrefixCategory.AnyWeapon, PrefixCategory.Magic);
	public override Color? GetAlpha(Color lightColor) {
		if (OriginsModIntegrations.CheckAprilFools()) {
			TextureAssets.Item[Type] = afTexture;
		} else {
			TextureAssets.Item[Type] = normalTexture;
		}
		return base.GetAlpha(lightColor);
	}
}
public class Crystal_Heart_Slime_Flying : ModProjectile, IArtifactMinion {
	public int MaxLife { get; set; }
	public float Life { get; set; }
	public override void SetStaticDefaults() {
		Main.projFrames[Type] = Main.projFrames[ProjectileID.BabySlime];
	}
	public override void SetDefaults() {
		Projectile.CloneDefaults(ProjectileID.BabySlime);
		Projectile.minionSlots = 0;
		Projectile.usesIDStaticNPCImmunity = true;
		Projectile.idStaticNPCHitCooldown = 10;
		Projectile.aiStyle = 0;
		Projectile.ContinuouslyUpdateDamageStats = true;
		MaxLife = 15 * 15;
	}
	public override void AI() {
		DrawOffsetX = -Projectile.width / 2;
		DrawOriginOffsetY = (int)(Projectile.height * -0.65f);
		Spiked_Slime_Minion.SlimeAI(Projectile);
		if (Projectile.frame >= 2) {
			return;
		}
		//DrawOffsetX = -Projectile.width / 2;
		int npcTarget = (int)Projectile.localAI[2];
		NPC target = default;
		if (npcTarget >= 0) {
			target = Main.npc[npcTarget];
			if (!target.active) Projectile.localAI[2] = -1;
			if (Projectile.ai[2] >= 0) {
				const float dist = 8 * 16;
				if (target.position.Y + target.height * 0.5f <= Projectile.position.Y + Projectile.height && Projectile.DistanceSQ(target.Center) < dist * dist) {
					ShootSpikes();
				}
			}
		}
		static bool IsPlatform(Tile tile) {
			return tile.HasTile && Main.tileSolidTop[tile.TileType];
		}
		float projectileBottom = Projectile.position.Y + Projectile.height;
		bool targetBelow = (target ?? (Entity)Main.player[Projectile.owner]).Bottom.Y > projectileBottom - (projectileBottom % 16);
		if (Projectile.tileCollide
			&& Projectile.velocity.Y > 0
			&& !targetBelow
			&& (IsPlatform(Framing.GetTileSafely((Projectile.BottomLeft + Vector2.UnitY).ToTileCoordinates()))
			|| IsPlatform(Framing.GetTileSafely((Projectile.BottomRight + Vector2.UnitY).ToTileCoordinates())))
		) {
			Projectile.velocity.Y = 0;
			float tileEmbedpos = projectileBottom;
			if ((int)tileEmbedpos / 16 == (int)(tileEmbedpos + 1) / 16) {
				Projectile.position.Y -= tileEmbedpos % 16;
			}
		}
		if (Projectile.ai[2] >= 0) {
			if ((Projectile.velocity.Y < -5.9f) && Projectile.ai[2] >= 0) {
				Projectile.ai[2] = 1;
			} else if (Projectile.ai[2] == 1 && Projectile.velocity.Y >= 0) {
				ShootSpikes();
			}
		} else Projectile.ai[2]++;
		Life -= 0.25f;
	}
	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
		if (Projectile.ai[2] >= 0) ShootSpikes();
	}
	public void ShootSpikes() {
		if (Projectile.owner == Main.myPlayer) {
			int spikeCount = Main.rand.Next(3, 7);
			for (int i = 0; i < spikeCount; i++) {
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center,
					new Vector2(0, -7.5f).RotatedBy(1 - (i / (float)(spikeCount - 1)) * 2),
					ModContent.ProjectileType<Crystal_Heart_Slime_Spike>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner
				);
			}
			Projectile.ai[2] = -45;
		}
		Terraria.Audio.SoundEngine.PlaySound(SoundID.Item154, Projectile.Center);
	}
	public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
		fallThrough = true;
		return true;
	}
	public override Color? GetAlpha(Color lightColor) {
		return lightColor * 0.8f;
	}
	public override bool OnTileCollide(Vector2 oldVelocity) {
		return false;
	}
}
public class Crystal_Heart_Slime_Spike : ModProjectile {
	public override void SetStaticDefaults() {
		Main.projFrames[Type] = Main.projFrames[ProjectileID.SpikedSlimeSpike];
		ProjectileID.Sets.MinionShot[Type] = true;
	}
	public override void SetDefaults() {
		Projectile.DamageType = DamageClass.Summon;
		Projectile.alpha = 255;
		Projectile.width = 6;
		Projectile.height = 6;
		Projectile.aiStyle = ProjAIStyleID.Arrow;
		Projectile.penetrate = 3;
		Projectile.friendly = true;
		Projectile.usesIDStaticNPCImmunity = true;
		Projectile.idStaticNPCHitCooldown = 10;
		//AIType = ProjectileID.SpikedSlimeSpike;
	}
	public override void AI() {
		if (Projectile.ai[0] >= 5f) {
			Projectile.ai[0] = 5f;
			Projectile.velocity.Y += 0.15f;
		}

		Dust dust = Dust.NewDustDirect(Projectile.position + Projectile.velocity,
			Projectile.width,
			Projectile.height,
			DustID.TintableDust,
			0f,
			0f,
			50,
			new Color(185, 43, 255, 150),
			1.0f
		);
		dust.velocity *= 0.3f;
		dust.velocity += Projectile.velocity * 0.3f;
		dust.noGravity = true;
		Projectile.alpha -= 50;
		if (Projectile.alpha < 0)
			Projectile.alpha = 0;
	}
}
