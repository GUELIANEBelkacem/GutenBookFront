using App1.Book_List;
using App1.Services;
using App1.Views;
using DLToolkit.Forms.Controls;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            FlowListView.Init();
            DependencyService.Register<MockDataStore>();
            MainPage = new NavigationPage(new WelcomePage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
