using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UseLogService.ViewModels
{
    public class MainUserContentViewModel
    {
        public MainUserContentViewModel()
        {
            Levels = Enum.GetValues(typeof(LogLevel));
        }

        public Array Levels { get; set; }
    }
}
