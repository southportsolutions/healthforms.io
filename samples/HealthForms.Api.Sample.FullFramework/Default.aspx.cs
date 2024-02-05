using System;
using System.Web;
using System.Web.UI;

namespace HealthForms.Api.Sample.FullFramework
{
    public partial class _Default : Page
    {
        

        protected void Page_Load(object sender, EventArgs e)
        {
            HttpContext.Current.Response.Redirect("GetCode.aspx", false);
        }
    }
}