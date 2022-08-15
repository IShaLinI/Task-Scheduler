using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scheduler
{
    public partial class MainWindow
    {
        string[] classes = { "Comm", "Eng", "Calc", "CSci" };
        string[] categories = { "Other", "Short Response", "Essay", "Quiz", "Exam", "Lab" };

        public MainWindow()
        {
            InitializeComponent();

            foreach(string Class in classes){
                cbxClass.Items.Add(Class);
                cbxEditClass.Items.Add(Class);
            }
            
            foreach(string Category in categories)
            {
                cbxCategory.Items.Add(Category);
                cbxEditCategory.Items.Add(Category);
            }

        }

        private void addTask(object sender, RoutedEventArgs e)
        {
            JObject task = 
                new JObject(
                    new JProperty("task",
                        new JObject(
                            new JProperty("title", txtTitle.Text),
                            new JProperty("class", cbxClass.SelectionBoxItem.ToString()),
                            new JProperty("category", cbxCategory.SelectionBoxItem.ToString()),
                            new JProperty("duedate", DateTime.Parse(dtpDue.SelectedDateTime.ToString()).ToString()),
                            new JProperty("link", txtLink.Text)
                        )
                    )
                );

            if (File.Exists(@"G:\My Drive\Schedule Data\" + txtTitle.Text + ".json"))
            {
                MessageBoxResult result = MessageBox.Show("Task with name: " + txtTitle.Text + " already exists. Override?", "Override Warning", System.Windows.MessageBoxButton.YesNo);
                if(result == MessageBoxResult.Yes)
                {
                    File.WriteAllText(@"G:\My Drive\Schedule Data\" + txtTitle.Text + ".json", task.ToString());
                }
            }
            else
            {
                File.WriteAllText(@"G:\My Drive\Schedule Data\" + txtTitle.Text + ".json", task.ToString());
            }

        }

        public void updateData() {
            lstWorkQue.Items.Clear();

            int tempIndex = lstEditTasks.SelectedIndex;
            lstEditTasks.Items.Clear();
            lstEditTasks.SelectedIndex = tempIndex;

            string[] tasks = Directory.GetFiles(@"G:\My Drive\Schedule Data\");

            JsonTasks = new JObject[tasks.Length];

            int iterations = 0;

            //Load JSON
            foreach (string task in tasks)
            {
                JsonTasks[iterations] = JObject.Parse(File.ReadAllText(Path.Combine(@"G:\My Drive\Schedule Data\", task)));
                iterations++;
            }

            //Sort Tasks
            while (true)
            {
                bool flipped = false;

                iterations = 0;
                foreach (JObject task in JsonTasks)
                {

                    DateTime SelectedDate = DateTime.Parse(task["task"]["duedate"].ToString());
                    DateTime NextDate;

                    if (iterations != JsonTasks.Length - 1)
                    {
                        NextDate = DateTime.Parse(JsonTasks[iterations + 1]["task"]["duedate"].ToString());
                    }
                    else
                    {
                        NextDate = SelectedDate;
                    }

                    int result = DateTime.Compare(SelectedDate, NextDate);

                    //If NextDate is Earlier than Selected Date
                    if (result > 0)
                    {
                        JObject temp = JsonTasks[iterations];

                        JsonTasks[iterations] = JsonTasks[iterations + 1];
                        JsonTasks[iterations + 1] = temp;

                        flipped = true;
                    }

                    iterations++;
                }

                if (flipped == false)
                {
                    break;
                }

            }

            //Create List Of Tasks
            foreach (JObject task in JsonTasks)
            {
                lstWorkQue.Items.Add(
                    "[" + task["task"]["class"].ToString() + "] | " +
                    task["task"]["title"].ToString() +
                    " | Due: " + DateTime.Parse(task["task"]["duedate"].ToString()).ToString("dddd, dd MMM h:mm tt")
                );

                lstEditTasks.Items.Add(
                    task["task"]["title"].ToString()
                );

            }

        }

        JObject[] JsonTasks;

        private void WorkQueLoaded(object sender, RoutedEventArgs e)
        {
            updateData();
        }

        private void QueTaskSelected(object sender, SelectionChangedEventArgs e)
        {
            if (lstWorkQue.IsLoaded)
            {
                if (lstWorkQue.SelectedIndex != -1)
                {
                    Uri result;
                    bool urlValidated = Uri.TryCreate(JsonTasks[lstWorkQue.SelectedIndex]["task"]["link"].ToString(), UriKind.Absolute, out result);

                    if (urlValidated)
                    {
                        System.Diagnostics.Process.Start(JsonTasks[lstWorkQue.SelectedIndex]["task"]["link"].ToString());
                    }
                }
            }

            lstWorkQue.SelectedIndex = -1;
        }

        private void lstEditTaskChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstEditTasks.IsLoaded)
            {
                if (lstEditTasks.SelectedIndex != -1)
                {
                    txtEditTitle.Text = JsonTasks[lstEditTasks.SelectedIndex]["task"]["title"].ToString();
                    cbxEditClass.SelectedIndex = Array.IndexOf(classes, JsonTasks[lstEditTasks.SelectedIndex]["task"]["class"].ToString());
                    cbxEditCategory.SelectedIndex = Array.IndexOf(categories, JsonTasks[lstEditTasks.SelectedIndex]["task"]["category"].ToString());
                    dtpEditDue.DisplayDate = DateTime.Parse(JsonTasks[lstEditTasks.SelectedIndex]["task"]["duedate"].ToString());
                    txtEditLink.Text = JsonTasks[lstEditTasks.SelectedIndex]["task"]["link"].ToString();
                }
            }

        }

        private void modifyTask(object sender, RoutedEventArgs e)
        {

            File.Delete(@"G:\My Drive\Schedule Data\" + lstEditTasks.SelectedItem.ToString() + ".json");

            DateTime duedate;

            if (dtpEditDue.SelectedDateTime == null)
            {
                duedate = DateTime.Parse(JsonTasks[lstEditTasks.SelectedIndex]["task"]["duedate"].ToString());
            }
            else {
                duedate = DateTime.Parse(dtpEditDue.SelectedDateTime.ToString());
            }

            JObject task =
                new JObject(
                    new JProperty("task",
                        new JObject(
                            new JProperty("title", txtEditTitle.Text),
                            new JProperty("class", cbxEditClass.SelectionBoxItem.ToString()),
                            new JProperty("category", cbxEditCategory.SelectionBoxItem.ToString()),
                            new JProperty("duedate", duedate),
                            new JProperty("link", txtEditLink.Text)
                        )
                    )
                );

            File.WriteAllText(@"G:\My Drive\Schedule Data\" + txtEditTitle.Text + ".json", task.ToString());

            updateData();
        }

        private void editTaskLoaded(object sender, RoutedEventArgs e)
        {
            updateData();
        }
    }
}
