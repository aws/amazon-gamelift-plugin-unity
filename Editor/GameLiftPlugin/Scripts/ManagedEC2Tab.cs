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
    }
    
    private void OnAnySettingChanged()
    {
        _model.Refresh();
    }

    private void UpdateStatus()
    {
        //_model.CurrentStackInfo.Status deployment status
        
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
        var tabName = "Tab4";
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
