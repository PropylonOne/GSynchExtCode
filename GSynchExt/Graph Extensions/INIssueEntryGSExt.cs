using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.PJ.Common.DAC;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.CR;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using PX.Objects.Common.Labels;
using PX.Objects;
using GSynchExt;
using PX.Objects.RQ;
using PX.Objects.PM;
using PX.Objects.CS;
using System.Security.Cryptography;

namespace PX.Objects.IN {
    public class INIssueEntryGSExt : PXGraphExtension<INIssueEntry>
    {


        public PXFilter<ProjectStockFilter> projectstockitemsfilter;

        [PXFilterable]
        [PXCopyPasteHiddenView]
        [PXImport]
        public PXSelect<ProjectStock,
                Where<ProjectStock.projectID, Equal<Current<ProjectStockFilter.contractID>>,
                    And<ProjectStock.totalAvailableQty, Greater<decimal0>>>> ProjectStockItems;

        public IEnumerable projectstockitems()
        {
            List<object> parameters = new List<object>();
            var projectItemSelect = new PXSelect<ProjectStock,
                Where<ProjectStock.projectID, Equal<Current<ProjectStockFilter.contractID>>,
                    And<ProjectStock.totalAvailableQty, Greater<decimal0>>>>(this.Base);

            PXDelegateResult delResult = new PXDelegateResult();
            delResult.Capacity = 202;
            delResult.IsResultFiltered = false;
            delResult.IsResultSorted = true;
            delResult.IsResultTruncated = false;

            var view = new PXView(this.Base, false, projectItemSelect.View.BqlSelect);
            var startRow = PXView.StartRow;
            int totalRows = 0;
            var resultset = view.Select(PXView.Currents, parameters.ToArray(), PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
            PXView.StartRow = 0;

            foreach (ProjectStock projectStockItem in resultset)
            {
                ProjectStock projectItem = projectStockItem;
                delResult.Add(projectItem);
            }

            return delResult;
        }


        public PXAction<INTran> addProjectItems;
        [PXUIField(DisplayName = "Add Project Stock Items", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton(DisplayOnMainToolbar = false)]
        public virtual IEnumerable AddProjectItems(PXAdapter adapter)
        {
            IEnumerable result = null;

            // if (this.Base.Document.Current.Hold == true && this.Base.Document.Current.ReqClassID != null)
            {
                if (ProjectStockItems.AskExt() == WebDialogResult.OK)
                {
                    result = AddSelectedProjecStockLines(adapter);
                }

                projectstockitemsfilter.Cache.Clear();
                ProjectStockItems.Cache.Clear();
                ProjectStockItems.ClearDialog();
                ProjectStockItems.View.Clear();
                ProjectStockItems.View.ClearDialog();
            }

            if (result != null)
            {
                return result;
            }

            return adapter.Get();
        }

        public PXAction<INTran> addSelectedProjecStockLines;
        [PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton(DisplayOnMainToolbar = false)]
        public virtual IEnumerable AddSelectedProjecStockLines(PXAdapter adapter)
        {
            //  if (this.Base.Document.Current.Hold == true)
            {
                this.Base.transactions.Cache.ForceExceptionHandling = true;

                foreach (ProjectStock line in ProjectStockItems.Cache.Cached)
                {
                    if (line.Selected == true)
                    {
                        INTran newline = new INTran();
                        newline.InventoryID = line.InventoryID;
                        newline.UOM = line.UOM;
                        newline.SiteID = line.SiteID;
                        newline.LotSerialNbr = line.LotSerialNbr;
                        newline.LocationID = line.LocationID;
                        newline.CostLayerType = CostLayerType.Project;
                        newline.ProjectID = line.ProjectID;
                        newline.TaskID = line.TaskID;
                        newline.CostCodeID = line.CostCodeID;
                        newline.Qty = line.QtySelected;
                        newline = this.Base.transactions.Insert(newline);

                        this.Base.transactions.Update(newline);
                    }
                }
            }

            return adapter.Get();
        }


        /// <summary>
        /// #UPGRADEIMPACTED
        /// </summary>
        [PXImport(typeof(INRegister))]
        [PXCopyPasteHiddenView]
        public
          PXSelect<INTranSplit,
          Where<
              INTranSplit.docType, Equal<INDocType.issue>,
              And<INTranSplit.refNbr, Equal<Current<INTran.refNbr>>,
              And<INTranSplit.lineNbr, Equal<Current<INTran.lineNbr>>>>>>
          splits;
        public virtual INRegister CreateIssue(MaterialTransferRequest mTRequest, CopyDialogInfo info)
        {
            INRegister reg = new INRegister();
            reg.ExtRefNbr = mTRequest.ReqNbr;
            reg.TranDesc = "Project Material Issue for " + mTRequest.ReqNbr;
            this.Base.issue.Insert(reg);

            var validLines = PXSelect<MTRequestDetails,
                    Where<MTRequestDetails.reqNbr,Equal<Required<MTRequestDetails.reqNbr>>,
                    And<MTRequestDetails.requestedQty, Greater<decimal0>,
                    And<MTRequestDetails.requestedQty, Greater<MTRequestDetails.issueQty>>>>>.Select(this.Base, mTRequest.ReqNbr);
            foreach (MTRequestDetails lineRec in validLines)
            {
                INTran tran = new INTran();
                INTranGSExt tranExt = PXCache<INTran>.GetExtension<INTranGSExt>(tran);
                tran.InventoryID = lineRec.InventoryID;
                tran.SiteID = mTRequest.ToSiteID;
                tran.LocationID = info.LocationID;
                tran.CostLayerType = info.CostLayerTypeForIssue;
                tran.ReasonCode = info.ReasonCode;
                tran.ProjectID = lineRec.ProjectID;
                tran.TaskID = lineRec.TaskID;
                tran.CostCodeID = lineRec.CostCode;
                tran.Qty = lineRec.RequestedQty-lineRec.IssueQty;
                tran.TranType = INTranType.Issue;

                tranExt.UsrcreatedByMTR = true;
                tranExt.UsrMTRRef = mTRequest.ReqNbr;
                this.Base.transactions.Insert(tran);
             }
            if (mTRequest == null) return reg;

            return reg;
        }

        public virtual INTran CreateIssueFromMTR(MaterialTransferRequest mTRequest, CopyDialogInfo info, bool redirect = false)
        {
            CreateIssue(mTRequest, info);
            if (this.Base.issue.Cache.IsDirty)
            {
                if (redirect)
                {
                    throw new PXRedirectRequiredException(this.Base, "");
                }
                else
                {
                    return this.Base.transactions.Current;
                }
            }
            throw new PXException("");
        }   


        #region Events
        protected virtual void _(Events.RowSelected<INRegister>e)
        {
            INRegister doc = (INRegister)e.Row;
            if (doc == null) return;

            bool notFromMTR = doc.CreatedByScreenID != "GS301027";
            PXUIFieldAttribute.SetEnabled<INRegister.extRefNbr>(e.Cache, doc, notFromMTR);
            PXUIFieldAttribute.SetEnabled<INRegister.tranDesc>(e.Cache, doc, notFromMTR);

            bool isReleased = doc.Status == INDocStatus.Released;
            addProjectItems.SetEnabled(!isReleased);
        }
        protected virtual void _(Events.RowSelected<INTran> e)
        {
            INTran doc = (INTran)e.Row;
            if (doc == null) return;

            bool notFromMTR = doc.CreatedByScreenID != "GS301027";

            PXUIFieldAttribute.SetEnabled<INTran.inventoryID>(e.Cache, doc, notFromMTR);
            PXUIFieldAttribute.SetEnabled<INTran.costLayerType>(e.Cache, doc, notFromMTR);
            PXUIFieldAttribute.SetEnabled<INTran.projectID>(e.Cache, doc, notFromMTR);
            PXUIFieldAttribute.SetEnabled<INTran.taskID>(e.Cache, doc, notFromMTR);
            PXUIFieldAttribute.SetEnabled<INTran.costCodeID>(e.Cache, doc, notFromMTR);
            PXUIFieldAttribute.SetEnabled<INTran.siteID>(e.Cache, doc, notFromMTR);
            PXUIFieldAttribute.SetEnabled<INTran.locationID>(e.Cache, doc, notFromMTR);
            PXUIFieldAttribute.SetEnabled<INTran.unitPrice>(e.Cache, doc, notFromMTR);
            PXUIFieldAttribute.SetEnabled<INTran.tranAmt>(e.Cache, doc, notFromMTR);


        }
        protected virtual void _(Events.RowPersisted<INRegister> e)
        {
            INRegister row = e.Row;
            if (row == null) return;
            MaterialTransferRequest mtr = MaterialTransferRequest.UK.Find(this.Base, row.ExtRefNbr);
            bool FromMTR = (row.CreatedByScreenID == "GS301027" || mtr != null);
            try
            {
                if (e.TranStatus == PXTranStatus.Completed && FromMTR)
                {
                    /// Recalculate the issued quantities for the request everytime the issue header is saved.
                    var MTRGraph = PXGraph.CreateInstance<MaterialTransferRequestEntry>();
                    MTRGraph.MatlRequest.Current = mtr;
                    MTRGraph.ReCalculateQuantities(MTRGraph, mtr, INDocType.Issue);
                }
            }
            catch(Exception ex)
            {
                throw new PXException(ex.Message);
            }
        }
         #endregion
    }
}
