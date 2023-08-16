using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ManagedEC2Tab : Tab
{
    private static readonly Dictionary<string, string> OSMappings = new Dictionary<string, string>
    {
        { "Amazon Linux 2 (AL2)", "" },
        { "Amazon Linux 2023 (AL2023)", "" },
        { "Windows Server 2012 (End of OS support on 10.10.2023)", "" },
        { "Windows Server 2016", "" }
    };

    private static readonly Dictionary<int, int> ScenarioMappings = new Dictionary<int, int>
    {
        { 0, 1 },
        { 1, 3 },
        { 2, 4 }
    };

    private static readonly Dictionary<int, string> ScenarioShortNames = new Dictionary<int, string>
    {
        { 1, "Single-Region" },
        { 3, "Spot" },
        { 4, "Flex" }
    };

	private GameLiftPlugin GameLiftConfig;
    private StackUpdateModelFactory _stackUpdateModelFactory;
    private DeploymentSettings _model;
    public ManagedEC2Tab(VisualElement root, GameLiftPlugin gameLiftConfig)
    {
        GameLiftConfig = gameLiftConfig;
        Root = root;
        TabNumber = 4;
        _stackUpdateModelFactory = new StackUpdateModelFactory(new ChangeSetUrlFormatter());
        TextProvider = TextProviderFactory.Create();
        SetupDeployment();
        SetupTab();
    }

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
        Debug.Log("Franky");
        var stackStatus = _model.CurrentStackInfo.StackStatus;

        Root.Q<Label>("DeployStatusLabel").text = stackStatus;
    }

    private void OnAnySettingChanged()
    {
        _model.Refresh();
    }

    private void UpdateStatus()
    {
        var status = _model.CurrentStackInfo.Status;

        // DrawInfo(_model.CurrentStackInfo.Details, GUILayout.Height(55));
        // DrawStackOutput(_labelCognitoClientId, _model.CurrentStackInfo.UserPoolClientId);
        // DrawStackOutput(_labelApiGateway, _model.CurrentStackInfo.ApiGatewayEndpoint);

        // if (_model.Status.IsDisplayed)
        // {
        //     _statusLabel.Draw(_model.Status.Message, _model.Status.Type);
        // }
    }
    
    // private void DrawButtons()
    // {
    //     
    //         bool disabled = !_model.CanCancel;
    //
    //         using (new EditorGUI.DisabledScope(disabled))
    //         {
    //             if (GUILayout.Button(_labelCancelButton) && ConfirmCancel())
    //             {
    //                 _model.CancelDeployment();
    //             }
    //         }
    //
    //         using (new EditorGUI.DisabledScope(!_model.CanDeploy))
    //         {
    //             if (GUILayout.Button(_labelStartButton))
    //             {
    //                 _model.StartDeployment(ConfirmChangeSet)
    //                     .ContinueWith(task =>
    //                     {
    //                         if (task.IsFaulted)
    //                         {
    //                             Debug.LogException(task.Exception);
    //                         }
    //                     });
    //             }
    //         }
    //     }
    // }
    
    // private bool ConfirmCancel()
    // {
    //     return EditorUtility.DisplayDialog(_textProvider.Get(Strings.TitleDeploymentCancelDialog),
    //         _textProvider.Get(Strings.LabelDeploymentCancelDialogBody),
    //         _textProvider.Get(Strings.LabelDeploymentCancelDialogOkButton),
    //         _textProvider.Get(Strings.LabelDeploymentCancelDialogCancelButton));
    // }

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
        
        var tabName = "Tab4";
        var scenarioShortName = ScenarioShortNames[_model.ScenarioIndex];
        var deploymentRadio = Root.Query<Foldout>("DeploymentSelectExpanded").Descendents<RadioButtonGroup>().First();
        Root.Q<Button>("SingleHelp").RegisterCallback<ClickEvent>(e =>
        {
            Application.OpenURL(scenarios[1].HelpUrl);
        });        
        Root.Q<Button>("SpotHelp").RegisterCallback<ClickEvent>(e =>
        {
            Application.OpenURL(scenarios[3].HelpUrl);
        });        
        Root.Q<Button>("FlexHelp").RegisterCallback<ClickEvent>(e =>
        {
            Application.OpenURL(scenarios[4].HelpUrl);
        });
        
        deploymentRadio.RegisterValueChangedCallback(e =>
        {
            var targetScenario = ScenarioMappings[e.newValue];
            _model.ScenarioIndex = targetScenario;
            scenarioShortName = ScenarioShortNames[_model.ScenarioIndex];
            Root.Query<Foldout>("Parameters").Descendents<VisualElement>("BuildName").Descendents<TextField>().First().value = $"{Application.productName}-{scenarioShortName}-Build";
            Root.Q<Foldout>("Deploy").text = $"Deploy ({scenarioShortName} Fleet)";
        });        
        
        Root.Q<Foldout>("Parameters").text = $"{Application.productName} parameters";
        Root.Q<Foldout>("Deploy").text = $"Deploy ({scenarioShortName} Fleet)";
        
        Root.Query<Foldout>("Parameters").Descendents<VisualElement>("FleetName").Descendents<TextField>().First().value = $"{Application.productName}-ManagedFleet";
        Root.Query<Foldout>("Parameters").Descendents<VisualElement>("BuildName").Descendents<TextField>().First().value = $"{Application.productName}-{scenarioShortName}-Build";
        Root.Query<VisualElement>("BuildOS").Children<DropdownField>().First().RegisterValueChangedCallback(e =>
        {
            var osTarget = OSMappings[e.newValue];
            //TODO: Set OS Target parameter in stackformation template
        });
        
        var gameServerFolder = Root.Q<VisualElement>("GameServerFolder");
        gameServerFolder.Q<TextField>().value = _model.BuildFolderPath;
        gameServerFolder.Q<Button>().RegisterCallback<ClickEvent>((e) => 
        {
            var value = EditorUtility.OpenFolderPanel("Game Server Build Folder Path", Application.dataPath, "");
            _model.BuildFolderPath = value;
            gameServerFolder.Q<TextField>().value = _model.BuildFolderPath;
        });   
        
        var gameServerFile = Root.Q<VisualElement>("GameServerFile");
        gameServerFile.Q<TextField>().value = _model.BuildFilePath;
        gameServerFile.Q<Button>().RegisterCallback<ClickEvent>((e) => 
        {
            var value = EditorUtility.OpenFilePanel("Game Server Executable Path", Application.dataPath, "");
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
                var currentFoldout = AllFoldoutElements.FirstOrDefault(element => element.name == "DeploymentSelect");
                var targetFoldout = AllFoldoutElements.FirstOrDefault(element => element.name == "DeploymentSelectExpanded");
                ChangeFoldout(currentFoldout, targetFoldout);
                break;
            }
            case "CreateResource":
            {
                //TODO Disable this button until resource is created, then call code to create resource
                ToggleButtons(button, false);
                if (!_model.CanDeploy)
                {
                    break;
                }
                _model.StartDeployment(ConfirmChangeSet)
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Debug.LogException(task.Exception);
                        }
                    });
                break;
            }
            case "RedeployResource":
            {
                //TODO Only show this button when we are deployed, button will redeploy resource
                ToggleButtons(button, false);
                break;
            }
            case "DeleteResource":
            {
                //TODO Only show this button when we are deployed, delete resource
                ToggleButtons(button, false);
                _model.DeleteDeployment();
                _waiter.WaitUntilDone(_model).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogException(task.Exception);
                    }
                    else
                    {
                        _model.RefreshCurrentStackInfo(); 
                    }
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
    
    protected Action DefaultAction { get; set; }
    
    private string[] _changes;
    private string[] _changeCount;
    private Waiter _waiter = new Waiter();

    protected TextProvider TextProvider { get; private set; }
    
    public Task<bool> SetUp()
    {
        var completionSource = new TaskCompletionSource<bool>();
        DefaultAction = () => completionSource.TrySetResult(false);
        return completionSource.Task;
    }
    
    private Task<bool> ConfirmChangeSet(ConfirmChangesRequest request)
    {
        _stackUpdateModelFactory.Create(request);
        return SetUp();
    }

}
