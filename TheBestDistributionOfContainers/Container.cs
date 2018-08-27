using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBestDistributionOfContainers
{
    /// <summary> Контейнер из задачи</summary>
    public class Container
    {
        public int Id { get; set; }

        /// <summary> Время разгрузки на разных разгрузчиках</summary>
        private List<LoaderContainerTime> LoadersTimes { get; set; } = new List<LoaderContainerTime>();

        public void LoadersTimesAdd(LoaderContainerTime loaderContainerTime)
        {
            LoadersTimes.Add(loaderContainerTime);
        }

        /// <summary> Вернуть время разгрузки этого контейнера данным рзагрузчиком</summary>
        public int GetTimeSpeedWithLoader(int IdLoader)
        {
            foreach (var i in LoadersTimes)
            {
                if (i.IdLoader == IdLoader) return i.Time;
            }

            throw new Exception("Такого разгрузчика не зафиксированно");
        }

        /// <summary> Вернуть ТОП в порядке возрастания времени разгрузки этого контейнера разгрузчиками.
        ///  0 месте лучший погрузчик</summary>
        public LoaderContainerTime[] GetTopLoaderContainerTime()
        {
            if (LoadersTimes.Count == 0) throw new Exception("Не известно ни чего о погрузчиках работающих с этим контейнером");
            var sortedLoadersTimes = LoadersTimes.OrderBy(u => u.Time);
            return sortedLoadersTimes.ToArray();
        }

        /// <summary> Вернуть следущего по эффективности разгрузчика данного контейнера после Id указанного или null</summary>
        public LoaderContainerTime GetNextLoaderInTOPContainerTime(int IdLoader)
        {
            var top = GetTopLoaderContainerTime();

            for (var i = 0; 0 < top.Length; i++)
            {
                if (top[i].IdLoader == IdLoader)
                    try
                    {
                        return top[i + 1];
                    }
                    catch
                    {
                        return null;
                    }

            }

            return null;
        }

    }

}
