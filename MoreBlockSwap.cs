using Terraria.ModLoader;

namespace MoreBlockSwap
{
    public class MoreBlockSwap : Mod
    {
        public static MoreBlockSwap Instance => ModContent.GetInstance<MoreBlockSwap>();

        public override void Load()
        {
            On.Terraria.WorldGen.WouldTileReplacementWork += BlockSwapHooks.WorldGen_WouldTileReplacementWork;
            On.Terraria.WorldGen.MoveReplaceTileAnchor += BlockSwapHooks.WorldGen_MoveReplaceTileAnchor;
            On.Terraria.WorldGen.KillTile_GetItemDrops += BlockSwapHooks.WorldGen_KillTile_GetItemDrops;
            On.Terraria.WorldGen.ReplaceTIle_DoActualReplacement += BlockSwapHooks.WorldGen_ReplaceTIle_DoActualReplacement;
            On.Terraria.Player.PlaceThing_ValidTileForReplacement += BlockSwapHooks.Player_PlaceThing_ValidTileForReplacement;
            On.Terraria.WorldGen.ReplaceTile += BlockSwapHooks.WorldGen_ReplaceTile;
            On.Terraria.WorldGen.KillTile_DropItems += BlockSwapHooks.WorldGen_KillTile_DropItems;
        }

        public override void Unload()
        {
            On.Terraria.WorldGen.WouldTileReplacementWork -= BlockSwapHooks.WorldGen_WouldTileReplacementWork;
            On.Terraria.WorldGen.MoveReplaceTileAnchor -= BlockSwapHooks.WorldGen_MoveReplaceTileAnchor;
            On.Terraria.WorldGen.KillTile_GetItemDrops -= BlockSwapHooks.WorldGen_KillTile_GetItemDrops;
            On.Terraria.WorldGen.ReplaceTIle_DoActualReplacement -= BlockSwapHooks.WorldGen_ReplaceTIle_DoActualReplacement;
            On.Terraria.Player.PlaceThing_ValidTileForReplacement -= BlockSwapHooks.Player_PlaceThing_ValidTileForReplacement;
            On.Terraria.WorldGen.ReplaceTile -= BlockSwapHooks.WorldGen_ReplaceTile;
            On.Terraria.WorldGen.KillTile_DropItems -= BlockSwapHooks.WorldGen_KillTile_DropItems;
        }
    }
}