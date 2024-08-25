using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
	public class Outbreak_Bomb : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "ThrownExplosive",
			"IsBomb",
            "ExpendableWeapon"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Bomb);
			Item.damage = 65;
			Item.shoot = ModContent.ProjectileType<Outbreak_Bomb_P>();
			Item.value *= 2;
			Item.rare = ItemRarityID.Blue;
            Item.ArmorPenetration += 2;
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 15);
			recipe.AddIngredient(ItemID.Bomb, 15);
			recipe.AddIngredient(ItemID.DemoniteOre);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
		}
	}
	public class Outbreak_Bomb_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Outbreak_Bomb";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 135;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Bomb;
			Projectile.friendly = true;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.friendly = true;
			Projectile.Damage();
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Poisoned, 7 * 60);
			target.AddBuff(Outbreak_Bomb_Owner_Buff.ID, Projectile.owner + 1);
		}
	}
	public class Outbreak_Bomb_Cloud : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ToxicCloud3;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.aiStyle = 0;
			Projectile.width = Projectile.height = 96;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 15;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.localAI[0] = Main.rand.Next(ProjectileID.ToxicCloud, ProjectileID.ToxicCloud3 + 1);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Poisoned, 7 * 60);
			target.AddBuff(Outbreak_Bomb_Owner_Buff.ID, Projectile.owner + 1);
		}
		public override bool PreDraw(ref Color lightColor) {
			int type = (int)Projectile.localAI[0];
			Main.instance.LoadProjectile(type);
			Texture2D texture = TextureAssets.Projectile[type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor * 0.5f,
				Projectile.rotation,
				texture.Size() * 0.5f,
				3,
				0
			);
			return false;
		}
	}
	public class Outbreak_Bomb_Owner_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ToxicCloud3;
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
			Main.debuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			if (!npc.poisoned) {
				npc.DelBuff(buffIndex--);
			} else {
				npc.buffTime[buffIndex]++;
			}
		}
	}
}
