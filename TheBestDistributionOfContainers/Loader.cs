using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBestDistributionOfContainers
{
    /// <summary> Загрузчик </summary>
    public class Loader
    {
        public int Id { get; set; }

        public List<Container> Containers { get; private set; } = new List<Container>();

        public int TotalWorkingTime { get; private set; } = 0;

        public void ContainersAdd(Container container)
        {
            Containers.Add(container);

            TotalWorkingTime += container.GetTimeSpeedWithLoader(this.Id);
        }

        public void ContainersRemove(Container container)
        {
            Containers.Remove(container);

            TotalWorkingTime -= container.GetTimeSpeedWithLoader(this.Id);
        }
    }

}
