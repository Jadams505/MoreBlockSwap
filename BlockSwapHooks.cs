using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MoreBlockSwap
{
    public static class BlockSwapHooks
    {
        internal static bool Player_PlaceThing_ValidTileForReplacement(On.Terraria.Player.orig_PlaceThing_ValidTileForReplacement orig, Player self)
        {
            bool vanillaCall = orig(self);
            return vanillaCall || IsTileValidForMoreBlockSwapReplacement(self.HeldItem.createTile, self.HeldItem.placeStyle, Player.tileTargetX, Player.tileTargetY);
        }

        internal static bool WorldGen_WouldTileReplacementWork(On.Terraria.WorldGen.orig_WouldTileReplacementWork orig, ushort attemptingToReplaceWith, int x, int y)
        {
            bool vanillaCall = orig(attemptingToReplaceWith, x, y);
            return vanillaCall || IsTileValidForMoreBlockSwapReplacement(attemptingToReplaceWith, Main.LocalPlayer.HeldItem.placeStyle, x, y);
        }

        private static bool IsTileValidForMoreBlockSwapReplacement(int heldTile, int placeStyle, int targetX, int targetY)
        {
            Tile tileToReplace = Main.tile[targetX, targetY];
            TileObjectData data = TileObjectData.GetTileData(tileToReplace);

            if (SwapValidityUtil.IsInvalidForReplacement(tileToReplace))
            {
                return false;
            }

            if (BlockSwapUtil.IsConversionCase(tileToReplace.TileType, heldTile, out _, out _))
            {
                return true;
            }

            if (data == null)
            {
                return false;
            }

            Point16 potentialTileEntityPos = BlockSwapUtil.TileEntityCoordinates(targetX, targetY, data.CoordinateWidth + data.CoordinatePadding, data.Width, data.Height);
            if (TileEntity.ByPosition.TryGetValue(potentialTileEntityPos, out _))
            {
                return false;
            }

            if (SwapValidityUtil.IsValidForCrossTypeReplacement(heldTile, placeStyle, targetX, targetY, tileToReplace))
            {
                return true;
            }

            if (SwapValidityUtil.IsValidForReplacementCustom(heldTile, placeStyle, tileToReplace))
            {
                return true;
            }

            if (heldTile == tileToReplace.TileType && heldTile < TileID.Count)
            {
                int tileToReplaceItemPlaceStyle = BlockSwapUtil.GetItemPlaceStyleFromTile(tileToReplace);
                if (data.RandomStyleRange > 0)
                {
                    return placeStyle == tileToReplaceItemPlaceStyle; // allows for replacement into a different random style
                }
                return placeStyle != tileToReplaceItemPlaceStyle;
            }
            return false;
        }

        internal static void WorldGen_MoveReplaceTileAnchor(On.Terraria.WorldGen.orig_MoveReplaceTileAnchor orig, ref int x, ref int y, ushort targetType, Tile t)
        {
            if (BlockSwapUtil.ShouldVanillaHandleSwap(targetType, t))
            {
                orig(ref x, ref y, targetType, t);
                return;
            }
            Point topLeftPos = BlockSwapUtil.TopLeftOfMultiTile(x, y, t);
            x = topLeftPos.X;
            y = topLeftPos.Y;
        }

        internal static void WorldGen_KillTile_GetItemDrops(On.Terraria.WorldGen.orig_KillTile_GetItemDrops orig, int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops)
        {
            orig(x, y, tileCache, out dropItem, out dropItemStack, out secondaryItem, out secondaryItemStack, includeLargeObjectDrops);
            
            if (includeLargeObjectDrops) // Only true when called in WorldGen.ReplaceTile
            {
                int targetTileId = tileCache.TileType;
                TileObjectData data = TileObjectData.GetTileData(tileCache);
                Player playerToGiveDropsTo = Main.player[Player.FindClosest(new Vector2(x, y) * 16f, 16, 16)]; // Can't use Main.LocalPlayer because this is called on the server
                int heldTile = playerToGiveDropsTo.HeldItem.createTile; // vanilla does this too so that means it works, right?

                if (BlockSwapUtil.IsConversionCase(targetTileId, heldTile, out _, out int dropOverride))
                {
                    dropItem = dropOverride;
                    return;
                }

                if (dropItem > 0)
                {
                    return;
                }

                if (data != null)
                {
                    if(targetTileId == TileID.OpenDoor)
                    {
                        targetTileId = TileID.ClosedDoor;
                    }

                    int style = BlockSwapUtil.GetItemPlaceStyleFromTile(tileCache);
                    int drop = BlockSwapUtil.GetItemDrop(targetTileId, style);
                    if (drop != -1)
                    {
                        dropItem = drop;
                    }
                }
            }
        }

        internal static void WorldGen_ReplaceTIle_DoActualReplacement(On.Terraria.WorldGen.orig_ReplaceTIle_DoActualReplacement orig, ushort targetType, int targetStyle, int topLeftX, int topLeftY, Tile t)
        {
            if (BlockSwapUtil.ShouldVanillaHandleSwap(targetType, t))
            {
                if(BlockSwapUtil.IsConversionCase(t.TileType, targetType, out int typeOverride, out _))
                {
                    targetType = (ushort)typeOverride;
                    ReplacementUtil.SingleTileSwap(targetType, targetStyle, topLeftX, topLeftY);
                    return;
                }
                orig(targetType, targetStyle, topLeftX, topLeftY, t);
                return;
            }

            if (t.TileType == TileID.OpenDoor && targetType == TileID.ClosedDoor)
            {
                targetType = TileID.OpenDoor;
            }

            ReplacementUtil.MultiTileSwap(targetType, targetStyle, topLeftX, topLeftY);
        }
    }
}
