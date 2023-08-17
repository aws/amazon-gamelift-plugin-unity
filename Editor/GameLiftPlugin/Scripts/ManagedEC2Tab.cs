using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using Editor.GameLiftPlugin.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ManagedEC2Tab : Tab
{
    private static readonly Dictionary<string, string> _osMappings = new Dictionary<string, string>
    {
        { "Amazon Linux 2 (AL2)", "" },
        { "Amazon Linux 2023 (AL2023)", "" },
        { "Windows Server 2012 (End of OS support on 10.10.2023)", "" },
        { "Windows Server 2016", "" }
    };

    private static readonly Dictionary<int, int> _scenarioMappings = new Dictionary<int, int>
    {
        { 0, 1 },
        { 1, 3 },
        { 2, 4 }
    };

    private static readonly Dictionary<int, string> _scenarioShortNames = new Dictionary<int, string>
    {
        { 1, "Single-Region" },
        { 3, "Spot" },
        { 4, "Flex" }
    };
    private readonly StackUpdateModelFactory _stackUpdateModelFactory;
    private readonly Waiter _waiter = new Waiter();

    private string[] _changeCount;

    private string[] _changes;
    private DeploymentSettings _model;
    private DropdownField _buildOSElement;
    private TextField _buildNameElement;
    private TextField _fleetNameElement;
    private TextField _launchParamsElement;

    public ManagedEC2Tab(VisualElement root, GameLiftPlugin gameLiftConfig)
    {
        Root = root;
        TabNumber = 4;
        _stackUpdateModelFactory = new StackUpdateModelFactory(new ChangeSetUrlFormatter());
        TextProvider = TextProviderFactory.Create();
        _waiter.InfoUpdated += OnCurrentStackInfoChanged;
        SetupDeployment();
        SetupTab();
        SetupAccountDetails();
        GameLiftConfig = gameLiftConfig;
    }
    public GameLiftPlugin GameLiftConfig { get; set; }

    private DeploymentStates DeploymentStatus
    {
        get
        {
            if (_model.IsDeploymentRunning) return DeploymentStates.Deploying;
            if (_model.CurrentStackInfo.StackStatus == StackStatus.DeleteInProgress) return DeploymentStates.Deleting;
            if (_model.HasCurrentStack) return DeploymentStates.Deployed;
            return DeploymentStates.NotDeployed;
        }
    }

    protected TextProvider TextProvider { get; private set; }

    private void SetupDeployment()
    {
        _model = DeploymentSettingsFactory.Create();
        _model.Refresh();
        _model.Restore();
        _ = _model.WaitForCurrentDeployment();
        Settings.SharedInstance.AnySettingChanged += OnAnySettingChanged;
        _model.CurrentStackInfoChanged += OnCurrentStackInfoChanged;
        OnCurrentStackInfoChanged();
    }

    private void OnCurrentStackInfoChanged()
    {
        UpdateDeploymentStatus();
    }


    private void OnAnySettingChanged()
    {
        _model.Refresh();
    }

    private void OnDisable()
    {
        //TODO Figureout where this will go, important to run
        Settings.SharedInstance.AnySettingChanged -= OnAnySettingChanged;
        _model.Save();
        _model.CancelWaitingForDeployment();
    }

    private void SetupTab()
    {
        var scenarios = ScenarioLocator.SharedInstance.GetScenarios().ToList();

        string tabName = "Tab4";
        string scenarioShortName = _scenarioShortNames[_model.ScenarioIndex];
        RadioButtonGroup deploymentRadio = Root.Query<Foldout>("DeploymentSelectExpanded").Descendents<RadioButtonGroup>().First();
        Root.Q<Button>("SingleHelp").RegisterCallback<ClickEvent>(_ => { Application.OpenURL(scenarios[1].HelpUrl); });
        Root.Q<Button>("SpotHelp").RegisterCallback<ClickEvent>(_ => { Application.OpenURL(scenarios[3].HelpUrl); });
        Root.Q<Button>("FlexHelp").RegisterCallback<ClickEvent>(_ => { Application.OpenURL(scenarios[4].HelpUrl); });

        deploymentRadio.RegisterValueChangedCallback(e =>
        {
            int targetScenario = _scenarioMappings[e.newValue];
            _model.ScenarioIndex = targetScenario;
            scenarioShortName = _scenarioShortNames[_model.ScenarioIndex];
            Root.Query<Foldout>("Parameters").Descendents<VisualElement>("BuildName").Descendents<TextField>().First()
                .value = $"{Application.productName}-{scenarioShortName}-Build";
            Root.Q<Foldout>("Deploy").text = $"Deploy ({_model.CurrentDeployer.DisplayName})";
        });

        Root.Q<Foldout>("Parameters").text = $"{Application.productName} parameters";
        Root.Q<Foldout>("Deploy").text = $"Deploy ({_model.CurrentDeployer.DisplayName})";

        _fleetNameElement = Root.Query<Foldout>("Parameters").Descendents<VisualElement>("FleetName").Descendents<TextField>().First();
        _buildNameElement = Root.Query<Foldout>("Parameters").Descendents<VisualElement>("BuildName").Descendents<TextField>().First();
        _buildOSElement = Root.Query<VisualElement>("BuildOS").Children<DropdownField>().First();
        _launchParamsElement = Root.Query<Foldout>("Parameters").Descendents<VisualElement>("LaunchParameters").Descendents<TextField>().First();
        
        _fleetNameElement.value = $"{Application.productName}-ManagedFleet";
        _buildNameElement.value = $"{Application.productName}-{scenarioShortName}-Build";

        VisualElement gameServerFolder = Root.Q<VisualElement>("GameServerFolder");
        gameServerFolder.Q<TextField>().value = _model.BuildFolderPath;
        gameServerFolder.Q<Button>().RegisterCallback<ClickEvent>(_ =>
        {
            string value = EditorUtility.OpenFolderPanel("Game Server Build Folder Path", Application.dataPath, "");
            _model.BuildFolderPath = value;
            gameServerFolder.Q<TextField>().value = _model.BuildFolderPath;
        });

        VisualElement gameServerFile = Root.Q<VisualElement>("GameServerFile");
        gameServerFile.Q<TextField>().value = _model.BuildFilePath;
        gameServerFile.Q<Button>().RegisterCallback<ClickEvent>(_ =>
        {
            string value = EditorUtility.OpenFilePanel("Game Server Executable Path", _model.BuildFolderPath, "");
            _model.BuildFilePath = value;
            gameServerFile.Q<TextField>().value = _model.BuildFilePath;
        });


        base.SetupTab(tabName, OnTabButtonClicked);
    }

    private void OnTabButtonClicked(ClickEvent evt, Button button)
    {
        Debug.Log(button.name + " Clicked");
        switch (button.name)
        {
            case "ShowMoreScenarios":
            {
                //TODO Switch from this wizard to expandedversion
                VisualElement currentFoldout = AllFoldoutElements.FirstOrDefault(element => element.name == "DeploymentSelect");
                VisualElement targetFoldout =
                    AllFoldoutElements.FirstOrDefault(element => element.name == "DeploymentSelectExpanded");
                ChangeFoldout(currentFoldout, targetFoldout);
                break;
            }
            case "CreateResource":
            case "RedeployResource":
            {
                ToggleButtons(button, false);
                if (!_model.CanDeploy) break;
                _model.FleetName = _fleetNameElement.value;
                _model.BuildName = _buildNameElement.value;
                _model.LaunchParameters = _launchParamsElement.value;
                _model.BuildOperatingSystem = _osMappings[_buildOSElement.value];
                _model.StartDeployment(ConfirmChangeSet)
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted) Debug.LogException(task.Exception);
                        ToggleButtons(button, true);                    
                        _model.RefreshCurrentStackInfo();

                    });
                break;
            }
            case "DeleteResource":
            {
                //TODO Only show this button when we are deployed, delete resource
                ToggleButtons(button, false);
                _model.DeleteDeployment();
                _waiter.WaitUntilDone(_model).ContinueWith(task =>
                {
                    if (task.IsFaulted) Debug.LogException(task.Exception);
                    ToggleButtons(button, true);
                    _model.RefreshCurrentStackInfo();
                });
                break;
            }
            case "LaunchClient":
            {
                EditorApplication.EnterPlaymode();
                break;
            }
        }
    }
    
    private void SetupAccountDetails()
    {
        if (GameLiftConfig.CurrentState.AllProfiles.Length >= 1)
        {
            var targetFoldout = Root.Q<VisualElement>("AccountDetails","Tab4Foldout");
            ChangeFoldout(null, targetFoldout);
        }
    }

    private void UpdateDeploymentStatus()
    {
        var element = Root.Q(className: "deploy-status");
        element.EnableInClassList("deploy-status--not-deployed", DeploymentStatus == DeploymentStates.NotDeployed);
        element.EnableInClassList("deploy-status--in-progress", DeploymentStatus == DeploymentStates.Deleting || DeploymentStatus == DeploymentStates.Deploying);
        element.EnableInClassList("deploy-status--deployed", DeploymentStatus == DeploymentStates.Deployed);

        var textElement = element.Q<Label>(className:"deploy-status__label");
        switch (DeploymentStatus)
        {
            case DeploymentStates.NotDeployed:
                textElement.text = "Not Deployed";
                break;
            case DeploymentStates.Deploying:
                textElement.text = "Deploying";
                break;
            case DeploymentStates.Deployed:
                textElement.text = "Deployed";
                break;
            case DeploymentStates.Deleting:
                textElement.text = "Deleting";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        element.Q<Button>(className: "deploy-status__console-link").RegisterCallback<ClickEvent>(_ =>
        {
            Application.OpenURL(Urls.AwsCloudFormationConsole);
        });
        
        var createButton = Root.Q<Button>("CreateResource");
        var redeployButton = Root.Q<Button>("RedeployResource");
        var deleteButton = Root.Q<Button>("DeleteResource");
        var launchButton = Root.Q<Button>("LaunchClientButton");

        createButton.SetEnabled(DeploymentStatus == DeploymentStates.NotDeployed);
        redeployButton.SetEnabled(DeploymentStatus == DeploymentStates.Deployed);
        deleteButton.SetEnabled(DeploymentStatus == DeploymentStates.Deployed);
        launchButton.SetEnabled(DeploymentStatus == DeploymentStates.Deployed);
    }

    private Task<bool> SetUp()
    {
        var completionSource = new TaskCompletionSource<bool>();
        return completionSource.Task;
    }

    private Task<bool> ConfirmChangeSet(ConfirmChangesRequest request)
    {
        _stackUpdateModelFactory.Create(request);
        return SetUp();
    }
    
    public override void OnAccountSelect()
    {
        
    }
    
    private enum DeploymentStates
    {
        NotDeployed,
        Deploying,
        Deployed,
        Deleting
    }
}
