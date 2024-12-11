using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization.Json;
using Microsoft.Win32;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Net.Http.Json;
using System.Security.Policy;


namespace memCatalog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Название папки для хранения
        public const string FOLDERNAME = "MyMems";

        //Имя категории которая отображает всё
        public const string FULLMEMCATEGORY = "Все мемы";

        //Для хранения пути к дефолтной папке
        private string myFolder = "";
        public string MyFolder { get => myFolder; private set => myFolder = value; }

        //Окно добавления мемов
        private AddMem addMemWindow;

        //Список мемов
        private static List<Mem> memCatalog = new List<Mem>();
        //Список категорий
        private static List<String> category = new List<String>();

        //Событие добавления или удаления
        public delegate void MemHandler(string message);
        public event MemHandler? Notify;

        public MainWindow()
        {
            InitializeComponent();


            //инициализация
            addMemWindow = new AddMem();

            //Проверка существует ли каталог для мемов
            if (Directory.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FOLDERNAME)))
            {
                //Переменная пути каталога
                MyFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FOLDERNAME);
            }
            else
            {
                //Создание каталога если его нет
                Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FOLDERNAME));
                MyFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FOLDERNAME);
            }

            //Добавляем категорию для отображения всех мемов и выбираем его по умолчанию
            addDefaultCategory();

            //Подписываюсь на событие
            Notify += Notification;
        }

        //Добавляем категорию для отображения всех мемов и выбираем его по умолчанию
        private void addDefaultCategory()
        {
            category.Add(FULLMEMCATEGORY);
            MainCatecoryBox.ItemsSource = category;
            MainCatecoryBox.SelectedIndex = 0;
        }

        //Вызов окна добавления мемов
        private void AddMem_Button_Click(object sender, RoutedEventArgs e)
        {
            //Проверка есть ли уже окно
            if (addMemWindow != null)
                addMemWindow.Close();

            addMemWindow = new AddMem();
            addMemWindow.Owner = this;
            addMemWindow.UpdateCategory(category);
            addMemWindow.Show();
        }

        public void AddMemInCatalog(Mem mem)
        {
            memCatalog.Add(mem); //Добавление мема
            Notify?.Invoke($"Я Вас Уведомляю, что мем успешно добавлен"); //Вызов события
        }

        //Обновить список показываемых мемов
        public void UpdateMemShowList()
        {
            MemList.Items.Clear();
            foreach (Mem mem in memCatalog)
            {
                MemList.Items.Add(mem.GetName());
            }
        }
        //Обновляем список показанных мемов с выбранной категорией
        public void UpdateMemShowListCategory(string category)
        {
            MemList.Items.Clear();
            foreach (Mem mem in memCatalog)
            {
                if(mem.GetCategoty() == category)
                    MemList.Items.Add(mem.GetName());
            }
        }
        //Обновление списка мемов согласно строки поиска
        public void UpdateMemShowListSearch(string searh)
        {
            //Очищаем показанный список
            MemList.Items.Clear();
            //Проходимя по мемам
            string nameMem = "";
            foreach (Mem mem in memCatalog)
            {
                nameMem = mem.GetName().ToLower();
                //Проверяем содержит ли имя строку поиска и делаем нижний регистр у двух строк, чтобы типа не зависило от регистра
                if (nameMem.Contains(searh.ToLower()))
                {
                    //Добавляем мем в отображаемый список
                    MemList.Items.Add(mem.GetName());
                }
            }
        }

        //Обновляем список категорий
        public void UpdateCategoryShowList()
        {
            MainCatecoryBox.ItemsSource = new List<string> (category);
        }

        private void MemList_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (MemList.SelectedItem == null) return;

            string nameSelectMem = MemList.SelectedItem.ToString();
            foreach (Mem mem in memCatalog)
            {
                if (mem.GetName() == nameSelectMem)
                {
                    //ImageBox.Source = new BitmapImage(new Uri(mem.GetPath())); //Просто присвоить картинку, без проверки на существование файла по пути

                    //Структура чтобы проверить существование ресурса мема, вдруг удалили
                    try
                    {
                        Uri uri = new Uri(mem.GetPath());
                        if (Uri.IsWellFormedUriString(mem.GetPath(), UriKind.RelativeOrAbsolute))
                        {
                            var fullFilePath = mem.GetPath();

                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                            bitmap.EndInit();

                            ImageBox.Source = bitmap;
                        }
                        else
                        {
                            if (uri.IsBaseOf(uri))
                            {
                                ImageBox.Source = new BitmapImage(uri);
                            }
                        }  
                    }
                    catch (Exception ex)
                    {
                        //Ставлю изображение ресурс, всемто утерянного мема
                        ImageBox.Source = new BitmapImage(new Uri("Image/error_image.png", UriKind.Relative));
                    }
                }
            }
        }

        //Удаление мема из каталога
        private void DelMem(object sender, RoutedEventArgs e)
        {
            if (MemList.SelectedItem != null)
            {
                foreach (Mem mem in memCatalog)
                {
                    if (mem.GetName() == MemList.SelectedValue.ToString())
                    {
                        //Ставлю изображение ресурс, всемто утерянного мема
                        ImageBox.Source = new BitmapImage(new Uri("Image/error_image.png", UriKind.Relative));

                        memCatalog.Remove(mem);
                        Category_Changed();
                        Notify?.Invoke("Мем успешно удалён. Мы его потеряли :(");  //вызов уведомления
                        return;
                    }
                }
            }
        }

        //Проверка есть ли такое имя в списке мемов
        public bool CheckNameMem(string name)
        {
            foreach (Mem mem in memCatalog)
            {
                if (mem.GetName().ToLower() == name.ToLower()) return true;
            }
            return false;
        }

        //Добавление новой категории и проверка существует ли уже такая
        public bool AddCategory(string categoryName)
        {
            foreach(string c in category)
            {
                if (c.ToLower() == categoryName.ToLower()) return false;
            }
            category.Add(categoryName);
            UpdateCategoryShowList();
            return true;
        }

        //Обновление строки поиска
        private void SearchTXT_Changed(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text == "")
            {
                //Обновляем список без параметров, так как поиск пустой
                UpdateMemShowList();
            }
            else
            {
                //вызываем функцию обновления списка мемов с текстом поиска
                UpdateMemShowListSearch(SearchBox.Text);
            }
        }

        //Выбор категории
        //Функция обновления для кнопки
        private void Category_Changed(object sender, SelectionChangedEventArgs e)
        {
            Category_Changed();
        }
        //функция простого вызова обновления категорий
        private void Category_Changed()
        {
            if ((string)MainCatecoryBox.SelectedItem == FULLMEMCATEGORY)
            {
                //вызов обновления списка без рараметров
                UpdateMemShowList();
            }
            else
            {
                //вызов обновления списка с указание фильтрующей категории
                UpdateMemShowListCategory((string)MainCatecoryBox.SelectedItem);
            }
        }

        //Вызов окна уведомления
        public void Notification(string str)
        {
            MessageBox.Show(str, "Уведомляю", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        //Функция сохранения
        private void Save_File(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            //UnicodeEncoding uniencoding = new UnicodeEncoding();

            //Установка фильтра файлов
            saveFileDialog.Filter = "json files (*.json)|*.json";
            saveFileDialog.FilterIndex = 1;
            //Изначальная директория сохранения
            saveFileDialog.InitialDirectory = myFolder;
 
            if(saveFileDialog.ShowDialog() == true)
            {
                //Параметр нужен для кодировки файла, чтобы отображалась кирилица
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.All),
                    WriteIndented = true,
                    IncludeFields = true
                };
                //Сериализация объектов списка
                string json = JsonSerializer.Serialize(memCatalog, options);
                //Запись в файл
                File.WriteAllText(saveFileDialog.FileName, json);

                Notify?.Invoke("Мама, тут всё сохранилось"); //Ведомление о сохранение
            }
        }

        //Функция загрузки
        private void Load_File(object sender, RoutedEventArgs e)
        {
            //Создание OpenFileDialog
            OpenFileDialog openFileDlg = new OpenFileDialog();
            //Установка фильтра на выбираемые файлы
            openFileDlg.Filter = "Сохранённый JSON (*.json)|*.json";
            openFileDlg.FilterIndex = 1;

            //Открываем диалоговое окно
            if (openFileDlg.ShowDialog() == true)
            {
                //Чтений файла
                var readJson = File.ReadAllText(openFileDlg.FileName);
                //Проверка на пустоту
                if (String.IsNullOrWhiteSpace(readJson))
                {
                    MessageBox.Show("Файл пуст", "Load error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.All),
                    WriteIndented = true,
                    IncludeFields = true
                };

                List<Mem> tmpList = JsonSerializer.Deserialize<List<Mem>>(readJson, options);
                if (tmpList != null)
                {
                    memCatalog = new List<Mem>(tmpList);

                    //проходим по списку мемов, чтобы получить их категории
                    category = new List<string>();
                    addDefaultCategory();
                    foreach (Mem mem in memCatalog)
                    {
                        //Добавление с помощью функции чтобы исключить повтарение категорий
                        AddCategory(mem.GetCategoty());
                    }
                    //вызов обновления списка мемов
                    UpdateMemShowList();
                    //вызов обновления списка категорий
                    UpdateCategoryShowList();

                    Notify?.Invoke("Я всё загрузил."); //Уведомление загрузки
                }
            }
        }
    }
}