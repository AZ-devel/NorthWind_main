using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind.RazorComponents.NavBars
{
    public partial class NavMenu
    {
        [Parameter]
        public RenderFragment NavBarBrand { get; set; }
        [Parameter]
        public RenderFragment NavBarItems { get; set; }
        bool CollapseNavMenu = true;
        string? NavMenuCssClass => CollapseNavMenu ? "collapse" : null;
        void ToggleNavMenu()
        {
            CollapseNavMenu = !CollapseNavMenu;
        }
    }
}
