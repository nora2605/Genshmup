﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Genshmup.Game;
using Genshmup.HelperClasses;

namespace Genshmup
{
    public partial class MainForm : Form
    {
        private Graphics g;
        private BufferedGraphics buffer;

        private int phase = 0;
        public bool invincible = false;

        private HelperClasses.Screen[] screens = new HelperClasses.Screen[7];

        private readonly List<string> eventBuffer = new();

        public MainForm()
        {
            InitializeComponent();
            MinimumSize = Size;
            MaximumSize = Size;

            screens = new HelperClasses.Screen[]
            {
                new Menu(),
                new Stage1(),
                new Stage2(),
                new Stage3(),
                new Credits(),
                new MusicRoom(),
                new StageSelect()
            };

            g = CreateGraphics();
            buffer = BufferedGraphicsManager.Current.Allocate(g, ClientRectangle);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            buffer.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            gameTimer.Interval = 15;
            gameTimer.Enabled = true;

            screens[0].Init();
        }

        private void Render()
        {
            screens[phase].Render(buffer.Graphics);
        }

        private bool Logic()
        {
            LogicExit le = screens[phase].Logic(eventBuffer.ToArray());
            if (le == LogicExit.CloseApplication)
            {
                Close();
                return true;
            }
            else if (le == LogicExit.ScreenChange)
            {
                eventBuffer.Clear();
                int nextScreen = screens[phase].NextScreen;
                screens[phase].Dispose();
                screens = new HelperClasses.Screen[]
                {
                    new Menu(),
                    new Stage1(),
                    new Stage2(),
                    new Stage3(),
                    new Credits(),
                    new MusicRoom(),
                    new StageSelect()
                };
                phase = Math.Max(0, Math.Min(nextScreen, screens.Length));
                screens[phase].Init();
            }
            if (phase == 0 || phase == 5 || phase == 6) eventBuffer.Clear(); // BIOS Input for Menus
            return false;
        }

        private void GameTick(object sender, EventArgs e)
        {
            Thread.Yield();
            if (Logic()) return;
            Render();
            buffer.Render(g);
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            string? ev = Enum.GetName(e.KeyCode);
            if (ev == null || eventBuffer.Contains(ev) && phase == 0) return;
            eventBuffer.Add(ev);
        }
        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            string? ev = Enum.GetName(e.KeyCode);
            if (ev != null) eventBuffer.RemoveAll(i => i == ev);
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                g = CreateGraphics();
                buffer = BufferedGraphicsManager.Current.Allocate(g, ClientRectangle);
            }
            catch { }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (HelperClasses.Screen s in screens)
            {
                s.Dispose();
            }
        }

        private void MainForm_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", "https://github.com/noah2605/Genshmup/blob/master/README.md");
        }
    }
}
