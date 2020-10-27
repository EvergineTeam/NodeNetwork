using ExampleCustom.PostProcessingGraph.ViewModel;
using NodeNetwork;
using ReactiveUI;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;

namespace ExampleCustom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<PostProcessingGraphViewerVM>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(PostProcessingGraphViewerVM), typeof(MainWindow), new PropertyMetadata(null));
        public PostProcessingGraphViewerVM ViewModel
        {
            get => (PostProcessingGraphViewerVM)this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => this.ViewModel;
            set => this.ViewModel = (PostProcessingGraphViewerVM)value;
        }

        static MainWindow()
        {
            NNViewRegistrar.RegisterSplat();
        }

        private readonly MenuItem groupNodesButton;
        private readonly MenuItem ungroupNodesButton;
        private readonly MenuItem openGroupButton;

        public MainWindow()
        {
            InitializeComponent();
            this.ViewModel = new PostProcessingGraphViewerVM();

            var nodeMenu = ((ContextMenu)Resources["nodeMenu"]).Items.OfType<MenuItem>();
            groupNodesButton = nodeMenu.First(c => c.Name == nameof(groupNodesButton));
            ungroupNodesButton = nodeMenu.First(c => c.Name == nameof(ungroupNodesButton));
            openGroupButton = nodeMenu.First(c => c.Name == nameof(openGroupButton));

            this.WhenActivated(d =>
            {
                this.OneWayBind(this.ViewModel, vm => vm.NetworkViewModel, v => v.networkView.ViewModel).DisposeWith(d);
                this.OneWayBind(this.ViewModel, vm => vm.NodeListViewModel, v => v.nodeList.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NetworkBreadcrumbBar, v => v.breadcrumbBar.ViewModel).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.GroupNodes, v => v.groupNodesButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.UngroupNodes, v => v.ungroupNodesButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenGroup, v => v.openGroupButton).DisposeWith(d);
            });
        }
    }
}
