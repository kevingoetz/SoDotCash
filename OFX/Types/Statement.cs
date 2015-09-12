﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFX.Types
{
    /// <summary>
    /// Container class for statement data retrieved from a financial institution for a single account
    /// </summary>
    public class Statement
    {
        /// <summary>
        /// Construct a Statement from source OFX data - StatementResponse object (bank account)
        /// </summary>
        /// <param name="sourceOFX">OFX StatementResponse object retrieved from FI</param>
        public Statement(StatementResponse statementResponse)
        {
            // Parse account information
            OwningAccount = Account.Create(statementResponse.BANKACCTFROM);

            // Parse currency into a string
            Currency = statementResponse.CURDEF.ToString();

            // Parse statement date range
            StartDate = OFXUtils.DateFromOFX(statementResponse.BANKTRANLIST.DTSTART);
            EndDate = OFXUtils.DateFromOFX(statementResponse.BANKTRANLIST.DTEND);

            // Parse account balance
            AccountBalance = OFXUtils.decimalStringToFixedInt(statementResponse.LEDGERBAL.BALAMT);

            // Convert OFX transactions into our normalized Transaction objects
            Transactions = new List<Transaction>(from ofxTransaction in statementResponse.BANKTRANLIST.STMTTRN select new Transaction(ofxTransaction));
        }

        /// <summary>
        /// Construct a Statement from source OFX data - StatementResponse object (credit card account)
        /// </summary>
        /// <param name="sourceOFX">OFX CreditCardStatementResponse object retrieved from FI</param>
        public Statement(CreditCardStatementResponse statementResponse)
        {
            // Parse account information
            OwningAccount = Account.Create(statementResponse.CCACCTFROM);

            // Parse currency into a string
            Currency = statementResponse.CURDEF.ToString();

            // Parse statement date range
            StartDate = OFXUtils.DateFromOFX(statementResponse.BANKTRANLIST.DTSTART);
            EndDate = OFXUtils.DateFromOFX(statementResponse.BANKTRANLIST.DTEND);

            // Parse account balance
            AccountBalance = OFXUtils.decimalStringToFixedInt(statementResponse.LEDGERBAL.BALAMT);

            // Convert OFX transactions into our normalized Transaction objects
            Transactions = new List<Transaction>(from ofxTransaction in statementResponse.BANKTRANLIST.STMTTRN select new Transaction(ofxTransaction));
        }

        /// <summary>
        /// Construct Statement objects for each statement responses included in the passed OFX response
        /// </summary>
        /// <param name="ofxResponse">OFX object populated with one or more statement responses</param>
        /// <returns></returns>
        public static List<Statement> CreateFromOFXResponse(OFX ofxResponse)
        {
            List<Statement> statementList = new List<Statement>();

            // Handle Credit Card responses
            foreach (
                var responseMessageSet in
                    ofxResponse.Items.Where(item => item.GetType() == typeof (CreditcardResponseMessageSetV1))
                        .Select(item => (CreditcardResponseMessageSetV1) item))
            {
                foreach (
                    var transactionResponse in
                        responseMessageSet.Items.Where(
                            item => item.GetType() == typeof (CreditCardStatementTransactionResponse))
                            .Select(item => (CreditCardStatementTransactionResponse) item))
                {
                    statementList.Add(new Statement(transactionResponse.CCSTMTRS));
                }
            }

            // Handle Bank responses
            foreach (
                var responseMessageSet in
                    ofxResponse.Items.Where(item => item.GetType() == typeof(BankResponseMessageSetV1))
                        .Select(item => (BankResponseMessageSetV1)item))
            {
                foreach (
                    var transactionResponse in
                        responseMessageSet.Items.Where(
                            item => item.GetType() == typeof(StatementTransactionResponse))
                            .Select(item => (StatementTransactionResponse)item))
                {
                    statementList.Add(new Statement(transactionResponse.STMTRS));
                }
            }

            // Return populated list
            return statementList;
        }

        /// <summary>
        /// The account associated with the transactions in this statement
        /// </summary>
        public
        Account OwningAccount { get; }

        /// <summary>
        /// Currency for transactions and balances in this statement
        /// </summary>
        public string Currency { get; }

        /// <summary>
        /// Transactions included in this statement
        /// </summary>
        public List<Transaction> Transactions { get; }

        /// <summary>
        /// Start of the date range of transactions included in this statement
        /// </summary>
        public DateTimeOffset StartDate { get; }

        /// <summary>
        /// End of the date range of transactions included in this statement
        /// </summary>
        public DateTimeOffset EndDate { get; }

        /// <summary>
        /// The account balance reported in this statement request. In the currency units of the account.
        /// </summary>
        public int AccountBalance { get; }
    }
}
