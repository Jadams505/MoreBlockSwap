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
        
        //public HashSet<int> TilesThatWork = new HashSet<int>
        //{
        //    TileID.Bottles,
        //    TileID.Tables, // TileID.Tables2 also exists so need to use either Main.tileTable or TileID.Sets.CountsAsTable
        //    TileID.Chairs,
        //    TileID.Anvils, // Should also consider TileID.MythrilAnvil,
        //    TileID.Furnaces, // Various forges
        //    TileID.WorkBenches,
        //    //TileID.Platforms, // Should use Vanilla
        //    //TileID.Containers, // Should use Vanulla
        //    TileID.Candles, // Simliar tiles not considered: Platinum, Water, Peace Candles
        //    TileID.Chandeliers, // Replacing always produces lit version, good enough for now
        //    TileID.Jackolanterns,
        //    TileID.Presents,
        //    TileID.HangingLanterns, // Same issues as candles,
        //    TileID.WaterCandle, // Merge with candles
        //    TileID.Books, // Test water bolt,
        //    TileID.Hellforge, // Merge with furnace
        //    TileID.ClayPot,
        //    TileID.Beds, // Always replaces to same direction as placed tile,
        //    TileID.Coral,
        //    TileID.ImmatureHerbs, // Gives you back the seed, not vanilla but possibly better
        //    TileID.Tombstones,
        //    TileID.Loom,
        //    TileID.Pianos,
        //    //TileID.Dressers, // Vanilla
        //    TileID.Benches,
        //    TileID.Bathtubs, // Same as beds
        //    TileID.Banners,
        //    TileID.CookingPots,
        //    TileID.Candelabras, // Platinum version not included
        //    TileID.Bookcases,
        //    TileID.Thrones,
        //    TileID.Bowls,
        //    TileID.GrandfatherClocks,
        //    TileID.Sawmill,
        //    TileID.TinkerersWorkbench,
        //    TileID.CrystalBall,
        //    TileID.DiscoBall,
        //    TileID.AdamantiteForge,
        //    TileID.MythrilAnvil,
        //    TileID.PressurePlates,
        //    TileID.Switches, // Lets you replace with the side anchored version, fix later
        //    TileID.Boulder,
        //    TileID.MusicBoxes,
        //    TileID.Explosives,
        //    TileID.InletPump, // Should make pumps swap with each other
        //    TileID.OutletPump,
        //    TileID.Timers,
        //    TileID.ChristmasTree,
        //    TileID.Sinks,
        //    TileID.PlatinumCandelabra,
        //    TileID.PlatinumCandle,
        //    TileID.WaterFountain,
        //    TileID.LandMine,
        //    TileID.SnowballLauncher,
        //    TileID.Firework,
        //    TileID.Blendomatic,
        //    TileID.MeatGrinder,
        //    TileID.Extractinator,
        //    TileID.Solidifier,
        //    TileID.DyePlants, // Swapping with bloodroot should be stopped
        //    TileID.DyeVat,
        //    TileID.Teleporter,
        //    TileID.LihzahrdAltar,
        //    TileID.MetalBars,
        //    TileID.Painting3X3,
        //    TileID.Painting4X3,
        //    TileID.Painting6X4,
        //    TileID.ImbuingStation,
        //    TileID.Painting2X3,
        //    TileID.Painting3X2,
        //    TileID.Autohammer,
        //    TileID.Pumpkins, // Same as immature herbs
        //    TileID.FireflyinaBottle, // Should swap with other hanging bottles
        //    TileID.LightningBuginaBottle,
        //    TileID.BunnyCage, // Should swap with same sized cages
        //    TileID.SquirrelCage,
        //    TileID.MallardDuckCage,
        //    TileID.DuckCage,
        //    TileID.BirdCage,
        //    TileID.BlueJay,
        //    TileID.CardinalCage,
        //    TileID.FishBowl,
        //    TileID.HeavyWorkBench,
        //    TileID.SnailCage,
        //    TileID.GlowingSnailCage,
        //    TileID.AmmoBox,
        //    TileID.MonarchButterflyJar,
        //    TileID.PurpleEmperorButterflyJar,
        //    TileID.RedAdmiralButterflyJar,
        //    TileID.UlyssesButterflyJar,
        //    TileID.SulphurButterflyJar,
        //    TileID.TreeNymphButterflyJar,
        //    TileID.ZebraSwallowtailButterflyJar,
        //    TileID.JuliaButterflyJar,
        //    TileID.ScorpionCage,
        //    TileID.BlackScorpionCage,
        //    TileID.FrogCage,
        //    TileID.MouseCage,
        //    TileID.BoneWelder,
        //    TileID.FleshCloningVat,
        //    TileID.GlassKiln,
        //    TileID.LihzahrdFurnace,
        //    TileID.LivingLoom,
        //    TileID.SkyMill,
        //    TileID.IceMachine,
        //    TileID.SteampunkBoiler,
        //    TileID.HoneyDispenser,
        //    TileID.PenguinCage,
        //    TileID.WormCage,
        //    TileID.BlueJellyfishBowl,
        //    TileID.GreenJellyfishBowl,
        //    TileID.PinkJellyfishBowl,
        //    TileID.ShipInABottle,
        //    TileID.SeaweedPlanter,
        //    TileID.ShellPile, // Should swap amongst each other
        //    TileID.FireworksBox,
        //    TileID.AlphabetStatues, // Should swap with other statues
        //    TileID.FireworkFountain,
        //    TileID.GrasshopperCage,
        //    TileID.BewitchingTable,
        //    TileID.AlchemyTable,
        //    TileID.Sundial, // 1.4.4 Moon dial
        //    TileID.GoldBirdCage,
        //    TileID.GoldBunnyCage,
        //    TileID.GoldButterflyCage,
        //    TileID.GoldFrogCage,
        //    TileID.GoldGrasshopperCage,
        //    TileID.GoldMouseCage,
        //    TileID.GoldWormCage,
        //    TileID.PeaceCandle,
        //    TileID.FishingCrate,
        //    TileID.SharpeningStation,
        //    TileID.TargetDummy,
        //    TileID.TrapdoorOpen,
        //    TileID.TallGateOpen,
        //    TileID.LavaLamp,
        //    TileID.CageEnchantedNightcrawler,
        //    TileID.CageBuggy,
        //    TileID.CageGrubby,
        //    TileID.CageSluggy,
        //    TileID.LunarMonolith, // Other monoliths
        //    TileID.Detonator,
        //    TileID.LunarCraftingStation,
        //    TileID.SquirrelOrangeCage,
        //    TileID.SquirrelGoldCage,
        //    TileID.LogicGateLamp,
        //    TileID.LogicGate,
        //    TileID.WeightedPressurePlate,
        //    TileID.WireBulb,
        //    TileID.FakeContainers, // FakeContainers2
        //    TileID.ProjectilePressurePad,
        //    TileID.BeeHive,
        //    TileID.SillyBalloonMachine,
        //    TileID.Pigronata,
        //    TileID.PartyMonolith,
        //    TileID.PartyBundleOfBalloonTile,
        //    TileID.PartyPresent,
        //    TileID.DjinnLamp, // Similar to bowls
        //    TileID.DefendersForge,
        //    TileID.WarTable,
        //    TileID.WarTableBanner,
        //    TileID.ElderCrystalStand,
        //    //TileID.Containers2, // Vanilla
        //    TileID.FakeContainers2, // Use TileID.Sets.BasicChestFake
        //    TileID.Tables2,
        //    TileID.BloodMoonMonolith, // Merge with other monoliths
        //    TileID.RollingCactus, // Maybe merge with boulders
        //    TileID.AntlionLarva,
        //    TileID.DrumSet,
        //    TileID.PicnicTable,
        //    TileID.PinWheel,
        //    TileID.WeatherVane,
        //    TileID.VoidVault,
        //    TileID.GolfCupFlag,
        //    TileID.Toilets,
        //    TileID.LesionStation,
        //    TileID.GoldGoldfishBowl,
        //    TileID.CatBast,
        //    TileID.VoidMonolith, // Other monoliths
        //    TileID.FoodPlatter, // Facing right gives item back
        //    TileID.BlackDragonflyJar,
        //    TileID.BlueDragonflyJar,
        //    TileID.GreenDragonflyJar,
        //    TileID.OrangeDragonflyJar,
        //    TileID.RedDragonflyJar,
        //    TileID.YellowDragonflyJar,
        //    TileID.GoldDragonflyJar,
        //    TileID.YellowDragonflyJar,
        //    TileID.BoulderStatue, // could only be swapped with other statues if there was a roof
        //    TileID.MaggotCage,
        //    TileID.RatCage,
        //    TileID.LadybugCage,
        //    TileID.OwlCage,
        //    TileID.PupfishBowl,
        //    TileID.GoldLadybugCage,
        //    TileID.LawnFlamingo, // swapping should not be possible but no items are lost
        //    TileID.PottedPlants1,
        //    TileID.PottedPlants2,
        //    TileID.TurtleCage,
        //    TileID.TurtleJungleCage,
        //    TileID.GrebeCage,
        //    TileID.SeagullCage,
        //    TileID.WaterStriderCage,
        //    TileID.GoldWaterStriderCage,
        //    TileID.SeahorseCage,
        //    TileID.GoldSeahorseCage,
        //    TileID.GolfTrophies,
        //    TileID.PlasmaLamp,
        //    TileID.FogMachine,
        //    TileID.GardenGnome,
        //    TileID.PinkFairyJar,
        //    TileID.GreenFairyJar,
        //    TileID.BlueFairyJar,
        //    TileID.SoulBottles,
        //    TileID.RockGolemHead, // swapping should not be possible
        //    TileID.HellButterflyJar,
        //    TileID.LavaflyinaBottle,
        //    TileID.MagmaSnailCage,
        //    TileID.HangingLanterns,
        //    TileID.BrazierSuspended, // merge with hanging lanterns
        //    TileID.VolcanoSmall,
        //    TileID.VolcanoLarge,
        //    TileID.VanityTreeSakuraSaplings,
        //    TileID.LavafishBowl,
        //    TileID.AmethystBunnyCage,
        //    TileID.TopazBunnyCage,
        //    TileID.SapphireBunnyCage,
        //    TileID.EmeraldBunnyCage,
        //    TileID.RubyBunnyCage,
        //    TileID.DiamondBunnyCage,
        //    TileID.AmberBunnyCage,
        //    TileID.AmethystSquirrelCage,
        //    TileID.TopazSquirrelCage,
        //    TileID.SapphireSquirrelCage,
        //    TileID.EmeraldSquirrelCage,
        //    TileID.RubySquirrelCage,
        //    TileID.DiamondSquirrelCage,
        //    TileID.AmberSquirrelCage,
        //    TileID.PottedLavaPlants, // merge with PottedPlants1
        //    TileID.PottedLavaPlantTendrils, // merge with PottedPlants2
        //    TileID.VanityTreeWillowSaplings,
        //    TileID.MasterTrophyBase,
        //    TileID.TruffleWormCage,
        //    TileID.EmpressButterflyJar,
        //    TileID.SliceOfCake,
        //    TileID.TeaKettle,
        //    TileID.PottedCrystalPlants, // merge with PottedPlants1
        //};

        public static HashSet<int> TilesThatDontWork = new HashSet<int>
        {
            TileID.ClosedDoor, // Doors have multiple frames that interfere also open doors need to be handled specifically
            TileID.Saplings, // Lets you replace always and consumes item probably need to do something with random place styles
            TileID.Signs, // Consumes item
            TileID.Statues, // Facing right does not drop item
            TileID.Lever, // Same as signs
            TileID.Cannon, // Switching portal color causes no item to drop
            TileID.Campfire, // Wait for 1.4.4
            TileID.BubbleMachine, // Fails when toggled off
            TileID.MushroomStatue, // Same as statues
            TileID.ItemFrame, // Same as signs
            TileID.Fireplace, // Same as bubble machine
            TileID.Chimney, // Same as fire place
            TileID.LogicSensor, // Since it is a tile entity it needs special handling when swapped
            TileID.AnnouncementBox, // Same as signs
            TileID.GemLocks, // Kills the gem inside
            TileID.GeyserTrap, // Same as signs
            TileID.SillyBalloonTile, // Same as signs
            TileID.DisplayDoll, // Same as signs
            TileID.WeaponsRack2, // Sames as signs
            TileID.HatRack, // Same as signs
            TileID.ArrowSign, // Sames as signs
            TileID.PaintedArrowSign, // merge with arrow sign
            TileID.Sandcastles, // Drops a sandcastle bucket instead of nothing/ even better sand
            TileID.TatteredWoodSign, // Same as signs
            TileID.GemSaplings, // Same as saplings
            TileID.TeleportationPylon, // Needs checking for only one in the world, plus tile entity handling
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
                /*targetType == TileID.Campfire || replaceType == TileID.Campfire ||*/
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
                if (TilesThatDontWork.Contains(heldTile))
                {
                    return false;
                }

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