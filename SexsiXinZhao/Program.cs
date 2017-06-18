using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace SexsiXinZhao
{
    class Program
    {
        private static AIHeroClient Player => EloBuddy.Player.Instance;
        private static CheckBox ComboQ, ComboQAA, ComboW, ComboE, ComboR, ComboItems, ComboSmite, FarmQ, FarmW, FarmE, FarmLQ, FarmLW, FarmLE, InterruptR;
        private static Slider ComboEmin, ComboRmin, FarmMana, FarmManaL;
        private static Spell.Active Q, W, R;
        private static Spell.Targeted E;
        private static Spell.SpellBase Smite;
        private static Item Botrk, Tiamat, Cutlass, Titanic_Hydra, Ravenous_Hydra, Youmuu;
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if(EloBuddy.Player.Instance.Hero != Champion.XinZhao) return;

            Menu();
            LoadSpells();

            Chat.Print("TRAdana Army - Allahu Akbar", System.Drawing.Color.DeepSkyBlue);

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
        }

        private static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (target != Player || target == null || !(target is AIHeroClient))
                return;

            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    foreach (var hero in EntityManager.Heroes.Enemies)
                    {
                        if (ComboQAA.CurrentValue && Q.IsReady() && (hero.Position - Player.Position).Length() < 300)
                        {
                            Q.Cast();
                        }
                    }
                    break;
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                Farm();
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (InterruptR.CurrentValue && (e.Sender.Position - Player.Position).Length() < R.Range && R.IsReady() && e.Sender.IsValidTarget() && e.Sender.IsEnemy)
            {
                Console.WriteLine(e.Sender);
                if (!e.Sender.HasBuff("xenzhaointimidate"))
                    R.Cast();
            }
        }

        private static void LoadSpells()
        {
            Smite = new Spell.Targeted(Player.GetSpellSlotFromName("summonersmite"), 500);
            if (Smite.Slot == SpellSlot.Unknown) { Smite = new Spell.Targeted(Player.GetSpellSlotFromName("itemsmiteaoe"), 500); }
            if (Smite.Slot == SpellSlot.Unknown) { Smite = new Spell.Targeted(Player.GetSpellSlotFromName("s5_summonersmiteplayerganker"), 500); }
            if (Smite.Slot == SpellSlot.Unknown) { Smite = new Spell.Targeted(Player.GetSpellSlotFromName("s5_summonersmitequick"), 500); }
            if (Smite.Slot == SpellSlot.Unknown) { Smite = new Spell.Targeted(Player.GetSpellSlotFromName("s5_summonersmiteduel"), 500); }
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Targeted(SpellSlot.E, 600);
            R = new Spell.Active(SpellSlot.R);
            Cutlass = new Item(3144, 550);
            Botrk = new Item(3153, 550);
            Youmuu = new Item(3142, 0);
            Titanic_Hydra = new Item(3748, 385);
            Ravenous_Hydra = new Item(3074, 385);
            Tiamat = new Item(3077, 385);
        }

        private static void Menu()
        {
            var main = MainMenu.AddMenu("Xin Zhao", "sexsiXin");
            var ComboMenu = main.AddSubMenu("Combo");
            {
                ComboQ = ComboMenu.Add("ComboQ", new CheckBox("Use Q in Combo"));
                ComboQAA = ComboMenu.Add("ComboQAA", new CheckBox("Reset AA with Q"));
                ComboW = ComboMenu.Add("ComboW", new CheckBox("Use W in Combo"));
                ComboE = ComboMenu.Add("ComboE", new CheckBox("Use E in Combo"));
                ComboEmin = ComboMenu.Add("ComboEmin", new Slider("E min range", 250, 20, 400));
                ComboR = ComboMenu.Add("ComboR", new CheckBox("Use R in Combo"));
                ComboRmin = ComboMenu.Add("ComboRmin", new Slider("Use R if enemies X >", 2, 1, 5));
                ComboItems = ComboMenu.Add("ComboItems", new CheckBox("Use Items"));
                ComboSmite = ComboMenu.Add("ComboSmite", new CheckBox("Use Smite in Combo"));
            }
            var FarmMenu = main.AddSubMenu("Jungle Clear");
            {
                FarmMana = FarmMenu.Add("FarmMana", new Slider("Mana percent for clear", 10));
                FarmQ = FarmMenu.Add("FarmQ", new CheckBox("Jungle Clear with Q"));
                FarmW = FarmMenu.Add("FarmW", new CheckBox("Jungle Clear with W"));
                FarmE = FarmMenu.Add("FarmE", new CheckBox("Jungle Clear with E"));
            }
            var FarmLMenuL = main.AddSubMenu("Lane Clear");
            {
                FarmManaL = FarmLMenuL.Add("FarmMana", new Slider("Mana percent for clear", 10));
                FarmLQ = FarmLMenuL.Add("FarmLQ", new CheckBox("Lane Clear with Q"));
                FarmLW = FarmLMenuL.Add("FarmLW", new CheckBox("Lane Clear with W"));
                FarmLE = FarmLMenuL.Add("FarmLE", new CheckBox("Lane Clear with E"));
            }
            var MiscMenu = main.AddSubMenu("Misc Menu");
            {
                InterruptR = MiscMenu.Add("InterruptR", new CheckBox("Use R to Interrupt"));
            }
        }

        private static void Combo()
        {
            foreach (var Enemy in EntityManager.Heroes.Enemies)
            {
                if (Enemy.IsValidTarget())
                {
                    if (Smite != null && Smite.IsReady() && ComboSmite.CurrentValue)
                    {
                        var target = TargetSelector.SelectedTarget == null
                            ? TargetSelector.GetTarget(700, DamageType.Physical)
                            : TargetSelector.SelectedTarget;
                        if (target != null && target.IsValidTarget() && !target.IsDead && (target.Position - Player.Position).Length() < 500)
                        {

                            Smite.Cast(target);
                        }
                        else
                        {
                            Smite.Cast(target);
                        }
                    }
                    if (Cutlass.IsOwned() && Cutlass.IsReady() && ComboItems.CurrentValue && !Player.IsDead)
                    {
                        var target = TargetSelector.SelectedTarget == null
                            ? TargetSelector.GetTarget(550, DamageType.Physical)
                            : TargetSelector.SelectedTarget;
                        if (target != null && target.IsValidTarget() && !target.IsDead && Cutlass.IsInRange(target))
                        {
                            Cutlass.Cast(target);
                        }
                        else
                        {
                            Cutlass.Cast(target);
                        }
                    }
                    if (Botrk.IsOwned() && Botrk.IsReady() && ComboItems.CurrentValue && !Player.IsDead)
                    {
                        var target = TargetSelector.SelectedTarget == null
                            ? TargetSelector.GetTarget(550, DamageType.Physical)
                            : TargetSelector.SelectedTarget;
                        if (target != null && target.IsValidTarget() && !target.IsDead && Botrk.IsInRange(target))
                        {
                            Botrk.Cast(target);
                        }
                        else
                        {
                            Botrk.Cast(target);
                        }
                    }
                    if (Youmuu.IsOwned() && Youmuu.IsReady() && ComboItems.CurrentValue && !Player.IsDead)
                    {
                        if (Player.CountEnemyChampionsInRange(500) > 0)
                        {
                            Youmuu.Cast();
                        }
                    }
                    if (Ravenous_Hydra.IsOwned() && Ravenous_Hydra.IsReady() && ComboItems.CurrentValue && !Player.IsDead)
                    {
                        if (Player.CountEnemyChampionsInRange(385) > 0)
                        {
                            Ravenous_Hydra.Cast();
                        }
                    }
                    if (Tiamat.IsOwned() && Tiamat.IsReady() && ComboItems.CurrentValue && !Player.IsDead)
                    {
                        if (Player.CountEnemyChampionsInRange(385) > 0)
                        {
                            Tiamat.Cast();
                        }
                    }
                    if (Titanic_Hydra.IsOwned() && Titanic_Hydra.IsReady() && ComboItems.CurrentValue && !Player.IsDead)
                    {
                        if (Player.CountEnemyChampionsInRange(385) > 0)
                        {
                            Titanic_Hydra.Cast();
                        }
                    }
                    if (!ComboQAA.CurrentValue && ComboQ.CurrentValue && Q.IsReady() && Enemy.IsValidTarget(E.Range))
                    {
                        Q.Cast();
                    }
                    if (!Enemy.IsDead && Enemy != null && Enemy.IsValidTarget(E.Range) && ComboE.CurrentValue)
                    {
                        if ((Enemy.Position - Player.Position).Length() > ComboEmin.CurrentValue)
                        {
                            E.Cast(Enemy);
                        }
                        if (E.GetSpellDamage(Enemy) >= Enemy.TotalShieldHealth())
                        {
                            E.Cast(Enemy);
                        }
                        if (Enemy.IsDashing())
                        {
                            E.Cast(Enemy);
                        }
                        if (Enemy.HealthPercent < 15)
                        {
                            E.Cast(Enemy);
                        }
                    }
                    if (ComboW.CurrentValue && W.IsReady())
                    {
                        var target = TargetSelector.GetTarget(250, DamageType.Physical);
                        if (target != null)
                        {
                            W.Cast();
                        }

                    }
                    if (ComboR.CurrentValue && R.IsReady())
                    {
                        var RDamage = E.GetSpellDamage(Enemy);
                        if (Player.CountEnemyChampionsInRange(R.Range) >= ComboRmin.CurrentValue)
                        {
                            if (Enemy.HasBuff("xenzhaointimidate"))
                            {
                                R.Cast();

                            }

                        }
                        if (Enemy.TotalShieldHealth() < RDamage)
                        {
                            R.Cast();

                        }

                    }
                }
            }
        }

        private static void Farm()
        {
            if (Player.ManaPercent > FarmMana.CurrentValue)
            {
                foreach (var Minion in EntityManager.MinionsAndMonsters.GetJungleMonsters())
                {
                    if (Minion.IsEnemy && !Minion.IsDead && Minion.IsValidTarget())
                    {
                        if (FarmQ.CurrentValue && Q.IsReady() && Minion.IsValidTarget(Q.Range))
                        {
                            Q.Cast();
                        }
                        if (FarmE.CurrentValue && E.IsReady() && Minion.IsValidTarget(E.Range))
                        {
                            E.Cast(Minion);
                        }
                        if (FarmW.CurrentValue && W.IsReady() && Minion.IsValidTarget(250))
                        {
                            W.Cast();
                        }

                    }
                }
            }
            if (Player.ManaPercent > FarmManaL.CurrentValue)
            {
                foreach (var Minion in EntityManager.MinionsAndMonsters.GetLaneMinions())
                {
                    if (Minion.IsEnemy && !Minion.IsDead && Minion.IsValidTarget())
                    {
                        if (FarmLQ.CurrentValue && Q.IsReady() && Minion.IsValidTarget(Q.Range))
                        {
                            Q.Cast();
                        }
                        if (FarmLE.CurrentValue && E.IsReady() && Minion.IsValidTarget(E.Range))
                        {
                            E.Cast(Minion);
                        }
                        if (FarmLW.CurrentValue && W.IsReady() && Minion.IsValidTarget(250))
                        {
                            W.Cast();
                        }

                    }
                }
            }

        }
    }
}
