using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Weapons.Ranged;
using Origins.NPCs;
using Origins.Tiles.Other;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Dyes;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Amber {
    [AutoloadEquip(EquipType.Head)]
	public class Amber_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => new string[] {
            "HardmodeArmorSet",
            "ExplosiveBoostGear"
        };
        public override void SetStaticDefaults() {
            ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Amber_Breastplate>() && legs.type == ModContent.ItemType<Amber_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "-38% explosive blast radius. All explosives are preserved in amber and release amber shards upon detonation\nAmber shards slow enemies, heal the player of any self-damage received, and decrease the defense of struck enemies by half";
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.explosiveBlastRadius -= 0.38f;
			originPlayer.amberSet = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Amber, 4);
			recipe.AddIngredient(ItemID.SoulofNight, 3);
			recipe.AddIngredient(ModContent.ItemType<Carburite_Item>(), 12);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		public string ArmorSetName => "Amber_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Amber_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Amber_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Amber_Breastplate : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.2f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Amber, 12);
			recipe.AddIngredient(ItemID.SoulofNight, 3);
			recipe.AddIngredient(ModContent.ItemType<Carburite_Item>(), 36);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Amber_Greaves : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.1f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Amber, 8);
			recipe.AddIngredient(ItemID.SoulofNight, 3);
			recipe.AddIngredient(ModContent.ItemType<Carburite_Item>(), 24);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Amber_Dye : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.Amber;
		public static int ID { get; private set; }
		public static int ShaderID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
			GameShaders.Armor.BindShader(Type, new ArmorShaderData(Main.PixelShaderRef, "ArmorStardust"))
			.UseImage("Images/Misc/noise")
			.UseColor(1.5f, 0.8f, 0.4f)
			.UseSecondaryColor(2.0f, 1.2f, 0.4f)
			.UseSaturation(1f);
			ShaderID = GameShaders.Armor.GetShaderIdFromItemId(Type);
		}
	}
	public class Amber_Shard : ModProjectile, IShadedProjectile {
		public override string Texture => "Terraria/Images/Item_" + ItemID.Amber;
		public int Shader => Amber_Dye.ShaderID;
		public override void SetDefaults() {
			Projectile.friendly = true;
			Projectile.localAI[2] = ModContent.ProjectileType<Shardcannon_P1>() + Main.rand.Next(3);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Amber_Debuff.Inflict(target, 300);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[(int)Projectile.localAI[2]].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				texture.Size() * 0.5f,
				1,
				SpriteEffects.None
			);
			return false;
		}
	}
	public class Amber_Debuff_Shard : ModDust {
		public record struct AmberDebuffShardData(Entity Parent, int Texture, Vector2 Offset, float Rotation);
		public override string Texture => "Terraria/Images/Item_" + ItemID.Amber;
		public override void OnSpawn(Dust dust) {
			dust.customData = new AmberDebuffShardData(null, ModContent.ProjectileType<Shardcannon_P1>() + Main.rand.Next(3), default, Main.rand.NextFloat(MathHelper.TwoPi));
		}
		public override bool Update(Dust dust) {
			bool remain = true;
			if(dust.customData is not AmberDebuffShardData data) {
				dust.active = false;
				return false;
			}
			if (!data.Parent.active) remain = false;
			Vector2 center = data.Parent.Center;

			Vector2 direction = new(data.Parent.direction, 1);
			if (data.Parent is NPC npc) {
				if (remain && !npc.HasBuff(Amber_Debuff.ID)) remain = false;
				center += npc.netOffset;
				center.Y += npc.gfxOffY;
				direction.Y = npc.directionY;
			} else if (data.Parent is Projectile && !data.Parent.active) {
				dust.active = false;
			}

			float rotation = data.Parent.GetRotation();
			dust.position = (data.Offset * direction).RotatedBy(rotation) + center;
			dust.rotation = data.Rotation + rotation;

			if (!remain && (dust.alpha += 7) >= 255) dust.active = false;
			return false;
		}
		public override bool PreDraw(Dust dust) {
			if (dust.customData is not AmberDebuffShardData data) {
				dust.active = false;
				return false;
			}
			Color lightColor = Lighting.GetColor((int)(dust.position.X + 4) / 16, (int)(dust.position.Y + 4) / 16);
			Texture2D texture = TextureAssets.Projectile[data.Texture].Value;
			Main.spriteBatch.Draw(
				texture,
				dust.position - Main.screenPosition,
				null,
				lightColor * (1 - dust.alpha / 255f),
				dust.rotation,
				texture.Size() * 0.5f,
				1,
				SpriteEffects.None,
			0);
			return false;
		}
		public static void SpawnDusts(Entity entity) {
			ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(Amber_Dye.ID);
			int type = ModContent.DustType<Amber_Debuff_Shard>();
			int size = entity.width + entity.height;
			List<Vector2> points = OriginExtensions.FelisCatusSampling(entity.Hitbox, (int)(size * 0.3f), size * 0.05f + 4, size * 0.25f + 8);
			Vector2 direction = new(entity.direction, 1);
			if (entity is NPC npc) {
				direction.Y = npc.directionY;
			}
			for (int i = 0; i < points.Count; i++) {
				Dust dust = Dust.NewDustPerfect(
					points[i],
					type,
					Vector2.Zero
				);
				dust.shader = shader;
				if (dust.customData is AmberDebuffShardData data) {
					dust.customData = data with {
						Parent = entity,
						Offset = (points[i] - entity.Center).RotatedBy(-entity.GetRotation()) * direction
					};
				}
				
			}
		}
	}
	public class Amber_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Item_" + ItemID.Amber;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public static void Inflict(NPC npc, int duration) {
			if (!npc.HasBuff(ID)) {
				Amber_Debuff_Shard.SpawnDusts(npc);
			}
			npc.AddBuff(ID, duration);
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.AddBuff(Slow_Debuff.ID, 5);
			npc.GetGlobalNPC<OriginGlobalNPC>().amberDebuff = true;
		}
	}
}
