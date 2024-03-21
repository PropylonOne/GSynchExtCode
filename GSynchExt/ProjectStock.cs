using System;
using GSynchExt;
using PX.Data;
using PX.Data.Update;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects
{
    [Serializable]
    [PXCacheName("Project Stock")]
    public class ProjectStock : IBqlTable
    {
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion

        #region CompanyID
        [PXDBInt(IsKey =true)]
        [PXUIField(DisplayName = "Company ID", Enabled = false)]
        public virtual int? CompanyID { get; set; }
        public abstract class companyID : PX.Data.BQL.BqlInt.Field<companyID> { }
        #endregion


        #region QtySelected
        public abstract class qtySelected : PX.Data.BQL.BqlDecimal.Field<qtySelected> { }
        protected Decimal? _QtySelected;
        [PXQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. Selected")]
        public virtual Decimal? QtySelected
        {
            get
            {
                return this._QtySelected ?? 0m;
            }
            set
            {
                if (value != null && value != 0m)
                    this._Selected = true;
                this._QtySelected = value;
            }
        }
        #endregion

        #region InventoryID
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Inventory ID", Enabled = false)]
        [PXSelector(typeof(Search<InventoryItem.inventoryID>), SubstituteKey = (typeof(InventoryItem.inventoryCD)))]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion



        #region Uom
        //    [PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [INUnit(DisplayName = "UoM")]
        public virtual string UOM { get; set; }
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
        #endregion

        #region ProjectID
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Project ID", Enabled = false)]
        [PXSelector(typeof(Search<PMProject.contractID>), typeof(PMProject.contractCD), typeof(PMProject.description), typeof(PMProject.status), SubstituteKey = (typeof(PMProject.contractCD)))]
        public virtual int? ProjectID { get; set; }
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
        #endregion

        #region TaskID
        [ProjectTask(typeof(ProjectStock.projectID), IsKey = true)]
        [PXUIField(DisplayName = "Task ID", Enabled = false)]
        public virtual int? TaskID { get; set; }
        public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
        #endregion


        #region Descr
        [PXDBString(256, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Descr", Enabled = false)]
        public virtual string Descr { get; set; }
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        #endregion

        #region SiteID
        [PXDBInt()]
        [PXUIField(DisplayName = "Warehouse", Enabled = false)]
        [PXSelector(typeof(Search<INSite.siteID>), SubstituteKey = (typeof(INSite.siteCD)))]
        public virtual int? SiteID { get; set; }
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        #endregion

        #region LocationID
        [PXDBInt()]
        [PXUIField(DisplayName = "Location ID", Enabled = false)]
        [PXSelector(typeof(Search<INLocation.locationID>), SubstituteKey = (typeof(INLocation.locationCD)))]
        public virtual int? LocationID { get; set; }
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
        #endregion

        #region CostCodeID
        [PXUIField(DisplayName = "Cost Code ID" ,Enabled = false)]
        [CostCode(null, null, null, DescriptionField = typeof(PMCostCode.description), IsKey = true)]
        public virtual int? CostCodeID { get; set; }
        public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
        #endregion

        #region LotSerialNbr
        [PXDBString(100, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Lot Serial Nbr", Enabled = false)]
        public virtual string LotSerialNbr { get; set; }
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        #endregion

        #region TotalTransferINQty
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "TotalTransferINQty" ,Enabled = false)]
        public virtual Decimal? TotalTransferINQty { get; set; }
        public abstract class totalTransferINQty : PX.Data.BQL.BqlDecimal.Field<totalTransferINQty> { }
        #endregion


        #region TotalTransferOUTQty
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "TotalTransferOUTQty", Enabled = false)]
        public virtual Decimal? TotalTransferOUTQty { get; set; }
        public abstract class totalTransferOUTQty : PX.Data.BQL.BqlDecimal.Field<totalTransferOUTQty> { }
        #endregion

        #region TotalIssueQty
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "TotalIssueQty", Enabled = false)]
        public virtual Decimal? TotalIssueQty { get; set; }
        public abstract class totalIssueQty : PX.Data.BQL.BqlDecimal.Field<totalIssueQty> { }
        #endregion

        #region TotalAvailableQty
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "TotalAvailableQty", Enabled = false)]
        public virtual Decimal? TotalAvailableQty { get; set; }
        public abstract class totalAvailableQty : PX.Data.BQL.BqlDecimal.Field<totalAvailableQty> { }
        #endregion

    }

    public partial class ProjectStockFilter : IBqlTable
    {
        #region ContractID
        [PXDBInt()]
        [PXUIField(DisplayName = "Project ID")]
        [PXSelector(typeof(Search<PMProject.contractID>), typeof(PMProject.contractCD), typeof(PMProject.description), typeof(PMProject.status), SubstituteKey = (typeof(PMProject.contractCD)))]
        public virtual int? ContractID { get; set; }
        public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
        #endregion

        #region FromWarehouse
        [PXDBInt()]
        [PXUIField(DisplayName = "From Warehouse")]
        [PXSelector(typeof(Search<INSite.siteID>), SubstituteKey = (typeof(INSite.siteCD)))]
        [PXDefault(typeof(Current<INRegister.siteID>))]
        public virtual int? FromWarehouse { get; set; }
        public abstract class fromWarehouse : PX.Data.BQL.BqlInt.Field<fromWarehouse> { }
        #endregion 




    }
}