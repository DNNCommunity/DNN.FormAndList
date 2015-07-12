using System;
using System.Diagnostics;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Modules.UserDefinedTable
{
    public partial class HelpPopup : PageBase
    {
        #region  Vom Web Form Designer generierter Code

        [DebuggerStepThrough]
        void InitializeComponent()
        {
        }

        void Page_Init(object sender, EventArgs e)
        {
            InitializeComponent();
        }

        #endregion

        void Page_Load(object sender, EventArgs e)
        {
            var content = Request.QueryString["resourcekey"];
            if (content.ToLowerInvariant() == "help_hiddencolumns" || content.ToLowerInvariant() == "help_tokens_6")
            {
                Title = Localization.GetString(content + "_Title", LocalResourceFile);
                lblContent.Text = Localization.GetString(content + "_Body", LocalResourceFile);
            }
        }
    }
}