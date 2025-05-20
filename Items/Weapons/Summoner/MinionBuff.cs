using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using Terraria.GameContent;
using ReLogic.Graphics;

namespace Origins.Items.Weapons.Summoner {
	public abstract class MinionBuff : ModBuff {
		public abstract IEnumerable<int> ProjectileTypes();
		public virtual bool IsArtifact => false;
		public virtual bool DrawHealthBars => IsArtifact;
		public virtual bool ShowCount => true;
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			if (GetType().GetProperty("ID", BindingFlags.Static | BindingFlags.Public) is PropertyInfo id && id.PropertyType == typeof(int)) id.SetValue(null, Type);
		}
		public override void Update(Player player, ref int buffIndex) {
			bool foundAny = false;
			foreach (int proj in ProjectileTypes()) {
				if (player.ownedProjectileCounts[proj] > 0) {
					player.buffTime[buffIndex] = 18000;
					foundAny = true;
				}
			}
			if (!foundAny) {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
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
