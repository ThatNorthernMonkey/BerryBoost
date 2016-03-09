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

        public static Farmer Player => StaticGameContext.WrappedGame.Player;

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
                this.IsFatigued = false;
                this.BerryBoosted = true;
                this.TimeToStartTheBoost = @event.Root.TimeOfDay;
                this.TimeToStopTheBoost = @event.Root.TimeOfDay + 300;
            }

            if (foodEaten.Name == "Joja Cola" && ModConfig.EnableJojaCola)
            {
                this.IsFatigued = false;
                this.JojaBoosted = true;
                this.TimeToStartTheBoost = @event.Root.TimeOfDay;
                this.TimeToStopTheBoost = @event.Root.TimeOfDay + 400;
            }
        }

        [Subscribe]
        public void UpdateCallback(PreUpdateEvent @event)
        {
            Console.WriteLine(Player.AddedSpeed);

            if (this.JojaBoosted)
            {
                if (Player.Running)
                    Player.AddedSpeed = 3;
                else
                    Player.AddedSpeed = 3;

                if (@event.Root.TimeOfDay >= this.TimeToStopTheBoost || @event.Root.TimeOfDay < this.TimeToStartTheBoost)
                {
                    if (Player.Running)
                        Player.AddedSpeed = 0;
                    else
                        Player.AddedSpeed = 0;

                    this.JojaBoosted = false;
                    this.IsFatigued = true;
                    this.TimeToStartFatigue = @event.Root.TimeOfDay;
                    this.TimeToStopFatigue = @event.Root.TimeOfDay + 100;
                }
            }
        
            // Prioritise Joja as it is faster
            if (this.BerryBoosted && !this.JojaBoosted)
            {
                if (Player.Running)
                    Player.AddedSpeed = 2;
                else
                    Player.AddedSpeed = 2;

                if (@event.Root.TimeOfDay >= this.TimeToStopTheBoost || @event.Root.TimeOfDay < this.TimeToStartTheBoost)
                {
                    if (Player.Running)
                        Player.AddedSpeed = 0;
                    else
                        Player.AddedSpeed = 0;
                    this.BerryBoosted = false;
                }
            }

            // Deal with fatigue
            if (this.IsFatigued)
            {
                if (Player.Running)
                    Player.AddedSpeed = -1;
                else
                    Player.AddedSpeed = -1;

                if (@event.Root.TimeOfDay >= this.TimeToStopFatigue || @event.Root.TimeOfDay < this.TimeToStartFatigue)
                {
                    if (Player.Running)
                        Player.AddedSpeed = 0;
                    else
                        Player.AddedSpeed = 0;

                      this.IsFatigued = false;
                }
            }
        }
    }

    public class Config
    {
        public bool EnableJojaCola { get; set; }
    }
}