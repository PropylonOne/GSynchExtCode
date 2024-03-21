﻿using System;
using System.Linq;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.AP;
using PX.Common;
using static PX.Data.Events;
using PX.SM;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.PM;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using static PX.Data.PXGenericInqGrph;
using static PX.Objects.IN.InventoryItem;
using PX.Objects.GL;


namespace GSynchExt
{
    public class APInvoiceEntryGSExt : PXGraphExtension<APInvoiceEntry>
    {

        #region Constants
        private string screenID = PXContext.GetScreenID();
        private bool isNotPettyCash;
        #endregion

        #region Dialogs
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public partial class InvoDialogInfo : IBqlTable
        {
            #region VendorID
            public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
            [Vendor(typeof(Search<Vendor.bAccountID>), DisplayName = "Vendor", DescriptionField = typeof(Vendor.acctName))]
            public virtual int? VendorID { get; set; }
            #endregion

            #region DefaultExpenseSubID
            public abstract class defaultExpenseSubID : PX.Data.BQL.BqlInt.Field<defaultExpenseSubID> { }
            [SubAccount(DisplayName = "Default Expense Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
            [PXDefault(typeof(PMProject.defaultExpenseSubID))]
            public virtual Int32? DefaultExpenseSubID
            {
                get;
                set;
            }
            #endregion

            #region DefaultExpenseAccountID
            public abstract class defaultExpenseAccountID : PX.Data.BQL.BqlInt.Field<defaultExpenseAccountID> { }
            [Account(DisplayName = "Default Cost Account")]
            [PXDefault(typeof(PMProject.defaultExpenseAccountID))]
            public virtual Int32? DefaultExpenseAccountID
            {
                get;
                set;
            }
            #endregion
        }
        #endregion

        public virtual APInvoice CreateAPBill(PMCostBudget budget, InvoDialogInfo info)
        {
           
            APInvoice aPInvoice = new APInvoice();
            if (budget == null) return aPInvoice;
            aPInvoice.VendorID = info.VendorID;
            aPInvoice = this.Base.Document.Insert(aPInvoice);

            APTran tran = new APTran();
            if(budget.InventoryID != 1)
            {
                tran.InventoryID = budget.InventoryID;
            }
            if (budget.InventoryID == 1)
            {
                tran.TranDesc = budget.Description;
            }
            tran.Qty = budget.RevisedQty - budget.ActualQty;
            tran.UOM = budget.UOM;
            tran.ProjectID = budget.ProjectID;
            tran.TaskID = budget.TaskID;
            tran.CostCodeID = budget.CostCodeID;
            tran.SubID = info.DefaultExpenseSubID;
            tran.AccountID = info.DefaultExpenseAccountID;
            this.Base.Transactions.Insert(tran);

            return aPInvoice;
        }
        public virtual APTran CreateAPBillFromCostBudget(PMCostBudget budget, InvoDialogInfo info, bool redirect = false)
        {
            CreateAPBill(budget, info);
            if (this.Base.Transactions.Cache.IsDirty)
            {
                if (redirect)
                    throw new PXRedirectRequiredException(this.Base, "");
                else
                    return this.Base.Transactions.Current;
            }
            throw new PXException("");
        }
        public override void Initialize()
        {
            base.Initialize();
            isNotPettyCash = screenID != "GS.30.20.00";
            var scID = PXContext.GetScreenID();

        }
        protected virtual void _(Events.FieldDefaulting<APPayment.docType> e)
        {
            if (e.Row == null) return;
            isNotPettyCash = screenID != "GS.30.20.00";
            if (isNotPettyCash == true) return;
            var userID = base.Base.Accessinfo.UserID;
            var userName = base.Base.Accessinfo.UserName;
            //APPaymentEntry graph = PXGraph.CreateInstance<APPaymentEntry>();

            APPayment payment = (APPayment)e.Row;
            APPaymentGSExt extension = PXCache<APPayment>.GetExtension<APPaymentGSExt>(payment);

            EPEmployee vendor = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.
                                Select(base.Base, userID);
            e.Cache.SetValueExt<APPayment.vendorID>(e.Row, vendor?.BAccountID);
            e.Cache.SetValueExt<APPayment.docType>(e.Row, APDocType.Prepayment);
            e.Cache.SetValueExt<APPayment.docDesc>(e.Row, "Petty Cash Request");
            e.Cache.SetValueExt<APPayment.paymentMethodID>(e.Row, "CASH");
            e.Cache.SetValueExt<APPaymentGSExt.usrIsPettyCash>(e.Row, true);

        }
    }
}