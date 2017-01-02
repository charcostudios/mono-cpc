using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace monocpc {


    public enum EPauseMenuOptions {
        Back = -1,
        Resume = 0,
        LoadSnapshot,
        Cheats,
        Reset,
        ToggleCRTShader,
        ThrottleSpeed,
        Quit
    }

    public class PauseMenu : ListMenuComponent {
        public PauseMenu(MainGame game, Rectangle menu_extents) : base(game, menu_extents) { }

        protected override void LoadContent() {
            List<string> pause_menu_options = new List<string>();
            pause_menu_options.Add("Unpause");      //Resume
            pause_menu_options.Add("Load Snapshot");	//LoadSnapshot
            pause_menu_options.Add("Cheats");      //Input
            pause_menu_options.Add("Reset CPC");		//Reset
            pause_menu_options.Add("Toggle CRT Shader");	//ToggleCRTShader
            pause_menu_options.Add("Throttle Speed");   //ThrottleSpeed
            pause_menu_options.Add("Quit");			//Quit

            SetupMenu("MONOCPC - Gavin Pugh 2011 - CharcoStudios 2016", pause_menu_options);
            SetupMenuToggle((int)EPauseMenuOptions.ThrottleSpeed, m_game.m_throttle_speed);
            SetupMenuToggle((int)EPauseMenuOptions.ToggleCRTShader, m_game.m_use_crt_shader);
            base.LoadContent();
        }

        protected override void MenuCallback(int chosen_index) {
            switch ((EPauseMenuOptions)chosen_index) {
                case EPauseMenuOptions.LoadSnapshot: {
                        m_game.m_pause_menu.Close();
                        m_game.m_snapshot_menu.ShowMenu();
                    }
                    break;

                case EPauseMenuOptions.Reset: {
                        m_game.ResetCPC();
                        m_game.Unpause();
                    }
                    break;

                case EPauseMenuOptions.Cheats: {
                        m_game.m_pause_menu.Close();
                        m_game.m_cheats_menu.ShowMenu();
                    }
                    break;

                case EPauseMenuOptions.Back:
                case EPauseMenuOptions.Resume: {
                        m_game.Unpause();
                    }
                    break;

                case EPauseMenuOptions.ToggleCRTShader: {
                        m_game.m_use_crt_shader = !m_game.m_use_crt_shader;
                    }
                    break;

                case EPauseMenuOptions.ThrottleSpeed: {
                        m_game.m_throttle_speed = !m_game.m_throttle_speed;
                    }
                    break;

                case EPauseMenuOptions.Quit: {
                        m_game.Exit();
                    }
                    break;
            }
        }
    }

    public class LoadSnapShotMenu : ListMenuComponent {
        public LoadSnapShotMenu(MainGame game, Rectangle menu_extents) : base(game, menu_extents) { }

        protected override void LoadContent() {
            SetupMenu("Choose a snapshot", Manifest.Games.Select(g => g.Title).ToList());
            base.LoadContent();
        }

        protected override void MenuCallback(int chosen_index) {
            if (chosen_index >= 0) {
                m_game.LoadSnapshotFile(chosen_index);
                m_game.Unpause();
            }
            else {
                m_game.m_snapshot_menu.Close();
                m_game.m_pause_menu.ShowMenu();
            }
        }
    }

    public class CheatsMenu : ListMenuComponent {
        public CheatsMenu(MainGame game, Rectangle menu_extents) : base(game, menu_extents) {
            //m_snapshot_input_menu.SetupMenu("Choose a snapshot", Enumerable.Concat( new string[] { "<Default>" }, m_snapshot_files).ToList(), SnapshotInputCallback);

        }

        protected override void MenuCallback(int chosen_index) {
            var cheats = m_game.m_current_game.Cheats;

            if (chosen_index == cheats.Count) {
                m_game.m_cheats_menu.Close();
                m_game.m_pause_menu.ShowMenu();
            }
            else if (chosen_index >= 0) {
                cheats[chosen_index].Toggle();
            }
            else
                m_game.Unpause();
        }

        protected override void OnBeforeShowMenu() {

            var cheats = m_game.m_current_game.Cheats;
            var choices = cheats.Select(c => c.Title).ToList();

            choices.Add("Back");
            SetupMenu("Cheats", choices);

            for (int n = 0; n < cheats.Count; n++) SetupMenuToggle(n, cheats[n].Active);
        }
    }


}
