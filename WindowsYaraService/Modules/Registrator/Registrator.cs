using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using WindowsYaraService.Base;
using WindowsYaraService.Base.Common;
using WindowsYaraService.Base.Jobs;
using WindowsYaraService.Base.Jobs.common;
using WindowsYaraService.Base.Jobs.NetJobs;
using WindowsYaraService.Modules.Network.Models;
using WindowsYaraService.Modules.Network.Models.Register;
using static WindowsYaraService.Modules.Registrator.Registrator;

namespace WindowsYaraService.Modules.Registrator
{
    class Registrator : BaseObservable<IListener>, INetworkListener
    {
        public interface IListener
        {
            void OnRegister();
        }

        private readonly Networking _networking;
        private CertHandler _certHandler;

        public Registrator(Networking networking)
        {
            _networking = networking;
            _certHandler = new CertHandler();
        }

        public void Enroll()
        {
            EnrollmentJob enrollmentJob = new EnrollmentJob();
            enrollmentJob.RegisterListener(this);
            _networking.ExecuteAsync(enrollmentJob);
        }

        private void InterogateEnrollment()
        {
            InterogateEnrollJob interogateEnrollJob = new InterogateEnrollJob(YaraService._GUID);
            interogateEnrollJob.RegisterListener(this);
            _networking.ExecuteAsync(interogateEnrollJob);
        }

        private void GetPrimalCertificate()
        {
            AsymmetricKeyParameter myCAprivateKey = null;
            X509Certificate2 MyRootCAcert = _certHandler.FindCertificate("STORE_CA", StoreName.Root, StoreLocation.LocalMachine);
            if (MyRootCAcert != null)
            {
                RSACryptoServiceProvider key = (RSACryptoServiceProvider)MyRootCAcert.PrivateKey;
                RSAParameters rsaparam = key.ExportParameters(true);
                myCAprivateKey = DotNetUtilities.GetRsaKeyPair(MyRootCAcert.GetRSAPrivateKey()).Private;
                //myCAprivateKey = DotNetUtilities.GetKeyPair(MyRootCAcert.PrivateKey).Private;
            }
            else
            {
                MyRootCAcert = _certHandler.GenerateCACertificate("CN=STORE_CA", ref myCAprivateKey);
                _certHandler.AddCertToStore(MyRootCAcert, StoreName.Root, StoreLocation.LocalMachine);
            }
            string wrapGUID = "CN=" + YaraService._GUID;
            X509Certificate2 TempCertificate = _certHandler.GenerateSelfSignedCertificate(wrapGUID, "CN=STORE_CA", myCAprivateKey);

            PrimalCertJob primalCertJob = new PrimalCertJob(TempCertificate, YaraService._GUID);
            primalCertJob.RegisterListener(this);
            _networking.ExecuteAsync(primalCertJob);
        }

        private void GetToken()
        {
            TokenJob tokenJob = new TokenJob(YaraService._GUID);
            tokenJob.RegisterListener(this);
            _networking.ExecuteAsync(tokenJob);
        }

        public void OnSuccess(object response)
        {
            RegisterResponse tempResponse;
            try
            {
                tempResponse = response as RegisterResponse;
            }
            catch(Exception ex)
            {
                tempResponse = new RegisterResponse()
                {
                    Type = RegisterTypes.PRIMAL,
                    RegisterObject = response as byte[]
                };
            }
            switch(tempResponse.Type)
            {
                case RegisterTypes.ENROLL:
                    string enrollResponse = tempResponse.RegisterObject as string;
                    if (enrollResponse == "OK")
                    {
                        InterogateEnrollment();
                    }
                    else if (enrollResponse == "NEED_NEW_TOKEN")
                    {
                        GetToken();
                    }
                    break;
                case RegisterTypes.INTEROGATE:
                    string interogationResult = tempResponse.RegisterObject as string;
                    if (interogationResult == "WAIT")
                    {
                        Thread.Sleep(3000);
                        InterogateEnrollment();
                    }
                    else
                    {
                        GetPrimalCertificate();
                    }
                    break;
                case RegisterTypes.PRIMAL:
                    _certHandler.FindAndRemove(YaraService._GUID, StoreName.My, StoreLocation.LocalMachine);
                    X509Certificate2 primalCert = new X509Certificate2(response as byte[]);
                    _certHandler.AddCertToStore(primalCert, StoreName.My, StoreLocation.LocalMachine);

                    GetToken();
                    break;
                case RegisterTypes.TOKEN:
                    byte[] encWithPrimalCertToken = tempResponse.RegisterObject as byte[];
                    byte[] encToken = DataProtection.Protect(encWithPrimalCertToken);
                    FileHandler.WriteBytesToFile("TOKEN", encToken);

                    foreach (var listener in GetListeners())
                    {
                        listener.Key.OnRegister();
                    }
                    break;
            }
        }

        public void OnFailure(INetJob netJob, string errorMessage)
        {
            
        }
    }
}
