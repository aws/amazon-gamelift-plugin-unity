using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.GameLift.Model;
using Amazon.Runtime.Internal;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.CoreAPI
{
    public class GameLiftComputeManager
    {
        private readonly CoreApi _coreApi;
        private readonly IAmazonGameLiftClientWrapper _amazonGameLiftWrapper;
        private string _fleetName;
        private string _fleetId;
        private VisualElement _container;
        private ErrorResponse _logger;
        private StateManager _stateManager;

        public GameLiftComputeManager(CoreApi coreApi, IAmazonGameLiftClientWrapper wrapper)
        {
            _coreApi = coreApi;
            _amazonGameLiftWrapper = wrapper;
        }

        public async Task<bool> RegisterFleetCompute(string computeName, string fleetId, string fleetLocation,
            string ipAddress)
        {
            var webSocketUrl = await RegisterCompute(computeName, fleetId, fleetLocation, ipAddress);
            if (webSocketUrl != null)
            {
                _stateManager.SelectedProfile.ComputeName = computeName;
                _stateManager.SelectedProfile.IpAddress = ipAddress;
                _stateManager.SelectedProfile.WebSocketUrl = webSocketUrl;
                
                var profileSaveResponse = _stateManager.SaveProfiles();

                return profileSaveResponse.Success;
            }
            return false;
        }

        private async Task<string> RegisterCompute(string computeName, string fleetId, string fleetLocation,
            string ipAddress)
        {
            try
            {
                var registerComputeRequest = new RegisterComputeRequest()
                {
                    ComputeName = computeName,
                    FleetId = fleetId,
                    IpAddress = ipAddress,
                    Location = fleetLocation
                };
                var registerComputeResponse =
                    await _amazonGameLiftWrapper.RegisterCompute(registerComputeRequest);

                return registerComputeResponse.Compute.GameLiftServiceSdkEndpoint;
            }
            catch (Exception ex)
            {
                var errorBox = _container.Q<VisualElement>("ComputeErrorInfoBox");
                errorBox.style.display = DisplayStyle.Flex;
                errorBox.Q<Label>().text = ex.Message;
                Debug.Log(ex.Message);
                return null;
            }
        }

        public async Task<bool> DeregisterCompute(string computeName, string fleetId)
        {
            try
            {
                var deregisterComputeRequest = new DeregisterComputeRequest()
                {
                    ComputeName = computeName,
                    FleetId = fleetId
                };
                var deregisterComputeResponse =
                    await _amazonGameLiftWrapper.DeregisterCompute(deregisterComputeRequest);

                return deregisterComputeResponse.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }
        }

        public async Task<bool> DescribeCompute(string computeName, string fleetId)
        {
            try
            {
                var describeComputeRequest = new DescribeComputeRequest()
                {
                    ComputeName = computeName,
                    FleetId = fleetId,
                };
                var describeComputeResponse =
                    await _amazonGameLiftWrapper.DescribeCompute(describeComputeRequest);

                return describeComputeResponse.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }
        }
    }
}