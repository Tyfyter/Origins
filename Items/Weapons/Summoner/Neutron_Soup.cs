using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Materials;
using Origins.NPCs;
using Origins.Projectiles.Weapons;
using ReLogic.Content;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
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
			Item.damage = 10;
			Item.DefaultToIncantation(26);
			Item.shoot = ModContent.ProjectileType<Neutron_Soup_P>();
			Item.shootSpeed = 10f;
			Item.mana = 14;
			Item.knockBack = 1f;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item8;
			Item.autoReuse = true;
			Item.channel = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.FragmentStardust, 18)
			.AddTile(TileID.LunarCraftingStation)
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
		public override bool AltFunctionUse(Player player) => true;
		public override float UseTimeMultiplier(Player player) {
			if (player.altFunctionUse == 2) return 0.25f;
			return 1;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				type = ModContent.ProjectileType<Neutron_Soup_Flames>();
				velocity *= 0.5f;
			}
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
		public static float Lifetime => 108f;
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
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			for (int i = 0; i < Projectile.oldPos.Length; i++)
				Projectile.oldRot[i] = Main.rand.NextFloatDirection();
		}
		float Size => Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize, MaxSize);
		public override void AI() {
			if (Projectile.localAI[2] == 0) {
				Projectile.localAI[2] = 1 + Projectile.wet.ToInt();
			}
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
			if (Projectile.ai[0] > Lifetime) {
				Projectile.Kill();
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
		static AutoLoadingTexture dstNoise = "Origins/Textures/DSTNoise";
		public override bool PreDraw(ref Color lightColor) {
			//dstNoise = "Origins/Textures/SC_Mask";
			float progress = Projectile.ai[0] / Lifetime;
			Flamethrower_Drawer.Draw(Projectile,
				1 - progress,
				TextureAssets.Projectile[Type].Value,
				Color.DarkCyan,
				sizes,
				8,
				//brightnessColorExponent: 0.25f,
				smokeAmount: (Projectile.localAI[2] - 1) * 0.5f + progress * 0.5f,
				sizeProgressOverride: i => Math.Min(1 - ((Projectile.ai[0] - i) / Lifetime), 1) * 0.25f,
				alphaMultiplier: Projectile.localAI[2] * 0.55f
				//tint: i => Color.White * (1 - float.Pow(progress, 1 - i / 32)),
				//pattern: dstNoise
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
