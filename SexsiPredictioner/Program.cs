using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EloBuddy.SDK.Spell;
using EloBuddy.SDK.Utils;
using SexsiPrediction.Collision;
using SexsiPrediction.Skillshots;
using SharpDX;
using Prediction = SexsiPrediction.Skillshots.Prediction;
using LeagueSharp.Data.DataTypes;
using LeagueSharp.Data;
using EloBuddy.SDK.Events;
using System.Diagnostics;
using System.Security.Cryptography;
using EloBuddy.Sandbox;
using System.Net;
using System.IO;
using System.Windows.Forms;

namespace SexsiPredictioner
{
    public class Spell
    {
        #region Fields

        private int chargedCastedT;

        private int chargeReqSentT;

        private float range = float.MaxValue;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Spell" /> class.
        /// </summary>
        /// <param name="slot">The slot.</param>
        public Spell(SpellSlot slot)
        {
            this.Slot = slot;

            Logger.Debug("{0} Spell Created", slot);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Spell" /> class.
        /// </summary>
        /// <param name="slot">The slot.</param>
        /// <param name="range">The range.</param>
        public Spell(SpellSlot slot, float range)
            : this(slot)
        {
            this.Range = range;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the name of the charged buff.
        /// </summary>
        /// <value>The name of the charged buff.</value>
        public string ChargedBuffName { get; set; }

        /// <summary>
        ///     Gets or sets the charged maximum range.
        /// </summary>
        /// <value>The charged maximum range.</value>
        public int ChargedMaxRange { get; set; }

        /// <summary>
        ///     Gets or sets the charged minimum range.
        /// </summary>
        /// <value>The charged minimum range.</value>
        public int ChargedMinRange { get; set; }

        /// <summary>
        ///     Gets or sets the name of the charged spell.
        /// </summary>
        /// <value>The name of the charged spell.</value>
        public string ChargedSpellName { get; set; }

        /// <summary>
        ///     Gets or sets the duration of the charge.
        /// </summary>
        /// <value>The duration of the charge.</value>
        public float ChargeDuration { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Spell" /> has collision.
        /// </summary>
        /// <value><c>true</c> if the spell has collision; otherwise, <c>false</c>.</value>
        public bool Collision { get; set; }

        /// <summary>
        ///     Gets or sets the delay.
        /// </summary>
        /// <value>The delay.</value>
        public float Delay { get; set; }

        /// <summary>
        ///     Gets or sets the hit chance.
        /// </summary>
        /// <value>The hit chance.</value>
        public SexsiPrediction.Skillshots.HitChance HitChance { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is charged spell.
        /// </summary>
        /// <value><c>true</c> if this instance is charged spell; otherwise, <c>false</c>.</value>
        public bool IsChargedSpell { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is charing.
        /// </summary>
        /// <value><c>true</c> if this instance is charing; otherwise, <c>false</c>.</value>
        public bool IsCharging => this.Ready && (Player.HasBuff(this.ChargedBuffName)
                                                 || Core.GameTickCount - this.chargedCastedT < 300 + Game.Ping);

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is skill shot.
        /// </summary>
        /// <value><c>true</c> if this instance is skill shot; otherwise, <c>false</c>.</value>
        public bool IsSkillShot { get; set; }

        /// <summary>
        ///     Gets or sets the range.
        /// </summary>
        /// <value>The range.</value>
        public float Range
        {
            get
            {
                if (!this.IsChargedSpell)
                {
                    return this.range;
                }

                if (this.IsCharging)
                {
                    return this.ChargedMinRange + Math.Min(
                               this.ChargedMaxRange - this.ChargedMinRange,
                               (Core.GameTickCount - this.chargedCastedT) * (this.ChargedMaxRange - this.ChargedMinRange)
                               / this.ChargeDuration - 150);
                }

                return this.ChargedMaxRange;
            }
			set
			{
				this.range = value;
			}
        }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="Spell" /> is ready.
        /// </summary>
        /// <value><c>true</c> if ready; otherwise, <c>false</c>.</value>
        public bool Ready => Player.Spellbook.GetSpell(this.Slot).State == SpellState.Ready;

        /// <summary>
        ///     Gets or sets the slot.
        /// </summary>
        /// <value>The slot.</value>
        public SpellSlot Slot { get; set; }

        /// <summary>
        ///     Gets or sets the speed.
        /// </summary>
        /// <value>The speed.</value>
        public float Speed { get; set; }

        /// <summary>
        ///     Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public LeagueSharp.Data.Enumerations.SpellType /*EloBuddy.SDK.Enumerations.SkillShotType*/ /*SkillType*/ Type { get; set; }

        /// <summary>
        ///     Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        public float Width { get; set; }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>The player.</value>
        private static AIHeroClient Player => ObjectManager.Player;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Casts the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns><c>true</c> if the spell was casted, <c>false</c> otherwise.</returns>
        public bool Cast(Obj_AI_Base target)
        {
            if (!this.Ready)
            {
                return false;
            }

            if (!this.IsSkillShot && !this.IsChargedSpell)
            {
                return Player.Spellbook.CastSpell(this.Slot, target);
            }

            var prediction = Prediction.Instance.GetPrediction(this.GetPredictionInput(target));

            if (prediction.HitChance < this.HitChance)
            {
                return false;
            }

            if (this.IsChargedSpell)
            {
                return this.IsCharging ? ShootChargedSpell(this.Slot, prediction.CastPosition) : this.StartCharging();
            }

            return Player.Spellbook.CastSpell(this.Slot, prediction.CastPosition);
        }

        /// <summary>
        ///     Casts this instance.
        /// </summary>
        /// <returns><c>true</c> if the spell was casted, <c>false</c> otherwise.</returns>
        public bool Cast()
        {
            if (!this.Ready)
            {
                return false;
            }

            if (this.IsSkillShot)
            {
                Logger.Warn("{0} is a skillshot, but casted like a self-activated ability.", this.Slot);
            }

            return Player.Spellbook.CastSpell(this.Slot);
        }

        /// <summary>
        ///     Casts the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Cast(Vector2 position)
        {
            return Player.Spellbook.CastSpell(
                this.Slot,
                new Vector3(position.X, NavMesh.GetHeightForPosition(position.X, position.Y), position.Y));
        }

        /// <summary>
        ///     Casts the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Cast(Vector3 position)
        {
            return Player.Spellbook.CastSpell(this.Slot, position);
        }

        /// <summary>
        ///     Casts the on unit.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CastOnUnit(GameObject obj)
        {
            return Player.Spellbook.CastSpell(this.Slot, obj);
        }

        /// <summary>
        ///     Gets the prediction.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>PredictionOutput.</returns>
        public PredictionOutput GetPrediction(Obj_AI_Base target)
        {
            return Prediction.Implementation.GetPrediction(this.GetPredictionInput(target));
        }

        /// <summary>
        ///     Gets the prediction input.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>PredictionInput.</returns>
        public PredictionInput GetPredictionInput(Obj_AI_Base target)
        {
            return new PredictionInput()
            {
                CollisionObjects = this.Collision ? CollisionableObjects.Minions : 0,
                Delay = this.Delay,
                Radius = this.Width,
                Speed = this.Speed,
                Range = this.Range,
                Target = target,
                Unit = Player
            };
        }

        /// <summary>
        ///     Sets the charged.
        /// </summary>
        /// <param name="spellName">Name of the spell.</param>
        /// <param name="buffName">Name of the buff.</param>
        /// <param name="minRange">The minimum range.</param>
        /// <param name="maxRange">The maximum range.</param>
        /// <param name="chargeDurationSeconds">The charge duration in seconds.</param>
        public void SetCharged(
            string spellName,
            string buffName,
            int minRange,
            int maxRange,
            float chargeDurationSeconds)
        {
            this.IsChargedSpell = true;
            this.ChargedSpellName = spellName;
            this.ChargedBuffName = buffName;
            this.ChargedMaxRange = maxRange;
            this.ChargedMinRange = minRange;
            this.ChargeDuration = chargeDurationSeconds * 1000;
            this.chargedCastedT = 0;

            Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
            {
                if (sender.IsMe && args.SData.Name == this.ChargedSpellName)
                {
                    this.chargedCastedT = Core.GameTickCount;
                }
            };

            Spellbook.OnCastSpell += (sender, args) =>
            {
                if (args.Slot == this.Slot && Core.GameTickCount - this.chargeReqSentT > 500 && this.IsCharging)
                {
                    this.Cast((Vector2)args.EndPosition);
                }
            };

            Logger.Debug(
                "{0} Set as Charged -> Spell Name: {1}, Buff Name: {2}, Min Range: {3}, Max Range: {4}, Charge Duration: {5}s",
                this.Slot,
                spellName,
                buffName,
                minRange,
                maxRange,
                chargeDurationSeconds);
        }

        /// <summary>
        ///     Sets the skillshot.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <param name="width">The width.</param>
        /// <param name="speed">The speed.</param>
        /// <param name="collision">if set to <c>true</c> the spell has collision.</param>
        /// <param name="type">The type.</param>
        /// <param name="hitchance">The hitchance.</param>
        public void SetSkillshot(
            float delay,
            float width,
            float speed,
            bool collision,
            LeagueSharp.Data.Enumerations.SpellType/*EloBuddy.SDK.Enumerations.SkillShotType*/ type,//SkillType type,
            SexsiPrediction.Skillshots.HitChance hitchance = SexsiPrediction.Skillshots.HitChance.Low)
        {
            this.Delay = delay;
            this.Width = width;
            this.Speed = speed;
            this.Type = type;
            this.Collision = collision;
            this.IsSkillShot = true;
            this.HitChance = hitchance;

            Logger.Debug(
                "{0} Set as SkillShot -> Range: {1}, Delay: {2},  Width: {3}, Speed: {4}, Collision: {5}, Type: {6}, MinHitChance: {7}",
                this.Slot,
                this.Range,
                delay,
                width,
                speed,
                collision,
                type,
                hitchance);
        }

        #endregion

        #region Methods

        private static bool ShootChargedSpell(SpellSlot slot, Vector3 position, bool releaseCast = true)
        {
            return Player.Spellbook.CastSpell(slot, position)
                   && Player.Spellbook.UpdateChargeableSpell(slot, position, releaseCast);
        }

        private bool StartCharging()
        {
            if (this.IsCharging || Core.GameTickCount - this.chargeReqSentT <= 400 + Game.Ping)
            {
                return false;
            }

            this.chargeReqSentT = Core.GameTickCount;
            return Player.Spellbook.CastSpell(this.Slot);
        }

        #endregion
    }

    public static class Utility
    {
        #region Champion Priority Arrays
        public static string[] lowPriority =
            {
                "Alistar", "Amumu", "Bard", "Blitzcrank", "Braum", "Cho'Gath", "Dr. Mundo", "Garen", "Gnar",
                "Hecarim", "Janna", "Jarvan IV", "Leona", "Lulu", "Malphite", "Nami", "Nasus", "Nautilus", "Nunu",
                "Olaf", "Rammus", "Renekton", "Sejuani", "Shen", "Shyvana", "Singed", "Sion", "Skarner", "Sona",
                "Soraka", "Tahm", "Taric", "Thresh", "Volibear", "Warwick", "MonkeyKing", "Yorick", "Zac", "Zyra"
            };

        public static string[] mediumPriority =
            {
                "Aatrox", "Akali", "Darius", "Diana", "Ekko", "Elise", "Evelynn", "Fiddlesticks", "Fiora", "Fizz",
                "Galio", "Gangplank", "Gragas", "Heimerdinger", "Irelia", "Jax", "Jayce", "Kassadin", "Kayle", "Kha'Zix",
                "Lee Sin", "Lissandra", "Maokai", "Mordekaiser", "Morgana", "Nocturne", "Nidalee", "Pantheon", "Poppy",
                "RekSai", "Rengar", "Riven", "Rumble", "Ryze", "Shaco", "Swain", "Trundle", "Tryndamere", "Udyr",
                "Urgot", "Vladimir", "Vi", "XinZhao", "Yasuo", "Zilean"
            };

        public static string[] highPriority =
            {
                "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
                "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Leblanc",
                "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra", "Talon",
                "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "VelKoz", "Viktor", "Xerath",
                "Zed", "Ziggs"
            };
        #endregion

        public static string[] HitchanceNameArray = { "Low", "Medium"};
        public static HitChance[] HitchanceArray = { HitChance.Low, HitChance.Medium};

        public static int GetPriority(string championName)
        {
            if (lowPriority.Contains(championName))
                return 1;

            if (mediumPriority.Contains(championName))
                return 2;

            if (highPriority.Contains(championName))
                return 3;

            return 2;
        }

        public static bool IsImmobilizeBuff(BuffType type)
        {
            return type == BuffType.Snare || type == BuffType.Stun || type == BuffType.Charm || type == BuffType.Knockup || type == BuffType.Suppression;
        }

        public static bool IsImmobileTarget(AIHeroClient target)
        {
            return target.Buffs.Count(p => IsImmobilizeBuff(p.Type)) > 0 || target.Spellbook.IsChanneling;
        }

        public static bool IsActive(this SpellBase s)
        {
            return ObjectManager.Player.Spellbook.GetSpell(ObjectManager.Player.GetSpellSlotFromName(s.Name)).ToggleState == 2;
        }

        public static async Task CastWithDelay(this SpellBase s, int delay)
        {
            System.Threading.Thread.Sleep(delay);
            s.Cast();
        }

        public static async Task DelayAction(Action act, int delay = 1)
        {
            System.Threading.Thread.Sleep(delay);
            act();
        }

        public static bool IsValidSlot(SpellSlot slot)
        {
            return slot == SpellSlot.Q || slot == SpellSlot.W || slot == SpellSlot.E || slot == SpellSlot.R;
        }
    }

    public class SexsiPredictioner
    {
        public static Spell[] Spells = { null, null, null, null };
        public static EloBuddy.SDK.Menu.Menu Config, skillshots;
        public static KeyBind Combo, Harass;
        public static EloBuddy.SDK.Menu.Values.CheckBox Enabled;
        public static EloBuddy.SDK.Menu.Values.ComboBox Hitc;

        public static IReadOnlyList<SpellDatabaseEntry> Spells2 =
            Data.Get<LeagueSharp.Data.DataTypes.SpellDatabase>().Spells;


        public static void Initialize()
        {
            #region Initialize Menu
            Config = EloBuddy.SDK.Menu.MainMenu.AddMenu("SexsiPredictioner", "sexsipredictioner");
            Combo = Config.Add("COMBOKEY", new KeyBind("Combo", false, KeyBind.BindTypes.HoldActive, 32));
            Harass = Config.Add("HARASSKEY", new KeyBind("Harass", false, KeyBind.BindTypes.HoldActive, 'C'));
            Enabled = Config.Add("ENABLED", new EloBuddy.SDK.Menu.Values.CheckBox("Enabled"));

            #region Initialize Spells
            skillshots = Config.AddSubMenu("Skillshots", "spredskillshots");
            foreach (var spell in Spells2/*SpellDatabase.Spells*/)
            {
                if (spell.ChampionName == ObjectManager.Player.CharData.BaseSkinName)
                { 
                Spells[(int)spell.Slot] = new Spell(spell.Slot, spell.Range);
                Spells[(int)spell.Slot].SetSkillshot(spell.Delay / 1000f, spell.Radius, spell.MissileSpeed, spell.CollisionObjects.Any(), spell.SpellType);//Collisionable, spell.SpellType);
                skillshots.Add(String.Format("{0}{1}", spell.ChampionName, spell.Slot), new EloBuddy.SDK.Menu.Values.CheckBox("Convert Spell " + spell.Slot.ToString(), true));
                }
            }
            #endregion


            Hitc = Config.Add("Hitchancex", new EloBuddy.SDK.Menu.Values.ComboBox("Hit Chance", 1, Utility.HitchanceNameArray));
            #endregion

            #region Initialize Events
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Obj_AI_Base.OnSpellCast += AIHeroClient_OnProcessSpellCast;
            #endregion

        }

        private static bool[] handleEvent = { true, true, true, true };

        public static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                SpellSlot slot = ObjectManager.Player.GetSpellSlotFromName(args.SData.Name);
                if (!Utility.IsValidSlot(slot))
                    return;

                if (!handleEvent[(int)slot])
                {
                    if (Spells[(int)slot] != null)
                        handleEvent[(int)slot] = true;
                }
            }
        }

        public static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            try
            {
            if (sender.Owner.IsMe)
            {
                if (Enabled.CurrentValue && (Combo.CurrentValue || Harass.CurrentValue))
                {
                    if (!Utility.IsValidSlot(args.Slot))
                        return;
                    
                        if (Spells[(int)args.Slot] == null)
                        return;
                        
                        if (!skillshots[String.Format("{0}{1}", ObjectManager.Player.ChampionName, args.Slot)].Cast<EloBuddy.SDK.Menu.Values.CheckBox>().CurrentValue)
                        return;
                        
                    if (handleEvent[(int)args.Slot])
                    {
                        args.Process = false;
                        var enemy = TargetSelector.GetTarget(Spells[(int)args.Slot].Range, DamageType.Physical);

                       


                        if (enemy != null)
                        {
                            SexsiPrediction.Skillshots.PredictionInput input = Spells[(int)args.Slot].GetPredictionInput(enemy);

                            if (input != null)
                            {

                            var pred = SexsiPrediction.Skillshots.Prediction.Implementation.GetPrediction(input);

                            if ( !(pred.HitChance >= Utility.HitchanceArray[Hitc.SelectedIndex]) )
                            {
                                args.Process = false;
                                return;
                            }


                            if (ObjectManager.Player.ChampionName == "Viktor" && args.Slot == SpellSlot.E)
                            {
                                handleEvent[(int)args.Slot] = false;
                                Spells[(int)args.Slot].Range = 500;
                                Spells[(int)args.Slot].HitChance = Utility.HitchanceArray[Hitc.SelectedIndex];
                                Spells[(int)args.Slot].Cast(pred.CastPosition);
                            }
                            else if (ObjectManager.Player.ChampionName == "Diana" && args.Slot == SpellSlot.Q)
                            {
                                handleEvent[(int)args.Slot] = false;
                                Spells[(int)args.Slot].HitChance = Utility.HitchanceArray[Hitc.SelectedIndex];
                                Spells[(int)args.Slot].Cast(pred.CastPosition);
                            }
                            else
                            {
                                Spells[(int)args.Slot].HitChance = Utility.HitchanceArray[Hitc.SelectedIndex];
                                Spells[(int)args.Slot].Cast(pred.CastPosition);
                                handleEvent[(int)args.Slot] = false;
                            }

                            }

                        }
                    }
                }
            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Chat.Print("SexsiPrediction has been loaded. Have Fun !");
            SexsiPredictioner.Initialize();
        }
    }
}
