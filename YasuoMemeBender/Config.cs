using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace YasuoMemeBender
{
    internal class Config
    {
        public static Menu gapcloseMenu,
            towerDiveMenu,
            spellMenu,
            spellEMenu,
            spellRMenu,
            comboMenu,
            killstealMenu,
            harassMenu,
            lasthitMenu,
            laneclearMenu,
            jungleclearMenu,
            antiMenu;

        public Config()
        {
            var Menu = MainMenu.AddMenu("Yasuo - The Last memebender", "YLM");

            gapcloseMenu = Menu.AddSubMenu("Gapclose settings", "ylm.gapclose"); //done
            towerDiveMenu = Menu.AddSubMenu("Tower dive settings", "ylm.towerdive"); //done 
            spellMenu = Menu.AddSubMenu("Spell Settings", "ylm.spell"); //done
            spellEMenu = Menu.AddSubMenu("E - Sweeping Blade settings", "ylm.spellSetting.E"); //done
            spellRMenu = Menu.AddSubMenu("R - Last Breath settings", "ylm.spellSetting.R"); //done
            comboMenu = Menu.AddSubMenu("Combo", "ylm.combo"); //done - Items - Summoners
            killstealMenu = Menu.AddSubMenu("Killsteal", "ylm.killsteal");
            harassMenu = Menu.AddSubMenu("Harass", "ylm.mixed"); //done
            lasthitMenu = Menu.AddSubMenu("Lasthit", "ylm.lasthit"); //done
            laneclearMenu = Menu.AddSubMenu("Laneclear", "ylm.laneclear"); //done
            jungleclearMenu = Menu.AddSubMenu("Jungleclear", "ylm.jungleclear"); //done
            //var drawMenu = Menu.AddSubMenu("Drawings", "ylm.drawings");
            antiMenu = Menu.AddSubMenu("Interrupt & Antigapclose", "ylm.anti"); //done


            #region GapClose

            gapcloseMenu.Add("ylm.gapclose.on", new CheckBox("Use Gapclose"));
            gapcloseMenu.Add("ylm.gapclose.hpcheck", new CheckBox("Check health before gapclosing"));
            gapcloseMenu.Add("ylm.gapclose.hpcheck2", new Slider("Only gapclose if my health > %",15));
            gapcloseMenu.Add("ylm.gapclose.stackQ", new CheckBox("Stack Q while gapclosing", false));
            gapcloseMenu.AddSeparator();
            gapcloseMenu.Add("ylm.gapclose.limit", new Slider("Set gapclose range", 3200, 650, 3200));
            gapcloseMenu.Add("ylm.gapclose.draw", new CheckBox("Draw gapclose target"));

            #endregion

            #region Tower Dive

            towerDiveMenu.Add("ylm.towerdive.enabled", new CheckBox("Tower Dive", false));
            towerDiveMenu.Add("ylm.towerdive.minAllies", new Slider("Min number of allies to dive", 3, 0, 5));

            #endregion

            #region Spell Settings

            spellEMenu.Add(
                "ylm.spelle.range", new CheckBox("Check distance of target and E endpos"));
            spellEMenu.Add(
                "ylm.spelle.rangeslider", new Slider("Maximum distance", 200, 150, 400));
            spellEMenu.AddSeparator();
            spellEMenu.AddLabel("> This option will check if the distance");
            spellEMenu.AddLabel("> between your target and the endposition of your E cast");
            spellEMenu.AddLabel("> is greater then the distance set in the slider.");
            spellEMenu.AddLabel("> If yes the cast will get blocked!");
            spellEMenu.AddLabel("> This prevents dashing too far away from your target!");

            spellRMenu.Add("ylm.spellr.auto", new CheckBox("Use Auto Ultimate",false));
            spellRMenu.Add(
                "ylm.spellr.targetnumber", new Slider("Number of Targets for Auto R",3,1,5));
            spellRMenu.AddLabel("Auto R ignores settings below and only checks for X targets");
            spellEMenu.AddSeparator();
            spellRMenu.Add("ylm.spellr.delay", new CheckBox("Delay the ultimate for more CC"));
            spellRMenu.Add("ylm.spellr.useqult", new CheckBox("Use Q while ulting"));
            spellRMenu.Add("ylm.spellr.towercheck", new CheckBox("Use Ultimate under towers"));
            spellEMenu.AddSeparator();
            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                spellRMenu.Add(
                    $"ylm.spellr.rtarget.{enemy.ChampionName.ToLower()}",
                    new ComboBox(enemy.ChampionName, 2, "Don't Use", "Use if killable", "Always use",
                        "Use advanced checks for target"));
            }
            spellEMenu.AddSeparator();
            spellRMenu.AddLabel(">> Advanced settings");
            spellRMenu.Add("ylm.spellr.targethealth", new CheckBox("Check for target health"));
            spellRMenu.Add(
                "ylm.spellr.targethealthslider", new Slider("Only ult if target health below < %",40));
            spellRMenu.Add("ylm.spellr.playerhealth", new CheckBox("Check for our health"));
            spellRMenu.Add(
                "ylm.spellr.playerhealthslider", new Slider("Only ult if player health bigger > %",40));


            spellEMenu.AddSeparator();
            spellMenu.Add("ylm.spell.autolevelon", new CheckBox("Auto Level Enable/Disable"));
            spellMenu.Add(
                "ylm.spell.autolevelskills",
                new ComboBox("Auto Level Skills", 0, "No Autolevel", "QEWQ - R>Q>E>W", "EQWQ - R>Q>E>W",
                    "EQEWE - R>Q>E>W", "QEWE - R>E>Q>W"));

            #endregion

            #region Combo

            comboMenu.Add("ylm.combo.mode", new ComboBox("Choose Combo mode", 0 , "Prefer Q3-E", "Prefer E-Q3"));
            comboMenu.Add("ylm.combo.items", new CheckBox("Use items in Combo"));
            comboMenu.AddSeparator();
            comboMenu.Add("ylm.combo.useq", new CheckBox("Use Q"));
            comboMenu.Add("ylm.combo.useq3", new CheckBox("Use Q3"));
            comboMenu.Add("ylm.combo.usee", new CheckBox("Use E"));
            comboMenu.Add("ylm.combo.user", new CheckBox("Use R"));
            comboMenu.Add("ylm.combo.useignite", new CheckBox("Use Ignite"));

            #endregion

            #region Harass

            harassMenu.Add("ylm.mixed.mode", new ComboBox("Choose Harass mode", 0 , "Normal Harass", "Safe Harass"));
            harassMenu.Add("ylm.mixed.lasthit", new CheckBox("Last hit"));
            harassMenu.AddLabel("Will use spellsettings from the lasthitmenu");
            harassMenu.AddSeparator();
            harassMenu.AddGroupLabel("Harass abilities");
            harassMenu.Add("ylm.mixed.useq", new CheckBox("Use Q"));
            harassMenu.Add("ylm.mixed.useq3", new CheckBox("Use Q"));
            harassMenu.Add("ylm.mixed.usee", new CheckBox("Use E"));

            #endregion

            #region Killsteal

            killstealMenu.Add("ylm.killsteal.usesmartks", new CheckBox("Use Smart Killsteal"));
            killstealMenu.AddSeparator();
            killstealMenu.Add("ylm.killsteal.useq", new CheckBox("Use Q"));
            killstealMenu.Add("ylm.killsteal.useq3", new CheckBox("Use Q3"));
            killstealMenu.Add("ylm.killsteal.usee", new CheckBox("Use E"));
            killstealMenu.Add("ylm.killsteal.user", new CheckBox("Use R"));

            #endregion

            #region Lasthit

            lasthitMenu.Add("ylm.lasthit.enabled", new CheckBox("Use skills to lasthit", false));
            lasthitMenu.Add("ylm.lasthit.useq", new CheckBox("Use Q"));
            lasthitMenu.Add("ylm.lasthit.useq3", new CheckBox("Use Q3"));
            lasthitMenu.Add("ylm.lasthit.usee", new CheckBox("Use E"));

            #endregion

            #region Laneclear
            laneclearMenu.Add(
                "ylm.laneclear.changeWithScroll", new CheckBox("Toggle LaneClear skills with scrolling wheel",false));
            laneclearMenu.Add("ylm.laneclear.enabled", new CheckBox("Use skills to laneclear"));
            laneclearMenu.Add("ylm.laneclear.modee", new ComboBox("Choose Laneclear mode for E",0, "Only lasthit with E", "Always use E"));
            laneclearMenu.Add("ylm.laneclear.modeq3", new ComboBox("Choose Laneclear mode for Q3", 0 , "Cast to best position", "Cast to X or more amount of units"));
            laneclearMenu.Add("ylm.laneclear.modeq3amount", new Slider("Min units to hit with Q3", 3, 1, 8));
            laneclearMenu.Add("ylm.laneclear.items", new CheckBox("Use items for Laneclear"));
            laneclearMenu.AddSeparator();
            laneclearMenu.Add("ylm.laneclear.useq", new CheckBox("Use Q"));
            laneclearMenu.Add("ylm.laneclear.useq3", new CheckBox("Use Q3"));
            laneclearMenu.Add("ylm.laneclear.usee", new CheckBox("Use E"));

            #endregion

            #region Jungleclear

            jungleclearMenu.Add("ylm.jungleclear.enabled", new CheckBox("Use skills to jungle clear"));
            jungleclearMenu.Add("ylm.jungleclear.items", new CheckBox("Use items for Jungleclear"));
            jungleclearMenu.AddSeparator();
            jungleclearMenu.Add("ylm.jungleclear.useq", new CheckBox("Use Q"));
            jungleclearMenu.Add("ylm.jungleclear.useq3", new CheckBox("Use Q3"));
            jungleclearMenu.Add("ylm.jungleclear.usee", new CheckBox("Use E"));

            #endregion

            #region Drawings

            //drawMenu.Add("ylm.drawings.drawdamage", "Draw damage on Healthbar").SetValue(new Circle(true, Color.GreenYellow)));
            //drawMenu.Add(
            //    "ylm.drawings.drawspellsready", "Draw spells only if not on cooldown").SetValue(true));
            //drawMenu.Add("ylm.drawings.drawq", "Draw Q").SetValue(new Circle(true, Color.DarkOrange)));
            //drawMenu.Add("ylm.drawings.draww", "Draw W").SetValue(new Circle(true, Color.DarkOrange)));
            //drawMenu.Add("ylm.drawings.drawe", "Draw E").SetValue(new Circle(true, Color.DarkOrange)));
            //drawMenu.Add("ylm.drawings.drawr", "Draw R").SetValue(new Circle(true, Color.DarkOrange)));

            #endregion

            #region Interrupt & Anti Gapclose

            antiMenu.AddGroupLabel("Anti-Interrupt");
            antiMenu.Add("ylm.anti.interrupt.useq3", new CheckBox("Use Q3"));
            antiMenu.AddSeparator();
            foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(h => h.IsEnemy))
            {
                antiMenu.Add($"ylm.anti.interrupt.{hero.ChampionName}", new CheckBox(hero.ChampionName));
            }

            antiMenu.AddGroupLabel("Anti-Gapcloser");
            antiMenu.Add("ylm.anti.gapclose.useq3", new CheckBox("Use Q3"));
             

            #endregion

        }
    }
}
