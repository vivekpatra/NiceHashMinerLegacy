﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Interfaces;

namespace NiceHashMiner.Configs {
    [Serializable]
    public abstract class BaseConfigFile<T> : IPathsProperties where T : class, IPathsProperties {
        #region Members
        [field: NonSerialized]
        readonly public static string CONF_FOLDER = @"configs\";

        private string _filePath = "";
        private string _filePathOld = "";

        [JsonIgnore]
        public string FilePath {
            get { return _filePath.Contains(CONF_FOLDER) ? _filePath : CONF_FOLDER + _filePath; }
            set { _filePath = value; }
        }
        [JsonIgnore]
        public string FilePathOld {
            get { return _filePathOld.Contains(CONF_FOLDER) ? _filePathOld : CONF_FOLDER + _filePathOld; }
            set { _filePathOld = value; }
        }

        [JsonIgnore]
        private bool FileLoaded { get { return _self != null; } }

        [field: NonSerialized]
        protected T _self;

        #endregion //Members

        public void InitializeConfig() {
            InitializePaths();
            ReadFile();
            if (FileLoaded) {
                _self.FilePath = this.FilePath;
                _self.FilePathOld = this.FilePathOld;
                InitializeObject();
            }
        }

        /// <summary>
        /// InitializePaths should be overrided in the subclass to specify filepath(old) paths.
        /// </summary>
        abstract protected void InitializePaths();
        /// <summary>
        /// InitializeObject must be overrided in the subclass to reinitialize values and references from the configuration files.
        /// Use the _self member and reinitialize all non null references (use DeepCopy or plain reference it is up to the implementor).
        /// IMPORTANT!!! Take extra care with arrays, lists and dictionaries, initialize them manually and not by DeepCopy or reference, the 
        /// reason for this is to be future proof if new keys/values are added so reinitialize them one by one if they exist.
        /// </summary>
        abstract protected void InitializeObject();

        private static void CheckAndCreateConfigsFolder () {
            try {
                if (Directory.Exists(CONF_FOLDER) == false) {
                    Directory.CreateDirectory(CONF_FOLDER);
                }
            } catch { }
        }

        protected void InitReferenceType<RefT>(ref RefT toInit, RefT fromInit) where RefT : IComparable<RefT> {
            if (fromInit != null) {
                toInit = fromInit;
            }
        }

        protected void InitDictionaryType<TKey, TValue>(ref IDictionary<TKey, TValue> toInit, IDictionary<TKey, TValue> fromInit) {
            if (fromInit != null) {
                foreach (var key in fromInit.Keys) {
                    if (toInit.ContainsKey(key)) {
                        toInit[key] = fromInit[key];
                    } else {
                        // TODO think if we let tamnpered data
                    }
                }
            }
        }

        protected void ReadFile() {
            CheckAndCreateConfigsFolder();
            try {
                if (new FileInfo(FilePath).Exists) {
                    _self = JsonConvert.DeserializeObject<T>(File.ReadAllText(FilePath));
                } else {
                    Commit();
                }
            } catch(Exception ex) {
                Helpers.ConsolePrint("BaseConfigFile", String.Format("ReadFile {0}: exception {1}", FilePath, ex.ToString()));
            }
        }

        public void Commit() {
            try {
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch (Exception ex) {
                Helpers.ConsolePrint("BaseConfigFile", String.Format("Commit {0}: exception {1}", FilePath, ex.ToString()));
            }
        }

        public bool ConfigFileExist() {
            return File.Exists(FilePath);
        }


    }
}
