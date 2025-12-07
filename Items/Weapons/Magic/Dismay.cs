using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Journal;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Magic {
	public class Dismay : ModItem, ICustomDrawItem, ICustomWikiStat, IJournalEntrySource {
        public string[] Categories => [
            "UsesBookcase",
			"SpellBook"
        ];
		public string EntryName => "Origins/" + typeof(Dismay_Entry).Name;
		public class Dismay_Entry : JournalEntry {
			public override string TextKey => "Dismay";
			public override JournalSortIndex SortIndex => new("The_Defiled", 5);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CursedFlames);
			Item.damage = 50;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.mana = 14;
			Item.crit = 6;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 5;
			Item.useAnimation = 25;
			Item.knockBack = 5;
			Item.shoot = ModContent.ProjectileType<Dismay_Spike>();
			Item.shootSpeed *= 1.2f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SoulofNight, 15)
			.AddIngredient(ItemID.SpellTome)
			.AddIngredient(ModContent.ItemType<Black_Bile>(), 20)
			.AddTile(TileID.Bookcases)
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (!player.ItemAnimationJustStarted) velocity = (OriginExtensions.Vec2FromPolar(player.itemRotation, velocity.Length()) * player.direction);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int n = (player.itemAnimationMax - player.itemAnimation) / player.itemTime + 1;
			velocity = velocity.RotatedBy(((n / 2) * ((n & 1) == 0 ? 1 : -1)) * 0.3f);
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false;
		}

		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			float itemRotation = drawPlayer.itemRotation;

			drawOrigin = new Vector2(20, 35);

			Vector2 pos = new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

			if (drawPlayer.gravDir == -1) drawInfo.itemEffect ^= SpriteEffects.FlipVertically;

			drawInfo.DrawDataCache.Add(new DrawData(
				itemTexture,
				pos,
				null,
				Item.GetAlpha(lightColor),
				itemRotation + MathHelper.PiOver2 * drawPlayer.direction,
				drawOrigin,
				drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect,
			0));
		}
	}
	public class Dismay_Spike : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.timeLeft = 25;
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 25;
			Projectile.ownerHitCheck = true;
		}
		public float movementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			Projectile.direction = projOwner.direction;
			Projectile.spriteDirection = projOwner.direction;
			Projectile.position.X = ownerMountedCenter.X - (Projectile.width / 2);
			Projectile.position.Y = ownerMountedCenter.Y - (Projectile.height / 2);
			if (movementFactor == 0f) {
				movementFactor = 1f;
				Projectile.netUpdate = true;
			}
			if (Projectile.timeLeft > 17) {
				movementFactor += 1f;
			}
			Projectile.position += Projectile.velocity * movementFactor;
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.rotation += MathHelper.PiOver2;
		}
		public override bool PreDraw(ref Color lightColor) {
			float totalLength = Projectile.velocity.Length() * movementFactor;
			int avg = (lightColor.R + lightColor.G + lightColor.B) / 3;
			lightColor = Color.Lerp(lightColor, new Color(avg, avg, avg), 0.5f);
			Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 18, System.Math.Min(58, (int)totalLength)), lightColor, Projectile.rotation, new Vector2(9, 0), Projectile.scale, SpriteEffects.None, 0);
			totalLength -= 58;
			Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 58;
			Texture2D texture = Mod.Assets.Request<Texture2D>("Projectiles/Weapons/Dismay_Mid").Value;
			int c = 0;
			Vector2 pos;
			for (int i = (int)totalLength; i > 0; i -= 58) {
				c++;
				pos = (Projectile.Center - Main.screenPosition) - (offset * c);
				Main.EntitySpriteDraw(texture, pos, new Rectangle(0, 0, 18, System.Math.Min(58, i)), lightColor, Projectile.rotation, new Vector2(9, 0), Projectile.scale, SpriteEffects.None, 0);
			}
			return false;
		}
	}
}
