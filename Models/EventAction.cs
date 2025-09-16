using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Mercurio.Driver.Models
{
    public class EventAction
    {
        public string Text { get; set; }
        public string IconGlyph { get; set; } 
        public ICommand Command { get; set; }
    }
}
