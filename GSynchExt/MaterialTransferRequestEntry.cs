using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.Update.ExchangeService;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.FA;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.PJ.Submittals.PJ.Graphs;
using PX.Objects.PM;
using PX.Objects.PO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GSynchExt.GSBOQMaint;
using static PX.Data.BQL.BqlPlaceholder;
using static PX.Objects.AP.APDocumentEnq;
using static PX.Objects.CA.CABankTran.FK;
using static PX.Objects.FA.FABookSettings.midMonthType;
using static PX.Objects.IN.InventoryTranSumEnqFilter;
using static PX.Objects.PM.GSProjectHelper;
using static PX.Objects.TX.CSTaxCalcType;
using BudgetKeyTuple = PX.Objects.CS.BudgetKeyTuple;
/// <summary>
/// This graph allows you to create a material request against a project's cost Budget
/// </summary>
namespace GSynchExt
{
    public class MaterialTransferRequestEntry : PXGraph<MaterialTransferRequestEntry, MaterialTransferRequest>
    {
        #region Selects
        public PXSelect<MaterialTransferRequest> MatlRequest;
        public PXSelect<MTRequestDetails, Where<MTRequestDetails.reqNbr,
            Equal<Current<MaterialTransferRequest.reqNbr>>>> MatlRequestDet;

        public PXSelect<MTRequestDetails, Where<MTRequestDetails.reqNbr,
          Equal<Current<MaterialTransferRequest.reqNbr>>,
          And<MTRequestDetails.selected, Equal<True>>>> MatlRequestSelectedDet;


        [PXHidden]
        public PXSelect<MTRequestDetails,
                Where<MTRequestDetails.reqNbr, Equal<Current<MTRequestDetails.reqNbr>>,
                And<MTRequestDetails.lineNbr, Equal<Current<MTRequestDetails.lineNbr>>>>> MatlRequestLineDet;

        public PXSelectGroupBy<MTRequestDetails, Where<MTRequestDetails.reqNbr, Equal<Current<MaterialTransferRequest.reqNbr>>>,
                Aggregate<GroupBy<MTRequestDetails.reqNbr,
                    Sum<MTRequestDetails.requestedQty,
                    Sum<MTRequestDetails.revisedQty,
                    Sum<MTRequestDetails.actualQty,
                    Sum<MTRequestDetails.issueQty,
                    Sum<MTRequestDetails.transferQty>>>>>>>> MatlRequestTotals;
        public PXSetup<FundTransferRequestSetup> AutoNumSetup;
        public PXFilter<CopyDialogInfo> CopyDialog; //Transfer
        public PXFilter<CopyDialogInfo> CopyDialog2; //Issue
        public SelectFrom<RequestedProjectMaterials>.View ReqProjectMaterials;
        public PXSelect<FundTransferRequestSetup> Setup;
        #endregion

        public IEnumerable matlRequestLineDet()
        {
            PXView select = new PXView(this, true, MatlRequestLineDet.View.BqlSelect);

            Int32 totalrow = 0;
            Int32 startrow = PXView.StartRow;

            List<object> result = select.Select(PXView.Currents, PXView.Parameters,
                   PXView.Searches, PXView.SortColumns, PXView.Descendings,
                   PXView.Filters, ref startrow, PXView.MaximumRows, ref totalrow);

            foreach (MTRequestDetails row in result)
            {
                if (row.IssueQty > 0)
                {
                    PXView.StartRow = 0;

                    INTran releasedIssue =
                     PXSelectGroupBy<INTran,
                      Where<INTran.released, Equal<True>,
                      And<INTran.tranType, Equal<INTranType.issue>,
                      And<INTranGSExt.usrMTRRef, Equal<Required<INTranGSExt.usrMTRRef>>,
                      And<INTran.projectID, Equal<Required<INTran.projectID>>,
                      And<INTran.inventoryID, Equal<Required<INTran.inventoryID>>,
                      And<INTran.taskID, Equal<Required<INTran.taskID>>,
                      And<INTran.costCodeID, Equal<Required<INTran.costCodeID>>>>>>>>>,
                      Aggregate<GroupBy<INTran.projectID,
                      GroupBy<INTran.inventoryID,
                          Sum<INTran.qty>>>>>.Select(this, row.ReqNbr, row.ProjectID, row.InventoryID, row.TaskID, row.CostCode).FirstOrDefault();

                    if (releasedIssue != null)
                    {
                        row.IssueQtyUnReleased = row.IssueQtyReleased - releasedIssue?.Qty;
                        row.IssueQtyReleased = releasedIssue?.Qty;

                    }
                    else
                    {
                        row.IssueQtyUnReleased = row.IssueQty;
                        row.IssueQtyReleased = 0;
                    }
                }
                else
                {
                    row.IssueQtyUnReleased = 0;
                    row.IssueQtyReleased = 0;
                }

                if (row.TransferQty > 0)
                {

                    INTran releasedTransfer =
                      PXSelectGroupBy<INTran,
                              Where<INTran.released, Equal<True>,
                              And<INTran.tranType, Equal<INTranType.transfer>,
                              And<INTran.toProjectID, Equal<Required<INTran.toProjectID>>,
                              And<INTran.inventoryID, Equal<Required<INTran.inventoryID>>,
                              And<INTran.toTaskID, Equal<Required<INTran.toTaskID>>,
                              And<INTranGSExt.usrMTRRef, Equal<Required<INTranGSExt.usrMTRRef>>,
                              And<INTran.toCostCodeID, Equal<Required<INTran.toCostCodeID>>>>>>>>>,
                              Aggregate<GroupBy<INTran.toProjectID,GroupBy<INTran.inventoryID,
                              Sum<INTran.qty>>>>>.Select(this, row.ProjectID, row.InventoryID, row.TaskID, row.ReqNbr, row.CostCode).FirstOrDefault();

                    if(releasedTransfer != null)
                    {
                        row.TransferQtyUnReleased = row.TransferQty - releasedTransfer?.Qty;
                        row.TransferQtyReleased = releasedTransfer?.Qty;

                    }
                    else
                    {
                        row.TransferQtyUnReleased = row.TransferQty;
                        row.TransferQtyReleased = 0;
                    }
                }
                else
                {
                    row.TransferQtyUnReleased = 0;
                    row.TransferQtyReleased = 0;
                }
            }
            return result;
        }

        #region Constructor
        public MaterialTransferRequestEntry()
        {
            //       FundTransferRequestSetup setup = AutoNumSetup.Current;

        }
        #endregion

        #region Events
        protected virtual void _(Events.RowSelected<MaterialTransferRequest> e)
        {
            MaterialTransferRequest doc = (MaterialTransferRequest)e.Row;
            if (doc == null) return;
            /// Set Totals
            MTRequestDetails totalsRec = this.MatlRequestTotals.Select().Where(x => x.Record.ReqNbr == doc.ReqNbr).FirstOrDefault();
            if (totalsRec != null)
            {
                doc.TransferQty = totalsRec.TransferQty;
                doc.IssueQty = totalsRec.IssueQty;
                doc.RequestQty = totalsRec.RequestedQty;
            }

            bool isReleased = doc.Status == GSynchExt.FTRStatus.Released && doc.ReqNbr != null;
            bool haslines = MatlRequestDet.Select().Count() > 0;
            bool isOnHold = doc.Status == GSynchExt.FTRStatus.OnHold;
            bool isClosed = doc.Status == GSynchExt.FTRStatus.Closed;
            e.Cache.AllowDelete = isOnHold;

            this.MatlRequestDet.Cache.AllowInsert = isOnHold;
            this.MatlRequestDet.Cache.AllowUpdate = isOnHold;
            this.MatlRequestDet.Cache.AllowDelete = isOnHold;

            CreateTransfer.SetEnabled(isReleased);
            CreateIssue.SetEnabled(isReleased);
            Close.SetEnabled(isReleased && !e.Cache.IsDirty);
            Reverse.SetEnabled(isClosed && !e.Cache.IsDirty);
            Hold2.SetEnabled(isReleased && !e.Cache.IsDirty);
            Cancel2.SetEnabled(isReleased && !e.Cache.IsDirty);
          //  loadBudget.SetEnabled(isOnHold && !e.Cache.IsDirty);

            CreateTransfer.SetVisible(isReleased);
            CreateIssue.SetVisible(isReleased);
            Close.SetVisible(isReleased && !e.Cache.IsDirty);
            Reverse.SetVisible(isClosed && !e.Cache.IsDirty);
            Hold2.SetVisible(isReleased && !e.Cache.IsDirty);
            Cancel2.SetVisible(isReleased && !e.Cache.IsDirty);

            PXUIFieldAttribute.SetEnabled<MaterialTransferRequest.projectID>(e.Cache, doc, !haslines && isOnHold);
            PXUIFieldAttribute.SetEnabled<MaterialTransferRequest.fromSiteID>(e.Cache, doc, isOnHold);
            PXUIFieldAttribute.SetEnabled<MaterialTransferRequest.toSiteID>(e.Cache, doc, isOnHold);

            PXUIFieldAttribute.SetRequired<MaterialTransferRequest.notify>(e.Cache, true);
            PXUIFieldAttribute.SetRequired<MaterialTransferRequest.fromSiteID>(e.Cache, true);
            PXUIFieldAttribute.SetRequired<MaterialTransferRequest.reqBy>(e.Cache, true);
            PXUIFieldAttribute.SetRequired<MaterialTransferRequest.reqDate>(e.Cache, true);
            PXUIFieldAttribute.SetRequired<MaterialTransferRequest.requestQty>(e.Cache, true);

        }

        protected virtual void _(Events.RowSelected<MTRequestDetails> e)
        {
            MTRequestDetails doc = (MTRequestDetails)e.Row;
            if (doc == null) return;

            MaterialTransferRequest req = MaterialTransferRequest.UK.Find(this, doc.ReqNbr);

            bool isReleased = req.Status == GSynchExt.FTRStatus.Released && doc.ReqNbr != null;
            bool haslines = MatlRequestDet.Select().Count() > 0;
            bool isOnHold = req.Status == GSynchExt.FTRStatus.OnHold;
            bool isClosed = req.Status == GSynchExt.FTRStatus.Closed;
            e.Cache.AllowDelete = isOnHold;

            this.MatlRequestDet.Cache.AllowInsert = isOnHold;
            this.MatlRequestDet.Cache.AllowUpdate = isOnHold;
            this.MatlRequestDet.Cache.AllowDelete = isOnHold;


        }
        protected virtual void _(Events.FieldVerifying<MTRequestDetails.requestedQty> e)
        {
            MTRequestDetails row = e.Row as MTRequestDetails;
            if (row == null) return;

            var reqQty = row.RevisedQty - row.ActualQty;

            var totalRec =
                PXSelectJoinGroupBy<INTran,
                        InnerJoin<INRegister,
                        On<INTran.docType, Equal<INRegister.docType>,
                        And<INTran.refNbr, Equal<INRegister.refNbr>>>>,
                        Where<INRegister.status, Equal<INDocStatus.released>,
                        And<INTran.projectID, Equal<Required<INTran.projectID>>,
                        And<INTran.tranType, Equal<INTranType.issue>,
                        And<INTranGSExt.usrcreatedByMTR, Equal<True>,
                        And<INTran.taskID, Equal<Required<INTran.taskID>>,
                        And<INTran.costCodeID, Equal<Required<INTran.costCodeID>>>>>>>>,
                        Aggregate<GroupBy<INTran.projectID,
                        GroupBy<INTran.inventoryID,
                            Sum<INTran.qty>>>>>.Select(this, row.ProjectID, row.InventoryID, row.CostCode);

            RequestedProjectMaterials reqProjectMaterials = RequestedProjectMaterials.MK.Find(this, row.ProjectID, row.TaskID, row.CostCode, row.InventoryID);


            INTran tran = (INTran)totalRec;
            row.RequestedQty = row.RevisedQty - (row.ActualQty - tran?.Qty) - reqProjectMaterials?.RequestedQty;
        }
        protected virtual void _(Events.FieldDefaulting<MaterialTransferRequest.reqBy> e)
        {
            if (e.Row == null) return;

            var userID = Accessinfo.UserID;

            MaterialTransferRequest req = (MaterialTransferRequest)e.Row;

            EPEmployee emp = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.
                                Select(this, userID);
            e.Cache.SetValueExt<MaterialTransferRequest.reqBy>(e.Row, emp?.BAccountID);
        }

        /*
         * TO DO
        protected virtual void _(Events.FieldVerifying<MaterialTransferRequest, MaterialTransferRequest.projectID> e)
        {
            MaterialTransferRequest row = e.Row as MaterialTransferRequest;
            if (row == null) return;

            PMProject proj = PMProject.PK.Find(this, row.ProjectID);
            if (proj == null) return;

            if(proj.BudgetFinalized != true)
            {
                throw new PXException(GSynchExt.Messages.BudgetNotLocked);
            }

        }
        */
        protected virtual void _(Events.RowUpdating<MTRequestDetails> e)
        {
            MTRequestDetails row = e.Row;
            if (row == null) return;

            var budget = PMCostBudget.PK.Find(this,
                row.ProjectID, row.TaskID, row.AccountGroupID, row.CostCode, row.InventoryID);

            RequestedProjectMaterials reqDetails = RequestedProjectMaterials.PK.Find(this,
                row.ProjectID, row.TaskID, row.AccountGroupID, row.CostCode, row.InventoryID);

            var ReqPMGraph = PXGraph.CreateInstance<RequestedProjectMaterialsEntry>();

            if (reqDetails == null) return;
            /*ReqPMGraph.ReqProjectMaterials.Current = reqDetails;
            var remainingQty = budget.RevisedQty - budget.ActualQty - row.TransferQty;
            ReqPMGraph.ReqProjectMaterials.Current.RequestedQty = remainingQty;
            ReqPMGraph.ReqProjectMaterials.Update(ReqPMGraph.ReqProjectMaterials.Current);
            ReqPMGraph.Actions.PressSave();*/
        }
        #endregion

        #region Action
        public PXAction<MaterialTransferRequest> LoadBudget;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Load BOM")]
        protected virtual IEnumerable loadBudget(PXAdapter adapter)
        {

            MaterialTransferRequest row = MatlRequest.Current;
            PXCache cache = MatlRequest.Cache;
            PXGraph graph = cache.Graph;

            if (row == null) return adapter.Get();
            bool hasLines = false;
            decimal balRequestQty = 0;

            HashSet<BudgetKeyTuple> existing = GetExistingCostBudgets();

            PXLongOperation.StartOperation(this, delegate ()
            {

                var validBudgetLines = PXSelectJoin<PMCostBudget, 
                                 InnerJoin<PMTask, On<PMTask.projectID, Equal<PMCostBudget.projectID>, 
                                 And<PMTask.taskID, Equal<PMCostBudget.projectTaskID>, 
                                 And<PMTask.status, NotEqual<ProjectTaskStatus.canceled>,
                                 And<PMTask.status, NotEqual<ProjectTaskStatus.completed>>>>>,
                                 LeftJoin<InventoryItem,
                                 On<InventoryItem.inventoryID, Equal<PMCostBudget.inventoryID>>>>,
                                 Where<InventoryItem.stkItem, Equal<True>,
                                 And<PMCostBudget.projectID, Equal<Required<PMCostBudget.projectID>>,
                                 And<PMCostBudget.revisedQty, Greater<decimal0>,
                                 And<PMCostBudget.revisedQty, Greater<PMCostBudget.actualQty>>>>>>.Select(this, row.ProjectID);


                foreach (PMCostBudget item in validBudgetLines)
                {

                    PMCostBudget budgetLine = (PMCostBudget)item;
                    /// Calculate the Open quantities (Requested Quantities)
                    RequestedProjectMaterials line = RequestedProjectMaterials.PK.Find(this, item.ProjectID, item.TaskID,
                                                                        item.AccountGroupID,
                                                                        item.CostCodeID, item.InventoryID);
                    balRequestQty = 0;
                    ReCalculateLineQuantities(budgetLine, INTranType.Issue, out balRequestQty);


                    if (line != null)
                    {
                        balRequestQty -= (decimal)line.RequestedQty;

                    }
                    ///Insert BoM items
                    if (!existing.Contains(BudgetKeyTuple.Create(item)) && balRequestQty > 0)
                    {
                        MTRequestDetails matDetLine = new MTRequestDetails();
                        matDetLine.InventoryID = budgetLine.InventoryID;
                        matDetLine.UoM = budgetLine.UOM;
                        matDetLine.CostCode = budgetLine.CostCodeID;
                        matDetLine.ProjectID = budgetLine.ProjectID;
                        matDetLine.TaskID = budgetLine.TaskID;
                        matDetLine.AccountGroupID = budgetLine.AccountGroupID;
                        matDetLine.UoM = budgetLine.UOM;
                        matDetLine.ActualQty = budgetLine.ActualQty;
                        matDetLine.RevisedQty = budgetLine.RevisedQty;
                        matDetLine.RequestedQty = balRequestQty;
                        matDetLine.IssueQty = decimal.Zero;
                        matDetLine.TransferQty = decimal.Zero;


                        this.MatlRequestDet.Insert(matDetLine);
                        hasLines = true;
                    }

                }
                if (!hasLines)
                {
                    throw new PXSetPropertyException(Messages.WarnNoBalanceQty, PXErrorLevel.RowWarning);

                }
                this.Actions.PressSave();
            });
            return adapter.Get();
        }


        public PXAction<MaterialTransferRequest> LoadBudget2;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Load BOM2")]
        protected virtual IEnumerable loadBudget2(PXAdapter adapter)
        {
            MaterialTransferRequest row = MatlRequest.Current;
            PXCache cache = MatlRequest.Cache;
            PXGraph graph = cache.Graph;

            if (row == null) return adapter.Get();
            bool hasLines = false;
            decimal balRequestQty = 0;

            HashSet<BudgetKeyTuple> existing = GetExistingCostBudgets();

            PXLongOperation.StartOperation(this, delegate ()
            {
                var validBudgetLines = PXSelectJoin<PMCostBudget, LeftJoin<InventoryItem,
                                    On<InventoryItem.inventoryID, Equal<PMCostBudget.inventoryID>>>,
                                    Where<InventoryItem.stkItem, Equal<True>,
                                    And<PMCostBudget.projectID, Equal<Required<PMCostBudget.projectID>>,
                                    And<PMCostBudget.revisedQty, Greater<decimal0>,
                                    And<PMCostBudget.revisedQty, Greater<PMCostBudget.actualQty>>>>>>.Select(this, row.ProjectID);

                foreach (PMCostBudget item in validBudgetLines)
                {

                    PMCostBudget budgetLine = (PMCostBudget)item;
                    /// Calculate the Open quantities (Requested Quantities)
                    RequestedProjectMaterials line = RequestedProjectMaterials.PK.Find(this, item.ProjectID, item.TaskID,
                                                                        item.AccountGroupID,
                                                                        item.CostCodeID, item.InventoryID);
                    balRequestQty = 0;
                    ReCalculateLineQuantities(budgetLine, INTranType.Issue, out balRequestQty);


                    if (line != null)
                    {
                        balRequestQty -= (decimal)line.RequestedQty;

                    }
                    ///Insert BoM items
                    if (!existing.Contains(BudgetKeyTuple.Create(item)) && balRequestQty > 0)
                    {
                        MTRequestDetails matDetLine = new MTRequestDetails();
                        matDetLine.InventoryID = budgetLine.InventoryID;
                        matDetLine.UoM = budgetLine.UOM;
                        matDetLine.CostCode = budgetLine.CostCodeID;
                        matDetLine.ProjectID = budgetLine.ProjectID;
                        matDetLine.TaskID = budgetLine.TaskID;
                        matDetLine.AccountGroupID = budgetLine.AccountGroupID;
                        matDetLine.UoM = budgetLine.UOM;
                        matDetLine.ActualQty = budgetLine.ActualQty;
                        matDetLine.RevisedQty = budgetLine.RevisedQty;
                        matDetLine.RequestedQty = balRequestQty;
                        matDetLine.IssueQty = decimal.Zero;
                        matDetLine.TransferQty = decimal.Zero;


                        this.MatlRequestDet.Insert(matDetLine);
                        hasLines = true;
                    }

                }
                if (!hasLines)
                {
                    throw new PXSetPropertyException(Messages.WarnNoBalanceQty, PXErrorLevel.RowWarning);

                }
                this.Actions.PressSave();
            });
            return adapter.Get();
        }

        public PXAction<MaterialTransferRequest> CreateTransfer;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Create Transfer", Enabled = true)]
        protected virtual IEnumerable createTransfer(PXAdapter adapter)
        {

            if (MatlRequest.Current == null)
                return adapter.Get();

            this.Save.Press();

            CopyDialogInfo info = CopyDialog.Current;

            if (CopyDialog.View.Answer == WebDialogResult.None)
            {
                CopyDialog.Cache.Clear();
                CopyDialogInfo filterdata = CopyDialog.Cache.Insert() as CopyDialogInfo;
            }
            if (CopyDialog.AskExt() != WebDialogResult.OK  /* || string.IsNullOrEmpty(CopyDialog.Current.NewRevisionID)*/)
            {
                return adapter.Get();
            }
            MaterialTransferRequest request = (MaterialTransferRequest)this.MatlRequest.Current;
            {
                PXLongOperation.StartOperation(this, () => PXGraph.CreateInstance<INTransferEntry>().GetExtension<INTransferEntryGSExt>().CreateTransferFromMTR(request, info, redirect: true));
            }

            return adapter.Get();
        }

        public PXAction<MaterialTransferRequest> CreateIssue;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Create Issue", Enabled = true)]
        protected virtual IEnumerable createIssue(PXAdapter adapter)
        {
            var row = MatlRequest.Current;
            if (row == null) return adapter.Get();
            ///Validations
            if (row.TransferQty == 0 || row.TransferQty - row.IssueQty <= 0)
            {
                if (MatlRequest.Ask(Messages.OktoIssue, MessageButtons.YesNo) != WebDialogResult.Yes) return adapter.Get();
            }

            CopyDialogInfo info = CopyDialog2.Current;

            if (CopyDialog2.View.Answer == WebDialogResult.None)
            {
                CopyDialog.Cache.Clear();
                CopyDialogInfo filterdata = CopyDialog.Cache.Insert() as CopyDialogInfo;
            }
            if (CopyDialog2.AskExt() != WebDialogResult.OK  /* || string.IsNullOrEmpty(CopyDialog.Current.NewRevisionID)*/)
            {
                return adapter.Get();
            }
            MaterialTransferRequest request = (MaterialTransferRequest)this.MatlRequest.Current;
            {
                PXLongOperation.StartOperation(this, () => PXGraph.CreateInstance<INIssueEntry>().GetExtension<INIssueEntryGSExt>().CreateIssueFromMTR(request, info, redirect: true));
            }

            return adapter.Get();
        }


        public PXAction<MaterialTransferRequest> ViewLineDetails;
        [PXButton(CommitChanges = true), PXUIField(DisplayName = "Line Details",
           Enabled = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable viewLineDetails(PXAdapter adapter)
        {
            MTRequestDetails currentLine = MatlRequestDet.Current;
            if (currentLine != null)
            {
                MatlRequestLineDet.AskExt();
            }
            return adapter.Get();
        }


        #region WorkFlow Actions

        public PXAction<MaterialTransferRequest> RemoveHold;
        [PXButton(), PXUIField(DisplayName = "Remove Hold",
        MapEnableRights = PXCacheRights.Select,
        MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable removeHold(PXAdapter adapter)
        {
            /// Update the interim table at the time of Release
            RequestedProjectMaterialsEntry ReqPMGraph = CreateInstance<RequestedProjectMaterialsEntry>();
            ProjectEntry ProjEntry = CreateInstance<ProjectEntry>();

            var details = this.MatlRequestDet.Select();
            try
            {
                foreach (MTRequestDetails requestlines in details)
                {
                    var interimMatRecord = RequestedProjectMaterials.PK.Find(this, requestlines.ProjectID, requestlines.TaskID, requestlines.AccountGroupID,
                                                       requestlines.CostCode, requestlines.InventoryID);
                    var budget = PMCostBudget.PK.Find(ProjEntry, requestlines.ProjectID, requestlines.TaskID, requestlines.AccountGroupID,
                                       requestlines.CostCode, requestlines.InventoryID);

                    if (interimMatRecord == null) /// No matching records in the intermediate table
                    {
                        RequestedProjectMaterials rec = new RequestedProjectMaterials();
                        rec.ProjectID = budget.ProjectID;
                        rec.TaskID = budget.TaskID;
                        rec.AccountGroupID = budget.AccountGroupID;
                        rec.CostCode = budget.CostCodeID;
                        rec.InventoryID = budget.InventoryID;
                        rec.RequestedQty = requestlines.RequestedQty ?? 0;
                        ReqPMGraph.ReqProjectMaterials.Insert(rec);
                    }
                    else /// Matching record exists in the intermediate table
                    {
                        RequestedProjectMaterials rec = interimMatRecord;
                        // Recalculate if Budget Actual Qty > 0
                        if (budget.ActualQty > 0)
                        {
                            decimal balRequestQty = 0;
                            ReCalculateLineQuantities(budget, INTranType.Issue, out balRequestQty);

                            //Rec.RequestedQty < = (BalEquestQty - Requested Qty)
                            if (requestlines.RequestedQty > (balRequestQty - rec.RequestedQty))
                            {
                                throw new PXException(Messages.ReleaseError, (rec.RequestedQty - (budget.RevisedQty - budget.ActualQty)), requestlines.TaskID, requestlines.CostCode, requestlines.InventoryID);
                            }
                            else
                            {
                                rec.RequestedQty += requestlines.RequestedQty ?? 0;
                            }
                        }
                        ReqProjectMaterials.Update(rec);
                    }
                }
                ReqPMGraph.Actions.PressSave();
            }
            catch (Exception e)
            {
                throw new PXException(e.Message);
            }
            return adapter.Get();
        }

        public PXAction<MaterialTransferRequest> Hold2;
        [PXButton(), PXUIField(DisplayName = "Hold",
        MapEnableRights = PXCacheRights.Select,
        MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable hold2(PXAdapter adapter)
        {
            ///Validation
            MaterialTransferRequest row = MatlRequest.Current;
            if (row != null)
            {
                if (row.TransferQty > decimal.Zero || row.IssueQty > decimal.Zero)
                {
                    throw new PXException(Messages.TransfersOrIssueExist, "set to Hold");
                }
            }
            /// Update the interim table at the time of changing to onhold. The requested quantities should be deducted.
            foreach (MTRequestDetails line in MatlRequestDet.Select())
            {
                DeductInterimTable(line);
            }

            return adapter.Get();
        }

        public PXAction<MaterialTransferRequest> Cancel2;
        [PXButton(), PXUIField(DisplayName = "Cancel",
        MapEnableRights = PXCacheRights.Select,
        MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable cancel2(PXAdapter adapter)
        {
            MaterialTransferRequest row = MatlRequest.Current;
            if (row != null)
            {
                if (row.TransferQty > decimal.Zero || row.IssueQty > decimal.Zero)
                {
                    throw new PXException(Messages.TransfersOrIssueExist, "Cancel");
                }
            }

            CancelMTRequest();
            return adapter.Get();
        }

        public PXAction<MaterialTransferRequest> Close;
        [PXButton(), PXUIField(DisplayName = "Close",
        MapEnableRights = PXCacheRights.Select,
        MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable close(PXAdapter adapter)
        {
            MaterialTransferRequest row = MatlRequest.Current;
            if (row != null)
            {
                if (row.IssueQty < row.TransferQty && row.TransferQty > decimal.Zero)
                {
                    throw new PXException(Messages.TransferredNotIssued, "Close");
                }
                if (MatlRequest.Ask(Messages.OktoClose, MessageButtons.YesNo) != WebDialogResult.Yes) return adapter.Get();
                try
                {
                    /// Update the request quantity and deductthe differenc from interim table
                    foreach (MTRequestDetails line in MatlRequestDet.Select())
                    {
                        MTRequestDetails matLine = line;
                        matLine.RequestedQty = line.IssueQty;
                        MatlRequestDet.Update(matLine);
                        this.Actions.PressSave();
                        MTRequestDetails difLine = line;
                        difLine.RequestedQty = line.RequestedQty - line.IssueQty;
                        DeductInterimTable(difLine);
                    }
                }
                catch (Exception e)
                {
                    throw new PXException(e.Message);
                }
            }
            return adapter.Get();
        }

        public PXAction<MaterialTransferRequest> Reverse;
        [PXButton(), PXUIField(DisplayName = "Reverse",
        MapEnableRights = PXCacheRights.Select,
        MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable reverse(PXAdapter adapter)
        {
            return adapter.Get();
        }

        #endregion

        #endregion

        #region Methods
        protected virtual HashSet<BudgetKeyTuple> GetExistingCostBudgets2()
        {
            HashSet<BudgetKeyTuple> existing = new HashSet<BudgetKeyTuple>();
            foreach (RequestedProjectMaterials line in ReqProjectMaterials.Select())
            {

                var budget = PMCostBudget.PK.Find(this, line.ProjectID, line.TaskID, line.AccountGroupID, line.CostCode, line.InventoryID);
                if (budget != null)
                {
                    existing.Add(BudgetKeyTuple.Create(budget));

                }
            }

            return existing;
        }
        protected virtual HashSet<BudgetKeyTuple> GetExistingCostBudgets()
        {
            HashSet<BudgetKeyTuple> existing = new HashSet<BudgetKeyTuple>();
            foreach (MTRequestDetails line in MatlRequestDet.Select())
            {

                var budget = PMCostBudget.PK.Find(this, line.ProjectID, line.TaskID, line.AccountGroupID, line.CostCode, line.InventoryID);
                existing.Add(BudgetKeyTuple.Create(budget));
            }

            return existing;
        }
        public virtual void CancelMTRequest()
        {

            foreach (MTRequestDetails line in MatlRequestDet.Select())
            {
                DeductInterimTable(line);
            }
            
        }

        public virtual MaterialTransferRequest CreateMTRequest(PMProject project)
        {
            MaterialTransferRequest req = this.MatlRequest.Insert();
            if (project == null) return req;
            req.ProjectID = project.ContractID;

            req = this.MatlRequest.Insert(req);
            return req;
        }
        public virtual MaterialTransferRequest CreateMTRequestFromProject(PMProject project, bool redirect = false)
        {

            CreateMTRequest(project);

            if (this.MatlRequest.Cache.IsDirty)
            {
                if (redirect)
                    throw new PXRedirectRequiredException(this, "");
                else
                    return this.MatlRequest.Current;
            }

            throw new PXException("");
        }

        protected virtual void DeductInterimTable(MTRequestDetails row)
        {
            RequestedProjectMaterials existingRows = RequestedProjectMaterials.PK.Find(this,
                                    row.ProjectID, row.TaskID, row.AccountGroupID, row.CostCode, row.InventoryID);
            if (existingRows == null) return;
            PMCostBudget budgetRow = PMCostBudget.PK.Find(this,
                                    row.ProjectID, row.TaskID, row.AccountGroupID, row.CostCode, row.InventoryID);
            if (budgetRow == null) return;

            var ReqPMGraph = PXGraph.CreateInstance<RequestedProjectMaterialsEntry>();

            ReqPMGraph.ReqProjectMaterials.Current = existingRows;
            ReqPMGraph.ReqProjectMaterials.Current.RequestedQty -= row.RequestedQty ?? 0;
            ReqPMGraph.ReqProjectMaterials.Update(ReqPMGraph.ReqProjectMaterials.Current);
            ReqPMGraph.Actions.PressSave();
        }
        protected virtual void IncreaseInterimTable(MTRequestDetails row)
        {
            RequestedProjectMaterials existingRows = RequestedProjectMaterials.PK.Find(this,
                        row.ProjectID, row.TaskID, row.AccountGroupID, row.CostCode, row.InventoryID);
            if (existingRows == null) return;
            PMCostBudget budgetRow = PMCostBudget.PK.Find(this,
                                    row.ProjectID, row.TaskID, row.AccountGroupID, row.CostCode, row.InventoryID);
            if (budgetRow == null) return;

            var ReqPMGraph = PXGraph.CreateInstance<RequestedProjectMaterialsEntry>();

            ReqPMGraph.ReqProjectMaterials.Current = existingRows;
            ReqPMGraph.ReqProjectMaterials.Current.RequestedQty += row.RequestedQty ?? 0;
            ReqPMGraph.ReqProjectMaterials.Update(ReqPMGraph.ReqProjectMaterials.Current);
            ReqPMGraph.Actions.PressSave();
        }
        public void ReCalculateQuantities(MaterialTransferRequestEntry graph, MaterialTransferRequest mtr, string docType)
        {
            foreach (MTRequestDetails lines in graph.MatlRequestDet.Select().Where(x => x.Record.ReqNbr == mtr.ReqNbr))
            {
                INTran totalRec = PXSelectGroupBy<INTran,
                                   Where<INTranGSExt.usrcreatedByMTR, Equal<True>,
                                   And<INTranGSExt.usrMTRRef, Equal<Required<INTranGSExt.usrMTRRef>>,
                                   And<INTran.inventoryID, Equal<Required<INTran.inventoryID>>,
                                   And<INTran.projectID, Equal<Required<INTran.projectID>>,
                                   And<INTran.taskID, Equal<Required<INTran.taskID>>,
                                   And<INTran.costCodeID, Equal<Required<INTran.costCodeID>>,
                                   And<INTran.docType, Equal<Required<INTran.docType>>>>>>>>>,
                                   Aggregate<GroupBy<INTran.projectID, GroupBy<INTran.inventoryID, Sum<INTran.qty>>>>>.Select(graph, mtr.ReqNbr, lines.InventoryID, lines.ProjectID, lines.TaskID, lines.CostCode, docType).FirstOrDefault();

               // if (totalRec == null) continue;

                if (totalRec != null && docType == INDocType.Issue)
                {
                    lines.IssueQty = totalRec.Qty;
                }

                INTran totalRec2 = PXSelectGroupBy<INTran,
                                  Where<INTran.inventoryID, Equal<Required<INTran.inventoryID>>,
                                  And<INTran.toProjectID, Equal<Required<INTran.toProjectID>>,
                                  And<INTran.toTaskID, Equal<Required<INTran.toTaskID>>,
                                  And<INTran.toCostCodeID, Equal<Required<INTran.toCostCodeID>>,
                                  And<INTran.docType, Equal<Required<INTran.docType>>,
                                  And<INTranGSExt.usrcreatedByMTR, Equal<True>,
                                  And<INTranGSExt.usrMTRRef, Equal<Required<INTranGSExt.usrMTRRef>>>>>>>>>,
                                  Aggregate<GroupBy<INTran.projectID, GroupBy<INTran.inventoryID, Sum<INTran.qty>>>>>.Select(graph, lines.InventoryID, lines.ProjectID, lines.TaskID, lines.CostCode, docType, mtr.ReqNbr).FirstOrDefault();

                //    if (totalRec2 == null) continue;

                if (totalRec2 != null && docType == INDocType.Transfer)
                {
                    lines.TransferQty = totalRec2.Qty;
                }

                graph.MatlRequestDet.Update(lines);
            }

            graph.Actions.PressSave();
        }
        public void ReCalculateLineQuantities(PMCostBudget row, string docType, out decimal balRequestQty)
        {
            balRequestQty = 0;

            if (row == null) return;
            INTran totalRec =
            PXSelectJoinGroupBy<INTran,
                    InnerJoin<INRegister,
                    On<INTran.docType, Equal<INRegister.docType>,
                    And<INTran.refNbr, Equal<INRegister.refNbr>>>>,
                    Where<INRegister.status, Equal<INDocStatus.released>,
                    And<INTran.tranType, Equal<INTranType.issue>,
                    And<INTranGSExt.usrcreatedByMTR, Equal<True>,
                    And<INTran.projectID, Equal<Required<INTran.projectID>>,
                    And<INTran.inventoryID, Equal<Required<INTran.inventoryID>>,
                    And<INTran.taskID, Equal<Required<INTran.taskID>>,
                    And<INTran.costCodeID, Equal<Required<INTran.costCodeID>>>>>>>>>,
                    Aggregate<GroupBy<INTran.projectID,
                    GroupBy<INTran.inventoryID,
                        Sum<INTran.qty>>>>>.Select(this, row.ProjectID, row.InventoryID, row.TaskID, row.CostCodeID);

            // RequestedProjectMaterials reqProjectMaterials = RequestedProjectMaterials.MK.Find(this, lines.ProjectID, lines.TaskID, lines.CostCode, lines.InventoryID);

            if (totalRec != null)
            {
                INTran tran = (INTran)totalRec;
                balRequestQty = (decimal)(row.RevisedQty - (row.ActualQty - tran?.Qty));

            }
            else
            {
                balRequestQty = (decimal)(row.RevisedQty - row.ActualQty);
            }
        }
        #endregion
    }
    #region Dialogs
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [PXHidden]
    public class CopyDialogInfo : IBqlTable
    {

        #region ReasonCode
        public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
        protected String _ReasonCode;
        [PXDBString(PX.Objects.CS.ReasonCode.reasonCodeID.Length, IsUnicode = true)]
        [PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.issue>>>))]
        [PXUIField(DisplayName = "Reason Code")]
        [PXDefault()]
        public virtual String ReasonCode
        {
            get;
            set;
        }
        #endregion

        #region ToLocationID
        public abstract class toLocationID : PX.Data.BQL.BqlInt.Field<toLocationID> { }
        [PXSelector(typeof(Search<INLocation.locationID, Where<INLocation.siteID, Equal<Current<MaterialTransferRequest.toSiteID>>>>), SubstituteKey = typeof(INLocation.locationCD))]
        //     [PXRestrictor(typeof(Where<INLocation.siteID, Equal<Current<MaterialTransferRequest.toSiteID>>>), "")]
        [PXUIField(DisplayName = "To Location ID")]
        public virtual Int32? ToLocationID
        {
            get;
            set;
        }
        #endregion

        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
        [PXSelector(typeof(Search<INLocation.locationID, Where<INLocation.siteID, Equal<Current<MaterialTransferRequest.fromSiteID>>>>), SubstituteKey = typeof(INLocation.locationCD))]
        //   [PXRestrictor(typeof(Where<INLocation.siteID, Equal<Current<MaterialTransferRequest.fromSiteID>>>), "")]

        [PXUIField(DisplayName = "From Location ID")]
        public virtual Int32? LocationID
        {
            get;
            set;
        }
        #endregion

        #region ToCostLayerType
        public abstract class toCostLayerType : PX.Data.BQL.BqlString.Field<toCostLayerType> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(PX.Objects.IN.CostLayerType.Project)]
        [PXUIField(DisplayName = "To Cost Layer Type", FieldClass = FeaturesSet.inventory.CostLayerType)]
        [CostLayerType.List]
        public virtual string ToCostLayerType
        {
            get;
            set;
        }
        #endregion

        #region CostLayerType
        public abstract class costLayerType : PX.Data.BQL.BqlString.Field<costLayerType> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(PX.Objects.IN.CostLayerType.Normal)]
        [PXUIField(DisplayName = "From Cost Layer Type", FieldClass = FeaturesSet.inventory.CostLayerType)]
        [CostLayerType.List]
        public virtual string CostLayerType
        {
            get;
            set;
        }
        #endregion

        #region CostLayerTypeForIssue
        public abstract class costLayerTypeForIssue : PX.Data.BQL.BqlString.Field<costLayerTypeForIssue> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(PX.Objects.IN.CostLayerType.Project)]
        [PXUIField(DisplayName = "Cost Layer Type", FieldClass = FeaturesSet.inventory.CostLayerType)]
        [CostLayerType.List]
        public virtual string CostLayerTypeForIssue
        {
            get;
            set;
        }
        #endregion

    }
    #endregion

}
