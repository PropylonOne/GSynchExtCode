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
using StatusDefinition = PX.Objects.PJ.Submittals.PJ.DAC.PJSubmittal.status;
using System.Collections.Generic;
using PX.Objects.Common.Labels;
using PX.Objects;
using GSynchExt;
using PX.Objects.RQ;
using PX.Objects.PM;
using static GSynchExt.GSBOQMaint;
using PX.Objects.CS;
using PX.Objects.CA;
using static PX.Objects.RQ.RQRequestEntryGSExt;
using static PX.Objects.TX.CSTaxCalcType;
using PX.Data.Update;

namespace PX.Objects.IN { 
    public class INTransferEntryGSExt : PXGraphExtension<INTransferEntry>
    {


        public PXFilter<ProjectStockFilter> projectstockitemsfilter;

        [PXFilterable]
        [PXCopyPasteHiddenView]
        [PXImport]
        public PXSelect<ProjectStock,
                Where<ProjectStock.projectID, Equal<Current<ProjectStockFilter.contractID>>,
                    And<ProjectStock.totalAvailableQty, Greater<decimal0>, And<ProjectStock.siteID,
                        Equal<Current<INRegister.siteID>>,
                        And<ProjectStock.companyID, Equal<CurrentCompany>>>>>> ProjectStockItems;

        public IEnumerable projectstockitems()
        {
            List<object> parameters = new List<object>();
            var projectItemSelect = new PXSelect<ProjectStock,
                Where<ProjectStock.projectID, Equal<Current<ProjectStockFilter.contractID>>,
                    And<ProjectStock.totalAvailableQty, Greater<decimal0>, And<ProjectStock.siteID,
                        Equal<Current<INRegister.siteID>>,
                        And<ProjectStock.companyID, Equal<CurrentCompany>>>>>>(this.Base);

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


/*

        public PXSelect<ProjectStock> stockView;

        #region Add Items
        // Add Inv Item action button
        public PXAction<ServiceMaterialRequest> addInvBySite;
        [PXUIField(DisplayName = "Add Items", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable AddInvBySite(PXAdapter adapter)
        {
            if (stockView.AskExt() == WebDialogResult.OK)
            {
              return AddInv(adapter);
            }
            stockView.Cache.Clear();
            return adapter.Get();
        }
        #endregion

        public PXAction<ServiceMaterialRequest> addInv;
        [PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable AddInv(PXAdapter adapter)
        {
            foreach (ProjectStock line in stockView.Cache.Cached)
            {
                if (line.Selected == true)
                {                  
                    INTran newline = PXCache<INTran>.
                        CreateCopy(this.Base.transactions.Insert(new INTran()));

                    newline.InventoryID = line.InventoryID;
                    newline.LocationID = line.LocationID ;
                    newline.CostLayerType = CostLayerType.Project ;
                    newline.SiteID = line.SiteID;

                    this.Base.transactions.Insert(newline);
                }
            }
            this.Base.transactions.Cache.Clear();
            return adapter.Get();
        }*/
        public virtual INRegister CreateTransfer(MaterialTransferRequest mTRequest,CopyDialogInfo info)
        {
            INRegister reg = new INRegister();
            if (mTRequest == null) return reg;
            reg.SiteID = mTRequest.FromSiteID;
            reg.ToSiteID = mTRequest.ToSiteID;
            reg.ExtRefNbr = mTRequest.ReqNbr;
            this.Base.transfer.Insert(reg);

            var req = PXSelect<MTRequestDetails,
                       Where<MTRequestDetails.reqNbr,
                       Equal<Required<MTRequestDetails.reqNbr>>,
                       And<MTRequestDetails.requestedQty, Greater<decimal0>>>>.Select(this.Base, mTRequest.ReqNbr);
            foreach (var MTRequestetails in req)
            {
                MTRequestDetails lineRec = (MTRequestDetails)MTRequestetails;
                if(lineRec.RequestedQty > lineRec.TransferQty) //new lines created for items that are not fully transferred
                {
                    INTran tran = new INTran();
                    INTranGSExt tranExt = PXCache<INTran>.GetExtension<INTranGSExt>(tran);
                    tran.LocationID = info.LocationID;
                    tran.CostLayerType = info.CostLayerType;
                    tran.ToLocationID = info.ToLocationID;
                    tran.ToCostLayerType = info.ToCostLayerType;
                    tran.InventoryID = lineRec.InventoryID;
                    tran.ToProjectID = lineRec.ProjectID;
                    tran.ToTaskID = lineRec.TaskID;
                    tran.ToCostCodeID = lineRec.CostCode;
                    tran.Qty = lineRec.RequestedQty - lineRec.TransferQty;
                    tran.OrigQty = lineRec.RequestedQty;


                    tranExt.UsrcreatedByMTR = true;
                    tranExt.UsrMTRRef = mTRequest.ReqNbr;

                    this.Base.transactions.Insert(tran);
                }
            }
           
            return reg;
        }
        public virtual INTran CreateTransferFromMTR(MaterialTransferRequest mTRequest, CopyDialogInfo info, bool redirect = false)
        {

            CreateTransfer(mTRequest, info);

        //    var transferRequestEntry = PXGraph.CreateInstance<MaterialTransferRequestEntry>();

            if (this.Base.transfer.Cache.IsDirty)
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
        public virtual INRegister CreateSMTransfer(ServiceMaterialRequest mTRequest, CopyDialogInfo2 info)
        {
            INRegister reg = new INRegister();
            reg.SiteID = mTRequest.FromSiteID;
            reg.ToSiteID = mTRequest.ToSiteID;
            reg.SiteID = info.FromSiteID;
            reg.ExtRefNbr = mTRequest.ReqNbr;
            this.Base.transfer.Insert(reg);

            var req = PXSelect<ServiceMaterialRequestDetails,
                       Where<ServiceMaterialRequestDetails.reqNbr,
                       Equal<Required<ServiceMaterialRequestDetails.reqNbr>>,
                       And<ServiceMaterialRequestDetails.requestedQty, Greater<decimal0>>>>.Select(this.Base, mTRequest.ReqNbr);
            foreach (var MTRequestetails in req)
            {
                ServiceMaterialRequestDetails lineRec = (ServiceMaterialRequestDetails)MTRequestetails;

                INTran tran = new INTran();
                INTranGSExt tranExt = PXCache<INTran>.GetExtension<INTranGSExt>(tran);
                tran.LocationID = info.LocationID;
                tran.CostLayerType = info.CostLayerType;
                tran.ToLocationID = info.ToLocationID;
                tran.ToCostLayerType = info.ToCostLayerType;
                tran.SiteID = info.FromSiteID;
                tran.InventoryID = lineRec.InventoryID;
                tran.Qty = lineRec.RequestedQty;
                tran.OrigQty = lineRec.RequestedQty;


                tranExt.UsrCreatedBySMR = true;
                tranExt.UsrSMRRef = mTRequest.ReqNbr;

                this.Base.transactions.Insert(tran);
                // this.Base.transactions.Update(tran);
            }
            if (mTRequest == null) return reg;

            return reg;
        }

        public virtual INTran CreateTransferFromSMR(ServiceMaterialRequest mTRequest, CopyDialogInfo2 info, bool redirect = false)
        {

            CreateSMTransfer(mTRequest, info);

            //    var transferRequestEntry = PXGraph.CreateInstance<MaterialTransferRequestEntry>();

            if (this.Base.transfer.Cache.IsDirty)
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

        #region Event Handlers

        protected virtual void _(Events.RowSelected<INRegister> e)
        {
            INRegister row = e.Row;
            if (row == null) return;

            bool isReleased = row.Status == INDocStatus.Released;
            addProjectItems.SetEnabled(!isReleased);


        }
        protected virtual void _(Events.RowPersisted<INRegister> e)
        {
            INRegister row = e.Row;
            if (row == null) return;
            MaterialTransferRequest mtr = MaterialTransferRequest.UK.Find(this.Base, row.ExtRefNbr);
            ServiceMaterialRequest smr = ServiceMaterialRequest.UK.Find(this.Base, row.ExtRefNbr);
            bool FromMTR = (row.CreatedByScreenID == "GS301027" || mtr != null);
            try
            {
                if (e.TranStatus == PXTranStatus.Completed && FromMTR)
                {
                    /// Recalculate the issued quantities for the request everytime the issue header is saved.
                    var MTRGraph = PXGraph.CreateInstance<MaterialTransferRequestEntry>();
                    MTRGraph.MatlRequest.Current = mtr;
                    MTRGraph.ReCalculateQuantities(MTRGraph, mtr, INDocType.Transfer);
                }
            }
            catch (Exception ex)
            {
                throw new PXException(ex.Message);
            }
        }
        protected virtual void _(Events.RowSelected<INTran> e)
        {
            INTran row = e.Row;
            if (row == null) return;
            INTranGSExt rowExt = PXCache<INTran>.GetExtension<INTranGSExt>(row);
            if (rowExt.UsrMTRRef != null || rowExt.UsrcreatedByMTR == true)
            {
                var MTRGraph = PXGraph.CreateInstance<MaterialTransferRequestEntry>();

                MTRequestDetails transferDetail =
                    PXSelect<MTRequestDetails,
                    Where<MTRequestDetails.reqNbr,
                    Equal<Required<MTRequestDetails.reqNbr>>,
                    And<MTRequestDetails.inventoryID,
                    Equal<Required<MTRequestDetails.inventoryID>>>>>.Select(MTRGraph, rowExt.UsrMTRRef, row.InventoryID);

                if (transferDetail == null) return;
                MaterialTransferRequest request = MaterialTransferRequest.UK.Find(MTRGraph, transferDetail.ReqNbr);
                INRegister header = INRegister.PK.Find(this.Base, INDocType.Transfer, row.RefNbr);
                if (header == null) return;
                if (request.ToSiteID == row.ToSiteID && header.Status == INDocStatus.Released)
                {
                    MTRGraph.MatlRequestDet.Current = transferDetail;
                    MTRGraph.MatlRequestDet.Current.TransferQty = row.Qty;
                    MTRGraph.MatlRequestDet.Update(MTRGraph.MatlRequestDet.Current);
                    MTRGraph.Actions.PressSave();
                }
            }
            if (rowExt.UsrSMRRef != null || rowExt.UsrCreatedBySMR == true)
            {
                var SMRGraph = PXGraph.CreateInstance<ServiceMaterialRequestEntry>();

                ServiceMaterialRequestDetails transferDetail =
                    PXSelect<ServiceMaterialRequestDetails,
                    Where<ServiceMaterialRequestDetails.reqNbr,
                    Equal<Required<ServiceMaterialRequestDetails.reqNbr>>,
                    And<ServiceMaterialRequestDetails.inventoryID,
                    Equal<Required<ServiceMaterialRequestDetails.inventoryID>>>>>.Select(SMRGraph, rowExt.UsrSMRRef, row.InventoryID);

                if (transferDetail == null) return;
                ServiceMaterialRequest request = ServiceMaterialRequest.UK.Find(SMRGraph, transferDetail.ReqNbr);
                INRegister header = INRegister.PK.Find(this.Base, INDocType.Transfer, row.RefNbr);
                if (header == null) return;
                if (request.ToSiteID == row.ToSiteID && header.Status == INDocStatus.Released)
                {
                    SMRGraph.MatlRequestDet.Current = transferDetail;
                    SMRGraph.MatlRequestDet.Current.TransferQty = row.Qty;
                    SMRGraph.MatlRequestDet.Update(SMRGraph.MatlRequestDet.Current);
                    SMRGraph.Actions.PressSave();
                }
            }

        }
        #endregion


        public class CurrentCompany : PX.Data.BQL.BqlInt.Constant<CurrentCompany> { public CurrentCompany() : base(PX.Data.Update.PXInstanceHelper.CurrentCompany) { } }

    }
}