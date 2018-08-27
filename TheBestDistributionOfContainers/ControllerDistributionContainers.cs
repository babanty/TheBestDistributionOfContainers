using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBestDistributionOfContainers
{
    public class ControllerDistributionContainers
    {
        private List<LoaderContainerTime> LoaderContainerTimes { get; set; }
        private List<Loader> Loaders { get; set; }
        private List<Container> Containers { get; set; }


        public ControllerDistributionContainers(List<LoaderContainerTime> loaderContainerTimes)
        {
            this.LoaderContainerTimes = loaderContainerTimes;

            Initialization();
        }

        /// <summary> Инициализация, высенесена отдельно т.к. объект нужно переинициализировать в другом методе </summary>
        private void Initialization()
        {
            Loaders = new List<Loader>();
            Containers = new List<Container>();

            // Считаем сколько загрузчиков и грузов с какими Id у нас вообще есть, столько их и создаем
            foreach (var i in LoaderContainerTimes)
            {
                if (SearchLoader(i.IdLoader) == null)
                {
                    Loaders.Add(new Loader() { Id = i.IdLoader });
                }

                if (SearchContainer(i.IdContainer) == null)
                {
                    Containers.Add(new Container() { Id = i.IdContainer });
                }

                // Закидываем контейнеру информацию о скорости разгрузки определенным погрзчиком
                SearchContainer(i.IdContainer).LoadersTimesAdd(i);
            }
        }

        private Loader SearchLoader(int id)
        {
            foreach (var i in Loaders)
            {
                if (i.Id == id) return i;
            }
            return null;
        }

        private Container SearchContainer(int id)
        {
            foreach (var i in Containers)
            {
                if (i.Id == id) return i;
            }
            return null;
        }

        /// <summary> Вернуть суммарное время работы при текущем распределении контейнеров</summary>
        public int GetTimeWork()
        {
            return GetTheMostLoadedLoader().TotalWorkingTime;
        }

        /// <summary> Венрунть в виде строки распределение грузов. в виде строк, разделённых символом перевода 
        /// строки ('\n') в формате: ID погрузчика:ID контейнера;ID контейнера;...ID контейнера; </summary>
        public string GetDistributionContainersString()
        {
            string result = "";

            foreach (var i in Loaders)
            {
                result += i.Id + ":";

                foreach (var c in i.Containers)
                {
                    result += c.Id + ";";
                }

                result += "\n";
            }


            return result;
        }

        /// <summary> Распределить контейнеры наилучшим образом</summary>
        public void SetBestAllocationContainersOnTrucks()
        {
            // Если выполнять как задачу комбинаторики, то вариантов решения n погрузчиков в степени n грузов, вариант не подходит.

            // Переинициализируем контроллер чтобы удалить старое распределение грузов
            Initialization();

            // 1 ступень. В массив прогрузчиков распределяем контейнеры по эффективности разгрузки. Контейнер достается тому кто разгрузит за лучшее время
            // УЖЕ НА ЭТОМ ЭТАПЕ ДОСТИГАЕТСЯ МАСКСИМАЛЬНАЯ СКОРОСТЬ НА ТЕСТОВЫХ ВХОДНЫХ ДАННЫХ ИЗ 10 КОНТЕЙНЕРОВ.
            foreach (var i in Containers)
            {
                SearchLoader(i.GetTopLoaderContainerTime()[0].IdLoader).ContainersAdd(i);
            }

            // 2 ступень.
            // Проверяем возможно ли перераспределить с самого занятого погрузчика контейнеры другим погрузчикам для уменьшения времени разгрузки чтобы
            // исключить ситуацию, когда самому навороченному погрузчику достались все грузы т.к. он эффективней всех их разгружает.
            // Вводим коэффициент эффективности (КПД) перераспределения. Эффективно то перераспределение, что будет иметь минимальную разницу во времени
            // между текущим разгрузчиком, которого освобождаем и следующим. n = V0 / V1;
            // Таким образом мы максимально эффективно перераспределим контейнеры с самых занятых погрузчиков
            while (redistributedContainersForEfficientRedistribution()) { }; // Запускаем рекурсию, пока получается улучшить время этим занимаемся

            // 3 ступень.
            // Проверяем нельзя ли перераспределить с других погрузчиков грузы так чтобы освободить какой-нибудь прогрузчик и накинуть ему еще 1 контейнер 
            // с самого занятого для уменьшения  времени. Требует много вычислений
            while (tryToRedistributeContainersFromTheBusyLoader()) { }; // Запускаем рекурсию, пока получается улучшить время этим занимаемся

        }

        // (была локальной ф-ей, но Mono 12 такое не любит)
        // объяснение дано в месте где вызывается ф-я
        private bool redistributedContainersForEfficientRedistribution()
        {
            // Выводим список грузов по эффективности их перераспределения, где под индексом 0 находится лучший контейнер к перераспределению
            var mostLoadedLoader = GetTheMostLoadedLoader();
            var listOfContainersOnTheEffectivenessOfTheirRedistribution = new List<ContainersOnTheEffectivenessOfTheirRedistribution>();
            foreach (var i in mostLoadedLoader.Containers) // формируем список
            {
                var forAddWithList = new ContainersOnTheEffectivenessOfTheirRedistribution();
                forAddWithList.Container = i;
                forAddWithList.IdNowLoader = mostLoadedLoader.Id;

                var speedNowLoader = i.GetTimeSpeedWithLoader(mostLoadedLoader.Id); // Скорость текущего разгрузчика
                var nextLoader = i.GetNextLoaderInTOPContainerTime(mostLoadedLoader.Id);
                if (nextLoader == null) break; // Значит перебрали уже всех
                var idNextLoader = nextLoader.IdLoader;    // Id следущего разгрузчика по скорости разгрузки 
                forAddWithList.IdNextLoader = idNextLoader;
                var speedNextLoader = i.GetTimeSpeedWithLoader(idNextLoader); // Скорость следущего разгрузчика
                forAddWithList.Efficiency = decimal.Parse(speedNowLoader.ToString()) / decimal.Parse(speedNextLoader.ToString()); // Эффективность  n = V0 / V1;

                listOfContainersOnTheEffectivenessOfTheirRedistribution.Add(forAddWithList);
            }
            // Сортируем в порядке убывания эффективности перераспределения
            listOfContainersOnTheEffectivenessOfTheirRedistribution = listOfContainersOnTheEffectivenessOfTheirRedistribution.OrderByDescending(u => u.Efficiency).ToList();

            // Пробуем перераспределить контейнеры с самго занятого разгрузчика до тех пор, пока это будет уменьшать суммарное время разгрузки
            for (var i = 0; i < listOfContainersOnTheEffectivenessOfTheirRedistribution.Count; i++)
            {
                var redistribution = listOfContainersOnTheEffectivenessOfTheirRedistribution[i]; // перераспределение с которым работаем, сделано т.к. иначе слишком длинные имена
                if (redistribution.IsRedistributed) // если данное перераспределение еще не исполнялось
                {
                    var timeNow = GetTimeWork();// Считаем текущее время разгрузки
                    ToTransferContainer(redistribution.Container.Id, redistribution.IdNowLoader, redistribution.IdNextLoader); // передаем контейнер
                    var newTime = GetTimeWork();// Считаем изменилось ли время
                    if (newTime < timeNow) // Время улучшилось, тогда так и оставляем
                    {
                        return true;
                    }
                    else  // Время ухудшилось, значит нельзя перераспределить, следующему по эффективности разгрузчику. Соотвественно проверяем коэф.эффектиности
                          // с другими разгрузчиками до тех пор пока этот коэф. не станет меньше коэффициента перераспределения худшего из грузов
                    {
                        ToTransferContainer(redistribution.Container.Id, redistribution.IdNextLoader, redistribution.IdNowLoader); // возвращаем контейнер на место
                                                                                                                                   // находим самый худший показатель эффективности перераспределния других грузов
                        var worstPerformanceMeasure = 1.0m;
                        foreach (var ii in listOfContainersOnTheEffectivenessOfTheirRedistribution)
                        {
                            if (ii.Efficiency < worstPerformanceMeasure) worstPerformanceMeasure = ii.Efficiency;
                        }
                        // перебираем всех погрузчиков, кроме двух и считаем какова эффективности перераспределения им
                        var alternativeRedistribution = new ContainersOnTheEffectivenessOfTheirRedistribution
                        {
                            Efficiency = -1.0m,
                            Container = redistribution.Container,
                            IdNowLoader = redistribution.IdNowLoader,
                            IsRedistributed = false
                        };
                        foreach (var ii in Loaders)
                        {
                            if (ii.Id == mostLoadedLoader.Id || ii.Id == redistribution.IdNextLoader) continue; // пропускам тех, что уже исследовали

                            var speedNowLoader = redistribution.Container.GetTimeSpeedWithLoader(mostLoadedLoader.Id); // Скорость текущего разгрузчика
                            var speedNextLoader = redistribution.Container.GetTimeSpeedWithLoader(ii.Id); // Скорость следущего разгрузчика
                            var efficiency = speedNowLoader / speedNextLoader; // Эффективность  n = V0 / V1;

                            if (efficiency > alternativeRedistribution.Efficiency)
                            {
                                alternativeRedistribution.Efficiency = efficiency;
                                alternativeRedistribution.IdNextLoader = ii.Id;
                            }
                        }
                        // если удалось найти перераспределение этого контейнера за эффективность не ниже самого худшего  на данный момент, то запихиваем
                        // в лист перераспределения
                        if (alternativeRedistribution.Efficiency != -1.0m && alternativeRedistribution.Efficiency > worstPerformanceMeasure)
                        {
                            listOfContainersOnTheEffectivenessOfTheirRedistribution.Add(alternativeRedistribution);
                        }
                        redistribution.IsRedistributed = true; // указываем, что старое указание на перераспределение выполнено
                    }

                }
            }

            return false; // Так и не удалось улучшить время за эту итерацию рекурсии, значит выходим из нее
        }

        // (была локальной ф-ей, но Mono 12 такое не любит)
        // объяснение дано в месте где вызывается ф-я
        private bool tryToRedistributeContainersFromTheBusyLoader()
        {
            var mostLoadedLoader = GetTheMostLoadedLoader();
            for (var i = 0; i < mostLoadedLoader.Containers.Count; i++) // пробуем перекинуть любой из контейнеров c самого занятого
            {
                for (var ii = 0; ii < Loaders.Count; ii++) // любому погрузчику
                {
                    for (var iii = 0; iii < Loaders[ii].Containers.Count; iii++)  // вместо любого груза этого прогрузчика
                    {
                        for (var iiii = 0; iiii < Loaders.Count; iiii++) // который отдастся любому из погрузчиков
                        {
                            var timeNow = GetTimeWork();    // засекаем текущее время разгрузки
                                                            // перемещаем контейнер спперва из более свободного любому погрузчику
                            var containerOne = Loaders[ii].Containers[iii].Id;
                            var containerTwo = mostLoadedLoader.Containers[i].Id;
                            ToTransferContainer(containerOne, Loaders[ii].Id, Loaders[iiii].Id);
                            // теперь перемещаем контейнер из самого занятого более свободному погрузчику
                            ToTransferContainer(containerTwo, mostLoadedLoader.Id, Loaders[ii].Id);
                            // засекаем время.
                            var newTime = GetTimeWork();    // засекаем текущее время разгрузки

                            if (newTime < timeNow) // время улучшили, возвращаем true
                            {
                                return true;
                            }
                            else // не получилось улучшить время возвращаем контенеры на место
                            {

                                ToTransferContainer(containerTwo, Loaders[ii].Id, mostLoadedLoader.Id);
                                ToTransferContainer(containerOne, Loaders[iiii].Id, Loaders[ii].Id);
                            }
                        }

                    }
                }
            }

            return false;
        }

        /// <summary> Перемещае контейнер </summary>
        private void ToTransferContainer(int idContainer, int idGivingLoader, int idReceivingLoader)
        {
            var transferContainer = SearchContainer(idContainer);               // находим перемещаемый контейнер
            SearchLoader(idReceivingLoader).ContainersAdd(transferContainer);   // Записываем контейнер новому погрузчику
            SearchLoader(idGivingLoader).ContainersRemove(transferContainer);  // Удаляем его из списка у старого погрузчика
        }

        /// <summary> Локальная структура - груз и эффективность его перераспределения </summary>
        struct ContainersOnTheEffectivenessOfTheirRedistribution
        {
            public Container Container;
            public decimal Efficiency;
            public int IdNowLoader;
            public int IdNextLoader;
            public bool IsRedistributed; // выполнено ли перераспределение
        }

        /// <summary> Вернуть самого занятого загрузчика</summary>
        private Loader GetTheMostLoadedLoader()
        {
            var maxTime = 0;
            Loader mostLoadedLoader = null;
            foreach (var i in Loaders)
            {
                if (i.TotalWorkingTime > maxTime)
                {
                    maxTime = i.TotalWorkingTime;
                    mostLoadedLoader = i;
                }
            }

            return mostLoadedLoader;
        }
    }

}
