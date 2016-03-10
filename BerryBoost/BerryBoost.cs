using System;
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
        private bool BerryBoosted { get; set; }
        private bool JojaBoosted { get; set; }
        private int TimeToStartTheBoost { get; set; }
        private int TimeToStopTheBoost { get; set; }
        private int TimeToStartFatigue { get; set; }
        private int TimeToStopFatigue { get; set; }
        private bool IsFatigued { get; set; }

        

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            var configLocation = Path.Combine(ParentPathOnDisk, "BerryBoostConfig.json");
            if (!File.Exists(configLocation))
            {
                ModConfig = new Config();
                ModConfig.EnableJojaCola = false;
                File.WriteAllBytes(configLocation, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ModConfig)));
                Console.WriteLine("The config file for BerryBoost has been loaded.\n\tJojaCola Enabled: {0}",
                    ModConfig.EnableJojaCola);
            }
            else
            {
                ModConfig = JsonConvert.DeserializeObject<Config>(Encoding.UTF8.GetString(File.ReadAllBytes(configLocation)));
                Console.WriteLine("The config file for BerryBoost has been loaded.\n\tJojaCola Enabled: {0}",
                  ModConfig.EnableJojaCola);
            }

            Console.WriteLine("BerryBoost v0.02 initialized.");
        }

        public Config ModConfig { get; private set; }

        [Subscribe]
        public void OnConsumeABerry(PlayerEatObjectEvent @event)
        {
            var foodEaten = @event.O;
            var timeFoodConsumed = @event.Root.TimeOfDay;
            //Time represented as int, e.g., 6:00 is 600

            if (foodEaten.Name == "Strawberry" || foodEaten.Name == "Cranberries" || foodEaten.Name == "Blueberry")
            {
                IsFatigued = false;
                BerryBoosted = true;
                TimeToStartTheBoost = @event.Root.TimeOfDay;
                TimeToStopTheBoost = @event.Root.TimeOfDay + 300; //300
            }

            if (foodEaten.Name == "Joja Cola" && ModConfig.EnableJojaCola)
            {
                IsFatigued = false;
                JojaBoosted = true;
                TimeToStartTheBoost = @event.Root.TimeOfDay;
                TimeToStopTheBoost = @event.Root.TimeOfDay + 400; //400
            }
        }

        [Subscribe]
        public void UpdateCallback(PreUpdateEvent @event)
        {
            
            var player = @event.Root.Player;
            if (JojaBoosted)
            {
                if (player.Running)
                    player.AddedSpeed = 3;
                else
                    player.AddedSpeed = 3;

                if (@event.Root.TimeOfDay >= this.TimeToStopTheBoost || @event.Root.TimeOfDay < this.TimeToStartTheBoost)
                {
                    if (player.Running)
                        player.AddedSpeed = 0;
                    else
                        player.AddedSpeed = 0;

                    JojaBoosted = false;
                    IsFatigued = true;
                    TimeToStartFatigue = @event.Root.TimeOfDay;
                    TimeToStopFatigue = @event.Root.TimeOfDay + 100; //100
                }
            }
        
            // Prioritise Joja as it is faster
            if (BerryBoosted && !JojaBoosted)
            {
                if (player.Running)
                    player.AddedSpeed = 2;
                else
                    player.AddedSpeed = 2;

                if (@event.Root.TimeOfDay >= this.TimeToStopTheBoost || @event.Root.TimeOfDay < this.TimeToStartTheBoost)
                {
                    if (player.Running)
                        player.AddedSpeed = 0;
                    else
                        player.AddedSpeed = 0;
                    BerryBoosted = false;
                }
            }

            // Deal with fatigue
            if (IsFatigued)
            {
                if (player.Running)
                    player.AddedSpeed = -1;
                else
                    player.AddedSpeed = -1;

                if (@event.Root.TimeOfDay >= this.TimeToStopFatigue || @event.Root.TimeOfDay < this.TimeToStartFatigue)
                {
                    if (player.Running)
                        player.AddedSpeed = 0;
                    else
                        player.AddedSpeed = 0;

                      IsFatigued = false;
                }
            }
        }
    }

    public class Config
    {
        public bool EnableJojaCola { get; set; }
    }
}