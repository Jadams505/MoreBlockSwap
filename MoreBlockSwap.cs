using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MoreBlockSwap
{
    public class MoreBlockSwap : Mod
    {
        public HashSet<int> TilesThatWork = new HashSet<int>
        {
            TileID.Bottles,
            TileID.Tables, // TileID.Tables2 also exists so need to use either Main.tileTable or TileID.Sets.CountsAsTable
            TileID.Chairs,
            TileID.Anvils, // Should also consider TileID.MythrilAnvil,
            TileID.Furnaces, // Various forges
            TileID.WorkBenches,
            //TileID.Platforms, // Should use Vanilla
            //TileID.Containers, // Should use Vanulla
            TileID.Candles, // Simliar tiles not considered: Platinum, Water, Peace Candles
            TileID.Chandeliers, // Replacing always produces lit version, good enough for now
            TileID.Jackolanterns,
            TileID.Presents,
            TileID.HangingLanterns, // Same issues as candles,
            TileID.WaterCandle, // Merge with candles
            TileID.Books, // Test water bolt,
            TileID.Hellforge, // Merge with furnace
            TileID.ClayPot,
            TileID.Beds, // Always replaces to same direction as placed tile,
            TileID.Coral,
            TileID.ImmatureHerbs, // Gives you back the seed, not vanilla but possibly better
            TileID.Tombstones,
            TileID.Loom,
            TileID.Pianos,
            //TileID.Dressers, // Vanilla
            TileID.Benches,
            TileID.Bathtubs, // Same as beds
            TileID.Banners,
            TileID.CookingPots,



            TileID.Count
        };

        public HashSet<int> TilesThatDontWork = new HashSet<int>
        {
            TileID.ClosedDoor, // Doors have multiple frames that interfere also open doors need to be handled specifically
            TileID.Saplings, // Lets you replace always and consumes item probably need to do something with random place styles
            TileID.Signs, // Consumes item


            TileID.Count
        };

        public override void Load()
        {
            On.Terraria.WorldGen.WouldTileReplacementWork += WorldGen_WouldTileReplacementWork;
            On.Terraria.WorldGen.MoveReplaceTileAnchor += WorldGen_MoveReplaceTileAnchor;
            On.Terraria.WorldGen.KillTile_GetItemDrops += WorldGen_KillTile_GetItemDrops;
            On.Terraria.WorldGen.ReplaceTIle_DoActualReplacement += WorldGen_ReplaceTIle_DoActualReplacement;
            On.Terraria.Player.PlaceThing_ValidTileForReplacement += Player_PlaceThing_ValidTileForReplacement;
        }

        public override void Unload()
        {
            On.Terraria.WorldGen.WouldTileReplacementWork -= WorldGen_WouldTileReplacementWork;
            On.Terraria.WorldGen.MoveReplaceTileAnchor -= WorldGen_MoveReplaceTileAnchor;
            On.Terraria.WorldGen.KillTile_GetItemDrops -= WorldGen_KillTile_GetItemDrops;
            On.Terraria.WorldGen.ReplaceTIle_DoActualReplacement -= WorldGen_ReplaceTIle_DoActualReplacement;
            On.Terraria.Player.PlaceThing_ValidTileForReplacement -= Player_PlaceThing_ValidTileForReplacement;
        }

        private bool WorldGen_WouldTileReplacementWork(On.Terraria.WorldGen.orig_WouldTileReplacementWork orig, ushort attemptingToReplaceWith, int x, int y)
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

        private void WorldGen_MoveReplaceTileAnchor(On.Terraria.WorldGen.orig_MoveReplaceTileAnchor orig, ref int x, ref int y, ushort targetType, Tile t)
        {
            if(ShouldVanillaHandleSwap(targetType, t))
            {
                orig(ref x, ref y, targetType, t);
                return;
            }
            TileObjectData data = TileObjectData.GetTileData(t);
            if(data != null)
            {
                int xAdjustment = t.TileFrameX % data.CoordinateFullWidth / (data.CoordinateWidth + data.CoordinatePadding);
                int yAdjustment = t.TileFrameY % data.CoordinateFullHeight / (data.CoordinateHeights[0] + data.CoordinatePadding);
                x -= xAdjustment;
                y -= yAdjustment;
            }
        }

        private void WorldGen_KillTile_GetItemDrops(On.Terraria.WorldGen.orig_KillTile_GetItemDrops orig, int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops)
        {
            orig(x, y, tileCache, out dropItem, out dropItemStack, out secondaryItem, out secondaryItemStack, includeLargeObjectDrops);
            if(dropItem > 0)
            {
                return;
            }
            if (includeLargeObjectDrops)
            {
                TileObjectData data = TileObjectData.GetTileData(tileCache);
                if (data != null)
                {
                    int style = GetItemPlaceStyleFromTile(tileCache);
                    int drop = GetItemDrop(tileCache.TileType, style);
                    if (drop != -1)
                    {
                        dropItem = drop;
                    }
                }
            }
        }

        private void WorldGen_ReplaceTIle_DoActualReplacement(On.Terraria.WorldGen.orig_ReplaceTIle_DoActualReplacement orig, ushort targetType, int targetStyle, int topLeftX, int topLeftY, Tile t)
        {
            if (ShouldVanillaHandleSwap(targetType, t))
            {
                orig(targetType, targetStyle, topLeftX, topLeftY, t);
                return;
            }
            TileObjectData data = TileObjectData.GetTileData(t);
            if(data != null)
            {
                bool canPlace = TileObject.CanPlace(topLeftX, topLeftY, targetType, targetStyle, 0, out TileObject placeData, onlyCheck: false, checkStay: true);

                int newTopLeftX;
                int newTopLeftY;
                int style = data.CalculatePlacementStyle(targetStyle, 0, placeData.random);
                //style -= data.Style;
                int adjustedStyle = 0;

                if(data.StyleWrapLimit > 0)
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

                Tile topLeftTile = Framing.GetTileSafely(topLeftX, topLeftY);
                int deltaX = newTopLeftX - topLeftTile.TileFrameX;
                int deltaY = newTopLeftY - topLeftTile.TileFrameY;

                for(int i = 0; i < data.Width; ++i)
                {
                    for(int j = 0; j < data.Height; ++j)
                    {
                        Tile tile = Framing.GetTileSafely(topLeftX + i, topLeftY + j);
                        tile.TileFrameX += (short)deltaX;
                        tile.TileFrameY += (short)deltaY;
                    }
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

        private static bool ShouldVanillaHandleSwap(int targetType, Tile tileToReplace)
        {
            TileObjectData toReplaceData = TileObjectData.GetTileData(tileToReplace);
            TileObjectData heldTileData = TileObjectData.GetTileData(targetType, 0);
            int replaceType = tileToReplace.TileType;
            return toReplaceData == null || heldTileData == null ||
                TileID.Sets.BasicChest[targetType] || TileID.Sets.BasicChest[replaceType] ||
                TileID.Sets.BasicDresser[targetType] || TileID.Sets.BasicDresser[replaceType] || 
                targetType == TileID.Campfire || replaceType == TileID.Campfire ||
                TileID.Sets.Platforms[targetType] || TileID.Sets.Platforms[replaceType] ||
                TileID.Sets.Torch[targetType] || TileID.Sets.Torch[replaceType];
        }

        private bool Player_PlaceThing_ValidTileForReplacement(On.Terraria.Player.orig_PlaceThing_ValidTileForReplacement orig, Player self)
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
                TileObjectData data = TileObjectData.GetTileData(tileToReplace);
                if (data == null)
                {
                    return false;
                }

                int tileToReplaceItemPlaceStyle = GetItemPlaceStyleFromTile(tileToReplace);
                if (data.RandomStyleRange > 0)
                {
                    return placeStyle == tileToReplaceItemPlaceStyle; // allows for replacement into a different random style
                }
                return placeStyle != tileToReplaceItemPlaceStyle;
            }
            return false;
        }

        public static int GetItemDrop(int targetTileId, int targetStyle)
        {
            for(int i = 0; i < ItemID.Count; ++i)
            {
                Item item = new Item();
                item.SetDefaults(i);
                if(item.createTile == targetTileId && item.placeStyle == targetStyle)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int GetItemPlaceStyleFromTile(Tile tile)
        {
            TileObjectData data = TileObjectData.GetTileData(tile);
            if(data == null)
            {
                return 0;
            }

            int tileObjectStyle = TileObjectData.GetTileStyle(tile);
            int placementStyle = data.CalculatePlacementStyle(tileObjectStyle, 0, 0);
            int calculatedStyle = placementStyle;

            if (data.RandomStyleRange > 0)
            {
                return placementStyle / data.RandomStyleRange; // normalize random style
            }

            if(data.StyleMultiplier > 1)
            {
                return tileObjectStyle;
            }

            int swl = data.StyleWrapLimit;

            if(swl > 0)
            {
                int startStyle = placementStyle / (swl * data.StyleLineSkip) * swl;
                int styleOffset = placementStyle % swl;

                return startStyle + styleOffset;
            }

            int col = tile.TileFrameX / data.CoordinateFullWidth;
            int row = tile.TileFrameY / data.CoordinateFullHeight;

            return data.StyleHorizontal ? col : row;
            //return calculatedStyle;
        }

        public static int GetStyleFromTile(Tile getTile)
        {
            TileObjectData data = TileObjectData.GetTileData(getTile);
            if (data == null)
            {
                return -1;
            }

            int col = getTile.TileFrameX / data.CoordinateFullWidth;
            int row = getTile.TileFrameY / data.CoordinateFullHeight;
            int swl = data.StyleWrapLimitVisualOverride ?? data.StyleWrapLimit;
            if (swl == 0)
            {
                swl = 1;
            }

            int style = data.StyleHorizontal ? (row * swl + col) : (col * swl + row);
            return style / data.StyleMultiplier;
        }
    }
}