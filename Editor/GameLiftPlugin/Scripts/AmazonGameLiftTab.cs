using AmazonGameLift.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AmazonGameLiftTab : Tab
{
    private readonly GameLiftPlugin _gameLiftConfig;

    public AmazonGameLiftTab(VisualElement root, GameLiftPlugin gameLiftConfig)
    {
        _gameLiftConfig = gameLiftConfig;
        Root = root;
        TabNumber = 1;
        SetupTab();
    }


    private void SetupTab()
    {
        var tabName = "Tab1";
        base.SetupTab(tabName, OnTabButtonClicked);
    }
    
    
    private void OnTabButtonClicked(ClickEvent evt, Button button)
    {
        switch (button.name)
        {
            case "AddProfile":
            {
                var targetTab = _gameLiftConfig.TabMenus[1];
                _gameLiftConfig.ChangeTab(button,targetTab);
                break;
            }
            case "DownloadSampleGame":
            {
                var filePackagePath = $"Packages/{Paths.PackageName}/{Paths.SampleGameInPackage}";
                AssetDatabase.ImportPackage(filePackagePath, interactive: true);
                break;
            }
        }
    }
    
}
