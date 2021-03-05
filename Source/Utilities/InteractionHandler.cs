﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities.Type;
using WindowsInput;
using WindowsInput.Native;

namespace Utilities
{
    public class InteractionHandler
    {
        private InputSimulator m_inputSimulator;

        public InteractionHandler()
        {
            m_inputSimulator = new InputSimulator();
        }

        public async Task GetToBurnerPage()
        {
            MousePoint toolButtonCoords = new MousePoint(437, 525);
            m_inputSimulator.Mouse.MoveMouseTo(toolButtonCoords.X, toolButtonCoords.Y);
            m_inputSimulator.Mouse.Sleep(100);
            m_inputSimulator.Mouse.LeftButtonClick();
            m_inputSimulator.Mouse.Sleep(100);
            await Task.Delay(TimeSpan.FromSeconds(5));

            MousePoint burnerTab = new MousePoint(950, 190);
            m_inputSimulator.Mouse.MoveMouseTo(burnerTab.X, burnerTab.Y);
            m_inputSimulator.Mouse.Sleep(100);
            m_inputSimulator.Mouse.LeftButtonClick();
            await Task.Delay(TimeSpan.FromSeconds(1));
        }


        public async Task LoginToAccount(long number)
        {
            await GetToBurnerPage();
            Dictionary<long, MousePoint> openBotButtonMap = new Dictionary<long, MousePoint>();
            openBotButtonMap.Add(1, new MousePoint(930, 390));
            openBotButtonMap.Add(2, new MousePoint(1160, 390));
            openBotButtonMap.Add(3, new MousePoint(1390, 390));
            openBotButtonMap.Add(4, new MousePoint(700, 540));
            openBotButtonMap.Add(5, new MousePoint(930, 540));
            openBotButtonMap.Add(6, new MousePoint(1160, 540));
            openBotButtonMap.Add(7, new MousePoint(1390, 540));
            openBotButtonMap.Add(8, new MousePoint(700, 680));
            openBotButtonMap.Add(9, new MousePoint(930, 680));
            openBotButtonMap.Add(10, new MousePoint(1160, 680));
            openBotButtonMap.Add(11, new MousePoint(1390, 680));
            openBotButtonMap.Add(12, new MousePoint(700, 825));
            openBotButtonMap.Add(13, new MousePoint(930, 825));
            openBotButtonMap.Add(14, new MousePoint(1160, 825));
            openBotButtonMap.Add(15, new MousePoint(1390, 825));

            if (!openBotButtonMap.ContainsKey(number))
            {
                throw new Exception("Invalid bot number selection");
            }

            var buttonCoord = openBotButtonMap[number];
            m_inputSimulator.Mouse.MoveMouseTo(buttonCoord.X, buttonCoord.Y);
            m_inputSimulator.Mouse.Sleep(100);
            m_inputSimulator.Mouse.LeftButtonClick();
            await Task.Delay(TimeSpan.FromSeconds(30));
        }

        public async Task GoToDM()
        {
            MousePoint dmButton = new MousePoint(50, 75);
            m_inputSimulator.Mouse.MoveMouseTo(dmButton.X, dmButton.Y);
            m_inputSimulator.Mouse.Sleep(100);
            m_inputSimulator.Mouse.LeftButtonClick();
            await Task.Delay(TimeSpan.FromSeconds(5));

            MousePoint latestMessageButton = new MousePoint(210, 280);
            m_inputSimulator.Mouse.MoveMouseTo(latestMessageButton.X, latestMessageButton.Y);
            m_inputSimulator.Mouse.Sleep(100);
            m_inputSimulator.Mouse.LeftButtonClick();
            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        public async Task CopyLatestKey()
        {
            MousePoint copy = new MousePoint(460, 680);
            m_inputSimulator.Mouse.MoveMouseTo(copy.X, copy.Y);
            m_inputSimulator.Mouse.Sleep(100);
            m_inputSimulator.Mouse.LeftButtonClick();
            m_inputSimulator.Mouse.LeftButtonDoubleClick();
            await Task.Delay(TimeSpan.FromSeconds(2));

            m_inputSimulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
            m_inputSimulator.Mouse.Sleep(100);
            await Task.Delay(TimeSpan.FromSeconds(1));

            //MousePoint messageZone = new MousePoint(430, 730);
            //m_inputSimulator.Mouse.MoveMouseTo(messageZone.X, messageZone.Y);
            //m_inputSimulator.Mouse.LeftButtonClick();
            //await Task.Delay(TimeSpan.FromSeconds(1));

        }

        public async Task PasteAndSendMessage()
        {
            MousePoint paste = new MousePoint(440, 720);
            m_inputSimulator.Mouse.MoveMouseTo(paste.X, paste.Y);
            m_inputSimulator.Mouse.Sleep(100);

            var key = Clipboard.GetText();
            await Task.Delay(TimeSpan.FromSeconds(1));
            m_inputSimulator.Keyboard.TextEntry(key);
            m_inputSimulator.Mouse.Sleep(100);
            m_inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            m_inputSimulator.Mouse.Sleep(100);
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        public async Task<string> GetNewKeyValue()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return Clipboard.GetText();
        }
    }
}