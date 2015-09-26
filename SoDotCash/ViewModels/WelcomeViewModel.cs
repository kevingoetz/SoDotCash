﻿using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using SoDotCash.Services;

namespace SoDotCash.ViewModels
{
    /// <summary>
    /// This class contains the buisiness logic interface for the Welcome View
    /// </summary>
    public class WelcomeViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the WelcomeViewModel class.
        /// </summary>
        public WelcomeViewModel()
        {
        }

        /// <summary>
        /// Binding for the Download Transactions button
        /// </summary>
        private ICommand _getStartedCommand;
        public ICommand GetStartedCommand
        {
            get { return _getStartedCommand ?? (_getStartedCommand = new RelayCommand(GetStarted, () => true)); }
        }

        /// <summary>
        /// Progresses from the welcome screen on to the accounts screen and positions to add account
        /// </summary>
        public void GetStarted()
        {
            // Transition view to add account
            var locator = new ViewModelLocator();
            locator.Main.ActiveViewModel = locator.AddAccount;
        }
    }
}