using ExampleCustom.ViewModels.Nodes;
using NodeNetwork;
using NodeNetwork.Toolkit.Group.AddEndpointDropPanel;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;
using System;

namespace ExampleCustom.PostProcessingGraph.Nodes
{
    public class GraphGroupSubnetIONodeViewModel : GraphNodeViewModel
    {
        static GraphGroupSubnetIONodeViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new NodeView(), typeof(IViewFor<GraphGroupSubnetIONodeViewModel>));
        }

        public NetworkViewModel Subnet { get; }

        public GraphNodeGroupIOBinding IOBinding
        {
            get => _ioBinding;
            set
            {
                if (_ioBinding != null)
                {
                    throw new InvalidOperationException("IOBinding is already set.");
                }
                _ioBinding = value;
                AddEndpointDropPanelVM = new AddEndpointDropPanelViewModel(_isEntranceNode, _isExitNode)
                {
                    NodeGroupIOBinding = IOBinding
                };
            }
        }
        private GraphNodeGroupIOBinding _ioBinding;

        public AddEndpointDropPanelViewModel AddEndpointDropPanelVM { get; set; }

        private readonly bool _isEntranceNode, _isExitNode;

        public GraphGroupSubnetIONodeViewModel(NetworkViewModel subnet, bool isEntranceNode, bool isExitNode, string name) 
            : base(name, false)
        {
            this.Subnet = subnet;
            _isEntranceNode = isEntranceNode;
            _isExitNode = isExitNode;
        }
    }
}
