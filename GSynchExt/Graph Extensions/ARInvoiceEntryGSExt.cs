using System;
using System.Linq;
using PX.Data;
using PX.Objects.AR;

namespace GSynchExt
{
    /// <summary>
    /// Customer Order Nbr is used to store the Solar Sales Revenue Gen ID (with the prefix based on the INV: SS, RR, SB)
    /// </summary>
    public class ARInvoiceEntryGSExt : PXGraphExtension<ARInvoiceEntry>
    {
        #region DAC Overrides
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(Enabled = false)]
        protected void ARInvoice_InvoiceNbr_CacheAttached(PXCache sender) { }

        #endregion
    }
}
