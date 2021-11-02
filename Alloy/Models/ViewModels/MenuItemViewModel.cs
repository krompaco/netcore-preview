using System.Collections.Generic;

namespace AlloyMvcTemplates.Models.ViewModels
{
    public class MenuItemViewModel
    {
        public List<MenuItemViewModel> ChildItems { get; set; }

        public string LinkUrl { get; set; }

        public string Text { get; set; }

        public int Level { get; set; }
    }
}
