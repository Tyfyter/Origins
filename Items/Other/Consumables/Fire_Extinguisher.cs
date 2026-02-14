using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Ranged;
using PegasusLib;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Items.Other.Consumables {
	[ReinitializeDuringResizeArrays]
	public class Fire_Extinguisher : ModItem, ICustomWikiStat, ICustomDrawItem {
		AutoLoadingAsset<Texture2D> bottleTexture = typeof(Fire_Extinguisher).GetDefaultTMLName("_Bottle");
		AutoLoadingAsset<Texture2D> cordTexture = typeof(Fire_Extinguisher).GetDefaultTMLName("_Cord");
		public static bool[] FireExtinguisherExtinguishes { get; } = NPCID.Sets.Factory.CreateNamedSet($"{nameof(FireExtinguisherExtinguishes)}")
		.RegisterBoolSet(
			BuffID.OnFire,
			BuffID.OnFire3,
			BuffID.ShadowFlame,
			BuffID.Frostburn,
			BuffID.Frostburn2
		);
		public virtual int MaxDurability => 120;
		public int durability;
		public int Durability {
			get => durability;
			set {
				durability = value;
				if (value <= 0) Item.SetDefaults(ModContent.ItemType<Empty_Fire_Extinguisher>());
			}
		}
		public override void SetStaticDefaults() {
			ItemID.Sets.SkipsInitialUseSound[Type] = false;
		}
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shoot = ModContent.ProjectileType<Fire_Extinguisher_P>();
			Item.shootSpeed = 6;
			Item.noMelee = true;
			Item.damage = 0;
			Item.useAnimation = 15;
			Item.useTime = 3;
			Item.width = 86;
			Item.height = 22;
			Item.rare = ItemRarityID.LightPurple;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item13;
			durability = MaxDurability;
		}
		public override Vector2? HoldoutOffset() => Vector2.Zero;
		public override bool CanUseItem(Player player) => durability > 0;
		public override bool? UseItem(Player player) {
			if (player.itemAnimation <= player.itemTimeMax) Durability--;
			return base.UseItem(player);
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<Scrap>(8)
			.AddIngredient<Silicon_Bar>(4)
			.AddIngredient(ItemID.Gel, 10)
			.Register();
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = 0; i < 2; i++) {
				Projectile.NewProjectile(
					source,
					position,
					velocity.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.9f, 1.1f),
					Main.rand.Next(Fire_Extinguisher_P.IDs),
					damage + 1,
					knockback
				);
			}
			if (player.wet) player.velocity -= velocity * 0.1f;
			return false;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Text.Contains("{0}")) {
					tooltips[i].Text = string.Format(tooltips[i].Text, durability, MaxDurability);
					break;
				}
			}
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (MaxDurability == 0) return;
			float portion = durability / (float)MaxDurability;
			const int width = 32;
			position -= new Vector2(width * 0.5f, (origin.Y - frame.Height) - 2) * scale;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				new Rectangle((int)position.X, (int)position.Y, (int)(width * scale), 2),
				Color.Black
			);
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				new Rectangle((int)position.X, (int)position.Y, (int)(width * portion * scale), 2),
				Main.hslToRgb(portion * 0.25f, 1, 0.5f)
			);
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;

			drawInfo.DrawDataCache.Add(new DrawData(
				bottleTexture,
				drawPlayer.MountedCenter.Floor() - Main.screenPosition,
				null,
				Item.GetAlpha(lightColor),
				0,
				new Vector2(2, 4).Apply(drawInfo.itemEffect, bottleTexture.Value.Size()),
				drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect
			));

			Vector2 pos = new((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y + drawInfo.mountOffSet));
			float itemRotation = drawPlayer.itemRotation;
			if (drawPlayer.direction == -1) itemRotation += MathHelper.Pi;

			drawInfo.DrawDataCache.Add(new DrawData(
				cordTexture,
				(pos + itemRotation.ToRotationVector2() * 14).Floor(),
				null,
				Item.GetAlpha(lightColor),
				drawPlayer.itemRotation,
				new Vector2(11, 5),
				drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect
			));
		}
		public override void NetSend(BinaryWriter writer) => writer.Write(durability);
		public override void NetReceive(BinaryReader reader) => durability = reader.ReadInt32();
		public override void SaveData(TagCompound tag) => tag[nameof(durability)] = durability;
		public override void LoadData(TagCompound tag) => durability = tag.TryGet(nameof(durability), out durability) ? durability : MaxDurability;
	}
	public class Empty_Fire_Extinguisher : Fire_Extinguisher {
		public override int MaxDurability => 0;
		public override string Texture => typeof(Fire_Extinguisher).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 0;
		}
		public override void AddRecipes() => Recipe.Create(ModContent.ItemType<Fire_Extinguisher>())
			.AddIngredient(Type)
			.AddIngredient(ItemID.Gel, 10)
			.Register();
		public override bool CanUseItem(Player player) => false;
		public override void ModifyTooltips(List<TooltipLine> tooltips) { }
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) { }
		public override void NetSend(BinaryWriter writer) { }
		public override void NetReceive(BinaryReader reader) { }
		public override void SaveData(TagCompound tag) { }
		public override void LoadData(TagCompound tag) { }
	}
	public class Fire_Extinguisher_P : ModProjectile {
		public static List<int> IDs { get; } = [];
		public bool hit = false;
		Vector2 splatterVelocity;
		int splatterEntity = -1;
		public static Color Color => Color.White * 0.4f;
		public virtual int DustType => ModContent.DustType<Paint_Coating_Gore>();
		public override string Texture => "Origins/Gores/" + typeof(Paint_Coating_Gore).Name;
		public override void SetStaticDefaults() {
			IDs.Add(Type);
		}
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 12;
			Projectile.friendly = false;
			Projectile.timeLeft = 30;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.penetrate = 1;
		}
		public override Color? GetAlpha(Color lightColor) => lightColor.MultiplyRGBA(Color);
		public override void AI() {
			if (hit) return;
			foreach (Player target in Main.ActivePlayers) {
				if (target.whoAmI == Projectile.owner) continue;
				if (Projectile.Hitbox.Intersects(target.Hitbox)) {
					for (int i = target.buffType.Length - 1; i >= 0; i--) {
						if (target.buffTime[i] <= 0) continue;
						if (Fire_Extinguisher.FireExtinguisherExtinguishes[target.buffType[i]]) {
							target.DelBuff(i);
						}
					}
					SetSplatter(Projectile.Center - Rectangle.Intersect(Projectile.Hitbox, target.Hitbox).Center(), target);
				}
			}
			foreach (NPC target in Main.ActiveNPCs) {
				if (Projectile.Hitbox.Intersects(target.Hitbox)) {
					for (int i = target.buffType.Length - 1; i >= 0; i--) {
						if (target.buffTime[i] <= 0) continue;
						if (Fire_Extinguisher.FireExtinguisherExtinguishes[target.buffType[i]]) {
							target.DelBuff(i);
						}
					}
					SetSplatter(Projectile.Center - Rectangle.Intersect(Projectile.Hitbox, target.Hitbox).Center(), target);
				}
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Vector2 newVelocity = Projectile.velocity;
			Projectile.velocity = oldVelocity;
			SetSplatter(newVelocity);
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(hit);
			if (hit) {
				writer.Write(splatterVelocity.X);
				writer.Write(splatterVelocity.Y);
				writer.Write(splatterEntity);
			}
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			if (reader.ReadBoolean()) {
				hit = true;
				Vector2 vel = reader.ReadVector2();
				int target = reader.ReadInt32();
				Entity entity = null;
				if (target >= 300) {
					entity = Main.npc[target - 300];
				} else if (target >= 0) {
					entity = Main.player[target];
				}
				Splatter(vel, entity);
				Projectile.Kill();
			}
		}
		public override void OnKill(int timeLeft) {
			if (hit) return;
			Dust.NewDustPerfect(
				Projectile.Center - Projectile.velocity,
				DustType,
				Projectile.velocity,
				newColor: Color
			);
		}
		void SetSplatter(Vector2 newVelocity, Entity collisionEntity = null) {
			if (!hit.TrySet(true)) return;
			Projectile.tileCollide = false;
			Projectile.aiStyle = 0;
			Projectile.netSpam = 0;
			Projectile.netUpdate = true;
			Projectile.timeLeft = 2;
			splatterVelocity = newVelocity;
			if (collisionEntity is NPC npc) {
				splatterEntity = npc.whoAmI - 300;
			} else if (collisionEntity is Player player) {
				splatterEntity = player.whoAmI;
			}
			Splatter(newVelocity, collisionEntity);
		}
		public void Splatter(Vector2 newVelocity, Entity collisionEntity = null) {
			Vector2 dustSpawnPosition = Projectile.Center + newVelocity;
			PaintStickData dustCustomData = new(true);
			if (collisionEntity is Player player) {
				dustCustomData = new(true, player, (dustSpawnPosition - player.Center) * new Vector2(player.direction, 1), 0, 0);
			} else if (collisionEntity is NPC npc) {
				dustCustomData = new(true, npc, (npc.Center - dustSpawnPosition) * new Vector2(npc.direction, npc.directionY), npc.rotation, 0);
			}
			Dust dust = Dust.NewDustPerfect(
				dustSpawnPosition,
				DustType,
				Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0f, 0.4f),
				newColor: Color
			);
			dust.customData = dustCustomData;
		}
	}
	public class Fire_Extinguisher_P2 : Fire_Extinguisher_P {
		public override int DustType => ModContent.DustType<Paint_Coating_Gore2>();
		public override string Texture => "Origins/Gores/" + typeof(Paint_Coating_Gore2).Name;
	}
	public class Fire_Extinguisher_P3 : Fire_Extinguisher_P {
		public override int DustType => ModContent.DustType<Paint_Coating_Gore3>();
		public override string Texture => "Origins/Gores/" + typeof(Paint_Coating_Gore3).Name;
	}
}
