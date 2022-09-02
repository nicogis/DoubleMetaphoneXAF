using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using DoubleMetaphoneXAF.Module.BusinessObjects.Database;

namespace DoubleMetaphoneXAF.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class VCSearchDoubleMetaphone : ViewController
    {
        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
        public VCSearchDoubleMetaphone()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
            TargetObjectType = typeof(Customers);
        }
        private FilterController standardFilterController;
        protected override void OnActivated()
        {
            base.OnActivated();
            standardFilterController = Frame.GetController<FilterController>();
            if (standardFilterController != null)
            {
                
                standardFilterController.CustomBuildCriteria += StandardFilterController_CustomBuildCriteria;
            }
        }

        private void StandardFilterController_CustomBuildCriteria(object sender, CustomBuildCriteriaEventArgs e)
        {
            if (string.IsNullOrEmpty(e.SearchText))
            {
                e.Handled = false;
                return;
            }
            
            e.Criteria = CriteriaOperator.Parse($"DoubleMetaphone('{e.SearchText}', [CompanyNameDoubleMetaphone]) or CharIndex('{e.SearchText}', [CompanyName]) > -1");
            e.Handled = true;    
        }

        
        protected override void OnDeactivated()
        {
            if (standardFilterController != null)
            {
                standardFilterController.CustomBuildCriteria -= StandardFilterController_CustomBuildCriteria;
            }
            base.OnDeactivated();
        }
    }
}
