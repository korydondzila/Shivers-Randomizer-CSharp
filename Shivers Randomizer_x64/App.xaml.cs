﻿using Shivers_Randomizer;
using Shivers_Randomizer_x64.room_randomizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using static Shivers_Randomizer_x64.utils.AppHelpers;

namespace Shivers_Randomizer_x64;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public MainWindow_x64 mainWindow;
    public Overlay_x64 overlay;
    public Multiplayer_Client? multiplayer_Client = null;// new Multiplayer_Client();

    private RectSpecial ShiversWindowDimensions = new();

    public UIntPtr processHandle;
    public UIntPtr MyAddress;
    public UIntPtr hwndtest;
    public bool AddressLocated;
    public bool EnableAttachButton;

    public int Seed;
    public bool setSeedUsed;
    public int FailureMessage;
    public int ScrambleCount;
    public int[] Locations = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };
    public int roomNumber;
    public int roomNumberPrevious;
    public int numberIxupiCaptured;
    public int numberIxupiCapturedTemp;
    public int firstToTheOnlyXNumber;
    public bool finalCutsceneTriggered;
    private bool elevatorUndergroundSolved;
    private bool elevatorBedroomSolved;
    private bool elevatorThreeFloorSolved;

    public bool settingsVanilla;
    public bool settingsIncludeAsh;
    public bool settingsIncludeLightning;
    public bool settingsEarlyBeth;
    public bool settingsExtraLocations;
    public bool settingsExcludeLyre;
    public bool settingsEarlyLightning;
    public bool settingsRedDoor;
    public bool settingsFullPots;
    public bool settingsFirstToTheOnlyFive;
    public bool settingsRoomShuffle;
    public bool settingsIncludeElevators;
    public bool settingsMultiplayer;
    public bool settingsOnly4x4Elevators;
    public bool settingsElevatorsStaySolved;

    public bool currentlyTeleportingPlayer = false;
    public RoomTransition? lastTransitionUsed;

    public bool disableScrambleButton;
    public int[] multiplayerLocations = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };
    public int[] ixupiLocations = new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    public bool currentlyRunningThreadOne = false;
    public bool currentlyRunningThreadTwo = false;

    public RoomTransition[] roomTransitions = Array.Empty<RoomTransition>();

    public App()
    {
        mainWindow = new MainWindow_x64(this);
        overlay = new Overlay_x64(this);
        mainWindow.Show();
    }

    public void Scramble()
    {
        if (multiplayer_Client != null)
        {
            settingsMultiplayer = multiplayer_Client.multiplayerEnabled;
        }

        //Check if seed was entered
        if (mainWindow.txtBox_Seed.Text != "")
        {
            //check if seed is too big, if not use it
            if (!int.TryParse(mainWindow.txtBox_Seed.Text, out Seed))
            {
                FailureMessage = 1;
                goto Failure;
            }
            setSeedUsed = true;
        }
        else
        {
            setSeedUsed = false;
            //if not seed entered, seed to the system clock
            Seed = (int)DateTime.Now.Ticks;

        }

        //If not a set seed, hide the system clock seed number so that it cant be used to cheat (unlikely but what ever)
        Random rngHidden = new(Seed);
        ;
        if (!setSeedUsed)
        {
            Seed = rngHidden.Next();
        }
        Random rng = new(Seed);


        //If early lightning then set flags for timer
        finalCutsceneTriggered = false;

        //Reset elevator flags
        elevatorUndergroundSolved = false;
        elevatorBedroomSolved = false;
        elevatorThreeFloorSolved = false;

    Scramble:
        Locations = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };

        //If Vanilla is selected then use the vanilla placement algorithm
        if (settingsVanilla)
        {
            Locations[0] = 212; //Places Ash Top in desk drawer
            Locations[4] = 217; //Places Lighting Top in slide
            Locations[10] = 202; //Places Ash bottom in Greenhouse
            VanillaPlacePiece(200, rng); //Place Water Bottom
            VanillaPlacePiece(201, rng); //Place Wax Bottom
            VanillaPlacePiece(203, rng); //Place Oil Bottom
            VanillaPlacePiece(204, rng); //Place Cloth Bottom
            VanillaPlacePiece(205, rng); //Place Wood Bottom
            VanillaPlacePiece(206, rng); //Place Crystal Bottom
            VanillaPlacePiece(207, rng); //Place Electricity Bottom
            VanillaPlacePiece(208, rng); //Place Sand Bottom
            VanillaPlacePiece(209, rng); //Place Metal Bottom
            VanillaPlacePiece(210, rng); //Place Water Top
            VanillaPlacePiece(211, rng); //Place Wax Top
            VanillaPlacePiece(213, rng); //Place Oil Top
            VanillaPlacePiece(214, rng); //Place Cloth Top
            VanillaPlacePiece(215, rng); //Place Wood Top
            VanillaPlacePiece(216, rng); //Place Crystal Top
            VanillaPlacePiece(218, rng); //Place Sand Top
            VanillaPlacePiece(219, rng); //Place Metal Top
        }
        else if (!settingsFirstToTheOnlyFive) //Normal Scramble
        {
            List<int> PiecesNeededToBePlaced = new();
            List<int> PiecesRemainingToBePlaced = new();
            int numberOfRemainingPots = 20;
            int numberOfFullPots = 0;

            //Check if ash is added to the scramble
            if (!settingsIncludeAsh)
            {
                Locations[0] = 212; //Places Ash Top in desk drawer
                Locations[10] = 202; //Places Ash bottom in Greenhouse
                numberOfRemainingPots -= 2;
            }
            //Check if lighting is added to the scramble
            if (!settingsIncludeLightning)
            {
                Locations[4] = 217; //Places Lighting Top in slide
                numberOfRemainingPots -= 1;
            }

            if (settingsFullPots)
            {
                if (settingsExcludeLyre && !settingsExtraLocations)
                {   //No more then 8 since ash/lighitng will be rolled outside of the count
                    numberOfFullPots = rng.Next(1, 9);//Roll how many completed pots. If no lyre and no extra locations you must have at least 1 completed to have room.
                }
                else
                {
                    numberOfFullPots = rng.Next(0, 9);//Roll how many completed pots
                }

                int FullPotRolled;
                for (int i = 0; i < numberOfFullPots; i++)
                {
                RollFullPot:
                    FullPotRolled = rng.Next(220, 230);//Grab a random pot
                    if (FullPotRolled == 222 || FullPotRolled == 227)//Make sure its not ash or lightning
                    {
                        goto RollFullPot;
                    }

                    if (PiecesNeededToBePlaced.Contains(FullPotRolled))//Make sure it wasnt already selected
                    {
                        goto RollFullPot;
                    }
                    PiecesNeededToBePlaced.Add(FullPotRolled);
                    numberOfRemainingPots -= 2;
                }
                if (rng.Next(0, 2) == 1 && settingsIncludeAsh) //Is ash completed
                {
                    PiecesNeededToBePlaced.Add(222);
                    numberOfRemainingPots -= 2;
                }
                if (rng.Next(0, 2) == 1 && settingsIncludeLightning) //Is lighting completed
                {
                    PiecesNeededToBePlaced.Add(227);
                    numberOfRemainingPots -= 2;
                }
            }

            int pieceBeingAddedToList; //Add remaining peices to list
            while (numberOfRemainingPots != 0)
            {
                pieceBeingAddedToList = rng.Next(0, 20) + 200;
                //Check if piece already added to list
                //Check if piece was ash and ash not included in scramble
                //Check if piece was lighting top and lightning not included in scramble
                if (PiecesNeededToBePlaced.Contains(pieceBeingAddedToList) ||
                    ((pieceBeingAddedToList == 202 || pieceBeingAddedToList == 212) && !settingsIncludeAsh) ||
                    ((pieceBeingAddedToList == 217) && !settingsIncludeLightning))
                {
                    continue;
                }
                //Check if completed pieces are used and the base pieces are rolled
                if ((pieceBeingAddedToList < 210 && PiecesNeededToBePlaced.Contains(pieceBeingAddedToList + 20)) || (pieceBeingAddedToList > 209 && PiecesNeededToBePlaced.Contains(pieceBeingAddedToList + 10)))
                {
                    continue;
                }
                PiecesNeededToBePlaced.Add(pieceBeingAddedToList);
                numberOfRemainingPots -= 1;
            }

            int RandomLocation;
            PiecesRemainingToBePlaced = new List<int>(PiecesNeededToBePlaced);
            while (PiecesRemainingToBePlaced.Count > 0)
            {
                RandomLocation = rng.Next(0, 23);
                if (!settingsExtraLocations && (RandomLocation == 2 || RandomLocation == 6 || RandomLocation == 13)) //Check if extra locations are used
                {
                    continue;
                }
                if (settingsExcludeLyre && settingsExtraLocations && numberOfFullPots == 0 && RandomLocation == 14)//Check if lyre excluded
                {
                    continue;
                }
                if (Locations[RandomLocation] != 0) //Check if location is filled
                {
                    continue;
                }
                Locations[RandomLocation] = PiecesRemainingToBePlaced[0];
                PiecesRemainingToBePlaced.RemoveAt(0);
            }

            //Check for bad scramble
            //Check if cloth behind cloth
            //Check if oil behind oil
            //Check if cloth behind oil AND oil behind cloth
            if (Locations[8] == 203 || Locations[8] == 213 || Locations[8] == 223 ||
                Locations[17] == 204 || Locations[17] == 214 || Locations[17] == 224 ||
                ((Locations[17] == 203 || Locations[17] == 213 || Locations[17] == 223) && (Locations[8] == 204 || Locations[8] == 214 || Locations[8] == 224)))
            {
                goto Scramble;
            }
        }
        else if (settingsFirstToTheOnlyFive) //First to the only X
        {
            List<int> PiecesNeededToBePlaced = new();
            List<int> PiecesRemainingToBePlaced = new();

            //Get number of sets
            firstToTheOnlyXNumber = int.Parse(mainWindow.txtBox_FirstToTheOnlyX.Text);
            int numberOfRemainingPots = 2 * firstToTheOnlyXNumber;

            //Check for invalid numbers
            if (numberOfRemainingPots == 0) //No Sets
            {
                FailureMessage = 2;
                goto Failure;
            }
            else if (numberOfRemainingPots == 2 && !settingsIncludeAsh && !settingsIncludeLightning) //1 set but didnt not include ash or lightning in scramble
            {
                FailureMessage = 3;
                goto Failure;
            }

            //If 1 set and either IncludeAsh/IncludeLighting is false then force the other. Else roll randomly from all available pots
            if (numberOfRemainingPots == 2 && (settingsIncludeAsh | settingsIncludeLightning))
            {
                if (!settingsIncludeAsh)//Force lightning
                {
                    PiecesNeededToBePlaced.Add(207);
                    Locations[4] = 217; //Places Lighting Top in slide
                }
                else if (!settingsIncludeLightning)//Force Ash
                {
                    Locations[0] = 212; //Places Ash Top in desk drawer
                    Locations[10] = 202; //Places Ash bottom in Greenhouse
                }
            }
            else
            {
                string[] SetsAvailable = new string[] { "Water", "Wax", "Ash", "Oil", "Cloth", "Wood", "Crystal", "Lightning", "Sand", "Metal" };

                //Determine which sets will be included in the scramble
                //First check if lighting/ash are included in the scramble. if not force them
                if (!settingsIncludeAsh)
                {
                    Locations[0] = 212; //Places Ash Top in desk drawer
                    Locations[10] = 202; //Places Ash bottom in Greenhouse
                    numberOfRemainingPots -= 2;
                    SetsAvailable[2] = "";
                }
                if (!settingsIncludeLightning)
                {
                    PiecesNeededToBePlaced.Add(207);
                    Locations[4] = 217; //Places Lighting Top in slide
                    numberOfRemainingPots -= 2;
                    SetsAvailable[7] = "";
                }

                //Next select from the remaining sets available
                while (numberOfRemainingPots > 0)
                {
                    int setSelected = 0;
                    //Pick a set
                    setSelected = rng.Next(0, 10);
                    switch (setSelected)
                    {
                        case 0: //Water
                            if (SetsAvailable.Any(s => s.Contains("Water")))
                            {
                                //Check/roll for full pot
                                if (settingsFullPots && rng.Next(0, 2) == 1)
                                {
                                    PiecesNeededToBePlaced.Add(220);
                                }
                                else
                                {
                                    PiecesNeededToBePlaced.Add(200);
                                    PiecesNeededToBePlaced.Add(210);
                                }

                                numberOfRemainingPots -= 2;
                                SetsAvailable[0] = "";
                            }
                            break;
                        case 1: //Wax
                            if (SetsAvailable.Any(s => s.Contains("Wax")))
                            {
                                //Check/roll for full pot
                                if (settingsFullPots && rng.Next(0, 2) == 1)
                                {
                                    PiecesNeededToBePlaced.Add(221);
                                }
                                else
                                {
                                    PiecesNeededToBePlaced.Add(201);
                                    PiecesNeededToBePlaced.Add(211);
                                }

                                numberOfRemainingPots -= 2;
                                SetsAvailable[1] = "";
                            }
                            break;
                        case 2: //Ash
                            if (SetsAvailable.Any(s => s.Contains("Ash")))
                            {
                                //Check/roll for full pot
                                if (settingsFullPots && rng.Next(0, 2) == 1)
                                {
                                    PiecesNeededToBePlaced.Add(222);
                                }
                                else
                                {
                                    PiecesNeededToBePlaced.Add(202);
                                    PiecesNeededToBePlaced.Add(212);
                                }

                                numberOfRemainingPots -= 2;
                                SetsAvailable[2] = "";
                            }
                            break;
                        case 3: //Oil
                            if (SetsAvailable.Any(s => s.Contains("Oil")))
                            {
                                //Check/roll for full pot
                                if (settingsFullPots && rng.Next(0, 2) == 1)
                                {
                                    PiecesNeededToBePlaced.Add(223);
                                }
                                else
                                {
                                    PiecesNeededToBePlaced.Add(203);
                                    PiecesNeededToBePlaced.Add(213);
                                }

                                numberOfRemainingPots -= 2;
                                SetsAvailable[3] = "";
                            }
                            break;
                        case 4: //Cloth
                            if (SetsAvailable.Any(s => s.Contains("Cloth")))
                            {
                                //Check/roll for full pot
                                if (settingsFullPots && rng.Next(0, 2) == 1)
                                {
                                    PiecesNeededToBePlaced.Add(224);
                                }
                                else
                                {
                                    PiecesNeededToBePlaced.Add(204);
                                    PiecesNeededToBePlaced.Add(214);
                                }

                                numberOfRemainingPots -= 2;
                                SetsAvailable[4] = "";
                            }
                            break;
                        case 5: //Wood
                            if (SetsAvailable.Any(s => s.Contains("Wood")))
                            {
                                //Check/roll for full pot
                                if (settingsFullPots && rng.Next(0, 2) == 1)
                                {
                                    PiecesNeededToBePlaced.Add(225);
                                }
                                else
                                {
                                    PiecesNeededToBePlaced.Add(205);
                                    PiecesNeededToBePlaced.Add(215);
                                }

                                numberOfRemainingPots -= 2;
                                SetsAvailable[5] = "";
                            }
                            break;
                        case 6: //Crystal
                            if (SetsAvailable.Any(s => s.Contains("Crystal")))
                            {
                                //Check/roll for full pot
                                if (settingsFullPots && rng.Next(0, 2) == 1)
                                {
                                    PiecesNeededToBePlaced.Add(226);
                                }
                                else
                                {
                                    PiecesNeededToBePlaced.Add(206);
                                    PiecesNeededToBePlaced.Add(216);
                                }

                                numberOfRemainingPots -= 2;
                                SetsAvailable[6] = "";
                            }
                            break;
                        case 7: //Lightning
                            if (SetsAvailable.Any(s => s.Contains("Lightning")))
                            {
                                //Check/roll for full pot
                                if (settingsFullPots && rng.Next(0, 2) == 1)
                                {
                                    PiecesNeededToBePlaced.Add(227);
                                }
                                else
                                {
                                    PiecesNeededToBePlaced.Add(207);
                                    PiecesNeededToBePlaced.Add(217);
                                }

                                numberOfRemainingPots -= 2;
                                SetsAvailable[7] = "";
                            }
                            break;
                        case 8: //Sand
                            if (SetsAvailable.Any(s => s.Contains("Sand")))
                            {
                                //Check/roll for full pot
                                if (settingsFullPots && rng.Next(0, 2) == 1)
                                {
                                    PiecesNeededToBePlaced.Add(228);
                                }
                                else
                                {
                                    PiecesNeededToBePlaced.Add(208);
                                    PiecesNeededToBePlaced.Add(218);
                                }

                                numberOfRemainingPots -= 2;
                                SetsAvailable[8] = "";
                            }
                            break;
                        case 9: //Metal
                            if (SetsAvailable.Any(s => s.Contains("Metal")))
                            {
                                //Check/roll for full pot
                                if (settingsFullPots && rng.Next(0, 2) == 1)
                                {
                                    PiecesNeededToBePlaced.Add(229);
                                }
                                else
                                {
                                    PiecesNeededToBePlaced.Add(209);
                                    PiecesNeededToBePlaced.Add(219);
                                }

                                numberOfRemainingPots -= 2;
                                SetsAvailable[9] = "";
                            }
                            break;
                    }
                }
                int RandomLocation;
                PiecesRemainingToBePlaced = new List<int>(PiecesNeededToBePlaced);
                while (PiecesRemainingToBePlaced.Count > 0)
                {
                    RandomLocation = rng.Next(0, 23);
                    if (!settingsExtraLocations && (RandomLocation == 2 || RandomLocation == 6 || RandomLocation == 13)) //Check if extra locations are used
                    {
                        continue;
                    }
                    if (settingsExcludeLyre && RandomLocation == 14)//Check if lyre excluded
                    {
                        continue;
                    }
                    if (Locations[RandomLocation] != 0) //Check if location is filled
                    {
                        continue;
                    }
                    Locations[RandomLocation] = PiecesRemainingToBePlaced[0];
                    PiecesRemainingToBePlaced.RemoveAt(0);
                }

                //Check for bad scramble
                //Check if cloth behind cloth
                //Check if oil behind oil
                //Check if cloth behind oil AND oil behind cloth
                //Check if a piece behind cloth with no cloth pot available
                //Check if a piece behind oil with no oil pot available
                if (Locations[8] == 203 || Locations[8] == 213 || Locations[8] == 223 ||
                    Locations[17] == 204 || Locations[17] == 214 || Locations[17] == 224 ||
                    ((Locations[17] == 203 || Locations[17] == 213 || Locations[17] == 223) && (Locations[8] == 204 || Locations[8] == 214 || Locations[8] == 224)) ||
                    (Locations[8] != 0 && !Locations.Contains(203) && !Locations.Contains(213) && !Locations.Contains(223)) ||
                    (Locations[17] != 0 && !Locations.Contains(204) && !Locations.Contains(214) && !Locations.Contains(224)))
                {
                    goto Scramble;
                }
            }
        }

        //Place pieces in memory
        PlacePieces();

        //Set bytes for mailbox/red door/beth. Only mailbox is set if vanilla shuffle is selected
        //This is now obsolete. If the room number isnt 922 then the scramble button isnt enabled. Thus if the randomizer didnt work the scramble button would never enable
        //writeMemory(369, 84); //Mailbox 
        if (!settingsVanilla)
        {
            if (settingsRedDoor)
            {
                WriteMemory(364, 144);
            }
            else
            {
                WriteMemory(364, 0);
            }
            if (settingsEarlyBeth)
            {
                WriteMemory(381, 128);
            }
            else
            {
                WriteMemory(381, 0);
            }
        }

        //Set ixupi captured number
        if (settingsFirstToTheOnlyFive)
        {
            WriteMemory(1712, 10 - firstToTheOnlyXNumber);
        }
        else//Set to 0 if not running First to The Only X
        {
            WriteMemory(1712, 0);
        }

        if (settingsRoomShuffle)
        {
            //Sets slide in lobby to get to tar ON
            WriteMemory(368, 64);

            roomTransitions = new RoomRandomizer(this, rng).RandomizeMap();
        }
        else
        {
            //Sets slide in lobby to get to tar OFF
            WriteMemory(368, 0);
        }

        ScrambleCount += 1;
        mainWindow.label_ScrambleFeedback.Content = "Scramble Number: " + ScrambleCount;

        //Set info for overlay
        overlay.SetInfo();

        //Set Seed info and flagset info
        mainWindow.label_Seed.Content = "Seed: " + Seed;
        mainWindow.label_Flagset.Content = "Flagset: " + overlay.flagset;


        //-----------Multiplayer------------
        if (settingsMultiplayer && multiplayer_Client != null)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                currentlyRunningThreadOne = true;

                //Disable scramble button till all data is dont being received by server
                disableScrambleButton = true;

                //Send starting pots to server
                multiplayer_Client.sendServerStartingPots(Locations);

                //Send starting flagset to server
                multiplayer_Client.sendServerFlagset(overlay.flagset);

                //Send starting seed
                multiplayer_Client.sendServerSeed(Seed);

                //Reenable scramble button
                disableScrambleButton = false;

                currentlyRunningThreadOne = false;
            }).Start();
        }


    Failure:
        switch (FailureMessage)
        {
            case 1:
                MessageBox.Show("Seed was not less then 2,147,483,647. Please try again with a smaller number");
                FailureMessage = 0;
                break;
            case 2:
                MessageBox.Show("Number of Ixupi must be greater than 0");
                FailureMessage = 0;
                break;
            case 3:
                MessageBox.Show("If selecting 1 pot set you must include either lighting or ash into the scramble");
                FailureMessage = 0;
                break;
            case 4:
                MessageBox.Show("");
                FailureMessage = 0;
                break;
        }
    }

    private void WaitServerResponse()
    {
        while (multiplayer_Client?.serverResponded == false)
        {
            Thread.Sleep(100);
        }
    }

    public void PlacePieces()
    {
        /*
        0 = Desk
        1 = Drawers
        2 = Cupboard
        3 = Library
        4 = Slide
        5 = Eagles Head
        6 = Eagles Nest
        7 = Ocean
        8 = Tar River
        9 = Theater
        10 = Greenhouse
        11 = Egypt
        12 = Chinese
        13 = Tiki Hut
        14 = Lyre
        15 = Skeleton
        16 = Anansi
        17 = Janitors Closet / Cloth
        18 = Ufo
        19 = Alchemy
        20 = Puzzle
        21 = Hanging / Gallows
        22 = Clock
        */

        WriteMemory(0, Locations[0]);//Desk Drawer
        WriteMemory(8, Locations[1]);//Workshop
        WriteMemory(16, Locations[2]);//Library Cupboard
        WriteMemory(24, Locations[3]);//Library Statue
        WriteMemory(32, Locations[4]);//Slide
        WriteMemory(40, Locations[5]);//Eagle
        WriteMemory(48, Locations[6]);//Eagles Nest
        WriteMemory(56, Locations[7]);//Ocean
        WriteMemory(64, Locations[8]);//Tar River
        WriteMemory(72, Locations[9]);//Theater
        WriteMemory(80, Locations[10]);//Green House / Plant Room
        WriteMemory(88, Locations[11]);//Egypt
        WriteMemory(96, Locations[12]);//Chinese Solitaire
        WriteMemory(104, Locations[13]);//Tiki Hut
        WriteMemory(112, Locations[14]);//Lyre
        WriteMemory(120, Locations[15]);//Skeleton
        WriteMemory(128, Locations[16]);//Anansi
        WriteMemory(136, Locations[17]);//Janitor Closet
        WriteMemory(144, Locations[18]);//UFO
        WriteMemory(152, Locations[19]);//Alchemy
        WriteMemory(160, Locations[20]);//Puzzle Room
        WriteMemory(168, Locations[21]);//Hanging / Gallows
        WriteMemory(176, Locations[22]);//Clock Tower
    }

    public void DispatcherTimer()
    {
        DispatcherTimer timer = new()
        {
            Interval = TimeSpan.FromMilliseconds(1)
        };
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private int syncCounter = 0;
    private void Timer_Tick(object? sender, EventArgs e)
    {
        syncCounter += 1;
        GetWindowRect(hwndtest, ref ShiversWindowDimensions);
        overlay.Left = ShiversWindowDimensions.Left;
        overlay.Top = ShiversWindowDimensions.Top + (int)SystemParameters.WindowCaptionHeight;
        overlay.labelOverlay.Foreground = IsIconic(hwndtest) ? overlay.brushTransparent : overlay.brushLime;

        if (Seed == 0)
        {
            overlay.labelOverlay.Content = "Not yet randomized";
        }

        //Check if a window exists, if not hide the overlay
        if (!IsWindow(hwndtest))
        {
            overlay.Hide();
        }
        else
        {
            overlay.Show();
        }

        int tempRoomNumber;

        //Monitor Room Number
        if (MyAddress != (UIntPtr)0x0 && processHandle != (UIntPtr)0x0) //Throws an exception if not checked in release mode.
        {
            tempRoomNumber = ReadMemory(-424, 2);

            if (tempRoomNumber != roomNumber)
            {
                roomNumberPrevious = roomNumber;
                roomNumber = tempRoomNumber;
            }
            mainWindow.label_roomPrev.Content = roomNumberPrevious;
            mainWindow.label_room.Content = roomNumber;
        }

        //If room number is 910 or 922 update the status text. If room number is not 922 disable the scramble button.
        if (roomNumber == 910 || roomNumber == 922)
        {
            mainWindow.label_ShiversDetected.Content = "Shivers Detected! :)";
            mainWindow.button_Scramble.IsEnabled = roomNumber == 922;
        }

        //Early lightning
        if (settingsEarlyLightning && !settingsVanilla)
        {
            EarlyLightning();
        }

        //Room Shuffle
        if (settingsRoomShuffle)
        {
            RoomShuffle();
        }

        //Elevators Stay Solved
        if (settingsElevatorsStaySolved)
        {
            //Check if an elevator has been solved
            if (ReadMemory(912, 1) == 1)
            {
                //Determine which elevator was solved
                if (roomNumber == 6300 || roomNumber == 4630)
                {
                    elevatorUndergroundSolved = true;
                }
                else if (roomNumber == 38130 || roomNumber == 37360)
                {
                    elevatorBedroomSolved = true;
                }
                else if (roomNumber == 10101 || roomNumber == 27211 || roomNumber == 33500)
                {
                    elevatorThreeFloorSolved = true;
                }
            }

            //Check if approaching an elevator and that elevator is solved, if so open the elevator and force a screen redraw
            //Check if elevator is already open or not
            if (ReadMemory(361, 1) != 2)
            {
                if ((roomNumber == 6290 || roomNumber == 4620) && elevatorUndergroundSolved)
                {
                    WriteMemory(361, 2);
                    redrawCurrentScreen(roomNumber);
                }
                else if ((roomNumber == 38110 || roomNumber == 37330) && elevatorBedroomSolved)
                {
                    WriteMemory(361, 2);
                    redrawCurrentScreen(roomNumber);
                }
                else if ((roomNumber == 10100 || roomNumber == 27212 || roomNumber == 33140) && elevatorThreeFloorSolved)
                {
                    WriteMemory(361, 2);
                    redrawCurrentScreen(roomNumber);
                }
            }
            
        }

        //Only 4x4 elevators
        if (settingsOnly4x4Elevators)
        {
            if (ReadMemory(212, 1) == 2) //Use 2 instead of 1 incase this code accidentaly gets moved before elevators stay solved code
            {
                WriteMemory(912, 0);
            }
        }





        /*
        bool runThreadIfAvailable = false;
        if (syncCounter > 1)
        {
            runThreadIfAvailable = true;
            syncCounter -= 1;
        }
        */
        mainWindow.label_syncCounter.Content = syncCounter;

        //---------Multiplayer----------
        if (multiplayer_Client != null)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //if (settingsMultiplayer && runThreadIfAvailable && !currentlyRunningThreadTwo && !currentlyRunningThreadOne)
                if (settingsMultiplayer && !currentlyRunningThreadTwo && !currentlyRunningThreadOne)
                {
                    currentlyRunningThreadTwo = true;
                    disableScrambleButton = true;

                    //Request current pot list from server
                    multiplayer_Client.sendServerRequestPotList();

                    //Monitor each location and send a sync update to server if it differs
                    for (int i = 0; i < 23; i++)
                    {
                        int potRead = ReadMemory(i * 8, 1);
                        if (potRead != multiplayerLocations[i])//All locations are 8 apart in the memory so can multiply by i
                        {
                            multiplayerLocations[i] = potRead;
                            multiplayer_Client.sendServerPotUpdate(i, multiplayerLocations[i]);
                        }
                    }

                    //Check if a piece needs synced from another player
                    for (int i = 0; i < 23; i++)
                    {
                        if (ReadMemory(i * 8, 1) != multiplayer_Client.syncPiece[i])  //All locations are 8 apart in the memory so can multiply by i
                        {
                            WriteMemory(i * 8, multiplayer_Client.syncPiece[i]);
                            multiplayerLocations[i] = multiplayer_Client.syncPiece[i];

                            //Force a screen redraw if looking at pot being synced
                            PotSyncRedraw(i);
                        }
                    }

                    //Check if an ixupi was captured
                    //if()

                    disableScrambleButton = false;
                    currentlyRunningThreadTwo = false;
                }
            }).Start();
        }

        //Label for ixupi captured number
        numberIxupiCaptured = ReadMemory(1712, 1);
        mainWindow.label_ixupidNumber.Content = numberIxupiCaptured;
    }

    private void PotSyncRedraw(int location)
    {
        int currentRoomNumber = roomNumber;
        int prevRoomNumber = roomNumber;
        int roomDestination = 0;

        switch (location)
        {
            case 0: //Desk Drawer
                roomDestination = 6220;
                break;
            case 1: //Workshop
                roomDestination = 7112;
                break;
            case 2: //Library Cupboard
                roomDestination = 8100;
                break;
            case 3: //Library Statue
                roomDestination = 8490;
                break;
            case 4: //Slide
                roomDestination = 9420;
                break;
            case 5: //Eagle
                roomDestination = 9760;
                break;
            case 6: //Eagles Nest
                roomDestination = 11310;
                break;
            case 7: //Ocean
                roomDestination = 12181;
                break;
            case 8: //Tar River
                roomDestination = 14080;
                break;
            case 9: //Theater
                roomDestination = 16420;
                break;
            case 10: //Green House / Plant Room
                roomDestination = 19220;
                break;
            case 11: //Egypt
                roomDestination = 20553;
                break;
            case 12://Chinese Solitaire
                roomDestination = 21070;
                break;
            case 13://Tiki Hut
                roomDestination = 22190;
                break;
            case 14://Lyre
                roomDestination = 23550;
                break;
            case 15://Skeleton
                roomDestination = 24320;
                break;
            case 16://Anansi
                roomDestination = 24380;
                break;
            case 17://Janitor Closet
                roomDestination = 25050;
                break;
            case 18://UFO
                roomDestination = 29080;
                break;
            case 19://Alchemy
                roomDestination = 30420;
                break;
            case 20://Puzzle Room
                roomDestination = 31310;
                break;
            case 21://Hanging / Gallows
                roomDestination = 32570;
                break;
            case 22://Clock Tower
                roomDestination = 35110;
                break;
        }

        if (currentRoomNumber == roomDestination)
        {
            redrawCurrentScreen(roomDestination);
        }
    }

    private void redrawCurrentScreen(int room)
    {
        int prevRoomNumber = roomNumber;

        while (prevRoomNumber != 922)
        {
            WriteMemory(-424, 922);
            Thread.Sleep(10);
            //ReadProcessMemory(processHandle, (ulong)MyAddress - 432, buffer, (ulong)buffer.Length, ref bytesRead);
            //prevRoomNumber = buffer[0] + (buffer[1] << 8);
            prevRoomNumber = ReadMemory(-432, 2);
        }

        while (prevRoomNumber != room)
        {
            WriteMemory(-424, room);
            Thread.Sleep(10);
            //ReadProcessMemory(processHandle, (ulong)MyAddress - 432, buffer, (ulong)buffer.Length, ref bytesRead);
            //prevRoomNumber = buffer[0] + (buffer[1] << 8);
            prevRoomNumber = ReadMemory(-432, 2);
        }
    }

    private void RoomShuffle()
    {
        RoomTransition? transition = roomTransitions.FirstOrDefault(transition =>
            roomNumberPrevious == transition.From && roomNumber == transition.DefaultTo && lastTransitionUsed != transition
        );

        if (transition != null)
        {
            lastTransitionUsed = transition;
            if (transition.ElevatorFloor.HasValue)
            {
                WriteMemory(916, transition.ElevatorFloor.Value);
            }

            //Stop Audio to prevent soft locks
            StopAudio(transition.NewTo);
        }
    }

    private void EarlyLightning()
    {

        int lightningLocation = ReadMemory(236, 2);

        //If in basement and Lightning location isnt 0. (0 means he has been captured already)
        if (roomNumber == 39010 && lightningLocation != 0)
        {
            WriteMemory(236, 39000);
        }

        numberIxupiCaptured = ReadMemory(1712, 1);

        if (numberIxupiCaptured == 10 && finalCutsceneTriggered == false)
        {
            //If moved properly to final cutscene, disable the trigger for final cutscene
            finalCutsceneTriggered = true;
            WriteMemory(-424, 935);
        }
    }

    public void StopAudio(int destination)
    {
        const int WM_LBUTTON = 0x0201;

        int tempRoomNumber = 933;

        //Trigger Merrick cutscene to stop audio
        WriteMemory(-424, 933);
        Thread.Sleep(20);
        //Set previous room so fortune teller audio does not play at conclusion of cutscene
        WriteMemory(-432, 922);

        //Force a mouse click to skip cutscene. Keep trying until it succeeds. Dont use a timer instead  of while loop as it gives user opportunity to click a direction after
        //cutscene but before being teleported to next room. This causes user to move to fortune teller room instead of intended destination
        while (tempRoomNumber == 933)
        {
            Thread.Sleep(20);
            tempRoomNumber = ReadMemory(-424, 2);
            PostMessage(hwndtest, WM_LBUTTON, 1, MakeLParam(580, 320));
            PostMessage(hwndtest, WM_LBUTTON, 0, MakeLParam(580, 320));
        }

        bool atDestination = false;

        while (!atDestination)
        {
            WriteMemory(-424, destination);
            Thread.Sleep(50);
            tempRoomNumber = ReadMemory(-424, 2);
            if (tempRoomNumber == destination)
            {
                atDestination = true;
            }
        }

    }

    private void VanillaPlacePiece(int potPiece, Random rng)
    {
        /*
        0 = Desk
        1 = Drawers
        2 = Cupboard
        3 = Library
        4 = Slide
        5 = Eagles Head
        6 = Eagles Nest
        7 = Ocean
        8 = Tar River
        9 = Theater
        10 = Greenhouse
        11 = Egypt
        12 = Chinese
        13 = Tiki Hut
        14 = Lyre
        15 = Skeleton
        16 = Anansi
        17 = Janitors Closet / Cloth
        18 = Ufo
        19 = Alchemy
        20 = Puzzle
        21 = Hanging
        22 = Clock
        */

        int locationRand = rng.Next(0, 23);
        while (true)
        {
            if (locationRand >= 23)
            {
                locationRand = 0;
            }

            //Check if piece is cloth and location is janitors closest
            if (potPiece == 204 || potPiece == 214)
            {
                if (locationRand == 17)
                {
                    locationRand += 1;
                    continue;
                }
            }
            //Checking oil is in the bathroom or tar river
            if (potPiece == 203 || potPiece == 213)
            {
                if (locationRand == 8 || locationRand == 17)
                {
                    locationRand += 1;
                    continue;
                }
            }
            //For extra locations, is disabled in vanilla
            if (1 == 1 && (locationRand == 2 || locationRand == 6 || locationRand == 13))
            {
                locationRand += 1;
                continue;
            }
            //Check if location is already filled
            if (Locations[locationRand] != 0)
            {
                locationRand += 1;
                continue;
            }

            break;
        }
        Locations[locationRand] = potPiece;
    }

    public void WriteMemory(int offset, int value)
    {
        uint bytesWritten = 0;
        uint numberOfBytes = 1;

        if (value < 256)
        { numberOfBytes = 1; }
        else if (value < 65536)
        { numberOfBytes = 2; }
        else if (value < 16777216)
        { numberOfBytes = 3; }
        else if (value <= 2147483647)
        { numberOfBytes = 4; }

        WriteProcessMemory(processHandle, (ulong)(MyAddress + offset), BitConverter.GetBytes(value), numberOfBytes, ref bytesWritten);
    }

    public int ReadMemory(int offset, int numbBytesToRead)
    {
        uint bytesRead = 0;
        byte[] buffer = new byte[2];
        ReadProcessMemory(processHandle, (ulong)(MyAddress + offset), buffer, (ulong)buffer.Length, ref bytesRead);

        if (numbBytesToRead == 1)
        {
            return buffer[0];
        }
        else if (numbBytesToRead == 2)
        {
            return (buffer[0] + (buffer[1] << 8));
        }
        else
        {
            return buffer[0];
        }
    }
}
