using DynamicData;
using ExampleCustom.PostProcessingGraph.Nodes;
using ExampleCustom.ViewModels.Nodes;
using NodeNetwork.Toolkit.BreadcrumbBar;
using NodeNetwork.Toolkit.Group;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.ViewModels;
using ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace ExampleCustom.PostProcessingGraph.ViewModel
{
    class NetworkBreadcrumb : BreadcrumbViewModel
    {
        #region Network
        private NetworkViewModel _network;
        public NetworkViewModel Network
        {
            get => _network;
            set => this.RaiseAndSetIfChanged(ref _network, value);
        }
        #endregion
    }

    public class PostProcessingGraphViewerVM : ReactiveObject
    {
        public BreadcrumbBarViewModel NetworkBreadcrumbBar { get; } = new BreadcrumbBarViewModel();

        private readonly ObservableAsPropertyHelper<NetworkViewModel> _network;
        private int index;

        public NetworkViewModel NetworkViewModel => _network.Value;
        public NodeListViewModel NodeListViewModel { get; } = new NodeListViewModel();

        public ReactiveCommand<Unit, Unit> GroupNodes { get; }
        public ReactiveCommand<Unit, Unit> UngroupNodes { get; }
        public ReactiveCommand<Unit, Unit> OpenGroup { get; }

        public PostProcessingGraphViewerVM()
        {
            // Breadcrumb Bar
            this.WhenAnyValue(vm => vm.NetworkBreadcrumbBar.ActiveItem).Cast<NetworkBreadcrumb>()
                .Select(b => b?.Network)
                .ToProperty(this, vm => vm.NetworkViewModel, out _network);
            NetworkBreadcrumbBar.ActivePath.Add(new NetworkBreadcrumb
            {
                Name = "Main",
                Network = new NetworkViewModel()
            });

            // First Node
            this.NetworkViewModel.Nodes.Add(new GraphNodeViewModel("Node0"));

            // Node list
            NodeListViewModel.AddNodeType(() => new GraphNodeViewModel("Node1"));
            NodeListViewModel.AddNodeType(() => new GraphNodeViewModel("Node2"));
            NodeListViewModel.AddNodeType(() => new GraphNodeViewModel("Node3"));

            // Groups
            var grouper = new NodeGrouper
            {
                GroupNodeFactory = subnet => new GraphGroupNodeViewModel(subnet, "Group1"),
                EntranceNodeFactory = () => new GraphGroupSubnetIONodeViewModel(this.NetworkViewModel, true, false, "Group Input"),
                ExitNodeFactory = () => new GraphGroupSubnetIONodeViewModel(this.NetworkViewModel, false, true, "Group Output"),
                SubNetworkFactory = () => new NetworkViewModel(),
                IOBindingFactory = (groupNode, entranceNode, exitNode) =>
                    new GraphNodeGroupIOBinding(groupNode, entranceNode, exitNode)
            };
            GroupNodes = ReactiveCommand.Create(() =>
            {
                var groupBinding = (GraphNodeGroupIOBinding)grouper.MergeIntoGroup(NetworkViewModel, NetworkViewModel.SelectedNodes.Items);
                string groupName = $"Group{index++}";
                groupBinding.EntranceNode.Name = groupName;
                groupBinding.ExitNode.Name = groupName;
                groupBinding.GroupNode.Name = groupName;

                ((GraphGroupNodeViewModel)groupBinding.GroupNode).IOBinding = groupBinding;
                ((GraphGroupSubnetIONodeViewModel)groupBinding.EntranceNode).IOBinding = groupBinding;
                ((GraphGroupSubnetIONodeViewModel)groupBinding.ExitNode).IOBinding = groupBinding;
            }, this.WhenAnyObservable(vm => vm.NetworkViewModel.SelectedNodes.CountChanged).Select(c => c > 1));

            var isGroupNodeSelected = this.WhenAnyValue(vm => vm.NetworkViewModel)
               .Select(net => net.SelectedNodes.Connect())
               .Switch()
               .Select(_ => NetworkViewModel.SelectedNodes.Count == 1 && NetworkViewModel.SelectedNodes.Items.First() is GraphGroupNodeViewModel);

            UngroupNodes = ReactiveCommand.Create(() =>
            {
                var selectedGroupNode = (GraphGroupNodeViewModel)NetworkViewModel.SelectedNodes.Items.First();
                grouper.Ungroup(selectedGroupNode.IOBinding);
            }, isGroupNodeSelected);

            OpenGroup = ReactiveCommand.Create(() =>
            {
                var selectedGroupNode = (GraphGroupNodeViewModel)NetworkViewModel.SelectedNodes.Items.First();
                NetworkBreadcrumbBar.ActivePath.Add(new NetworkBreadcrumb
                {
                    Network = selectedGroupNode.Subnet,
                    Name = selectedGroupNode.Name
                });
            }, isGroupNodeSelected);
        }
    }
}
