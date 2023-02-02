using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace MoreBlockSwap
{
    public static class BlockSwapHooks
    {
        internal static bool Player_PlaceThing_ValidTileForReplacement(On.Terraria.Player.orig_PlaceThing_ValidTileForReplacement orig, Player self)
        {
            bool vanillaCall = orig(self);
            return vanillaCall || IsTileValidForMoreBlockSwapReplacement(self);
        }

        private static bool IsTileValidForMoreBlockSwapReplacement(Player player)
        {
            int heldTile = player.HeldItem.createTile;
            int placeStyle = player.HeldItem.placeStyle;

            Tile tileToReplace = Main.tile[Player.tileTargetX, Player.tileTargetY];

            if (heldTile == tileToReplace.TileType && heldTile < TileID.Count)
            {
                /*
                if (BlockSwapSystem.Instance.TilesThatDontWork.Contains(heldTile))
                {
                    return false;
                }
                */

                TileObjectData data = TileObjectData.GetTileData(tileToReplace);
                if (data == null)
                {
                    return false;
                }

                int tileToReplaceItemPlaceStyle = BlockSwapUtil.GetItemPlaceStyleFromTile(tileToReplace);
                if (data.RandomStyleRange > 0)
                {
                    return placeStyle == tileToReplaceItemPlaceStyle; // allows for replacement into a different random style
                }
                return placeStyle != tileToReplaceItemPlaceStyle;
            }
            return false;
        }

        internal static bool WorldGen_WouldTileReplacementWork(On.Terraria.WorldGen.orig_WouldTileReplacementWork orig, ushort attemptingToReplaceWith, int x, int y)
        {
            bool vanillaCall = orig(attemptingToReplaceWith, x, y);
            return vanillaCall || WouldMoreBlockSwapTileReplacementWork(attemptingToReplaceWith, x, y);
        }

        private static bool WouldMoreBlockSwapTileReplacementWork(ushort attemptingToReplaceWith, int x, int y)
        {
            Tile tileToReplace = Framing.GetTileSafely(x, y);
            if (tileToReplace.TileType == attemptingToReplaceWith && attemptingToReplaceWith < TileID.Count)
            {
                TileObjectData data = TileObjectData.GetTileData(tileToReplace);
                if (data != null)
                {
                    return true;
                }
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
            TileObjectData data = TileObjectData.GetTileData(t);
            if (data != null)
            {
                int xAdjustment = t.TileFrameX % data.CoordinateFullWidth / (data.CoordinateWidth + data.CoordinatePadding);
                int yAdjustment = t.TileFrameY % data.CoordinateFullHeight / (data.CoordinateHeights[0] + data.CoordinatePadding);
                x -= xAdjustment;
                y -= yAdjustment;
            }
        }

        internal static void WorldGen_KillTile_GetItemDrops(On.Terraria.WorldGen.orig_KillTile_GetItemDrops orig, int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops)
        {
            orig(x, y, tileCache, out dropItem, out dropItemStack, out secondaryItem, out secondaryItemStack, includeLargeObjectDrops);
            if (dropItem > 0)
            {
                return;
            }
            if (includeLargeObjectDrops)
            {
                TileObjectData data = TileObjectData.GetTileData(tileCache);
                if (data != null)
                {
                    int style = BlockSwapUtil.GetItemPlaceStyleFromTile(tileCache);
                    int drop = BlockSwapUtil.GetItemDrop(tileCache.TileType, style);
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
                orig(targetType, targetStyle, topLeftX, topLeftY, t);
                return;
            }
            TileObjectData data = TileObjectData.GetTileData(t);
            if (data != null)
            {
                bool canPlace = TileObject.CanPlace(topLeftX, topLeftY, targetType, targetStyle, 0, out TileObject placeData, onlyCheck: false, checkStay: true);

                int newTopLeftX;
                int newTopLeftY;
                int style = data.CalculatePlacementStyle(targetStyle, 0, placeData.random);
                int adjustedStyle = 0;

                if (data.StyleWrapLimit > 0)
                {
                    adjustedStyle = style / data.StyleWrapLimit * data.StyleLineSkip;
                    style %= data.StyleWrapLimit;
                }

                if (data.StyleHorizontal)
                {
                    newTopLeftX = data.CoordinateFullWidth * style;
                    newTopLeftY = data.CoordinateFullHeight * adjustedStyle;
                }
                else
                {
                    newTopLeftX = data.CoordinateFullWidth * adjustedStyle;
                    newTopLeftY = data.CoordinateFullHeight * style;
                }

                int newFrameX = newTopLeftX;
                for (int i = 0; i < data.Width; ++i)
                {
                    int newFrameY = newTopLeftY;
                    for (int j = 0; j < data.Height; ++j)
                    {
                        Tile tile = Framing.GetTileSafely(topLeftX + i, topLeftY + j);
                        tile.TileFrameX = (short)newFrameX;
                        tile.TileFrameY = (short)newFrameY;
                        tile.Clear(TileDataType.TilePaint);
                        newFrameY += data.CoordinateHeights[j] + data.CoordinatePadding;
                    }
                    newFrameX += data.CoordinateWidth + data.CoordinatePadding;
                }

                for (int i = 0; i < data.Width; ++i)
                {
                    for (int j = 0; j < data.Height; ++j)
                    {
                        WorldGen.SquareTileFrame(topLeftX + i, topLeftY + j);
                    }
                }
            }
        }
    }
}
