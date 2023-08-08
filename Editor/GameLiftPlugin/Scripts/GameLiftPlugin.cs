using System;
using System.Collections.Generic;
using System.Linq;
using AmazonGameLift.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GameLiftPlugin : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [SerializeField]
    public Texture2D cursor;
    
    private VisualElement root;
    public List<Tab> AllTabs = new();
    public List<VisualElement> TabMenus;
    public VisualElement CurrentTab;
    private List<Button> _buttons = new();

    private readonly Color _focusColor = new(0.172549f, 0.3647059f, 0.5294118f, 1);

    public readonly AwsCredentialsCreation CreationModel;
    public readonly AwsCredentialsUpdate UpdateModel;
    private AwsCredentials _awsCredentials;
    public string[] allProfileNames;

    public State CurrentState = new();

    private GameLiftPlugin()
    {
        _awsCredentials = AwsCredentialsFactory.Create();
        CreationModel = _awsCredentials.Creation;
        UpdateModel = _awsCredentials.Update;
        _awsCredentials.SetUp();

    }

    
    
    [MenuItem("GameLift/GameLift Configuration")]
    public static void ShowWindow()
    {
        var window = GetWindow<GameLiftPlugin>();
        window.titleContent = new GUIContent("GameLift Plugin");
        
    }

    private void CreateGUI()
    {
        root = rootVisualElement;
        VisualElement uxml = m_VisualTreeAsset.Instantiate();
        root.Add(uxml);
        
        SetupLinks();
        DisableDefaultButtons();
        SetupTextFieldEvents();
        SetupTabMenu();
        SetupProfiles();
        SetupTabs();
    }

    private void TESTADDERROR(Tab tab)
    {
        //root.Add(tab.BuildInfoBox(Tab.InfoType.Info, "sdasdas", new Button()));
    }

    private void SetupProfiles()
    {
        CurrentState.AllProfiles = UpdateModel.AllProlfileNames;
    }

    private void SetupLinks()
    {
        var labels = root.Query<VisualElement>(null,"link").ToList();
        foreach (var label in labels)
        {
            label.RegisterCallback<MouseUpEvent,VisualElement>(OnLinkClicked,label);
        }
    }

    private void DisableDefaultButtons()
    {
        var allButtons = root.Query<Button>(null, "DefaultDisabled").ToList();
        foreach (var button in allButtons)
        {
            button.SetEnabled(false);
        }
    }

    private void SetupTextFieldEvents()
    {
        var allTextFields  = root.Query<TextField>().ToList();
        foreach (var textField in allTextFields)
        {
            textField.RegisterValueChangedCallback(OnTextChangeButton);
        }
    }

    private void SetupTabs()
    {
        foreach (var tab in TabMenus)
        {
            switch (tab.name)
            {
                case "AmazonGameLiftTab":
                {
                    var awsGameLiftTab = new AmazonGameLiftTab(root, this);
                    AllTabs.Add(awsGameLiftTab);
                    TESTADDERROR(awsGameLiftTab);
                    break;
                }
                case "AWSAccountCredentialsTab":
                {
                    var awsCredentialsTab = new AWSCredentialsTab(root, this);
                    AllTabs.Add(awsCredentialsTab);
                    break;
                }
                case "GameLiftAnywhereTab":
                {
                    var gameLiftAnywhereTab = new GameLiftAnywhereTab(root, this);
                    AllTabs.Add(gameLiftAnywhereTab);
                    break;
                }
                case "ManagedEC2Tab":
                {
                    var managedEc2Tab = new ManagedEC2Tab(root, this);
                    AllTabs.Add(managedEc2Tab);
                    break;
                }
            }
        }
    }
    
    private void SetupTabMenu()
    {
        var tabButtons = root.Query<Button>(null, "TabButton").ToList();
        foreach (var button in tabButtons)
        {
            button.RegisterCallback<ClickEvent, Button>(OnTabButtonPress, button);
            button.RegisterCallback<MouseOverEvent, Button>(OnTabButtonHover, button);
            button.RegisterCallback<MouseLeaveEvent, Button>(OnTabButtonHover, button);
            _buttons.Add(button);
        }

        _buttons[0].style.backgroundColor = _focusColor;
        TabMenus = root.Query<VisualElement>(null, "TabMenu").ToList();
        CurrentTab = TabMenus[0];
    }
    
    private void OnTabButtonPress(ClickEvent evt, Button button)
    {
        var targetTab = TabMenus.FirstOrDefault(tabMenu => tabMenu.name == button.name + "Tab");
        ChangeTab(button, targetTab);
    }

    private void OnTabButtonHover(MouseOverEvent evt, Button button)
    {
        if (button.style.backgroundColor != _focusColor)
        {
            button.style.backgroundColor = new StyleColor(Color.grey);
        }
    }
    
    private void OnTabButtonHover(MouseLeaveEvent evt, Button button)
    {
        if (button.style.backgroundColor != _focusColor)
        {
            button.style.backgroundColor = new StyleColor(Color.clear);
        }
    }

    private void OnLinkClicked(MouseUpEvent evt, VisualElement linkLabel)
    {
        if (linkLabel.tooltip != "")
        {
            Application.OpenURL(linkLabel.tooltip);
        }
        else
        {
            Debug.Log("Link clicked But still needs link in tooltip");
        }
    }

    public void ChangeTab(Button targetButton, VisualElement targetTab)
    {
        if (CurrentTab != null)
        {
            CurrentTab.style.display = DisplayStyle.None;
            foreach (var button in _buttons)
            {
                button.style.backgroundColor = new StyleColor(Color.clear);
            }
        }
        CurrentTab = targetTab;
        if (CurrentTab != null)
        {
            CurrentTab.style.display = DisplayStyle.Flex;
            targetButton.style.backgroundColor = new StyleColor(_focusColor);
        }
    }
    
    private void OnTextChangeButton(ChangeEvent<string> evt)
    {
        var fieldName = "";
        var words = evt.currentTarget.ToString().Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length >= 2)
        {
            fieldName =  words[1];
        }
        fieldName = fieldName.Substring(0, fieldName.Length-5);
        
        var button = root.Q<Button>(fieldName);
        button?.SetEnabled(true);
    }
    
    
}

public struct State
{
    public bool SelectedBootstrapped;
    public string[] AllProfiles;
    public string SelectedProfile;
    public bool ActiveFleet;
}
