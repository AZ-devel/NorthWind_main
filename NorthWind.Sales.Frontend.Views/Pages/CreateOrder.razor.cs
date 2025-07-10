using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NorthWind.Sales.Frontend.Views.ViewModels.CreateOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind.Sales.Frontend.Views.Pages
{
    public partial class CreateOrder
    {
        [Inject]
        CreateOrderViewModel ViewModel { get; set; }
        ErrorBoundary ErrorBoundaryRef;
        void Recover()
        {
            ErrorBoundaryRef?.Recover();
        }
    }

}
