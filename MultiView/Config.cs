using System;
using System.IO;
using UnityEngine;

namespace MultiView
{
    public class Config
    {
        public float fov = 90;
        public float positionSmooth = 10;
        public float rotationSmooth = 5;
        public bool multiView = true;
        public float multiViewPosX = 0;
        public float multiViewPosY = 0;
        public float multiViewWidth = .33f;
        public float multiViewHeight = .33f;

        public event Action<Config> ConfigChangedEvent;

        private readonly FileSystemWatcher _configWatcher;
        private bool _saving;

        private string FilePath;

        public Config(string filePath)
        {
            FilePath = filePath;

            if (File.Exists(FilePath))
            {
                Load();
                var text = File.ReadAllText(FilePath);
            }
            else
            {
                Save();
            }

            _configWatcher = new FileSystemWatcher(Environment.CurrentDirectory)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "MultiView.cfg",
                EnableRaisingEvents = true
            };
            _configWatcher.Changed += ConfigWatcherOnChanged;
        }

        ~Config()
        {
            _configWatcher.Changed -= ConfigWatcherOnChanged;
        }

        public void Save()
        {
            _saving = true;
            ConfigSerializer.SaveConfig(this, FilePath);
        }

        public void Load()
        {
            ConfigSerializer.LoadConfig(this, FilePath);
        }

        private void ConfigWatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            if (_saving)
            {
                _saving = false;
                return;
            }

            Load();

            if (ConfigChangedEvent != null)
            {
                ConfigChangedEvent(this);
            }
        }
    }
}