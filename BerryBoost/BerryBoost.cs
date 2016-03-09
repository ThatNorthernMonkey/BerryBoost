﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Storm.ExternalEvent;
using Storm.StardewValley;
using Storm.StardewValley.Event;
using Storm.StardewValley.Wrapper;


namespace BerryBoost
{
    [Mod]
    public class BerryBoost : DiskResource
    {

        private bool Boosted { get; set; }
        private int TimeToStartTheBoost { get; set; }
        private int TimeToStopTheBoost { get; set; }

        public static Farmer Player => StaticGameContext.WrappedGame.Player;


        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            Console.WriteLine("BerryBoost initialized.");
        }

        [Subscribe]
        public void OnConsumeABerry(PlayerEatObjectEvent @event)
        {
            var foodEaten = @event.O;
            var player = @event.Root.Player;
            var timeFoodConsumed = @event.Root.TimeOfDay;
            //Console.WriteLine("Start Speed: " + Player.AddedSpeed.ToString());
            //Time represented as int, e.g., 6:00 is 600
            //Console.WriteLine(timeFoodConsumed);
            //Console.WriteLine(foodEaten.Name);


            if (foodEaten.Name == "Strawberry" || foodEaten.Name == "Cranberries" || foodEaten.Name == "Blueberry")
            {             
                this.Boosted = true;
                this.TimeToStartTheBoost = @event.Root.TimeOfDay;
                this.TimeToStopTheBoost = @event.Root.TimeOfDay + 180;
                //Console.WriteLine("New Speed: " + Player.AddedSpeed.ToString());
                //Console.WriteLine("Time to stop boost: " + this.TimeToStopTheBoost);              
            }
        }

        [Subscribe]
        public void UpdateCallback(PreUpdateEvent @event)
        {  
            if (this.Boosted)
            {
                if (Player.Running)
                    Player.AddedSpeed = 2;
                else
                    Player.AddedSpeed = 2;

                if (@event.Root.TimeOfDay >= this.TimeToStopTheBoost)
                {
                    if (Player.Running)
                        Player.AddedSpeed = 0;
                    else
                        Player.AddedSpeed = 0;
                    this.Boosted = false;
                }
            }
        }
    }
}
