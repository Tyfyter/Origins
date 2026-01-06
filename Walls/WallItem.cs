using Terraria.ModLoader;

namespace Origins.Walls {
	public class WallItem(ModWall wall) : ModItem {
		[field: CloneByReference]
		public ModWall Wall { get; } = wall;
		public override string Name => Wall.Name + "_Item";
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 396;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(Wall.Type);
		}
	}
}
