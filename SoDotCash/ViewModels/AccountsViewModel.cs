﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using SoDotCash.Models;
using SoDotCash.Services;
using SoDotCash.ViewModels.Navigation;

namespace SoDotCash.ViewModels
{
    /// <summary>
    /// ViewModel handling data exposure and backing functionality for the AccountsView view element
    /// </summary>
    public class AccountsViewModel : ModernViewModelBase
    {
        private readonly IModernNavigationService _modernNavigationService;

        public AccountsViewModel(IModernNavigationService navService)
        {
            _modernNavigationService = navService;
        }

        /// <summary>
        /// Enumeration providing names to the tabs on the view
        /// </summary>
        public enum TabIndex
        {
            Overview = 0,
            Ledger,
            Import,
            Configuration
        };


        #region [ Public Bound Properties ]

        /// <summary>
        /// Provides the collection of accounts mapped by the account type of each
        /// </summary>
        private Dictionary<string, ObservableCollection<Account>> _accountsView;
        public Dictionary<string, ObservableCollection<Account>> AccountsView
        {
            get
            {
                // If there are no accounts, fill in collection on first attempt to retrieve
                if (_accountsView == null)
                    UpdateAccounts();
                return _accountsView;
            }
        } 

        /// <summary>
        /// Bound current account
        /// </summary>
        private Account _selectedAccount;
        public Account SelectedAccount
        {
            get { return _selectedAccount; }
            set
            {
                // Assign
                _selectedAccount = value;

                // Update indicator of whether this account allows the user to add transactions manually
                RaisePropertyChanged(() => IsManualAccount);
                RaisePropertyChanged(() => IsAutomaticAccount);

                // Update indicator of whether any account is selected
                RaisePropertyChanged(() => HasSelectedAccount);

                // Transactions will be updated since this is a different account
                _transactions = null;
                RaisePropertyChanged(() => Transactions);
                RaisePropertyChanged(() => SelectedAccountDailyBalances);
                RaisePropertyChanged(() => AccountHighBalance);
                RaisePropertyChanged(() => AccountLowBalance);
                RaisePropertyChanged(() => AccountAvgBalance);

                // Selected account name is changed
                RaisePropertyChanged(() => SelectedAccountName);

                // Name of account is changed
                RaisePropertyChanged(() => FiUserName);

                // If no tab is selected, show overview tab
                if (value!=null && String.IsNullOrEmpty(SelectedTabSource))
                    SelectedTabSource = "/Views/AccountsViewTabs/AccountGraph.xaml";

                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Bindable indicator of whether any account is selected
        /// </summary>
        public bool HasSelectedAccount => SelectedAccount != null;

        /// <summary>
        /// Bound to selected account name. Used for configuration tab.
        /// </summary>
        public string SelectedAccountName
        {
            get
            {
                if (SelectedAccount == null)
                    return "";
                return SelectedAccount.AccountName;
            }
            set
            {
                using (var dataService = new DataService())
                {
                    // Update in account object
                    SelectedAccount.AccountName = value;

                    // Save to database
                    dataService.UpdateAccount(SelectedAccount);
                }

                RaisePropertyChanged(() => SelectedAccount.AccountName);

                RaisePropertyChanged();
            }
            
        }

        /// <summary>
        /// Bound current transaction
        /// </summary>
        private Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get { return _selectedTransaction; }
            set
            {
                // Assign
                _selectedTransaction = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Bound source XAML path of the currently selected tab
        /// </summary>
        private string _selectedTabSource;
        public string SelectedTabSource
        {
            get { return _selectedTabSource;  }
            set
            {
                _selectedTabSource = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Provides the collection of transactions for the currently selected account
        /// </summary>
        private ObservableCollection<Transaction>_transactions ; 
        public ObservableCollection<Transaction> Transactions
        {
            get
            {
                if (SelectedAccount == null)
                    return null;
                using (var dataService = new DataService())
                {
                    // Retrieve account - we need to get an entity in the current db session
                    var transactions = dataService.GetTransactionsByDate(SelectedAccount);

                    // Calculate running balance and attach edit notifications
                    int balance = 0;
                    foreach (var transaction in transactions)
                    {
                        // Calculate balance
                        balance += transaction.Amount;
                        transaction.Balance = balance;

                        // Attach notifications for edit events
                        transaction.EditEnded += OnTransactionEditEnded;
                    }

                    // Reverse transactions so newest are listed first
                    _transactions = new ObservableCollection<Transaction>(transactions.Reverse());

                    // Ensure the new item row is at the top of the view
                    ((IEditableCollectionView) CollectionViewSource.GetDefaultView(_transactions)).NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtBeginning;

                    // Get notified when the user modifies entries in the datagrid
                   _transactions.CollectionChanged += TransactionsOnCollectionChanged;
                    return _transactions;

                }
            }
        }

        /// <summary>
        /// Encapsulating class for a balance associated with a day
        /// </summary>
        public class DailyBalance
        {
            public DateTime Date { get; set; }
            public decimal Balance { get; set; }
        }

        /// <summary>
        /// Helper function which calculates daily balances from a date-ordered enumeration of transactions
        /// </summary>
        /// <param name="transactions">Enumeration of transactions ordered by date</param>
        /// <returns>Ordered collection of daily balances - in the same order as the transactions provided</returns>
        private static IEnumerable<DailyBalance> GetDailyBalances(IEnumerable<Transaction> transactions)
        {
            ObservableCollection<DailyBalance> dailyBalances = new ObservableCollection<DailyBalance>();

            // Sorted list of transaction dates so we can find the earliest
            var sortedTransactionDates = from transaction in transactions
                orderby transaction.Date
                select transaction.Date;

            // If no transactions, nothing to do
            if (!sortedTransactionDates.Any())
                return dailyBalances;

            // Start with earliest date in transactions - begining of day
            var startDate = sortedTransactionDates.First();
            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            var oneDay = new TimeSpan(1,0,0,0);

            // End at the start of tomorrow
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day) + oneDay;
            decimal balance = 0.0m;
            while (startDate < DateTime.Now)
            {
                var nextDate = startDate + oneDay;

                // Get transactions for day
                var dailyTransactions = from transaction in transactions
                    where transaction.Date >= startDate && transaction.Date < nextDate
                    orderby transaction.Date descending
                    select transaction;

                // Get ending balance for the day
                if (dailyTransactions.Any())
                    balance = dailyTransactions.First().LocalizedBalance;

                // Add to set
                dailyBalances.Add(new DailyBalance {Date=startDate, Balance = balance});

                // Next day
                startDate = nextDate;
            }

            return dailyBalances;
        }

        /// <summary>
        /// Bound daily balances for the selected account
        /// </summary>
        public IEnumerable<DailyBalance> SelectedAccountDailyBalances
        {
            get
            {
                // Retrieve transactions
                var transactions = _transactions ?? Transactions;
                if (transactions == null)
                    return null;

                var startDate = DateTime.Now - new TimeSpan(SummaryDays, 0, 0, 0);

                var result = (from balance in GetDailyBalances(transactions) where balance.Date >= startDate orderby balance.Date select balance );
                return result;
            }
            
        }

        /// <summary>
        /// Determination of whether the user should be able to add transactions to the ledger
        /// </summary>
        public bool IsManualAccount
        {
            get
            {
                // If there is no selected account, transactions cannot be added
                if (SelectedAccount == null)
                    return false;

                // If the account is associated with a financial institution user, it is an auto-update account
                //  and users may not add transactions manually
                return (!IsAutomaticAccount);
            }
        }

        /// <summary>
        /// Determination of whether this is an automatic account - used for configuration field visibility check
        /// </summary>
        public bool IsAutomaticAccount
        {
            get
            {
                // If there is no selected account, the account is not automatic
                if (SelectedAccount == null)
                    return false;

                // If the account is associated with a financial institution user, it is an auto-update account
                return (SelectedAccount.IsAssociatedWithFinancialInstitution);
            }
        }

        #endregion

        #region [Account Summary Bindings]

        /// <summary>
        /// Number of days included in summary
        /// </summary>
        private readonly int SummaryDays = 365;

        /// <summary>
        /// High daily balance over the configured summary days
        /// </summary>
        public DailyBalance AccountHighBalance
        {
            get
            {
                var balances = SelectedAccountDailyBalances;
                return balances == null || !balances.Any() ? null : (from balance in balances orderby balance.Balance descending select balance).First();
            }
        }

        /// <summary>
        /// Low daily balance over the configured summary days
        /// </summary>
        public DailyBalance AccountLowBalance
        {
            get
            {
                var balances = SelectedAccountDailyBalances;
                return balances == null || !balances.Any() ? null : (from balance in balances orderby balance.Balance select balance).First();
            }
        }

        /// <summary>
        /// Average daily balance over the configured summary days - just the balance
        /// </summary>
        public decimal AccountAvgBalance
        {
            get
            {
                var balances = SelectedAccountDailyBalances;
                return balances == null || !balances.Any() ? 0.0m : (from balance in balances select balance.Balance).Average();
            }
        }

        #endregion

        #region [Account Configuration Bindings]

        /// <summary>
        /// Bound userid for FI account
        /// </summary>
        public string FiUserName
        {
            get
            {
                return SelectedAccount?.FinancialInstitutionUser?.UserId;
            }
            set
            {
                SelectedAccount.FinancialInstitutionUser.UserId = value;

                // Clear credentials verification status
                CredentialsFailed = false;
                CredentialsVerified = false;

                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Bound indicator of whether the current credentials have been validated 
        /// </summary>
        private bool _credentialsVerified;
        public bool CredentialsVerified
        {
            get { return _credentialsVerified;  }
            set
            {
                _credentialsVerified = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Bound indicator of whether the current credentials failed validation 
        /// </summary>
        private bool _credentialsFailed;
        public bool CredentialsFailed
        {
            get { return _credentialsFailed; }
            set
            {
                _credentialsFailed = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// Called to refresh the list of accounts from the database
        /// </summary>
        public void UpdateAccounts()
        {
            var accountsByType = new Dictionary<string, ObservableCollection<Account>>();

            // Retrieve the accounts from the database
            using (var dataService = new DataService())
            {
                // Map all accounts by type
                foreach (var account in dataService.GetAccountsWithFi())
                {
                    // Add category if needed
                    ObservableCollection<Account> accountList;
                    if (!accountsByType.ContainsKey(account.AccountType))
                    {
                        accountList = new ObservableCollection<Account>();
                        accountsByType.Add(account.AccountType, accountList);
                    }
                    else
                    {
                        accountList = accountsByType[account.AccountType];
                    }

                    // Add to the list under this type
                    accountList.Add(account);
                }
            }

            // Store in cached view
            _accountsView = accountsByType;

            // Notify of property update
            RaisePropertyChanged(() => AccountsView);
        }

        /// <summary>
        /// Handle modification events from the DataGrid containing transactions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="notifyCollectionChangedEventArgs"></param>
        private void TransactionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            switch (notifyCollectionChangedEventArgs.Action)
            {
                // Handle a new transaction in the collection
                case NotifyCollectionChangedAction.Add:
                    {
                        // Attach notification event handlers for this transaction
                        var transaction = (Transaction)notifyCollectionChangedEventArgs.NewItems[0];
                        transaction.EditBegun += OnNewTransactionEditBegun;
                        transaction.EditEnded += OnTransactionEditEnded;

                        break;
                    }
            }
        }

        /// <summary>
        /// Called after the DB context has been opened for editing a new transaction
        /// </summary>
        /// <param name="sender">Always a Transaction</param>
        /// <param name="eventArgs">Not used</param>
        private void OnNewTransactionEditBegun(object sender, EventArgs eventArgs)
        {
            var transaction = (Transaction)sender;

            // Ensure the Account is attached to the same context as the transaction
            transaction.EditContext.SetUnchanged(SelectedAccount);
            transaction.Account = SelectedAccount;

            // This is a new transaction. Ensure it is being ADDED to the database
            transaction.EditContext.SetAdded(transaction);
        }

        /// <summary>
        /// Called after the transaction is saved to the database
        /// </summary>
        /// <param name="sender">Always a Transaction</param>
        /// <param name="eventArgs">Not used</param>
        private void OnTransactionEditEnded(object sender, EventArgs eventArgs)
        {
            // Need to re-sort the data and recalculate balances
            RaisePropertyChanged(() => Transactions);
            RaisePropertyChanged(() => SelectedAccountDailyBalances);
        }

        /// <summary>
        /// Binding for the Add Account button
        /// </summary>
        private ICommand _addAccountCommand;
        public ICommand AddAccountCommand
        {
            get { return _addAccountCommand ?? (_addAccountCommand = new RelayCommand(AddAccount, () => true)); }
        }

        /// <summary>
        /// Transitions to Add Account view
        /// </summary>
        public void AddAccount()
        {
            // Transition view
            _modernNavigationService.NavigateTo(nameof(ViewModelLocator.AddAccount));
        }

        /// <summary>
        /// Binding for the Delete Transaction action (delete key from ledger)
        /// </summary>
        private ICommand _deleteTransactionCommand;
        public ICommand DeleteTransactionCommand
        {
            get { return _deleteTransactionCommand ?? (_deleteTransactionCommand = new RelayCommand(DeleteTransaction, () => IsManualAccount)); }
        }

        /// <summary>
        /// Deletes the transaction currently selected in the datagrid
        /// </summary>
        public void DeleteTransaction()
        {
            using (var dataService = new DataService())
            {
                // Delete the transaction
                dataService.DeleteTransaction(SelectedTransaction);
            }

            // Need to re-sort the data and recalculate balances
            RaisePropertyChanged(() => Transactions);
            RaisePropertyChanged(() => SelectedAccountDailyBalances);
        }

        /// <summary>
        /// Binding for the Import Transactions button
        /// </summary>
        private ICommand _importTransactionsCommand;
        public ICommand ImportTransactionsCommand
        {
            get { return _importTransactionsCommand ?? (_importTransactionsCommand = new RelayCommand(ImportTransactions, () => SelectedAccount!=null )); }
        }

        /// <summary>
        /// Prompts for an OFX file and imports transactions
        /// </summary>
        public async void ImportTransactions()
        {
            var fileDialog = new OpenFileDialog {Filter = "Bank Statements (*.ofx, *.qfx)|*.ofx;*.qfx"};
            if (fileDialog.ShowDialog() != true)
            {
                // Cancel selected, ignore
                return;
            }

            // Open the file the user selected for read
            using (var ofxFileStream = fileDialog.OpenFile())
            {
                // Parse the file and merge transactions into the current account
                await Task.Run(() => UpdateService.MergeOfxFileIntoAccount(SelectedAccount, ofxFileStream)).ConfigureAwait(false);
            }

            // Update transactions
            RaisePropertyChanged(() => Transactions);
            RaisePropertyChanged(() => SelectedAccountDailyBalances);

            // Move to transactions tab
            SelectedTabSource = "/Views/AccountsViewTabs/AccountLedger.xaml";
        }


        /// <summary>
        /// Binding for the Download Transactions button
        /// </summary>
        private ICommand _downloadTransactionsCommand;
        public ICommand DownloadTransactionsCommand
        {
            get { return _downloadTransactionsCommand ?? (_downloadTransactionsCommand = new RelayCommand(DownloadTransactions, () => SelectedAccount?.FiUserId != null)); }
        }

        /// <summary>
        /// Initiates a download of transactions from the remote financial institution
        /// </summary>
        public async void DownloadTransactions()
        {
            // Download transactions asynchronously and wait for response
            await UpdateService.DownloadOfxTransactionsForAccount(SelectedAccount).ConfigureAwait(false);

            // Update transactions
            RaisePropertyChanged(() => Transactions);
            RaisePropertyChanged(() => SelectedAccountDailyBalances);
            // Move to transactions tab

            SelectedTabSource = "/Views/AccountsViewTabs/AccountLedger.xaml";
        }

        /// <summary>
        /// Binding for the Delete Account button
        /// </summary>
        private ICommand _deleteAccountCommand;
        public ICommand DeleteAccountCommand
        {
            get { return _deleteAccountCommand ?? (_deleteAccountCommand = new RelayCommand(DeleteSelectedAccount, () => true)); }
        }

        /// <summary>
        /// Deletes the selected account from the database
        /// </summary>
        public void DeleteSelectedAccount()
        {
            using (var dataService = new DataService())
            {
                // Delete the account
                dataService.DeleteAccount(SelectedAccount);
            }

            // Set to no account
            SelectedAccount = null;

            // Return to Overview tab

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
               NavigationCommands.GoToPage.Execute("/Views/AccountsViewTabs/AccountGraph.xaml",
                   NavigationService.GetDescendantFromName(Application.Current.MainWindow, "TabFrame"))
                ));

            // Update the list of accounts
            UpdateAccounts();
        }

        /// <summary>
        /// Binding for the Unlink Account button
        /// </summary>
        private ICommand _unlinkAccountCommand;
        public ICommand UnlinkAccountCommand
        {
            get { return _unlinkAccountCommand ?? (_unlinkAccountCommand = new RelayCommand(UnlinkSelectedAccount, () => IsAutomaticAccount)); }
        }

        /// <summary>
        /// Unlink the selected automatic update account, turning it into a manual update account
        /// </summary>
        public void UnlinkSelectedAccount()
        {
            using (var dataService = new DataService())
            {
                // Unlink from fiUser
                dataService.UnlinkAccount(SelectedAccount);
            }

            // Manual and automatic account properties changed 
            RaisePropertyChanged(() => IsAutomaticAccount);
            RaisePropertyChanged(() => IsManualAccount);
        }

        /// <summary>
        /// Binding for the Unlink Account button
        /// </summary>
        private ICommand _verifyAndSaveCredentialsCommand;
        public ICommand VerifyAndSaveCredentialsCommand
        {
            get { return _verifyAndSaveCredentialsCommand ?? (_verifyAndSaveCredentialsCommand = new RelayCommand<object>(async p => await VerifyAndSaveCredentials(p), CanVerifyCredentials)); }
        }

        /// <summary>
        /// Logic for determining whether the Verify and Save action should be available
        /// </summary>
        /// <param name="passwordEntry">Password entry box</param>
        /// <returns>True - Can verify  False - Not enough information to verify</returns>
        private bool CanVerifyCredentials(object passwordEntry)
        {
            // Can only verify for automatic update accounts
            if (!IsAutomaticAccount)
                return false;

            // Cannot have a null or empty user name
            if (string.IsNullOrEmpty(FiUserName))
                return false;

            // Retrieve password from entry
            var passwordBox = passwordEntry as PasswordBox;
            return !string.IsNullOrEmpty(passwordBox?.Password);
        }


        /// <summary>
        /// Verify the user provided credentials against the configured FI. If they verify
        /// </summary>
        public async Task VerifyAndSaveCredentials(object passwordEntry)
        {
            // Retrieve password from entry
            var passwordBox = passwordEntry as PasswordBox;
            var password = passwordBox?.Password;

            // Store the account we're updating in case it changes while we're validating
            var updateAccount = SelectedAccount;

            // Form credentials into proper type for verification
            var credentials = new OFX.Types.Credentials(FiUserName, password);

            // Verify credentials and update if verification fails
            try
            {
                
                await
                    UpdateService.VerifyAccountCredentials(
                        SelectedAccount.FinancialInstitutionUser.FinancialInstitution,
                        credentials).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Verify failed
                CredentialsFailed = true;
                CredentialsVerified = false;
                return;
            }

            // Verification OK
            CredentialsFailed = false;

            // Update FI user
            updateAccount.FinancialInstitutionUser.UserId = credentials.UserId;
            updateAccount.FinancialInstitutionUser.Password = credentials.Password;

            using (var dataService = new DataService())
            {
                // Save to DB
                dataService.UpdateFiUser(updateAccount.FinancialInstitutionUser);
            }

            // Saved
            CredentialsVerified = true;
        }

    }
}
