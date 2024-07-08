using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace MultithreadDownloader
{
    internal class ProxyManager
    {
        private List<string> Addresses = new List<string>();
        private int index;
        private string Address;
        private DownloadThread IssuerThread;
        private bool OutOfProxies;
        enum ProxyDistributionStates
        {
            NoProxy = 0,
            Single = 1,
            SingleSwithching = 2,
            Multiple = 3,
            MultipleCycle = 4
        }
        enum OutOfProxyBehaviourStates
        {
            UseLastUsedProxy=0,
            DontUseProxy=1
        }
        private ProxyDistributionStates DistributorBehaviour;
        private OutOfProxyBehaviourStates OutOfProxyBehaviour;
        ProxyManager(DownloadThread issuer,List<string> addresses, ProxyDistributionStates distrules=0, OutOfProxyBehaviourStates outofproxy=0)
        {
            IssuerThread = issuer;
            Addresses=addresses;
            DistributorBehaviour = distrules;
            OutOfProxyBehaviour = outofproxy;
        }

       


        public string GetProxy() {
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
                            return ExecNoProxiesBehaviour();
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
                default: return null;
            }
            
        }
        private string ExecNoProxiesBehaviour()
        {
            switch (OutOfProxyBehaviour)
            {
                case OutOfProxyBehaviourStates.UseLastUsedProxy:
                    {
                        return IssuerThread.Proxy;
                    }
                case OutOfProxyBehaviourStates.DontUseProxy:
                    {
                        return null;
                    }
                default: return null;
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
}
