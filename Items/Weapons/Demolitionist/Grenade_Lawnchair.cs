using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Empowerments;
namespace Origins.Items.Weapons.Demolitionist {
	public class Grenade_Lawnchair : ModItem, ICustomDrawItem, ICustomWikiStat {
        public string[] Categories => [
            "Launcher"
        ];
        public static AutoCastingAsset<Texture2D> UseTexture { get; private set; }
		public override void Unload() {
			UseTexture = null;
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				UseTexture = ModContent.Request<Texture2D>(Texture + "_Use");
			}
		}
		public override void SetDefaults() {
			Item.knockBack = 5.75f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 40;
			Item.useTime = 40;
			Item.width = 50;
			Item.height = 14;
			Item.shoot = ProjectileID.Grenade;
			Item.UseSound = SoundID.Item36;
			Item.shootSpeed = 5.35f;
			Item.noMelee = false;
			Item.damage = 0;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.useAmmo = ItemID.Grenade;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Lime;
			Item.useLimitPerAnimation = 3;
		}
		public override float UseTimeMultiplier(Player player) => 1f / Item.useTime;
		public override void AddRecipes() {
			/*Recipe.Create(Type)
			.AddIngredient(ItemID.ChlorophyteBar, 18)
			.AddTile(TileID.MythrilAnvil)
			.Register();*/
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			float itemRotation = drawPlayer.itemRotation;

			Vector2 pos = new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

			drawInfo.DrawDataCache.Add(new DrawData(
				UseTexture,
				pos,
				null,
				Item.GetAlpha(lightColor),
				itemRotation,
				drawOrigin,
				drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect,
				0));
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = Main.rand.Next(6, 9); i-- > 0;) {
				Projectile.NewProjectile(source, position, velocity.RotatedByRandom(1.4f) * Main.rand.NextFloat(0.9f, 1), type, damage, knockback, player.whoAmI);
			}
			return false;
		}
	}
}
