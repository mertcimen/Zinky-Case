using System.ComponentModel;
using Base_Systems.Scripts.Managers;
using SRF.Service;
using UnityEngine;
using UnityEngine.Scripting;

public delegate void SROptionsPropertyChanged(object sender, string propertyName);

#if !DISABLE_SRDEBUGGER
[Preserve]
#endif
public partial class SROptions : INotifyPropertyChanged
{
    private int _targetLevel = 1;

    private static SROptions _current;

    public static SROptions Current
    {
        get { return _current; }
    }

#if !DISABLE_SRDEBUGGER
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void OnStartup()
    {
        _current = new SROptions(); // Need to reset options here so if we enter play-mode without a domain reload there will be the default set of options.
        SRServiceManager.GetService<SRDebugger.Internal.InternalOptionsRegistry>().AddOptionContainer(Current);
    }
#endif

    public event SROptionsPropertyChanged PropertyChanged;
    
#if UNITY_EDITOR
    [JetBrains.Annotations.NotifyPropertyChangedInvocator]
#endif
    public void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, propertyName);
        }

        if (InterfacePropertyChangedEventHandler != null)
        {
            InterfacePropertyChangedEventHandler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private event PropertyChangedEventHandler InterfacePropertyChangedEventHandler;

    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
        add { InterfacePropertyChangedEventHandler += value; }
        remove { InterfacePropertyChangedEventHandler -= value; }
    }
    [Category("Game Controls")]
    public void GoToNextLevel()
    {
        LevelManager.Instance.LoadNextLevel();
    }
    [Category("Game Controls")]
    public void GoToBackLevel()
    {
        if(LevelManager.Instance.LevelNo!=1)
            LevelManager.Instance.LoadBackLevel();
    }
    [Category("Game Controls")]
    public void ResetGame()
    {
        PlayerPrefs.DeleteAll();
        LevelManager.Instance.RestartFromFirstLevel();
    }
    
    
    
    [Category("Game Controls")]
    public int TargetLevel
    {
        get { return _targetLevel; }
        set
        {
            if (_targetLevel == value) return;
            _targetLevel = value;
            OnPropertyChanged(nameof(TargetLevel));
        }
    }
    
    [Category("Game Controls")]
    public void GoToTargetLevel()
    {
        if(_targetLevel<1)
            return;
        LevelManager.Instance.LoadTargetLevel(_targetLevel);
    }
}
 