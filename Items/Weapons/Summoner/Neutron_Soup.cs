using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.NPCs;
using ReLogic.Content;
using System;
using Terraria;
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
			Item.channel = true;
		}
		public override void AddRecipes() => Recipe.Create(Type)
			.AddIngredient(ItemID.FragmentStardust, 18)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		public override void UseItemFrame(Player player) {
			Incantations.HoldItemFrame(player);
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.neutronSoupOffset += originPlayer.neutronSoupSpeed;
			originPlayer.neutronSoupOffset -= originPlayer.neutronSoupOffset * 0.01f;
		}
		public override void HoldItemFrame(Player player) => Incantations.HoldItemFrame(player);
		public bool BackHand => true;
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) => Incantations.DrawInHand(
			SmolTexture,
			ref drawInfo,
			lightColor
		);
		public override bool AltFunctionUse(Player player) => true;
		public override float UseTimeMultiplier(Player player) {
			if (player.altFunctionUse == 2) return 0.2f;
			return 1;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				type = ModContent.ProjectileType<Neutron_Soup_Flames>();
				OriginPlayer originPlayer = player.OriginPlayer();
				if (player.ItemUsesThisAnimation == 1) originPlayer.neutronSoupSpeed = Main.rand.NextFloat(0.02f, 0.025f) * Main.rand.NextBool().ToDirectionInt();
				velocity = velocity.RotatedBy(originPlayer.neutronSoupOffset) * 0.5f;
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				Vector2 shootDir = velocity.Normalized(out _);
				Vector2 moveDir = new(player.controlRight.ToInt() - player.controlLeft.ToInt(), player.controlDown.ToInt() - player.controlUp.ToInt());
				if (moveDir.LengthSquared() > 1) moveDir.Normalize();
				player.velocity -= velocity * 0.2f
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
	public class Neutron_Soup_Flames : ModProjectile {
		static AutoLoadingTexture starsTexture = typeof(Neutron_Soup_Flames).GetDefaultTMLName("_Stars");
		static AutoLoadingTexture starsColormap = typeof(Neutron_Soup_Flames).GetDefaultTMLName("_Stars_Colormap");
		public static float Lifetime => 108f;
		public static float FadeTime => 15f;
		public static float MinSize => 16f;
		public static float MaxSize => 66f;
		private readonly float[] sizes = new float[32];
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 32;
		}
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 6;
			Projectile.penetrate = 4;
			Projectile.friendly = true;
			Projectile.alpha = 255;
			Projectile.extraUpdates = 2;
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			for (int i = 0; i < Projectile.oldPos.Length; i++)
				Projectile.oldRot[i] = Main.rand.NextFloatDirection();
		}
		float Size => Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize, MaxSize);
		public override void AI() {
			Projectile.localAI[0] += 1f;
			for (int i = sizes.Length - 1; i > 0; i--) {
				sizes[i] = sizes[i - 1];
			}
			sizes[0] = Size;
			if (Projectile.localAI[2] == 1) {
				Lighting.AddLight(Projectile.Center, 0f, 0.85f, 0.4f);
			}
			//Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FrostStaff);
			Projectile.ai[0]++;
			Projectile.scale = Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize / 96f, MaxSize / 96f);
			Projectile.alpha = (int)(200 * (1 - (Projectile.localAI[0] / Lifetime)));
			Projectile.rotation += 0.3f * Projectile.direction;
			if (Projectile.ai[0] > Lifetime - FadeTime) {
				if (++Projectile.localAI[2] > FadeTime) Projectile.Kill();
			}
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			int scale = (int)(Size / 2) - hitbox.Width;
			hitbox.Inflate(scale, scale);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Neutron_Soup_Buff.ID, 240);
			if (target.life > 0) Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
		}
		public override bool PreDraw(ref Color lightColor) {
			//dstNoise = "Origins/Textures/SC_Mask";
			float progress = Projectile.ai[0] / Lifetime;
			float alphaMult = 1 - (Projectile.localAI[2] / FadeTime);
			Flamethrower_Drawer.Draw(Projectile,
				1 - progress,
				TextureAssets.Projectile[Type].Value,
				new Color(40, 60, 128),
				sizes,
				0,
				smokeAmount: progress,
				sizeProgressOverride: i => Math.Min(1 - ((Projectile.ai[0] - i) / Lifetime), 1) * 0.25f,
				alphaMultiplier: 0.55f * alphaMult,
				tint: i => new Color(0, 80, 128) * alphaMult
			);
			Flamethrower_Drawer.Draw(Projectile,
				1 - progress,
				starsColormap,
				Color.Black,
				sizes,
				8,
				smokeAmount: 0.15f,
				sizeProgressOverride: i => Math.Min(1 - ((Projectile.ai[0] - i) / Lifetime), 1) * 0.25f,
				alphaMultiplier: 0.5f * alphaMult,
				tint: i => new Color(255, 255, 255, 128) * alphaMult,
				pattern: starsTexture
			);
			return false;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = Vector2.Zero;
			return false;
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
