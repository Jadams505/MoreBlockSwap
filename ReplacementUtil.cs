using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace MoreBlockSwap
{
    public static class ReplacementUtil
    {
        public static void SingleTileSwap(ushort targetType, int targetStyle, int topLeftX, int topLeftY)
        {
            Tile replaceTile = Framing.GetTileSafely(topLeftX, topLeftY);
            CustomEliminateNaturalExtras(topLeftX, topLeftY, replaceTile.TileType, targetType);

            replaceTile.TileType = targetType;
            replaceTile.Clear(TileDataType.TilePaint);

            if (!WorldGen.CanPoundTile(topLeftX, topLeftY))
            {
                replaceTile.Slope = SlopeType.Solid;
                replaceTile.IsHalfBlock = false;
            }

            WorldGen.SquareTileFrame(topLeftX, topLeftY);
        }

        public static void CustomEliminateNaturalExtras(int x, int y, int swappingFrom, int swappingTo)
        {
            if (WorldGen.InWorld(x, y, 2))
            {
                Tile top = Main.tile[x, y - 1];
                Tile bottom = Main.tile[x, y + 1];

                if (StopBreakage(swappingFrom, swappingTo, top, bottom))
                {
                    return;
                }
                
                if (top.HasTile && (TileID.Sets.ReplaceTileBreakUp[top.TileType] || (top.TileType == 165 && (top.TileFrameY == 36 || top.TileFrameY == 54 || top.TileFrameY == 90))))
                {
                    WorldGen.KillTile(x, y - 1);
                }
                
                if (bottom.HasTile && (TileID.Sets.ReplaceTileBreakDown[bottom.TileType] || (bottom.TileType == 165 && (bottom.TileFrameY == 0 || bottom.TileFrameY == 18 || bottom.TileFrameY == 72))))
                {
                    WorldGen.KillTile(x, y + 1);
                }  
            }
        }

        private static bool StopBreakage(int swappingFrom, int swappingTo, Tile top, Tile bottom)
        {
            return true;
        }

        public static void MultiTileSwap(ushort targetType, int targetStyle, int topLeftX, int topLeftY)
        {
            Tile topLeftTile = Main.tile[topLeftX, topLeftY];
            int oldType = topLeftTile.TileType;
            TileObjectData heldData = TileObjectData.GetTileData(targetType, targetStyle);
            TileObjectData replaceData = TileObjectData.GetTileData(topLeftTile);
            Point newTopLeftFrame = DetermineNewTileStart(targetType, targetStyle, topLeftX, topLeftY, out TileObject placeData);

            if (replaceData.Width == 1 && replaceData.Height == 1)
            {
                ClearSlopeFor1x1Tile(targetType, targetStyle, topLeftX, topLeftY);
            }

            int newFrameX = newTopLeftFrame.X;
            for (int i = 0; i < replaceData.Width; ++i)
            {
                int newFrameY = newTopLeftFrame.Y;
                for (int j = 0; j < replaceData.Height; ++j)
                {
                    Tile tile = Framing.GetTileSafely(topLeftX + i, topLeftY + j);
                    tile.TileType = targetType;
                    tile.TileFrameX = (short)newFrameX;
                    tile.TileFrameY = (short)newFrameY;
                    tile.Clear(TileDataType.TilePaint);
                    newFrameY += heldData.CoordinateHeights[j] + heldData.CoordinatePadding;
                }
                newFrameX += heldData.CoordinateWidth + heldData.CoordinatePadding;
            }

            /* This almost works, multiplayer has issues, just means that you can't place a tile entity tile with block swap
            if(placeData.type != 0 && heldData.HookPostPlaceMyPlayer.hook != null) 
            {
                // Very important for tile entities
                TileObjectData.CallPostPlacementPlayerHook(topLeftX + heldData.Origin.X, topLeftY + heldData.Origin.Y, placeData.type, placeData.style, 1, placeData.alternate, placeData);
            }
            */

            PostSwapCleanUp(oldType, topLeftTile.TileType, topLeftX, topLeftY);

            for (int i = 0; i < replaceData.Width; ++i)
            {
                for (int j = 0; j < replaceData.Height; ++j)
                {
                    WorldGen.SquareTileFrame(topLeftX + i, topLeftY + j);
                }
            }
        }

        public static void PostSwapCleanUp(int oldTileType, int newTileType, int topLeftX, int topLeftY)
        {
            if (Main.tileSign[oldTileType] && !Main.tileSign[newTileType])
            {
                Sign.KillSign(topLeftX, topLeftY);
            }
        }

        // Most 1x1 tiles with TileObjectData cannot be sloped
        // Exceptions: Metal Bars and Platforms
        public static void ClearSlopeFor1x1Tile(ushort targetType, int targetStyle, int topLeftX, int topLeftY)
        {
            Tile replaceTile = Framing.GetTileSafely(topLeftX, topLeftY);

            if(targetType != replaceTile.TileType)
            {
                replaceTile.Slope = SlopeType.Solid;
                replaceTile.IsHalfBlock = false;
            }
        }

        private static Point DetermineNewTileStart(ushort targetType, int targetStyle, int topLeftX, int topLeftY, out TileObject placeData)
        {
            Tile topLeftTile = Main.tile[topLeftX, topLeftY];
            TileObjectData heldData = TileObjectData.GetTileData(targetType, targetStyle);
            placeData = default;

            if (GetCustomTileStart(targetType, targetStyle, topLeftTile) is Point customFrame)
            {
                return customFrame;
            }

            Point newTopLeftFrame = new Point(topLeftTile.TileFrameX, topLeftTile.TileFrameY);
            if (heldData != null)
            {
                int xOffset = heldData != null ? heldData.Origin.X : 0;
                int yOffset = heldData != null ? heldData.Origin.Y : 0;
                bool canPlace = TileObject.CanPlace(topLeftX + xOffset, topLeftY + yOffset, targetType, targetStyle, 0, out placeData, onlyCheck: false, checkStay: true);

                int col = heldData.CalculatePlacementStyle(targetStyle, placeData.alternate, placeData.random);
                int row = 0;

                if (heldData.StyleWrapLimit > 0)
                {
                    row = col.SafeDivide(heldData.StyleWrapLimit) * heldData.StyleLineSkip;
                    col %= heldData.StyleWrapLimit;
                }

                if (heldData.StyleHorizontal)
                {
                    newTopLeftFrame.X = heldData.CoordinateFullWidth * col;
                    newTopLeftFrame.Y = heldData.CoordinateFullHeight * row;
                }
                else
                {
                    newTopLeftFrame.X = heldData.CoordinateFullWidth * row;
                    newTopLeftFrame.Y = heldData.CoordinateFullHeight * col;
                }
            }

            return newTopLeftFrame;
        }

        private static Point? GetCustomTileStart(ushort targetType, int targetStyle, Tile topLeftTile)
        {
            if (targetType == TileID.ClosedDoor && topLeftTile.TileType == TileID.OpenDoor)
            {
                if (BlockSwapUtil.GetPlaceStyleForDoor(topLeftTile, out int doorStyle))
                {
                    int normalizedReplaceStyle = doorStyle % 36;
                    int normalizedTargetStyle = targetStyle % 36;
                    int styleDiff = normalizedTargetStyle - normalizedReplaceStyle;

                    int replaceCol = doorStyle / 36;
                    int targetCol = targetStyle / 36;
                    int colDiff = targetCol - replaceCol;

                    int newTopLeftX = colDiff * 72 + topLeftTile.TileFrameX;
                    int newTopLeftY = styleDiff * 54 + topLeftTile.TileFrameY;

                    return new Point(newTopLeftX, newTopLeftY);
                }
            }
            return null;
        }
    }
}
