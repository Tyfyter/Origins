using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Projectiles {
	public class VanillaWhipScaleSupport : GlobalProjectile {
		public override bool InstancePerEntity => true;
		public float ScaleModifier { get; private set; } = 1f;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => ProjectileID.Sets.IsAWhip[entity.type] && entity.ModProjectile?.Mod is not Origins;
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (!OriginConfig.Instance.VanillaWhipScale) return;
			if (source is EntitySource_ItemUse itemUse) SetScaleModifier(projectile, itemUse.Player.GetAdjustedItemScale(itemUse.Item));
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			if (!OriginConfig.Instance.VanillaWhipScale) return;
			binaryWriter.Write(ScaleModifier);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			if (!OriginConfig.Instance.VanillaWhipScale) return;
			ScaleModifier = binaryReader.ReadSingle();
		}
		public void SetScaleModifier(Projectile projectile, float modifier) {
			projectile.scale = (projectile.scale / ScaleModifier) * modifier;
			ScaleModifier = modifier;
		}
	}
}
