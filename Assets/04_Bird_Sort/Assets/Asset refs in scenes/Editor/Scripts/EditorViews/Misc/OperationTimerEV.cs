#if UNITY_EDITOR

using System;
using UnityEditor;
using System.Diagnostics;

namespace SearchEngine.EditorViews
{
    public class OperationTimerEV : IEditorView
    {
        protected Stopwatch timer = new Stopwatch();
        private string titleTemplate = @"Operation time {0:hh\\:mm\\:sss}";
        private TimeSpan allTime = TimeSpan.Zero;

        public bool IsRunning()
        {
            return timer.IsRunning;
        }   
                 
        public void ShowGUI ()
        {
            string notification;
            if(timer.IsRunning)
                notification = string.Format(titleTemplate, timer.Elapsed);
            else
                notification = string.Format(titleTemplate, allTime);
            EditorGUILayout.LabelField(notification.Substring(0, notification.Length>=26?26:23)); //^_^//
        }
        
        public void ReStart()
        {
            timer.Reset();
            timer.Start();
        }
        
        public void Stop()
        {
            if (timer.IsRunning)
            {
                timer.Stop();
                allTime = timer.Elapsed;
                timer.Reset();
            }
        } 
    }
}

#endif