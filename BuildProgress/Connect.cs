using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Drawing;

namespace BuildProgress
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2
	{
		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
            
		}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;

            _applicationObject.Events.BuildEvents.OnBuildBegin += OnBuildBegin;
            _applicationObject.Events.BuildEvents.OnBuildProjConfigDone += OnBuildProjConfigDone;
            _applicationObject.Events.BuildEvents.OnBuildDone += OnBuildDone;
		}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
		
		private DTE2 _applicationObject;
		private AddIn _addInInstance;
        private int _projectProgressPercentagePoints;
        private int _currentProgressValue;
        private int _maxProgressValue;

        private bool _buildErrorDetected;

        private void OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            var _numberOfProjects = _applicationObject.Solution.Projects.Count;

            _projectProgressPercentagePoints = (int)Math.Ceiling((decimal)100 / _numberOfProjects);
            _currentProgressValue = 0;
            _maxProgressValue = _projectProgressPercentagePoints * _numberOfProjects;

            UpdateProgressValue(false);
        }

        private void OnBuildProjConfigDone(string Project, string ProjectConfig, string Platform, string SolutionConfig, bool Success)
        {
            UpdateProgressValue(!Success);
        }

        private void UpdateProgressValue(bool errorThrown)
        {
            if (_currentProgressValue < 0)
                _currentProgressValue = 0;

            if (_currentProgressValue > _maxProgressValue)
                _currentProgressValue = _maxProgressValue;

            TaskbarManager.Instance.SetProgressValue(_currentProgressValue, _maxProgressValue);
            if (errorThrown)
            {
                _buildErrorDetected = true;
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error);
            }

            _currentProgressValue += _projectProgressPercentagePoints;
        }

        private void OnBuildDone(vsBuildScope scope, vsBuildAction action)
        {
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            if (_buildErrorDetected)
            {
                TaskbarManager.Instance.SetOverlayIcon((Icon)Resources.ResourceManager.GetObject("cross"), "Build Failed");
            }
            else
            {
                TaskbarManager.Instance.SetOverlayIcon((Icon)Resources.ResourceManager.GetObject("accept"), "Build Succeeded");
            }
        }
	}
}