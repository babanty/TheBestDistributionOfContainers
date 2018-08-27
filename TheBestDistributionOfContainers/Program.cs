// Добрый день. Само решение распологается в ControllerDistributionContainers.SetBestAllocationContainersOnTrucks() {154 строк кода}
// Все остальное обертка визуализирующая ситуацию.
// Я буду очень рад обратной связи, например в виде: "Ты занял n место в списке из m участников" или более развернутого ответа, спасибо :)
// Данный алгоритм состоит из трех ступеней перераспределения грузов, на тестовых данных лучшее время достигается уже на 1 ступени из всего 1 строки (лаконично). 
// Алгоритм рассчитан на большое количество грузов, было бы отлично проверить его на выборке из 50 и более контейнеров.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace TheBestDistributionOfContainers
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = GetData("Data");
            //var data = Console.ReadLine(); // Ждем данные в стандартном потоке
            var parseData = ParseinitialData(data); // Конвертируем данные в объекты

            var controllerDistributionContainers = new ControllerDistributionContainers(parseData); // Создаем контроллер отвечающий за респределние контейнеров
            controllerDistributionContainers.SetBestAllocationContainersOnTrucks(); // Распределяем контейнеры наилучшим образом

            var result = controllerDistributionContainers.GetDistributionContainersString(); // конвертируем ответ в строку
            Console.Write(result);
            Console.WriteLine("Время разгрузки: " + controllerDistributionContainers.GetTimeWork() + " мин.");// TODO удалить
            Console.ReadKey();
        }


        // Распрсиваем строку с данными о скорости разгурзки определенных контейнеров определенными разгрузчиками
        private static List<LoaderContainerTime> ParseinitialData(string data)
        {
            var result = new List<LoaderContainerTime>();

            var arrData = Regex.Split(data, "\n");

            // Очищаем строку от лишних символов
            for (var i = 0; i < arrData.Length; i++)
            {
                arrData[i] = arrData[i].Replace("п»ї", "");
                arrData[i] = arrData[i].Replace("\r", "");
            }

            // парсим строчки
            for (var i = 0; i < arrData.Length; i++)
            {
                try
                {
                    result.Add(parseinitialDataContainer(arrData[i]));
                }
                catch
                {
                    Console.WriteLine("Входные данные не могут быть обработаны т.к. не соотвествуют шаблону.");
                    Console.ReadKey();
                    break;
                }
            }

            if (result.Count == 0) throw new Exception("Данные не содержат информации в формате: ID погрузчика/ID контейнера: время погрузки этого контейнера на этом погрузчике в минутах.");
            return result;
        }

        // Распарсиваем строку с определенным контейнером и его разгрузчиком (была локальной ф-ей, но Mono 12 такое не любит)
        private static LoaderContainerTime parseinitialDataContainer(string dataContainer)
        {
            // пример что на входе: ID погрузчика/ID контейнера: время погрузки этого контейнера на этом погрузчике в минутах: 8/6:16 
            string idLoader = Regex.Split(dataContainer, "/")[0];
            string idContainer = Regex.Split(Regex.Split(dataContainer, "/")[1], ":")[0];
            string time = Regex.Split(Regex.Split(dataContainer, "/")[1], ":")[1];

            var loaderContainerTime = new LoaderContainerTime();
            loaderContainerTime.Time = Int32.Parse(time);
            loaderContainerTime.IdLoader = Int32.Parse(idLoader);
            loaderContainerTime.IdContainer = Int32.Parse(idContainer);
            return loaderContainerTime;
        }

        // УДАЛИТЬ В КОНЕЧНОЙ ВЕРСИИ Т.К. ДАННЫЙ ПРИХОДЯТ ЧЕРЕЗ STDIN , а не через txt TODO
        private static string GetData(string name)
        {
            return ReadFile(@"E:\MyDesktop\Code\c# обучение\Тест касперского 24.08.2018\TheBestDistributionOfContainers\TheBestDistributionOfContainers\" + name + ".txt");
        }
        // УДАЛИТЬ В КОНЕЧНОЙ ВЕРСИИ Т.К. ДАННЫЙ ПРИХОДЯТ ЧЕРЕЗ STDIN TODO
        private static string ReadFile(string Path)
        {
            string result;
            // чтение из файла
            using (FileStream fstream = File.OpenRead(Path))
            {
                // преобразуем строку в байты
                byte[] array = new byte[fstream.Length];
                // считываем данные
                fstream.Read(array, 0, array.Length);
                // декодируем байты в строку
                result = System.Text.Encoding.Default.GetString(array); // Сообщение их текста
            }

            return result;
        }
    }

}
