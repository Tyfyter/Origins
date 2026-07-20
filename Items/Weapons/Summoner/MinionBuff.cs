using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria;
using System.Reflection;
using Terraria.GameContent;
using ReLogic.Graphics;
using System.Linq;
using Terraria.ID;

namespace Origins.Items.Weapons.Summoner {
	[ReinitializeDuringResizeArrays]
	public abstract class MinionBuff : ModBuff {
		public static bool[] SkipInCount = ProjectileID.Sets.Factory.CreateBoolSet();
		public abstract IEnumerable<int> ProjectileTypes();
		public virtual bool IsArtifact => false;
		public virtual bool DrawHealthBars => IsArtifact;
		public virtual bool ShowSlots => false;
		public virtual bool ShowCount => true;
		public override string Texture => ModContent.HasAsset(base.Texture) ? base.Texture : base.Texture.Replace("Items/Weapons/Summoner", "Buffs");
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			if (GetType().GetProperty("ID", BindingFlags.Static | BindingFlags.Public) is PropertyInfo id && id.PropertyType == typeof(int)) id.SetValue(null, Type);
		}
		public override void Update(Player player, ref int buffIndex) {
			foreach (int proj in ProjectileTypes()) {
				if (player.ownedProjectileCounts[proj] > 0) {
					player.buffTime[buffIndex] = 18000;
					SetBuffFlag(player);
					return;
				}
			}
			player.DelBuff(buffIndex--);
		}
		protected virtual void SetBuffFlag(Player player) { }
		public override bool RightClick(int buffIndex) {
			HashSet<int> toKill = ProjectileTypes().ToHashSet();
			try {
				ArtifactMinionSystem.IsDismissingMinion = false;
				foreach (Projectile other in Main.ActiveProjectiles) {
					if (!other.IsLocallyOwned() || !toKill.Contains(other.type)) continue;
					other.Kill();
				}
			} finally {
				ArtifactMinionSystem.IsDismissingMinion = false;
			}
			return true;
		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			if (DrawHealthBars && OriginClientConfig.Instance.ArtifactMinionHealthbarStyle != ArtifactMinionHealthbarStyles.UnderMinion) {
				float startY = drawParams.TextPosition.Y;
				foreach (int proj in ProjectileTypes()) {
					ArtifactMinionSystem.DrawBuffHealthbars(proj, ref drawParams, startY);
				}
			}
			if (ShowSlots) {
				float slots = 0;
				HashSet<int> kinds = ProjectileTypes().ToHashSet();
				foreach (Projectile other in Main.ActiveProjectiles) {
					if (!other.IsLocallyOwned() || !kinds.Contains(other.type)) continue;
					slots += other.minionSlots;
				}
				spriteBatch.DrawString(FontAssets.ItemStack.Value, $"{slots:0.#}", drawParams.TextPosition, drawParams.DrawColor, 0f, default, 0.8f, SpriteEffects.None, 0f);
				drawParams.TextPosition.Y += FontAssets.ItemStack.Value.LineSpacing * 0.8f * 0.9f;
			}
			if (ShowCount) {
				int count = 0;
				foreach (int proj in ProjectileTypes()) if (!SkipInCount[proj]) count += Main.LocalPlayer.ownedProjectileCounts[proj];
				spriteBatch.DrawString(FontAssets.ItemStack.Value, count + "", drawParams.TextPosition, drawParams.DrawColor, 0f, default, 0.8f, SpriteEffects.None, 0f);
			}
		}
	}
}
