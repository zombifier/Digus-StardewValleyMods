﻿using System;
using System.Collections.Generic;
using System.Linq;
using AnimalHusbandryMod.animals;
using AnimalHusbandryMod.animals.data;
using AnimalHusbandryMod.common;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Tools;
using DataLoader = AnimalHusbandryMod.common.DataLoader;

namespace AnimalHusbandryMod.tools
{
    public class ParticipantRibbonOverrides : ToolOverridesBase
    {
        public const string ParticipantRibbonItemId = "DIGUS.ANIMALHUSBANDRYMOD.ParticipantRibbon";
        internal const string ParticipantRibbonKey = "DIGUS.ANIMALHUSBANDRYMOD/ParticipantRibbon";

        internal static readonly Dictionary<string, FarmAnimal> Animals = new Dictionary<string, FarmAnimal>();
        internal static readonly Dictionary<string, Pet> Pets = new Dictionary<string, Pet>();

        public static bool GetOneNew(GenericTool __instance, ref Item __result)
        {
            if (!IsParticipantRibbon(__instance)) return true;

            __result = (Item)ToolsFactory.GetParticipantRibbon();
            return false;
        }

        public static void loadDisplayName(GenericTool __instance, ref string __result)
        {
            if (!IsParticipantRibbon(__instance)) return;

            __result = DataLoader.i18n.Get("Tool.ParticipantRibbon.Name");
        }

        public static void loadDescription(GenericTool __instance, ref string __result)
        {
            if (!IsParticipantRibbon(__instance)) return;

            __result = DataLoader.i18n.Get("Tool.ParticipantRibbon.Description");
        }

        public static void canBeTrashed(Item __instance, ref bool __result)
        {
            if (!IsParticipantRibbon(__instance)) return;

            __result = true;
        }

        public static void CanAddEnchantment(Tool __instance, ref bool __result)
        {
            if (!IsParticipantRibbon(__instance)) return;

            __result = false;
        }

        public static bool beginUsing(GenericTool __instance, GameLocation location, int x, int y, StardewValley.Farmer who, ref bool __result)
        {
            if (!IsParticipantRibbon(__instance)) return true;

            try
            {
                string participantRibbonId = __instance.modData[ParticipantRibbonKey];

                x = (int)who.GetToolLocation(false).X;
                y = (int)who.GetToolLocation(false).Y;
                Rectangle rectangle = new Rectangle(x - Game1.tileSize / 2, y - Game1.tileSize / 2, Game1.tileSize, Game1.tileSize);

                if (!DataLoader.ModConfig.DisableAnimalContest)
                {
                    if (location is Farm farm)
                    {
                        foreach (FarmAnimal farmAnimal in farm.animals.Values)
                        {
                            if (farmAnimal.GetBoundingBox().Intersects(rectangle))
                            {
                                Animals[participantRibbonId] = farmAnimal;
                                break;
                            }
                        }
                        if (!Animals.ContainsKey(participantRibbonId) || Animals[participantRibbonId] == null)
                        {
                            foreach (Pet localPet in farm.characters.Where(i => i is Pet))
                            {
                                if (localPet.GetBoundingBox().Intersects(rectangle))
                                {
                                    Pets[participantRibbonId] = localPet;
                                    break;
                                }
                            }
                        }
                    }
                    else if (location is AnimalHouse animalHouse)
                    {
                        foreach (FarmAnimal farmAnimal in animalHouse.animals.Values)
                        {
                            if (farmAnimal.GetBoundingBox().Intersects(rectangle))
                            {
                                Animals[participantRibbonId] = farmAnimal;
                                break;
                            }
                        }
                    }
                    else if (location is FarmHouse)
                    {
                        foreach (Pet localPet in location.characters.Where(i => i is Pet))
                        {
                            if (localPet.GetBoundingBox().Intersects(rectangle))
                            {
                                Pets[participantRibbonId] = localPet;
                                break;
                            }
                        }
                    }
                }

                string dialogue = "";
                Animals.TryGetValue(participantRibbonId, out FarmAnimal animal);
                if (animal != null)
                {
                    if (animal.isBaby())
                    {
                        dialogue = DataLoader.i18n.Get("Tool.ParticipantRibbon.CantBeBaby");
                    }
                    else if (AnimalContestController.HasParticipated(animal))
                    {
                        dialogue = DataLoader.i18n.Get("Tool.ParticipantRibbon.HasAlreadyParticipatedContest", new { animalName = animal.displayName });
                    }
                    else if (AnimalContestController.IsNextParticipant(animal))
                    {
                        dialogue = DataLoader.i18n.Get("Tool.ParticipantRibbon.IsAlreadyParticipant", new { animalName = animal.displayName });
                    }
                    else if (AnimalContestController.GetNextContestParticipant() is { } participant)
                    {
                        string participantName = participant.displayName;
                        dialogue = DataLoader.i18n.Get("Tool.ParticipantRibbon.AnotherParticipantAlready", new { participantName });
                    }
                    else
                    {
                        animal.doEmote(8, true);
                        if (who != null && Game1.player.Equals(who))
                        {
                            animal.makeSound();
                        }
                        animal.pauseTimer = 200;
                    }
                }
                Pets.TryGetValue(participantRibbonId, out Pet pet);
                if (pet != null)
                {
                    if (AnimalContestController.IsNextParticipant(pet))
                    {
                        dialogue = DataLoader.i18n.Get("Tool.ParticipantRibbon.IsAlreadyParticipant",
                            new { animalName = pet.displayName });
                    }
                    else if (AnimalContestController.GetNextContestParticipant() is Character character)
                    {
                        string participantName = character.displayName;
                        dialogue = DataLoader.i18n.Get("Tool.ParticipantRibbon.AnotherParticipantAlready", new { participantName });
                    }
                    else
                    {
                        pet.doEmote(8, true);
                        if (who != null && Game1.player.Equals(who))
                        {
                            pet.playContentSound();
                        }
                        pet.Halt();
                        pet.CurrentBehavior = "SitDown";
                        pet.Halt();
                        pet.Sprite.setCurrentAnimation(
                            new List<FarmerSprite.AnimationFrame>() { new FarmerSprite.AnimationFrame(18, 200) });

                    }
                }
                if (dialogue.Length > 0)
                {
                    if (who != null && Game1.player.Equals(who))
                    {
                        DelayedAction.showDialogueAfterDelay(dialogue, 150);
                    }

                    Pets[participantRibbonId] = pet = null;
                    Animals[participantRibbonId] = animal = null;
                }

                who.Halt();
                int currentFrame = who.FarmerSprite.CurrentFrame;
                if (animal != null || pet != null)
                {
                    switch (who.FacingDirection)
                    {
                        case 0:
                            who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1] { new FarmerSprite.AnimationFrame(62, 200, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true) });
                            break;
                        case 1:
                            who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1] { new FarmerSprite.AnimationFrame(58, 200, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true) });
                            break;
                        case 2:
                            who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1] { new FarmerSprite.AnimationFrame(54, 200, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true) });
                            break;
                        case 3:
                            who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1] { new FarmerSprite.AnimationFrame(58, 200, false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true) });
                            break;
                    }
                    Rectangle boundingBox;
                    boundingBox = animal != null ? animal.GetBoundingBox() : pet.GetBoundingBox();

                    double numX = boundingBox.Center.X;
                    double numY = boundingBox.Center.Y;

                    Vector2 vectorRibbon = new Vector2((float)numX - 32, (float)numY-32);
                    var ribbonScale = Game1.pixelZoom * 0.75f;
                    TemporaryAnimatedSprite ribbonSprite = new TemporaryAnimatedSprite(__instance.GetToolData().Texture,
                            Game1.getSourceRectForStandardTileSheet(DataLoader.ToolsSprites, __instance.CurrentParentTileIndex, 16, 16),
                            750.0f, 1, 1, vectorRibbon, false, false, ((float)boundingBox.Bottom + 0.1f) / 10000f, 0.0f,
                            Color.White, ribbonScale, 0.0f, 0.0f, 0.0f)
                        { delayBeforeAnimationStart = 25 };
                    location.temporarySprites.Add(ribbonSprite);

                }
                else
                {
                    who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1] { new FarmerSprite.AnimationFrame(currentFrame, 0, false, who.FacingDirection == 3, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true) });
                }
                who.FarmerSprite.oldFrame = currentFrame;
                who.UsingTool = true;
                who.CanMove = false;

                __result = true;
            }
            catch (Exception e)
            {
                AnimalHusbandryModEntry.monitor.Log($"Error while trying to use the Participant Ribbon.",LogLevel.Error);
                AnimalHusbandryModEntry.monitor.Log($"Details of the error above: {e}", LogLevel.Trace);
            }
            return false;
        }

        public static void DoFunction(GenericTool __instance, GameLocation location, int x, int y, int power, StardewValley.Farmer who)
        {
            if (!IsParticipantRibbon(__instance)) return;
            
            string participantRibbonId = __instance.modData[ParticipantRibbonKey];
            try
            {

                Animals.TryGetValue(participantRibbonId, out FarmAnimal animal);
                if (animal != null)
                {
                    AnimalContestController.MakeAnimalParticipant(animal);
                    Game1.player.removeItemFromInventory(__instance);

                }
                else
                {
                    Pets.TryGetValue(participantRibbonId, out Pet pet);
                    if (pet != null)
                    {
                        AnimalContestController.MakeAnimalParticipant(pet);
                        Game1.player.removeItemFromInventory(__instance);
                    }
                }
            }
            catch (Exception e)
            {
                AnimalHusbandryModEntry.monitor.Log($"Error while trying to use the Participant Ribbon.",
                    LogLevel.Error);
                AnimalHusbandryModEntry.monitor.Log($"Details of the error above: {e}", LogLevel.Trace);
            }
            finally
            {
                Animals[participantRibbonId] = null;
                Pets[participantRibbonId] = null;

                if (Game1.activeClickableMenu == null)
                {
                    who.CanMove = true;
                }
                else
                {
                    who.Halt();
                }
                who.UsingTool = false;
                who.canReleaseTool = true;
            }

            return;
        }

        public static bool drawTool(Farmer f, int currentToolIndex)
        {
            Tool correntTool = f.CurrentTool;
            if (!IsParticipantRibbon(correntTool)) return true;
            correntTool.draw(Game1.spriteBatch);
            return false;
        }

        public static bool endUsing(GameLocation location, Farmer who)
        {
            Tool correntTool = who.CurrentTool;
            if (!IsParticipantRibbon(correntTool)) return true;

            who.stopJittering();
            who.canReleaseTool = false;
            int addedAnimationMultiplayer = ((!(who.Stamina <= 0f)) ? 1 : 2);
            if (Game1.isAnyGamePadButtonBeingPressed() || !who.IsLocalPlayer)
            {
                who.lastClick = who.GetToolLocation();
            }
            return false;
        }

        private static bool IsParticipantRibbon(Item tool)
        {
            return tool?.modData?.ContainsKey(ParticipantRibbonKey) ?? false;
        }

    }
}
