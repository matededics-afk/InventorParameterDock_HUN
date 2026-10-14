using System;
using System.Runtime.InteropServices;
using Inventor;

namespace InventorParameterDock
{
    [Guid("B0F1397E-8B66-4D98-B01F-D0C4F6905173")]
    [ComVisible(true)]
    public sealed class AddInServer : ApplicationAddInServer
    {
        private Inventor.Application app;
        private Inventor.ApplicationEvents appEvents;
        private DockableWindow dock;
        private ParameterPanel panel;

        public void Activate(ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            app = addInSiteObject.Application;
            panel = new ParameterPanel(app);
            dock = app.UserInterfaceManager.DockableWindows.Add(
                "{B0F1397E-8B66-4D98-B01F-D0C4F6905173}",
                "MumurParameterDock", "Paraméterek");
            dock.AddChild(panel.Handle.ToInt64());
            if (!dock.IsCustomized)
                dock.SetDockingState(DockingStateEnum.kDockRight);
            dock.Visible = true;
            appEvents = app.ApplicationEvents;
            appEvents.OnActivateDocument += OnActivateDocument;
            appEvents.OnNewEditObject += OnNewEditObject;
            panel.RefreshParameters();
        }

        private void OnActivateDocument(Inventor.Document documentObject,
            EventTimingEnum beforeOrAfter, NameValueMap context,
            out HandlingCodeEnum handlingCode)
        {
            handlingCode = HandlingCodeEnum.kEventNotHandled;
            if (beforeOrAfter == EventTimingEnum.kAfter && panel != null)
                panel.RefreshParameters();
        }

        private void OnNewEditObject(object editObject, EventTimingEnum beforeOrAfter,
            NameValueMap context, out HandlingCodeEnum handlingCode)
        {
            handlingCode = HandlingCodeEnum.kEventNotHandled;
            if (beforeOrAfter == EventTimingEnum.kAfter && panel != null)
                panel.RefreshParameters();
        }

        public void Deactivate()
        {
            if (appEvents != null)
            {
                appEvents.OnActivateDocument -= OnActivateDocument;
                appEvents.OnNewEditObject -= OnNewEditObject;
            }
            if (dock != null)
            {
                dock.Clear();
                dock.Delete();
                dock = null;
            }
            if (panel != null) { panel.Dispose(); panel = null; }
            appEvents = null;
            app = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        public void ExecuteCommand(int commandID) { }
        public object Automation { get { return null; } }
    }
}
