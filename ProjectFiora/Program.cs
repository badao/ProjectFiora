using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using LeagueSharp.Common.Data;
using ItemData = LeagueSharp.Common.Data.ItemData;
using FioraProject.Evade;

namespace FioraProject
{
    using static FioraPassive;
    using static GetTargets;
    using static Combos;
    public static class Program
    {
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public static Orbwalking.Orbwalker Orbwalker;

        public static Spell Q, W, E, R;

        public static Menu Menu;


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Fiora")
                return;
            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 750);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);
            W.SetSkillshot(0.75f, 80, 2000, false, SkillshotType.SkillshotLine);
            W.MinHitChance = HitChance.High;


            Menu = new Menu("Project" + Player.ChampionName, Player.ChampionName, true);
            Menu.SetFontStyle(System.Drawing.FontStyle.Bold,SharpDX.Color.DeepPink);

            Menu orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
            Orbwalker = new FioraProject.Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);
            Menu ts = Menu.AddSubMenu(new Menu("Target Selector", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);
            Menu spellMenu = Menu.AddSubMenu(new Menu("Spell", "Spell"));

            Menu Harass = spellMenu.AddSubMenu(new Menu("Harass", "Harass"));

            Menu Combo = spellMenu.AddSubMenu(new Menu("Combo", "Combo"));

            Menu Target = Menu.AddSubMenu(new Menu("Targeting Modes", "Targeting Modes"));

            Menu PriorityMode = Target.AddSubMenu(new Menu("Priority", "Priority Mode"));

            Menu OptionalMode = Target.AddSubMenu(new Menu("Optional", "Optional Mode"));

            Menu SelectedMode = Target.AddSubMenu(new Menu("Selected", "Selected Mode"));

            Menu LaneClear = spellMenu.AddSubMenu(new Menu("Lane Clear", "Lane Clear"));

            spellMenu.AddItem(new MenuItem("Orbwalk Last Right Click", "Orbwalk Last Right Click")
                .SetValue(new KeyBind('Y', KeyBindType.Press))).ValueChanged += OrbwalkLastClick.OrbwalkLRCLK_ValueChanged;

            Menu JungClear = spellMenu.AddSubMenu(new Menu("Jungle Clear", "Jungle Clear"));

            Menu Misc = Menu.AddSubMenu(new Menu("Misc","Misc"));

            Menu Draw = Menu.AddSubMenu(new Menu("Draw", "Draw")); ;

            Harass.AddItem(new MenuItem("Use Q Harass", "Q Enable").SetValue(true));
            Harass.AddItem(new MenuItem("Use Q Harass Gap", "Use Q to gapclose").SetValue(true));
            Harass.AddItem(new MenuItem("Use Q Harass Pre Pass", "Use Q to hit pre-passive spot").SetValue(true));
            Harass.AddItem(new MenuItem("Use Q Harass Pass", "Use Q to hit passive").SetValue(true));
            Harass.AddItem(new MenuItem("Use E Harass", "E Enable").SetValue(true));
            Harass.AddItem(new MenuItem("Mana Harass", "Mana Harass").SetValue(new Slider(40, 0, 100)));

            Combo.AddItem(new MenuItem("Use Q Combo", "Q Enable").SetValue(true));
            Combo.AddItem(new MenuItem("Use Q Combo Gap", "Use Q to gapclose").SetValue(true));
            Combo.AddItem(new MenuItem("Use Q Combo Pre Pass", "Use Q to hit pre-passive spot").SetValue(true));
            Combo.AddItem(new MenuItem("Use Q Combo Pass", "Use Q to hit passive").SetValue(true));
            Combo.AddItem(new MenuItem("Use Q Combo Gap Minion", "Use Q minion to gapclose").SetValue(false));
            Combo.AddItem(new MenuItem("Use Q Combo Gap Minion Value", "Q minion gapclose if % cdr >=").SetValue(new Slider(25, 0, 40)));
            Combo.AddItem(new MenuItem("Use E Combo", "E Enable").SetValue(true));
            Combo.AddItem(new MenuItem("Use R Combo", "R Enable").SetValue(true));
            Combo.AddItem(new MenuItem("Use R Combo LowHP", "Use R LowHP").SetValue(true));
            Combo.AddItem(new MenuItem("Use R Combo LowHP Value", "R LowHP if player hp <").SetValue(new Slider(40, 0, 100)));
            Combo.AddItem(new MenuItem("Use R Combo Killable", "Use R Killable").SetValue(true));
            Combo.AddItem(new MenuItem("Use R Combo On Tap", "Use R on Tap").SetValue(true));
            Combo.AddItem(new MenuItem("Use R Combo On Tap Key", "R on Tap key").SetValue(new KeyBind('G', KeyBindType.Press)));
            Combo.AddItem(new MenuItem("Use R Combo Always", "Use R Always").SetValue(false));

            Target.AddItem(new MenuItem("Targeting Mode", "Targeting Mode").SetValue(new StringList(new string[] { "Optional", "Selected", "Priority", "Normal" })));
            Target.AddItem(new MenuItem("Orbwalk To Passive Range", "Orbwalk To Passive Range").SetValue(new Slider(300, 250, 500)));
            Target.AddItem(new MenuItem("Focus Ulted Target", "Focus Ulted Target").SetValue(false));
            Target.AddItem(new MenuItem("Note1", "Go in each Mode menu to customize what you want!"));
            Target.AddItem(new MenuItem("Note2", "Please remember Orbwalk to Passive spot only works"));
            Target.AddItem(new MenuItem("Note3", "in \" Combo Orbwalk to Passive\" mode can be found"));
            Target.AddItem(new MenuItem("Note4", "in orbwalker menu!"));

            PriorityMode.AddItem(new MenuItem("Priority Range", "Priority Range").SetValue(new Slider(1000, 300, 1000)));
            PriorityMode.AddItem(new MenuItem("Priority Orbwalk to Passive", "Orbwalk to Passive").SetValue(true));
            PriorityMode.AddItem(new MenuItem("Priority Under Tower", "Under Tower").SetValue(true));
            foreach (var hero in HeroManager.Enemies)
            {
                PriorityMode.AddItem(new MenuItem("Priority" + hero.ChampionName, hero.ChampionName).SetValue(new Slider(2, 1, 5)));
            }

            OptionalMode.AddItem(new MenuItem("Optional Range", "Optional Range").SetValue(new Slider(1000, 300, 1000)));
            OptionalMode.AddItem(new MenuItem("Optional Orbwalk to Passive", "Orbwalk to Passive").SetValue(true));
            OptionalMode.AddItem(new MenuItem("Optional Under Tower", "Under Tower").SetValue(false));
            OptionalMode.AddItem(new MenuItem("Optional Switch Target Key", "Switch Target Key").SetValue(new KeyBind('T', KeyBindType.Press)));
            OptionalMode.AddItem(new MenuItem("Note5", "Also Can Left-click the target to switch!"));

            SelectedMode.AddItem(new MenuItem("Selected Range", "Selected Range").SetValue(new Slider(1000, 300, 1000)));
            SelectedMode.AddItem(new MenuItem("Selected Orbwalk to Passive", "Orbwalk to Passive").SetValue(true));
            SelectedMode.AddItem(new MenuItem("Selected Under Tower", "Under Tower").SetValue(false));
            SelectedMode.AddItem(new MenuItem("Selected Switch If No Selected", "Switch to Optional if no target").SetValue(true));

            LaneClear.AddItem(new MenuItem("Use E LClear", "E Enable").SetValue(true));
            LaneClear.AddItem(new MenuItem("Use Timat LClear", "Tiamat Enable").SetValue(true));
            LaneClear.AddItem(new MenuItem("minimum Mana LC", "minimum Mana").SetValue(new Slider(40, 0, 100)));

            JungClear.AddItem(new MenuItem("Use E JClear", "E Enable").SetValue(true));
            JungClear.AddItem(new MenuItem("Use Timat JClear", "Tiamat Enable").SetValue(true));
            JungClear.AddItem(new MenuItem("minimum Mana JC", "minimum Mana").SetValue(new Slider(40, 0, 100)));

            Misc.AddItem(new MenuItem("WallJump","WallJump").SetValue(new KeyBind('H',KeyBindType.Press)));

            Draw.AddItem(new MenuItem("Draw Q", "Draw Q").SetValue(false));
            Draw.AddItem(new MenuItem("Draw W", "Draw W").SetValue(false));
            Draw.AddItem(new MenuItem("Draw Optional Range", "Draw Optional Range").SetValue(true));
            Draw.AddItem(new MenuItem("Draw Selected Range", "Draw Selected Range").SetValue(true));
            Draw.AddItem(new MenuItem("Draw Priority Range", "Draw Priority Range").SetValue(true));
            Draw.AddItem(new MenuItem("Draw Target", "Draw Target").SetValue(true));
            Draw.AddItem(new MenuItem("Draw Vitals", "Draw Vitals").SetValue(false));
            Draw.AddItem(new MenuItem("Draw Fast Damage", "Draw Fast Damage").SetValue(false)).ValueChanged += DrawHP_ValueChanged;

            if (HeroManager.Enemies.Any())
            {
                Evade.Evade.Init();
                EvadeTarget.Init();
                TargetedNoMissile.Init();
                OtherSkill.Init();
            }
            OrbwalkLastClick.Init();
            Menu.AddToMainMenu();
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;

            //GameObject.OnCreate += GameObject_OnCreate;
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += AfterAttack;
            Orbwalking.AfterAttackNoTarget += Orbwalking_AfterAttackNoTarget;
            Orbwalking.OnAttack += OnAttack;
            Obj_AI_Base.OnProcessSpellCast += oncast;
            Game.OnWndProc += Game_OnWndProc;
            //Utility.HpBarDamageIndicator.DamageToUnit = GetFastDamage;
            //Utility.HpBarDamageIndicator.Enabled = DrawHP;
            CustomDamageIndicator.Initialize(GetFastDamage);
            CustomDamageIndicator.Enabled = DrawHP;

            //evade
            FioraProject.Evade.Evade.Evading += EvadeSkillShots.Evading;
            Game.PrintChat("Welcome to FioraWorld");
        }

        // events 
        public static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
                return;
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.OrbwalkPassive
                || OrbwalkLastClickActive)
            {
                if (Ecombo && E.IsReady())
                {
                    E.Cast();
                }
                else if (HasItem())
                {
                    CastItem();
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && (unit is Obj_AI_Hero))
            {
                if (Eharass && E.IsReady() && Player.ManaPercent >= Manaharass)
                {
                    E.Cast();
                }
                else if (HasItem())
                {
                    CastItem();
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                // jungclear
                if (EJclear && E.IsReady() && Player.Mana * 100 / Player.MaxMana >= ManaJclear && !Orbwalker.ShouldWait()
                    && Player.Position.CountMinionsInRange(Orbwalking.GetRealAutoAttackRange(Player) + 200, true) >= 1)
                {
                    E.Cast();
                }
                else if (TimatJClear && HasItem() && !Orbwalker.ShouldWait()
                    && Player.Position.CountMinionsInRange(Orbwalking.GetRealAutoAttackRange(Player) + 200, true) >= 1)
                {
                    CastItem();
                }
                // laneclear
                if (ELclear && E.IsReady() && Player.Mana * 100 / Player.MaxMana >= ManaLclear && !Orbwalker.ShouldWait()
                    && Player.Position.CountMinionsInRange(Orbwalking.GetRealAutoAttackRange(Player) + 200, false) >= 1)
                {
                    E.Cast();
                }
                else if (TimatLClear && HasItem() && !Orbwalker.ShouldWait()
                    && Player.Position.CountMinionsInRange(Orbwalking.GetRealAutoAttackRange(Player) + 200, false) >= 1)
                {
                    CastItem();
                }
            }

        }
        private static void Orbwalking_AfterAttackNoTarget(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
                return;
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.OrbwalkPassive
                || OrbwalkLastClickActive)
            {
                if (Ecombo && E.IsReady() && Player.CountEnemiesInRange(Orbwalking.GetRealAutoAttackRange(Player) + 200) >= 1)
                {
                    E.Cast();
                }
                else if (HasItem() && Player.CountEnemiesInRange(Orbwalking.GetRealAutoAttackRange(Player) + 200) >= 1)
                {
                    CastItem();
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && (unit is Obj_AI_Hero))
            {
                if (Eharass && E.IsReady() && Player.ManaPercent >= Manaharass
                    && Player.CountEnemiesInRange(Orbwalking.GetRealAutoAttackRange(Player) + 200) >= 1)
                {
                    E.Cast();
                }
                else if (HasItem() && Player.CountEnemiesInRange(Orbwalking.GetRealAutoAttackRange(Player) + 200) >= 1)
                {
                    CastItem();
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                // jungclear
                if (EJclear && E.IsReady() && Player.Mana * 100 / Player.MaxMana >= ManaJclear && !Orbwalker.ShouldWait()
                    && Player.Position.CountMinionsInRange(Orbwalking.GetRealAutoAttackRange(Player) + 200, true) >= 1)
                {
                    E.Cast();
                }
                else if (TimatJClear && HasItem() && !Orbwalker.ShouldWait()
                    && Player.Position.CountMinionsInRange(Orbwalking.GetRealAutoAttackRange(Player) + 200, true) >= 1)
                {
                    CastItem();
                }
                // laneclear
                if (ELclear && E.IsReady() && Player.Mana * 100 / Player.MaxMana >= ManaLclear && !Orbwalker.ShouldWait()
                    && Player.Position.CountMinionsInRange(Orbwalking.GetRealAutoAttackRange(Player) + 200, false) >= 1)
                {
                    E.Cast();
                }
                else if (TimatLClear && HasItem() && !Orbwalker.ShouldWait()
                    && Player.Position.CountMinionsInRange(Orbwalking.GetRealAutoAttackRange(Player) + 200, false) >= 1)
                {
                    CastItem();
                }
            }
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
            FioraPassiveUpdate();
            OrbwalkToPassive();
            WallJump();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.OrbwalkPassive)
            {
                Combo();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {

            }
        }
        public static void oncast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spell = args.SData;
            if (!sender.IsMe)
                return;
            if (spell.Name.Contains("ItemTiamatCleave"))
            {

            }
            if (spell.Name.Contains("FioraQ"))
            {

            }
            if (spell.Name == "FioraE")
            {

                Orbwalking.ResetAutoAttackTimer();
            }
            if (spell.Name == "ItemTitanicHydraCleave")
            {
                Orbwalking.ResetAutoAttackTimer();
            }
            if (spell.Name.ToLower().Contains("fiorabasicattack"))
            {
            }

        }
        public static void OnAttack(AttackableUnit unit, AttackableUnit target)
        {

            if (unit.IsMe
                && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo
                || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.OrbwalkPassive
                || OrbwalkLastClickActive))
            {
                if (ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                    ItemData.Youmuus_Ghostblade.GetItem().Cast();
            }

        }


        //harass
        public static bool Qharass { get { return Menu.Item("Use Q Harass").GetValue<bool>(); } }
        private static bool Eharass { get { return Menu.Item("Use E Harass").GetValue<bool>(); } }
        public static bool CastQGapCloseHarass { get { return Menu.Item("Use Q Harass Gap").GetValue<bool>(); } }
        public static bool CastQPrePassiveHarass { get { return Menu.Item("Use Q Harass Pre Pass").GetValue<bool>(); } }
        public static bool CastQPassiveHarasss { get { return Menu.Item("Use Q Harass Pass").GetValue<bool>(); } }
        public static int Manaharass { get { return Menu.Item("Mana Harass").GetValue<Slider>().Value; } }

        //combo
        public static bool Qcombo { get { return Menu.Item("Use Q Combo").GetValue<bool>(); } }
        private static bool Ecombo { get { return Menu.Item("Use E Combo").GetValue<bool>(); } }
        public static bool CastQGapCloseCombo { get { return Menu.Item("Use Q Combo Gap").GetValue<bool>(); } }
        public static bool CastQPrePassiveCombo { get { return Menu.Item("Use Q Combo Pre Pass").GetValue<bool>(); } }
        public static bool CastQPassiveCombo { get { return Menu.Item("Use Q Combo Pass").GetValue<bool>(); } }
        public static bool CastQMinionGapCloseCombo { get { return Menu.Item("Use Q Combo Gap Minion").GetValue<bool>(); } }
        public static int ValueQMinionGapCloseCombo { get { return Menu.Item("Use Q Combo Gap Minion Value").GetValue<Slider>().Value; } }
        public static bool Rcombo { get { return Menu.Item("Use R Combo").GetValue<bool>(); } }
        public static bool UseRComboLowHP { get { return Menu.Item("Use R Combo LowHP").GetValue<bool>(); } }
        public static int ValueRComboLowHP { get { return Menu.Item("Use R Combo LowHP Value").GetValue<Slider>().Value; } }
        public static bool UseRComboKillable { get { return Menu.Item("Use R Combo Killable").GetValue<bool>(); } }
        public static bool UseRComboOnTap { get { return Menu.Item("Use R Combo On Tap").GetValue<bool>(); } }
        public static bool RTapKeyActive { get { return Menu.Item("Use R Combo On Tap Key").GetValue<KeyBind>().Active; } }
        public static bool UseRComboAlways { get { return Menu.Item("Use R Combo Always").GetValue<bool>(); } }

        //jclear && lclear
        private static bool ELclear { get { return Menu.Item("Use E LClear").GetValue<bool>(); } }
        private static bool TimatLClear { get { return Menu.Item("Use Timat LClear").GetValue<bool>(); } }
        private static bool EJclear { get { return Menu.Item("Use E JClear").GetValue<bool>(); } }
        private static bool TimatJClear { get { return Menu.Item("Use Timat JClear").GetValue<bool>(); } }
        public static int ManaJclear { get { return Menu.Item("minimum Mana JC").GetValue<Slider>().Value; } }
        public static int ManaLclear { get { return Menu.Item("minimum Mana LC").GetValue<Slider>().Value; } }

        //orbwalkpassive
        private static float OrbwalkToPassiveRange { get { return Menu.Item("Orbwalk To Passive Range").GetValue<Slider>().Value; } }
        private static bool OrbwalkToPassiveTargeted { get { return Menu.Item("Selected Orbwalk to Passive").GetValue<bool>(); } }
        private static bool OrbwalkToPassiveOptional { get { return Menu.Item("Optional Orbwalk to Passive").GetValue<bool>(); } }
        private static bool OrbwalkToPassivePriority { get { return Menu.Item("Priority Orbwalk to Passive").GetValue<bool>(); } }
        private static bool OrbwalkTargetedUnderTower { get { return Menu.Item("Selected Under Tower").GetValue<bool>(); } }
        private static bool OrbwalkOptionalUnderTower { get { return Menu.Item("Optional Under Tower").GetValue<bool>(); } }
        private static bool OrbwalkPriorityUnderTower { get { return Menu.Item("Priority Under Tower").GetValue<bool>(); } }

        // orbwalklastclick
        public static bool OrbwalkLastClickActive { get { return Menu.Item("Orbwalk Last Right Click").GetValue<KeyBind>().Active; } }

        #region Drawing
        private static bool DrawQ { get { return Menu.Item("Draw Q").GetValue<bool>(); } }
        private static bool DrawW { get { return Menu.Item("Draw W").GetValue<bool>(); } }
        private static bool DrawQcast { get { return Menu.Item("Draw Q cast").GetValue<bool>(); } }
        private static bool DrawOptionalRange { get { return Menu.Item("Draw Optional Range").GetValue<bool>(); } }
        private static bool DrawSelectedRange { get { return Menu.Item("Draw Selected Range").GetValue<bool>(); } }
        private static bool DrawPriorityRange { get { return Menu.Item("Draw Priority Range").GetValue<bool>(); } }
        private static bool DrawTarget { get { return Menu.Item("Draw Target").GetValue<bool>(); } }
        private static bool DrawHP { get { return Menu.Item("Draw Fast Damage").GetValue<bool>(); } }
        private static bool DrawVitals { get { return Menu.Item("Draw Vitals").GetValue<bool>(); } }
        private static void DrawHP_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            if (sender != null)
            {
                //Utility.HpBarDamageIndicator.Enabled = e.GetNewValue<bool>();
                CustomDamageIndicator.Enabled = e.GetNewValue<bool>();
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;
            if (DrawQ)
                Render.Circle.DrawCircle(Player.Position, 400, Color.Green);
            if (DrawW)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.Green);
            }
            if (DrawOptionalRange && TargetingMode == TargetMode.Optional)
            {
                Render.Circle.DrawCircle(Player.Position, OptionalRange, Color.DeepPink);
            }
            if (DrawSelectedRange && TargetingMode == TargetMode.Selected)
            {
                Render.Circle.DrawCircle(Player.Position, SelectedRange, Color.DeepPink);
            }
            if (DrawPriorityRange && TargetingMode == TargetMode.Priority)
            {
                Render.Circle.DrawCircle(Player.Position, PriorityRange, Color.DeepPink);
            }
            if (DrawTarget && TargetingMode != TargetMode.Normal)
            {
                var hero = GetTarget();
                if (hero != null)
                    Render.Circle.DrawCircle(hero.Position, 75, Color.Yellow, 5);
            }
            if (DrawVitals && TargetingMode != TargetMode.Normal)
            {
                var hero = GetTarget();
                if (hero != null)
                {
                    var status = hero.GetPassiveStatus(0f);
                    if (status.HasPassive && status.PassivePredictedPositions.Any())
                    {
                        foreach (var x in status.PassivePredictedPositions)
                        {
                            Render.Circle.DrawCircle(x.To3D(), 50, Color.Yellow);
                        }
                    }
                }
            }
            if (activewalljump)
            {
                var Fstwall = GetFirstWallPoint(Player.Position.To2D(), Game.CursorPos.To2D());
                if (Fstwall != null)
                {
                    var firstwall =((Vector2)Fstwall);
                    var pos = firstwall.Extend(Game.CursorPos.To2D(), 100);
                    var Lstwall = GetLastWallPoint(firstwall, Game.CursorPos.To2D());
                    if (Lstwall != null)
                    {
                        var lastwall = ((Vector2)Lstwall);
                        if (InMiddileWall(firstwall,lastwall))
                        {
                        for (int i = 0; i <= 359; i++)
                        {
                            var pos1 = pos.RotateAround(firstwall, i);
                            var pos2 = firstwall.Extend(pos1, 400);
                            if (pos1.InTheCone(firstwall, Game.CursorPos.To2D(), 60) && pos1.IsWall() && !pos2.IsWall())
                            {
                                Render.Circle.DrawCircle(firstwall.To3D(), 50, Color.Green);
                                goto Finish;
                            }
                        }

                        Render.Circle.DrawCircle(firstwall.To3D(), 50, Color.Red);
                        }
                    }
                }
            Finish: ;
            }

        }
        private static void Drawing_OnEndScene(EventArgs args)
        {
        }

        #endregion Drawing

        #region WallJump
        public static bool usewalljump = true;
        public static bool activewalljump { get { return Menu.Item("WallJump").GetValue<KeyBind>().Active; } }
        public static int movetick;
        public static void WallJump()
        {
            if (usewalljump && activewalljump)
            {
                var Fstwall = GetFirstWallPoint(Player.Position.To2D(), Game.CursorPos.To2D());
                if (Fstwall != null)
                {
                    var firstwall = ((Vector2)Fstwall);
                    var Lstwall = GetLastWallPoint(firstwall, Game.CursorPos.To2D());
                    if (Lstwall != null)
                    {
                        var lastwall = ((Vector2)Lstwall);
                        if (InMiddileWall(firstwall, lastwall))
                        {
                            var y = Player.Position.Extend(Game.CursorPos, 30);
                            for (int i = 20; i <= 300; i = i + 20)
                            {
                                if (Utils.GameTimeTickCount - movetick < (70 + Math.Min(60, Game.Ping)))
                                    break;
                                if (Player.Distance(Game.CursorPos) <= 1200 && Player.Position.To2D().Extend(Game.CursorPos.To2D(), i).IsWall())
                                {
                                    Player.IssueOrder(GameObjectOrder.MoveTo, Player.Position.To2D().Extend(Game.CursorPos.To2D(), i - 20).To3D());
                                    movetick = Utils.GameTimeTickCount;
                                    break;
                                }
                                Player.IssueOrder(GameObjectOrder.MoveTo,
                                    Player.Distance(Game.CursorPos) <= 1200 ?
                                    Player.Position.To2D().Extend(Game.CursorPos.To2D(), 200).To3D() :
                                    Game.CursorPos);
                            }
                            if (y.IsWall() && Prediction.GetPrediction(Player, 500).UnitPosition.Distance(Player.Position) <= 10 && Q.IsReady())
                            {
                                var pos = Player.Position.To2D().Extend(Game.CursorPos.To2D(), 100);
                                for (int i = 0; i <= 359; i++)
                                {
                                    var pos1 = pos.RotateAround(Player.Position.To2D(), i);
                                    var pos2 = Player.Position.To2D().Extend(pos1, 400);
                                    if (pos1.InTheCone(Player.Position.To2D(), Game.CursorPos.To2D(), 60) && pos1.IsWall() && !pos2.IsWall())
                                    {
                                        Q.Cast(pos2);
                                    }

                                }
                            }
                        }
                        else if (Utils.GameTimeTickCount - movetick >= (70 + Math.Min(60, Game.Ping)))
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                            movetick = Utils.GameTimeTickCount;
                        }
                    }
                    else if (Utils.GameTimeTickCount - movetick >= (70 + Math.Min(60, Game.Ping)))
                    {
                        Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        movetick = Utils.GameTimeTickCount;
                    }
                }
                else if (Utils.GameTimeTickCount - movetick >= (70 + Math.Min(60, Game.Ping)))
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    movetick = Utils.GameTimeTickCount;
                }
            }
        }
        public static Vector2? GetFirstWallPoint(Vector2 from, Vector2 to, float step = 25)
        {
            var direction = (to - from).Normalized();

            for (float d = 0; d < from.Distance(to); d = d + step)
            {
                var testPoint = from + d * direction;
                var flags = NavMesh.GetCollisionFlags(testPoint.X, testPoint.Y);
                if (flags.HasFlag(CollisionFlags.Wall) || flags.HasFlag(CollisionFlags.Building))
                {
                    return from + (d - step) * direction;
                }
            }

            return null;
        }
        public static Vector2? GetLastWallPoint (Vector2 from, Vector2 to , float step = 25)
        {
            var direction = (to - from).Normalized();
            var Fstwall = GetFirstWallPoint(from, to);
            if (Fstwall != null)
            {
                var firstwall = ((Vector2)Fstwall);
                for (float d = step; d < firstwall.Distance(to) + 1000; d = d + step)
                {
                    var testPoint = firstwall + d * direction;
                    var flags = NavMesh.GetCollisionFlags(testPoint.X, testPoint.Y);
                    if (!flags.HasFlag(CollisionFlags.Wall) && !flags.HasFlag(CollisionFlags.Building))
                    //if (!testPoint.IsWall())
                    {
                        return firstwall + d * direction;
                    }
                }
            }

            return null;
        }
        public static bool InMiddileWall (Vector2 firstwall, Vector2 lastwall)
        {
            var midwall = new Vector2((firstwall.X + lastwall.X)/2,(firstwall.Y + lastwall.Y)/2);
            var point = midwall.Extend(Game.CursorPos.To2D(), 50);
            for (int i = 0; i <= 350; i = i + 10  )
            {
                var testpoint = point.RotateAround(midwall, i);
                var flags = NavMesh.GetCollisionFlags(testpoint.X,testpoint.Y);
                if (!flags.HasFlag(CollisionFlags.Wall) && !flags.HasFlag(CollisionFlags.Building))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion WallJump

        #region OrbwalkToPassive
        private static void OrbwalkToPassive()
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.OrbwalkPassive)
            {
                var target = GetTarget(OrbwalkToPassiveRange);
                if (target.IsValidTarget(OrbwalkToPassiveRange) && !target.IsZombie)
                {
                    var status = target.GetPassiveStatus(0);
                    if (Player.Position.To2D().Distance(target.Position.To2D()) <= OrbwalkToPassiveRange && status.HasPassive
                        && ((TargetingMode == TargetMode.Selected && OrbwalkToPassiveTargeted && (OrbwalkTargetedUnderTower || !Player.UnderTurret(true)))
                        || (TargetingMode == TargetMode.Optional && OrbwalkToPassiveOptional && (OrbwalkOptionalUnderTower || !Player.UnderTurret(true)))
                        || (TargetingMode == TargetMode.Priority && OrbwalkToPassivePriority && (OrbwalkPriorityUnderTower || !Player.UnderTurret(true)))))
                    {
                        var point = status.PassivePredictedPositions.OrderBy(x => x.Distance(Player.Position.To2D())).FirstOrDefault();
                        point = point.IsValid() ? point : Game.CursorPos.To2D();
                        Orbwalker.SetOrbwalkingPoint(point.To3D());
                    }
                    else Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                }
                else Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
            }
            else Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
        }
        #endregion OrbwalkToPassive








<<<<<<< HEAD
=======
        #region GetTarget

        public static TargetMode TargetingMode
        {
            get
            {
                var menuindex = Menu.Item("Targeting Mode").GetValue<StringList>().SelectedIndex;
                switch (menuindex)
                {
                    case 0:
                        return TargetMode.Optional;
                    case 1:
                        return TargetMode.Selected;
                    case 2:
                        return TargetMode.Priority;
                    default:
                        return TargetMode.Normal;
                }

            }
        }
        public enum TargetMode
        {
            Normal = 0,
            Optional = 1,
            Selected = 2,
            Priority = 3
        }
        public static Obj_AI_Hero GetTarget(float range = 500)
        {
            if (TargetingMode == TargetMode.Normal)
                return GetStandarTarget(range);
            if (TargetingMode == TargetMode.Optional)
                return GetOptionalTarget();
            if (TargetingMode == TargetMode.Priority)
                return GetPriorityTarget();
            if (TargetingMode == TargetMode.Selected)
                return GetSelectedTarget();
            return null;
        }
        #region Normal
        public static Obj_AI_Hero GetStandarTarget(float range)
        {
            var ulted = GetUltedTarget();
            if (ulted.IsValidTarget(500))
                return ulted;
            return TargetSelector.GetTarget(range, TargetSelector.DamageType.Physical);
        }
        #endregion Normal

        #region Priority
        public static float PriorityRange { get { return Menu.Item("Priority Range").GetValue<Slider>().Value; } }
        public static int PriorityValue(Obj_AI_Hero target)
        {
            return Menu.Item("Priority" + target.ChampionName).GetValue<Slider>().Value;
        }
        public static Obj_AI_Hero GetPriorityTarget()
        {
            var ulted = GetUltedTarget();
            if (ulted.IsValidTarget(PriorityRange))
                return ulted;
            return HeroManager.Enemies.Where(x => x.IsValidTarget(PriorityRange) && !x.IsZombie)
                                    .OrderByDescending(x => PriorityValue(x))
                                    .ThenBy(x => x.Health)
                                    .FirstOrDefault();
        }
        #endregion Priority

        #region Selected
        public static float SelectedRange { get { return Menu.Item("Selected Range").GetValue<Slider>().Value; } }
        public static bool SwitchIfNoTargeted { get { return Menu.Item("Selected Switch If No Selected").GetValue<bool>(); } }
        public static Obj_AI_Hero GetSelectedTarget()
        {
            var ulted = GetUltedTarget();
            if (ulted.IsValidTarget(SelectedRange))
                return ulted;
            var tar = TargetSelector.GetSelectedTarget();
            var tarD = tar.IsValidTarget(SelectedRange) && !tar.IsZombie ? tar : null;
            if (tarD != null)
                return tarD;
            else
            {
                if (SwitchIfNoTargeted)
                    return GetOptionalTarget();
                return null;
            }
        }
        #endregion Selected

        #region Optional
        public static Obj_AI_Hero SuperOldOptionalTarget = null;
        public static Obj_AI_Hero OldOptionalTarget = null;
        public static Obj_AI_Hero PreOptionalTarget = null;
        public static Obj_AI_Hero OptionalTarget = null;
        public static float OptionalRange { get { return Menu.Item("Optional Range").GetValue<Slider>().Value; } }
        public static uint OptionalKey { get { return Menu.Item("Optional Switch Target Key").GetValue<KeyBind>().Key; } }
        public static Obj_AI_Hero GetOptionalTarget()
        {
            var ulted = GetUltedTarget();
            if (ulted.IsValidTarget(OptionalRange))
            {
                OptionalTarget = ulted;
                return OptionalTarget;
            }
            if (OptionalTarget.IsValidTarget(OptionalRange) && !OptionalTarget.IsZombie)
                return OptionalTarget;
            OptionalTarget = HeroManager.Enemies.Where(x => x.IsValidTarget(OptionalRange) && !x.IsZombie)
                                .OrderBy(x => Player.Distance(x.Position)).FirstOrDefault();
            return OptionalTarget;
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (uint)WindowsMessages.WM_KEYDOWN)
            {
                if (args.WParam == (uint)OptionalKey)
                {
                    OptionalTarget = GetOptionalTarget();
                    if (OptionalTarget == null)
                    {
                        PreOptionalTarget = HeroManager.Enemies.Where(x => x.IsValidTarget(OptionalRange) && !x.IsZombie)
                                                       .OrderBy(x => OldOptionalTarget != null ? x.NetworkId == OldOptionalTarget.NetworkId : x.IsEnemy)
                                                       .ThenBy(x => Player.Distance(x.Position)).FirstOrDefault();
                        if (PreOptionalTarget != null)
                        {
                            OptionalTarget = PreOptionalTarget;
                        }
                        return;
                    }
                    PreOptionalTarget = HeroManager.Enemies.Where(x => x.IsValidTarget(OptionalRange) && !x.IsZombie && x.NetworkId != OptionalTarget.NetworkId)
                                                   .OrderBy(x => OldOptionalTarget != null ? x.NetworkId == OldOptionalTarget.NetworkId : x.IsEnemy)
                                                   .ThenBy(x => Player.Distance(x.Position)).FirstOrDefault();
                    if (PreOptionalTarget != null)
                    {
                        OldOptionalTarget = OptionalTarget;
                        OptionalTarget = PreOptionalTarget;
                    }
                    return;
                }
            }
            if (args.Msg == (uint)WindowsMessages.WM_LBUTTONDOWN)
            {
                OptionalTarget = GetOptionalTarget();
                if (OptionalTarget == null)
                {
                    PreOptionalTarget = HeroManager.Enemies.Where(x => x.IsValidTarget(OptionalRange)
                                                    && x.IsValidTarget(400, true, Game.CursorPos) && !x.IsZombie)
                                                   .OrderBy(x => Game.CursorPos.To2D().Distance(x.Position.To2D())).FirstOrDefault();
                    if (PreOptionalTarget != null)
                    {
                        OptionalTarget = PreOptionalTarget;
                    }
                    return;
                }
                PreOptionalTarget = HeroManager.Enemies.Where(x => x.IsValidTarget(OptionalRange)
                                                && x.IsValidTarget(400, true, Game.CursorPos) && !x.IsZombie)
                                               .OrderBy(x => Game.CursorPos.To2D().Distance(x.Position.To2D())).FirstOrDefault();
                if (PreOptionalTarget != null)
                {
                    OldOptionalTarget = OptionalTarget;
                    OptionalTarget = PreOptionalTarget;
                }
                return;
            }
        }
        #endregion Optional

        #endregion GetTarget

        #region Item
        public static bool HasItem()
        {
            if (ItemData.Tiamat_Melee_Only.GetItem().IsReady() || ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady()
                || Items.CanUseItem(3748))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void CastItem()
        {

            if (ItemData.Tiamat_Melee_Only.GetItem().IsReady())
                ItemData.Tiamat_Melee_Only.GetItem().Cast();
            if (ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
                ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
            // titanic hydra
            if (Items.CanUseItem(3748))
                Items.UseItem(3748);
        }
        #endregion Item

        #region FioraPassive
        public static Obj_AI_Hero GetUltedTarget()
        {
            return HeroManager.Enemies.FirstOrDefault(x => x != null && x.IsValid && FioraUltiPassiveObjects
                                .Any(i => i != null && i.IsValid && i.Position.To2D().Distance(x.Position.To2D()) <= 50));
        }
        public static List<Vector2> GetRadiusPoints(Vector2 targetpredictedpos, Vector2 passivepredictedposition)
        {
            List<Vector2> RadiusPoints = new List<Vector2>();
            for (int i = 50; i <= 300; i = i + 25)
            {
                var x = targetpredictedpos.Extend(passivepredictedposition, i);
                for (int j = -45; j <= 45; j = j + 5)
                {
                    RadiusPoints.Add(x.RotateAround(targetpredictedpos, j * (float)(Math.PI / 180)));
                }
            }
            return RadiusPoints;
        }
        public static PassiveStatus GetPassiveStatus(this Obj_AI_Hero target, float delay = 0.25f)
        {
            var allobjects = GetPassiveObjects()
                .Where(x => x.Object != null && x.Object.IsValid
                           && x.Object.Position.To2D().Distance(target.Position.To2D()) <= 50);
            var targetpredictedpos = Prediction.GetPrediction(target, delay).UnitPosition.To2D();
            if (!allobjects.Any())
            {
                return new PassiveStatus(false, PassiveType.None, new Vector2(), new List<PassiveDirection>(), new List<Vector2>());
            }
            else
            {
                var x = allobjects.First();
                var listdirections = new List<PassiveDirection>();
                foreach (var a in allobjects)
                {
                    listdirections.Add(a.PassiveDirection);
                }
                var listpositions = new List<Vector2>();
                foreach (var a in listdirections)
                {
                    if (a == PassiveDirection.NE)
                    {
                        var pos = targetpredictedpos;
                        pos.Y = pos.Y + 200;
                        listpositions.Add(pos);
                    }
                    else if (a == PassiveDirection.NW)
                    {
                        var pos = targetpredictedpos;
                        pos.X = pos.X + 200;
                        listpositions.Add(pos);
                    }
                    else if (a == PassiveDirection.SE)
                    {
                        var pos = targetpredictedpos;
                        pos.X = pos.X - 200;
                        listpositions.Add(pos);
                    }
                    else if (a == PassiveDirection.SW)
                    {
                        var pos = targetpredictedpos;
                        pos.Y = pos.Y - 200;
                        listpositions.Add(pos);
                    }
                }
                if (x.PassiveType == PassiveType.PrePassive)
                {
                    return new PassiveStatus(true, PassiveType.PrePassive, targetpredictedpos, listdirections, listpositions);
                }
                if (x.PassiveType == PassiveType.NormalPassive)
                {
                    return new PassiveStatus(true, PassiveType.NormalPassive, targetpredictedpos, listdirections, listpositions);
                }
                if (x.PassiveType == PassiveType.UltiPassive)
                {
                    return new PassiveStatus(true, PassiveType.UltiPassive, targetpredictedpos, listdirections, listpositions);
                }
                return new PassiveStatus(false, PassiveType.None, new Vector2(), new List<PassiveDirection>(), new List<Vector2>());
            }
        }
        public static List<PassiveObject> GetPassiveObjects()
        {
            List<PassiveObject> PassiveObjects = new List<PassiveObject>();
            foreach (var x in FioraPrePassiveObjects.Where(i => i != null && i.IsValid))
            {
                PassiveObjects.Add(new PassiveObject(x.Name, x, PassiveType.PrePassive, GetPassiveDirection(x)));
            }
            foreach (var x in FioraPassiveObjects.Where(i => i != null && i.IsValid))
            {
                PassiveObjects.Add(new PassiveObject(x.Name, x, PassiveType.NormalPassive, GetPassiveDirection(x)));
            }
            foreach (var x in FioraUltiPassiveObjects.Where(i => i != null && i.IsValid))
            {
                PassiveObjects.Add(new PassiveObject(x.Name, x, PassiveType.UltiPassive, GetPassiveDirection(x)));
            }
            return PassiveObjects;
        }
        public static PassiveDirection GetPassiveDirection(Obj_GeneralParticleEmitter x)
        {
            if (x.Name.Contains("NE"))
            {
                return PassiveDirection.NE;
            }
            else if (x.Name.Contains("SE"))
            {
                return PassiveDirection.SE;
            }
            else if (x.Name.Contains("NW"))
            {
                return PassiveDirection.NW;
            }
            else
            {
                return PassiveDirection.SW;
            }
        }
        public class PassiveStatus
        {
            public bool HasPassive;
            public PassiveType PassiveType;
            public Vector2 TargetPredictedPosition;
            public List<PassiveDirection> PassiveDirections = new List<PassiveDirection>();
            public List<Vector2> PassivePredictedPositions = new List<Vector2>();
            public PassiveStatus(bool hasPassive, PassiveType passiveType, Vector2 targetPredictedPosition
                , List<PassiveDirection> passiveDirections, List<Vector2> passivePredictedPositions)
            {
                HasPassive = hasPassive;
                PassiveType = passiveType;
                TargetPredictedPosition = targetPredictedPosition;
                PassiveDirections = passiveDirections;
                PassivePredictedPositions = passivePredictedPositions;
            }
        }
        public enum PassiveType
        {
            None, PrePassive, NormalPassive, UltiPassive
        }
        public enum PassiveDirection
        {
            NE, SE, NW, SW
        }
        public class PassiveObject
        {
            public string PassiveName;
            public Obj_GeneralParticleEmitter Object;
            public PassiveType PassiveType;
            public PassiveDirection PassiveDirection;
            public PassiveObject(string passiveName, Obj_GeneralParticleEmitter obj, PassiveType passiveType, PassiveDirection passiveDirection)
            {
                PassiveName = passiveName;
                Object = obj;
                PassiveType = passiveType;
                PassiveDirection = passiveDirection;
            }
        }
        private static List<Obj_GeneralParticleEmitter> FioraUltiPassiveObjects = new List<Obj_GeneralParticleEmitter>();
        //{
        //    get
        //    {
        //        var x = ObjectManager.Get<Obj_GeneralParticleEmitter>()
        //        .Where(a => a.Name.Contains("Fiora_Base_R_Mark") || (a.Name.Contains("Fiora_Base_R") && a.Name.Contains("Timeout_FioraOnly.troy")))
        //        .ToList();
        //        return x;
        //    }
        //}
        private static List<Obj_GeneralParticleEmitter> FioraPassiveObjects = new List<Obj_GeneralParticleEmitter>();
        //{
        //    get
        //    {
        //        var x = ObjectManager.Get<Obj_GeneralParticleEmitter>().Where(a => FioraPassiveName.Contains(a.Name)).ToList();
        //        return x;
        //    }
        //}
        private static List<Obj_GeneralParticleEmitter> FioraPrePassiveObjects = new List<Obj_GeneralParticleEmitter>();
        //{
        //    get
        //    {
        //        var x = ObjectManager.Get<Obj_GeneralParticleEmitter>().Where(a => FioraPrePassiveName.Contains(a.Name)).ToList();
        //        return x;
        //    }
        //}
        private static List<string> FioraPassiveName = new List<string>()
        {
            "Fiora_Base_Passive_NE.troy",
            "Fiora_Base_Passive_SE.troy",
            "Fiora_Base_Passive_NW.troy",
            "Fiora_Base_Passive_SW.troy",
            "Fiora_Base_Passive_NE_Timeout.troy",
            "Fiora_Base_Passive_SE_Timeout.troy",
            "Fiora_Base_Passive_NW_Timeout.troy",
            "Fiora_Base_Passive_SW_Timeout.troy"
        };
        private static List<string> FioraPrePassiveName = new List<string>()
        {
            "Fiora_Base_Passive_NE_Warning.troy",
            "Fiora_Base_Passive_SE_Warning.troy",
            "Fiora_Base_Passive_NW_Warning.troy",
            "Fiora_Base_Passive_SW_Warning.troy"
        };
        private static void FioraPassiveUpdate()
        {
            FioraPrePassiveObjects = new List<Obj_GeneralParticleEmitter>();
            FioraPassiveObjects = new List<Obj_GeneralParticleEmitter>();
            FioraUltiPassiveObjects = new List<Obj_GeneralParticleEmitter>();
            var ObjectEmitter = ObjectManager.Get<Obj_GeneralParticleEmitter>().ToList();
            FioraPrePassiveObjects.AddRange(ObjectEmitter.Where(a => FioraPrePassiveName.Contains(a.Name)));
            FioraPassiveObjects.AddRange(ObjectEmitter.Where(a => FioraPassiveName.Contains(a.Name)));
            FioraUltiPassiveObjects.AddRange(ObjectEmitter
                .Where(a =>
                       a.Name.Contains("Fiora_Base_R_Mark")
                       || (a.Name.Contains("Fiora_Base_R") && a.Name.Contains("Timeout_FioraOnly.troy"))));
        }
        #endregion FioraPassive

        #region Math And Extensions
        public static int CountMinionsInRange(this Vector3 Position, float Range, bool JungleTrueEnemyFalse)
        {
            return
                MinionManager.GetMinions(Range,MinionTypes.All,JungleTrueEnemyFalse? MinionTeam.Neutral:MinionTeam.Enemy).Count;
        }
        public static float AngleToRadian(this int Angle)
        {
            return Angle * (float)Math.PI / 180f;
        }
        public static bool InTheCone(this Vector2 pos, Vector2 centerconePolar, Vector2 centerconeEnd, double coneAngle)
        {
            return AngleBetween(pos, centerconePolar, centerconeEnd) < coneAngle / 2;
        }
        public static double AngleBetween(Vector2 a, Vector2 center, Vector2 c)
        {
            float a1 = c.Distance(center);
            float b1 = a.Distance(c);
            float c1 = center.Distance(a);
            if (a1 == 0 || c1 == 0) { return 0; }
            else
            {
                return Math.Acos((a1 * a1 + c1 * c1 - b1 * b1) / (2 * a1 * c1)) * (180 / Math.PI);
            }
        }
        public static Vector2 RotateAround(this Vector2 pointToRotate, Vector2 centerPoint, float angleInRadians)
        {
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Vector2
            {
                X =
                    (float)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (float)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }
        public static double AngleBetween(Vector2 a, Vector2 b)
        {
            var Theta1 = Math.Atan2(a.Y, a.X);
            var Theta2 = Math.Atan2(b.Y, b.X);
            var Theta = Math.Abs(Theta1 - Theta2);
            return
                Theta > 180 ? 360 - Theta : Theta;
        }
        #endregion  Math And Extensions

        #region HINH1
        private enum DrawType
        {
            Circle = 1,
            HINH1 = 2
        }
        private static int drawtick = 0;
        private static int drawstate = 0;
        private static void DrawDraw(Vector3 center, float radius, Color color, DrawType DrawedType, int width = 5)
        {
            switch (DrawedType)
            {
                case DrawType.Circle:
                    DrawCircle(center, radius, color, width);
                    break;
                case DrawType.HINH1:
                    DrawHinh1(center, radius, color, width);
                    break;
            }
        }
        private static void DrawHinh1(Vector3 center, float radius, Color color, int width = 5)
        {
            Render.Circle.DrawCircle(center, radius, color, width, false);
            return;
            var pos1y = center;
            pos1y.X = pos1y.X + radius;
            var pos1 = pos1y.To2D().RotateAround(center.To2D(), drawstate.AngleToRadian());
            var pos1a = center.Extend(pos1.To3D(), radius * 5 / 8).To2D().RotateAround(center.To2D(), (18).AngleToRadian());
            var pos2 = pos1.RotateAround(center.To2D(), (36).AngleToRadian());
            var pos3 = pos1.RotateAround(center.To2D(), (72).AngleToRadian());
            var pos4 = pos1.RotateAround(center.To2D(), (108).AngleToRadian());
            var pos5 = pos1.RotateAround(center.To2D(), (144).AngleToRadian());
            var pos6 = pos1.RotateAround(center.To2D(), (180).AngleToRadian());
            var pos7 = pos1.RotateAround(center.To2D(), (216).AngleToRadian());
            var pos8 = pos1.RotateAround(center.To2D(), (252).AngleToRadian());
            var pos9 = pos1.RotateAround(center.To2D(), (288).AngleToRadian());
            var pos10 = pos1.RotateAround(center.To2D(), (324).AngleToRadian());
            var pos2a = pos1a.RotateAround(center.To2D(), (36).AngleToRadian());
            var pos3a = pos1a.RotateAround(center.To2D(), (72).AngleToRadian());
            var pos4a = pos1a.RotateAround(center.To2D(), (108).AngleToRadian());
            var pos5a = pos1a.RotateAround(center.To2D(), (144).AngleToRadian());
            var pos6a = pos1a.RotateAround(center.To2D(), (180).AngleToRadian());
            var pos7a = pos1a.RotateAround(center.To2D(), (216).AngleToRadian());
            var pos8a = pos1a.RotateAround(center.To2D(), (252).AngleToRadian());
            var pos9a = pos1a.RotateAround(center.To2D(), (288).AngleToRadian());
            var pos10a = pos1a.RotateAround(center.To2D(), (324).AngleToRadian());
            Drawing.DrawLine(Drawing.WorldToScreen(pos1.To3D()), Drawing.WorldToScreen(pos1a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos2.To3D()), Drawing.WorldToScreen(pos1a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos2.To3D()), Drawing.WorldToScreen(pos2a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos3.To3D()), Drawing.WorldToScreen(pos2a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos3.To3D()), Drawing.WorldToScreen(pos3a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos4.To3D()), Drawing.WorldToScreen(pos3a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos4.To3D()), Drawing.WorldToScreen(pos4a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos5.To3D()), Drawing.WorldToScreen(pos4a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos5.To3D()), Drawing.WorldToScreen(pos5a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos6.To3D()), Drawing.WorldToScreen(pos5a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos6.To3D()), Drawing.WorldToScreen(pos6a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos7.To3D()), Drawing.WorldToScreen(pos6a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos7.To3D()), Drawing.WorldToScreen(pos7a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos8.To3D()), Drawing.WorldToScreen(pos7a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos8.To3D()), Drawing.WorldToScreen(pos8a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos9.To3D()), Drawing.WorldToScreen(pos8a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos9.To3D()), Drawing.WorldToScreen(pos9a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos10.To3D()), Drawing.WorldToScreen(pos9a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos10.To3D()), Drawing.WorldToScreen(pos10a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos1.To3D()), Drawing.WorldToScreen(pos10a.To3D()), width, color);

            Drawing.DrawLine(Drawing.WorldToScreen(pos1.To3D()), Drawing.WorldToScreen(pos2.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos3.To3D()), Drawing.WorldToScreen(pos2.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos3.To3D()), Drawing.WorldToScreen(pos4.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos5.To3D()), Drawing.WorldToScreen(pos4.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos5.To3D()), Drawing.WorldToScreen(pos6.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos7.To3D()), Drawing.WorldToScreen(pos6.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos7.To3D()), Drawing.WorldToScreen(pos8.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos9.To3D()), Drawing.WorldToScreen(pos8.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos9.To3D()), Drawing.WorldToScreen(pos10.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos1.To3D()), Drawing.WorldToScreen(pos10.To3D()), width, color);

            Drawing.DrawLine(Drawing.WorldToScreen(pos1a.To3D()), Drawing.WorldToScreen(pos2a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos3a.To3D()), Drawing.WorldToScreen(pos2a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos3a.To3D()), Drawing.WorldToScreen(pos4a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos5a.To3D()), Drawing.WorldToScreen(pos4a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos5a.To3D()), Drawing.WorldToScreen(pos6a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos7a.To3D()), Drawing.WorldToScreen(pos6a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos7a.To3D()), Drawing.WorldToScreen(pos8a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos9a.To3D()), Drawing.WorldToScreen(pos8a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos9a.To3D()), Drawing.WorldToScreen(pos10a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos1a.To3D()), Drawing.WorldToScreen(pos10a.To3D()), width, color);

            DrawCircle(center, radius * 2 / 8, color, width, 10);

            if (Utils.GameTimeTickCount >= drawtick + 10)
            {
                drawtick = Utils.GameTimeTickCount;
                drawstate += 2;
            }


        }

        private static void DrawHinh2(Vector3 center, float radius, Color color, int width = 5)
        {
            var n = 100 - (drawstate % 102);
            DrawCircle(center, radius * n / 100, Color.Yellow, width * 3, 10);
            DrawCircle(center, radius * (n + 20 > 100 ? n - 80 : n + 20) / 100, Color.LightGreen);
            DrawCircle(center, radius * (n + 40 > 100 ? n - 60 : n + 40) / 100, Color.Orange);
            DrawCircle(center, radius * (n + 60 > 100 ? n - 40 : n + 60) / 100, Color.LightPink);
            DrawCircle(center, radius * (n + 80 > 100 ? n - 20 : n + 80) / 100, Color.PaleVioletRed);

            if (Utils.GameTimeTickCount >= drawtick + 10)
            {
                drawtick = Utils.GameTimeTickCount;
                drawstate += 2;
            }
        }

        public static void DrawCircle(Vector3 center,
            float radius,
            Color color,
            int thickness = 5,
            int quality = 60)
        {
            Render.Circle.DrawCircle(center, radius, color, thickness, false);

            //var pointList = new List<Vector3>();
            //for (var i = 0; i < quality; i++)
            //{
            //    var angle = i * Math.PI * 2 / quality;
            //    pointList.Add(
            //        new Vector3(
            //            center.X + radius * (float)Math.Cos(angle), center.Y + radius * (float)Math.Sin(angle),
            //            center.Z));
            //}

            //for (var i = 0; i < pointList.Count; i++)
            //{
            //    var a = pointList[i];
            //    var b = pointList[i == pointList.Count - 1 ? 0 : i + 1];

            //    var aonScreen = Drawing.WorldToScreen(a);
            //    var bonScreen = Drawing.WorldToScreen(b);

            //    Drawing.DrawLine(aonScreen.X, aonScreen.Y, bonScreen.X, bonScreen.Y, thickness, color);
            //}
        }

        #endregion HINH1

        #region Evade
        private static void Evading()
        {

            var parry = Evade.EvadeSpellDatabase.Spells.FirstOrDefault(i => i.Enable && i.IsReady && i.Slot == SpellSlot.W);
            if (parry == null)
            {
                return;
            }
            var skillshot =
                Evade.Evade.SkillshotAboutToHit(Player, 0 + Game.Ping + Program.Menu.SubMenu("Evade").SubMenu("Spells").SubMenu(parry.Name).Item("WDelay").GetValue<Slider>().Value)
                    .Where(
                        i =>
                        parry.DangerLevel <= i.DangerLevel)
                    .MaxOrDefault(i => i.DangerLevel);
            if (skillshot != null)
            {
                var target = GetTarget(W.Range);
                if (target.IsValidTarget(W.Range))
                    Player.Spellbook.CastSpell(parry.Slot, target.Position);
                else
                {
                    var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(W.Range));
                    if (hero != null)
                        Player.Spellbook.CastSpell(parry.Slot, hero.Position);
                    else
                        Player.Spellbook.CastSpell(parry.Slot, Player.ServerPosition.Extend(skillshot.Start.To3D(), 100));
                }
            }
        }
        #endregion Evade

        #region Targeted Skillshot
        internal class EvadeTarget
        {
            #region Static Fields

            private static readonly List<Targets> DetectedTargets = new List<Targets>();

            private static readonly List<SpellData> Spells = new List<SpellData>();

            #endregion

            #region Methods

            internal static void Init()
            {
                LoadSpellData();

                Spells.RemoveAll(i => !HeroManager.Enemies.Any(
                a =>
                string.Equals(
                    a.ChampionName,
                    i.ChampionName,
                    StringComparison.InvariantCultureIgnoreCase)));

                var evadeMenu = new Menu("Evade Targeted SkillShot", "EvadeTarget");
                {
                    evadeMenu.Bool("W", "Use W");
                    var aaMenu = new Menu("Auto Attack", "AA");
                    {
                        aaMenu.Bool("B", "Basic Attack",false);
                        aaMenu.Slider("BHpU", "-> If Hp < (%)", 35);
                        aaMenu.Bool("C", "Crit Attack",false);
                        aaMenu.Slider("CHpU", "-> If Hp < (%)", 40);
                        evadeMenu.AddSubMenu(aaMenu);
                    }
                    foreach (var hero in
                        HeroManager.Enemies.Where(
                            i =>
                            Spells.Any(
                                a =>
                                string.Equals(
                                    a.ChampionName,
                                    i.ChampionName,
                                    StringComparison.InvariantCultureIgnoreCase))))
                    {
                        evadeMenu.AddSubMenu(new Menu("-> " + hero.ChampionName, hero.ChampionName.ToLowerInvariant()));
                    }
                    foreach (var spell in
                        Spells.Where(
                            i =>
                            HeroManager.Enemies.Any(
                                a =>
                                string.Equals(
                                    a.ChampionName,
                                    i.ChampionName,
                                    StringComparison.InvariantCultureIgnoreCase))))
                    {
                        ((Menu)evadeMenu.SubMenu(spell.ChampionName.ToLowerInvariant())).Bool(
                            spell.MissileName,
                            spell.MissileName + " (" + spell.Slot + ")",
                            false);
                    }
                }
                Menu.AddSubMenu(evadeMenu);
                Game.OnUpdate += OnUpdateTarget;
                GameObject.OnCreate += ObjSpellMissileOnCreate;
                GameObject.OnDelete += ObjSpellMissileOnDelete;
            }

            private static void LoadSpellData()
            {
                Spells.Add(
                    new SpellData { ChampionName = "Ahri", SpellNames = new[] { "ahrifoxfiremissiletwo" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData { ChampionName = "Ahri", SpellNames = new[] { "ahritumblemissile" }, Slot = SpellSlot.R });
                Spells.Add(
                    new SpellData { ChampionName = "Akali", SpellNames = new[] { "akalimota" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData { ChampionName = "Anivia", SpellNames = new[] { "frostbite" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Annie", SpellNames = new[] { "disintegrate" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Brand",
                        SpellNames = new[] { "brandconflagrationmissile" },
                        Slot = SpellSlot.E
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Brand",
                        SpellNames = new[] { "brandwildfire", "brandwildfiremissile" },
                        Slot = SpellSlot.R
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Caitlyn",
                        SpellNames = new[] { "caitlynaceintheholemissile" },
                        Slot = SpellSlot.R
                    });
                Spells.Add(
                    new SpellData { ChampionName = "Cassiopeia", SpellNames = new[] { "cassiopeiatwinfang" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Elise", SpellNames = new[] { "elisehumanq" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Ezreal",
                        SpellNames = new[] { "ezrealarcaneshiftmissile" },
                        Slot = SpellSlot.E
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "FiddleSticks",
                        SpellNames = new[] { "fiddlesticksdarkwind", "fiddlesticksdarkwindmissile" },
                        Slot = SpellSlot.E
                    });
                Spells.Add(
                    new SpellData { ChampionName = "Gangplank", SpellNames = new[] { "parley" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData { ChampionName = "Janna", SpellNames = new[] { "sowthewind" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData { ChampionName = "Kassadin", SpellNames = new[] { "nulllance" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Katarina",
                        SpellNames = new[] { "katarinaq", "katarinaqmis" },
                        Slot = SpellSlot.Q
                    });
                Spells.Add(
                    new SpellData { ChampionName = "Kayle", SpellNames = new[] { "judicatorreckoning" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Leblanc",
                        SpellNames = new[] { "leblancchaosorb", "leblancchaosorbm" },
                        Slot = SpellSlot.Q
                    });
                Spells.Add(new SpellData { ChampionName = "Lulu", SpellNames = new[] { "luluw" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData { ChampionName = "Malphite", SpellNames = new[] { "seismicshard" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "MissFortune",
                        SpellNames = new[] { "missfortunericochetshot", "missFortunershotextra" },
                        Slot = SpellSlot.Q
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Nami",
                        SpellNames = new[] { "namiwenemy", "namiwmissileenemy" },
                        Slot = SpellSlot.W
                    });
                Spells.Add(
                    new SpellData { ChampionName = "Nunu", SpellNames = new[] { "iceblast" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Pantheon", SpellNames = new[] { "pantheonq" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Ryze",
                        SpellNames = new[] { "spellflux", "spellfluxmissile" },
                        Slot = SpellSlot.E
                    });
                Spells.Add(
                    new SpellData { ChampionName = "Shaco", SpellNames = new[] { "twoshivpoison" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Shen", SpellNames = new[] { "shenvorpalstar" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData { ChampionName = "Sona", SpellNames = new[] { "sonaqmissile" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData { ChampionName = "Swain", SpellNames = new[] { "swaintorment" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Syndra", SpellNames = new[] { "syndrar" }, Slot = SpellSlot.R });
                Spells.Add(
                    new SpellData { ChampionName = "Taric", SpellNames = new[] { "dazzle" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Teemo", SpellNames = new[] { "blindingdart" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData { ChampionName = "Tristana", SpellNames = new[] { "detonatingshot" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Tristana", SpellNames = new[] { "tristanar" }, Slot = SpellSlot.R });
                Spells.Add(
                    new SpellData { ChampionName = "TwistedFate", SpellNames = new[] { "bluecardattack" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData { ChampionName = "TwistedFate", SpellNames = new[] { "goldcardattack" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData { ChampionName = "TwistedFate", SpellNames = new[] { "redcardattack" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Urgot",
                        SpellNames = new[] { "urgotheatseekinghomemissile" },
                        Slot = SpellSlot.Q
                    });
                Spells.Add(
                    new SpellData { ChampionName = "Vayne", SpellNames = new[] { "vaynecondemnmissile" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Veigar", SpellNames = new[] { "veigarprimordialburst" }, Slot = SpellSlot.R });
                Spells.Add(
                    new SpellData { ChampionName = "Viktor", SpellNames = new[] { "viktorpowertransfer" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Vladimir",
                        SpellNames = new[] { "vladimirtidesofbloodnuke" },
                        Slot = SpellSlot.E
                    });
            }

            private static void ObjSpellMissileOnCreate(GameObject sender, EventArgs args)
            {
                var missile = sender as MissileClient;
                if (missile == null || !missile.IsValid)
                {
                    return;
                }
                var caster = missile.SpellCaster as Obj_AI_Hero;
                if (caster == null || !caster.IsValid || caster.Team == Player.Team || !(missile.Target != null && missile.Target.IsMe))
                {
                    return;
                }
                var spellData =
                    Spells.FirstOrDefault(
                        i =>
                        i.SpellNames.Contains(missile.SData.Name.ToLower())
                        && Menu.SubMenu("EvadeTarget").SubMenu(i.ChampionName.ToLowerInvariant()).Item(i.MissileName).GetValue<bool>());
                if (spellData == null && Orbwalking.IsAutoAttack(missile.SData.Name)
                    && (!missile.SData.Name.ToLower().Contains("crit")
                            ? Menu.SubMenu("EvadeTarget").SubMenu("AA").Item("B").GetValue<bool>()
                              && Player.HealthPercent < Menu.SubMenu("EvadeTarget").SubMenu("AA").Item("BHpU").GetValue<Slider>().Value
                            : Menu.SubMenu("EvadeTarget").SubMenu("AA").Item("C").GetValue<bool>()
                              && Player.HealthPercent < Menu.SubMenu("EvadeTarget").SubMenu("AA").Item("CHpU").GetValue<Slider>().Value))
                {
                    spellData = new SpellData { ChampionName = caster.ChampionName, SpellNames = new[] { missile.SData.Name } };
                }
                if (spellData == null)
                {
                    return;
                }
                DetectedTargets.Add(new Targets { Start = caster.ServerPosition, Obj = missile });
            }

            private static void ObjSpellMissileOnDelete(GameObject sender, EventArgs args)
            {
                var missile = sender as MissileClient;
                if (missile == null || !missile.IsValid)
                {
                    return;
                }
                var caster = missile.SpellCaster as Obj_AI_Hero;
                if (caster == null || !caster.IsValid || caster.Team == Player.Team)
                {
                    return;
                }
                DetectedTargets.RemoveAll(i => i.Obj.NetworkId == missile.NetworkId);
            }

            private static void OnUpdateTarget(EventArgs args)
            {
                if (Player.IsDead)
                {
                    return;
                }
                if (Player.HasBuffOfType(BuffType.SpellShield) || Player.HasBuffOfType(BuffType.SpellImmunity))
                {
                    return;
                }
                if (!Menu.SubMenu("EvadeTarget").Item("W").GetValue<bool>() || !W.IsReady())
                {
                    return;
                }
                foreach (var target in
                    DetectedTargets.Where(i => W.IsInRange(i.Obj, 150 + Game.Ping * i.Obj.SData.MissileSpeed / 1000)).OrderBy(i => i.Obj.Position.Distance(Player.Position)))
                {
                    var tar = GetTarget(W.Range);
                    if (tar.IsValidTarget(W.Range))
                        Player.Spellbook.CastSpell(SpellSlot.W, tar.Position);
                    else
                    {
                        var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(W.Range));
                        if (hero != null)
                            Player.Spellbook.CastSpell(SpellSlot.W, hero.Position);
                        else
                            Player.Spellbook.CastSpell(SpellSlot.W, Player.ServerPosition.Extend(target.Start, 100));
                    }
                }
            }

            #endregion

            private class SpellData
            {
                #region Fields

                public string ChampionName;

                public SpellSlot Slot;

                public string[] SpellNames = { };

                #endregion

                #region Public Properties

                public string MissileName
                {
                    get
                    {
                        return this.SpellNames.First();
                    }
                }

                #endregion
            }

            private class Targets
            {
                #region Fields

                public MissileClient Obj;

                public Vector3 Start;

                #endregion
            }
        }
        #endregion Targeted Skillshot

        #region Targeted NoMissile
        internal class TargetedNoMissile
        {
            private static readonly List<SpellData> Spells = new List<SpellData>();

            private static readonly List<DashTarget> DetectedDashes = new List<DashTarget>();
            internal static void Init()
            {
                LoadSpellData();
                Spells.RemoveAll(i => !HeroManager.Enemies.Any(
                a =>
                string.Equals(
                    a.ChampionName,
                    i.ChampionName,
                    StringComparison.InvariantCultureIgnoreCase)));
                var evadeMenu = new Menu("Evade Targeted None-SkillShot", "EvadeTargetNone");
                {
                    evadeMenu.Bool("W", "Use W");
                    //var aaMenu = new Menu("Auto Attack", "AA");
                    //{
                    //    aaMenu.Bool("B", "Basic Attack");
                    //    aaMenu.Slider("BHpU", "-> If Hp < (%)", 35);
                    //    aaMenu.Bool("C", "Crit Attack");
                    //    aaMenu.Slider("CHpU", "-> If Hp < (%)", 40);
                    //    evadeMenu.AddSubMenu(aaMenu);
                    //}
                    foreach (var hero in
                        HeroManager.Enemies.Where(
                            i =>
                            Spells.Any(
                                a =>
                                string.Equals(
                                    a.ChampionName,
                                    i.ChampionName,
                                    StringComparison.InvariantCultureIgnoreCase))))
                    {
                        evadeMenu.AddSubMenu(new Menu("-> " + hero.ChampionName, hero.ChampionName.ToLowerInvariant()));
                    }
                    foreach (var spell in
                        Spells.Where(
                            i =>
                            HeroManager.Enemies.Any(
                                a =>
                                string.Equals(
                                    a.ChampionName,
                                    i.ChampionName,
                                    StringComparison.InvariantCultureIgnoreCase))))
                    {
                        ((Menu)evadeMenu.SubMenu(spell.ChampionName.ToLowerInvariant())).Bool(
                            spell.ChampionName + spell.Slot,
                            spell.ChampionName + " (" + spell.Slot + ")",
                            false);
                    }
                }
                Menu.AddSubMenu(evadeMenu);
                Game.OnUpdate += OnUpdateDashes;
                Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            }

            private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
            {
                var caster = sender as Obj_AI_Hero;
                if (caster == null || !caster.IsValid || caster.Team == Player.Team || !(args.Target != null && args.Target.IsMe))
                {
                    return;
                }
                var spellData =
                   Spells.FirstOrDefault(
                       i =>
                       caster.ChampionName.ToLowerInvariant() == i.ChampionName.ToLowerInvariant()
                       && (i.UseSpellSlot ? args.Slot == i.Slot :
                       i.SpellNames.Any(x => x.ToLowerInvariant() == args.SData.Name.ToLowerInvariant()))
                       && Menu.SubMenu("EvadeTargetNone").SubMenu(i.ChampionName.ToLowerInvariant())
                       .Item(i.ChampionName + i.Slot).GetValue<bool>());
                if (spellData == null)
                {
                    return;
                }
                if (spellData.IsDash)
                {
                    DetectedDashes.Add(new DashTarget { Hero = caster, DistanceDash = spellData.DistanceDash, TickCount = Utils.GameTimeTickCount });
                }
                else
                {
                    if (Player.IsDead)
                    {
                        return;
                    }
                    if (Player.HasBuffOfType(BuffType.SpellShield) || Player.HasBuffOfType(BuffType.SpellImmunity))
                    {
                        return;
                    }
                    if (!Menu.SubMenu("EvadeTargetNone").Item("W").GetValue<bool>() || !W.IsReady())
                    {
                        return;
                    }
                    var tar = GetTarget(W.Range);
                    if (tar.IsValidTarget(W.Range))
                        Player.Spellbook.CastSpell(SpellSlot.W, tar.Position);
                    else
                    {
                        var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(W.Range));
                        if (hero != null)
                            Player.Spellbook.CastSpell(SpellSlot.W, hero.Position);
                        else
                            Player.Spellbook.CastSpell(SpellSlot.W, Player.ServerPosition.Extend(caster.Position, 100));
                    }
                }
            }

            private static void OnUpdateDashes(EventArgs args)
            {
                DetectedDashes.RemoveAll(
                    x =>
                    x.Hero == null || !x.Hero.IsValid
                    || (!x.Hero.IsDashing() && Utils.GameTimeTickCount > x.TickCount + 500));

                if (Player.IsDead)
                {
                    return;
                }
                if (Player.HasBuffOfType(BuffType.SpellShield) || Player.HasBuffOfType(BuffType.SpellImmunity))
                {
                    return;
                }
                if (!Menu.SubMenu("EvadeTargetNone").Item("W").GetValue<bool>() || !W.IsReady())
                {
                    return;
                }
                foreach (var target in
                     DetectedDashes.OrderBy(i => i.Hero.Position.Distance(Player.Position)))
                {
                    var dashdata = target.Hero.GetDashInfo();
                    if (dashdata != null && target.Hero.Position.To2D().Distance(Player.Position.To2D())
                        < target.DistanceDash + Game.Ping * dashdata.Speed / 1000)
                    {
                        var tar = GetTarget(W.Range);
                        if (tar.IsValidTarget(W.Range))
                            Player.Spellbook.CastSpell(SpellSlot.W, tar.Position);
                        else
                        {
                            var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(W.Range));
                            if (hero != null)
                                Player.Spellbook.CastSpell(SpellSlot.W, hero.Position);
                            else
                                Player.Spellbook.CastSpell(SpellSlot.W, Player.ServerPosition.Extend(target.Hero.Position, 100));
                        }
                    }
                }
            }

            #region SpellData
            private static void LoadSpellData()
            {
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Akali",
                        UseSpellSlot = true,
                        Slot = SpellSlot.R,
                        IsDash = true
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Alistar",
                        UseSpellSlot = true,
                        Slot = SpellSlot.W
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Alistar",
                        UseSpellSlot = true,
                        Slot = SpellSlot.W
                    });
                //blitz
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Blitzcrank",
                        Slot = SpellSlot.E,
                        SpellNames = new[] { "PowerFistAttack" }
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Brand",
                        UseSpellSlot = true,
                        Slot = SpellSlot.E
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Chogath",
                        UseSpellSlot = true,
                        Slot = SpellSlot.R
                    });
                //darius W confirmed
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Darius",
                        Slot = SpellSlot.W,
                        SpellNames = new[] { "DariusNoxianTacticsONHAttack" }
                    });

                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Darius",
                        UseSpellSlot = true,
                        Slot = SpellSlot.R
                    });
                //ekkoE confirmed
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Ekko",
                        Slot = SpellSlot.E,
                        SpellNames = new[] { "EkkoEAttack" }
                    });
                //eliseQ confirm
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Elise",
                        Slot = SpellSlot.Q,
                        SpellNames = new[] { "EliseSpiderQCast" }
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Evelynn",
                        UseSpellSlot = true,
                        Slot = SpellSlot.E,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Fiddlesticks",
                        UseSpellSlot = true,
                        Slot = SpellSlot.Q,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Fizz",
                        UseSpellSlot = true,
                        Slot = SpellSlot.Q,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Garen",
                        Slot = SpellSlot.Q,
                        SpellNames = new[] { "GarenQAttack" }
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Garen",
                        UseSpellSlot = true,
                        Slot = SpellSlot.R,
                    });
                // hercarim E confirmed
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Hecarim",
                        Slot = SpellSlot.E,
                        SpellNames = new[] { "HecarimRampAttack" }
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Irelia",
                        UseSpellSlot = true,
                        Slot = SpellSlot.Q,
                        IsDash = true
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Irelia",
                        UseSpellSlot = true,
                        Slot = SpellSlot.E,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Jarvan",
                        UseSpellSlot = true,
                        Slot = SpellSlot.R,
                    });
                ////jax W later
                //Spells.Add(
                //    new SpellData
                //    {
                //        ChampionName = "Jax",
                //        Slot = SpellSlot.W,
                //        SpellNames = new[] { "JaxEmpowerAttack", "JaxEmpowerTwo" }
                //    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Jax",
                        UseSpellSlot = true,
                        Slot = SpellSlot.Q,
                        IsDash = true
                    });
                //jax R confirmed
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Jax",
                        Slot = SpellSlot.R,
                        SpellNames = new[] { "JaxRelentlessAttack" }
                    });
                //jayce Q confirm
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Jayce",
                        Slot = SpellSlot.Q,
                        SpellNames = new[] { "JayceToTheSkies" },
                        IsDash = true,
                        DistanceDash = 400
                    });
                //jayce E confirm
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Jayce",
                        Slot = SpellSlot.E,
                        SpellNames = new[] { "JayceThunderingBlow" }
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Khazix",
                        UseSpellSlot = true,
                        Slot = SpellSlot.Q,
                    });
                //leesin Q2 later
                //Spells.Add(
                //    new SpellData
                //    {
                //        ChampionName = "Leesin",
                //        Slot = SpellSlot.Q,
                //        SpellNames = new[] { "BlindMonkQTwo" },
                //        IsDash = true
                //    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Leesin",
                        UseSpellSlot = true,
                        Slot = SpellSlot.R,
                    });
                //leona Q confirmed
                Spells.Add(
                   new SpellData
                   {
                       ChampionName = "Leona",
                       Slot = SpellSlot.Q,
                       SpellNames = new[] { "LeonaShieldOfDaybreakAttack" }
                   });
                // lissandra R
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Lissandra",
                        UseSpellSlot = true,
                        Slot = SpellSlot.R,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Lucian",
                        UseSpellSlot = true,
                        Slot = SpellSlot.Q,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Malzahar",
                        UseSpellSlot = true,
                        Slot = SpellSlot.E,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Malzahar",
                        UseSpellSlot = true,
                        Slot = SpellSlot.R,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Maokai",
                        UseSpellSlot = true,
                        Slot = SpellSlot.W,
                        IsDash = true
                    });
                //mordekaiserQ confirmed
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Mordekaiser",
                        Slot = SpellSlot.Q,
                        SpellNames = new[] { "MordekaiserQAttack", "MordekaiserQAttack1", "MordekaiserQAttack2" }
                    });
                // mordekaiser R confirmed
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Mordekaiser",
                        UseSpellSlot = true,
                        Slot = SpellSlot.R,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Nasus",
                        Slot = SpellSlot.Q,
                        SpellNames = new[] { "NasusQAttack" }
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Nasus",
                        UseSpellSlot = true,
                        Slot = SpellSlot.W,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "MonkeyKing",
                        Slot = SpellSlot.Q,
                        SpellNames = new[] { "MonkeyKingQAttack" }
                    });
                //nidalee Q confirmed
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Nidalee",
                         Slot = SpellSlot.Q,
                         SpellNames = new[] { "NidaleeTakedownAttack", "Nidalee_CougarTakedownAttack" }
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Olaf",
                         UseSpellSlot = true,
                         Slot = SpellSlot.E,
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Pantheon",
                         UseSpellSlot = true,
                         Slot = SpellSlot.W,
                     });
                //poppy Q later
                //Spells.Add(
                //     new SpellData
                //     {
                //         ChampionName = "Poppy",
                //         Slot = SpellSlot.Q,
                //         SpellNames = new[] { "PoppyDevastatingBlow" }
                //     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Poppy",
                         UseSpellSlot = true,
                         Slot = SpellSlot.E,
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Poppy",
                         UseSpellSlot = true,
                         Slot = SpellSlot.R,
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Quinn",
                         UseSpellSlot = true,
                         Slot = SpellSlot.E,
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Rammus",
                         UseSpellSlot = true,
                         Slot = SpellSlot.E,
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "RekSai",
                         UseSpellSlot = true,
                         Slot = SpellSlot.E,
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Renekton",
                         Slot = SpellSlot.W,
                         SpellNames = new[] { "RenektonExecute", "RenektonSuperExecute" }
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Ryze",
                         UseSpellSlot = true,
                         Slot = SpellSlot.W,
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Singed",
                         UseSpellSlot = true,
                         Slot = SpellSlot.E,
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Skarner",
                         UseSpellSlot = true,
                         Slot = SpellSlot.R,
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "TahmKench",
                         UseSpellSlot = true,
                         Slot = SpellSlot.W,
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Talon",
                         UseSpellSlot = true,
                         Slot = SpellSlot.E,
                     });
                //talonQ confirmed
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Talon",
                         Slot = SpellSlot.Q,
                         SpellNames = new[] { "TalonNoxianDiplomacyAttack" }
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Trundle",
                         UseSpellSlot = true,
                         Slot = SpellSlot.R,
                     });
                //udyr E : todo : check for stun buff
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Udyr",
                         Slot = SpellSlot.E,
                         SpellNames = new[] { "UdyrBearAttack", "UdyrBearAttackUlt" }
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Vi",
                         UseSpellSlot = true,
                         Slot = SpellSlot.R,
                         IsDash = true,
                     });
                //viktor Q confirmed
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Viktor",
                         Slot = SpellSlot.Q,
                         SpellNames = new[] { "ViktorQBuff" }
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Vladimir",
                         UseSpellSlot = true,
                         Slot = SpellSlot.Q,
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Volibear",
                         UseSpellSlot = true,
                         Slot = SpellSlot.W,
                     });
                //volibear Q confirmed
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Volibear",
                         Slot = SpellSlot.Q,
                         SpellNames = new[] { "VolibearQAttack" }
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Warwick",
                         UseSpellSlot = true,
                         Slot = SpellSlot.Q,
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Warwick",
                         UseSpellSlot = true,
                         Slot = SpellSlot.R,
                     });
                //xinzhaoQ3 confirmed
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "XinZhao",
                         Slot = SpellSlot.Q,
                         SpellNames = new[] { "XenZhaoThrust3" }
                     });
                Spells.Add(
                     new SpellData
                     {
                         ChampionName = "Yorick",
                         UseSpellSlot = true,
                         Slot = SpellSlot.E,
                     });
                //yorick Q
                //Spells.Add(
                //     new SpellData
                //     {
                //         ChampionName = "Yorick",
                //         Slot = SpellSlot.Q,
                //         SpellNames = new[] {"" }
                //     });
                Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Zilean",
                     UseSpellSlot = true,
                     Slot = SpellSlot.E,
                 });
            }
            #endregion SpellData

            private class SpellData
            {
                #region Fields

                public string ChampionName;

                public bool UseSpellSlot = false;

                public SpellSlot Slot;

                public string[] SpellNames = { };

                public bool IsDash = false;

                public float DistanceDash = 200;

                #endregion

                #region Public Properties

                public string MissileName
                {
                    get
                    {
                        return this.SpellNames.First();
                    }
                }

                #endregion
            }
            private class DashTarget
            {
                #region Fields

                public Obj_AI_Hero Hero;

                public float DistanceDash = 200;

                public int TickCount;

                #endregion
            }
        }
        #endregion Targeted NoMissile

        #region OtherSkill
        internal class OtherSkill
        {
            private static readonly List<SpellData> Spells = new List<SpellData>();

            // riven variables
            private static int RivenDashTick;
            private static int RivenQ3Tick;
            private static Vector2 RivenDashEnd = new Vector2();
            private static float RivenQ3Rad = 150;

            // fizz variables
            private static Vector2 FizzFishEndPos = new Vector2();
            private static GameObject FizzFishChum = null;
            private static int FizzFishChumStartTick;
            internal static void Init()
            {
                LoadSpellData();
                Spells.RemoveAll(i => !HeroManager.Enemies.Any(
                                a =>
                                string.Equals(
                                    a.ChampionName,
                                    i.ChampionName,
                                    StringComparison.InvariantCultureIgnoreCase)));
                var evadeMenu = new Menu("Block Other skils", "EvadeOthers");
                {
                    evadeMenu.Bool("W", "Use W");
                    foreach (var hero in
                        HeroManager.Enemies.Where(
                            i =>
                            Spells.Any(
                                a =>
                                string.Equals(
                                    a.ChampionName,
                                    i.ChampionName,
                                    StringComparison.InvariantCultureIgnoreCase))))
                    {
                        evadeMenu.AddSubMenu(new Menu("-> " + hero.ChampionName, hero.ChampionName.ToLowerInvariant()));
                    }
                    foreach (var spell in
                        Spells.Where(
                            i =>
                            HeroManager.Enemies.Any(
                                a =>
                                string.Equals(
                                    a.ChampionName,
                                    i.ChampionName,
                                    StringComparison.InvariantCultureIgnoreCase))))
                    {
                        ((Menu)evadeMenu.SubMenu(spell.ChampionName.ToLowerInvariant())).Bool(
                            spell.ChampionName + spell.Slot,
                            spell.ChampionName + " (" + (spell.Slot == SpellSlot.Unknown ? "Passive" : spell.Slot.ToString()) + ")",
                            false);
                    }
                }
                Menu.AddSubMenu(evadeMenu);
                Game.OnUpdate += Game_OnUpdate;
                GameObject.OnCreate += GameObject_OnCreate;
                GameObject.OnDelete += GameObject_OnDelete;
                Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
                Obj_AI_Hero.OnPlayAnimation += Obj_AI_Hero_OnPlayAnimation;
                CustomEvents.Unit.OnDash += Unit_OnDash;
            }

            private static void GameObject_OnDelete(GameObject sender, EventArgs args)
            {
                var missile = sender as MissileClient;
                if (missile == null || !missile.IsValid)
                    return;
                var caster = missile.SpellCaster as Obj_AI_Hero;
                if (!(caster is Obj_AI_Hero) || caster.Team == Player.Team)
                    return;
                if (missile.SData.Name == "FizzMarinerDoomMissile")
                {
                    FizzFishEndPos = missile.Position.To2D();
                }
            }

            private static void GameObject_OnCreate(GameObject sender, EventArgs args)
            {
                if (sender.Name == "Fizz_UltimateMissile_Orbit.troy" && FizzFishEndPos.IsValid()
                    && sender.Position.To2D().Distance(FizzFishEndPos) <= 300)
                {
                    FizzFishChum = sender;
                    if (Utils.GameTimeTickCount >= FizzFishChumStartTick + 5000)
                        FizzFishChumStartTick = Utils.GameTimeTickCount;
                }
            }

            private static void Unit_OnDash(Obj_AI_Base sender, Dash.DashItem args)
            {
                var caster = sender as Obj_AI_Hero;
                if (caster == null || !caster.IsValid || caster.Team == Player.Team)
                {
                    return;
                }
                // riven dash
                if (caster.ChampionName == "Riven"
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Riven").ToLowerInvariant())
                    .Item("Riven" + SpellSlot.Q).GetValue<bool>())
                {
                    RivenDashTick = Utils.GameTimeTickCount;
                    RivenDashEnd = args.EndPos;
                }
            }

            private static void Obj_AI_Hero_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
            {
                var caster = sender as Obj_AI_Hero;
                if (caster == null || !caster.IsValid || caster.Team == Player.Team)
                {
                    return;
                }
                // riven Q3
                if (caster.ChampionName == "Riven"
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Riven").ToLowerInvariant())
                    .Item("Riven" + SpellSlot.Q).GetValue<bool>()
                    && args.Animation.ToLower() == "spell1c")
                {
                    RivenQ3Tick = Utils.GameTimeTickCount;
                    if (caster.HasBuff("RivenFengShuiEngine"))
                        RivenQ3Rad = 150;
                    else
                        RivenQ3Rad = 225;
                }
                // others
                var spellDatas =
                   Spells.Where(
                       i =>
                       caster.ChampionName.ToLowerInvariant() == i.ChampionName.ToLowerInvariant()
                       && Menu.SubMenu("EvadeOthers").SubMenu(i.ChampionName.ToLowerInvariant())
                       .Item(i.ChampionName + i.Slot).GetValue<bool>());
                if (!spellDatas.Any())
                {
                    return;
                }
                foreach (var spellData in spellDatas)
                {
                    //reksaj W
                    if (!Player.HasBuff("reksaiknockupimmune") && spellData.ChampionName == "Reksai" 
                        && spellData.Slot == SpellSlot.W && args.Animation == "Spell2_knockup" )// chua test
                    {
                        if (Player.Position.To2D().Distance(caster.Position.To2D())
                            <= Player.BoundingRadius + caster.BoundingRadius + caster.AttackRange)
                            SolveInstantBlock();
                        return;
                    }
                }
            }

            private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
            {
                var caster = sender as Obj_AI_Hero;
                if (caster == null || !caster.IsValid || caster.Team == Player.Team)
                {
                    return;
                }
                var spellDatas =
                   Spells.Where(
                       i =>
                       caster.ChampionName.ToLowerInvariant() == i.ChampionName.ToLowerInvariant()
                       && Menu.SubMenu("EvadeOthers").SubMenu(i.ChampionName.ToLowerInvariant())
                       .Item(i.ChampionName + i.Slot).GetValue<bool>());
                if (!spellDatas.Any())
                {
                    return;
                }
                foreach (var spellData in spellDatas)
                {
                    // auto attack
                    if (args.SData.IsAutoAttack() && args.Target != null && args.Target.IsMe)
                    {
                        if (spellData.ChampionName == "Jax" && spellData.Slot == SpellSlot.W && caster.HasBuff("JaxEmpowerTwo"))
                        {
                            SolveInstantBlock();
                            return;
                        }
                        if (spellData.ChampionName == "Yorick" && spellData.Slot == SpellSlot.Q && caster.HasBuff("YorickSpectral"))
                        {
                            SolveInstantBlock();
                            return;
                        }
                        if (spellData.ChampionName == "Poppy" && spellData.Slot == SpellSlot.Q && caster.HasBuff("PoppyDevastatingBlow"))
                        {
                            SolveInstantBlock();
                            return;
                        }
                        if (spellData.ChampionName == "Rengar" && spellData.Slot == SpellSlot.Q
                            && (caster.HasBuff("rengarqbase") || caster.HasBuff("rengarqemp")))
                        {
                            SolveInstantBlock();
                            return;
                        }
                        if (spellData.ChampionName == "Nautilus" && spellData.Slot == SpellSlot.Unknown
                            && (!Player.HasBuff("nautiluspassivecheck")))
                        {
                            SolveInstantBlock();
                            return;
                        }
                        if (spellData.ChampionName == "Udyr" && spellData.Slot == SpellSlot.E && caster.HasBuff("UdyrBearStance")
                            && (Player.HasBuff("udyrbearstuncheck")))
                        {
                            SolveInstantBlock();
                            return;
                        }
                        return;
                    }
                    // aoe
                    if (spellData.ChampionName == "Riven" && spellData.Slot == SpellSlot.W && args.Slot == SpellSlot.W)// chua test
                    {
                        if (Player.Position.To2D().Distance(caster.Position.To2D())
                            <= Player.BoundingRadius + caster.BoundingRadius + caster.AttackRange)
                            SolveInstantBlock();
                        return;
                    }
                    if (spellData.ChampionName == "Diana" && spellData.Slot == SpellSlot.E && args.Slot == SpellSlot.E)// chua test
                    {
                        if (Player.Position.To2D().Distance(caster.Position.To2D())
                            <= Player.BoundingRadius + 450)
                            SolveInstantBlock();
                        return;
                    }
                    if (spellData.ChampionName == "Maokai" && spellData.Slot == SpellSlot.R && args.SData.Name == "maokaidrain3toggle")
                    {
                        if (Player.Position.To2D().Distance(caster.Position.To2D())
                            <= Player.BoundingRadius + 575)
                            SolveInstantBlock();
                        return;
                    }
                    if (spellData.ChampionName == "Kalista" && spellData.Slot == SpellSlot.E && args.Slot == SpellSlot.E)
                    {
                        if (Player.Position.To2D().Distance(caster.Position.To2D())
                            <= 950
                            && Player.HasBuff("kalistaexpungemarker"))
                            SolveInstantBlock();
                        return;
                    }
                    if (spellData.ChampionName == "Kennen" && spellData.Slot == SpellSlot.W && args.Slot == SpellSlot.W)// chua test
                    {
                        if (Player.Position.To2D().Distance(caster.Position.To2D())
                            <= 800
                            && Player.HasBuff("kennenmarkofstorm") && Player.GetBuffCount("kennenmarkofstorm") == 2)
                            SolveInstantBlock();
                        return;
                    }
                    if (spellData.ChampionName == "Azir" && spellData.Slot == SpellSlot.R && args.Slot == SpellSlot.R)// chua test
                    {
                        Vector2 start = caster.Position.To2D().Extend(args.End.To2D(), - 300);
                        Vector2 end = start.Extend(caster.Position.To2D(), 750);
                        Render.Circle.DrawCircle(start.To3D(), 50, Color.Red);
                        Render.Circle.DrawCircle(end.To3D(), 50, Color.Red);
                        float width = caster.Level >= 16 ? 125 * 6/2 :
                                    caster.Level >= 11 ? 125 * 5/2 :
                                    125 * 4/2;
                        FioraProject.Evade.Geometry.Rectangle Rect = new FioraProject.Evade.Geometry.Rectangle(start, end, width);
                        var Poly = Rect.ToPolygon();
                        if (!Poly.IsOutside(Player.Position.To2D()))
                        {
                            SolveInstantBlock();
                        }
                        return;
                    }
                }
            }

            private static void Game_OnUpdate(EventArgs args)
            {
                if (Player.HasBuff("vladimirhemoplaguedebuff") && HeroManager.Enemies.Any(x => x.ChampionName == "Vladimir")
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Vladimir").ToLowerInvariant())
                    .Item("Vladimir" + SpellSlot.R).GetValue<bool>())
                {
                    var buff = Player.GetBuff("vladimirhemoplaguedebuff");
                    if (buff == null)
                        return;
                    SolveBuffBlock(buff);
                }

                if (Player.HasBuff("zedrdeathmark") && HeroManager.Enemies.Any(x => x.ChampionName == "Zed")
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Zed").ToLowerInvariant())
                    .Item("Zed" + SpellSlot.R).GetValue<bool>())
                {
                    var buff = Player.GetBuff("zedrdeathmark");
                    if (buff == null)
                        return;
                    SolveBuffBlock(buff);
                }

                if (Player.HasBuff("tristanaechargesound") && HeroManager.Enemies.Any(x => x.ChampionName == "Tristana")
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Tristana").ToLowerInvariant())
                    .Item("Tristana" + SpellSlot.E).GetValue<bool>())
                {
                    var buff = Player.GetBuff("tristanaechargesound ");
                    if (buff == null)
                        return;
                    SolveBuffBlock(buff);
                }

                if (Player.HasBuff("SoulShackles") && HeroManager.Enemies.Any(x => x.ChampionName == "Morgana")
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Morgana").ToLowerInvariant())
                    .Item("Morgana" + SpellSlot.R).GetValue<bool>())
                {
                    var buff = Player.GetBuff("SoulShackles");
                    if (buff == null)
                        return;
                    SolveBuffBlock(buff);
                }

                if (Player.HasBuff("NocturneUnspeakableHorror") && HeroManager.Enemies.Any(x => x.ChampionName == "Nocturne")
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Nocturne").ToLowerInvariant())
                    .Item("Nocturne" + SpellSlot.E).GetValue<bool>())
                {
                    var buff = Player.GetBuff("NocturneUnspeakableHorror");
                    if (buff == null)
                        return;
                    SolveBuffBlock(buff);
                }

                if (Player.HasBuff("karthusfallenonetarget") && HeroManager.Enemies.Any(x => x.ChampionName == "Karthus")
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Karthus").ToLowerInvariant())
                    .Item("Karthus" + SpellSlot.R).GetValue<bool>())
                {
                    var buff = Player.GetBuff("karthusfallenonetarget");
                    if (buff == null)
                        return;
                    SolveBuffBlock(buff);
                }

                if (Player.HasBuff("KarmaSpiritBind") && HeroManager.Enemies.Any(x => x.ChampionName == "Karma")
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Karma").ToLowerInvariant())
                    .Item("Karma" + SpellSlot.R).GetValue<bool>())
                {
                    var buff = Player.GetBuff("KarmaSpiritBind");
                    if (buff == null)
                        return;
                    SolveBuffBlock(buff);
                }

                if ((Player.HasBuff("LeblancSoulShackle") || (Player.HasBuff("LeblancShoulShackleM")))
                    && HeroManager.Enemies.Any(x => x.ChampionName == "Karma")
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Karma").ToLowerInvariant())
                    .Item("Karma" + SpellSlot.R).GetValue<bool>())
                {
                    var buff = Player.GetBuff("LeblancSoulShackle");
                    if (buff != null)
                    {
                        SolveBuffBlock(buff);
                        return;
                    }
                    var buff2 = Player.GetBuff("LeblancShoulShackleM");
                    if (buff2 != null)
                    {
                        SolveBuffBlock(buff2);
                        return;
                    }
                }

                // jax E
                var jax = HeroManager.Enemies.FirstOrDefault(x => x.ChampionName == "Jax" && x.IsValidTarget());

                if (jax != null && jax.HasBuff("JaxCounterStrike")
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Jax").ToLowerInvariant())
                    .Item("Jax" + SpellSlot.E).GetValue<bool>())
                {
                    var buff = jax.GetBuff("JaxCounterStrike");
                    if (buff != null)
                    {
                        if ((buff.EndTime - Game.Time) * 1000 <= 650 + Game.Ping && Player.Position.To2D().Distance(jax.Position.To2D())
                            <= Player.BoundingRadius + jax.BoundingRadius + jax.AttackRange + 100)
                        {
                            SolveInstantBlock();
                            return;
                        }
                    }
                }

                //maokai R
                var maokai = HeroManager.Enemies.FirstOrDefault(x => x.ChampionName == "Maokai" && x.IsValidTarget());
                if (maokai != null && maokai.HasBuff("MaokaiDrain3")
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Maokai").ToLowerInvariant())
                    .Item("Maokai" + SpellSlot.R).GetValue<bool>())
                {
                    var buff = maokai.GetBuff("MaokaiDrain3");
                    if (buff != null)
                    {
                        if (Player.Position.To2D().Distance(maokai.Position.To2D())
                            <= Player.BoundingRadius + 475)
                            SolveBuffBlock(buff);
                    }
                }

                // nautilus R
                if (Player.HasBuff("nautilusgrandlinetarget") && HeroManager.Enemies.Any(x => x.ChampionName == "Nautilus")
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Nautilus").ToLowerInvariant())
                    .Item("Nautilus" + SpellSlot.R).GetValue<bool>())
                {
                    var buff = Player.GetBuff("nautilusgrandlinetarget");
                    if (buff == null)
                        return;
                    var obj = ObjectManager.Get<GameObject>().Where(x => x.Name == "GrandLineSeeker").FirstOrDefault();
                    if (obj == null)
                        return;
                    if (obj.Position.To2D().Distance(Player.Position.To2D()) <= 300 + 700 * Game.Ping / 1000)
                    {
                        SolveInstantBlock();
                        return;
                    }
                }

                //rammus Q
                var ramus = HeroManager.Enemies.FirstOrDefault(x => x.ChampionName == "Rammus" && x.IsValidTarget());
                if (ramus != null
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Rammus").ToLowerInvariant())
                    .Item("Rammus" + SpellSlot.Q).GetValue<bool>())
                {
                    var buff = ramus.GetBuff("PowerBall");
                    if (buff != null)
                    {
                        var waypoints = ramus.GetWaypoints();
                        if (waypoints.Count == 1)
                        {
                            if (Player.Position.To2D().Distance(ramus.Position.To2D())
                                <= Player.BoundingRadius + ramus.AttackRange + ramus.BoundingRadius)
                            {
                                SolveInstantBlock();
                                return;
                            }
                        }
                        else
                        {
                            if (Player.Position.To2D().Distance(ramus.Position.To2D())
                                <= Player.BoundingRadius + ramus.AttackRange + ramus.BoundingRadius
                                + ramus.MoveSpeed * (0.5f + Game.Ping / 1000))
                            {
                                if (waypoints.Any(x => x.Distance(Player.Position.To2D())
                                    <= Player.BoundingRadius + ramus.AttackRange + ramus.BoundingRadius + 70))
                                {
                                    SolveInstantBlock();
                                    return;
                                }
                                for (int i = 0; i < waypoints.Count() - 2; i++)
                                {
                                    if (Player.Position.To2D().Distance(waypoints[i], waypoints[i + 1], true)
                                        <= Player.BoundingRadius + ramus.BoundingRadius + ramus.AttackRange + 70)
                                    {
                                        SolveInstantBlock();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }

                //fizzR
                if (HeroManager.Enemies.Any(x => x.ChampionName == "Fizz")
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Fizz").ToLowerInvariant())
                    .Item("Fizz" + SpellSlot.R).GetValue<bool>())
                {
                    if (FizzFishChum != null && FizzFishChum.IsValid
                        && Utils.GameTimeTickCount - FizzFishChumStartTick >= 1500 - 250 - Game.Ping
                        && Player.Position.To2D().Distance(FizzFishChum.Position.To2D()) <= 250 + Player.BoundingRadius)
                    {
                        SolveInstantBlock();
                        return;
                    }
                }

                //nocturne R
                var nocturne = HeroManager.Enemies.FirstOrDefault(x => x.ChampionName == "Nocturne" && x.IsValidTarget());
                if (nocturne != null
                    && Menu.SubMenu("EvadeOthers").SubMenu(("Nocturne").ToLowerInvariant())
                    .Item("Nocturne" + SpellSlot.R).GetValue<bool>())
                {
                    var buff = Player.GetBuff("nocturneparanoiadash");
                    if (buff != null && Player.Position.To2D().Distance(nocturne.Position.To2D()) <= 300 + 1200 * Game.Ping / 1000)
                    {
                        SolveInstantBlock();
                        return;
                    }
                }


                // rivenQ3
                var riven = HeroManager.Enemies.FirstOrDefault(x => x.ChampionName == "Riven" && x.IsValidTarget());
                if (riven != null && Menu.SubMenu("EvadeOthers").SubMenu(("Riven").ToLowerInvariant())
                    .Item("Riven" + SpellSlot.Q).GetValue<bool>() && RivenDashEnd.IsValid())
                {
                    if (Utils.GameTimeTickCount - RivenDashTick <= 100 && Utils.GameTimeTickCount - RivenQ3Tick <= 100
                        && Math.Abs(RivenDashTick - RivenQ3Tick) <= 100 && Player.Position.To2D().Distance(RivenDashEnd) <= RivenQ3Rad)
                    {
                        SolveInstantBlock();
                        return;
                    }
                }

            }
            private static void SolveBuffBlock(BuffInstance buff)
            {
                if (Player.IsDead || Player.HasBuffOfType(BuffType.SpellShield) || Player.HasBuffOfType(BuffType.SpellImmunity)
                    || !Menu.SubMenu("EvadeOthers").Item("W").GetValue<bool>() || !W.IsReady())
                    return;
                if (buff == null)
                    return;
                if ((buff.EndTime - Game.Time) * 1000 <= 250 + Game.Ping)
                {
                    var tar = GetTarget(W.Range);
                    if (tar.IsValidTarget(W.Range))
                        Player.Spellbook.CastSpell(SpellSlot.W, tar.Position);
                    else
                    {
                        var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(W.Range));
                        if (hero != null)
                            Player.Spellbook.CastSpell(SpellSlot.W, hero.Position);
                        else
                            Player.Spellbook.CastSpell(SpellSlot.W, Player.ServerPosition.Extend(Game.CursorPos, 100));
                    }
                }
            }
            private static void SolveInstantBlock()
            {
                if (Player.IsDead || Player.HasBuffOfType(BuffType.SpellShield) || Player.HasBuffOfType(BuffType.SpellImmunity)
                    || !Menu.SubMenu("EvadeOthers").Item("W").GetValue<bool>() || !W.IsReady())
                    return;
                var tar = GetTarget(W.Range);
                if (tar.IsValidTarget(W.Range))
                    Player.Spellbook.CastSpell(SpellSlot.W, tar.Position);
                else
                {
                    var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(W.Range));
                    if (hero != null)
                        Player.Spellbook.CastSpell(SpellSlot.W, hero.Position);
                    else
                        Player.Spellbook.CastSpell(SpellSlot.W, Player.ServerPosition.Extend(Game.CursorPos, 100));
                }
            }
            private static void LoadSpellData()
            {
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Azir",
                        Slot = SpellSlot.R,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Fizz",
                        Slot = SpellSlot.R,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Jax",
                        Slot = SpellSlot.W,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Jax",
                        Slot = SpellSlot.E,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Riven",
                        Slot = SpellSlot.Q,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Riven",
                        Slot = SpellSlot.W,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Diana",
                        Slot = SpellSlot.E,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Kalista",
                        Slot = SpellSlot.E,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Karma",
                        Slot = SpellSlot.W,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Karthus",
                        Slot = SpellSlot.R,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Kennen",
                        Slot = SpellSlot.W,
                    });
                //Spells.Add(
                //    new SpellData
                //    {
                //        ChampionName = "Leesin",
                //        Slot = SpellSlot.Q,
                //    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Leblanc",
                        Slot = SpellSlot.E,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Maokai",
                        Slot = SpellSlot.R,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Morgana",
                        Slot = SpellSlot.R,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Nautilus",
                        Slot = SpellSlot.Unknown,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Nautilus",
                        Slot = SpellSlot.R,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Nocturne",
                        Slot = SpellSlot.E,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Nocturne",
                        Slot = SpellSlot.R,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Nocturne",
                        Slot = SpellSlot.R,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Rammus",
                        Slot = SpellSlot.Q,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Rengar",
                        Slot = SpellSlot.Q,
                    });
                Spells.Add(
                new SpellData
                {
                    ChampionName = "Reksai",
                    Slot = SpellSlot.W,
                });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Vladimir",
                        Slot = SpellSlot.R,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Zed",
                        Slot = SpellSlot.R,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Tristana",
                        Slot = SpellSlot.E,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Udyr",
                        Slot = SpellSlot.E,
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Yorick",
                        Slot = SpellSlot.Q,
                    });
            }
            private class SpellData
            {
                #region Fields

                public string ChampionName;

                public SpellSlot Slot;

                #endregion

                #region Public Properties


                #endregion
            }
        }
        #endregion OtherSkill
>>>>>>> origin/master
    }
    // jax E, riven W,dianaE,maokaiR, ---------------> solving
    // :D Vladimir R Zed R tris E, morganaR,nocturn E,Karthus R, FizzR,karmaW leblancE------> solving
    //jax W, yorick Q, poppy Q rengar Q   nautilus, ---------> solving
    //,  riven Q3,  leesin Q2,   nocturne R, ramus Q, -----> solving
    // kalista E,kennen W, -----> solving

    // LeblancSoulShackle , LeblancShoulShackleM nautiluspassivecheck kennenmarkofstorm  kalistaexpungemarker--- done
    // nautilusgrandlinetarget NocturneUnspeakableHorror GrandLineSeeker (nau R) nocturneparanoiadash---- done
    // , ,  ,  , RivenFengShuiEngine , ====> done
}
