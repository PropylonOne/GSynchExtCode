using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Common;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.WorkflowAPI;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM.Extensions;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.Common.Bql;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects;
using PX.SM;
using PX.TM;
using System.Collections.Generic;
using System;
using PX.Objects.GL;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using static PX.Objects.AP.APDocumentEnq;
using PX.Data.EP;

namespace GSynchExt
{
    [PXProjection(typeof(Select<CATransfer>))]
    public partial class CATransferGSExt : CATransfer
    {
       
        
        
        #region TransferNbr
        public abstract class transferNbr : PX.Data.BQL.BqlString.Field<transferNbr> { }

        /// <summary>
        /// The user-friendly unique identifier of the transfer.
        /// This field is the auto-numbering key field.
        /// </summary>
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXSelector(typeof(CATransferGSExt.transferNbr))]
        [AutoNumber(typeof(CASetup.transferNumberingID), typeof(CATransferGSExt.inDate))]
        public override string TransferNbr
        {
            get;
            set;
        }
        #endregion

        #region OutCuryInfoID
        public abstract class outCuryInfoID : PX.Data.BQL.BqlLong.Field<outCuryInfoID> { }

        /// <summary>
        /// The identifier of the exchange rate record for the outcoming amount.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyInfo.CuryInfoID"/> field.
        /// </value>
		[PXDBLong]
        [CurrencyInfo]
        public override  long? OutCuryInfoID
        {
            get;
            set;
        }
        #endregion

        #region InCuryInfoID
        public abstract class inCuryInfoID : PX.Data.BQL.BqlLong.Field<inCuryInfoID> { }

        /// <summary>
        /// The identifier of the exchange rate record for the incoming amount.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyInfo.CuryInfoID"/> field.
        /// </value>
		[PXDBLong]
        [CurrencyInfo]
        public override long? InCuryInfoID
        {
            get;
            set;
        }
        #endregion


       #region OutAccountID
        public abstract class outAccountID : PX.Data.BQL.BqlInt.Field<outAccountID> { }

        /// <summary>
        /// The identifier of the source <see cref="CashAccount">cash account</see> from which the funds are transferred.
        /// </summary>
        /// <value>
        /// Corresponds to the value of the <see cref="CashAccount.CashAccountID"/> field.
        /// </value>
        [PXDefault]
        [TransferCashAccount(PairCashAccount = typeof(inAccountID), DescriptionField = typeof(CashAccount.descr))]
        public override int? OutAccountID
        {
            get;
            set;
        }
        #endregion
        #region InAccountID
        public abstract class inAccountID : PX.Data.BQL.BqlInt.Field<inAccountID> { }

        /// <summary>
        /// The identifier of the destination <see cref="CashAccount">cash account</see> to which the funds are transferred.
        /// </summary>
        /// <value>
        /// Corresponds to the value of the <see cref="CashAccount.CashAccountID"/> field.
        /// </value>
        [PXDefault]
        [TransferCashAccount(PairCashAccount = typeof(outAccountID), DisplayName = "Destination Account", DescriptionField = typeof(CashAccount.descr))]
        public override int? InAccountID
        {
            get;
            set;
        }
        #endregion

        #region InCuryID
        public abstract class inCuryID : PX.Data.BQL.BqlString.Field<inCuryID> { }

        /// <summary>
        /// The currency of denomination for the destination cash account.
        /// </summary>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
        [PXUIField(DisplayName = "Destination Currency", Enabled = false)]
        [PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<CATransfer.inAccountID>>>>))]
        [PXSelector(typeof(Currency.curyID))]
        public override string InCuryID
        {
            get;
            set;
        }
        #endregion
        #region OutCuryID
        public abstract class outCuryID : PX.Data.BQL.BqlString.Field<outCuryID> { }

        /// <summary>
        /// The currency of denomination for the source cash account.
        /// </summary>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
        [PXUIField(DisplayName = "Source Currency", Enabled = false)]
        [PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<CATransfer.outAccountID>>>>))]
        [PXSelector(typeof(Currency.curyID))]
        public override string OutCuryID
        {
            get;
            set;
        }
        #endregion
        #region CuryTranOut
        public abstract class curyTranOut : PX.Data.BQL.BqlDecimal.Field<curyTranOut> { }

        /// <summary>
        /// The amount of the transfer outcomes from the source cash account (in the specified currency).
        /// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Source Amount")]
        [PXDBCurrency(typeof(CATransfer.outCuryInfoID), typeof(CATransfer.tranOut))]
        public override decimal? CuryTranOut
        {
            get;
            set;
        }
        #endregion
        #region CuryTranIn
        public abstract class curyTranIn : PX.Data.BQL.BqlDecimal.Field<curyTranIn> { }

        /// <summary>
        /// The amount of the transfer incomes to the destination cash account (in the specified currency).
        /// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Destination Amount")]
        [PXDBCurrency(typeof(CATransfer.inCuryInfoID), typeof(CATransfer.tranIn))]
        public override decimal? CuryTranIn
        {
            get;
            set;
        }
        #endregion
        #region TranOut
        public abstract class tranOut : PX.Data.BQL.BqlDecimal.Field<tranOut> { }

        /// <summary>
        /// The amount of the transfer outcomes from the source cash account (in the base currency).
        /// </summary>
		[PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Currency Amount", Enabled = false)]
        public override decimal? TranOut
        {
            get;
            set;
        }
        #endregion
        #region TranIn
        public abstract class tranIn : PX.Data.BQL.BqlDecimal.Field<tranIn> { }

        /// <summary>
        /// The amount of the transfer incomes to the destination cash account (in the base currency).
        /// </summary>
		[PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Currency Amount", Enabled = false)]
        public override decimal? TranIn
        {
            get;
            set;
        }
        #endregion
        #region InDate
        public abstract class inDate : PX.Data.BQL.BqlDateTime.Field<inDate> { }

        /// <summary>
        /// The date of the transfer receipt.
        /// </summary>
		[PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Receipt Date", Visibility = PXUIVisibility.SelectorVisible)]
        public override DateTime? InDate
        {
            get;
            set;
        }
        #endregion
        #region OutDate
        public abstract class outDate : PX.Data.BQL.BqlDateTime.Field<outDate> { }

        /// <summary>
        /// The date of the transaction (when funds were withdrawn from the source cash account).
        /// </summary>
		[PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Transfer Date")]
        public override DateTime? OutDate
        {
            get;
            set;
        }
        #endregion

        #region InBranchID
        public abstract class inBranchID : PX.Data.BQL.BqlInt.Field<inBranchID> { }
        [PXFormula(typeof(Default<inAccountID>))]
        [PXDefault(typeof(Search<CashAccount.branchID, Where<CashAccount.cashAccountID, Equal<Current<inAccountID>>>>))]
        [PXDBInt]
        public override int? InBranchID
        {
            get;
            set;
        }
        #endregion

        #region OutBranchID
        public abstract class outBranchID : PX.Data.BQL.BqlInt.Field<outBranchID> { }
        [PXFormula(typeof(Default<outAccountID>))]
        [PXDefault(typeof(Search<CashAccount.branchID, Where<CashAccount.cashAccountID, Equal<Current<outAccountID>>>>))]
        [PXDBInt]
        public override int? OutBranchID
        {
            get;
            set;
        }
        #endregion
        #region InTranPeriodID
        public abstract class inTranPeriodID : IBqlField { }

        [PeriodID]
        public override string InTranPeriodID { get; set; }
        #endregion
        #region InPeriodID
        public abstract class inPeriodID : PX.Data.BQL.BqlString.Field<inPeriodID> { }

        [CAOpenPeriod(typeof(inDate), typeof(inAccountID), typeof(Selector<inAccountID, CashAccount.branchID>), masterFinPeriodIDType: typeof(inTranPeriodID))]
        [PXUIField(DisplayName = "In Period", Visible = false,
            ErrorHandling = PXErrorHandling.Always, MapErrorTo = typeof(inDate))]
        public override string InPeriodID
        {
            get;
            set;
        }
        #endregion
        #region OutTranPeriodID
        public abstract class outTranPeriodID : PX.Data.BQL.BqlString.Field<outTranPeriodID> { }

        [PeriodID]
        public override string OutTranPeriodID { get; set; }
        #endregion
        #region OutPeriodID

        public abstract class outPeriodID : PX.Data.BQL.BqlString.Field<outPeriodID> { }

        [CAOpenPeriod(typeof(outDate), typeof(outAccountID), typeof(Selector<outAccountID, CashAccount.branchID>), masterFinPeriodIDType: typeof(outTranPeriodID))]
        [PXUIField(DisplayName = "Out Period", Visible = false,
            ErrorHandling = PXErrorHandling.Always, MapErrorTo = typeof(outDate))]
        public override string OutPeriodID
        {
            get;
            set;
        }
        #endregion
        #region OutExtRefNbr
        public abstract class outExtRefNbr : PX.Data.BQL.BqlString.Field<outExtRefNbr> { }

        /// <summary>
        /// The reference number of the transfer for the source cash account.
        /// This is a number provided by an external bank or organization.
        /// This field is entered manually.
        /// </summary>
		[PXDBString(40, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Document Ref.")]
        public override string OutExtRefNbr
        {
            get;
            set;
        }
        #endregion
        #region InExtRefNbr
        public abstract class inExtRefNbr : PX.Data.BQL.BqlString.Field<inExtRefNbr> { }

        /// <summary>
        /// The reference number of the transfer for the target cash account.
        /// This is a number provided by an external bank or organization.
        /// The value of the field is entered by a user.
        /// </summary>
        [PXDBString(40, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Document Ref.")]
        public override string InExtRefNbr
        {
            get;
            set;
        }
        #endregion
        #region TranIDOut
        public abstract class tranIDOut : PX.Data.BQL.BqlLong.Field<tranIDOut> { }

        /// <summary>
        /// The unique identifier of the outcoming CA transaction.
        /// </summary>
        /// /// <value>
        /// Corresponds to the value of the <see cref="CATran.TranID"/> field.
        /// </value>
        [PXDBLong]
        [TransferCashTranID]
        [PXSelector(typeof(Search<CATran.tranID>), DescriptionField = typeof(CATran.batchNbr))]
        public override long? TranIDOut
        {
            get;
            set;
        }
        #endregion
        #region TranIDIn
        public abstract class tranIDIn : PX.Data.BQL.BqlLong.Field<tranIDIn> { }

        /// <summary>
        /// The unique identifier of the incoming CA transaction.
        /// </summary>
        /// /// <value>
        /// Corresponds to the value of the <see cref="CATran.TranID"/> field.
        /// </value>
		[PXDBLong]
        [TransferCashTranID]
        [PXSelector(typeof(Search<CATran.tranID>), DescriptionField = typeof(CATran.batchNbr))]
        public override long? TranIDIn
        {
            get;
            set;
        }
        #endregion
        #region ExpenseCntr
        public abstract class expenseCntr : PX.Data.BQL.BqlInt.Field<expenseCntr> { }

        [PXDBInt]
        [PXDefault(0)]
        public override int? ExpenseCntr { get; set; }
        #endregion
        #region RGOLAmt
        public abstract class rGOLAmt : PX.Data.BQL.BqlDecimal.Field<rGOLAmt> { }

        /// <summary>
        /// A read-only box that displays the difference between the amount in the base currency specified for the source account 
        /// and the amount in the base currency resulting for the destination cash account, 
        /// for cases when the source and destination currencies are different.
        /// </summary>
		[PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "RGOL", Enabled = false)]
        public override decimal? RGOLAmt
        {
            get;
            set;
        }
        #endregion
        #region Hold
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
        protected bool? _Hold;

        /// <summary>
        /// Indicates (if set to <c>true</c>) that the transfer is on hold. The value of the field can be set to <c>false</c> only for balanced transfers.
        /// </summary>
		[PXDBBool]
        [PXDefault(typeof(Search<CASetup.holdEntry>))]
        [PXUIField(DisplayName = "Hold")]
        public override bool? Hold
        {
            get
            {
                return this._Hold;
            }

            set
            {
                this._Hold = value;
            }
        }
        #endregion
        #region Released
        public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
        protected bool? _Released;

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the transfer is released.
        /// </summary>
		[PXDBBool]
        [PXDefault(false)]
        public override bool? Released
        {
            get
            {
                return _Released;
            }

            set
            {
                this._Released = value;
            }
        }
        #endregion
        #region OrigTransferNbr
        public abstract class origTransferNbr : PX.Data.BQL.BqlString.Field<origTransferNbr> { }

        /// <summary>
        /// The number of the original transfer.
        /// </summary>
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Orig. Tran. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(CATransfer.transferNbr))]
        public override string OrigTransferNbr
        {
            get;
            set;
        }
        #endregion
        #region ReverseCount
        public abstract class reverseCount : PX.Data.BQL.BqlInt.Field<reverseCount> { }

        /// <summary>
        /// The read-only field, reflecting the number of transactions in the system, which reverse this transaction.
        /// </summary>
        /// <value>
        /// This field is populated only by the <see cref="CashTransferEntry"/> graph (corresponds to the Funds Transfer CA.30.10.00 screen).
        /// </value>
        [PXInt]
        [PXUIField(DisplayName = "Reversing Transactions", Visible = false, Enabled = false, IsReadOnly = true)]
        public int? ReverseCount
        {
            get;
            set;
        }
        #endregion
        
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        [PXDBCreatedByID]
        public override Guid? CreatedByID
        {
            get;
            set;
        }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        public override string CreatedByScreenID
        {
            get;
            set;
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
        public override DateTime? CreatedDateTime
        {
            get;
            set;
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        public override Guid? LastModifiedByID
        {
            get;
            set;
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        public override string LastModifiedByScreenID
        {
            get;
            set;
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
        public override DateTime? LastModifiedDateTime
        {
            get;
            set;
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        public override byte[] tstamp
        {
            get;
            set;
        }
        #endregion
        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        /// <summary>
        /// The status of the transfer.
        /// </summary>
        /// <value>
        /// The field can have one of the values described in <see cref="CATransferStatus.ListAttribute"/>.
        /// </value>
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [CATransferStatus.List]
        public override string Status
        {
            get;
            set;
        }
        #endregion
        #region ClearedOut
        public abstract class clearedOut : PX.Data.BQL.BqlBool.Field<clearedOut> { }

        /// <summary>
        /// Indicates (if set to <c>true</c>) that this outcoming transaction has been cleared.
        /// </summary>
		[PXDBBool]
        [PXUIField(DisplayName = "Cleared")]
        [PXDefault(false)]
        public override bool? ClearedOut
        {
            get;
            set;
        }
        #endregion
        #region ClearDateOut
        public abstract class clearDateOut : PX.Data.BQL.BqlDateTime.Field<clearDateOut> { }

        /// <summary>
        /// The date when the outcoming transaction was cleared in the process of reconciliation.
        /// </summary>
		[PXDBDate]
        [PXUIField(DisplayName = "Clear Date", Required = false)]
        public override DateTime? ClearDateOut
        {
            get;
            set;
        }
        #endregion
        #region ClearedIn
        public abstract class clearedIn : PX.Data.BQL.BqlBool.Field<clearedIn> { }

        /// <summary>
        /// Indicates (if set to <c>true</c>) that this incoming transaction has been cleared.
        /// </summary>
		[PXDBBool]
        [PXUIField(DisplayName = "Cleared")]
        [PXDefault(false)]
        public override bool? ClearedIn
        {
            get;
            set;
        }
        #endregion
        #region ClearDateIn
        public abstract class clearDateIn : PX.Data.BQL.BqlDateTime.Field<clearDateIn> { }

        /// <summary>
        /// The date when the incoming transaction was cleared in the process of reconciliation.
        /// </summary>
		[PXDBDate]
        [PXUIField(DisplayName = "Clear Date", Required = false)]
        public override DateTime? ClearDateIn
        {
            get;
            set;
        }
        #endregion
        #region CashBalanceIn
        public abstract class cashBalanceIn : PX.Data.BQL.BqlDecimal.Field<cashBalanceIn> { }

        /// <summary>
        /// The actual balance of the target cash account.
        /// </summary>
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXCurrency(typeof(inCuryInfoID))]
        [PXUIField(DisplayName = "Available Balance", Enabled = false)]
        [CashBalance(typeof(CATransfer.inAccountID))]
        public override decimal? CashBalanceIn
        {
            get;
            set;
        }
        #endregion
        #region CashBalanceOut
        public abstract class cashBalanceOut : PX.Data.BQL.BqlDecimal.Field<cashBalanceOut> { }

        /// <summary>
        /// The actual balance of the source cash account.
        /// </summary>
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXCurrency(typeof(outCuryInfoID))]
        [PXUIField(DisplayName = "Available Balance", Enabled = false)]
        [CashBalance(typeof(CATransfer.outAccountID))]
        public override decimal? CashBalanceOut
        {
            get;
            set;
        }
        #endregion
        #region InGLBalance
        public abstract class inGLBalance : PX.Data.BQL.BqlDecimal.Field<inGLBalance> { }

        /// <summary>
        /// The balance of the target account, as recorded in the General Ledger.
        /// </summary>
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXCurrency(typeof(inCuryInfoID))]
        [PXUIField(DisplayName = "GL Balance", Enabled = false)]
        [GLBalance(typeof(inAccountID), null, typeof(inDate))]
        public override decimal? InGLBalance
        {
            get;
            set;
        }
        #endregion
        #region OutGLBalance
        public abstract class outGLBalance : PX.Data.BQL.BqlDecimal.Field<outGLBalance> { }

        /// <summary>
        /// A read-only box displaying the balance of the source account recorded in the General Ledger
        /// for the financial period that includes the transfer date.
        /// </summary>
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXCurrency(typeof(outCuryInfoID))]
        [PXUIField(DisplayName = "GL Balance", Enabled = false)]
        [GLBalance(typeof(outAccountID), null, typeof(outDate))]
        public override decimal? OutGLBalance
        {
            get;
            set;
        }
        #endregion
        #region TranIDOut_CATran_batchNbr
        /// <summary>
        /// The batch number for the transfer. Only released transfers have batch numbers.
        /// The field is used as a link to the batch that contains the transaction for the source account in the General Ledger.
        /// This is a override field, which is filled in from the <see cref = "CATran.batchNbr" /> field (<see cref = "CashTransferEntry.CATransfer_TranIDOut_CATran_BatchNbr_FieldSelecting" />).
        /// </summary>
        public abstract class tranIDOut_CATran_batchNbr : PX.Data.BQL.BqlString.Field<tranIDOut_CATran_batchNbr> { }
        #endregion
        #region TranIDIn_CATran_batchNbr
        /// <summary>
        /// The number of the batch that contains the transaction for the target account in the General Ledger.
        /// Only released transfers have batch numbers.
        /// This is a override field, which is filled in from the <see cref = "CATran.batchNbr" /> field (<see cref="CashTransferEntry.CATransfer_TranIDIn_CATran_BatchNbr_FieldSelecting" />).
        /// </summary>
        public abstract class tranIDIn_CATran_batchNbr : PX.Data.BQL.BqlString.Field<tranIDIn_CATran_batchNbr> { }
        #endregion

    }
}