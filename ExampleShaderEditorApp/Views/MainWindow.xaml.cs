using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using ExampleShaderEditorApp.ViewModels;
using NodeNetwork.Views.Controls;
using ReactiveUI;

namespace ExampleShaderEditorApp.Views
{
    public partial class MainWindow : Window, IViewFor<MainViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(null));

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.ViewModel = new MainViewModel();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.NodeListViewModel, v => v.nodeList.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NetworkViewModel, v => v.networkView.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.NetworkViewModel.MaxZoomLevel, v => v.MaxZoom.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.NetworkViewModel.MinZoomLevel, v => v.MinZoom.Text).DisposeWith(d);
                this.BindCommand(ViewModel, x => x.NetworkViewModel.CenterView, v => v.Test5).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ShaderPreviewViewModel, v => v.shaderPreviewView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ShaderPreviewViewModel.FragmentShaderSource, v => v.shaderSource.Text, source => string.Join("\n", source)).DisposeWith(d);

                this.WhenAnyValue(v => v.shaderPreviewView.ActualWidth).BindTo(this, v => v.shaderPreviewView.Height).DisposeWith(d);
            });

            nodeList.CVS.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
        }

        private void Test1_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NetworkViewModel.ZoomLevel++;
        }

        private void Test2_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NetworkViewModel.ZoomLevel--;
        }

        private void Test3_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NetworkViewModel.Position = new Point();
        }

        private void Test4_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NetworkViewModel.Position = new Point(ViewModel.NetworkViewModel.Position.X + 100, ViewModel.NetworkViewModel.Position.Y);
        }
    }
}
