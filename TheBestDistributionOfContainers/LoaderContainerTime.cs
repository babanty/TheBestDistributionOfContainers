using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBestDistributionOfContainers
{
    /// <summary> Строка поступающая на вход программе, содержащая id контейнера, разгрузчика и время разгрузки</summary>
    public class LoaderContainerTime
    {
        public int Time { get; set; }
        public int IdLoader { get; set; }
        public int IdContainer { get; set; }
    }

}
