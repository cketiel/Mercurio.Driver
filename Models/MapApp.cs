using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercurio.Driver.Models
{
    public partial class MapApp : ObservableObject
    {
        public string Name { get; set; }
        public string Icon { get; set; } // Image file name in Resources/Images
        public string Scheme { get; set; } // Unique identifier (URL Scheme for iOS, Package Name for Android)

        [ObservableProperty]
        private bool isSelected;
    }
}
