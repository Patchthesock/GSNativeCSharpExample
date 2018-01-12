using System;
//using System.Configuration;
using System.Collections.Generic;
using System.IO;
using GameSparks;
using GameSparks.Core;

#if __IOS__
using Foundation;
#elif __ANDROID__
using Android.App;
#elif __WINDOWS__
//using Xamarin.Forms;
using System.Threading;
using Windows.Storage;
#endif


namespace GameSparksRealTime
{
    public class GSPlatform : IGSPlatform
    {
#if !__WINDOWS__
        const string dataPath = "PersistantData.xml";
        KeyValueStore dataStore;
#endif

        private DispatchAdapter dispatchAdapter;

#if __IOS__
        public GSPlatform (NSObject owner)
#elif __ANDROID__
        public GSPlatform(Activity activity)
#else
        public GSPlatform(string apiKey, string apiSecret, string apiCredential)
#endif
        {
#if !__WINDOWS__
            ApiKey = apiKey;
            ApiSecret = apiSecret;
            ApiCredential = apiCredential;
            string newDataPath = Path.Combine(PersistentDataPath, dataPath);
#endif

#if __IOS__
            dispatchAdapter = new DispatchAdapter (owner);
#elif __ANDROID__
            dispatchAdapter = new DispatchAdapter(activity);
#elif __WINDOWS__
            dispatchAdapter = new DispatchAdapter();
#endif
            dispatchAdapter = new DispatchAdapter();
#if !__WINDOWS__
            dataStore = new KeyValueStore();
            try
            {
                if (File.Exists(newDataPath))
                {
                    using (var stream = File.OpenRead(newDataPath))
                    {
                        dataStore = KeyValueStore.Read(stream);
                    }
                }
            }
            catch
            {
            }
#endif
        }

        public void MessageReceived(IDictionary<String, object> message)
        {
#if __WINDOWS__
            //System.Diagnostics.Debug.WriteLine("MessageReceived:" + message["@class"].ToString());
#else
            //Console.WriteLine("MessageReceived:" + message["@class"].ToString());
#endif
        }

        public void StorePersistantData(String key, String data)
        {
#if __WINDOWS__
            var localSettings = ApplicationData.Current.LocalSettings;

            localSettings.Values[key] = data;
#else
            dataStore.Set(key, data);

            string newDataPath = Path.Combine(PersistentDataPath, dataPath);

            using (var stream = File.OpenWrite(newDataPath))
            {
                dataStore.Serialize(stream);
            }
#endif
        }

        public string LoadPersistantData(string key)
        {
#if __WINDOWS__
            var localSettings = ApplicationData.Current.LocalSettings;

            return (String)localSettings.Values[key];
#else
            var value = dataStore.Get(key);

            return value;
#endif
        }

        public String AuthToken
        {
            get
            {
                return LoadPersistantData("authToken");
            }
            set
            {
                StorePersistantData("authToken", value);
            }
        }


        public String UserId
        {
            get
            {
                return LoadPersistantData("playerId");
            }
            set
            {
                StorePersistantData("playerId", value);
            }
        }

        int _requestTimeoutSeconds = 5;

        public int RequestTimeoutSeconds
        {
            get { return _requestTimeoutSeconds; }
            set { _requestTimeoutSeconds = value; }
        }

        public String DeviceName
        {
            get
            {
                return "ConsoleTest";
            }
        }

        public String DeviceType
        {
            get
            {
                return "Console";
            }
        }

        public GSData DeviceStats
        {
            get
            {
                return new GSData();
            }
        }

        public String DeviceId
        {
            get
            {
                return "TestDevice";
            }
        }

        public String DeviceOS
        {
            get
            {
#if __IOS__
                return "iOS";
#elif __ANDROID__
                return "Android";
#elif __WINDOWS__
                return "Windows";
#endif
                return "Windows";
            }
        }

        public String Platform
        {
            get
            {
#if __IOS__
                return "iOS";
#elif __ANDROID__
                return "Android";
#elif __WINDOWS__
                return "Windows";
#endif
                return "Windows";
            }
        }

        public bool ExtraDebug
        {
            get
            {
                return true;
            }
        }

        public string ApiKey { get; private set; }
        public string ApiSecret { get; private set; }
        public string ApiCredential { get; private set; }

        public String ApiDomain { get { return null; } }

        public String ApiStage
        {
            get
            {
                return "preview";
            }
        }


        public void DebugMsg(String message)
        {
#if __WINDOWS__
            System.Diagnostics.Debug.WriteLine(Environment.CurrentManagedThreadId + " " + message);
#else
            //Console.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId + " " + message);
#endif
        }

        public void ExecuteOnMainThread(Action action)
        {
#if __IOS__ || __ANDROID__ || __WINDOWS__
            dispatchAdapter.Invoke(action);
#else
            action();
#endif
        }


        public String SDK
        {
            get
            {
                return "Xamarin Test";
            }
        }

        public string PersistentDataPath
        {
            get
            {
#if __IOS__ || __ANDROID__
                return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#elif __WINDOWS__
                return ApplicationData.Current.LocalFolder.Path;
#else
                return ".";
#endif
            }
        }

        public IGameSparksTimer GetTimer()
        {
            return new GameSparksTimer();
        }

        public string MakeHmac(string stringToHmac, string secret)
        {
            return GameSparksUtil.MakeHmac(stringToHmac, secret);
        }

        public IGameSparksWebSocket GetSocket(string url, Action<string> messageReceived, Action closed, Action opened, Action<string> error)
        {
            var websocket = new GameSparksWebSocket();
            websocket.Initialize(url, messageReceived, closed, opened, error);
            return websocket;
        }

        private class DispatchAdapter
        {
#if __IOS__
            public readonly NSObject owner;

            public DispatchAdapter(NSObject owner)
            {
                this.owner = owner;
            }

            public void Invoke(Action action)
            {
                owner.BeginInvokeOnMainThread(new Action(action));
            }
#elif __ANDROID__
            public readonly Activity owner;

            public DispatchAdapter(Activity owner)
            {
                this.owner = owner;
            }

            public void Invoke(Action action)
            {
                owner.RunOnUiThread(action);
            }
#elif __WINDOWS__
            private SynchronizationContext synchronizationContext;

            public DispatchAdapter()
            {
                synchronizationContext = SynchronizationContext.Current;
            }

            public void Invoke(Action action)
            {
                //Device.BeginInvokeOnMainThread(action);
            
                synchronizationContext.Post((obj) => {
                    try
                    {
                        action();
                    }
                    catch (Exception exception)
                    {                   
                    }
                }, null);
            }
#endif
        }
    }
}

