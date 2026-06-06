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

namespace Origins.Items.Weapons.Summoner {
	public abstract class MinionBuff : ModBuff {
		public abstract IEnumerable<int> ProjectileTypes();
		public virtual bool IsArtifact => false;
		public virtual bool DrawHealthBars => IsArtifact;
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
			if (ShowCount) {
				int count = 0;
				foreach (int proj in ProjectileTypes()) count += Main.LocalPlayer.ownedProjectileCounts[proj];
				spriteBatch.DrawString(FontAssets.ItemStack.Value, count + "", drawParams.TextPosition, drawParams.DrawColor, 0f, default, 0.8f, SpriteEffects.None, 0f);
			}
		}
	}
}
