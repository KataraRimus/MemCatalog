using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace memCatalog
{
    /// <summary>
    /// Логика взаимодействия для AddMem.xaml
    /// </summary>

    public partial class AddMem : Window
    {
        //переменная означающая выбор пустой категории в комбобоксе, чтобы можно было указать новую категорию
        private string category_void = "";

        //Создание OpenFileDialog
        private Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();


        public AddMem()
        {
            InitializeComponent();
        }

        //Установка категорий
        public void UpdateCategory(List<String> category)
        {
            List<string> tmpCategory = new List<string>(category);
            tmpCategory.Remove(MainWindow.FULLMEMCATEGORY);
            tmpCategory.Add(category_void);
            SelectCategory.ItemsSource = tmpCategory;
            SelectCategory.SelectedItem = category_void;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            //Установка фильтра на выбираемые файлы
            openFileDlg.Filter = "Картинки(*.PNG;*.JPG;*.GIF)|*.PNG;*.JPG;*.GIF" + "|Все файлы (*.*)|*.* ";

            //Открытие диалогового окна выбора файла
            Nullable<bool> result = openFileDlg.ShowDialog();

            if (result == true)
            {
                FileNameTextBox.Text = openFileDlg.FileName;
                URLBox.IsEnabled = false;
            }
        }

        private void AddMem_inCatalog(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Owner;
            if (mainWindow == null)
            {
                MessageBox.Show("Непредвиденная ошибка", "Error 404", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            //Проверка на заполняемость полей
            if (String.IsNullOrWhiteSpace(NameMem.Text))
            {
                MessageBox.Show("Имя обязательны для заполнения", "Name error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (mainWindow.CheckNameMem(NameMem.Text))
            {
                MessageBox.Show("Такое название уже есть в каталоге", "Name error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if ((string)SelectCategory.SelectedItem == category_void && String.IsNullOrWhiteSpace(newCategoryStr.Text))
            {
                MessageBox.Show("Выберите категорию или впишите новую", "Category error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //выбираем категорию для присвоения мема
            string category;
            if ((string)SelectCategory.SelectedItem == category_void || SelectCategory.SelectedItem == null || String.IsNullOrWhiteSpace((string)SelectCategory.SelectedItem))
            {
                category = newCategoryStr.Text;
                //Добавляем новую категорию
                if (!mainWindow.AddCategory(category))
                {
                    MessageBox.Show("Категория с таким именем существует", "Category error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else 
            {
                category = (string)SelectCategory.SelectedItem;
            }

            //определяем путь к файлу или URL
            string newPathName = "";

            if (URLBox.IsEnabled && !String.IsNullOrWhiteSpace(URLBox.Text))
            {
                //если вписан url
                newPathName = URLBox.Text;
            }
            else
            {
                //если выбран файл

                //Новый путь к файлу
                newPathName = System.IO.Path.Combine(mainWindow.MyFolder, category + "_" + NameMem.Text + "_" + System.IO.Path.GetFileName(openFileDlg.FileName));
                //Копирование файла в свою папку
                System.IO.File.Copy(openFileDlg.FileName, newPathName);
            }

            Mem newMem = new Mem(NameMem.Text, category, newPathName);
            mainWindow.AddMemInCatalog(newMem);
            mainWindow.UpdateCategoryShowList();
            mainWindow.UpdateMemShowList();
            this.Close();
        }

        //При выборе категории из списка отключить или включить поле для ввода новой категории
        private void Selected_Category(object sender, SelectionChangedEventArgs e)
        {

            if ((string)SelectCategory.SelectedItem == category_void)
            {
                newCategoryStr.IsEnabled = true;
            }
            else
            {
                newCategoryStr.IsEnabled = false;
            }
        }

        //Если что-то вписано в путь к файлу, то сделать ввод URL неактивным, а если пусто, то активным
        private void Path_text_changed(object sender, TextChangedEventArgs e)
        {
            if(String.IsNullOrWhiteSpace(FileNameTextBox.Text))
            {
                URLBox.IsEnabled = true;
            }
            else
            {
                URLBox.IsEnabled = false;
            }
        }

        //Если вписан URL то отключить кнопку и строку пути
        private void URL_text_changed(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(URLBox.Text))
            {
                FileNameTextBox.IsEnabled = true;
                BrowseButton.IsEnabled = true;
            }
            else
            {
                FileNameTextBox.IsEnabled = false;
                BrowseButton.IsEnabled = false;
            }
        }
    }
}
