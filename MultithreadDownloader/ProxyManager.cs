using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace MultithreadDownloader
{
    public class ProxyManager
    {
        private List<string> Addresses = new List<string>();
        private int index;
        private string Address;
        private DownloadThread IssuerThread;
        private bool OutOfProxies;
        public enum ProxyDistributionStates
        {
            NoProxy = 0,
            Single = 1,
            SingleSwithching = 2,
            Multiple = 3,
            MultipleCycle = 4
        }
        public enum OutOfProxyBehaviourStates
        {
            UseLastUsedProxy=0,
            StartOver=1,
            DontUseProxy=2
        }
        private ProxyDistributionStates DistributorBehaviour;
        private OutOfProxyBehaviourStates OutOfProxyBehaviour;
        public ProxyManager(ProxyConfiguration proxConfig, List<string> addresses = null)
        {
            Addresses=addresses;
            DistributorBehaviour =proxConfig.DistributionPolicy;
            OutOfProxyBehaviour= proxConfig.OutOfProxyBehaviour;
            
        }


        public string GetProxy(DownloadThread IssuerThread) {

            if (IssuerThread.ReconnectCount < IssuerThread.MaxReconnect & IssuerThread.Proxy!="") return IssuerThread.Proxy;
            IssuerThread.ReconnectCount = 0;
            switch (DistributorBehaviour)
            {
                case ProxyDistributionStates.NoProxy:
                    {
                        return "";
                    }
                    case ProxyDistributionStates.Single:
                    {
                        return Addresses[index];
                    }
                    case ProxyDistributionStates.Multiple:
                    {
                        if (OutOfProxies)
                        {
                            return ExecNoProxiesBehaviour(IssuerThread);
                        }
                        else
                        {
                            Address = Addresses[index];
                            Switch();
                            return Address;
                        }

                    }
                    case ProxyDistributionStates.MultipleCycle:
                    {
                        if (OutOfProxies)
                        {
                            index = 0;
                        }
                        Address = Addresses[index];
                        Switch();
                        return Address;
                    }
                default: return "";
            }
            
        }
        private string ExecNoProxiesBehaviour(DownloadThread IssuerThread)
        {
            switch (OutOfProxyBehaviour)
            {
                case OutOfProxyBehaviourStates.UseLastUsedProxy:
                    {
                        return IssuerThread.Proxy;
                    }
                case OutOfProxyBehaviourStates.StartOver:
                    {
                        index = 0;
                        Address = Addresses[index];
                        Switch();
                        OutOfProxies = false;
                        return Address;
                    }
                case OutOfProxyBehaviourStates.DontUseProxy:
                    {
                        return "";
                    }
                default: return "";
            }
        }
        private void Switch() 
        { 
            index++;
            if (index > Addresses.Count - 1)
            {
                OutOfProxies=true;
            }
        }


    }

    public class ProxyDistributionOption
    {
        public ProxyDistributionOption(string displayName, ProxyManager.ProxyDistributionStates value)
        {
            DisplayName = displayName;
            Value = value;
        }
        public ProxyManager.ProxyDistributionStates Value { get; set; }
        public string DisplayName { get; set; }

        public override string ToString() => DisplayName;

    }

    public class OutOfProxyBehaviourOption
    {
        public OutOfProxyBehaviourOption(string displayName, ProxyManager.OutOfProxyBehaviourStates value)
        {
            DisplayName = displayName;
            Value = value;
        }
        public ProxyManager.OutOfProxyBehaviourStates Value { get; set; }
        public string DisplayName { get; set; }
        public override string ToString() => DisplayName;
    }

    public class ProxyConfiguration
    {
        public ProxyManager.ProxyDistributionStates DistributionPolicy { get; set; }
        public ProxyManager.OutOfProxyBehaviourStates OutOfProxyBehaviour { get; set; }
    }
}
