// Copyright (c) 2013 Ivan Krivyakov
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using G2Cy.WpfHost.Interfaces;
using Microsoft.Extensions.Logging;
using UseLogService.ViewModels;

namespace UseLogService
{
    /// <summary>
    /// Interaction logic for MainUserControl.xaml
    /// </summary>
    public partial class MainUserControl : UserControl, IUnsavedData
    {
        private bool _textLogged = true;
        public MainUserControl(MainUserContentViewModel viewModel)
        {
            InitializeComponent();
            //Log = logger;
            DataContext = viewModel;
            //Level.SelectedItem = LogLevel.Debug;
            Message.TextChanged += Message_TextChanged;
        }

        void Message_TextChanged(object sender, TextChangedEventArgs e)
        {
            _textLogged = false;
            Logged.Visibility = Visibility.Hidden;
        }

        public ILogger Log { get; set; }

        public string[] GetNamesOfUnsavedItems()
        {
            if (!_textLogged) return new[] {"Log message text"};
            return null;
        }

        private void LogIt_Click(object sender, RoutedEventArgs e)
        {
            LogIt();
        }

        private void Message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) LogIt();
        }

        private void LogIt()
        {
            var message = "From UseLogService: " + Message.Text;
            switch ((LogLevel)Level.SelectedItem)
            {
                case LogLevel.Debug: Log.LogDebug(message); break;
                case LogLevel.Info: Log.LogInformation(message); break;
                case LogLevel.Warning: Log.LogWarning(message); break;
                case LogLevel.Error: Log.LogError(message); break;
            }
            _textLogged = true;
            Logged.Visibility = Visibility.Visible;
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            //Message.Focus();
        }

    }
}
