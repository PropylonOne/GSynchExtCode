﻿using System;
using System.Collections;
using PX.Data;
using PX.Data.Licensing;
using PX.Objects.CA;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.TM;
using static PX.Data.BQL.BqlPlaceholder;
using static PX.Objects.FA.FABookSettings.midMonthType;
using static PX.Objects.TX.CSTaxCalcType;

namespace GSynchExt
{
    public class ChangeOrderEntryGSExt : PXGraphExtension<ChangeOrderEntry>
    {
        protected virtual void _(Events.RowPersisting<PMChangeOrder> e)
        {
            PMChangeOrder row = e.Row as PMChangeOrder;
            if (row == null) return;
            if(row.CreatedByID != null)
            {
                Contact cRec = PXSelect<Contact, Where<Contact.userID, Equal<Required<Contact.userID>>>>.Select(this.Base, row.CreatedByID);
                EPCompanyTreeMember rec = PXSelect<EPCompanyTreeMember, Where<EPCompanyTreeMember.contactID, Equal<Required<EPCompanyTreeMember.contactID>>>>.Select(this.Base, cRec?.ContactID);
                row.WorkgroupID = rec?.WorkGroupID;
            }
        }
    }
}
