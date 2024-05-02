using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class CanisterGlobalItem : GlobalItem {
		public static Dictionary<int, int> ItemToCanisterID { get; private set; } = new();
		public static Dictionary<Type, int> TypeToCanisterID { get; private set; } = new();
		public static List<CanisterData> CanisterDatas { get; private set; } = new();
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			if (entity.ModItem is ICanisterAmmo canister) {
				if (!ItemToCanisterID.TryGetValue(entity.type, out _)) {
					ItemToCanisterID.Add(entity.type, CanisterDatas.Count);
					TypeToCanisterID.Add(entity.ModItem.GetType(), CanisterDatas.Count);
					CanisterDatas.Add(canister.GetCanisterData with { WhatAmI = CanisterDatas.Count });
				}
				return true;
			}
			return false;
		}
		public static int GetCanisterType(int type) {
			return ItemToCanisterID.TryGetValue(type, out int canisterID) ? canisterID : -1;
		}
		public static int GetCanisterType(Type type) {
			return TypeToCanisterID.TryGetValue(type, out int canisterID) ? canisterID : - 1;
		}
	}
	public interface ICanisterAmmo {
		CanisterData GetCanisterData { get; }
	}
	public record struct CanisterData(Color OuterColor, Color InnerColor, int WhatAmI = -999);
	public class CanisterGlobalProjectile : GlobalProjectile {
		public override bool InstancePerEntity => true;
		ICanisterProjectile canister;
		int canisterID;
		CanisterData canisterData;
		public int CanisterID { 
			get => canisterID;
			set {
				canisterID = value;
				if (canisterID == -1) return;
				canisterData = CanisterGlobalItem.CanisterDatas[value];
			}
		}
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			if (entity.ModProjectile is ICanisterProjectile) {
				return true;
			}
			return false;
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			canister = projectile.ModProjectile as ICanisterProjectile;
			if (source is EntitySource_ItemUse_WithAmmo ammoSource) {
				CanisterID = CanisterGlobalItem.GetCanisterType(ammoSource.AmmoItemIdUsed);
			}
		}
		public override bool PreDraw(Projectile projectile, ref Color lightColor) {
			Vector2 origin = canister.OuterTexture.Value.Size() * 0.5f;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				canister.InnerTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.InnerColor,
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				canister.OuterTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.OuterColor.MultiplyRGBA(lightColor),
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			return false;
		}
	}
	public interface ICanisterProjectile {
		public abstract AutoLoadingAsset<Texture2D> OuterTexture { get; }
		public abstract AutoLoadingAsset<Texture2D> InnerTexture { get; }
	}
}
