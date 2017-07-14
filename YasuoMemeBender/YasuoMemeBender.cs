using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using YasuoMemeBender.Skills;

namespace YasuoMemeBender
{
    internal class YasuoMemeBender
    {
        private bool doingEQCombo;

        public int Knockup = 0;

        public YasuoMemeBender()
        {
            if (ObjectManager.Player.ChampionName != "Yasuo")
            {
                return;
            }
            OnLoad();
        }

        public void SetUpEvents()
        {
            new Config();
            Game.OnUpdate += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc; //Changing laneclear with scroll wheel
            Interrupter.OnInterruptableSpell += Interrupter2_OnInterruptableTarget;
            Gapcloser.OnGapcloser += OnEnemyCustomGapcloser;
        }

        public void SetUpSkills()
        {
            SteelTempest.Q = new Spell.Skillshot(SpellSlot.Q, 475, SkillShotType.Linear, 400, int.MaxValue, 20);
            SteelTempest.QEmp = new Spell.Skillshot(SpellSlot.Q, 475, SkillShotType.Linear, 400, 1000, 90);
        }

        public void OnLoad()
        {
            SetUpEvents();
            SetUpSkills(); 
            Chat.Print("Loaded Yasuo - The Last MemeBender");
        }

        public void Game_OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            if (Config.spellRMenu["ylm.spellr.auto"].Cast<CheckBox>().CurrentValue)
            {
                LastBreath.AutoUltimate();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) | Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                Clear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Escape();
            }
        }

        private void OnEnemyCustomGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs args)
        {
            if (!SteelTempest.Empowered || !SteelTempest.Q.IsReady() || !sender.IsEnemy)
                return;
            SteelTempest.CastQ(sender);
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs interruptableSpellEventArgs)
        {
            var unit = sender as AIHeroClient;

            if (unit != null && !sender.IsEnemy || (!Config.antiMenu["ylm.anti.interrupt.useq3"].Cast<CheckBox>().CurrentValue ||
                                 !Config.antiMenu[string.Format("ylm.anti.interrupt.{0}", unit.ChampionName)].Cast<CheckBox>().CurrentValue))
                return;

            if (!SteelTempest.Empowered || !SteelTempest.Q.IsReady())
                return;

            SteelTempest.CastQ(unit);
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != 0x20a || !Config.laneclearMenu["ylm.laneclear.changeWithScroll"].Cast<CheckBox>().CurrentValue)
                return;

            Config.laneclearMenu["ylm.laneclear.enabled"].Cast<CheckBox>().CurrentValue = !Config.laneclearMenu["ylm.laneclear.enabled"].Cast<CheckBox>().CurrentValue;
        }

        public void Combo()
        {
            if (doingEQCombo)
            {
                if (ObjectManager.Player.IsDashing()
                    && SteelTempest.Q.IsReady() && SteelTempest.Empowered &&
                    ObjectManager.Player.CountEnemyChampionsInRange(250) > 0)
                {
                    SteelTempest.QDash.Cast();
                    doingEQCombo = false;
                }
                if (doingEQCombo)
                {
                    return;
                }
            }
            var comboTarget = TargetSelector.GetTarget(LastBreath.R.Range, DamageType.Physical);

            if (comboTarget != null && comboTarget.IsValid)
            {
                if (YasuoUtils.DecideKnockup(comboTarget))
                {
                    var bestUnit = YasuoUtils.BestQDashKnockupUnit();
                    if (bestUnit != null)
                    {
                        doingEQCombo = true;
                        SweepingBlade.E.Cast(bestUnit);
                        Core.DelayAction(() => { doingEQCombo = false; }, 100);
                    }
                }
                else
                {
                    SweepingBlade.GapClose(comboTarget);
                }
            }
            else
            {
                SweepingBlade.GapClose();
                return;
            }

            var targetDistance = comboTarget.Distance(ObjectManager.Player, true);
            if (Config.comboMenu["ylm.combo.useq"].Cast<CheckBox>().CurrentValue && SteelTempest.Q.IsReady() &&
                targetDistance <= SteelTempest.Q.RangeSquared)
            {
                SteelTempest.Q.Cast(comboTarget);                     
            }

            if (Config.comboMenu["ylm.combo.useq3"].Cast<CheckBox>().CurrentValue && SteelTempest.Empowered && SteelTempest.Q.IsReady() &&
                targetDistance <= SteelTempest.QEmp.RangeSquared)
            {
                SteelTempest.QEmp.Cast(comboTarget);                     
            }
            if (Config.comboMenu["ylm.combo.user"].Cast<CheckBox>().CurrentValue && LastBreath.ShouldUlt(comboTarget))
            {
                LastBreath.CastR(comboTarget);
            }
            //TODO: Items
        }

        public void Harass()
        {
            var qRange = SteelTempest.Empowered ? SteelTempest.QEmp.Range : SteelTempest.Q.Range;
            var useQ = (SteelTempest.Empowered && Config.harassMenu["ylm.mixed.useq3"].Cast<CheckBox>().CurrentValue)
                       || (!SteelTempest.Empowered && Config.harassMenu["ylm.mixed.useq"].Cast<CheckBox>().CurrentValue);

            var eTarget = TargetSelector.GetTarget(SweepingBlade.E.Range, DamageType.Magical);
            var qTarget = TargetSelector.GetTarget(qRange, DamageType.Physical);

            if (useQ && qTarget != null && SteelTempest.Q.IsReady())
            {
                if (ObjectManager.Player.IsDashing() &&
                    ObjectManager.Player.Distance(qTarget, true) < -375)
                {
                    SteelTempest.QDash.Cast();
                }
                else
                {
                    if (SteelTempest.Empowered)
                    {
                        SteelTempest.QEmp.Cast(qTarget);
                    }
                    else
                    {
                        SteelTempest.Q.Cast(qTarget);
                    }
                }
            }

            if (eTarget != null && Config.harassMenu["ylm.mixed.usee"].Cast<CheckBox>().CurrentValue && SweepingBlade.CanCastE(eTarget))
            {
                if (Config.harassMenu["ylm.mixed.mode"].Cast<ComboBox>().CurrentValue == 0)
                {
                    SweepingBlade.E.Cast(eTarget);
                }
                else
                {
                    if (!SteelTempest.Q.IsReady() && eTarget.Distance(ObjectManager.Player, true) <= 200 * 200)
                        //Run away we useless :^(
                    {
                        SweepingBlade.RunAway(eTarget);
                    }
                    else if (SteelTempest.Q.IsReady())
                    {
                        SweepingBlade.E.Cast(eTarget);
                    }
                }
            }
            if (Config.harassMenu["ylm.mixed.lasthit"].Cast<CheckBox>().CurrentValue)
            {
                SteelTempest.LastHitQ();
                SweepingBlade.LaneE(true);
            }
        }

        public void LastHit()
        {
            if (Config.lasthitMenu["ylm.lasthit.enabled"].Cast<CheckBox>().CurrentValue)
            {
                SteelTempest.LastHitQ();
                SweepingBlade.LaneE(true);
            }
        }

        public void Clear()
        {
            if (Config.laneclearMenu["ylm.laneclear.enabled"].Cast<CheckBox>().CurrentValue)
            {
                SteelTempest.ClearQ();
                SweepingBlade.LaneE();
            }
            if (Config.jungleclearMenu["ylm.jungleclear.enabled"].Cast<CheckBox>().CurrentValue)
            {
                SteelTempest.ClearQ(true);
                SweepingBlade.JungleClearE();
            }
        }

        public void Escape()
        {
            var escapeTarget = YasuoUtils.ClosestMinion(Game.CursorPos);
            if (escapeTarget != null)
            {
                SweepingBlade.GapClose(escapeTarget, true);
            }
            if (SteelTempest.Empowered && SteelTempest.Q.IsReady() && !ObjectManager.Player.IsDashing())
            {
                var q3Target = TargetSelector.GetTarget(SteelTempest.QEmp.Range, DamageType.Physical);
                if (q3Target != null)
                {
                    SteelTempest.QEmp.Cast(q3Target);
                }
            }
        }


    }
}
