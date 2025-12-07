using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Bomboomstick : ModItem, ICustomDrawItem, ICustomWikiStat {
        public string[] Categories => [
            "Launcher"
        ];
        public static AutoCastingAsset<Texture2D> UseTexture { get; private set; }
		public override void Unload() {
			UseTexture = null;
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				UseTexture = Mod.Assets.Request<Texture2D>("Items/Weapons/Demolitionist/Bomboomstick_Use");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Boomstick);
			Item.damage = 43;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.useAmmo = ItemID.Grenade;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Lime;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.ChlorophyteBar, 18)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			float itemRotation = drawPlayer.itemRotation;

			Vector2 pos = new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

			int frame;
			switch (drawPlayer.itemAnimationMax - drawPlayer.itemAnimation) {
				case 7:
				case 6:
				case 5:
				case 4:
					frame = 1;
					break;

				case 9:
				case 8:
				case 3:
				case 2:
					frame = 2;
					break;
				default:
				frame = 0;
				break;
			}

			drawInfo.DrawDataCache.Add(new DrawData(
				UseTexture,
				pos,
				new Rectangle(0, 24 * frame, 70, 22),
				Item.GetAlpha(lightColor),
				itemRotation,
				drawOrigin,
				drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect,
				0));
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = Main.rand.Next(3, 5); i-- > 0;) {
				Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.3f), type, damage / 2, knockback, player.whoAmI);
			}
			return false;
		}
	}
}
