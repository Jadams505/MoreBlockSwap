using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MoreBlockSwap
{
    public static class BlockSwapUtil
    {
        /// <summary>
        /// Used to determine what item should drop from a given tile
        /// </summary>
        /// <param name="tile">The tile in the world that is going to be replaced and must drop an item</param>
        /// <returns>The placeStyle of the item that is going to drop from the tile</returns>
        public static int GetItemPlaceStyleFromTile(Tile tile)
        {
            TileObjectData data = TileObjectData.GetTileData(tile);
            if (data == null)
            {
                return 0;
            }

            int customStyle = GetCustomPlaceStyleFromTile(tile);
            if (customStyle != -1)
            {
                return customStyle;
            }

            int tileObjectStyle = TileObjectData.GetTileStyle(tile);
            int placementStyle = data.CalculatePlacementStyle(tileObjectStyle, 0, 0);

            if (data.RandomStyleRange > 0)
            {
                return placementStyle / data.RandomStyleRange; // normalize random style
            }

            if (data.StyleMultiplier > 1)
            {
                return tileObjectStyle;
            }

            int swl = data.StyleWrapLimit;

            if (swl > 0)
            {
                int startStyle = swl * placementStyle.SafeDivide(swl * data.StyleLineSkip);
                int styleOffset = placementStyle % swl;

                return startStyle + styleOffset;
            }

            int frameX = tile.TileFrameX;
            int frameY = tile.TileFrameY;
            int col = frameX.SafeDivide(data.CoordinateFullWidth);
            int row = frameY.SafeDivide(data.CoordinateFullHeight);

            return data.StyleHorizontal ? col : row;
        }

        public static bool GetPlaceStyleForDoor(Tile tile, out int style)
        {
            TileObjectData data = TileObjectData.GetTileData(tile);
            ModTile mTile = TileLoader.GetTile(tile.TileType);
            int frameX = tile.TileFrameX;
            int frameY = tile.TileFrameY;

            if ((mTile != null && mTile.OpenDoorID != -1) || tile.TileType == TileID.ClosedDoor)
            {
                int row = frameY.SafeDivide(data.CoordinateFullHeight);
                int col = frameX.SafeDivide(data.CoordinateFullWidth * 3);

                style =  row + col * data.StyleWrapLimit;
                return true;
            }
            else if ((mTile != null && mTile.CloseDoorID != -1) || tile.TileType == TileID.OpenDoor)
            {
                int row = frameY.SafeDivide(data.CoordinateFullHeight);
                int col = frameX.SafeDivide(data.CoordinateFullWidth * 2);
                style =  row + col * 36;
                return true;
            }
            style = 0;
            return false;
        }

        public static int GetCustomPlaceStyleFromTile(Tile tile)
        {
            TileObjectData data = TileObjectData.GetTileData(tile);
            int tileObjectStyle = 0, alterante = -1;
            TileObjectData.GetTileInfo(tile, ref tileObjectStyle, ref alterante);
            int calculatedStyle = data.CalculatePlacementStyle(tileObjectStyle, alterante, 0);

            if(GetPlaceStyleForDoor(tile, out int doorStyle))
            {
                return doorStyle;
            }

            int frameX = tile.TileFrameX;
            return tile.TileType switch
            {
                TileID.Saplings => 0, // makes all saplings drop acorns
                TileID.Statues => calculatedStyle % 165,
                TileID.Cannon => Math.Clamp(frameX.SafeDivide(data.CoordinateFullWidth), 0, 3),
                TileID.SillyBalloonTile => 2 * frameX.SafeDivide(data.CoordinateFullWidth * 2), // Purple = 0, Green = 2, Pink = 4
                TileID.Campfire => frameX.SafeDivide(data.CoordinateFullWidth), // Remove with 1.4.4

                TileID.GeyserTrap => 0,
                TileID.BubbleMachine => 0,
                TileID.MushroomStatue => 0,
                TileID.ItemFrame => 0,
                TileID.Fireplace => 0,
                TileID.Chimney => 0,
                TileID.Signs => 0,
                TileID.AnnouncementBox => 0,
                TileID.ArrowSign => 0,
                TileID.PaintedArrowSign => 0,
                TileID.TatteredWoodSign => 0,
                TileID.Lever => 0,
                TileID.CatBast => 0,
                TileID.LawnFlamingo => 0,
                TileID.SnowballLauncher => 0,
                TileID.Lampposts => 0,

                _ => -1
            };
        }

        public static bool ShouldVanillaHandleSwap(int targetType, Tile tileToReplace)
        {
            TileObjectData toReplaceData = TileObjectData.GetTileData(tileToReplace);
            TileObjectData heldTileData = TileObjectData.GetTileData(targetType, 0);
            int replaceType = tileToReplace.TileType;
            return toReplaceData == null || heldTileData == null ||
                (TileID.Sets.BasicChest[targetType] && TileID.Sets.BasicChest[replaceType]) ||
                (TileID.Sets.BasicDresser[targetType] && TileID.Sets.BasicDresser[replaceType]) ||
                /*(targetType == TileID.Campfire && replaceType == TileID.Campfire) ||*/
                (TileID.Sets.Platforms[targetType] && TileID.Sets.Platforms[replaceType]) ||
                (TileID.Sets.Torch[targetType] && TileID.Sets.Torch[replaceType]);
        }

        public static bool IsConversionCase(int replaceTile, int heldTile, out int swapTileId, out int drop)
        {
            swapTileId = heldTile;
            drop = 0;
            if(replaceTile != heldTile)
            {
                if (replaceTile == TileID.GolfGrass && heldTile == TileID.HallowedGrass)
                {
                    swapTileId = TileID.GolfGrassHallowed;
                    return true;
                }

                if (replaceTile == TileID.GolfGrassHallowed && heldTile == TileID.Grass)
                {
                    swapTileId = TileID.GolfGrass;
                    return true;
                }

                if (TileID.Sets.Conversion.Grass[replaceTile] && TileID.Sets.Conversion.Grass[heldTile])
                {
                    if ((replaceTile == TileID.GolfGrassHallowed && heldTile == TileID.HallowedGrass) || // can't swap mowed hallow and hallow grass
                        (replaceTile == TileID.GolfGrass && heldTile == TileID.Grass) || // can't swap mowed and regular grass
                        (replaceTile == TileID.JungleGrass || heldTile == TileID.JungleGrass)) // can't swap any dirt based grass with jungle grass
                    {
                        return false;
                    }
                    return true;
                }

                if ((replaceTile == TileID.JungleGrass && heldTile == TileID.MushroomGrass) ||
                    (replaceTile == TileID.MushroomGrass && heldTile == TileID.JungleGrass))
                {
                    return true;
                }

                if (TileID.Sets.Conversion.Moss[replaceTile])
                {
                    if (TileID.Sets.Conversion.Moss[heldTile])
                    {
                        return true;
                    }

                    if(heldTile == TileID.GrayBrick)
                    {
                        swapTileId = MossConversion(replaceTile);
                        drop = ItemID.StoneBlock;
                        return swapTileId != -1;
                    }
                }

                if (TileID.Sets.Conversion.MossBrick[replaceTile])
                {
                    if (TileID.Sets.Conversion.Moss[heldTile])
                    {
                        swapTileId = MossConversion(heldTile);
                        return swapTileId != -1 && replaceTile != swapTileId;
                    }

                    if(heldTile == TileID.Stone)
                    {
                        swapTileId = MossConversion(replaceTile);
                        drop = ItemID.GrayBrick;
                        return swapTileId != -1;
                    }
                }
            }
            return false;
        }

        private static int MossConversion(int mossTileId)
        {
            return mossTileId switch
            {
                182 => 515,
                515 => 182,
                180 => 513,
                513 => 180,
                179 => 512,
                512 => 179,
                381 => 517,
                517 => 381,
                534 => 535,
                535 => 534,
                536 => 537,
                537 => 536,
                539 => 540,
                540 => 539,
                625 => 626,
                626 => 625,
                627 => 628,
                628 => 627,
                183 => 516,
                516 => 183,
                181 => 514,
                514 => 181,
                _ => -1,
            };
        }

        public static Point TopLeftOfMultiTile(int tilePosX, int tilePosY, Tile tile)
        {
            TileObjectData data = TileObjectData.GetTileData(tile);
            if (data != null)
            {
                int frameX = tile.TileFrameX;
                int frameY = tile.TileFrameY;

                int xAdjustment = frameX.SafeMod(data.CoordinateFullWidth).SafeDivide(data.CoordinateWidth + data.CoordinatePadding);
                int yAdjustment = 0;

                int yInnerFrame = frameY.SafeMod(data.CoordinateFullHeight);
                while(yAdjustment < data.CoordinateHeights.Length && yInnerFrame > 0)
                {
                    yInnerFrame -= (data.CoordinateHeights[yAdjustment] + data.CoordinatePadding);
                    yAdjustment++;
                }

                tilePosX -= xAdjustment;
                tilePosY -= yAdjustment;
            }
            return new Point(tilePosX, tilePosY);
        }

        // This assumes that tile entites are placed in the top left of multi-tiles
        // But even vanilla doesn't match this pattern (weapon rack, and item frames)
        public static Point16 TileEntityCoordinates(int tileCoordX, int tileCoordY)
        {
            Tile tile = Framing.GetTileSafely(tileCoordX, tileCoordY);
            Point topLeft = TopLeftOfMultiTile(tileCoordX, tileCoordY, tile);

            return new Point16(topLeft.X, topLeft.Y);
        }

        public static int SafeDivide(this int dividend, int divisor)
        {
            if(divisor == 0)
            {
                return dividend;
            }
            return dividend / divisor;
        }

        public static int SafeMod(this int lhs, int rhs)
        {
            if(rhs == 0)
            {
                return lhs;
            }
            return lhs % rhs;
        }
    }
}
