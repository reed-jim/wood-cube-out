using System;
using System.Collections.Generic;
using Falcon.FalconCore.Scripts.Logs;
using Falcon.FalconCore.Scripts.Utils.Generics;
using UnityEngine;

namespace Falcon.FalconCore.Scripts.Services.GameObjs
{
    public class FGameObj : MonoBehaviour
    {
        private static FGameObj _instance;

        private bool gameStop;
        private List<IPioneerService> pioneerServices;
        private List<ITerminalService> terminalServices;

        protected List<IPioneerService> PioneerServices =>
            pioneerServices ??= FGenerics.GetInstances<IPioneerService>();

        protected List<ITerminalService> TerminalServices =>
            terminalServices ??= FGenerics.GetInstances<ITerminalService>();

        public static FGameObj Instance
        {
            get
            {
                if (_instance == null)
                {
                    var gObject = GameObject.Find("Falcon");
                    if (gObject == null) gObject = new GameObject("Falcon");

                    _instance = gObject.GetComponent<FGameObj>();
                    if (_instance == null) _instance = gObject.AddComponent<FGameObj>();
                    _instance.enabled = true;
                    if (Application.isPlaying) DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        public static bool ApplicationRunning { get; private set; }

        private void Awake()
        {
            ApplicationRunning = true;
        }

        public void Update()
        {
            try
            {
                OnUpdate?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception e)
            {
                CoreLogger.Instance.Error(e);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (Application.isPlaying && !hasFocus)
                CheckGameStop();
            else if (Application.isPlaying) CheckGameContinue();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (Application.isPlaying && pauseStatus)
                CheckGameStop();
            else if (Application.isPlaying) CheckGameContinue();
        }

        private void OnApplicationQuit()
        {
            ApplicationRunning = false;
            CheckGameStop();
        }

        public T AddIfNotExist<T>() where T : MonoBehaviour
        {
            var val = gameObject.GetComponent<T>();
            if (val == null) val = gameObject.AddComponent<T>();
            return val;
        }

        public Component AddIfNotExist(Type type)
        {
            var val = gameObject.GetComponent(type);
            if (val == null) val = gameObject.AddComponent(type);
            return val;
        }

        public static event EventHandler OnGameStop;
        public static event EventHandler OnGameContinue;
        public static event EventHandler OnUpdate;

        private void CheckGameStop()
        {
            if (!gameStop)
                gameStop = true;
            else
                return;

            CoreLogger.Instance.Info("On Game Stop");

            try
            {
                OnGameStop?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception e)
            {
                CoreLogger.Instance.Error(e);
            }

            foreach (var terminalService in TerminalServices)
                try
                {
                    terminalService.OnPostStop();
                }
                catch (Exception e)
                {
                    CoreLogger.Instance.Error(e);
                }
        }

        private void CheckGameContinue()
        {
            if (gameStop)
                gameStop = false;
            else
                return;
            CoreLogger.Instance.Info("On Game Continue");

            foreach (var pioneerService in PioneerServices)
                try
                {
                    pioneerService.OnPreContinue();
                }
                catch (Exception e)
                {
                    CoreLogger.Instance.Error(e);
                }

            try
            {
                OnGameContinue?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception e)
            {
                CoreLogger.Instance.Error(e);
            }
        }
    }
}