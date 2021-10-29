using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace LessonFinal
{
    [Serializable]
    class Program
    {
        public static int count;
        public static BinaryFormatter formatter = new BinaryFormatter();

        static void Main(string[] args)
        {
            string command;
            string settingPath = @".\settings.bin";
            string workDir;
            int j;
            int SrowPerPage = 1;
            int ErowPerPage = Convert.ToInt32(ConfigurationManager.AppSettings["recPerPage"]);
            string tr = "none";
            string[] comm;


            //проверяем, существует ли файл, хранящий последнее значение пути к директории, которое использовалось при работе программы в предыдущий раз
            //если существует, десериализуем функцией TakeSettings и кладем полученный путь в workDir
            //отображаем дерево файлов полученного пути
            if (File.Exists(settingPath))
            {
                workDir = TakeSettings(settingPath, formatter);
                count = 1;
                j = 0;
                GetDisplayDirTree(workDir, j, SrowPerPage, ErowPerPage);
            }

            //бесконечный цикл, и которого может быть осуществлен выход только по команде e
            while (true)
            {
                try
                {
                    //вводим значение команды и путь через пробел
                    Console.WriteLine("\n");
                    Console.WriteLine("Please input needed command:");
                    command = Console.ReadLine();
                    Console.Clear();

                    //бьем строку на массив символов по пробелу
                    comm = command.Split(' ');

                    if (comm[0] == "ls")
                    {
                        workDir = comm[1];
                        j = 0;
                        int CurrSrow = 0;
                        int PrevCurrSrow = 0;
                        int n = 0;

                        //отображаем первую страницу дерева файлов: SrowPerPage - значение строки с которой начинаем отображение, ErowPerPage - количество строк на страницу
                        count = 1;
                        CurrSrow = GetDisplayDirTree(workDir, j, SrowPerPage, ErowPerPage) - 1;
                        PrintCommand();
                        tr = Console.ReadLine();

                        //если далее следует выход из режима просмотра дерева файлов, сохраняем последнее значение директории в файл функцией SaveSettings
                        if (tr == "e")
                            SaveSettings(settingPath, workDir, formatter);
                        Console.Clear();

                        //если выход не последовал, переходим к режиму постраничного просмотра дерева файлов
                        //пока пользователь не ввел e для перехода к другой команде
                        while (tr != "e")
                            //если введено n - отображаем следующую страницу
                            if (tr == "n")
                            {
                                count = 1;
                                //в переменную PrevCurrSrow сохраняем последнее значение CurrSrow
                                PrevCurrSrow = CurrSrow;
                                //печатаем дерево файлов заданной директории workDir начиная с номера строки CurrSrow + SrowPerPage до номера CurrSrow + ErowPerPage
                                //сохраняем возвращаемое значение (номер записи, на которой остановилась печать - 1) в переменную CurrSrow
                                CurrSrow = GetDisplayDirTree(workDir, j, CurrSrow + SrowPerPage, CurrSrow + ErowPerPage) - 1;

                                //обработка случая если страница последняя
                                //если количество всего напечатанных строк не делится нацело на количество отображаемых элементов на страницу, это, вероятно, последняя страница
                                //если это первая печать (n = 0), просто отображаем дерево последней попытки и делаем n+1, при последующих попытках ввода n
                                //отображаем, что это последняя страница и ждем команду p
                                if (CurrSrow % ErowPerPage != 0)
                                {
                                    while (tr == "n")
                                    {
                                        if (n != 0)
                                        {
                                            //отображаем последнюю возможную страницу и сообщение о том, что она последняя
                                            //ждём пока пользователь не введет команду p - просмотр предыдущей страницы
                                            //если пользователь нажимает e - выход из команды ls, записываем значение рабочей директории в файл settings.bin функцией SaveSettings
                                            count = 1;
                                            GetDisplayDirTree(workDir, j, ((CurrSrow / ErowPerPage) * ErowPerPage + SrowPerPage), CurrSrow);
                                            Console.WriteLine("\n");
                                            Console.WriteLine("This is the final page! Please use p - command.");
                                            PrintCommand();
                                            tr = Console.ReadLine();
                                            if (tr == "e")
                                                SaveSettings(settingPath, workDir, formatter);
                                            Console.Clear();
                                        }
                                        else
                                        {
                                            n = n + 1;
                                            break;
                                        }
                                    }
                                }

                                //обработка случая, если количество файлов делится нацело на количество записей на страницу и при этом страница является последней
                                //если предыдущее значение CurrSrow = текущему, значит печати больше не производилось и страница последняя
                                else if (PrevCurrSrow == CurrSrow)
                                {
                                    while (tr == "n")
                                    {
                                        //отображаем последнюю возможную страницу и сообщение о том, что она последняя
                                        //ждём пока пользователь не введет команду p - просмотр предыдущей страницы
                                        //если пользователь нажимает e - выход из команды ls, записываем значение рабочей директории в файл settings.bin функцией SaveSettings
                                        count = 1;
                                        GetDisplayDirTree(workDir, j, CurrSrow - ErowPerPage + SrowPerPage, CurrSrow);
                                        Console.WriteLine("\n");
                                        Console.WriteLine("This is the final page! Please use p - command.");
                                        PrintCommand();
                                        tr = Console.ReadLine();
                                        if (tr == "e")
                                            SaveSettings(settingPath, workDir, formatter);
                                        Console.Clear();
                                    }
                                }

                                else
                                {
                                    //если страница не последняя, продолжаем отображать последующие страницы
                                    PrintCommand();
                                    tr = Console.ReadLine();
                                    if (tr == "e")
                                        SaveSettings(settingPath, workDir, formatter);
                                }
                                Console.Clear();
                            }

                            //если введено p - отображаем предыдущую страницу
                            else if (tr == "p")
                            {
                                n = 0;

                                //проверка, находимся ли мы на первой странице печати (когда CurrSrow - ErowPerPage = 0)
                                if ((CurrSrow - ErowPerPage) > 0)
                                {
                                    //если нет, делаем обработку для случая, если мы на последней странице, а количество файлов не делится нацело на количество отображаемых записей на страницу
                                    if (CurrSrow % ErowPerPage == 0)
                                    {
                                        count = 1;
                                        CurrSrow = GetDisplayDirTree(workDir, j, (CurrSrow - 2 * ErowPerPage) + 1, CurrSrow - ErowPerPage) - 1;
                                        PrintCommand();
                                        tr = Console.ReadLine();
                                        if (tr == "e")
                                            SaveSettings(settingPath, workDir, formatter);
                                        Console.Clear();
                                    }
                                    //логика для вывода в случае если количество напечатанных записей делится нацело на требуемое количества записей на страницу
                                    else
                                    {
                                        count = 1;
                                        CurrSrow = GetDisplayDirTree(workDir, j, ((CurrSrow / ErowPerPage) * ErowPerPage - ErowPerPage + 1), (CurrSrow / ErowPerPage) * ErowPerPage) - 1;
                                        PrintCommand();
                                        tr = Console.ReadLine();
                                        if (tr == "e")
                                            SaveSettings(settingPath, workDir, formatter);
                                        Console.Clear();
                                    }
                                }
                                //если страница первая, печатаем первую страницу и сообщение о том, что она первая
                                //ждем ввода команды n или e
                                else
                                {
                                    while (tr == "p")
                                    {
                                        count = 1;
                                        CurrSrow = GetDisplayDirTree(workDir, j, SrowPerPage, ErowPerPage) - 1;
                                        Console.WriteLine("This is the first page!");
                                        PrintCommand();
                                        tr = Console.ReadLine();
                                        if (tr == "e")
                                            SaveSettings(settingPath, workDir, formatter);
                                        Console.Clear();
                                    }
                                }
                            }
                            //обработка ошибки на случай, если введена некорректная команда
                            else
                            {
                                Console.WriteLine("Command is wrong! Please input correct one.");
                                PrintCommand();
                                tr = Console.ReadLine();
                                Console.Clear();
                            }
                    }
                    //если первая часть введенной команды = cf, копируем файл, указанный первым в файл, указанный вторым с помощью функции File.Copy
                    else if (comm[0] == "cf")
                    {
                        Console.Clear();
                        File.Copy(comm[1], comm[2]);

                    }
                    //если первая часть введенной команды = cd, копируем директорию, указанную во ввдененной команде первой в директорию, указанную второй
                    else if (comm[0] == "cd")
                    {   //у первой директории берем имя
                        string sourName = new DirectoryInfo(comm[1]).Name;
                        //составляем путь: путь ко второй директории + имя первой = путь к копии первой директории внутри второй
                        //создаем из получившегося пути директорию
                        string commDir = Path.Combine(comm[2], sourName);
                        Directory.CreateDirectory(commDir);

                        //копируем содержимое директории в её копию по новому пути
                        GetDirectoryTree(comm[1], commDir);
                        Console.Clear();
                    }
                    //если первая часть введенной команды = df, удаляем файл, указанный через пробел с помощью функции File.Delete
                    else if (comm[0] == "df")
                    {
                        File.Delete(comm[1]);
                        Console.Clear();
                    }
                    //если первая часть введенной команды = dd, удаляем директорию и все директории, лежащие в ней (аргумент функции true)
                    else if (comm[0] == "dd")
                    {
                        Directory.Delete(comm[1], true);
                        Console.Clear();
                    }
                    // если первая часть введенной команды = fi, проверяем, чем является вторая часть команды - файлом или директорией
                    else if (comm[0] == "fi")
                    {
                        //если файлом
                        if (File.Exists(comm[1]))
                        {
                            //создаем объект класса FileInfo и передаем значение пути к файлу
                            //отображаем информацию о файле с помощью методов класса
                            Console.Clear();
                            FileInfo file = new FileInfo(comm[1]);
                            Console.WriteLine("\n");
                            Console.WriteLine($"FilePath: {comm[1]} | SystemAttribytes: {File.GetAttributes(comm[1])} | Size: {file.Length} bytes | CreationDate: {File.GetCreationTime(comm[1]).ToString("d")}");
                        }
                        //если директорией, делаем то же самое, что для файла, кроме определения размера директории - для этого пишем специальную функцию GetDirectorySize
                        else if (Directory.Exists(comm[1]))
                        {
                            Console.Clear();
                            Console.WriteLine("\n");
                            Console.WriteLine($"FilePath: {comm[1]} | SystemAttribytes: {File.GetAttributes(comm[1])} | Size: {GetDirectorySize(comm[1], 0)} bytes | CreationDate: {File.GetCreationTime(comm[1]).ToString("d")}");
                        }
                    }
                    //если введенная команда = e, принудительно завершаем бесконечный цикл
                    else if (command == "e")
                        break;

                    //обработка ошибки на случай, если введена некорректная команда
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Command is wrong! Please input correct one.");
                    }

                }
                //обработка возможных исключений, которые может выбросить блок кода выше
                catch (IOException)
                {
                    Console.WriteLine("Wrong file path. Please try again");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Destination path is empty. Please input");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Access is denied");
                }
                catch
                {
                    Console.WriteLine("Something went wrong. Please check inputed data and try again");
                }
            }
        }

        //функция получения директорий для отображения
        static int GetDisplayDirTree(string workDir, int j, int SrowPerPage, int ErowPerPage)
        {
            //делаем массив из директорий переданной директории
            string[] dirArray = Directory.GetDirectories(workDir);

            for (int i = 0; i < dirArray.GetLength(0); i++)
            {
                //если количество уже отображенных файлов меньше требуемого количества для отображения
                //берем имя директории и печатаем функцией  PrintScreen            
                if (SrowPerPage <= ErowPerPage)
                {
                    string dirName = new System.IO.DirectoryInfo(dirArray[i]).Name;

                    SrowPerPage = PrintScreen(j, dirName, SrowPerPage, ErowPerPage);

                    //если в директории есть вложенные директории, проходимся по ним и их файлам рекурсивным вызовом функции и печатаем их имена
                    //если вложенных директорий нет, печатаем только вложенные файлы
                    if (Directory.GetDirectories(dirArray[i]).GetLength(0) > 0)
                    {
                        if (j < 2)
                        {
                            SrowPerPage = GetDisplayDirTree(dirArray[i], j + 1, SrowPerPage, ErowPerPage);
                            SrowPerPage = GetDisplayFileTree(dirArray[i], j, SrowPerPage, ErowPerPage);
                        }
                    }
                    else
                        SrowPerPage = GetDisplayFileTree(dirArray[i], j + 1, SrowPerPage, ErowPerPage);
                }
                else
                    break;
            }
            if (SrowPerPage > 0)
                SrowPerPage = GetDisplayFileTree(workDir, j + 1, SrowPerPage, ErowPerPage);

            return SrowPerPage;
        }

        //функция печати дерева файлов на экран
        static int PrintScreen(int j, string name, int SrowPerPage, int ErowPerPage)
        {
            //если количество вызовов функции становится >= переданному значению начальной записи для печати
            if (count >= SrowPerPage)
            {
                //сверяемся, что значение начальной записи для печати <= количеству записей, которое должно быть напечатано на странице
                //печатаем имена директорий и файлов в зависимости от j - уровня вложенности
                if (SrowPerPage <= ErowPerPage)
                {
                    switch (j)
                    {
                        case 0:
                            Console.WriteLine(name);
                            SrowPerPage = SrowPerPage + 1;
                            count = count + 1;
                            return SrowPerPage;
                        case 1:
                            Console.WriteLine($"     |--- {name}");
                            SrowPerPage = SrowPerPage + 1;
                            count = count + 1;
                            return SrowPerPage;
                        case 2:
                            Console.WriteLine($"                 |-----  {name}");
                            SrowPerPage = SrowPerPage + 1;
                            count = count + 1;
                            return SrowPerPage;
                        default:
                            return SrowPerPage;
                    }
                }
                else
                {
                    count = count + 1;
                    return SrowPerPage;
                }
            }
            else
            {
                count = count + 1;
                return SrowPerPage;
            }
        }


        //функция получения файлов для отображения в дереве
        static int GetDisplayFileTree(string workDir, int j, int SrowPerPage, int ErowPerPage)
        {
            //если количество уже отображенных файлов меньше требуемого количества для отображения
            if (SrowPerPage <= ErowPerPage)
            {
                //делаем массив из файлов переданной директории и печатаем каждый функцией PrintScreen
                string[] fileDir = Directory.GetFiles(workDir);

                if (fileDir.GetLength(0) > 0)
                    for (int k = 0; k < fileDir.GetLength(0); k++)
                        SrowPerPage = PrintScreen(j, Path.GetFileName(fileDir[k]), SrowPerPage, ErowPerPage);
                return SrowPerPage;
            }
            else
                return SrowPerPage;
        }


        //функция копирования содержимого директории
        static void GetDirectoryTree(string sourceDir, string targetDir)
        {
            //создаем массив из директорий директории, которую нужно скопировать
            string[] sourceArray = Directory.GetDirectories(sourceDir);

            //если есть хоть одна директория (длина получившегося массива > 0) проходимся по каждой и составляем путь к ней в директории получившейся в ветке кода команды cd выше
            if (sourceArray.Length > 0)
            {
                for (int i = 0; i < sourceArray.Length; i++)
                {
                    string sourName = new DirectoryInfo(sourceArray[i]).Name;
                    string commDir = Path.Combine(targetDir, sourName);
                    Directory.CreateDirectory(commDir);

                    //если в директории есть дочерние директории делаем с ними то же самое, что с родительской на предыдущем этапе запуская функцию рекурсивно
                    if (Directory.GetDirectories(sourceArray[i]).Length > 0)
                    {
                        GetDirectoryTree(sourceArray[i], commDir);
                        //параллельно копируем специальной функцией найденные в директории файлы
                        GetFiles(sourceArray[i], commDir);
                    }
                    else
                    {   //если дочерних директорий нет, просто копируем файлы
                        GetFiles(sourceArray[i], commDir);
                    }
                }
            }
            else
            {
                GetFiles(sourceDir, targetDir);
            }

        }

        //функция копирования файлов директории 
        static void GetFiles(string sourceDir, string targetDir)
        {   //создаем массив из путей к файлам директории sourceDir
            string[] filesArray = Directory.GetFiles(sourceDir);

            for (int i = 0; i < filesArray.Length; i++)
            {   //для у каждого значения полученного массива берем имя файла
                //берем название директории, куда надо скопировать файл и склеиваем, чтобы получился путь
                string fileName = Path.GetFileName(filesArray[i]);
                string commDir = Path.Combine(targetDir, fileName);
                //копируем содержимое файла директории sourceDir в получившийся аналогичный файл директории targetDir
                File.Copy(filesArray[i], commDir);
            }
        }

        //функция получения размера директории (размер директории = сумме размеров находящихся в ней файлов)
        static long GetDirectorySize(string workDir, long commSize)
        {
            //составляем массивы дочерних директорий и файлов, находящихся в родительской
            string[] dirArray = Directory.GetDirectories(workDir);
            string[] files = Directory.GetFiles(workDir);

            //для каждого файла получившегося массива определяем его размер и плюсуем его в переменную, где будет содержаться суммарный размер всех файлов, найденных функцией
            for (int i = 0; i < files.GetLength(0); i++)
            {
                FileInfo f = new FileInfo(files[i]);
                commSize = commSize + f.Length;
            }

            //если в дочерних директориях есть вложенные директории, запускаем функцию рекурсивно и по ходу суммируем размеры найденных файлов в переменную commSize
            //если вложенных папок нет, суммируем просто размеры находящихся там файлов
            for (int i = 0; i < dirArray.GetLength(0); i++)
            {

                if (Directory.GetDirectories(dirArray[i]).GetLength(0) > 0)
                {
                    GetDirectorySize(dirArray[i], commSize);
                }
                else
                {
                    files = Directory.GetFiles(workDir);

                    for (int j = 0; j < files.GetLength(0); j++)
                    {
                        FileInfo f = new FileInfo(files[j]);
                        commSize = commSize + f.Length;
                    }
                }
            }

            return commSize;
        }

        //функия сериализации - передаем путь к файлу для сохранения настроек и последнее актуальное значение директории
        static void SaveSettings(string settingPath, string workDir, BinaryFormatter formatter)
        {
            using (FileStream fs = new FileStream(settingPath, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, workDir);
            }
        }

        //функия для десериализации - передаем путь к файлу с ранее сохраненными настройками, возвращаем значение директории
        //которое было сохранено при предыдущем использовании программы
        static string TakeSettings(string settingPath, BinaryFormatter formatter)
        {
            using (FileStream fs = new FileStream(settingPath, FileMode.Open))
            {
                string workDir = (string)formatter.Deserialize(fs);
                return workDir;
            }
        }

        //функция печати сообщения о вводе с отрисовкой разделителя
        static void PrintCommand()
        {
            Console.WriteLine("\n");
            Console.WriteLine("=====================================================================================================================");
            Console.WriteLine("\n");
            Console.WriteLine("What would you like to do? n - to see next page, p - to see previous page, e - exit");
        }

    }
}
