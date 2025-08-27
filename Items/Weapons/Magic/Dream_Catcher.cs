using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using PegasusLib;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

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
			Item.damage = 7;
			Item.holdStyle = ItemHoldStyleID.HoldLamp;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 18;
			Item.useTime = 6;
			Item.mana = 16;
			Item.shoot = ModContent.ProjectileType<Dream_Catcher_Shard>();
			Item.shootSpeed = 8f;
			Item.knockBack = 0.5f;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
			if (player.altFunctionUse != 2) mult *= 0.5f;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {

			} else {
				ModifyShootStats(ref position, ref velocity, ref type, ref damage, ref knockback);
			}
		}
		public static void ModifyShootStats(ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity += Main.rand.NextVector2Circular(1, 1);
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
			Projectile.friendly = true;
			Projectile.width = 5;
			Projectile.height = 5;
			Projectile.extraUpdates = 1;
			Projectile.penetrate = 7;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60;
			Projectile.light = 0.025f;
			CooldownSlot = -2;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}
		readonly bool[] hasHit = new bool[Main.maxPlayers];
		public override bool CanHitPvp(Player target) => !hasHit[target.whoAmI];
		public override void OnHitPlayer(Player target, Player.HurtInfo info) => hasHit[target.whoAmI] = true;
	}
}
