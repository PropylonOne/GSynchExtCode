using System;
using System.Collections;
using System.Runtime.CompilerServices;
using GSynchExt;
using PX.Api;
using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.EP;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.RQ;
using static GSynchExt.SolarSite;
using Events = PX.Data.Events;

namespace GSynchExt
{
    public class DistrictsEntry : PXGraph<DistrictsEntry, Provinces>
    {
        public PXFilter<Provinces> MasterView;

        public PXSelect<Districts, Where<Districts.stateID, Equal<Current<Provinces.stateID>>>> DetailsView;
        public PXSelect<Cluster, Where<Cluster.stateID, Equal<Current<Provinces.stateID>>>> ClusterView;
        public PXSelect<Phase, Where<Phase.stateID, Equal<Current<Provinces.stateID>>>> PhaseVieww;

        public PXSelect<CEBLocations, Where<CEBLocations.stateID,
                Equal<Current<Provinces.stateID>>>> LocCEBView;

        public PXSelect<LECOLocations, Where<LECOLocations.stateID,
                Equal<Current<Provinces.stateID>>>> LocLECOView;

        public PXSelect<CEBLocations, Where<CEBLocations.stateID,
               Equal<Current<Provinces.stateID>>>> LocView;


      /*
       protected virtual void _(Events.RowSelected<CEBLocations> e)
        {
            BqlCommand cmd = BqlCommand.CreateInstance(typeof(Select<CEBLocations>));
            String viewName = this.ViewNames[LocLECOView.View];
            PXView view2 = e.Cache.Graph.TypedViews.GetView(cmd, true);
            var view = e.Cache.GetName();
            if (view != null)
            {
                if(view2.ToString() == "LocLECOView")
                {

                }
            }
        }
      
        */
    }
}