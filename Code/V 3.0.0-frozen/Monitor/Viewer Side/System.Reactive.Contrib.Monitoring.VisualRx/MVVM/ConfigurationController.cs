#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Contrib.Monitoring.UI.Properties;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Reflection;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// MVC controller hold the view model and
    /// get invoked from the service
    /// </summary>
    /// <example>
    /// Command-line Sample: PluginFolder:"C:\tmp" PluginFolder:"Plug-ins\Data"
    /// </example>
    internal static class ConfigurationController
    {
        private const string FILE_NAME = "VisualRx.settings";
        private const string FILE_KEY = "VisualRx.settings.x";
        private const string KEY_STORAGE = "VisualRx.key";
        private const string IV_STORAGE = "VisualRx.iv";
        private static readonly string FILE_PATH;
        private static readonly string FILE_KEY_PATH;

        private static readonly Aes _cryptoAlgorithm;

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ConfigurationController"/> class.
        /// </summary>
        static ConfigurationController()
        {
            try
            {
                string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                location = Path.GetDirectoryName(location);
                FILE_PATH = Path.Combine(location, FILE_NAME);
                FILE_KEY_PATH = Path.Combine(location, FILE_KEY);

                Value = new Configuration();
               _cryptoAlgorithm = AesManaged.Create();
                SetCryptoKeys();
                Load();
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("Fail to create the ConfigurationController: {0}", ex);
            }

            #endregion // Exception Handling
        }

        #region SetCryptoKeys

        /// <summary>
        /// Sets the crypto keys.
        /// </summary>
        private static void SetCryptoKeys()
        {
            #region var prm = new CspParameters (...)

            var prm = new CspParameters
            {
                KeyContainerName = FILE_NAME,
                Flags = CspProviderFlags.NoPrompt |
                        CspProviderFlags.UseNonExportableKey |
                        CspProviderFlags.UseMachineKeyStore
            };

            #endregion // var prm = new CspParameters (...)
            var cryptoStorage = new RSACryptoServiceProvider(prm)
            {
                PersistKeyInCsp = true,
            };

            #region Restore the secure key

            bool keyInfoExists = File.Exists(FILE_KEY_PATH);
            if (keyInfoExists)
            {
                try
                {
                    var fs = File.OpenRead(FILE_KEY_PATH);
                    using (var br = new BinaryReader(fs))
                    {
                        // read the symmetric key
                        int len = br.ReadInt32();
                        byte[] k = br.ReadBytes(len);
                        _cryptoAlgorithm.Key = cryptoStorage.Decrypt(k, true);
                        len = br.ReadInt32();
                        byte[] iv = br.ReadBytes(len);
                        _cryptoAlgorithm.IV = cryptoStorage.Decrypt(iv, true);
                    }
                }
                #region Exception Handling

                catch (CryptographicException ex)
                {
                    TraceSourceMonitorHelper.Warn("Fail to decrypt the symmetric key: {0}", ex);
                    keyInfoExists = false;
                    string date = DateTime.Now.ToString("yyyyMMdd.HHmmss");
                    File.Move(FILE_KEY_PATH, FILE_KEY_PATH + "." + date);
                    File.Move(FILE_PATH, FILE_PATH + "." + date);
                }

                #endregion // Exception Handling
            }
            if(!keyInfoExists)
            {
                // regenerate and save symmetric key
                byte[] k = cryptoStorage.Encrypt(_cryptoAlgorithm.Key, true);
                byte[] iv = cryptoStorage.Encrypt(_cryptoAlgorithm.IV, true);
                var fs = File.OpenWrite(FILE_KEY_PATH);
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(k.Length);
                    bw.Write(k);
                    bw.Write(iv.Length);
                    bw.Write(iv);
                }
            }

            #endregion // Restore the secure key
        }

        #endregion // SetCryptoKeys

        #endregion // Ctor

        public static event EventHandler ValueChanged = (s, e) => { };

        #region Value

        private static Configuration _value;
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public static Configuration Value
        {
            get { return _value; }
            private set
            {
                _value = value;
                ValueChanged(null, EventArgs.Empty);
            }
        }

        #endregion // Value

        #region Load

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        public static void Load()
        {
            if (!File.Exists(FILE_PATH))
            {
                Save();
                return;
            }

            try
            {
                var ser = new NetDataContractSerializer();
                var fs = File.OpenRead(FILE_PATH);
                using (var crypto = new CryptoStream(fs, _cryptoAlgorithm.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    Value = ser.Deserialize(crypto) as Configuration;
                }
            }
            #region Exception Handling

            catch (Exception ex)
            {
                string date = DateTime.Now.ToString("yyyyMMdd.HHmmss");
                File.Move(FILE_PATH, FILE_PATH + "." + date);
                Value = new Configuration();
                Save();
                TraceSourceMonitorHelper.Error("Fail to load the Configuration: {0}", ex);
            }

            #endregion // Exception Handling
        }

        #endregion // Load

        #region Save

        /// <summary>
        /// Saves the configuration.
        /// </summary>
        /// <returns></returns>
        public static Exception Save()
        {
            try
            {
                ObservableCollection<DiscoveryPath> paths = Value.PluginDiscoveryPaths;
                #region Validation

                for (int i = paths.Count - 1; i >= 0; i--)
                {
                    if (string.IsNullOrWhiteSpace(paths[i].Path))
                        paths.RemoveAt(i);
                }

                #region Validation

                if (File.Exists(FILE_PATH))
                {
                    File.Delete(FILE_PATH);
                }

                #endregion // Validation

                #endregion // Validation

                var ser = new NetDataContractSerializer();
                var fs = File.OpenWrite(FILE_PATH);
                using (var crypto = new CryptoStream(fs, _cryptoAlgorithm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    ser.Serialize(crypto, Value);
                }
                return null;
            }
            #region Exception Handling

            catch (Exception ex)
            {
                try
                {
                    File.Move(FILE_PATH, FILE_PATH + "." + DateTime.Now.ToString("yyyy-MM-dd HHmmss"));
                }
                #region Exception Handling

                catch { }

                #endregion // Exception Handling
                TraceSourceMonitorHelper.Error("Fail to Save the Configuration: {0}", ex);
                return ex;
            }

            #endregion // Exception Handling
        }

        #endregion // Save
    }
}